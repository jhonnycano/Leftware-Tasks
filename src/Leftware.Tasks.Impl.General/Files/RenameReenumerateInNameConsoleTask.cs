using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;
using System.Text.RegularExpressions;

namespace Leftware.Tasks.Impl.General.Files;

[Descriptor("FileRename - Find and change number in filename for files in folder")]
internal class RenameReenumerateInNameConsoleTask : CommonTaskBase
{
    private const string SOURCE = "source";
    private const string PATTERN = "pattern";
    private const string RECURSIVE = "recursive";
    private const string FIND = "find";
    private const string REPLACE = "replace";
    private const string PADDING = "padding";

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new ReadFolderTaskParameter(SOURCE, "Source folder"),
            new ReadStringTaskParameter(PATTERN, "Pattern").WithDefaultValue("*.*"),
            new ReadBoolTaskParameter(RECURSIVE, "Recursive"),
            new ReadStringTaskParameter(FIND, "Text before the number to operate"),
            new ReadIntegerTaskParameter(REPLACE, "Number to add (or negative number to subtract)")
                .WithRange(-1000, 1000),
            new ReadIntegerTaskParameter(PADDING, "Padding to use (default:2)")
                .WithRange(0, 40)
                .WithDefaultValue(2),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var source = input.Get<string>(SOURCE);
        var pattern = input.Get<string>(PATTERN);
        var recursive = input.Get<bool>(RECURSIVE);
        var find = input.Get<string>(FIND);
        var replace = input.Get<int>(REPLACE);
        var padding = input.Get<int>(PADDING);

        var files = GetFiles(source, pattern, recursive);

        foreach (var file in files)
        {
            var oldName = Path.GetFileNameWithoutExtension(file);
            var extension = Path.GetExtension(file);

            var newName = oldName;
            var match = Regex.Match(newName, find + "(?<number>-?\\d+)");
            var number = Convert.ToInt32(match.Groups["number"].Value);
            var newNumber = (number + replace).ToString("d" + padding);
            newName = newName.Replace(find + match.Groups["number"].Value, find + newNumber);
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
