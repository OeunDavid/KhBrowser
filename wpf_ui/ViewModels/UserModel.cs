using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using ToolLib.Tool;

namespace ToolKHBrowser.ViewModels
{
    public static class UserModel
    {
        public static string getPath()
        {
            return System.Environment.CurrentDirectory;
        }
    }
    public class APIResponseDataRecord
    {
        [JsonProperty("data")]
        public User data { get; set; }
        [JsonProperty("code")]
        public string code { get; set; }
        [JsonProperty("message")]
        public string message { get; set; }
    }
    public class User
    {
        [JsonProperty("success")]
        public int success { get; set; }
        [JsonProperty("PCName")]
        public string PCName { get; set; }
        [JsonProperty("LicenseDay")]
        public int LicenseDay { get; set; }
    }
}
