using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolKHBrowser.ToolLib.Cmd
{
    public class ExecutionResult
    {
        public int ExitCode { get; set; }
        public string StandardOutput { get; set; } = "";
        public string StandardError { get; set; } = "";
        public bool IsSuccess => ExitCode == 0;
    }
}
