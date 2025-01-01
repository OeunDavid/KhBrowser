using System;
using System.Drawing;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using Emgu.CV.Ocl;
using Microsoft.Edge.SeleniumTools;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chrome.ChromeDriverExtensions;
using OpenQA.Selenium.Interactions;
using WpfUI.ToolLib.Data;
using WpfUI.ViewModels;

namespace WpfUI.ToolLib.Tool
{
    public class MyWebDriver
    {
        private EdgeDriverService edge_driver_service = null;
        private ChromeDriverService driver_service = null;
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public IWebDriver GetWebDriver(string browserKey = "", int requestScreen = 0, int numberScreen = 0, string userAgent= "", string proxy = "", bool useImage= true, bool isShowBrowser = true)
        {
            if (ConfigData.GetBrowserType() == "edge")
            {
                return GetEdgeDriver(browserKey, requestScreen, numberScreen, proxy, useImage, isShowBrowser,userAgent);
            }

            return GetChromeDriver(browserKey, requestScreen, numberScreen, proxy, useImage, isShowBrowser, userAgent);
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public IWebDriver GetEdgeDriver(string browser_key = "", int num_request = 3, int num_open = 6, string str_proxy = "", bool useImage= true, bool isShowBrowser = true, string userAgent= "")
        {
            string msedgedriverExe = @"microsoftedgedriver.exe";
            edge_driver_service = EdgeDriverService.CreateChromiumService(@"" + ConfigData.GetPathDriver(), msedgedriverExe);
            edge_driver_service.HideCommandPromptWindow = true;

            EdgeOptions options = new EdgeOptions();
            options.UseChromium = true;
            options.PageLoadStrategy = PageLoadStrategy.Eager;

            var cacheViewModel= DIConfig.Get<ICacheViewModel>();
            int width = cacheViewModel.GetCacheDao().Get("screen:width").Total;
            int height = cacheViewModel.GetCacheDao().Get("screen:height").Total;
            string deviceType = cacheViewModel.GetCacheDao().Get("config:deviceType").Value;

            if (width <= 0)
            {
                width = Int32.Parse(System.Windows.SystemParameters.VirtualScreenWidth.ToString());
            }
            if (height <= 0)
            {
                height = Int32.Parse(System.Windows.SystemParameters.VirtualScreenHeight.ToString());
            }
            double h, w, x = 0, y = 0;

            num_request--;
            if (num_open <= 3)
            {
                if (num_open == 1)
                {
                    w = width;
                    h = height;
                }
                else if (num_open == 2)
                {
                    w = width / 2;
                    h = height;
                    x = w * num_request;
                }
                else
                {
                    w = width / 3;
                    h = height;
                    x = w * num_request;
                }
            }
            else
            {
                h = height / 2;
                w = width / 3;
                if (num_request >= 3)
                {
                    y = h;
                    num_request = num_request % 3;
                }
                x = w * num_request;
            }
            options.AddArguments(@"--window-size=" + w + "," + h);
            options.AddArgument(String.Format(@"--window-position={0},{1}", x, y));

            options.AddArguments("--disable-notifications", "start-maximized", "--no-sandbox", "--disable-gpu", "--disable-dev-shm-usage", "--disable-web-security", "--disable-rtc-smoothness-algorithm", "--disable-webrtc-hw-decoding", "--disable-webrtc-hw-encoding", "--disable-webrtc-multiple-routes", "--disable-webrtc-hw-vp8-encoding", "--enforce-webrtc-ip-permission-check", "--force-webrtc-ip-handling-policy", "--ignore-certificate-errors", "--disable-infobars", "--disable-popup-blocking");
            options.AddUserProfilePreference("profile.default_content_setting_values.notifications", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.plugins", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.popups", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.geolocation", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.auto_select_certificate", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.mixed_script", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.media_stream", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.media_stream_mic", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.media_stream_camera", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.protocol_handlers", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.midi_sysex", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.push_messaging", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.ssl_cert_decisions", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.metro_switch_to_desktop", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.protected_media_identifier", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.site_engagement", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.durable_storage", 1);
            options.AddUserProfilePreference("useAutomationExtension", true);

            //options.AddArguments(@"--disable-dev-shm-usage");
            //options.AddArguments(@"--lang=en-US");
            //options.AddArguments(@"--disable-notifications");
            //options.AddArguments(@"--disable-popup-blocking");
            ////options.AddAdditionalCapability("useAutomationExtension", false);
            ////options.AddExcludedArgument("enable-automation");

            ////Hide scrollbars for special pages
            ////options.AddArguments(@"--hide-scrollbars");

            ////Each tag uses a separate process
            ////options.AddArguments(@"--process-per-tab");
            ////Each site uses a separate process
            ////options.AddArguments(@"--process-per-site");

            ////Cancel Sandbox Mode
            ////options.AddArguments(@"--no-sandbox");
            ////Specify the Cache size in Byte
            //options.AddArguments(@"--disk-cache-size=0");
            ////Google Documents mentions the need to add this attribute to avoid bug s
            //options.AddArguments(@"--disable-gpu");
            ////--disable-plugins
            //options.AddArguments(@"--disable-plugins");

            ////options.AddUserProfilePreference("profile.cookie_controls_mode", 0);


            //options.AddArguments(@"--disable-session-crashed-bubble");
            //options.AddArguments(@"--disable-application-cache");
            //options.AddArguments(@"--disable-dev-shm-usage");
            //options.AddArguments(@"--ignore-certificate-errors");

            if (!isShowBrowser)
            {
                options.AddArguments(@"--headless");
            }
            if (useImage)
            {
                options.AddUserProfilePreference("profile.default_content_setting_values.images", 1);

                options.AddArguments(@"--enable-images");
                options.AddArguments(@"--blink-settings=imagesEnabled=true");
            }
            else
            {
                options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);

                //Disable images
                options.AddArguments(@"--disable-images");
                //Increase speed without loading pictures
                options.AddArguments(@"--blink-settings=imagesEnabled=false");
            }
            if (userAgent != "")
            {
                options.AddArguments(@"--user-agent=" + userAgent);
            }
            if (deviceType.Contains("app"))
            {
                options.AddArguments(@"--app=https://www.facebook.com");
            }
            IWebDriver driver = null;
            try
            {
                string dir = ConfigData.GetBrowserDataDirectory();
                if (!string.IsNullOrEmpty(browser_key))
                {
                    options.AddArguments(@"--user-data-dir=" + dir + "\\" + browser_key);
                }
                if (!string.IsNullOrEmpty(str_proxy))
                {
                    Proxy proxy = new Proxy();
                    proxy.Kind = ProxyKind.Manual;

                    proxy.SslProxy = str_proxy;
                    proxy.HttpProxy = str_proxy;
                    proxy.FtpProxy = str_proxy;
                    options.Proxy = proxy;
                }

                driver = new EdgeDriver(edge_driver_service, options);
            }
            catch (WebDriverException) { }
            if (driver != null)
            {
                Thread.Sleep(1000);
                try
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("return window.stop");
                }
                catch (Exception) { }
            }

