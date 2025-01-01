using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolLib.Http;

namespace ToolLib.Test
{
    public class TestHttp
    {
        public static void TestGet()
        {
            var http = ToolDiConfig.Get<HttpHelper>();
            var url = "http://2fa.live/tok/MU2ZT6UXVYS7IKIOOV6SZWWESKE5MXZ4";
            var response = http.Get(url);
            var resp = response.GetResponse<Dictionary<string, object>>();
            Console.WriteLine("done : " + resp);
        }
    }
}
