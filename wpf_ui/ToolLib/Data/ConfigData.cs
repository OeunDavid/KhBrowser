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
            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);

            return strWorkPath;
        }
        public static string GetBrowserKey(string uid)
        {
            return Constant.FB_BROWSER_KEY+" "+uid;
        }
        public static string GetPathDriver()
        {
            return GetPath() + "/driver";
        }
        public static string GetBrowserDataDirectory()
        {
            var die= DIConfig.Get<ICacheViewModel>().GetCacheDao().Get("config:microsoftEdgeProfile").Value.ToString();
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
            return DIConfig.Get<ICacheViewModel>().GetCacheDao().Get("config:runType").Value.ToString();
        }
        public static string GetBrowserType()
        {
            string browserType= DIConfig.Get<ICacheViewModel>().GetCacheDao().Get("config:browserType").Value.ToString();
            if(browserType == "chrome")
            {
                return "chrome";
            }

            return "edge";
        }
        public static bool IsLoginByCookie()
        {
            bool cookie = false;
            string loginType = DIConfig.Get<ICacheViewModel>().GetCacheDao().Get("config:loginType").Value.ToString();
            if (loginType == "cookie")
            {
                cookie = true;
            }

            return cookie;
        }
        public static bool IsUseImage()
        {
            bool image = true;
            string useImage = DIConfig.Get<ICacheViewModel>().GetCacheDao().Get("config:useImage").Value.ToString();
            if (useImage == "no")
            {
                image = false;
            }

            return image;
        }
        public static string ReadTextFromTextFile(string file_name, string path="")
        {
            string line = "", str = "";
            if(string.IsNullOrEmpty(path))
            {
                path = GetPath();
            }
            try
            {
                StreamReader sr = new StreamReader(path + "/" + file_name);
                line = sr.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    str = line;
                }
                while (line != null)
                {
                    //Read the next line
                    line = sr.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        str += "\r\n" + line;
                    }
                }
                sr.Close();
            }
            catch (Exception) { }

            return str;
        }
        public static void Write(string file_name, string text, string path = "")
        {
            if (string.IsNullOrEmpty(path))
            {
                path = GetPath();
            }
            try
            {
                //Pass the filepath and filename to the StreamWriter Constructor
                StreamWriter sw = new StreamWriter(@path + "/" + file_name);
                //Write a line of text
                sw.WriteLine(text);
                //Write a second line of text
                //sw.WriteLine("From the StreamWriter class");
                //Close the file
                sw.Close();
            }
            catch (Exception)
            {
                //Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                //Console.WriteLine("Executing finally block.");
            }
        }
    }
}
