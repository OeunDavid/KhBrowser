using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolKHBrowser.ToolLib.Data;
using ToolLib.Tool;
using ToolKHBrowser.ViewModels;

namespace ToolKHBrowser.ToolLib.Tool
{
    public static class MBasicTool
    {
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]

        public static string LoggedIn(IWebDriver driver, FbAccount data, bool isLoginByCookie = false, string url = "")
        {
            // 0) go to mbasic
            if (string.IsNullOrEmpty(url))
            {
                FBTool.GoToFacebook(driver, Constant.FB_MBASIC_URL);
            }
            else
            {
                FBTool.GoToFacebook(driver, url);
                FBTool.WaitingPageLoading(driver);

                if (string.IsNullOrEmpty(FBTool.GetUserId(driver)))
                    FBTool.GoToFacebook(driver, Constant.FB_MBASIC_URL);
            }

            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(800);

            // 1) already logged in?
            if (!string.IsNullOrEmpty(FBTool.GetUserId(driver)))
                return "success";

            // 2) login attempt
            if (isLoginByCookie && !string.IsNullOrWhiteSpace(data.Cookie))
            {
                FBTool.LoginFbByCookie(driver, data.Cookie);
                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(800);

                if (string.IsNullOrEmpty(FBTool.GetUserId(driver)))
                    LoginByUID(driver, data.UID, data.Password);
            }
            else
            {
                LoginByUID(driver, data.UID, data.Password);
            }

            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1200);

            // 3) if logged in after login
            if (!string.IsNullOrEmpty(FBTool.GetUserId(driver)))
                return "success";

            // 4) HARD GUARD: if back to login page => not 2FA, don't type OTP
            if (IsOnLoginPage(driver))
                return FBTool.GetResults(driver);

            // 5) WWW push approval: try switch to authenticator code
            if (IsPushApprovalPage(driver))
            {
                bool switched = TrySwitchPushToAuthenticator_WWW(driver);

                // If cannot switch, user must approve on phone
                if (!switched && IsPushApprovalPage(driver))
                {
                    data.Description = "Need approval on phone (push notification)";
                    return "Need 2FA";
                }

                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(800);
            }

            // 6) OTP 2FA screen -> auto-fill if secret exists, otherwise manual
            if (Is2FAVisible(driver))
            {
                // Auto 2FA only when secret exists
                if (!string.IsNullOrWhiteSpace(data.TwoFA))
                {
                    bool sent = VerifyTwoFactorAuthentication(driver, data);
                    Thread.Sleep(1200);

                    if (!string.IsNullOrEmpty(FBTool.GetUserId(driver)))
                        return "success";
                }

                // Manual fallback
                data.Description = "Need 2FA: Please enter the 6-digit code in the browser...";
                bool ok = FBTool.WaitForLoginSuccess(driver, 180);

                if (ok && !string.IsNullOrEmpty(FBTool.GetUserId(driver)))
                {
                    data.Description = "Login success after 2FA";
                    return "success";
                }

                data.Description = "2FA not completed";
                return "Need 2FA";
            }

