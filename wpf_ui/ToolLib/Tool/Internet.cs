using DotRas;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolKHBrowser;
using ToolKHBrowser.ToolLib.Tool;
using ToolKHBrowser.ViewModels;

namespace ToolKHBrowser.ToolLib.Tool
{
    class VPNData
    {
        public static RasPhoneBook Pbk = new RasPhoneBook();
        public static String PbkPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.User);

        public static bool NameExists(String name)
        {
            foreach (RasEntry pbkEntry in VPNData.Pbk.Entries)
            {
                if (pbkEntry.Name.Equals(name))
                {
                    return true;
                }
            }
            return false;
        }
    }
    public static class Internet
    {
        private static RasDialer dial = new RasDialer();
        public static void Connect(string connection_name = "")
        {
            if (!connection_name.Equals(""))
            {
                VPNData.Pbk.Open(VPNData.PbkPath);
                for (int connectionNumber = 0; connectionNumber < VPNData.Pbk.Entries.Count; connectionNumber++)
                {
                    if (connection_name.Equals(VPNData.Pbk.Entries[connectionNumber].Name))
                    {
                        try
                        {
                            dial.EntryName = VPNData.Pbk.Entries[connectionNumber].Name;
                            dial.Credentials = VPNData.Pbk.Entries[connectionNumber].GetCredentials();
                            dial.PhoneBookPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.User);
                            dial.DialAsync();

                            if (dial.Credentials.Password.Equals(""))
                            {
                                //dial.DialAsyncCancel();
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
        }
        public static void Disconnect(string connection_name = "")
        {
            foreach (RasConnection connection in RasConnection.GetActiveConnections())
            {
                if (connection.EntryName.Equals(connection_name))
                {
                    connection.HangUp();
                    break;
                }
            }
        }
        public static string GetIPAddress()
        {
            string IPAddress = string.Empty;
            IPHostEntry Host = default(IPHostEntry);
            string Hostname = null;
            Hostname = System.Environment.MachineName;
            Host = Dns.GetHostEntry(Hostname);
            foreach (IPAddress IP in Host.AddressList)
            {
                if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    IPAddress = Convert.ToString(IP);
                }
            }

            return IPAddress;
        }
        private static void StartBrowserResetIP(IWebDriver driver, string protocol_id)
        {
            int loop = 0;
            do
            {
                try
                {
                    Thread.Sleep(1000);
                    driver.FindElement(By.Id(protocol_id)).Click();
                    loop = 100;
                }
                catch (Exception) { loop++; }
            } while (loop < 5);
        }
        private static void CellularResetIP(string interfaces, string profile)
        {
            try
            {
                string _str = "nesh mbn disconnect interface=A0405BA6-8319-43D6-84AA-548FF2673F29";
                _str.Replace('|', '"');

            }
            catch (Exception) { }
        }
        private static void WiFiConnect()
        {
            ProcessStartInfo info1 = new ProcessStartInfo();
            info1.FileName = "ipconfig";
            info1.Arguments = "/renew"; // connect
            info1.WindowStyle = ProcessWindowStyle.Hidden;
            Process p1 = Process.Start(info1);
            p1.WaitForExit();
        }
        private static void WiFiDisconnect()
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "ipconfig";
            info.Arguments = "/release"; // disconnect
            info.WindowStyle = ProcessWindowStyle.Hidden;
            Process p = Process.Start(info);
            p.WaitForExit();
        }
        private static void CloseBrowser(string commend)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + commend;
            process.StartInfo = startInfo;
            process.Start();
        }
        private static void BrowserCodeResetIP(string code)
        {
            IWebDriver driver = null;
            Boolean is_first = true;
            int loop = 0;
            string[] arr = code.Split('\n');
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = arr[i].Trim();
                if (!string.IsNullOrEmpty(arr[i]))
                {
                    string[] a = arr[i].Split('=');
                    if (a.Length > 1)
                    {
                        switch (a[0])
                        {
                            case "SCROLL":
                                int scroll = Int32.Parse(a[1].ToString());
                                for (int s = 0; s < scroll; s++)
                                {
                                    FBTool.Scroll(driver, 500);
                                }
                                break;
                            case "XPROXY":
                                try
                                {
                                    driver.Navigate().GoToUrl(a[1]);
                                }
                                catch { }
                                FBTool.WaitingPageLoading(driver);
                                break;
                            case "XPATH_INPUT":
                                string[] a11 = a[1].Split(',');

                                if (a11.Length > 0)
                                {
                                    loop = 0;
                                    do
                                    {
                                        Thread.Sleep(500);
                                        try
                                        {
                                            driver.FindElement(By.XPath(a11[0])).SendKeys(a11[1]);
                                            loop = 100;
                                        }
                                        catch (Exception) { }
                                        loop++;
                                    } while (loop < 10);
                                }
                                break;
                            case "CMD":
                                string[] a2 = a[1].Split(',');
                                string con = "";
                                try
                                {
                                    con = a2[1];
                                }
                                catch { }
                                if (a2[0] == "CONNECT")
                                {
                                    Connect(con);
                                }
                                else if (a2[0] == "DISCONNECT")
                                {
                                    Disconnect(con);
                                }
                                break;
                            case "CMD_CLOSE_BROWSER":
                                try
                                {
                                    CloseBrowser(a[1]);
                                }
                                catch (Exception) { }
                                break;
                            case "WIFI":
                                string[] aw = a[1].Split(',');
                                if (aw[0] == "CONNECT")
                                {
                                    WiFiConnect();
                                }
                                else if (aw[0] == "DISCONNECT")
                                {
                                    WiFiDisconnect();
                                }
                                break;
                            case "FRAM":
                                try
                                {
                                    driver.SwitchTo().Frame(Int32.Parse(a[1]));
                                }
                                catch (Exception) { }
                                break;
                            case "GOTO":
                                if (is_first)
                                {
                                    driver = new MyWebDriver().GetWebDriver("", 1, 6);
                                    Thread.Sleep(1000);
                                }
                                if (a.Length > 2)
                                {
                                    try
                                    {
                                        driver.Navigate().GoToUrl(a[1] + "=" + a[2]);
                                    }
                                    catch (Exception) { }
                                }
                                else
                                {
                                    try
                                    {
                                        driver.Navigate().GoToUrl(a[1]);
                                    }
                                    catch (Exception) { }
                                }
                                FBTool.WaitingPageLoading(driver);
                                break;
                            case "WAIT":
                                try
                                {
                                    Thread.Sleep(Int32.Parse(a[1].ToString()));
                                }
                                catch (Exception) { }
                                break;
                            case "INPUT":
                                string[] a1 = a[1].Split(',');

                                if (a1.Length > 0)
                                {
                                    loop = 0;
                                    do
                                    {
                                        try
                                        {
                                            driver.FindElement(By.Id(a1[0])).SendKeys(a1[1]);
                                            loop = 100;
                                        }
                                        catch (Exception) { }
                                        loop++;
                                    } while (loop < 10);
                                }
                                break;
                            case "BUTTON":
                                loop = 0;
                                do
                                {
                                    Thread.Sleep(200);
                                    try
                                    {
                                        driver.FindElement(By.Id(a[1])).Click();
                                        loop = 100;
                                    }
                                    catch (Exception) { }
                                    loop++;
                                } while (loop < 10);
                                break;
                            case "XPATH_CLICK":
                                loop = 0;
                                do
                                {
                                    Thread.Sleep(200);
                                    try
                                    {
                                        driver.FindElement(By.XPath(a[1])).Click();
                                        loop = 100;
                                    }
                                    catch (Exception) { }
                                    loop++;
                                } while (loop < 10);
                                break;
                        }
                    }
                    is_first = false;
                }
            }
            WaitingInternet();
            if (driver != null)
            {
                try
                {
                    driver.Quit();
                }
                catch (Exception) { }
            }
        }
        public static void ResetIP(IWebDriver driver = null)
        {
            var cache = DIConfig.Get<ICacheViewModel>().GetCacheDao().Get("config:configModem");
            if (cache != null && cache.Value != null)
            {
                BrowserCodeResetIP(cache.Value.ToString());
            }
            WaitingInternet();
            Thread.Sleep(2000);
        }
        public static Boolean IsInternetAvailable()
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                //Do your stuffs when network available
                return true;

            }
            else
            {
                //When network not available,  
                return false;
            }
        }
        public static Boolean WaitingInternet()
        {
            bool is_internet = false;
            int loop = 0;
            do
            {
                Thread.Sleep(10);
                is_internet = IsInternetAvailable();
                loop++;
            } while (!is_internet && loop < 1000);

            return is_internet;
        }
    }
}
