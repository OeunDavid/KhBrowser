using Emgu.CV.Ocl;
using Microsoft.Edge.SeleniumTools;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chrome.ChromeDriverExtensions;
using OpenQA.Selenium.Interactions;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using ToolKHBrowser.ToolLib.Data;
using ToolKHBrowser.ViewModels;

namespace ToolKHBrowser.ToolLib.Tool
{
    public class MyWebDriver
    {
        private EdgeDriverService edge_driver_service = null;
        private ChromeDriverService driver_service = null;
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public IWebDriver GetWebDriver(string browserKey = "", int requestScreen = 0, int numberScreen = 0, string userAgent = "", string proxy = "", bool useImage = true, bool isShowBrowser = true)
        {
            if (ConfigData.GetBrowserType() == "edge")
            {
                return GetEdgeDriver(browserKey, requestScreen, numberScreen, proxy, useImage, isShowBrowser, userAgent);
            }

            return GetChromeDriver(browserKey, requestScreen, numberScreen, proxy, useImage, isShowBrowser, userAgent);
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public IWebDriver GetEdgeDriver(
            string browser_key = "",
            int num_request = 3,
            int num_open = 6,
            string str_proxy = "",
            bool useImage = true,
            bool isShowBrowser = true,
            string userAgent = "")
        {
            // ✅ your real exe name
            //string driverExe = "microsoftedgedriver.exe";
            string driverExe = "msedgedriver.exe";


            string driverDir = ConfigData.GetPathDriver();
            string driverPath = Path.Combine(driverDir, driverExe);

            // ✅ clear error if missing
            if (!File.Exists(driverPath))
                throw new FileNotFoundException("EdgeDriver not found: " + driverPath);

            edge_driver_service = EdgeDriverService.CreateChromiumService(driverDir, driverExe);
            edge_driver_service.HideCommandPromptWindow = true;

            var options = new EdgeOptions();
            options.UseChromium = true;
            options.PageLoadStrategy = PageLoadStrategy.Eager;

            // ✅ better stability
            options.AddArgument("--no-first-run");
            options.AddArgument("--no-default-browser-check");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-popup-blocking");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--remote-debugging-port=0");

            if (!isShowBrowser)
                options.AddArgument("--headless=new");

            if (!string.IsNullOrWhiteSpace(userAgent))
                options.AddArgument("--user-agent=" + userAgent);

            // ✅ profile fix (unique per screen)
            try
            {
                string dir = ConfigData.GetBrowserDataDirectory();
                if (!string.IsNullOrEmpty(browser_key))
                {
                    string profileDir = Path.Combine(dir, $"{browser_key}__{num_request}");
                    Directory.CreateDirectory(profileDir);
                    options.AddArgument("--user-data-dir=" + profileDir);
                }
            }
            catch { }

            // images
            if (useImage)
                options.AddArgument("--blink-settings=imagesEnabled=true");
            else
                options.AddArgument("--blink-settings=imagesEnabled=false");

            // proxy
            if (!string.IsNullOrWhiteSpace(str_proxy))
            {
                var proxy = new Proxy
                {
                    Kind = ProxyKind.Manual,
                    SslProxy = str_proxy,
                    HttpProxy = str_proxy,
                    FtpProxy = str_proxy
                };
                options.Proxy = proxy;
            }

            IWebDriver driver = null;
            try
            {
                driver = new EdgeDriver(edge_driver_service, options);
            }
            catch (Exception ex)
            {
                // if failed, try to kill existing processes and retry ONCE
                if (ex.Message.Contains("DevToolsActivePort") || ex.Message.Contains("session not created"))
                {
                    KillBrowserProcesses("msedge", "msedgedriver");
                    Thread.Sleep(2000);
                    try
                    {
                        driver = new EdgeDriver(edge_driver_service, options);
                    }
                    catch { }
                }
            }

            if (driver != null)
                Thread.Sleep(800);

            return driver;
        }

