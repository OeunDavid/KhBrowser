using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ToolLib.Http;

namespace ToolLib.Tool
{
    public static class TwoFactorRequest 
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public static string GetPassCode(string token)
        {
            try
            {
                token = Regex.Replace(token, @"\s+", "");
                var http = ToolDiConfig.Get<HttpHelper>();
                var url = "http://2fa.live/tok/" + token;
                var response = http.Get(url);
                var resp = response.GetResponse<Dictionary<string, object>>();

                return resp["token"]+"";        
            } catch(Exception e) {
                log.Error("error call to 2fa : "+token, e);
            }
            return "";
        }
    }
}
