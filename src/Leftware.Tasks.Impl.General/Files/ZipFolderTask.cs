using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;
using System.Diagnostics;

namespace Leftware.Tasks.Impl.General.Files;

[Descriptor("FileSystem - Zip folder")]
public class ZipFolderTask : CommonTaskBase
{
    private const string DEFAULT_PREFIX = "--**DEFAULT-PREFIX**--";
    private const string SOURCE = "source";
    private const string TARGET = "target";
    private const string PREFIX = "prefix";
    private const string PATTERN = "pattern";

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
            {
                new SelectFromCollectionTaskParameter(SOURCE, "Folder to compress", Defs.Collections.FAVORITE_FOLDER, true),
                new SelectFromCollectionTaskParameter(TARGET, "Folder to save compressed file", Defs.Collections.FAVORITE_FOLDER, true),
                new ReadStringTaskParameter(PREFIX, "File prefix (def:folder name)").WithDefaultValue(DEFAULT_PREFIX),
                new SelectFromCollectionTaskParameter(PATTERN, "File pattern", Defs.Collections.FILE_PATTERN, true),
            };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var source = input.Get<string>(SOURCE);
        var target = input.Get<string>(TARGET);
        var prefix = input.Get<string>(PREFIX);
        var pattern = input.Get<string>(PATTERN);

        if (source.EndsWith("\\")) source = source[..^1];
        if (target.EndsWith("\\")) target = target[..^1];
        if (prefix == DEFAULT_PREFIX) prefix = Path.GetFileName(source);

        var path7zip = Context.SettingsProvider.GetSetting(Defs.Settings.PATH_7ZIP, true);
        if (string.IsNullOrEmpty(path7zip) || path7zip == Defs.VALUE_NOT_FOUND)
        {
            UtilConsole.WriteError("path for executable 7-zip not found. Operation cancelled");
            return;
        }

        var targetFile = "{{ prefix }}{{ pattern }}.7z".FormatLiquid(new { prefix, pattern });
        targetFile = string.Format(targetFile, DateTime.Now);
        targetFile = Path.Combine(target, targetFile);

        var template = "a -t7z \"{{ targetFile }}\" \"{{ source }}\\*\"";
        var cmd = template.FormatLiquid(new { targetFile, source });

        var process = new Process
        {
            StartInfo = new ProcessStartInfo(path7zip, cmd)
            {
                UseShellExecute = false
            }
        };
        process.Start();
    }
}