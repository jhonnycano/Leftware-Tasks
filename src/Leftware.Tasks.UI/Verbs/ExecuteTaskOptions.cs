using CommandLine;

namespace Leftware.Tasks.UI.Verbs;

[Verb("task", HelpText = "Starts application in interactive console mode")]
public class ExecuteTaskOptions
{
    [Option]
    public string? Task { get; set; }

    [Option]
    public IEnumerable<string>? TaskParams { get; set; }

    [Option]
    public bool Pause { get; set; }
}