        private void KillBrowserProcesses(params string[] processNames)
        {
            try
            {
                foreach (var name in processNames)
                {
                    var processes = Process.GetProcessesByName(name);
                    foreach (var p in processes)
                    {
                        try
                        {
                            p.Kill();
                            p.WaitForExit(3000);
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }
        //public IWebDriver GetEdgeDriver(string browser_key = "", int num_request = 3, int num_open = 6, string str_proxy = "", bool useImage= true, bool isShowBrowser = true, string userAgent= "")
        //{
        //    string msedgedriverExe = @"microsoftedgedriver.exe";
        //    edge_driver_service = EdgeDriverService.CreateChromiumService(@"" + ConfigData.GetPathDriver(), msedgedriverExe);
        //    edge_driver_service.HideCommandPromptWindow = true;

        //    EdgeOptions options = new EdgeOptions();
        //    options.UseChromium = true;
        //    options.PageLoadStrategy = PageLoadStrategy.Eager;

        //    var cacheViewModel= DIConfig.Get<ICacheViewModel>();
        //    int width = cacheViewModel.GetCacheDao().Get("screen:width").Total;
        //    int height = cacheViewModel.GetCacheDao().Get("screen:height").Total;
        //    string deviceType = cacheViewModel.GetCacheDao().Get("config:deviceType").Value;

        //    if (width <= 0)
        //    {
        //        width = (int)System.Windows.SystemParameters.VirtualScreenWidth;
        //    }
        //    if (height <= 0)
        //    {
        //        height = (int)System.Windows.SystemParameters.VirtualScreenHeight;
        //    }
        //    double h, w, x = 0, y = 0;

        //    num_request--;
        //    if (num_open <= 3)
        //    {
        //        if (num_open == 1)
        //        {
        //            w = width;
        //            h = height;
        //        }
        //        else if (num_open == 2)
        //        {
        //            w = width / 2;
        //            h = height;
        //            x = w * num_request;
        //        }
        //        else
        //        {
        //            w = width / 3;
        //            h = height;
        //            x = w * num_request;
        //        }
        //    }
        //    else
        //    {
        //        h = height / 2;
        //        w = width / 3;
        //        if (num_request >= 3)
        //        {
        //            y = h;
        //            num_request = num_request % 3;
        //        }
        //        x = w * num_request;
        //    }
        //    options.AddArguments(@"--window-size=" + w + "," + h);
        //    options.AddArgument(String.Format(@"--window-position={0},{1}", x, y));

        //    options.AddArguments("--disable-notifications", "start-maximized", "--no-sandbox", "--disable-gpu", "--disable-dev-shm-usage", "--disable-web-security", "--disable-rtc-smoothness-algorithm", "--disable-webrtc-hw-decoding", "--disable-webrtc-hw-encoding", "--disable-webrtc-multiple-routes", "--disable-webrtc-hw-vp8-encoding", "--enforce-webrtc-ip-permission-check", "--force-webrtc-ip-handling-policy", "--ignore-certificate-errors", "--disable-infobars", "--disable-popup-blocking");
        //    options.AddUserProfilePreference("profile.default_content_setting_values.notifications", 1);
        //    options.AddUserProfilePreference("profile.default_content_setting_values.plugins", 1);
        //    options.AddUserProfilePreference("profile.default_content_setting_values.popups", 1);
        //    options.AddUserProfilePreference("profile.default_content_setting_values.geolocation", 1);
        //    options.AddUserProfilePreference("profile.default_content_setting_values.auto_select_certificate", 1);
        //    options.AddUserProfilePreference("profile.default_content_setting_values.mixed_script", 1);
        //    options.AddUserProfilePreference("profile.default_content_setting_values.media_stream", 1);
        //    options.AddUserProfilePreference("profile.default_content_setting_values.media_stream_mic", 1);
        //    options.AddUserProfilePreference("profile.default_content_setting_values.media_stream_camera", 1);
        //    options.AddUserProfilePreference("profile.default_content_setting_values.protocol_handlers", 1);
        //    options.AddUserProfilePreference("profile.default_content_setting_values.midi_sysex", 1);
        //    options.AddUserProfilePreference("profile.default_content_setting_values.push_messaging", 1);
        //    options.AddUserProfilePreference("profile.default_content_setting_values.ssl_cert_decisions", 1);
        //    options.AddUserProfilePreference("profile.default_content_setting_values.metro_switch_to_desktop", 1);
        //    options.AddUserProfilePreference("profile.default_content_setting_values.protected_media_identifier", 1);
        //    options.AddUserProfilePreference("profile.default_content_setting_values.site_engagement", 1);
        //    options.AddUserProfilePreference("profile.default_content_setting_values.durable_storage", 1);
        //    options.AddUserProfilePreference("useAutomationExtension", true);

        //    //options.AddArguments(@"--disable-dev-shm-usage");
        //    //options.AddArguments(@"--lang=en-US");
        //    //options.AddArguments(@"--disable-notifications");
        //    //options.AddArguments(@"--disable-popup-blocking");
        //    ////options.AddAdditionalCapability("useAutomationExtension", false);
        //    ////options.AddExcludedArgument("enable-automation");

        //    ////Hide scrollbars for special pages
        //    ////options.AddArguments(@"--hide-scrollbars");

        //    ////Each tag uses a separate process
        //    ////options.AddArguments(@"--process-per-tab");
        //    ////Each site uses a separate process
        //    ////options.AddArguments(@"--process-per-site");

        //    ////Cancel Sandbox Mode
        //    ////options.AddArguments(@"--no-sandbox");
        //    ////Specify the Cache size in Byte
        //    //options.AddArguments(@"--disk-cache-size=0");
        //    ////Google Documents mentions the need to add this attribute to avoid bug s
        //    //options.AddArguments(@"--disable-gpu");
        //    ////--disable-plugins
        //    //options.AddArguments(@"--disable-plugins");

        //    ////options.AddUserProfilePreference("profile.cookie_controls_mode", 0);


        //    //options.AddArguments(@"--disable-session-crashed-bubble");
        //    //options.AddArguments(@"--disable-application-cache");
        //    //options.AddArguments(@"--disable-dev-shm-usage");
        //    //options.AddArguments(@"--ignore-certificate-errors");

        //    if (!isShowBrowser)
        //    {
        //        options.AddArguments(@"--headless");
        //    }
        //    if (useImage)
        //    {
        //        options.AddUserProfilePreference("profile.default_content_setting_values.images", 1);

        //        options.AddArguments(@"--enable-images");
        //        options.AddArguments(@"--blink-settings=imagesEnabled=true");
        //    }
        //    else
        //    {
        //        options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);

        //        //Disable images
        //        options.AddArguments(@"--disable-images");
        //        //Increase speed without loading pictures
        //        options.AddArguments(@"--blink-settings=imagesEnabled=false");
        //    }
        //    if (userAgent != "")
        //    {
        //        options.AddArguments(@"--user-agent=" + userAgent);
        //    }
        //    if (deviceType.Contains("app"))
        //    {
        //        options.AddArguments(@"--app=https://www.facebook.com");
        //    }
        //    IWebDriver driver = null;
        //    try
        //    {
        //        string dir = ConfigData.GetBrowserDataDirectory();
        //        if (!string.IsNullOrEmpty(browser_key))
        //        {
        //            options.AddArguments(@"--user-data-dir=" + dir + "\\" + browser_key);
        //        }
        //        if (!string.IsNullOrEmpty(str_proxy))
        //        {
        //            Proxy proxy = new Proxy();
        //            proxy.Kind = ProxyKind.Manual;

        //            proxy.SslProxy = str_proxy;
        //            proxy.HttpProxy = str_proxy;
        //            proxy.FtpProxy = str_proxy;
        //            options.Proxy = proxy;
        //        }

        //        driver = new EdgeDriver(edge_driver_service, options);
        //    }
        //    catch (WebDriverException) { }
        //    if (driver != null)
        //    {
        //        Thread.Sleep(1000);
        //        try
        //        {
        //            ((IJavaScriptExecutor)driver).ExecuteScript("return window.stop");
        //        }
        //        catch (Exception) { }
        //    }

        //    return driver;
        //}
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public IWebDriver GetChromeDriver(
    string browser_key = "",
    int num_request = 3,
    int num_open = 6,
    string str_proxy = "",
    bool useImage = true,
    bool isShowBrowser = true,
    string userAgent = "")
        {
            ChromeOptions options = new ChromeOptions();

            // ===== window size/position (keep your logic) =====
            var cacheViewModel = DIConfig.Get<ICacheViewModel>();
            int width = 0, height = 0;
            string deviceType = "";

            try
            {
                width = cacheViewModel.GetCacheDao().Get("screen:width").Total;
                height = cacheViewModel.GetCacheDao().Get("screen:height").Total;

                var cacheDevice = cacheViewModel.GetCacheDao().Get("config:deviceType");
                if (cacheDevice?.Value != null) deviceType = cacheDevice.Value.ToString();
            }
            catch { }

            if (width <= 0) width = (int)System.Windows.SystemParameters.VirtualScreenWidth;
            if (height <= 0) height = (int)System.Windows.SystemParameters.VirtualScreenHeight;

            double h, w, x = 0, y = 0;
            num_request--;
            if (num_open <= 3)
            {
                if (num_open == 1) { w = width; h = height; }
                else if (num_open == 2) { w = width / 2; h = height; x = w * num_request; }
                else { w = width / 3; h = height; x = w * num_request; }
            }
            else
            {
                h = height / 2;
                w = width / 3;
                if (num_request >= 3) { y = h; num_request = num_request % 3; }
                x = w * num_request;
            }

            options.AddArgument($"--window-size={w},{h}");
            options.AddArgument($"--window-position={x},{y}");

            // ===== options =====
            options.AddExcludedArgument("enable-automation"); // correct (no --)
            options.AddUserProfilePreference("useAutomationExtension", false);

            options.AddArguments(
                "--no-sandbox",
                "--disable-gpu",
                "--disable-dev-shm-usage",
                "--disable-notifications",
                "--disable-popup-blocking",
                "--disable-infobars",
                "--ignore-certificate-errors",
                "--remote-debugging-port=0"
            );

            if (!isShowBrowser)
                options.AddArgument("--headless");

            options.AddArgument(useImage ? "--blink-settings=imagesEnabled=true"
                                         : "--blink-settings=imagesEnabled=false");

            if (!string.IsNullOrWhiteSpace(userAgent))
                options.AddArgument("--user-agent=" + userAgent);

            // Profile folder (use your tool folder, not real Chrome/Edge profile)
            if (!string.IsNullOrEmpty(browser_key))
            {
                string baseDirProfile = ConfigData.GetBrowserDataDirectory(); // ensure this is CHROME tool directory
                string profileDir = Path.Combine(baseDirProfile, browser_key);
                Directory.CreateDirectory(profileDir);
                options.AddArgument("--user-data-dir=" + profileDir);
            }

            if (!string.IsNullOrEmpty(deviceType) && deviceType.Contains("app"))
                options.AddArgument("--app=https://www.facebook.com");

            // Proxy
            if (!string.IsNullOrWhiteSpace(str_proxy))
            {
                var proxy = new Proxy
                {
                    Kind = ProxyKind.Manual,
                    SslProxy = str_proxy,
                    HttpProxy = str_proxy,
                    FtpProxy = str_proxy
                };
                options.Proxy = proxy;
            }

            // ===== find chromedriver.exe (project driver folder OR output driver folder) =====
            string chromeDriverPath = FindDriverExe("chromedriver.exe");
            if (chromeDriverPath == null)
                throw new FileNotFoundException(
                    "chromedriver.exe not found.\n" +
                    "Put it in your project folder: /driver/chromedriver.exe\n" +
                    "Or set file properties: Copy to Output Directory."
                );

            string driverDir = Path.GetDirectoryName(chromeDriverPath);

            driver_service = ChromeDriverService.CreateDefaultService(driverDir, "chromedriver.exe");
            driver_service.HideCommandPromptWindow = true;

            try
            {
                var driver = new ChromeDriver(driver_service, options);
                Thread.Sleep(500);
                return driver;
            }
            catch (Exception ex)
            {
                // retry once after kill (common session/devtools issues)
                if (ex.Message.Contains("DevToolsActivePort") || ex.Message.Contains("session not created"))
                {
                    KillBrowserProcesses("chrome", "chromedriver");
                    Thread.Sleep(1500);

                    var driver = new ChromeDriver(driver_service, options);
                    Thread.Sleep(500);
                    return driver;
                }

                throw;
            }
        }
        private static string FindDriverExe(string exeName)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            string[] paths =
            {
        Path.Combine(baseDir, "driver", exeName), // output\driver
        Path.Combine(baseDir, exeName),           // output
        Path.Combine(baseDir, "..", "..", "..", "driver", exeName), // project\driver
        Path.Combine(baseDir, "..", "..", "..", "..", "driver", exeName) // sometimes extra level
    };

            foreach (var p in paths)
            {
                var full = Path.GetFullPath(p);
                if (File.Exists(full)) return full;
            }

            return null;
        }
    }
}
