using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;

namespace Leftware.Tasks.Impl.General.Media;

[Descriptor("FileConversion - Convert gif to mp4")]
internal class ConvertGifToMp4Task : CommonTaskBase
{
    private const string SOURCE = "source";

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new ReadFolderTaskParameter(SOURCE, "Folder with .gif files"),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var source = input.Get<string>(SOURCE);

        var cmd = Context.SettingsProvider.GetSetting(Defs.Settings.PATH_FFMPEG, true);
        if (cmd == null) return;

        var files = Directory.GetFiles(source, "*.gif");
        var argsTemplate = "-f gif -i {{GifFile}} {{Mp4File}}";

        foreach (var file in files)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var args = argsTemplate.FormatLiquid(new {
                GifFile = file,
                Mp4File = Path.Combine(source, name + ".mp4")
            });
            var output = UtilProcess.Invoke(cmd, args);
            Console.WriteLine(output);
        }
    }
}
