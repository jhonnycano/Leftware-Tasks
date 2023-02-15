using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;

namespace Leftware.Tasks.Impl.General.Files;

[Descriptor("FileRename - Add prefix or suffix to files in folder")]
internal class RenameAddPrefixSuffixConsoleTask : CommonTaskBase
{
    private const string SOURCE = "source";
    private const string PATTERN = "pattern";
    private const string RECURSIVE = "recursive";
    private const string PREFIX = "prefix";
    private const string SUFFIX = "suffix";

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new ReadFolderTaskParameter(SOURCE, "Source folder"),
            new ReadStringTaskParameter(PATTERN, "Pattern").WithDefaultValue("*.*"),
            new ReadBoolTaskParameter(RECURSIVE, "Recursive"),
            new ReadStringTaskParameter(PREFIX, "Prefix to add").AllowEmpty(),
            new ReadStringTaskParameter(SUFFIX, "Suffix to add").AllowEmpty(),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var source = input.Get<string>(SOURCE);
        var pattern = input.Get<string>(PATTERN);
        var recursive = input.Get<bool>(RECURSIVE);
        var prefix = input.Get<string>(PREFIX);
        var suffix = input.Get<string>(SUFFIX);

        var files = GetFiles(source, pattern, recursive);

        foreach (var file in files)
        {
            var oldName = Path.GetFileNameWithoutExtension(file);
            var extension = Path.GetExtension(file);
            var newName = prefix + oldName + suffix + extension;
            var newFile = Path.Combine(Path.GetDirectoryName(file), newName);
            if (file == newFile) continue;

            File.Move(file, newFile);
        }
    }

    private IList<string> GetFiles(string source, string pattern, bool recursive)
    {
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = Directory.GetFiles(source, pattern, searchOption);
        return files;
    }
}