            // 7) everything else -> use your existing result parser
            return FBTool.GetResults(driver);
        }


        // =======================
        // Helpers (same class)
        // =======================

        //private static bool IsOnLoginPage(IWebDriver driver)
        //{
        //    try
        //    {
        //        bool hasEmail = driver.FindElements(By.Name("email")).Any(e => e.Displayed);
        //        bool hasPass = driver.FindElements(By.Name("pass")).Any(e => e.Displayed);

        //        if (hasEmail && hasPass) return true;

        //        string url = driver.Url ?? "";
        //        return url.IndexOf("login", StringComparison.OrdinalIgnoreCase) >= 0;
        //    }
        //    catch { return false; }
        //}

        private static bool IsOnLoginPage(IWebDriver driver)
        {
            if (driver == null) return false;

            try
            {
                string url = (driver.Url ?? "").ToLower();

                // 1️⃣ Strong detection: actual login form visible
                bool hasEmail = driver.FindElements(By.Name("email"))
                                      .Any(e => e.Displayed && e.Enabled);

                bool hasPass = driver.FindElements(By.Name("pass"))
                                     .Any(e => e.Displayed && e.Enabled);

                bool hasLoginBtn = driver.FindElements(By.Name("login"))
                                         .Any(e => e.Displayed && e.Enabled);

                if (hasEmail && hasPass && hasLoginBtn)
                    return true;

                // 2️⃣ URL fallback (ONLY if clearly login page)
                if (url.EndsWith("/login") || url.Contains("/login.php"))
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }
        private static bool IsPushApprovalPage(IWebDriver driver)
        {
            try
            {
                string html = (driver.PageSource ?? "").ToLower();

                // Push approval page - detected by HTML content only
                // (URL pattern varies, especially after bot verification)
                if (html.Contains("check your notifications on another device") ||
                    html.Contains("waiting for approval") ||
                    html.Contains("approve from another device") ||
                    (html.Contains("try another way") && html.Contains("notification")) ||
                    html.Contains("រង់ចាំការអនុម័ត") ||
                    html.Contains("chờ phê duyệt"))
                    return true;

                return false;
            }
            catch { return false; }
        }

        private static bool Is2FAVisible(IWebDriver driver)
        {
            try
            {
                // mbasic approvals_code
                bool approvals =
                    driver.FindElements(By.Name("approvals_code")).Any(e => e.Displayed && e.Enabled) ||
                    driver.FindElements(By.Id("approvals_code")).Any(e => e.Displayed && e.Enabled);

                // some pages show aria-label
                bool loginCode =
                    driver.FindElements(By.XPath("//input[@aria-label='Login code']")).Any(e => e.Displayed && e.Enabled);

                return approvals || loginCode;
            }
            catch { return false; }
        }
        private static bool TrySwitchPushToAuthenticator_WWW(IWebDriver driver)
        {
            return FBTool.HandlePushApproval_WWW(driver);
        }



        private static IWebElement FindClickableByText(WebDriverWait wait, IWebDriver driver, string text)
        {
            try
            {
                return wait.Until(d =>
                {
                    // find span/div with text then go up to role=button or button
                    var node = d.FindElements(By.XPath($"//*[self::span or self::div][normalize-space(text())='{text}']")).FirstOrDefault();
                    if (node == null) return null;

                    var clickable = node.FindElements(By.XPath("./ancestor::*[@role='button' or self::button][1]")).FirstOrDefault();
                    if (clickable == null) clickable = node;

                    return (clickable.Displayed && clickable.Enabled) ? clickable : null;
                });
            }
            catch { return null; }
        }

        private static IWebElement FindClickableContainsAny(WebDriverWait wait, IWebDriver driver, string[] containsTexts)
        {
            try
            {
                string xpath = string.Join(" or ", containsTexts.Select(t => $"contains(.,'{t}')"));
                return wait.Until(d =>
                {
                    var node = d.FindElements(By.XPath($"//*[{xpath}]")).FirstOrDefault();
                    if (node == null) return null;

                    var clickable = node.FindElements(By.XPath("./ancestor::*[@role='button' or self::button][1]")).FirstOrDefault();
                    if (clickable == null) clickable = node;

                    return (clickable.Displayed && clickable.Enabled) ? clickable : null;
                });
            }
            catch { return null; }
        }

        private static void JsClick(IWebDriver driver, IWebElement el)
        {
            try { el.Click(); }
            catch
            {
                try { ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", el); }
                catch { }
            }
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static int Login(IWebDriver driver, FbAccount data, bool isLoginByCookie = false, string url = "")
        {
            if (string.IsNullOrEmpty(url))
            {
                FBTool.GoToFacebook(driver, Constant.FB_MBASIC_URL);
            }
            else
            {
                FBTool.GoToFacebook(driver, url);
                FBTool.WaitingPageLoading(driver);
                if (string.IsNullOrEmpty(FBTool.GetUserId(driver)))
                {
                    FBTool.GoToFacebook(driver, Constant.FB_MBASIC_URL);
                }
            }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            if (string.IsNullOrEmpty(FBTool.GetUserId(driver)))
            {
                if (isLoginByCookie)
                {
                    FBTool.LoginByCookie(driver, data.Cookie);
                    FBTool.WaitingPageLoading(driver);
                    Thread.Sleep(1000);
                    if (string.IsNullOrEmpty(FBTool.GetUserId(driver)))
                    {
                        LoginByUID(driver, data.UID, data.Password);
                    }
                }
                else
                {
                    LoginByUID(driver, data.UID, data.Password);
                }
                if (!string.IsNullOrEmpty(data.TwoFA))
                {
                    Thread.Sleep(1000);
                    if (Is2FA(driver))
                    {
                        VerifyTwoFactorAuthentication(driver, data);
                    }
                }
            }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);            

            return FBTool.GetResult(driver,data);
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool VerifyTwoFactorAuthentication(IWebDriver driver, FbAccount data)
        {
            string res = FBTool.AutoFill2FACode(driver, data);
            return res == "ok";
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void LoginByUID(IWebDriver driver, string uid, string password)
        {
            string email = uid;
            string pass = password;
            if (!string.IsNullOrEmpty(email))
            {
                email = email.Trim();
            }
            if (!string.IsNullOrEmpty(pass))
            {
                pass = pass.Trim();
            }
            try
            {
                driver.FindElement(By.Name("email")).Clear();
                Thread.Sleep(600);
                driver.FindElement(By.Name("email")).SendKeys(email);// email
                Thread.Sleep(600);
            }
            catch (Exception) { }
            try
            {
                driver.FindElement(By.Name("pass")).Clear();
                Thread.Sleep(600);
                driver.FindElement(By.Name("pass")).SendKeys(pass);// password
                Thread.Sleep(600);
            }
            catch (Exception) { }
            try
            {
                driver.FindElement(By.Name("login")).Click();
                Thread.Sleep(2500);
            }
            catch (Exception) { }
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool Is2FA(IWebDriver driver)
        {
            bool isWorking = false;
            int counter = 3;
            do
            {
                Thread.Sleep(1000);
                try
                {
                    driver.FindElement(By.XPath("//input[@aria-label='Login code']"));
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//input[@name='approvals_code']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div[3]/div[1]/div/form/div/div[2]/div[3]/span/input"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);

            return isWorking;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void ChangeLanguage(IWebDriver driver)
        {
            try
            {
                driver.Navigate().GoToUrl("https://mbasic.facebook.com/language");
            }
            catch (Exception) { }
            int counter = 10;
            bool isWorking = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//input[@value='English (US)']")).Click();
                    isWorking = true;
                    Thread.Sleep(1000);
                }
                catch (Exception) { }
            } while (!isWorking && counter-- > 0);
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void LogoutAllDevices(IWebDriver driver)
        {
            bool isWorking = false;
            try
            {
                driver.Navigate().GoToUrl("https://mbasic.facebook.com/settings/security_login/sessions/log_out_all/confirm/?_rdc=1&_rdr");
            }
            catch (Exception) { }

            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);

            try
            {
                driver.FindElement(By.XPath("//span[contains(text(),'Log Out')]")).Click();
                isWorking = true;
            }
            catch (Exception) { }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div/div/div[2]/div/table/tbody/tr/td/div[2]/a[1]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("//*[contains(text(),'Log Out')]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            Thread.Sleep(2000);
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void ChangeTime(IWebDriver driver)
        {
            try
            {
                driver.Navigate().GoToUrl("https://mbasic.facebook.com/settings/sms/time/?view=start");
            }
            catch (Exception) { }

            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            bool isWorking = false;
            int counter = 6;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.Id("sms_time_1")).Click();
                    Thread.Sleep(1000);
                    isWorking = true;
                }
                catch (Exception) { }
            } while (!isWorking && counter-- > 0);
            try
            {
                driver.Navigate().GoToUrl("https://mbasic.facebook.com/settings/sms/time/?view=stop");
            }
            catch (Exception) { }

            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            isWorking = false;
            counter = 6;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.Id("sms_time_1")).Click();
                    Thread.Sleep(1500);
                    isWorking = true;
                }
                catch (Exception) { }
            } while (!isWorking && counter-- > 0);
        }
    }
}
