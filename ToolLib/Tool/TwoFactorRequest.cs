using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ToolLib.Http;

namespace ToolLib.Tool
{
    public interface ITwoFactorRequest
    {
        string getPassCode(string token);
    }
    public class TwoFactorRequest : ITwoFactorRequest
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IHttpHelper httpHelper;
        public TwoFactorRequest(IHttpHelper httpHelper)
        {
            this.httpHelper = httpHelper;
        }
        public string getPassCode(string token)
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
