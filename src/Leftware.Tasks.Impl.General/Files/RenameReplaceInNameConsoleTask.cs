using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;

namespace Leftware.Tasks.Impl.General.Files;

[Descriptor("FileRename - Replace string in name for files in folder")]
internal class RenameReplaceInNameConsoleTask : CommonTaskBase
{
    private const string SOURCE = "source";
    private const string PATTERN = "pattern";
    private const string RECURSIVE = "recursive";
    private const string FIND = "find";
    private const string REPLACE = "replace";

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new ReadFolderTaskParameter(SOURCE, "Source folder"),
            new ReadStringTaskParameter(PATTERN, "Pattern").WithDefaultValue("*.*"),
            new ReadBoolTaskParameter(RECURSIVE, "Recursive"),
            new ReadStringTaskParameter(FIND, "String to find in names"),
            new ReadStringTaskParameter(REPLACE, "New string to put in files instead of the found string"),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var source = input.Get<string>(SOURCE);
        var pattern = input.Get<string>(PATTERN);
        var recursive = input.Get<bool>(RECURSIVE);
        var find = input.Get<string>(FIND);
        var replace = input.Get<string>(REPLACE);

        var files = GetFiles(source, pattern, recursive);

        foreach (var file in files)
        {
            var oldName = Path.GetFileNameWithoutExtension(file);
            var extension = Path.GetExtension(file);

            var newName = oldName;
            newName = newName.Replace(find, replace);
            newName += extension;
            newName = Path.Combine(Path.GetDirectoryName(file), newName);
            if (file == newName) continue;

            File.Move(file, newName);
        }
    }

    private static IList<string> GetFiles(string source, string pattern, bool recursive)
    {
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = Directory.GetFiles(source, pattern, searchOption);
        return files;
    }
}
