using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolKHBrowser.ToolLib.Data;
using ToolKHBrowser.ViewModels;
using ToolLib.Http;
using WpfUI.ToolLib.Data;

namespace ToolLib.Tool
{
    public static class LocalData
    {
        public static void RunCMD(string command)
        {
            //try
            //{
            //    System.Diagnostics.Process process = new System.Diagnostics.Process();
            //    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            //    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //    //startInfo.FileName = "cmd.exe";
            //    startInfo.Arguments = "/C " + command;
            //    process.StartInfo = startInfo;
            //    process.Start();
            //}
            //catch (Exception) { }
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C " + command,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process.Start(startInfo);
            }
            catch (Exception) { }
        }
        public static string GetPath()
        {
            return System.Environment.CurrentDirectory;
        }
        public static string GetPathTMP()
        {
            return GetPath() + "/tmp";
        }
        public static string GetPathDriver()
        {
            return GetPath() + "/driver";
        }
        public static string GetBrowserDataDirectory()
        {
            return ConfigData.GetBrowserDataDirectory();
        }
        public static void CatchGroupNoPending(string gId, string fileName= "cache_group_page_profile.txt")
        {
            string path = GetPath() + "\\" + @fileName;
            if(File.Exists(path))
            {
                using (StreamWriter stream = File.AppendText(path))
                {
                    stream.WriteLine(gId);
                }
            }            
        }
        public static string[] GetFiles(string dir)
        {
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                try
                {
                    return Directory.GetFiles(dir);
                }
                catch (Exception) { }
            }

