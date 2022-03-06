using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;

namespace Leftware.Tasks.Impl.General.Files;

[Descriptor("FileRename - Remove prefix or suffix from files in folder")]
internal class RenameRemovePrefixSuffixConsoleTask : CommonTaskBase
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
            new ReadStringTaskParameter(PREFIX, "Prefix to remove"),
            new ReadStringTaskParameter(SUFFIX, "Suffix to remove"),
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

            var newName = oldName;

            if (!string.IsNullOrEmpty(prefix) && newName.StartsWith(prefix)) newName = newName[prefix.Length..];
            if (!string.IsNullOrEmpty(suffix) && newName.EndsWith(suffix)) newName = newName[..suffix.Length];
            newName += extension;
            newName = Path.Combine(Path.GetDirectoryName(file), newName);
            if (file == newName) continue;

            File.Move(file, newName);
        }
    }

    private IList<string> GetFiles(string source, string pattern, bool recursive)
    {
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = Directory.GetFiles(source, pattern, searchOption);
        return files;
    }
}
