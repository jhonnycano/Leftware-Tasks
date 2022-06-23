using DotLiquid;
using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.Model;
using Leftware.Tasks.Core.TaskParameters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using System.Security.Cryptography;
using System.Text;
using StringExtensions = Leftware.Tasks.Core.StringExtensions;

namespace Leftware.Tasks.Impl.General;

[Descriptor("Code Generation - Execute code generator")]
internal class ExecuteCodeGeneratorTask : CommonTaskBase
{
    private const string SOURCE_DIRECTORY = "source-directory";
    internal enum FileCaptionText { None, Ok, Skip, Force, Remove }
    internal enum FileCaptionColor { Red, Green, Blue, Violet, Cyan, Yellow }

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new ReadStringTaskParameter(SOURCE_DIRECTORY, "Source directory")
                .AllowEmpty()
        };
    }

    public async override Task Execute(IDictionary<string, object> input)
    {
        var sourceDirectory = input.Get<string>(SOURCE_DIRECTORY);
        if (string.IsNullOrEmpty(sourceDirectory))
            sourceDirectory = Environment.CurrentDirectory;

        var setup = GetSetup(sourceDirectory);
        if (setup == null) return;

        var model = GetModel(sourceDirectory);
        if (model == null) return;

        foreach(var item in setup.Items)
        {
            var setupItem = item.Value;

            AnsiConsole.MarkupLine($"[Cyan]Generating {item.Key}[/]");
            AnsiConsole.MarkupLine($"  Path: {setupItem.TargetPath}");
            Console.WriteLine();

            var fullTargetPath = Path.GetFullPath(Path.Combine(sourceDirectory, setupItem.TargetPath));
            var files = new List<string>(Directory.GetFiles(fullTargetPath, "*.*", SearchOption.AllDirectories));

            var modelData = GetModelData(setupItem, model);
            if (modelData == null) continue;

            var template = GetTemplate(setupItem, sourceDirectory);
            if (template == null) continue;

            if (modelData.Type == JTokenType.Array)
            {
                foreach(var modelItem in modelData)
                {
                    GenerateCode(template, modelItem, setupItem, sourceDirectory, files);
                }
            }
            else if (modelData.Type == JTokenType.Object)
            {
                GenerateCode(template, modelData, setupItem, sourceDirectory, files);
            }
            else
            {
                UtilConsole.WriteError("Model is not object nor array. Skipping");
                continue;
            }

            if (setupItem.DeleteUnhandledFiles && files.Count > 0)
            {
                foreach(var file in files)
                {
                    File.Delete(file);
                    var relativeFile = Path.GetRelativePath(sourceDirectory, fullTargetPath);
                    WriteFileCaption(FileCaptionText.Remove, FileCaptionColor.Red, relativeFile, "");
                }
            }
        }

        await Task.CompletedTask;
    }

    private static CodeGenerationSetup? GetSetup(string dir)
    {
        try
        {
            var filePath = GetFirstWithExtension(dir, "*.setup.json", "Found file {0} for setup");
            if (filePath == null) return null;

            var content = File.ReadAllText(filePath);
            var setup = JsonConvert.DeserializeObject<CodeGenerationSetup>(content);
            return setup;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return null;
        }
    }

    private static JObject? GetModel(string dir)
    {
        try
        {
            var filePath = GetFirstWithExtension(dir, "*.model.json", "Found file {0} for model");
            if (filePath == null) return null;

            var content = File.ReadAllText(filePath);
            var model = JObject.Parse(content);
            return model;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return null;
        }
    }

    private static JToken? GetModelData(CodeGenerationSetupItem setupItem, JObject model)
    {
        try
        {
            var token = model.SelectToken(setupItem.DataSourcePath);
            return token;
        }
        catch
        {
            UtilConsole.WriteError($"Could not read dataSource {setupItem.DataSourcePath} from model");
            throw;
        }
    }

    private static Template? GetTemplate(CodeGenerationSetupItem setupItem, string dir)
    {
        try
        {
            var templatePath = Path.Combine(dir, setupItem.Template);
            if (!File.Exists(templatePath))
            {
                UtilConsole.WriteError($"Template not found at {templatePath}");
                return null;
            }
            var content = File.ReadAllText(templatePath);
            var liquidTemplate = Template.Parse(content);
            return liquidTemplate;

        }
        catch (Exception)
        {
            UtilConsole.WriteError($"Error reading template {setupItem.Template}");
            return null;
        }
    }

    private static void GenerateCode(Template template, JToken model, CodeGenerationSetupItem setupItem, string dir, IList<string> files)
    {
        var targetFile = GenerateTargetFileName(setupItem, model, dir);
        if (targetFile == null) return;

        var filePathRelativeToDir = Path.GetRelativePath(dir, targetFile);
        var filePathRelativeToTargetPath = Path.GetRelativePath(setupItem.TargetPath, filePathRelativeToDir);
        var targetDir = Path.GetDirectoryName(targetFile);
        if (targetDir is null) throw new InvalidOperationException("targetDir is null");

        if (File.Exists(targetFile))
        {
            if (setupItem.SkipIfAlreadyexists)
            {
                WriteFileCaption(FileCaptionText.Skip, FileCaptionColor.Violet, filePathRelativeToTargetPath, "marked as 'Skip if already exists'");
                files.Remove(targetFile);
                return;
            }
        }

        if (!SatisfiesFilter(setupItem, model, filePathRelativeToTargetPath)) return;
        if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

        var result = ExecuteTemplate(template, model);
        if (result == null) return;

        if (File.Exists(targetFile))
        {
            if (setupItem.ForceOverwrite)
            {
                AnsiConsole.MarkupLine("[red]FORCE   [/] " + filePathRelativeToTargetPath);
            }
            else
            {
                var md5Existing = GetMd5(File.ReadAllText(targetFile));
                var md5New = GetMd5(result);
                if (md5Existing == md5New)
                {
                    WriteFileCaption(FileCaptionText.Skip, FileCaptionColor.Blue, filePathRelativeToTargetPath, "unchanged");
                    files.Remove(targetFile);
                    return;
                }
            }
        }

        WriteFileCaption(FileCaptionText.Ok, FileCaptionColor.Green, filePathRelativeToTargetPath, "");
        File.WriteAllText(targetFile, result);
        files.Remove(targetFile);
    }

    private static string? GenerateTargetFileName(CodeGenerationSetupItem setupItem, JToken model, string dir)
    {
        try
        {
            var (fileName, errors) = StringExtensions.ApplyLiquid(setupItem.FileName, model.ToString());
            if (errors.Count > 0)
            {
                UtilConsole.WriteError("Error generating target file name");
                WriteErrors(errors);
                return null;
            }
            var targetPath = Path.Combine(dir, setupItem.TargetPath);
            var fullFileName = Path.Combine(targetPath, fileName);
            var fileDir = Path.GetDirectoryName(fullFileName);
            if (fileDir == null) return null;

            var targetDir = Path.GetFullPath(fileDir);
            if (targetDir == null) return null;

            fullFileName = Path.GetFullPath(fullFileName);
            return fullFileName;
        }
        catch (Exception)
        {
            UtilConsole.WriteError("Unknown error generating target file name");
            return null;
        }
    }

    private static string? ExecuteTemplate(Template template, JToken model)
    {
        var (result, errors) = StringExtensions.ApplyLiquid(template, model.ToString());
        if (errors.Count > 0)
        {
            WriteErrors(errors);
            return null;
        }

        return result;
    }

    private static bool SatisfiesFilter(CodeGenerationSetupItem setupItem, JToken model, string relativeFileName)
    {
        if (string.IsNullOrEmpty(setupItem.DataSourceFilter)) return true;

        var filterResult = false;
        try
        {
            var token = model.SelectToken(setupItem.DataSourceFilter);
            if (token is null) return false;

            filterResult = (bool)token;
        }
        catch
        {
        }

        if (!filterResult)
        {
            WriteFileCaption(FileCaptionText.Skip, FileCaptionColor.Yellow, relativeFileName, "filtered (expression)");
            return false;
        }
        return true;
    }

    private static string? GetFirstWithExtension(string dir, string extension, string label)
    {
        var files = Directory.GetFiles(dir, extension);
        if (files.Length == 0)
        {
            UtilConsole.WriteError($"Setup file not found. Try to create a file with extension {extension}");
            return null;
        }

        var filePath = files[0];
        Console.WriteLine(label, filePath);
        return filePath;
    }

    private static void WriteFileCaption(FileCaptionText captionText, FileCaptionColor captionColor, string message, string reason)
    {
        AnsiConsole.MarkupLine($"[{captionColor}]{captionText,-8}[/] {message,-65} [grey]{reason,-15}[/]");
    }

    private static void WriteErrors(IList<string> errors)
    {
        if (errors == null || errors.Count <= 0) return;

        Console.WriteLine($"Following errors occurred while performing transformation:");
        foreach (var err in errors)
        {
            Console.WriteLine(err);
        }
    }

    private static string GetMd5(string content)
    {
        var encoded = Encoding.UTF8.GetBytes(content);
        using var algo = MD5.Create();
        var hash = algo.ComputeHash(encoded);
        var encodedString = BitConverter.ToString(hash)
            .Replace("-", string.Empty)
           .ToLower();
        return encodedString;
    }
}
