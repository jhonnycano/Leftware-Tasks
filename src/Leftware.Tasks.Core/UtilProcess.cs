using System.Diagnostics;

namespace Leftware.Tasks.Core;

public static class UtilProcess
{
    public static string Invoke(string cmd, string args, string? workingDir = null)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = cmd,
            Arguments = args,
            CreateNoWindow = true,
            UseShellExecute = true,
            RedirectStandardOutput = false,
            RedirectStandardError = false,                
            //WindowStyle = ProcessWindowStyle.Hidden,
        };
        if (!string.IsNullOrEmpty(workingDir)) processInfo.WorkingDirectory = workingDir;

        using (var process = new Process())
        {
            process.EnableRaisingEvents = false;

            process.Start();
            process.WaitForExit();
        }

        // todo: investigate whether it's feasible to gather command output
        return "";
    }

    public static string InvokeNoShell(string cmd, string args, string? workingDir = null)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = cmd,
            Arguments = args,
            UseShellExecute = false
        };
        if (!string.IsNullOrEmpty(workingDir)) processInfo.WorkingDirectory = workingDir;
        using (var process = new Process())
        {
            process.Start();
            process.WaitForExit();
        }

        // todo: investigate whether it's feasible to gather command output
        return "";
    }

}
