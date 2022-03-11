using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Leftware.Tasks.Impl.General;

[Descriptor("General - Transform text in Clipboard")]
internal class TransformTextInClipboardTask : CommonTaskBase
{
    private const string PATH = "path";

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new SelectFromCollectionTaskParameter(PATH, "Text transform source (file with method public string Execute(string input) {} )", Defs.Collections.TEXT_TRANSFORM_SOURCE),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        /*
        var pathKey = input.Get<string>(PATH);

        var path = Context.CollectionProvider.GetItemContentAs<string>(Defs.Collections.TEXT_TRANSFORM_SOURCE, pathKey);
        var content = File.ReadAllText(path);
        var clipboard = WindowsClipboard.GetText();
        var templateCode = @"
using System;
using Leftware.Common;

public class DynamicTransform : ITransformExecutor
{
    {{ code }}
}
";
        var code = templateCode.FormatLiquid(new { code = content });
        //var currentAssemblyLocation = GetType().Assembly.Location;
        var compileResult = UtilCompile.Compile(code);
        if (!compileResult.Success)
        {
            _console.WriteLine("Failed to compile dynamic code");
            _console.WriteLine(compileResult.Errors[0]);
            return;
        }

        var assembly = compileResult.Assembly;
        var t = assembly.GetType("DynamicTransform");
        var instance = (ITransformExecutor)Activator.CreateInstance(t);
        var result = instance.Execute(clipboard);

        WindowsClipboard.SetText(result);
        */
    }

    private static class WindowsClipboard
    {
        private const uint CF_UNICODE_TEXT = 13;

        public static void SetText(string text)
        {
            OpenClipboard();

            EmptyClipboard();
            IntPtr hGlobal = default;
            try
            {
                var bytes = (text.Length + 1) * 2;
                hGlobal = Marshal.AllocHGlobal(bytes);

                if (hGlobal == default) ThrowWin32();

                var target = GlobalLock(hGlobal);

                if (target == default) ThrowWin32();

                try
                {
                    Marshal.Copy(text.ToCharArray(), 0, target, text.Length);
                }
                finally
                {
                    GlobalUnlock(target);
                }

                if (SetClipboardData(CF_UNICODE_TEXT, hGlobal) == default) ThrowWin32();

                hGlobal = default;
            }
            finally
            {
                if (hGlobal != default) Marshal.FreeHGlobal(hGlobal);

                CloseClipboard();
            }
        }

        public static void OpenClipboard()
        {
            var num = 10;
            while (true)
            {
                if (OpenClipboard(default)) break;

                if (--num == 0) ThrowWin32();

                Thread.Sleep(100);
            }
        }

        public static string GetText()
        {
            var powershell = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardOutput = true,
                    FileName = "powershell",
                    Arguments = "-command \"Get-Clipboard\""
                }
            };

            powershell.Start();
            var text = powershell.StandardOutput.ReadToEnd();
            powershell.StandardOutput.Close();
            powershell.WaitForExit();
            return text.TrimEnd();
        }

        private static void ThrowWin32()
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr data);

        [DllImport("user32.dll")]
        private static extern bool EmptyClipboard();
    }
}
