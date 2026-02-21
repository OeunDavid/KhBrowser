using System;
using System.IO;
using System.Threading;
using CliWrap;
using CliWrap.Buffered;
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

                    // Execute buffered to capture stdout/stderr
                    var result = Cli.Wrap(PATH)
                        .WithArguments(args)
                        .WithWorkingDirectory(string.IsNullOrWhiteSpace(dir) ? Environment.CurrentDirectory : dir)
                        .ExecuteBufferedAsync()
                        .GetAwaiter()
                        .GetResult();

                    var exec = new ExecutionResult
                    {
                        ExitCode = result.ExitCode,
                        StandardOutput = result.StandardOutput ?? "",
                        StandardError = result.StandardError ?? ""
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