using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolKHBrowser.ToolLib.Data;
using WpfUI.ViewModels;

namespace WpfUI.ToolLib.Data
{
    public static class ConfigData
    {
        public static string GetPath()
        {
            try
            {
                string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                return System.IO.Path.GetDirectoryName(strExeFilePath);
            }
            catch { return AppDomain.CurrentDomain.BaseDirectory; }
        }
        public static string GetBrowserKey(string uid)
        {
            return Constant.FB_BROWSER_KEY+" "+uid;
        }
        public static string GetPathDriver()
        {
            return Path.Combine(GetPath(), "driver");
        }
        public static string GetBrowserDataDirectory()
        {
            var cache = DIConfig.Get<ICacheViewModel>().GetCacheDao().Get("config:microsoftEdgeProfile");
            string die = cache?.Value?.ToString() ?? "";
            if(string.IsNullOrEmpty(die))
            {
                die= Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\Microsoft\\Edge\\User Data";
                DIConfig.Get<ICacheViewModel>().GetCacheDao().Set("config:microsoftEdgeProfile", die);
            }

            return die;
        }
        public static int GetShowScreen()
        {
            int num = 6;
            try
            {
                num = DIConfig.Get<ICacheViewModel>().GetCacheDao().Get("config:showScreen").Total;
                if(num == 0)
                {
                    num = 6;
                }
            }
            catch (Exception) { }

            return num;
        }
        public static string GetRunType()
        {
            var cache = DIConfig.Get<ICacheViewModel>().GetCacheDao().Get("config:runType");
            return cache?.Value?.ToString() ?? "web";
        }
        public static string GetBrowserType()
        {
            var cache = DIConfig.Get<ICacheViewModel>().GetCacheDao().Get("config:browserType");
            string browserType = cache?.Value?.ToString() ?? "edge";
            if(browserType == "chrome")
            {
                return "chrome";
            }

            return "edge";
        }
        public static bool IsLoginByCookie()
        {
            bool cookie = false;
            var cache = DIConfig.Get<ICacheViewModel>().GetCacheDao().Get("config:loginType");
            string loginType = cache?.Value?.ToString() ?? "";
            if (loginType == "cookie")
            {
                cookie = true;
            }

            return cookie;
        }
        public static bool IsUseImage()
        {
            bool image = true;
            var cache = DIConfig.Get<ICacheViewModel>().GetCacheDao().Get("config:useImage");
            string useImage = cache?.Value?.ToString() ?? "";
            if (useImage == "no")
            {
                image = false;
            }

            return image;
        }
        public static string ReadTextFromTextFile(string file_name, string path = "")
        {
            if (string.IsNullOrEmpty(path))
            {
                path = GetPath();
            }

            string fullPath = Path.Combine(path, file_name);
            if (!File.Exists(fullPath)) return "";

            try
            {
                using (StreamReader sr = new StreamReader(fullPath))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (Exception) { }

            return "";
        }
        public static void Write(string file_name, string text, string path = "")
        {
            if (string.IsNullOrEmpty(path))
            {
                path = GetPath();
            }

            try
            {
                string fullPath = Path.Combine(path, file_name);
                string directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (StreamWriter sw = new StreamWriter(fullPath))
                {
                    sw.WriteLine(text);
                }
            }
            catch (Exception) { }
        }
    }
}
