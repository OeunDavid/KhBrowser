using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using ToolLib;
using WpfUI.ToolLib.Mail;

namespace WpfUI
{
    class Program
    {
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void Main(String[] args)
        {
            //ClientEmailTest.Test();
            
        }
    }
}
