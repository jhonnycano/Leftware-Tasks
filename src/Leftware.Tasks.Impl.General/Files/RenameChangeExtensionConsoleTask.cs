using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;

namespace Leftware.Tasks.Impl.General.Files;

[Descriptor("FileRename - Change extension for files in folder")]
internal class RenameChangeExtensionConsoleTask : CommonTaskBase
{
    private const string SOURCE = "source";
    private const string PATTERN = "pattern";
    private const string RECURSIVE = "recursive";
    private const string NEW_EXTENSION = "newExtension";

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new ReadFolderTaskParameter(SOURCE, "Source folder"),
            new ReadStringTaskParameter(PATTERN, "Pattern").WithDefaultValue("*.*"),
            new ReadBoolTaskParameter(RECURSIVE, "Recursive"),
            new ReadStringTaskParameter(NEW_EXTENSION, "New extension"),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var source = input.Get<string>(SOURCE);
        var pattern = input.Get<string>(PATTERN);
        var recursive = input.Get<bool>(RECURSIVE);
        var newExtension = input.Get<string>(NEW_EXTENSION);

        var files = GetFiles(source, pattern, recursive);

        foreach (var file in files)
        {
            var oldName = Path.GetFileNameWithoutExtension(file);
            // var extension = Path.GetExtension(file);

            var newName = oldName;
            newName += newExtension;
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
