using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using OtpNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Management;
using System.Windows.Markup;
using ToolKHBrowser;
using ToolKHBrowser.Helper;
using ToolKHBrowser.ToolLib.Data;
using ToolKHBrowser.ViewModels;
using ToolKHBrowser.ViewModels;
using ToolLib.Tool;
using Cookie = OpenQA.Selenium.Cookie;

namespace ToolKHBrowser.ToolLib.Tool
{
    public static class FBTool
    {
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool CheckLiveUID(string uid)
        {
            try
            {
                string text = new WebClient().DownloadString("https://graph.facebook.com/" + uid + "/picture?redirect=false");
                if (!string.IsNullOrEmpty(text))
                {
                    if (text.Contains("height") && text.Contains("width"))
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception) { }

            return false;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        
        public static string LoggedIn(IWebDriver driver, FbAccount data, bool isLoginByCookie = false)
        {
            string runType = ConfigData.GetRunType();
            string result = "";
            Thread.Sleep(1500);

            try
            {
                new Actions(driver).SendKeys(Keys.Escape).Perform(); // ✅ FIX
            }
            catch { }

            switch (runType)
            {
                case "mbasic":
                    result = MBasicTool.LoggedIn(driver, data, isLoginByCookie);
                    break;
                case "mobile":
                    result = MobileFBTool.LoggedIn(driver, data, isLoginByCookie);
                    break;
                default:
                    result = WebFBTool.LoggedIn(driver, data, isLoginByCookie);
                    break;
            }

            return result;
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        
        public static int Login(IWebDriver driver, FbAccount data, bool isLoginByCookie = false)
        {
            string runType = ConfigData.GetRunType();
            int result = 0;
            Thread.Sleep(1500);

            try
            {
                new Actions(driver).SendKeys(Keys.Escape).Perform(); // ✅ FIX
            }
            catch { }

            switch (runType)
            {
                case "mbasic":
                    result = MBasicTool.Login(driver, data, isLoginByCookie);
                    break;
                case "mobile":
                    result = MobileFBTool.Login(driver, data, isLoginByCookie);
                    break;
                default:
                    result = WebFBTool.Login(driver, data, isLoginByCookie);
                    break;
            }

            return result;
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]

        //public static string GetResults(IWebDriver driver, int counter = 10)
        //{
        //    try { ((IJavaScriptExecutor)driver).ExecuteScript("return window.stop"); } catch { }

        //    bool isThread = counter > 0;
        //    string result = "";
        //    bool isWorking = false;
        //    string url = "";
        //    string source = "";

        //    do
        //    {
        //        if (isThread) Thread.Sleep(1000);

        //        try { source = (driver.PageSource ?? "").ToLower().Trim(); } catch { source = ""; }
        //        try { url = (driver.Url ?? ""); } catch { url = ""; }

        //        // ✅ CHECKPOINT (includes OTP / approvals / locked / disabled)
        //        if (url.Contains("/checkpoint/"))
        //        {
        //            isWorking = true;

        //            if (url.Contains("/checkpoint/?next"))
        //            {
        //                result = "Approvals";
        //            }
        //            else if (url.Contains("956"))
        //            {
        //                result = "Lock 956";
        //                if (source.Contains("code by email")) result += ": Need confirm code by email";
        //                else if (source.Contains("your phone")) result += ": Need confirm code by phone";
        //                else if (source.Contains("confirm your identity")) result += ": Need confirm identity";
        //            }
        //            else if (url.Contains("5049/"))
        //            {
        //                IWebElement elDismiss = null;
        //                try { elDismiss = driver.FindElement(By.XPath("//div[@aria-label='Dismiss']")); } catch { }
        //                if (elDismiss == null)
        //                {
        //                    try
        //                    {
        //                        elDismiss = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div[2]/div/div/div[1]/div/div/div/div/div/div/div/div/div/div/div[3]/div"));
        //                    }
        //                    catch { }
        //                }

        //                if (elDismiss != null)
        //                {
        //                    WebFBTool.ClickElement(driver, elDismiss);
        //                    Thread.Sleep(2000);
        //                    result = "success";
        //                }
        //                else
        //                {
        //                    // still checkpoint
        //                    result = "Checkpoint";
        //                }
        //            }
        //            else if (url.Contains("/checkpoint/8282"))
        //            {
        //                result = "Lock 2882";
        //            }
        //            else if (url.Contains("/checkpoint/disabled"))
        //            {
        //                result = "Disabled";
        //            }
        //            else
        //            {
        //                // ✅ VERY IMPORTANT: checkpoint is not auto-fail
        //                // It can be OTP / approvals, so we return a “Need …”
        //                result = "Need 2FA";
        //            }
        //        }
        //        // ✅ LOGIN page
        //        else if (url.Contains("/login/"))
        //        {
        //            isWorking = true;
        //            result = "Wrong password";
        //            if (source.Contains("old password")) result += ": Old password";
        //            else if (source.Contains("incorrect")) result += ": Incorrect username/password";
        //        }
        //        // ✅ 2FA page
        //        else if (url.Contains("two_step_verification") || url.Contains("two_factor"))
        //        {
        //            isWorking = true;
        //            result = "Need 2FA";
        //            if (source.Contains("enter the characters") || source.Contains("captcha"))
        //            {
        //                result = "Need enter captcha";
        //            }
        //        }
        //        // ✅ safe device / accountscenter
        //        else if (url.Contains("accountscenter") || url.Contains("login/save-device/?login_source=login"))
        //        {
        //            isWorking = true;
        //            result = "success";
        //        }
        //        else if (url.Contains("show_webview_confirm_dialog"))
        //        {
        //            isWorking = true;
        //            result = "success";
        //            StartReviewData(driver);
        //        }
        //        else if (url.Contains("confirmemail") || url.Contains("confirmation") || url.Contains("confirm")
        //              || url.Contains("account_creation") || url.Contains("/login/save-device") || url.Contains("confirmemail.php"))
        //        {
        //            isWorking = true;
        //            result = "Confirm";
        //        }
        //        else
        //        {
        //            if (!isThread)
        //            {
        //                result = "success";
        //                isWorking = true;
        //            }
        //            else if (!string.IsNullOrEmpty(GetUserId(driver)))
        //            {
        //                result = "success";
        //                isWorking = true;
        //            }
        //        }

        //    } while (!isWorking && counter-- > 0);

        //    // keep your spam check block as-is (unchanged)
        //    if (result.Contains("success"))
        //    {
        //        counter = 10;
        //        isWorking = false;

        //        do
        //        {
        //            result = "success";
        //            Thread.Sleep(1000);

        //            try { source = driver.FindElement(By.TagName("body")).Text.Trim().ToLower(); } catch { source = ""; }

        //            if (!string.IsNullOrEmpty(source))
        //            {
        //                if (source.Contains("what happened") &&
        //                    (source.Contains("use groups") || source.Contains("spam") || source.Contains("we removed")))
        //                {
        //                    if (source.Contains("use groups"))
        //                    {
        //                        result = "Spam: Can not use groups";
        //                        isWorking = true;
        //                    }
        //                    else if (source.Contains("we removed"))
        //                    {
        //                        result = "Spam: Removed content/comment";
        //                    }
        //                    else
        //                    {
        //                        result = "Need remove spam";
        //                    }

        //                    Close(driver);
        //                    Thread.Sleep(1000);
        //                }
        //                else
        //                {
        //                    result = "success";
        //                    isWorking = true;
        //                }
        //            }
        //        } while (!isWorking && counter-- > 0);
        //    }

        //    return result;
        //}

        public static string GetResults(IWebDriver driver, int counter = 10)
        {
            try { ((IJavaScriptExecutor)driver).ExecuteScript("return window.stop"); } catch { }

            bool isThread = counter > 0;
            string result = "";
            bool isWorking = false;
            string url = "";
            string source = "";

            do
            {
                if (isThread) Thread.Sleep(1000);

                try { source = (driver.PageSource ?? "").ToLower().Trim(); } catch { source = ""; }
                try { url = (driver.Url ?? "").ToLower(); } catch { url = ""; }

                // ✅ FORCE SUCCESS if c_user cookie exists
                if (!string.IsNullOrEmpty(GetUserId(driver)))
                {
                    result = "success";
                    isWorking = true;
                }

                // ✅ REMEMBER BROWSER (post 2FA screen)
                else if (url.Contains("remember_browser") || url.Contains("remember-browser"))
                {
                    try { ClickAlwaysConfirmRememberBrowser(driver, 5); } catch { }

                    // Even if not clicked, treat as success
                    result = "success";
                    isWorking = true;
                }

                // ✅ CHECKPOINT
                else if (url.Contains("/checkpoint/"))
                {
                    isWorking = true;

                    if (url.Contains("/checkpoint/?next"))
                        result = "Approvals";

                    else if (url.Contains("/checkpoint/disabled"))
                        result = "Disabled";

                    else
                        result = "Need 2FA"; // still real checkpoint
                }

                // ✅ LOGIN PAGE
                else if (url.Contains("/login/"))
                {
                    isWorking = true;
                    result = "Wrong password";

                    if (source.Contains("incorrect"))
                        result = "Wrong password: Incorrect username/password";
                }

                // ✅ REAL 2FA INPUT PAGE
                else if ((url.Contains("two_step_verification") || url.Contains("two_factor"))
                         && !url.Contains("remember_browser"))
                {
                    isWorking = true;
                    result = "Need 2FA";

                    if (source.Contains("captcha"))
                        result = "Need enter captcha";
                }

                // ✅ SAFE DEVICE / ACCOUNTS CENTER
                else if (url.Contains("accountscenter")
                         || url.Contains("login/save-device")
                         || url.Contains("save-device"))
                {
                    result = "success";
                    isWorking = true;
                }

                // ✅ CONFIRM EMAIL
                else if (url.Contains("confirmemail")
                      || url.Contains("confirmation")
                      || url.Contains("account_creation"))
                {
                    result = "Confirm";
                    isWorking = true;
                }

                // ✅ DEFAULT SUCCESS
                else
                {
                    if (!isThread)
                    {
                        result = "success";
                        isWorking = true;
                    }
                }

            } while (!isWorking && counter-- > 0);

            return result;
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static int GetResult(IWebDriver driver, FbAccount data)
        {
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("return window.stop");
            }
            catch (Exception) { }

            int result = 0;
            int counter = 2;
            bool isWorking = false;
            string url = "";
            do
            {
                Thread.Sleep(1000);
                try
                {
                    url = driver.Url;
                }
                catch (Exception) { }
                if (url.Contains("/checkpoint/"))
                {
                    if (url.Contains("/checkpoint/?next"))
                    {
                        // aprove device
                        result = 0;
                        data.Description = "Login Approvals";
                    }
                    else if (url.Contains("/checkpoint/8282"))
                    {
                        // locked
                        result = 0;
                        data.Description = "Locked";
                    }
                    else if (url.Contains("/checkpoint/disabled"))
                    {
                        // disabled
                        result = 0;
                        data.Description = "Disabled";
                    }
                    else
                    {
                        result = 0;
                        data.Description = "Checkpoint";
                    }
                    isWorking = true;
                }
                else if (url.Contains("show_webview_confirm_dialog"))
                {
                    isWorking = true;
                    result = 1;
                    data.Description = "Required: Review your data setting";

                    StartReviewData(driver);
                }
                else if (url.Contains("confirmemail") || url.Contains("confirmation") || url.Contains("confirm") || url.Contains("account_creation") || url.Contains("/login/save-device") || url.Contains("confirmemail.php"))
                {
                    isWorking = true;
                    result = 0;
                    data.Description = "Confirm";
                }
                else if (!string.IsNullOrEmpty(GetUserId(driver)))
                {
                    result = 1;
                    isWorking = true;
                }
            } while (!isWorking && counter-- > 0);

            return result;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void StartReviewData(IWebDriver driver)
        {
            int counter = 4;
            bool isWorking = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Get Started']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Get started']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div[1]/div[2]/div/div/div/div/div/div/div/div/div/div[3]/div/div/div/div/div/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return;
            }
            counter = 6;
            isWorking = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Accept and Continue']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Accept and continue']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div[1]/div/div/div/div/div/div/div/div/div/div[3]/div/div/div/div/div[2]/div[1]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div[1]/div[2]/div/div/div/div/div/div/div/div/div/div[3]/div/div/div/div/div[2]/div[1]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            counter = 4;
            isWorking = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Close']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='close']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div[1]/div[2]/div/div/div/div/div/div/div/div/div/div[3]/div/div/div/div/div/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (isWorking)
            {
                Thread.Sleep(1000);
            }
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        //public static void UseData(IWebDriver driver, string dataMode="")
        //{
        //    if (dataMode == "data" || dataMode == "free")
        //    {
        //        string url = "";
        //        try
        //        {
        //            url = driver.Url;
        //        }
        //        catch (Exception) { }
        //        if (!url.Contains("/zero/"))
        //        {
        //            if (dataMode == "data")
        //            {
        //                try
        //                {
        //                    //driver.Navigate().GoToUrl("https://mobile.facebook.com/mobile/zero/carrier_page/settings_page/");
        //                    driver.Navigate().GoToUrl("https://m.facebook.com/zero/policy/optin?_rdc=1&_rdr");
        //                }
        //                catch (Exception) { }
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    driver.Navigate().GoToUrl("https://free.facebook.com/zero/toggle/welcome/?source=bookmark&_rdc=1&_rdr");
        //                }
        //                catch (Exception) { }
        //            }
        //        }
        //        FBTool.WaitingPageLoading(driver);
        //        Thread.Sleep(1000);
        //        if (dataMode=="data")
        //        {
        //            try
        //            {
        //                url = driver.Url;
        //            }
        //            catch (Exception) { }
        //            if (url.Contains("free.facebook.com"))
        //            {
        //                try
        //                {
        //                    driver.Navigate().GoToUrl("https://mobile.facebook.com/mobile/zero/carrier_page/settings_page/");
        //                }
        //                catch (Exception) { }
        //                FBTool.WaitingPageLoading(driver);
        //                Thread.Sleep(1000);
        //                UseData1(driver);
        //            }
        //            else
        //            {
        //                UseData(driver);
        //            }
        //        }
        //        else
        //        {
        //            FreeMode(driver);
        //        }
        //    }
        //}
        public static void UseData(IWebDriver driver, string dataMode = "")
        {
            if (dataMode != "data" && dataMode != "free")
                return;

            // ✅ FIX: Do NOT interrupt 2FA / checkpoint / login
            string url = "";
            try { url = (driver.Url ?? "").ToLower(); } catch { url = ""; }

            if (url.Contains("/checkpoint/")
                || url.Contains("two_step_verification")
                || url.Contains("two_factor")
                || url.Contains("/login/"))
            {
                return;
            }

            if (!url.Contains("/zero/"))
            {
                if (dataMode == "data")
                {
                    try { driver.Navigate().GoToUrl("https://m.facebook.com/zero/policy/optin?_rdc=1&_rdr"); } catch { }
                }
                else
                {
                    try { driver.Navigate().GoToUrl("https://free.facebook.com/zero/toggle/welcome/?source=bookmark&_rdc=1&_rdr"); } catch { }
                }
            }

            WaitingPageLoading(driver);
            Thread.Sleep(1000);

            if (dataMode == "data")
            {
                try { url = (driver.Url ?? "").ToLower(); } catch { url = ""; }

                if (url.Contains("free.facebook.com"))
                {
                    try { driver.Navigate().GoToUrl("https://mobile.facebook.com/mobile/zero/carrier_page/settings_page/"); } catch { }
                    WaitingPageLoading(driver);
                    Thread.Sleep(1000);
                    UseData1(driver);
                }
                else
                {
                    // ✅ FIX: your old code called the wrong overload UseData(driver)
                    // This version calls the correct flow to click "OK, Use Data"
                    UseData(driver);
                }
            }
            else
            {
                FreeMode(driver);
            }
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool UseData1(IWebDriver driver)
        {
            bool isSuccess = false;
            bool isWorking = false;
            try
            {
                driver.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div/div[1]/div/article/form/div/a")).Click();
                isWorking = true;
            }
            catch (Exception) { }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div[1]/div/div[2]/div[1]/a")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("//span[contains(text(),'No Thanks')]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            // ok, use data
            if (isWorking)
            {
                Thread.Sleep(2500);
            }
            isWorking = false;
            try
            {
                driver.FindElement(By.XPath("//span[contains(text(),'OK, Use Data')]")).Click();
                isWorking = true;
            }
            catch (Exception) { }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div/div/div/article/form/button")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div/div/div/div/table/tbody/tr/td/div[1]/article/div[2]/table/tbody/tr/td/a[1]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div/div/div/div/table/tbody/tr/td/div[1]/div[3]/div[2]/table/tbody/tr/td/a[1]/span")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("//span[contains(text(),'Use Data')]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.ClassName("v")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            isSuccess = isWorking;
            if (isWorking)
            {
                Thread.Sleep(1000);
                isWorking = false;
                try
                {
                    driver.FindElement(By.XPath("/html/body/div/div/div/div/table/tbody/tr/td/div[1]/div[2]/div[2]/table/tbody/tr/td/form[1]/input[4]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div/div/div[2]/div/table/tbody/tr/td/div/div[3]/div[1]/a")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//input[@value='Turn off this message']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//input[@type='submit']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[2]/div/a")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            }

            return isSuccess;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void UseData(IWebDriver driver)
        {
            bool isWorking = false;
            try
            {
                driver.FindElement(By.XPath("//span[contains(text(),'No Thanks')]")).Click();
                isWorking = true;
            }
            catch (Exception) { }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div/div/div/article/form/div/a")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div/div[1]/div/article/form/div/a")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            // ok, use data
            if (!isWorking)
            {
                return;
            }
            Thread.Sleep(3500);

            isWorking = false;
            try
            {
                driver.FindElement(By.XPath("//span[contains(text(),' OK, Use Data ')]")).Click();
                isWorking = true;
            }
            catch (Exception) { }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div/div/div/article/form/button")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if(isWorking)
            {
                WaitingPageLoading(driver);
                Thread.Sleep(1000);
            }
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void FreeMode(IWebDriver driver)
        {
            bool isWorking = false;
            int counter = 6;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//input[@value='Continue']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//input[@type='submit']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div/div/div/div/table/tbody/tr/td/article/form/input[3]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div/div/div/div/table/tbody/tr/td/div[2]/form/input[3]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div/div/div/div/table/tbody/tr/td/div[2]/form/input[3]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        //public static void QuitBrowser(IWebDriver driver, string browserKey = "", string uid= "")
        //{
        //    try
        //    {
        //        ((IJavaScriptExecutor)driver).ExecuteScript("return window.stop");
        //    }
        //    catch (Exception) { }
        //    WaitingPageLoading(driver);
        //    Thread.Sleep(500);
        //    try
        //    {
        //        driver.Quit();
        //    }
        //    catch (Exception) { }
        //    string cache = DIConfig.Get<ICacheViewModel>().GetCacheDao().Get("config:no_clear_cache").Value.ToString();
        //    if (cache != "True")
        //    {
        //        if (!string.IsNullOrEmpty(browserKey))
        //        {
        //            LocalData.ClearCache(browserKey);
        //        }
        //    }
        //    if(!string.IsNullOrEmpty(uid) && (browserKey.Contains("@") || browserKey.StartsWith("+") || browserKey.StartsWith("0")))
        //    {
        //        Thread.Sleep(1000);
        //        //string newBrowserKey= ConfigData.GetBrowserKey(uid);
        //        //string sou = ConfigData.GetBrowserDataDirectory() + "\\" + browserKey;
        //        //try
        //        //{
        //        //    string des = ConfigData.GetBrowserDataDirectory() + "\\" + newBrowserKey;

        //        //    Directory.Move(sou, des);
        //        //    Thread.Sleep(1000);
        //        //    Directory.Delete(sou, true);
        //        //}
        //        //catch (Exception) { }
        //        CloneDataProfile(browserKey, uid);
        //    }
        //}
        public static void QuitBrowser(IWebDriver driver, string browserKey = "", string uid = "", bool forceQuit = false)
        {
            // ✅ If not forcing, don't close when user must enter OTP / checkpoint
            if (!forceQuit)
            {
                string url = "";
                try { url = (driver.Url ?? "").ToLower(); } catch { url = ""; }

                bool needHuman =
                    url.Contains("/checkpoint/") ||
                    url.Contains("two_step_verification") ||
                    url.Contains("two_factor") ||
                    url.Contains("auth") ||
                    url.Contains("security");

                if (needHuman)
                {
                    // keep browser open for user to type OTP
                    return;
                }
            }

            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("return window.stop");
            }
            catch { }

            WaitingPageLoading(driver);
            Thread.Sleep(500);

            try { driver.Quit(); } catch { }

            // keep your cache / clone logic
            var cacheObj = DIConfig.Get<ICacheViewModel>().GetCacheDao().Get("config:no_clear_cache");
            string cache = cacheObj?.Value?.ToString() ?? "";
            if (cache != "True")
            {
                if (!string.IsNullOrEmpty(browserKey))
                {
                    LocalData.ClearCache(browserKey);
                }
            }

            if (!string.IsNullOrEmpty(uid) && (browserKey.Contains("@") || browserKey.StartsWith("+") || browserKey.StartsWith("0")))
            {
                Thread.Sleep(1000);
                CloneDataProfile(browserKey, uid);
            }
        }

        public static void CloneDataProfile(string browserKey, string uid)
        {
            string newBrowserKey = ConfigData.GetBrowserKey(uid);
            if (browserKey != newBrowserKey)
            {
                string sou = ConfigData.GetBrowserDataDirectory() + "\\" + browserKey;
                try
                {
                    string des = ConfigData.GetBrowserDataDirectory() + "\\" + newBrowserKey;

                    Directory.Move(sou, des);
                    Thread.Sleep(1000);
                    Directory.Delete(sou, true);
                }
                catch (Exception) { }
            }
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void DeleteProfile(string uid)
        {
            string browserKey = ConfigData.GetBrowserKey(uid);
            string sou = ConfigData.GetBrowserDataDirectory() + "\\" + browserKey;
            try
            {
                Directory.Delete(sou, true);
            }
            catch (Exception) { }
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool IsEnglish(IWebDriver driver)
        {
            bool isEnglish = false;
            try
            {
                driver.FindElement(By.XPath("//*[text() = 'Like']"));
                isEnglish = true;
            }
            catch (Exception) { }
            if (!isEnglish)
            {
                try
                {
                    driver.FindElement(By.XPath("//*[text() = 'Add a Mobile Number']"));
                    isEnglish = true;
                }
                catch (Exception) { }
            }
            if (!isEnglish)
            {
                try
                {
                    driver.FindElement(By.XPath("//*[text() = 'Find Friends']"));
                    isEnglish = true;
                }
                catch (Exception) { }
            }
            if (!isEnglish)
            {
                try
                {
                    driver.FindElement(By.XPath("//*[text() = 'Friends']"));
                    isEnglish = true;
                }
                catch (Exception) { }
            }
            if (!isEnglish)
            {
                try
                {
                    driver.FindElement(By.XPath("//*[text() = 'Home']"));
                    isEnglish = true;
                }
                catch (Exception) { }
            }
            if (!isEnglish)
            {
                try
                {
                    driver.FindElement(By.XPath("//*[text() = 'Groups']"));
                    isEnglish = true;
                }
                catch (Exception) { }
            }
            if (!isEnglish)
            {
                try
                {
                    driver.FindElement(By.XPath("//*[text() = 'Live Video']"));
                    isEnglish = true;
                }
                catch (Exception) { }
            }
            if (!isEnglish)
            {
                try
                {
                    driver.FindElement(By.XPath("//*[text() = 'Create Story']"));
                    isEnglish = true;
                }
                catch (Exception) { }
            }
            if (!isEnglish)
            {
                try
                {
                    var soure = driver.PageSource.ToLower();
                    if(soure.Contains("trust this device?"))
                    {
                        isEnglish = true;
                    }
                }
                catch (Exception) { }
            }

            return isEnglish;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void GoToFacebook(IWebDriver driver, string url)
        {
            if (driver == null) return;

            try { driver.Navigate().GoToUrl(url); } catch { }
            try { WaitingPageLoading(driver); } catch { }
            Thread.Sleep(1000);
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static string GetCookie(IWebDriver driver)
        {
            string sCookie = "";
            try
            {
                var cookies = driver.Manage().Cookies.AllCookies;
                foreach (Cookie c in cookies)
                {
                    try
                    {
                        sCookie += c.Name + "=" + c.Value + ";";
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception) { }

            return sCookie;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool IsLogin(IWebDriver driver)
        {
            if (string.IsNullOrEmpty(FBTool.GetUserId(driver)))
            {
                return false;
            } else
            {
                try
                {
                    var source = driver.PageSource.ToString().ToLower();
                    if(source.Contains("log in") || source.Contains("forgot account?"))
                    {
                        return false;
                    }
                } catch(Exception) { }
            }

            return true;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static string GetUserId(IWebDriver driver)
        {
            string userId = "";
            try
            {
                Cookie c = driver.Manage().Cookies.GetCookieNamed("c_user");
                if (c != null)
                {
                    userId = c.Value;
                }
            }
            catch (Exception) { }
            if(!string.IsNullOrEmpty(userId))
            {
                try
                {
                    driver.FindElement(By.Name("email"));
                    userId = "";
                } catch(Exception) { }
            }

            return userId;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void WaitingPageLoading(IWebDriver driver)
        {
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete");
            }
            catch (Exception) { }
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static int GetResult(IWebDriver driver)
        {
            int result = 0;
            int counter = 5;
            bool isWorking = false;
            string url = "";
            do
            {
                Thread.Sleep(1000);
                try
                {
                    url = driver.Url;
                }
                catch (Exception) { }
                if (url.Contains("/checkpoint/"))
                {
                    result = 3;
                    isWorking = true;
                }
                else if (url.Contains("confirmemail") || url.Contains("confirmation"))
                {
                    result = 2;
                    isWorking = true;
                }
                else if (!string.IsNullOrEmpty(GetUserId(driver)))
                {
                    result = 1;
                    isWorking = true;
                }
            } while (!isWorking && counter-- > 0);

            return result;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void LoginFbByCookie(IWebDriver driver, string _cookies)
        {
            try
            {
                string script = "function setCookie(t) { \r\n var list = t.split(';'); console.log(list); \r\n for (var i = list.length - 1; i >= 0; i--) { \r\n var cname = list[i].split('=')[0]; \r\n  var cvalue = list[i].split('=')[1]; \r\n  var d = new Date(); \r\n  d.setTime(d.getTime() + (7*24*60*60*1000)); \r\n  var expires = ';domain=.facebook.com;expires='+ d.toUTCString(); document.cookie = cname + '=' + cvalue + '; ' + expires; \r\n }\r\n } \r\n  function hex2a(hex) { \r\n  var str = ''; \r\n for (var i = 0; i < hex.length; i += 2) { \r\n var v = parseInt(hex.substr(i, 2), 16);\r\n if (v) str += String.fromCharCode(v); \r\n                                            }\r\n                                        return str; } \r\n                                        var cookie = '" + _cookies + "';\r\n                                        setCookie(cookie);\r\n                                        document.location='https://mbasic.facebook.com/'";
                _ = (string)((IJavaScriptExecutor)driver).ExecuteScript(script, new object[0]);
                Thread.Sleep(1000);
                driver.Navigate().Refresh();
            }
            catch (Exception) { }
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void LoginByCookie(IWebDriver driver, string cookie)
        {
            //LoginFbByCookie(driver, cookie);

            //return;
            if (string.IsNullOrEmpty(cookie))
            {
                return;
            }
            string[] arr = cookie.Split(';');
            string[] a;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] != "")
                {
                    a = arr[i].Split('=');
                    if (a.Length > 0)
                    {
                        try
                        {
                            driver.Manage().Cookies.AddCookie(new Cookie(a[0].Trim(), a[1].Trim()));
                        }
                        catch (Exception) { }
                    }
                }
            }
            try
            {
                driver.Navigate().Refresh();
            }
            catch (Exception) { }
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void ScrollUp(IWebDriver driver, int ms = 1000)
        {
            Actions a_provider = new Actions(driver);
            IAction keyup = null;
            do
            {
                try
                {
                    keyup = a_provider.SendKeys(OpenQA.Selenium.Keys.ArrowUp).Build();
                    keyup.Perform();
                }
                catch (Exception) { }
                ms = ms - 500;
                Thread.Sleep(500);
            } while (ms > 0);
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void ScrollDown(IWebDriver driver)
        {
            int counter = 100;
            do
            {
                try
                {
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    js.ExecuteScript("window.scrollBy(0, 1)", "");
                }
                catch (Exception) { }
            } while (counter-- > 0);
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void Scroll(IWebDriver driver, int ms = 1000, bool is_like = true)
        {
            Actions a_provider = new Actions(driver);
            IAction keydown = null;
            if (is_like)
            {
                try
                {
                    driver.FindElement(By.XPath("//a[text() = 'Like']")).Click();
                }
                catch (Exception) { }
                try
                {
                    driver.FindElement(By.XPath("//span[text() = 'Like']")).Click();
                }
                catch (Exception) { }
            }
            do
            {
                try
                {
                    keydown = a_provider.SendKeys(OpenQA.Selenium.Keys.ArrowDown).Build();
                    keydown.Perform();
                }
                catch (Exception) { }
                ms = ms - 500;
                Thread.Sleep(500);
            } while (ms > 0);
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void Scroll(IWebDriver driver, IWebElement element)
        {
            Actions a_provider = new Actions(driver);
            IAction keydown = null;
            try
            {
                keydown = a_provider.MoveToElement(element);
                keydown.Perform();
            }
            catch(Exception) { }
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool IsDataMode(IWebDriver driver)
        {
            bool isWorking = false;
            int counter = 3;
            do
            {
                Thread.Sleep(1000);
                try
                {
                    string url = driver.Url;
                    if(!url.Contains("zero/policy/"))
                    {
                        isWorking = true;
                    }
                }
                catch (Exception) { }
            } while (!isWorking && counter-- > 0);

            return isWorking;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void MobileUseData(IWebDriver driver, bool isReload = true)
        {
            bool isWorking = false;
            if (isReload)
            {
                string url = driver.Url;
                if (!url.Contains("/zero/"))
                {
                    try
                    {
                        driver.Navigate().GoToUrl("https://m.facebook.com/zero/policy/optin?_rdc=1&_rdr");
                    }
                    catch (Exception) { }
                }
            }
            WaitingPageLoading(driver);
            Thread.Sleep(2000);
            try
            {
                driver.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div/div[1]/div/article/form/div/a")).Click();
                isWorking = true;
            }
            catch (Exception) { }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("//span[contains(text(),'No Thanks')]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            // ok, use data
            if (!isWorking)
            {
                return;
            }
            Thread.Sleep(3000);
            isWorking = false;
            try
            {
                driver.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div/div/div/article/form/button")).Click();
                isWorking = true;
            }
            catch (Exception) { }
            try
            {
                driver.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div/div/div/article/form/button")).Click();
                isWorking = true;
            }
            catch (Exception) { }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("//span[contains(text(),'OK, Use Data')]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div/div/div/div/table/tbody/tr/td/div[1]/article/div[2]/table/tbody/tr/td/a[1]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div/div/div/div/table/tbody/tr/td/div[1]/div[3]/div[2]/table/tbody/tr/td/a[1]/span")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("//span[contains(text(),'Use Data')]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.ClassName("v")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (isWorking)
            {
                Thread.Sleep(1000);
                isWorking = false;
                try
                {
                    driver.FindElement(By.XPath("/html/body/div/div/div/div/table/tbody/tr/td/div[1]/div[2]/div[2]/table/tbody/tr/td/form[1]/input[4]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//input[@value='Turn off this message']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//input[@type='submit']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[2]/div/a")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            }
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool Close(IWebDriver driver)
        {
            bool isWorking = false;
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[7]/div[1]/div/div[2]/div/div/div/div[2]/div")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[6]/div[1]/div/div[2]/div/div/div/div[2]/div")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[5]/div[1]/div/div[2]/div/div/div/div[2]/div")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            //if (!isWorking)
            //{
            //    try
            //    {
            //        driver.FindElement(By.XPath("//*[@aria-label='Close']")).Click();
            //        isWorking = true;
            //    }
            //    catch (Exception) { }
            //}
            //if (!isWorking)
            //{
            //    try
            //    {
            //        driver.FindElement(By.XPath("//div[@aria-label='Close']")).Click();
            //        isWorking = true;
            //    }
            //    catch (Exception) { }
            //}
            if(!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[2]/form/div/div[1]/div/div/div[1]/div/div[1]/div[1]/div[2]/div")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }

            return isWorking;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static string GetPageUserId(IWebDriver driver)
        {
            string userId = "";
            try
            {
                Cookie c = driver.Manage().Cookies.GetCookieNamed("i_user");
                if (c != null)
                {
                    userId = c.Value;
                }
            }
            catch (Exception) { }

            return userId;
        }
        public static bool WaitForLoginSuccess(IWebDriver driver, int timeoutSeconds = 180)
        {
            var end = DateTime.Now.AddSeconds(timeoutSeconds);

            while (DateTime.Now < end)
            {
                if (!string.IsNullOrEmpty(GetUserId(driver)))
                    return true;

                Thread.Sleep(1000);
            }
            return false;
        }
        public static bool WaitForUserComplete2FA(IWebDriver driver, FbAccount data, int timeoutSeconds = 180)
        {
            data.Description = "Need 2FA: Please enter 6-digit code in the browser";

            var end = DateTime.Now.AddSeconds(timeoutSeconds);
            while (DateTime.Now < end)
            {
                // ✅ once c_user exists, login is done
                if (!string.IsNullOrEmpty(GetUserId(driver)))
                {
                    data.Description = "2FA completed";
                    return true;
                }

                Thread.Sleep(1000);
            }

            data.Description = "2FA timeout (not completed)";
            return false;
        }

        public static string GenerateFacebook2FA(string secret)
        {
            secret = secret.Replace(" ", "").Trim();

            var secretBytes = Base32Encoding.ToBytes(secret);
            var totp = new Totp(secretBytes);

            return totp.ComputeTotp();
        }
        //public static bool Is2FA(IWebDriver driver)
        //{
        //    try
        //    {
        //        var url = (driver.Url ?? "").ToLower();
        //        if (url.Contains("two_step_verification") || url.Contains("two_factor") || url.Contains("/checkpoint/"))
        //            return true;

        //        string html = (driver.PageSource ?? "").ToLower();
        //        if (html.Contains("approvals_code") || html.Contains("two-step verification") || html.Contains("two-factor authentication")
        //            || html.Contains("waiting for approval") || html.Contains("check your notifications") || html.Contains("try another way")
        //            || html.Contains("authentication app") || html.Contains("6-digit")
        //            || html.Contains("រង់ចាំការអនុម័ត") || html.Contains("chờ phê duyệt"))
        //            return true;

        //        return false;
        //    }
        //    catch { return false; }
        //}

        public static bool Is2FA(IWebDriver driver)
        {
            try
            {
                var url = (driver.Url ?? "").ToLowerInvariant();

                // ✅ IMPORTANT: remember_browser is NOT a 2FA input screen
                if (url.Contains("remember_browser") || url.Contains("remember-browser"))
                    return false;

                // 2FA / checkpoint screens
                if (url.Contains("two_step_verification") || url.Contains("two_factor") || url.Contains("/checkpoint/"))
                    return true;

                string html = (driver.PageSource ?? "").ToLowerInvariant();

                // If we are on remember_browser, also treat as NOT 2FA even if HTML has "trust this device"
                if (html.Contains("trust this device") || html.Contains("always confirm"))
                    return false;

                if (html.Contains("approvals_code")
                    || html.Contains("two-step verification")
                    || html.Contains("two-factor authentication")
                    || html.Contains("waiting for approval")
                    || html.Contains("check your notifications")
                    || html.Contains("try another way")
                    || html.Contains("authentication app")
                    || html.Contains("6-digit")
                    || html.Contains("រង់ចាំការអនុម័ត")
                    || html.Contains("chờ phê duyệt"))
                    return true;

                return false;
            }
            catch { return false; }
        }
        public static bool IsTwoFaPage(IWebDriver driver)
        {
            return Is2FA(driver); // Unified detection
        }

        public static string GetSafeGroupUrl(string baseUrl, string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return baseUrl;

            groupId = groupId.Trim();

            if (groupId.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                groupId.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return groupId;
            }

            if (groupId.StartsWith("groups/", StringComparison.OrdinalIgnoreCase))
                groupId = groupId.Substring(7);
            if (groupId.StartsWith("/groups/", StringComparison.OrdinalIgnoreCase))
                groupId = groupId.Substring(8);

            // Strip trailing slashes
            groupId = groupId.Trim('/');

            return baseUrl.TrimEnd('/') + "/groups/" + groupId;
        }
        /// <summary>
        /// JS-based helper: Finds any clickable element whose innerText contains any of the keywords.
        /// Returns true if found and clicked.
        /// </summary>
        private static bool JsClickByText(IWebDriver driver, string[] keywords)
        {
            var js = (IJavaScriptExecutor)driver;
            foreach (var kw in keywords)
            {
                try
                {
                    // Use querySelectorAll to find all elements and match by text
                    var script = @"
                        var kw = arguments[0].toLowerCase();
                        var allEls = document.querySelectorAll('a, button, [role=""button""], [role=""link""], div, span');
                        for (var i = 0; i < allEls.length; i++) {
                            var el = allEls[i];
                            var txt = (el.innerText || el.textContent || '').trim().toLowerCase();
                            if (txt === kw || txt.indexOf(kw) === 0) {
                                el.click();
                                return true;
                            }
                        }
                        return false;
                    ";
                    var result = js.ExecuteScript(script, kw);
                    if (result != null && result is bool && (bool)result) return true;
                }
                catch { }
            }
            return false;
        }

        public static bool HandlePushApproval_WWW(IWebDriver driver, int timeoutSec = 20)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSec));

                // Detect push approval screen
                bool isPush = wait.Until(d =>
                {
                    var html = (d.PageSource ?? "").ToLowerInvariant();
                    return html.Contains("check your notifications")
                        || html.Contains("waiting for approval")
                        || html.Contains("approve the login")
                        || html.Contains("try another way")
                        || html.Contains("រង់ចាំការអនុម័ត")
                        || html.Contains("chờ phê duyệt");
                });

                if (!isPush) return false;

                // Step 1: Click "Try another way"
                string[] tryTexts = { "try another way", "use another way", "use another method", "វិធីផ្សេង", "thử cách khác" };

                bool clickedTry = wait.Until(_ =>
                    TwoFaHelper.JsClickByTextStrong(driver, tryTexts)
                    || SeleniumClickFallback(driver, tryTexts)
                );

                if (!clickedTry) return false;

                // Step 2: Wait for methods screen OR OTP input to appear
                wait.Until(d =>
                {
                    var html = (d.PageSource ?? "").ToLowerInvariant();
                    return html.Contains("authentication app")
                        || html.Contains("code generator")
                        || html.Contains("enter the code")
                        || d.FindElements(By.XPath("//input[@type='text' or @type='tel' or @inputmode='numeric']")).Any();
                });

                // Step 3: Click "Authentication app" if it exists
                string[] authTexts = { "authentication app", "code generator", "enter code", "authenticator", "xác thực", "វិធីផ្ទៀងផ្ទាត់" };

                bool clickedAuth = TwoFaHelper.JsClickByTextStrong(driver, authTexts)
                                || SeleniumClickFallback(driver, authTexts);

                // Some flows go directly to code input without needing to click auth option
                // Step 4: Click Continue/Next if present
                string[] contTexts = { "continue", "next", "confirm", "ok", "បន្ត", "tiếp tục" };
                TwoFaHelper.JsClickByTextStrong(driver, contTexts);
                SeleniumClickFallback(driver, contTexts);

                // Final: confirm we are not stuck on push page anymore
                bool leftPush = wait.Until(d =>
                {
                    var html = (d.PageSource ?? "").ToLowerInvariant();
                    bool stillPush = html.Contains("check your notifications")
                                  || html.Contains("waiting for approval");
                    bool hasOtp = d.FindElements(By.XPath("//input[@type='text' or @type='tel' or @inputmode='numeric']")).Any();
                    bool hasAuth = html.Contains("authentication app") || html.Contains("enter the code") || html.Contains("code generator");
                    return !stillPush || hasOtp || hasAuth;
                });

                return leftPush;
            }
            catch
            {
                return false;
            }
        }
        private static bool SeleniumClickFallback(IWebDriver driver, string[] texts)
        {
            try
            {
                var els = driver.FindElements(By.XPath("//a | //button | //*[@role='button'] | //*[@role='link'] | //*[@role='menuitem'] | //label | //*[@role='radio']"));
                foreach (var el in els)
                {
                    try
                    {
                        var t = (el.Text ?? "").Trim().ToLowerInvariant();
                        if (string.IsNullOrWhiteSpace(t)) continue;

                        if (texts.Any(x => !string.IsNullOrWhiteSpace(x) && t.Contains(x.ToLowerInvariant())))
                        {
                            try { el.Click(); } catch { ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", el); }
                            return true;
                        }
                    }
                    catch { }
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// All-in-one 2FA handler. Detects push vs code-entry screen, switches if needed,
        /// finds the input, fills the TOTP code, clicks Continue.
        /// Returns: "ok" on success, or an error description string.
        /// </summary>
        public static string AutoFill2FACode(IWebDriver driver, ToolKHBrowser.ViewModels.FbAccount account)
        {
            if (driver == null) return "driver is null";
            string twoFaSecret = account?.TwoFA;
            if (string.IsNullOrWhiteSpace(twoFaSecret)) return "TwoFA secret is empty";

            // Helper to update status in the grid
            Action<string> setStatus = (msg) => {
                if (account != null) {
                    account.Description = "2FA: " + msg;
                }
            };

            try
            {
                // -- Step 1: Handle push screen if present --
                if (HandlePushApproval_WWW(driver))
                {
                    setStatus("Switched to code input.");
                }

                // -- Step 2: Find the 6-digit code input --
                IWebElement codeInput = null;
                for (int w = 0; w < 15 && codeInput == null; w++)
                {
                    codeInput = driver.FindElements(By.Name("approvals_code")).FirstOrDefault(e => e.Displayed && e.Enabled)
                             ?? driver.FindElements(By.Id("approvals_code")).FirstOrDefault(e => e.Displayed && e.Enabled)
                             ?? driver.FindElements(By.XPath("//input[@placeholder='Code' or @placeholder='code']")).FirstOrDefault(e => e.Displayed && e.Enabled)
                             ?? driver.FindElements(By.XPath("//input[contains(@aria-label,'Code')]")).FirstOrDefault(e => e.Displayed && e.Enabled);

                    if (codeInput == null)
                    {
                        // Broad scan
                        codeInput = driver.FindElements(By.TagName("input")).FirstOrDefault(inp =>
                        {
                            try
                            {
                                if (!inp.Displayed || !inp.Enabled) return false;
                                string n = (inp.GetAttribute("name") ?? "").ToLower();
                                string id = (inp.GetAttribute("id") ?? "").ToLower();
                                string t = (inp.GetAttribute("type") ?? "text").ToLower();
                                return t != "password" && t != "hidden" && t != "checkbox" && t != "radio" && t != "submit" && t != "email"
                                    && !n.Contains("email") && !id.Contains("email");
                            }
                            catch { return false; }
                        });
                    }
                    if (codeInput == null) Thread.Sleep(1000);
                }

                if (codeInput == null) return "code input not found";
                setStatus("Code input found.");

                // -- Step 3: Loop filling codes (retry up to 4x) --
                for (int attempt = 1; attempt <= 4; attempt++)
                {
                    string code = TwoFactorRequest.GetPassCode(twoFaSecret);
                    if (string.IsNullOrWhiteSpace(code)) { Thread.Sleep(1000); continue; }

                    setStatus($"Filling code {code}...");
                    try { codeInput.Click(); } catch { }
                    
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value='';", codeInput);
                    Thread.Sleep(100);
                    codeInput.SendKeys(code);
                    Thread.Sleep(200);
                    ((IJavaScriptExecutor)driver).ExecuteScript(
                        "arguments[0].dispatchEvent(new Event('input',{bubbles:true})); arguments[0].dispatchEvent(new Event('change',{bubbles:true}));", codeInput);
                    
                    Thread.Sleep(500);

                    // Click Continue
                    IWebElement btnContinue = null;
                    btnContinue = driver.FindElements(By.XPath("//button[@type='submit']")).FirstOrDefault(e => e.Displayed && e.Enabled)
                               ?? driver.FindElements(By.XPath("//input[@type='submit']")).FirstOrDefault(e => e.Displayed && e.Enabled);

                    if (btnContinue == null)
                    {
                        foreach (var el in driver.FindElements(By.XPath("//div[@role='button'] | //button | //a")))
                        {
                            try
                            {
                                string t = (el.Text ?? "").ToLower();
                                if ((t.Contains("continue") || t.Contains("confirm") || t.Contains("submit") || t.Contains("next")
                                  || t.Contains("បន្ត") || t.Contains("បញ្ជាក់") || t.Contains("tiếp tục") || t.Contains("xác nhận"))
                                  && el.Displayed && el.Enabled)
                                { btnContinue = el; break; }
                            }
                            catch { }
                        }
                    }

                    if (btnContinue != null)
                    {
                        try { btnContinue.Click(); } catch { ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btnContinue); }
                    }
                    else
                    {
                        codeInput.SendKeys(OpenQA.Selenium.Keys.Enter);
                    }

                    Thread.Sleep(4000);
                    // Check if success (input gone)
                    if (driver.FindElements(By.TagName("input")).All(inp => {
                        try { return !inp.Displayed || (inp.GetAttribute("type") ?? "").ToLower() == "hidden"; } catch { return true; }
                    }))
                    {
                        return "ok";
                    }
                    Thread.Sleep(1000);
                }

                return "failed after 4 attempts";
            }
            catch (Exception ex) { return "error: " + ex.Message; }
        }

        public static void HandlePostLoginPrompts(IWebDriver driver)
        {
            try
            {
                Thread.Sleep(2000);
                string[] texts = { "not now", "save browser", "ok", "continue", "បន្ត", "មិនមែនឥឡូវនេះទេ", "tiếp tục", "lúc khác" };
                foreach (var t in texts)
                {
                    try
                    {
                        var elements = driver.FindElements(By.XPath($"//*[contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '{t}')]"));
                        foreach (var el in elements)
                        {
                            if (el.Displayed && el.Enabled)
                            {
                                try { el.Click(); } catch { ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", el); }
                                Thread.Sleep(1000);
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        public static string FinalizeLoginFlow(IWebDriver driver, FbAccount account, int seconds = 120)
        {
            if (driver == null) return "Driver null";

            DateTime end = DateTime.UtcNow.AddSeconds(seconds);

            while (DateTime.UtcNow < end)
            {
                string url = "";
                try { url = (driver.Url ?? "").ToLower(); } catch { }

                // ✅ 0) remember_browser FIRST (because URL contains two_factor)
                if (url.Contains("remember_browser") || url.Contains("remember-browser"))
                {
                    ClickAlwaysConfirmRememberBrowser(driver, timeoutSec: 8);

                    // If cookie indicates logged-in, treat as success (prevents timeout)
                    try
                    {
                        var cUser = driver.Manage().Cookies.GetCookieNamed("c_user");
                        if (cUser != null && !string.IsNullOrWhiteSpace(cUser.Value))
                            return "success";
                    }
                    catch { }

                    Thread.Sleep(1000);
                    continue;
                }

                // ✅ 1) then check 2FA
                try
                {
                    if (Is2FA(driver))
                        return "Need 2FA";
                }
                catch { }

                // checkpoint
                if (url.Contains("/checkpoint/"))
                {
                    try { HandleCheckpointContinue(driver); } catch { }
                    Thread.Sleep(800);
                    continue;
                }

                // login page
                if (url.Contains("facebook.com/login") || url.Contains("/login/"))
                {
                    Thread.Sleep(1000);
                    continue;
                }

                // confirm login
                string uid = "";
                try { uid = GetUserId(driver); } catch { }

                bool looksLoggedIn =
                    !string.IsNullOrEmpty(uid) &&
                    !url.Contains("/checkpoint/") &&
                    !url.Contains("two_step_verification") &&
                    !url.Contains("two_factor") &&
                    !url.Contains("/login/");

                if (looksLoggedIn)
                    return "success";

                Thread.Sleep(1000);
            }

            string lastUrl = "";
            try { lastUrl = driver.Url ?? ""; } catch { }

            return "Login not finalized (timeout). LastUrl=" + lastUrl;
        }

        public static bool ClickAlwaysConfirmRememberBrowser(IWebDriver driver, int timeoutSec = 15)
        {
            if (driver == null) return false;

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSec));

            // Wait until remember_browser page is loaded
            wait.Until(d =>
            {
                var url = (d.Url ?? "").ToLowerInvariant();
                return url.Contains("remember_browser") || url.Contains("remember-browser");
            });

            // Try multiple times because FB re-renders UI
            for (int attempt = 0; attempt < 8; attempt++)
            {
                // 1) Try in current context
                if (TryClickAlwaysConfirmInCurrentContext(driver))
                    return true;

                // 2) Try inside iframes (IMPORTANT)
                driver.SwitchTo().DefaultContent();
                var frames = driver.FindElements(By.TagName("iframe"));
                foreach (var fr in frames)
                {
                    try
                    {
                        driver.SwitchTo().DefaultContent();
                        driver.SwitchTo().Frame(fr);

                        if (TryClickAlwaysConfirmInCurrentContext(driver))
                        {
                            driver.SwitchTo().DefaultContent();
                            return true;
                        }
                    }
                    catch { /* ignore */ }
                }

                driver.SwitchTo().DefaultContent();
                Thread.Sleep(700);
            }

            driver.SwitchTo().DefaultContent();
            return false;
        }

        private static bool TryClickAlwaysConfirmInCurrentContext(IWebDriver driver)
        {
            try
            {
                // Most FB buttons are div[role=button]
                var candidates = driver.FindElements(By.CssSelector("div[role='button'], button"));

                // Prefer exact match first
                foreach (var el in candidates)
                {
                    var t = (el.Text ?? "").Trim();
                    if (t.Equals("Always confirm it’s me", StringComparison.OrdinalIgnoreCase) ||
                        t.Equals("Always confirm it's me", StringComparison.OrdinalIgnoreCase) ||
                        t.StartsWith("Always confirm", StringComparison.OrdinalIgnoreCase))
                    {
                        return DispatchMouseClick(driver, el);
                    }
                }

                // Fallback: use JS scan by innerText/textContent (catches when el.Text is empty)
                bool js = (bool)((IJavaScriptExecutor)driver).ExecuteScript(@"
            const nodes = Array.from(document.querySelectorAll('div[role=""button""],button'));
            const txt = el => ((el.innerText || el.textContent || '')+'').trim().toLowerCase();
            for (const el of nodes) {
              const t = txt(el);
              if (t && t.includes('always confirm')) {
                el.scrollIntoView({block:'center', inline:'center'});
                // dispatch real events
                const r = el.getBoundingClientRect();
                const x = r.left + r.width/2, y = r.top + r.height/2;
                const opts = {bubbles:true, cancelable:true, view:window, clientX:x, clientY:y};
                el.dispatchEvent(new MouseEvent('mousemove', opts));
                el.dispatchEvent(new MouseEvent('mousedown', opts));
                el.dispatchEvent(new MouseEvent('mouseup', opts));
                el.dispatchEvent(new MouseEvent('click', opts));
                return true;
              }
            }
            return false;
        ");
                if (js) return true;

                // LAST RESORT: click the 2nd big button (your UI shows 2 buttons)
                var displayed = candidates.Where(x => x.Displayed).ToList();
                if (displayed.Count >= 2)
                    return DispatchMouseClick(driver, displayed[1]);

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static bool DispatchMouseClick(IWebDriver driver, IWebElement el)
        {
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", el);
            }
            catch { }

            // Try normal click
            try { el.Click(); return true; } catch { }

            // Strong click: dispatch mouse events on element
            try
            {
                return (bool)((IJavaScriptExecutor)driver).ExecuteScript(@"
            const el = arguments[0];
            el.scrollIntoView({block:'center', inline:'center'});
            const r = el.getBoundingClientRect();
            const x = r.left + r.width/2, y = r.top + r.height/2;
            const opts = {bubbles:true, cancelable:true, view:window, clientX:x, clientY:y};
            el.dispatchEvent(new MouseEvent('mousemove', opts));
            el.dispatchEvent(new MouseEvent('mousedown', opts));
            el.dispatchEvent(new MouseEvent('mouseup', opts));
            el.dispatchEvent(new MouseEvent('click', opts));
            return true;
        ", el);
            }
            catch { return false; }
        }
        public static void HandleCheckpointContinue(IWebDriver driver)
        {
            // Tries multiple common buttons
            for (int i = 0; i < 8; i++)
            {
                string url = "";
                try { url = (driver.Url ?? "").ToLower(); } catch { }
                if (!url.Contains("/checkpoint/")) break;

                try
                {
                    SeleniumX.ClickByText(driver, "Continue", 2);
                    SeleniumX.ClickByText(driver, "OK", 1);
                    SeleniumX.ClickByText(driver, "This was me", 1);
                    SeleniumX.ClickByText(driver, "Yes", 1);
                    SeleniumX.ClickByText(driver, "Confirm", 1);
                    SeleniumX.ClickByText(driver, "Done", 1);
                }
                catch { }

                Thread.Sleep(700);
            }
        }
    }
}
