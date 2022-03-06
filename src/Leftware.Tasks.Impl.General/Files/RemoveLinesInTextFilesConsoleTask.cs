using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;
using System.Text.RegularExpressions;

namespace Leftware.Tasks.Impl.General.Files;

[Descriptor("FileSystem - Remove lines in text files")]
internal class RemoveLinesInTextFilesConsoleTask : CommonTaskBase
{
    private const string SOURCE = "source";
    private const string PATTERN = "pattern";
    private const string RECURSIVE = "recursive";
    private const string SEARCH = "search";

    public RemoveLinesInTextFilesConsoleTask()
    {
    }

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new ReadFolderTaskParameter(SOURCE, "Source folder"),
            new ReadStringTaskParameter(PATTERN, "Pattern").WithDefaultValue("*.*"),
            new ReadBoolTaskParameter(RECURSIVE, "Recursive"),
            new ReadStringTaskParameter(SEARCH, "Search"),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var source = input.Get<string>(SOURCE);
        var pattern = input.Get<string>(PATTERN);
        var recursive = input.Get<bool>(RECURSIVE);
        var search = input.Get<string>(SEARCH);

        var files = GetFiles(source, pattern, recursive);

        foreach (var file in files)
            try
            {
                var lines = File.ReadAllLines(file);
                var regex = new Regex(search);

                var resultLines = lines.Where(line => !regex.IsMatch(line)).ToList();

                File.WriteAllLines(file, resultLines);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
    }

    private static IList<string> GetFiles(string source, string pattern, bool recursive)
    {
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = Directory.GetFiles(source, pattern, searchOption);
        return files;
    }
}
