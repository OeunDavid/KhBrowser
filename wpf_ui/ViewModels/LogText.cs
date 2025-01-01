using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfUI.ViewModels
{
    public interface ILogText
    {
        void add(string text);
        string log();
    }
    public class LogText : ILogText
    {
        public const int MAX_LOG_LENGTH = 50;
        public List<string> logs = new List<string>();
        public void add(string text)
        {
            if(logs.Count > MAX_LOG_LENGTH)
            {
                logs.RemoveAt(logs.Count - 1);
            }
            logs.Insert(0, text);
        }

        public string log()
        {
            StringBuilder sb = new StringBuilder();
            foreach(var str in logs)
            {
                sb.Append(str).Append("\n");
            }
            return sb.ToString();
        }
    }
}
