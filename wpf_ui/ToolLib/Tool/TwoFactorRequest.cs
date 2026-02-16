using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace ToolLib.Tool
{
    public static class TwoFactorRequest 
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public static string GetPassCode(string token)
        {
            try
            {
                return ToolKHBrowser.Helper.TwoFaHelper.GenerateCode(token);  
            } catch(Exception e) {
                log.Error("error generate 2fa locally : "+token, e);
            }
            return "";
        }
    }
}
