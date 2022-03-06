using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;

namespace Leftware.Tasks.Impl.General.Files;

[Descriptor("Files - Convert hierarchy folder to flat folder")]
internal class ConvertHierarchyFolderToFlatFolderTask : CommonTaskBase
{
    private const string SOURCE_FOLDER = "sourceFolder";
    private const string TARGET_FOLDER = "targetFolder";
    private const string SEPARATOR = "separator";

    public ConvertHierarchyFolderToFlatFolderTask()
    {
    }

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new ReadFolderTaskParameter(SOURCE_FOLDER, "Source folder"),
            new ReadFolderTaskParameter(TARGET_FOLDER, "Target folder"),
            new ReadStringTaskParameter(SEPARATOR, "Separator"),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var sourceFolder = input.Get<string>(SOURCE_FOLDER);
        var targetFolder = input.Get<string>(TARGET_FOLDER);
        var separator = input.Get<string>(SEPARATOR);

        var collissions = new List<string>();
        var list = new List<Tuple<string, string>>();
        var di = new DirectoryInfo(sourceFolder);

        ExtractNamesInDirectory(di, sourceFolder, separator, collissions, list);
        if (collissions.Count > 0)
        {
            UtilConsole.WriteError("Name collissions found: ");
            foreach (var col in collissions) Console.WriteLine(col);
            return;
        }

        foreach (var item in list)
        {
            var newName = Path.GetFullPath(Path.Combine(targetFolder, item.Item1));
            var currentFile = Path.GetFullPath(Path.Combine(sourceFolder, item.Item2));
            Console.WriteLine($"Moving file {currentFile} to {newName}");
            File.Move(currentFile, newName);
        }
    }

    private static void ExtractNamesInDirectory(DirectoryInfo di, string sourceFolder, string separator,
        IList<string> collissions, IList<Tuple<string, string>> list)
    {
        foreach (var fsi in di.GetFileSystemInfos())
            if (fsi is FileInfo)
            {
                var relativePath = Path.GetRelativePath(sourceFolder, fsi.FullName);
                var newPath = relativePath.Replace("\\", separator);
                if (list.Any(t => t.Item1 == newPath))
                {
                    collissions.Add(relativePath);
                    continue;
                }

                list.Add(new Tuple<string, string>(newPath, relativePath));
            }
            else if (fsi is DirectoryInfo innerDi)
                ExtractNamesInDirectory(innerDi, sourceFolder, separator, collissions, list);
    }
}
