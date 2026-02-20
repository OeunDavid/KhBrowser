using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CliWrap;
using CliWrap.Models; 

namespace ToolLib
{
    public interface IAdbCommand
    {
        ExecutionResult execute(string dir,params string[] p);
    }

    public class AdbCommand:IAdbCommand
    {
     
        public static readonly string PATH = System.IO.Path.Combine(ToolKHBrowser.ToolLib.Data.ConfigData.GetPath(), "tool", "adb.exe");
        const int RETRY = 1;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public ExecutionResult execute(string dir, params string[] p)
        {
            int cout = 0;
            while (cout++ < RETRY)
            {
                try
                {
                    var cmd = new Cli(PATH);
                    var c = String.Join(" ", p);
                   // log.Debug(" cmd : " + c);
                    cmd.SetArguments(c);
                    cmd.SetWorkingDirectory(dir);
                    cmd.SetStandardErrorCallback((str) =>
                    {
                        //log.Info("error for cmd " + c + " -> " + str);
                    });
                    cmd.SetStandardOutputCallback((str) => {
                        //log.Info("result for cmd " + c + " -> " + str);
                    }
                        );
                    ExecutionResult result = cmd.Execute();
                    log.Info(" result " + result.StandardOutput + " \n " + result.StandardError);
                    return result;
                }
                catch (Exception e)
                {
                    Thread.Sleep(500);
                    log.Error("error execute adb");
                }

            }
            return null;
        }
    }
}
