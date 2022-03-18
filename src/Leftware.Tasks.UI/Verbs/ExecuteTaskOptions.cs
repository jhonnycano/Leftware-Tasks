using CommandLine;

namespace Leftware.Tasks.UI.Verbs;

[Verb("task", HelpText = "Starts application in interactive console mode")]
public class ExecuteTaskOptions
{
    [Option('t', "task")]
    public string? Task { get; set; }

    [Option('p', "params")]
    public IEnumerable<string>? TaskParams { get; set; }

    [Option('z', "pause")]
    public bool Pause { get; set; }
}