            return driver;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public IWebDriver GetChromeDriver(string browser_key = "", int num_request = 3, int num_open = 6, string str_proxy = "", bool useImage = true, bool isShowBrowser = true, string userAgent = "")
        {
            ChromeOptions options = new ChromeOptions();

            var cacheViewModel = DIConfig.Get<ICacheViewModel>();
            int width = cacheViewModel.GetCacheDao().Get("screen:width").Total;
            int height = cacheViewModel.GetCacheDao().Get("screen:height").Total;
            string deviceType = cacheViewModel.GetCacheDao().Get("config:deviceType").Value;

            if (width <= 0)
            {
                width = Int32.Parse(System.Windows.SystemParameters.VirtualScreenWidth.ToString());
            }
            if (height <= 0)
            {
                height = Int32.Parse(System.Windows.SystemParameters.VirtualScreenHeight.ToString());
            }
            double h, w, x = 0, y = 0;

            num_request--;
            if (num_open <= 3)
            {
                if (num_open == 1)
                {
                    w = width;
                    h = height;
                }
                else if (num_open == 2)
                {
                    w = width / 2;
                    h = height;
                    x = w * num_request;
                }
                else
                {
                    w = width / 3;
                    h = height;
                    x = w * num_request;
                }
            }
            else
            {
                h = height / 2;
                w = width / 3;
                if (num_request >= 3)
                {
                    y = h;
                    num_request = num_request % 3;
                }
                x = w * num_request;
            }
            options.AddArguments(@"--window-size=" + w + "," + h);
            options.AddArgument(String.Format(@"--window-position={0},{1}", x, y));

            //options.AddArguments(@"--disable-dev-shm-usage");
            //options.AddArguments(@"--lang=en-US");
            //options.AddArguments(@"--disable-notifications");
            //options.AddArguments(@"--disable-popup-blocking");
            ////options.AddAdditionalCapability("useAutomationExtension", false);
            options.AddExcludedArgument(@"--enable-automation");


            options.AddArguments("--disable-notifications", "start-maximized", "--no-sandbox", "--disable-gpu", "--disable-dev-shm-usage", "--disable-web-security", "--disable-rtc-smoothness-algorithm", "--disable-webrtc-hw-decoding", "--disable-webrtc-hw-encoding", "--disable-webrtc-multiple-routes", "--disable-webrtc-hw-vp8-encoding", "--enforce-webrtc-ip-permission-check", "--force-webrtc-ip-handling-policy", "--ignore-certificate-errors", "--disable-infobars", "--disable-popup-blocking");
            options.AddUserProfilePreference("profile.default_content_setting_values.notifications", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.plugins", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.popups", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.geolocation", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.auto_select_certificate", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.mixed_script", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.media_stream", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.media_stream_mic", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.media_stream_camera", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.protocol_handlers", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.midi_sysex", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.push_messaging", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.ssl_cert_decisions", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.metro_switch_to_desktop", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.protected_media_identifier", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.site_engagement", 1);
            options.AddUserProfilePreference("profile.default_content_setting_values.durable_storage", 1);
            options.AddUserProfilePreference("useAutomationExtension", true);

            //Hide scrollbars for special pages
            //options.AddArguments(@"--hide-scrollbars");

            //Each tag uses a separate process
            //options.AddArguments(@"--process-per-tab");
            //Each site uses a separate process
            //options.AddArguments(@"--process-per-site");

            //Cancel Sandbox Mode
            //options.AddArguments(@"--no-sandbox");
            //Specify the Cache size in Byte
            //options.AddArguments(@"--disk-cache-size=0");
            ////Google Documents mentions the need to add this attribute to avoid bug s
            //options.AddArguments(@"--disable-gpu");
            ////--disable-plugins
            //options.AddArguments(@"--disable-plugins");

            //options.AddUserProfilePreference("profile.cookie_controls_mode", 0);

            //options.AddArguments(@"--disable-session-crashed-bubble");
            //options.AddArguments(@"--disable-application-cache");
            //options.AddArguments(@"--ignore-certificate-errors");

            if (useImage)
            {
                //options.AddUserProfilePreference("profile.default_content_setting_values.images", 0);
                options.AddArguments(@"--blink-settings=imagesEnabled=true");
            }
            else
            {
                //options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);

                //Disable images
                options.AddArguments(@"--disable-images");
                //Increase speed without loading pictures
                options.AddArguments(@"--blink-settings=imagesEnabled=false");
            }
            if (userAgent != "")
            {
                options.AddArguments(@"--user-agent=" + userAgent);
            }
            if (!isShowBrowser)
            {
                options.AddArguments(@"--headless");
            }
            if (!string.IsNullOrEmpty(browser_key))
            {
                string dir = ConfigData.GetBrowserDataDirectory();

                options.AddArguments(@"--user-data-dir=" + dir + "\\" + browser_key);
            }
            if (deviceType.Contains("app"))
            {
                options.AddArguments(@"--app=https://www.facebook.com");
            }
            IWebDriver driver = null;
            if (!string.IsNullOrEmpty(str_proxy))
            {
                string[] proxyArr = str_proxy.Split(':');
                if (proxyArr.Length <= 2)
                {
                    Proxy proxy = new Proxy();
                    proxy.Kind = ProxyKind.Manual;

                    proxy.SslProxy = str_proxy;
                    proxy.HttpProxy = str_proxy;
                    proxy.FtpProxy = str_proxy;
                    options.Proxy = proxy;
                }
                else
                {
                    try
                    {
                        options.AddHttpProxy(proxyArr[0], Int32.Parse(proxyArr[1]), proxyArr[2], proxyArr[3]);
                    }
                    catch (Exception) { }
                }
            } else
            {

                options.AddArguments(@"--disable-extensions");
            }
            try
            {
                driver_service = ChromeDriverService.CreateDefaultService(@"" + ConfigData.GetPathDriver());
                driver_service.HideCommandPromptWindow = true;
                //driver_service.SuppressInitialDiagnosticInformation = true;

                driver = new ChromeDriver(driver_service, options);

                Thread.Sleep(1000);
            }
            catch (Exception) { }
            if (driver != null)
            {
                try
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("return window.stop");
                }
                catch (Exception) { }
            }

            return driver;
        }


    }
}
