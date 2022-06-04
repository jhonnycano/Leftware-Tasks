using DotLiquid;
using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.Model;
using Leftware.Tasks.Core.TaskParameters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using StringExtensions = Leftware.Tasks.Core.StringExtensions;

namespace Leftware.Tasks.Impl.General;

[Descriptor("Code Generation - Execute code generator")]
internal class ExecuteCodeGeneratorTask : CommonTaskBase
{
    private const string SOURCE_DIRECTORY = "source-directory";

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        var sourceTypes = new[] { "File", "Inline" };
        return new List<TaskParameter>
        {
            new ReadStringTaskParameter(SOURCE_DIRECTORY, "Source directory")
                .AllowEmpty()
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
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

            var modelData = GetModelData(setupItem, model);
            if (modelData == null) continue;

            var template = GetTemplate(setupItem, sourceDirectory);
            if (template == null) continue;

            if (modelData.Type == JTokenType.Array)
            {
                foreach(var modelItem in modelData)
                {
                    GenerateCode(template, modelItem, setupItem, sourceDirectory);
                }
            }
            else if (modelData.Type == JTokenType.Object)
            {
                GenerateCode(template, modelData, setupItem, sourceDirectory);
            }
            else
            {
                UtilConsole.WriteError("Model is not object nor array. Skipping");
                continue;
            }
        }

        await Task.CompletedTask;
    }

    private CodeGenerationSetup? GetSetup(string dir)
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

    private JObject? GetModel(string dir)
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

    private JToken GetModelData(CodeGenerationSetupItem setupItem, JObject model)
    {
        try
        {
            var token = model.SelectToken(setupItem.DataSourcePath);
            return token;
        }
        catch (Exception ex)
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

    private void GenerateCode(Template template, JToken model, CodeGenerationSetupItem setupItem, string dir)
    {
        var targetFile = GenerateTargetFileName(setupItem, model, dir);
        if (targetFile == null) return;

        var result = ExecuteTemplate(template, model);
        if (result == null) return;

        AnsiConsole.MarkupLine("[/green] OK [/] " + targetFile);
        File.WriteAllText(targetFile, result);
    }

    private string? GenerateTargetFileName(CodeGenerationSetupItem setupItem, JToken model, string dir)
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
            if (File.Exists(fullFileName))
            {
                AnsiConsole.MarkupLine("[violet]SKIP[/] " + fullFileName);
                return null;
            }

            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
            return fullFileName;
        }
        catch (Exception)
        {
            UtilConsole.WriteError("Unknown error generating target file name");
            return null;
        }
    }

    private string? ExecuteTemplate(Template template, JToken model)
    {
        var (result, errors) = StringExtensions.ApplyLiquid(template, model.ToString());
        if (errors.Count > 0)
        {
            WriteErrors(errors);
            return null;
        }

        return result;
    }

    private string? GetFirstWithExtension(string dir, string extension, string label)
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

    private static void WriteErrors(IList<string> errors)
    {
        if (errors == null || errors.Count <= 0) return;

        Console.WriteLine($"Following errors occurred while performing transformation:");
        foreach (var err in errors)
        {
            Console.WriteLine(err);
        }
    }
}
