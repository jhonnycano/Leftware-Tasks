namespace Leftware.Tasks.Core.Model;

public class CodeGenerationSetupItem
{
    public string Template { get; set; } = "";
    public string TargetPath { get; set; } = "";
    public string FileName { get; set; } = "";
    public string DataSourcePath { get; set; } = "";
    public string DataSourceFilter { get; set; } = "";
    public bool SkipIfAlreadyexists { get; set; } = false;
    public bool ForceOverwrite { get; set; } = false;
    public bool DeleteUnhandledFiles { get; set; } = false;
}
