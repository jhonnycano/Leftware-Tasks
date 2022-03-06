using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;
using System.Text.RegularExpressions;

namespace Leftware.Tasks.Impl.General.Files;

[Descriptor("FileSystem - Replace in text files")]
internal class ReplaceInTextFilesConsoleTask : CommonTaskBase
{
    private const string SOURCE = "source";
    private const string PATTERN = "pattern";
    private const string RECURSIVE = "recursive";
    private const string SEARCH = "search";
    private const string USE_REGEX = "useRegex";
    private const string REPLACE = "replace";

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new ReadFolderTaskParameter(SOURCE, "Source folder"),
            new ReadStringTaskParameter(PATTERN, "Pattern").WithDefaultValue("*.*"),
            new ReadBoolTaskParameter(RECURSIVE, "Recursive"),
            new ReadStringTaskParameter(SEARCH, "Search"),
            new ReadBoolTaskParameter(USE_REGEX, "useRegex"),
            new ReadStringTaskParameter(REPLACE, "replace"),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var source = input.Get<string>(SOURCE);
        var pattern = input.Get<string>(PATTERN);
        var recursive = input.Get<bool>(RECURSIVE);
        var search = input.Get<string>(SEARCH);
        var useRegex = input.Get<bool>(USE_REGEX);
        var replace = input.Get<string>(REPLACE);

        var files = GetFiles(source, pattern, recursive);

        foreach (var file in files)
        {
            var txt = File.ReadAllText(file);

            if (useRegex)
            {
                var regex = new Regex(search);
                txt = regex.Replace(txt, replace);
            }
            else
                txt = txt.Replace(search, replace);

            File.WriteAllText(file, txt);
        }
    }

    private static IList<string> GetFiles(string source, string pattern, bool recursive)
    {
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = Directory.GetFiles(source, pattern, searchOption);
        return files;
    }
}
