using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
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
using ToolKHBrowser.ViewModels;
using ToolLib.Tool;
using WpfUI;
using WpfUI.ToolLib.Data;
using WpfUI.ViewModels;
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
        //public static string LoggedIn(IWebDriver driver, FbAccount data, bool isLoginByCookie = false)
        //{
        //    string runType = ConfigData.GetRunType();
        //    string result = "";
        //    Thread.Sleep(1500);
        //    try
        //    {
        //        Actions action = new Actions(driver);
        //        action.SendKeys(Keys.Escape);
        //    }
        //    catch (Exception) { }
        //    switch (runType)
        //    {
        //        case "mbasic":
        //            result = MBasicTool.LoggedIn(driver, data, isLoginByCookie);
        //            break;
        //        case "mobile":
        //            result = MobileFBTool.LoggedIn(driver, data, isLoginByCookie);
        //            break;
        //        default:
        //            result = WebFBTool.LoggedIn(driver, data, isLoginByCookie);
        //            break;
        //    }

        //    return result;
        //}
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
        //public static int Login(IWebDriver driver, FbAccount data, bool isLoginByCookie= false)
        //{
        //    string runType = ConfigData.GetRunType();
        //    int result = 0;
        //    Thread.Sleep(1500);
        //    try
        //    {
        //        Actions action = new Actions(driver);
        //        action.SendKeys(Keys.Escape);
        //    }
        //    catch (Exception) { }
        //    switch (runType)
        //    {
        //        case "mbasic":
        //            result = MBasicTool.Login(driver, data, isLoginByCookie);
        //            break;
        //        case "mobile":
        //            result = MobileFBTool.Login(driver, data, isLoginByCookie);
        //            break;
        //        default:
        //            result = WebFBTool.Login(driver, data, isLoginByCookie);
        //            break;
        //    }

        //    return result;
        //}
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
        //public static string GetResults(IWebDriver driver, int counter = 4)
        //{
        //    try
        //    {
        //        ((IJavaScriptExecutor)driver).ExecuteScript("return window.stop");
        //    }
        //    catch (Exception) { }
        //    bool isThread = true;
        //    string result = "";
        //    bool isWorking = false;
        //    string url = "";
        //    string source = "";
        //    if (counter > 0)
        //    {
        //        isThread = true;
        //    }
        //    do
        //    {
        //        if (isThread)
        //        {
        //            Thread.Sleep(1000);
        //        }
        //        try
        //        {
        //            source = driver.PageSource.ToString().ToLower().Trim();
        //        }
        //        catch (Exception) { }
        //        try
        //        {
        //            url = driver.Url;
        //        }
        //        catch (Exception) { }
        //        if (url.Contains("/checkpoint/"))
        //        {
        //            isWorking = true;
        //            if (url.Contains("/checkpoint/?next"))
        //            {
        //                // aprove device
        //                result = "Approvals";
        //            }
        //            else if (url.Contains("956"))
        //            {
        //                // locked
        //                result = "​Lock 956";
        //                if(source.Contains("code by email"))
        //                {
        //                    result += ": Need confirm code by email";
        //                } else if(source.Contains("your phone"))
        //                {
        //                    result += ": Need confirm code by phone";
        //                } else if(source.Contains("confirm your identity"))
        //                {
        //                    result += ": Need confirm identity";
        //                }
        //            }
        //            else if (url.Contains("5049/"))
        //            {
        //                // We suspect automated
        //                //result = "We suspect automated ...";
        //                IWebElement elDismiss = null;
        //                try
        //                {
        //                    elDismiss= driver.FindElement(By.XPath("//div[@aria-label='Dismiss']"));
        //                } catch(Exception) { }
        //                if(elDismiss == null)
        //                {
        //                    try
        //                    {
        //                        elDismiss = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div[2]/div/div/div[1]/div/div/div/div/div/div/div/div/div/div/div[3]/div"));
        //                    }
        //                    catch (Exception) { }
        //                }
        //                if(elDismiss != null)
        //                {
        //                    WebFBTool.ClickElement(driver, elDismiss);
        //                    Thread.Sleep(2000);
        //                    result = "success";
        //                }
        //            }
        //            else if (url.Contains("/checkpoint/8282"))
        //            {
        //                // locked
        //                result = "Lock 2882";
        //            }
        //            else if (url.Contains("/checkpoint/disabled"))
        //            {
        //                // disabled
        //                result = "Desabled";
        //            }
        //            else
        //            {
        //                result = "Checkpoint";
        //            }
        //        }
        //        else if (url.Contains("/login/"))
        //        {
        //            // wrong password
        //            result = "Wrong password";
        //            if(source.Contains("old password"))
        //            {
        //                result += ": Old password";
        //            } else if(source.Contains("incorrect"))
        //            {
        //                result += ": Incorrect username/password";
        //            }
        //        }
        //        else if (url.Contains("two_step_verification") || url.Contains("/checkpoint/"))
        //        {
        //            isWorking = true;
        //            result = "Need 2FA";   // not "Error 2FA"
        //        }

        //        else if (url.Contains("accountscenter") || url.Contains("login/save-device/?login_source=login"))
        //        {
        //            // new layout for set mail
        //            isWorking = true;
        //            result = "success";
        //        }
        //        else if (url.Contains("show_webview_confirm_dialog") || url.Contains("accountscenter"))
        //        {
        //            //Required: Review your data setting
        //            isWorking = true;
        //            result = "success";
        //            StartReviewData(driver);
        //        }
        //        else if (url.Contains("confirmemail") || url.Contains("confirmation") || url.Contains("confirm") || url.Contains("account_creation") || url.Contains("/login/save-device") || url.Contains("confirmemail.php"))
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

        //    if(result.Contains("success"))
        //    {
        //        counter = 10;
        //        isWorking = false;

        //        do
        //        {
        //            result = "success";
        //            Thread.Sleep(1000);
        //            try
        //            {
        //                source = driver.FindElement(By.TagName("body")).Text.Trim().ToLower();
        //            }
        //            catch (Exception) { }
        //            if (!string.IsNullOrEmpty(source))
        //            {
        //                if (source.Contains("what happened") && 
        //                    (source.Contains("use groups") || 
        //                    source.Contains("spam") || 
        //                    source.Contains("we removed")))
        //                {
        //                    if (source.Contains("use groups"))
        //                    {
        //                        result = "Spam: Can not use groups";
        //                        isWorking= true;
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
        //                } else
        //                {
        //                    result = "success";
        //                    isWorking = true;
        //                }
        //            }
        //        } while(!isWorking && counter-- > 0);
        //    }

        //    return result;
        //}
        public static string GetResults(IWebDriver driver, int counter = 4)
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
                try { url = (driver.Url ?? ""); } catch { url = ""; }

                // ✅ CHECKPOINT (includes OTP / approvals / locked / disabled)
                if (url.Contains("/checkpoint/"))
                {
                    isWorking = true;

                    if (url.Contains("/checkpoint/?next"))
                    {
                        result = "Approvals";
                    }
                    else if (url.Contains("956"))
                    {
                        result = "Lock 956";
                        if (source.Contains("code by email")) result += ": Need confirm code by email";
                        else if (source.Contains("your phone")) result += ": Need confirm code by phone";
                        else if (source.Contains("confirm your identity")) result += ": Need confirm identity";
                    }
                    else if (url.Contains("5049/"))
                    {
                        IWebElement elDismiss = null;
                        try { elDismiss = driver.FindElement(By.XPath("//div[@aria-label='Dismiss']")); } catch { }
                        if (elDismiss == null)
                        {
                            try
                            {
                                elDismiss = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div[2]/div/div/div[1]/div/div/div/div/div/div/div/div/div/div/div[3]/div"));
                            }
                            catch { }
                        }

                        if (elDismiss != null)
                        {
                            WebFBTool.ClickElement(driver, elDismiss);
                            Thread.Sleep(2000);
                            result = "success";
                        }
                        else
                        {
                            // still checkpoint
                            result = "Checkpoint";
                        }
                    }
                    else if (url.Contains("/checkpoint/8282"))
                    {
                        result = "Lock 2882";
                    }
                    else if (url.Contains("/checkpoint/disabled"))
                    {
                        result = "Disabled";
                    }
                    else
                    {
                        // ✅ VERY IMPORTANT: checkpoint is not auto-fail
                        // It can be OTP / approvals, so we return a “Need …”
                        result = "Need 2FA";
                    }
                }
                // ✅ LOGIN page
                else if (url.Contains("/login/"))
                {
                    isWorking = true;
                    result = "Wrong password";
                    if (source.Contains("old password")) result += ": Old password";
                    else if (source.Contains("incorrect")) result += ": Incorrect username/password";
                }
                // ✅ 2FA page
                else if (url.Contains("two_step_verification") || url.Contains("two_factor"))
                {
                    isWorking = true;
                    result = "Need 2FA";
                    if (source.Contains("enter the characters") || source.Contains("captcha"))
                    {
                        result = "Need enter captcha";
                    }
                }
                // ✅ safe device / accountscenter
                else if (url.Contains("accountscenter") || url.Contains("login/save-device/?login_source=login"))
                {
                    isWorking = true;
                    result = "success";
                }
                else if (url.Contains("show_webview_confirm_dialog"))
                {
                    isWorking = true;
                    result = "success";
                    StartReviewData(driver);
                }
                else if (url.Contains("confirmemail") || url.Contains("confirmation") || url.Contains("confirm")
                      || url.Contains("account_creation") || url.Contains("/login/save-device") || url.Contains("confirmemail.php"))
                {
                    isWorking = true;
                    result = "Confirm";
                }
                else
                {
                    if (!isThread)
                    {
                        result = "success";
                        isWorking = true;
                    }
                    else if (!string.IsNullOrEmpty(GetUserId(driver)))
                    {
                        result = "success";
                        isWorking = true;
                    }
                }

            } while (!isWorking && counter-- > 0);

            // keep your spam check block as-is (unchanged)
            if (result.Contains("success"))
            {
                counter = 10;
                isWorking = false;

                do
                {
                    result = "success";
                    Thread.Sleep(1000);

                    try { source = driver.FindElement(By.TagName("body")).Text.Trim().ToLower(); } catch { source = ""; }

                    if (!string.IsNullOrEmpty(source))
                    {
                        if (source.Contains("what happened") &&
                            (source.Contains("use groups") || source.Contains("spam") || source.Contains("we removed")))
                        {
                            if (source.Contains("use groups"))
                            {
                                result = "Spam: Can not use groups";
                                isWorking = true;
                            }
                            else if (source.Contains("we removed"))
                            {
                                result = "Spam: Removed content/comment";
                            }
                            else
                            {
                                result = "Need remove spam";
                            }

                            Close(driver);
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            result = "success";
                            isWorking = true;
                        }
                    }
                } while (!isWorking && counter-- > 0);
            }

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
        public static void GoToFacebook(IWebDriver driver,string url)
        {
            try
            {
                driver.Navigate().GoToUrl(url);
            } catch(Exception) { }
            WaitingPageLoading(driver);
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
        public static bool Is2FA(IWebDriver driver)
        {
            try
            {
                var url = (driver.Url ?? "").ToLower();
                if (url.Contains("two_step_verification") || url.Contains("two_factor"))
                    return true;

                // sometimes Facebook uses checkpoint for approvals/2FA
                if (url.Contains("/checkpoint/"))
                    return true;

                return false;
            }
            catch { return false; }
        }
        public static bool IsTwoFaPage(IWebDriver driver)
        {
            string url = "";
            try { url = (driver.Url ?? "").ToLower(); } catch { }

            if (url.Contains("two_step_verification") || url.Contains("two_factor"))
                return true;

            // Sometimes FB uses checkpoint flows for OTP
            if (url.Contains("/checkpoint/"))
            {
                try
                {
                    var src = (driver.PageSource ?? "").ToLower();
                    if (src.Contains("authentication app") || src.Contains("two-factor") || src.Contains("6-digit"))
                        return true;
                }
                catch { }
            }

            return false;
        }

        public static string GetSafeGroupUrl(string baseUrl, string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return baseUrl;

            groupId = groupId.Trim();

            // If it's already a full URL, don't prepend baseUrl/groups/
            if (groupId.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                groupId.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                // Optionally normalize base (e.g. if we are on web but got mobile link, we might want to swap)
                // For now, return as-is if it's a valid FB group link
                return groupId;
            }

            // If it's just a numeric ID or slug, construction should be: baseUrl + "/groups/" + groupId
            // But we must be careful: some places in code might already pass "/groups/ID" or "groups/ID"
            if (groupId.StartsWith("groups/", StringComparison.OrdinalIgnoreCase))
                groupId = groupId.Substring(7);
            if (groupId.StartsWith("/groups/", StringComparison.OrdinalIgnoreCase))
                groupId = groupId.Substring(8);

            // Strip trailing slashes
            groupId = groupId.Trim('/');

            return baseUrl.TrimEnd('/') + "/groups/" + groupId;
        }

    }
}