            return null;
        }
        public static void DeleteFile(string file)
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception) { }
        }
        public static void DeleteProfile(string profile)
        {
            try
            {
                Directory.Delete(GetBrowserDataDirectory() + "\\" + profile, true);
            }
            catch (Exception) { }
        }
        public static void ClearCache(string profile)
        {
            ObservableCollection<string> cacheList = new ObservableCollection<string>()
            {
                "GrShaderCache",
                "ShaderCache",
                "SmartScreen",
                "hyphen-data",
                "Edge Shopping",
                "Edge Kids Mode",
                "Edge Travel",
                "Edge Wallet",
                "Subresource Filter",
                "Speech Recognition",
                "Autofill",
                "chrome_debug.log",
                "WidevineCdm",
                "SwReporter",
                "pnacl",
                "OnDeviceHeadSuggestModel",
                "MEIPreload",
                "MediaFoundationWidevineCdm",
                "BrowserMetrics",

                "Crashpad",
                "EdgeOnnxRuntimeDirectML",
                "optimization_guide_model_store",

                "Default\\Cache",
                "Default\\Code Cache",
                "Default\\GPUCache",
                "Default\\IndexedDB",
                "Default\\Service Worker",
                "Default\\Session Storage",
                "Default\\optimization_guide_prediction_model_downloads"
            };
            foreach(var dir in cacheList)
            {
                string folder = profile;
                if(!profile.Contains(":")) 
                {
                    folder = GetBrowserDataDirectory() + "\\" + profile;
                }

                try
                {
                    Directory.Delete(folder + "\\" + dir, true);
                }
                catch (Exception) { }
            }            
        }
        public static string[] getArrayList(string dir)
        {
            string[] arr = null;
            if (!string.IsNullOrEmpty(dir))
            {
                if (File.Exists(dir))
                {
                    string str = LocalData.ReadTextFromTextFile(dir.Trim(), true);

                    arr = str.Trim().Split('\n');
                }
            }

            return arr;
        }
        public static string GetDomain()
        {
            string domain = ReadTextFromTextFile("domain.txt");
            if (string.IsNullOrEmpty(domain))
            {
                domain = "http://api.toolkh24.com";
            }

            return domain;
        }
        public static string[] GetFile(string dir)
        {
            if (!string.IsNullOrEmpty(dir) && !File.Exists(dir))
            {
                try
                {
                    return Directory.GetFiles(dir);
                }
                catch(Exception) { }
            }

            return null;
        }
        public static string ReadTextFromTextFile(string file_name, bool isDir = false)
        {
            string line = "", str = "";
            string path = file_name;
            if (!isDir)
            {
                path = GetPath() + "/" + file_name;
            }
            try
            {
                StreamReader sr = new StreamReader(path);
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
        public static void WriteTMP(string file_name, string text)
        {
            Write(file_name, text, GetPathTMP());
        }
        public static string ReadTMP(string file_name)
        {
            return Read(file_name, GetPathTMP());
        }
        public static string Read(string file_name, string path = "")
        {
            if (string.IsNullOrEmpty(path))
            {
                path = GetPath();
            }
            string line = "";

            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader(path + "/" + file_name);
                //Read the first line of text
                line = sr.ReadLine();
                //Continue to read until you reach end of file
                /*while (line != null)
                {
                    //Read the next line
                    line = sr.ReadLine();
                }
                */
                //close the file
                sr.Close();
                //Console.ReadLine();
            }
            catch (Exception)
            {
                //Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                //Console.WriteLine("Executing finally block.");
            }

            return line;//JsonSerializer.Serialize(line);
        }
        public static string GetHotmailCodeVerify_HotmailBox(string hotmail, string password)
        {
            try
            {
                var http = ToolDiConfig.Get<HttpHelper>();
                var url = "https://getcode.hotmailbox.me/facebook?email=" + hotmail + "&password=" + password + "&timeout=60";
                var response = http.Get(url);
                var resp = response.GetResponse<Dictionary<string, object>>();

                return resp["VerificationCode"] + "";
            }
            catch (Exception e)
            {
                //log.Error("error call to 2fa : " + token, e);
            }

            return "";
        }
        public static string GetHotmailCodeVerify(string hotmail, string password)
        {
            try
            {
                var http = ToolDiConfig.Get<HttpHelper>();
                var url = "https://tools.dongvanfb.net/api/get_code?mail=" + hotmail + "&pass=" + password + "&type=facebook";
                var response = http.Get(url);
                var resp = response.GetResponse<Dictionary<string, object>>();

                return resp["code"] + "";
            }
            catch (Exception)
            {
                //log.Error("error call to 2fa : " + token, e);
            }

            return "";
        }
        public static string GetFiveSimCodeVerify(string apikey, string order_id)
        {
            try
            {
                var http = ToolDiConfig.Get<HttpHelper>();
                var url = "https://5sim.net/v1/user/check/" + order_id;
                var pheaders = new Dictionary<string, string>();
                pheaders["Authorization"] = "Bearer " + apikey;
                pheaders["Accept"] = "application/json";
                var response = http.Get(url, pheaders);
                var resp = response.GetResponse<Dictionary<string, object>>();
                var sms = resp["sms"];
                string[] arr = ((IEnumerable)sms).Cast<object>()
                             .Select(x => x.ToString())
                             .ToArray();
                Code data = JsonConvert.DeserializeObject<Code>(arr[0] + "");

                return data.code.ToString();
            }
            catch (Exception)
            {
                
            }

            return "";
        }
        public static string FiveSimBuyPhoneNumber(string apikey, string country, string operators= "any", string product = "facebook")
        {
            try
            {
                var http = ToolDiConfig.Get<HttpHelper>();
                var url = "https://5sim.net/v1/user/buy/activation/" + country.ToLower().Trim() + "/" + operators.ToLower().Trim() + "/" + product.ToLower().Trim();

                var pheaders = new Dictionary<string, string>();
                pheaders["Authorization"] = "Bearer " + apikey;
                pheaders["Accept"] = "application/json";

                var response = http.Get(url,pheaders);
                var resp = response.GetResponse<Dictionary<string, string>>();

                return resp["phone"]+"|"+resp["id"];
            }
            catch (Exception)
            {
                //Console.WriteLine("error call to 2fa : ", e);
            }

            return "";
        }
        public static string GetHotmail(string apikey = "", string domain = "OUTLOOK")
        {
            try
            {
                var http = ToolDiConfig.Get<HttpHelper>();
                var url = "https://api.hotmailbox.me/mail/buy?apikey=" + apikey + "&mailcode=" + domain + "&quantity=1";
                var response = http.Get(url);
                var resp = response.GetResponse<Dictionary<string, object>>();

                HotmailboxBuyMailData data = JsonConvert.DeserializeObject<HotmailboxBuyMailData>(resp["Data"] + "");

                return data.emails[0].Email.ToString() + "|" + data.emails[0].Password.ToString();
            }
            catch (Exception e)
            {
                //Console.WriteLine("error call to 2fa : ", e);
            }

            return "";
        }
    }
}
