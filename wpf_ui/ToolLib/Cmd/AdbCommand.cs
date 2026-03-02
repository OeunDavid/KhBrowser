using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ToolKHBrowser.ToolLib.Cmd;

namespace ToolLib
{
    public interface IAdbCommand
    {
        ExecutionResult execute(string dir, params string[] p);
    }

    public class AdbCommand : IAdbCommand
    {
        public static readonly string PATH =
            Path.Combine(ToolKHBrowser.ToolLib.Data.ConfigData.GetPath(), "tool", "adb.exe");

        const int RETRY = 1;
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ExecutionResult execute(string dir, params string[] p)
        {
            int count = 0;

            while (count++ < RETRY)
            {
                try
                {
                    if (!File.Exists(PATH))
                        throw new FileNotFoundException("adb.exe not found", PATH);

                    // CliWrap wants arguments as a single string or array
                    string args = string.Join(" ", p ?? Array.Empty<string>());

                    // Make sure dir exists, otherwise adb can fail
                    if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    // Execute adb and capture stdout/stderr.
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = PATH,
                        Arguments = args,
                        WorkingDirectory = string.IsNullOrWhiteSpace(dir) ? Environment.CurrentDirectory : dir,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    string stdOut = "";
                    string stdErr = "";
                    int exitCode = -1;

                    using (var process = Process.Start(startInfo))
                    {
                        if (process == null)
                            throw new InvalidOperationException("Failed to start adb process.");

                        stdOut = process.StandardOutput.ReadToEnd();
                        stdErr = process.StandardError.ReadToEnd();
                        process.WaitForExit();
                        exitCode = process.ExitCode;
                    }

                    var exec = new ExecutionResult
                    {
                        ExitCode = exitCode,
                        StandardOutput = stdOut ?? "",
                        StandardError = stdErr ?? ""
                    };

                    log.Info(" result " + exec.StandardOutput + " \n " + exec.StandardError);
                    return exec;
                }
                catch (Exception e)
                {
                    Thread.Sleep(500);
                    log.Error("error execute adb: " + e.Message, e);
                }
            }

            return new ExecutionResult
            {
                ExitCode = -1,
                StandardOutput = "",
                StandardError = "ADB execute failed"
            };
        }
    }
}
