using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Linq;
using ToolKHBrowser.ToolLib.Data;
using ToolKHBrowser.ViewModels;
using ToolLib.Data;
using ToolLib.Tool;
using ToolKHBrowser.ViewModels;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using Pages = ToolKHBrowser.ViewModels.Pages;

namespace ToolKHBrowser.ToolLib.Tool
{
    public static class WebFBTool
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static bool TrySetClipboardTextSafe(string text)
        {
            try
            {
                var value = text ?? string.Empty;
                if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                {
                    System.Windows.Forms.Clipboard.SetText(value);
                    return true;
                }

                Exception clipboardError = null;
                var thread = new Thread(() =>
                {
                    try
                    {
                        System.Windows.Forms.Clipboard.SetText(value);
                    }
                    catch (Exception ex)
                    {
                        clipboardError = ex;
                    }
                });
                thread.IsBackground = true;
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join(1500);

                return clipboardError == null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void GoToWeb(IWebDriver driver)
        {

        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static string LoggedIn(IWebDriver driver, FbAccount data, bool isLoginByCookie = false, bool isCloseAllPopup = true, string url = "")
        {
            if (string.IsNullOrEmpty(url))
            {
                FBTool.GoToFacebook(driver, Constant.FB_WEB_URL);
            }
            else
            {
                FBTool.GoToFacebook(driver, url);
                FBTool.WaitingPageLoading(driver);
                if (string.IsNullOrEmpty(FBTool.GetUserId(driver)))
                {
                    FBTool.GoToFacebook(driver, Constant.FB_WEB_URL);
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
                        LoginByUID(driver, data);
                    }
                }
                else
                {
                    LoginByUID(driver, data);
                }
                if (!string.IsNullOrEmpty(data.TwoFA))
                {
                    // Wait up to 10s for success or 2FA screen
                    for (int i = 0; i < 10; i++)
                    {
                        if (!string.IsNullOrEmpty(FBTool.GetUserId(driver)) || Is2FA(driver))
                            break;
                        Thread.Sleep(1000);
                    }

                    if (Is2FA(driver))
                    {
                        VerifyTwoFactorAuthentication(driver, data);
                    }
                }
            }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);

            return FBTool.GetResults(driver);
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool IsLogin()
        {
            bool login = false;

            return login;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static int Login(IWebDriver driver, FbAccount data, bool isLoginByCookie = false, bool isCloseAllPopup = true, string url = "")
        {
            int result = 0;
            if (string.IsNullOrEmpty(url))
            {
                FBTool.GoToFacebook(driver, Constant.FB_WEB_URL);
            }
            else
            {
                FBTool.GoToFacebook(driver, url);
                FBTool.WaitingPageLoading(driver);
                if (string.IsNullOrEmpty(FBTool.GetUserId(driver)))
                {
                    FBTool.GoToFacebook(driver, Constant.FB_WEB_URL);
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
                        LoginByUID(driver, data);
                    }
                }
                else
                {
                    LoginByUID(driver, data);
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

            return FBTool.GetResult(driver, data);
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void LoginByUID(IWebDriver driver, FbAccount data)
        {
            Thread.Sleep(2000);
            string email = data.UID ?? "";
            string pass = data.Password ?? "";

            try { email = email.Trim(); } catch { }
            try { pass = pass.Trim(); } catch { }

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass)) return;

            // 1) Find Email field
            IWebElement inputEmail = null;
            try
            {
                inputEmail = driver.FindElements(By.Id("email")).FirstOrDefault(e => e.Displayed)
                          ?? driver.FindElements(By.Name("email")).FirstOrDefault(e => e.Displayed)
                          ?? driver.FindElements(By.XPath("//input[@name='email']")).FirstOrDefault(e => e.Displayed)
                          ?? driver.FindElements(By.XPath("//input[@placeholder='Email or mobile number']")).FirstOrDefault(e => e.Displayed)
                          ?? driver.FindElements(By.XPath("//input[@placeholder='Email address or phone number']")).FirstOrDefault(e => e.Displayed);
            }
            catch { }

            if (inputEmail != null)
            {
                try
                {
                    inputEmail.Click();
                    inputEmail.SendKeys(OpenQA.Selenium.Keys.Control + "a");
                    inputEmail.SendKeys(OpenQA.Selenium.Keys.Delete);
                    Thread.Sleep(500);
                    inputEmail.SendKeys(email);
                }
                catch { }
            }

            // 2) Find Password field
            IWebElement inputPass = null;
            try
            {
                inputPass = driver.FindElements(By.Id("pass")).FirstOrDefault(e => e.Displayed)
                         ?? driver.FindElements(By.Name("pass")).FirstOrDefault(e => e.Displayed)
                         ?? driver.FindElements(By.XPath("//input[@name='pass']")).FirstOrDefault(e => e.Displayed)
                         ?? driver.FindElements(By.XPath("//input[@type='password']")).FirstOrDefault(e => e.Displayed);
            }
            catch { }

            if (inputPass != null)
            {
                try
                {
                    inputPass.Click();
                    inputPass.Clear();
                    Thread.Sleep(500);
                    inputPass.SendKeys(pass);
                }
                catch { }
            }

            // 3) Find Login Button
            try
            {
                IWebElement btnLogin = driver.FindElements(By.Name("login")).FirstOrDefault(e => e.Displayed)
                                   ?? driver.FindElements(By.XPath("//button[@name='login']")).FirstOrDefault(e => e.Displayed)
                                   ?? driver.FindElements(By.XPath("//button[@type='submit']")).FirstOrDefault(e => e.Displayed)
                                   ?? driver.FindElements(By.XPath("//div[@role='button' and (contains(.,'Log In') or contains(.,'login'))]")).FirstOrDefault(e => e.Displayed);

                if (btnLogin != null)
                {
                    btnLogin.Click();
                }
                else
                {
                    // Fallback to Enter key
                    inputPass?.SendKeys(OpenQA.Selenium.Keys.Enter);
                }
            }
            catch { }
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool Is2FA(IWebDriver driver)
        {
            return FBTool.Is2FA(driver);
        }
        public static bool TwoStepVerification(IWebDriver driver)
        {
            return FBTool.HandlePushApproval_WWW(driver);
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void VerifyTwoFactorAuthentication(IWebDriver driver, FbAccount data)
        {
            // Use the centralized robust 2FA logic
            FBTool.AutoFill2FACode(driver, data);
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void AllowCookies(IWebDriver driver)
        {
            bool isWorking = false;
            try
            {
                driver.FindElement(By.XPath("//div[@aria-label='Allow essential and optional cookies']")).Click();
                isWorking = true;
            }
            catch (Exception) { }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div[2]/div/div/div/div[2]/div/div[2]/div[1]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (isWorking)
            {
                Thread.Sleep(1000);
            }
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void CloseAllPopup(IWebDriver driver)
        {
            FBTool.Close(driver);
            Thread.Sleep(500);
            bool isWorking = CloseCrossApp(driver);
            if (isWorking)
            {
                Thread.Sleep(1000);
            }
            isWorking = StepNextAfterLogin(driver);
            if (isWorking)
            {
                Thread.Sleep(1000);
                isWorking = StepGetStartAfterLogin(driver);
                if (isWorking)
                {
                    Thread.Sleep(1000);
                }
            }
            StepCloseIntroductionAfterLogin(driver);
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool StepNextAfterLogin(IWebDriver driver)
        {
            bool isWorking = false;

            try
            {
                driver.FindElement(By.XPath("/html/body/div[11]/div[1]/div/div[2]/div/div/div/div/div[1]/div[2]/div")).Click();
                Thread.Sleep(1000);
            }
            catch (Exception) { }
            try
            {
                driver.FindElement(By.XPath("//div[@aria-label='Next']")).Click();
                Thread.Sleep(1000);
                isWorking = true;
            }
            catch (Exception) { }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[9]/div[1]/div/div[2]/div/div/div/div/div/div[1]/div/div[5]/div")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[10]/div[1]/div/div[2]/div/div/div/div/div/div[1]/div/div[5]/div")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[11]/div[1]/div/div[2]/div/div/div/div/div/div[1]/div/div[5]/div")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }

            return isWorking;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool StepGetStartAfterLogin(IWebDriver driver)
        {
            bool isWorking = false;
            try
            {
                driver.FindElement(By.XPath("//div[@aria-label='Get Started']")).Click();
                isWorking = true;
            }
            catch (Exception) { }
            if (!isWorking)
            {
                Thread.Sleep(1000);
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Get started']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                Thread.Sleep(1000);
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[9]/div[1]/div/div[2]/div/div/div/div/div/div[2]/div/div/div[3]/div")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[10]/div[1]/div/div[2]/div/div/div/div/div/div[2]/div/div/div[3]/div")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[11]/div[1]/div/div[2]/div/div/div/div/div/div[2]/div/div/div[3]/div")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }

            return isWorking;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool StepCloseIntroductionAfterLogin(IWebDriver driver)
        {
            bool is_working = false;
            try
            {
                driver.FindElement(By.XPath("//div[@aria-label='Close Introduction']")).Click();
                is_working = true;
            }
            catch (Exception) { }
            try
            {
                driver.FindElement(By.XPath("//*[@aria-label='Close']")).Click();
                is_working = true;
            }
            catch (Exception) { }
            try
            {
                driver.FindElement(By.XPath("/html/body/div[10]/div[1]/div/div[2]/div/div/div/div[1]/div[2]/div")).Click();
                is_working = true;
            }
            catch (Exception) { }
            try
            {
                driver.FindElement(By.XPath("/html/body/div[11]/div[1]/div/div[2]/div/div/div/div[1]/div[2]/div")).Click();
                is_working = true;
            }
            catch (Exception) { }
            try
            {
                driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div[2]/div/div/div/div[1]/div[2]/div")).Click();
                is_working = true;
            }
            catch (Exception) { }

            return is_working;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool CloseCrossApp(IWebDriver driver)
        {
            bool isWorking = true;

            try
            {
                driver.FindElement(By.XPath("//div[@aria-label='Done']")).Click();
                isWorking = true;
            }
            catch (Exception) { }

            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[4]/div[1]/div/div[2]/div/div/div/div/div/div[1]/div/div[3]/div/div[2]/div[1]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div/div[2]/div/div/div/div/div/div[1]/div/div[3]/div/div[1]/div[1]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[9]/div[1]/div/div[2]/div/div/div/div/div/div[1]/div/div[3]/div/div[1]/div[1]")).Click();
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[3]/div[1]/div/div[2]/div/div/div/div/div/div[1]/div/div[3]/div/div[1]/div[1]")).Click();
                }
                catch (Exception) { }
            }

            return isWorking;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static string GetName(IWebDriver driver)
        {
            string name = "";
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/bookmarks");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            CloseAllPopup(driver);
            Thread.Sleep(1000);


            try
            {
                name = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div/div[1]/div[1]/div/div[1]/ul/li/div/a/div[1]/div[2]/div/div/div/div/span/span")).GetAttribute("innerHTML").Trim();
            }
            catch (Exception) { }
            if (string.IsNullOrEmpty(name))
            {
                try
                {
                    name = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div[1]/ul/li/div/a/div[1]/div[2]/div/div/div/div/span/span")).GetAttribute("innerHTML").Trim();
                }
                catch (Exception) { }
            }

            return name;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static int GetFriends(IWebDriver driver)
        {
            string friends = "";
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/profile.php");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            CloseAllPopup(driver);
            Thread.Sleep(1000);

            try
            {
                friends = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div/div[3]/div/div/div/div[1]/div/div/div[1]/div/div/div/div/div/div/a[3]/div[1]/span/span[2]")).GetAttribute("innerHTML").Trim();
            }
            catch (Exception) { }
            if (string.IsNullOrEmpty(friends))
            {
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-haspopup='menu']")).Click();
                    Thread.Sleep(1500);
                }
                catch (Exception) { }
                try
                {
                    friends = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div[1]/div[1]/div/div/div/div/div[1]/div/a[1]/div[1]/div/div/span/span[2]")).GetAttribute("innerHTML").Trim();
                }
                catch (Exception) { }
            }
            int num = 0;
            try
            {
                num = Int32.Parse(friends);
            }
            catch (Exception) { }
            if (num == 0)
            {
                num = MobileFBTool.GetTotalFriends(driver);
            }

            return num;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static string GetGender(IWebDriver driver)
        {
            string gender = "";
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/me/about_contact_and_basic_info/");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            CloseAllPopup(driver);
            Thread.Sleep(1000);
            IWebElement elm = null;
            try
            {
                elm = driver.FindElement(By.XPath("//div[@aria-label='Add your gender']"));
            }
            catch (Exception) { }
            if (elm == null)
            {
                try
                {
                    elm = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div/div[4]/div/div/div/div/div[1]/div/div/div/div/div[2]/div/div/div/div[3]/div[6]/div/div/div[2]/div"));
                }
                catch (Exception) { }
            }
            if (elm != null)
            {
                FBTool.Scroll(driver, elm);
                FBTool.Scroll(driver, 1000, false);
                try
                {
                    elm.Click();
                    Thread.Sleep(2000);
                }
                catch (Exception) { }
            }
            try
            {
                IReadOnlyCollection<IWebElement> element = driver.FindElements(By.XPath("//div[@role='combobox']"));
                gender = element.ElementAt(0).FindElement(By.TagName("span")).GetAttribute("innerHTML");
            }
            catch (Exception) { }

            return gender;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static int GetFriendsRequest(IWebDriver driver)
        {
            string str = "";
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/friends/requests");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            CloseAllPopup(driver);
            Thread.Sleep(1000);

            try
            {
                str = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[1]/div/div[2]/div[1]/div[2]/div/div[2]/div/div/div/div/div/h2/span/span")).GetAttribute("innerHTML").Trim();
            }
            catch (Exception) { }
            if (!string.IsNullOrEmpty(str))
            {
                string[] arr = str.Split(' ');
                try
                {
                    return Int32.Parse(arr[0].Trim());
                }
                catch (Exception) { }
            }

            return 0;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static string GetBirthday(IWebDriver driver)
        {
            string birthday = "";
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/me/about_contact_and_basic_info/");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            CloseAllPopup(driver);
            Thread.Sleep(1000);
            IWebElement elm = null;
            try
            {
                elm = driver.FindElement(By.XPath("//div[@aria-label='Add your birthday']"));
            }
            catch (Exception) { }
            if (elm == null)
            {
                try
                {
                    elm = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div/div[4]/div/div/div/div/div[1]/div/div/div/div/div[2]/div/div/div/div[3]/div[7]/div/div/div[2]/div"));
                }
                catch (Exception) { }
            }
            // click button date of birth
            if (elm != null)
            {
                FBTool.Scroll(driver, elm);
                FBTool.Scroll(driver, 1000, false);
                try
                {
                    elm.Click();
                    Thread.Sleep(2000);
                }
                catch (Exception) { }
            }
            // get date of birth
            string m = "", d = "", y = "";
            try
            {
                IReadOnlyCollection<IWebElement> element = driver.FindElements(By.XPath("//div[@role='combobox']"));
                m = element.ElementAt(0).FindElement(By.TagName("span")).GetAttribute("innerHTML");
                d = element.ElementAt(1).FindElement(By.TagName("span")).GetAttribute("innerHTML");
                y = element.ElementAt(2).FindElement(By.TagName("span")).GetAttribute("innerHTML");
            }
            catch (Exception) { }
            if (!string.IsNullOrEmpty(d) && !string.IsNullOrEmpty(m) && !string.IsNullOrEmpty(y))
            {
                m = GetMonthFromString(m);

                birthday = d + "/" + m + "/" + y;
            }

            return birthday;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static string GetMonthFromString(string month)
        {
            string m = "";

            switch (month)
            {
                case "January":
                    m = "01";
                    break;
                case "February":
                    m = "02";
                    break;
                case "March":
                    m = "03";
                    break;
                case "April":
                    m = "04";
                    break;
                case "May":
                    m = "05";
                    break;
                case "June":
                    m = "06";
                    break;
                case "July":
                    m = "07";
                    break;
                case "August":
                    m = "08";
                    break;
                case "September":
                    m = "09";
                    break;
                case "October":
                    m = "10";
                    break;
                case "November":
                    m = "11";
                    break;
                case "December":
                    m = "12";
                    break;
            }

            return m;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void PublicPost(IWebDriver driver)
        {
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/settings?tab=followers");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            CloseAllPopup(driver);
            try
            {
                driver.SwitchTo().Frame(0);
            }
            catch (Exception) { }
            int loop = 0;
            do
            {
                Thread.Sleep(1000);
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div[1]/div[1]/div/div[2]/div[2]/div[2]/div/ul/li[1]/div/div/div/form/div/div[2]")).Click();
                    Thread.Sleep(1000);
                    loop = 100;
                    driver.FindElement(By.XPath("//span[text() = 'Public']")).Click();
                }
                catch (Exception) { }
                loop++;
            } while (loop < 3);

            Thread.Sleep(2000);
            try
            {
                driver.SwitchTo().DefaultContent();
            }
            catch (Exception) { }
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static string GetPrimaryContact(IWebDriver driver)
        {
            string email = "";
            int counter;
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/settings/?tab=account");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            CloseAllPopup(driver);
            try
            {
                driver.SwitchTo().Frame(0);
            }
            catch (Exception) { }
            counter = 6;
            do
            {
                Thread.Sleep(500);
                try
                {
                    email = driver.FindElement(By.XPath("/html/body/div[1]/div[1]/div[1]/div/div[2]/div[2]/div[2]/div/ul/li[3]/a/span[3]/strong")).GetAttribute("innerHTML").Trim();
                }
                catch (Exception) { }
            } while (string.IsNullOrEmpty(email) && counter-- > 0);

            Thread.Sleep(2000);
            try
            {
                driver.SwitchTo().DefaultContent();
            }
            catch (Exception) { }

            return email;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static string GetGroupIDs(IWebDriver driver)
        {
            string ids = "";
            try
            {
                IReadOnlyCollection<IWebElement> elements = driver.FindElements(By.XPath("//a[@role='link']"));
                for (int i = 0; i < elements.Count; i++)
                {
                    string href = elements.ElementAt(i).GetAttribute("href");
                    try
                    {
                        if (href.Contains("/groups/") && !href.Contains("notif"))
                        {
                            string[] arr = href.Split('/');
                            if (arr.Length > 5)
                            {
                                string id = arr[arr.Length - 2].Trim();
                                if (!ids.Contains(id) && !id.Contains("members"))
                                {
                                    if (!string.IsNullOrEmpty(id))
                                    {
                                        if (!string.IsNullOrEmpty(ids))
                                        {
                                            ids += ",";
                                        }
                                        ids += id;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception) { }

            return ids;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static int GetTotalGroup(IWebDriver driver)
        {
            int total = 0;
            bool isWorking = true;

            try
            {
                driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/me/?sk=groups");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            CloseAllPopup(driver);
            FBTool.Scroll(driver, 6000, false);

            string path = "/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div/div[4]/div/div/div/div/div/div/div/div/div[3]/div";
            string path1 = "/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div/div[4]/div/div/div/div/div/div/div/div/div/div[3]/div";
            string path2 = "/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div/div[4]/div/div/div/div/div/div/div/div/div[3]/div";
            isWorking = true;
            do
            {
                isWorking = false;
                try
                {
                    driver.FindElement(By.XPath(path + "[" + (total + 1) + "]"));
                    isWorking = true;
                    total++;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath(path1 + "[" + (total + 1) + "]"));
                        isWorking = true;
                        total++;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath(path2 + "[" + (total + 1) + "]"));
                        isWorking = true;
                        total++;
                    }
                    catch (Exception) { }
                }
            } while (isWorking);

            return total;
        }
        public static void Marketplace(IWebDriver driver, string marketingplace)
        {
            try
            {
                driver.Navigate().GoToUrl("https://www.facebook.com/marketplace/?ref=app_tab");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            bool isWorking = false;
            int counter = 4;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.Id("seo_filters")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[2]/div/div/div/div/div/div/div[3]/div/div[1]/div/div[1]/div/span/div/div/div/div[1]/div/div/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[2]/div/div/div/div/div/div/div[3]/div/div[1]/div/div/div/span/div/div/div/div[1]/div/div/div/div/span")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return;
            }
            IWebElement element = null;
            isWorking = false;
            counter = 6;
            do
            {
                Thread.Sleep(500);
                try
                {
                    element = driver.FindElement(By.XPath("//input[@aria-label='Location']"));
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//input[@aria-autocomplete='list']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return;
            }
            try
            {
                element.SendKeys(OpenQA.Selenium.Keys.Control + 'a');
                Thread.Sleep(500);
                element.SendKeys(OpenQA.Selenium.Keys.Delete);
            }
            catch (Exception) { }
            Thread.Sleep(1000);
            try
            {
                element.SendKeys(marketingplace);
            }
            catch (Exception) { }
            Thread.Sleep(1000);
            try
            {
                element.SendKeys(OpenQA.Selenium.Keys.ArrowDown);
                Thread.Sleep(1500);
                element.SendKeys(OpenQA.Selenium.Keys.Enter);
            }
            catch (Exception) { }
            Thread.Sleep(1000);
            isWorking = false;
            try
            {
                driver.FindElement(By.XPath("//div[@aria-label='Apply']")).Click();
                isWorking = true;
            }
            catch (Exception) { }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div[4]/div/div[2]/div/div/div/div/div")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
        }
        public static void Checkin(IWebDriver driver, string searchCheckin)
        {
            string url = "";
            try
            {
                url = driver.Url;
            }
            catch (Exception) { }
            if (!url.EndsWith(".com/") && !url.EndsWith(".com"))
            {
                try
                {
                    driver.Navigate().GoToUrl("https://facebook.com/");
                }
                catch (Exception) { }
            }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            bool isWorking = false;
            int counter = 4;
            do
            {
                Thread.Sleep(500);
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div[2]/div/div/div/div[3]/div/div[2]/div/div/div/div[1]/div/div[1]/span")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div[2]/div/div/div/div[3]/div/div[2]/div/div/div/div[1]/div/div[1]/span")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return;
            }
            Thread.Sleep(1000);
            counter = 6;
            isWorking = false;
            do
            {
                Thread.Sleep(1000);
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Check in']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[3]/div[1]/div[2]/div/div[4]/div/span/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return;
            }
            Thread.Sleep(2000);
            isWorking = false;
            counter = 4;
            IWebElement element = null;
            do
            {
                Thread.Sleep(500);
                try
                {
                    element = driver.FindElement(By.XPath("//input[@aria-label='Where are you?']"));
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//input[@type='search']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return;
            }
            try
            {
                element.SendKeys(searchCheckin);
            }
            catch (Exception) { }
            Thread.Sleep(1000);
            try
            {
                element.SendKeys(OpenQA.Selenium.Keys.ArrowDown);
                Thread.Sleep(1500);
                element.SendKeys(OpenQA.Selenium.Keys.Enter);
            }
            catch (Exception) { }
            Thread.Sleep(1000);
            isWorking = SubmitPost(driver);
            if (isWorking)
            {
                Thread.Sleep(4000);
            }
        }
        public static bool GoToPost(IWebDriver driver)
        {
            if (TryOpenCreatePostDialog(driver))
            {
                return true;
            }

            bool isWorking = false, isSuccess = false;
            try
            {
                driver.FindElement(By.XPath("//span[text() = 'Photo/Video']")).Click();
                isWorking = true;
            }
            catch (Exception) { }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("//span[text() = 'Photo/video']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div[2]/div/div/div[3]/div/div[2]/div/div/div/div[1]/div/div[1]/span")).Click();

                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div[2]/div/div/div[2]/div/div[2]/div/div/div/div[1]/div/div[1]/span")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div[2]/div/div/div/div[3]/div/div[2]/div/div/div/div[1]/div/div[1]/span")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div/div[1]/div[1]/div/div[2]/div/div/div/div[3]/div/div[2]/div/div/div/div[1]/div/div[1]/span")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            isSuccess = isWorking;
            int counter = 6;
            isWorking = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Select audience']"));
                    isWorking = true;
                }
                catch (Exception) { }
            } while (!isWorking && counter-- > 0);
            if (isWorking)
            {
                isWorking = false;
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[6]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[2]/div/div/div[2]/div/div/div[1]/div/div/div[2]/div/div[1]")).Click();
                    Thread.Sleep(1000);
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[2]/div/div/div[2]/div/div/div[1]/div/div/div[2]/div/div[1]/div/div[1]")).Click();
                        Thread.Sleep(1000);
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (isWorking)
                {
                    isWorking = false;
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[6]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[2]/div/div/div[2]/div/div/div[2]/div/div[2]/div")).Click();
                        Thread.Sleep(1000);
                        isWorking = true;
                    }
                    catch (Exception) { }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[2]/div/div/div[2]/div/div/div[2]/div/div[2]/div")).Click();
                            isWorking = true;
                            Thread.Sleep(2000);
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//div[@aria-label='Done']")).Click();
                            isWorking = true;
                            Thread.Sleep(2000);
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//span[text() = 'Done']")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                }
            }

            if (!isSuccess)
            {
                isSuccess = TryOpenCreatePostDialog(driver);
            }

            return isSuccess;
        }

        private static bool IsCreatePostDialogOpen(IWebDriver driver)
        {
            try
            {
                return driver.FindElements(By.XPath("//div[@role='dialog']")).Any(e =>
                {
                    try { return e.Displayed; } catch (Exception) { return false; }
                });
            }
            catch (Exception) { }

            return false;
        }

        private static bool TryOpenCreatePostDialog(IWebDriver driver)
        {
            if (IsCreatePostDialogOpen(driver))
            {
                return true;
            }

            string[] openSelectors = new[]
            {
                "//div[contains(@aria-label,\"What's on your mind\") or contains(@aria-label,\"What’s on your mind\") or contains(@aria-label,'Create post')]",
                "//span[contains(text(),\"What's on your mind\") or contains(text(),\"What’s on your mind\")]/ancestor::*[@role='button'][1]",
                "//div[@role='button' and .//span[contains(text(),'Create post')]]",
                "//div[@role='button' and .//span[contains(text(),'Create Post')]]",
                "//div[@aria-label='Create post' or @aria-label='Create Post']"
            };

            foreach (var xp in openSelectors)
            {
                try
                {
                    var element = driver.FindElements(By.XPath(xp)).FirstOrDefault(e =>
                    {
                        try { return e.Displayed && e.Enabled; } catch (Exception) { return false; }
                    });
                    if (element == null)
                    {
                        continue;
                    }

                    try { element.Click(); }
                    catch (Exception) { ClickElement(driver, element); }

                    Thread.Sleep(1200);
                    if (IsCreatePostDialogOpen(driver))
                    {
                        return true;
                    }
                }
                catch (Exception) { }
            }

            return IsCreatePostDialogOpen(driver);
        }

        private static bool TryAttachMediaToCreateDialog(IWebDriver driver, string file, int postWaiting)
        {
            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
            {
                return false;
            }

            bool attached = false;
            for (int attempt = 0; attempt < 10 && !attached; attempt++)
            {
                try
                {
                    var inputs = driver.FindElements(By.XPath("//div[@role='dialog']//input[@type='file']")).ToList();
                    if (inputs.Count == 0)
                    {
                        inputs = driver.FindElements(By.XPath("//input[@type='file']")).ToList();
                    }

                    foreach (var input in inputs)
                    {
                        try
                        {
                            input.SendKeys(file);
                            attached = true;
                            break;
                        }
                        catch (Exception) { }
                    }
                }
                catch (Exception) { }

                if (!attached)
                {
                    Thread.Sleep(600);
                }
            }

            if (attached)
            {
                Thread.Sleep(postWaiting);
            }
            return attached;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool PostTimeline(IWebDriver driver, string caption, string file, int postWaiting, int submitWaiting)
        {
            bool isWorking = false, isPhoto = false;
            bool captionInserted = string.IsNullOrWhiteSpace(caption);
            int counter = 0;

            isWorking = GoToPost(driver);
            if (!isWorking || !IsCreatePostDialogOpen(driver))
            {
                return false;
            }

            Thread.Sleep(2000);
            if (!string.IsNullOrEmpty(file))
            {
                isPhoto = TryAttachMediaToCreateDialog(driver, file, postWaiting);
                if (!isPhoto)
                {
                    try
                    {
                        var closeBtn = driver.FindElements(By.XPath("//*[@aria-label='Close'] | //div[@aria-label='Close']")).FirstOrDefault();
                        if (closeBtn != null)
                        {
                            try { closeBtn.Click(); }
                            catch (Exception) { ClickElement(driver, closeBtn); }
                        }
                    }
                    catch (Exception) { }
                    return false;
                }
            }
            if (!string.IsNullOrEmpty(caption))
            {
                isWorking = false;
                counter = 2;
                if (!isPhoto)
                {
                    FBTool.ScrollUp(driver, 1000);
                }
                do
                {
                    Thread.Sleep(1000);
                    if (!isWorking)
                    {
                        try
                        {
                            var editors = driver.FindElements(By.XPath(
                                "//div[@role='dialog']//div[@role='textbox' and @contenteditable='true']" +
                                " | //div[@role='dialog']//div[@contenteditable='true']"));
                            IWebElement editor = null;
                            long bestScore = -1;
                            foreach (var el in editors)
                            {
                                try
                                {
                                    if (el.Displayed && el.Enabled)
                                    {
                                        var size = el.Size;
                                        long score = (long)Math.Max(1, size.Width) * Math.Max(1, size.Height);
                                        if (score > bestScore)
                                        {
                                            bestScore = score;
                                            editor = el;
                                        }
                                    }
                                }
                                catch (Exception) { }
                            }

                            if (editor != null)
                            {
                                try { ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", editor); } catch (Exception) { }
                                try { editor.Click(); Thread.Sleep(300); } catch (Exception) { }

                                bool typed = false;
                                var normalizedCaption = (caption ?? "").Replace("\r\n", "\n").Replace("\r", "\n");
                                var lines = normalizedCaption.Split(new[] { '\n' }, StringSplitOptions.None);

                                // First try paste (more reliable for Unicode + multiline in Facebook editor)
                                try
                                {
                                    if (TrySetClipboardTextSafe(caption))
                                    {
                                        new Actions(driver)
                                            .KeyDown(OpenQA.Selenium.Keys.Control)
                                            .SendKeys("v")
                                            .KeyUp(OpenQA.Selenium.Keys.Control)
                                            .Build()
                                            .Perform();
                                        Thread.Sleep(500);
                                    }
                                }
                                catch (Exception) { }

                                try
                                {
                                    var currentTextAfterPaste = "";
                                    try
                                    {
                                        currentTextAfterPaste = ((IJavaScriptExecutor)driver).ExecuteScript(
                                            "return (arguments[0].innerText || arguments[0].textContent || '')",
                                            editor)?.ToString() ?? "";
                                    }
                                    catch (Exception)
                                    {
                                        currentTextAfterPaste = editor.Text ?? "";
                                    }
                                    currentTextAfterPaste = currentTextAfterPaste.Replace("\r\n", "\n").Replace("\r", "\n");
                                    if (!string.IsNullOrWhiteSpace(normalizedCaption) &&
                                        currentTextAfterPaste.Contains((lines.Length > 0 ? lines[0] : normalizedCaption).Trim()))
                                    {
                                        typed = true;
                                    }
                                }
                                catch (Exception) { }

                                try
                                {
                                    if (!typed && lines.Length > 0)
                                    {
                                        editor.SendKeys(lines[0]);
                                        for (int li = 1; li < lines.Length; li++)
                                        {
                                            // Facebook post editor usually needs Shift+Enter for a newline
                                            new Actions(driver)
                                                .KeyDown(OpenQA.Selenium.Keys.Shift)
                                                .SendKeys(OpenQA.Selenium.Keys.Enter)
                                                .KeyUp(OpenQA.Selenium.Keys.Shift)
                                                .Build()
                                                .Perform();
                                            editor.SendKeys(lines[li]);
                                        }
                                    }
                                    // Verify Facebook editor actually received most of the content.
                                    try
                                    {
                                        var currentText = "";
                                        try
                                        {
                                            currentText = ((IJavaScriptExecutor)driver).ExecuteScript(
                                                "return (arguments[0].innerText || arguments[0].textContent || '')",
                                                editor)?.ToString() ?? "";
                                        }
                                        catch (Exception)
                                        {
                                            currentText = editor.Text ?? "";
                                        }
                                        currentText = currentText.Replace("\r\n", "\n").Replace("\r", "\n");
                                        var firstLine = lines.Length > 0 ? lines[0] : "";
                                        if (!string.IsNullOrWhiteSpace(normalizedCaption))
                                        {
                                            // If only the first line appears, treat as failed and use JS fallback below.
                                            if (!string.IsNullOrWhiteSpace(firstLine) &&
                                                currentText.Trim() == firstLine.Trim() &&
                                                normalizedCaption.Contains("\n"))
                                            {
                                                typed = false;
                                            }
                                            else
                                            {
                                                typed = currentText.Trim().Length > 0;
                                            }
                                        }
                                        else
                                        {
                                            typed = true;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        typed = true;
                                    }
                                }
                                catch (Exception) { }

                                if (!typed)
                                {
                                    try
                                    {
                                        var js = (IJavaScriptExecutor)driver;
                                        js.ExecuteScript(@"
                                            var el = arguments[0], txt = arguments[1];
                                            if (!el) return false;
                                            el.focus();
                                            try { el.innerText = txt; } catch (e) {}
                                            if ((el.innerText || '').trim().length === 0 && (el.textContent || '').trim().length === 0) {
                                                try { el.textContent = txt; } catch (e) {}
                                            }
                                            el.dispatchEvent(new Event('input', { bubbles: true }));
                                            el.dispatchEvent(new Event('change', { bubbles: true }));
                                            return true;
                                        ", editor, caption);
                                        try
                                        {
                                            var currentText = ((IJavaScriptExecutor)driver).ExecuteScript(
                                                "return (arguments[0].innerText || arguments[0].textContent || '')",
                                                editor)?.ToString() ?? "";
                                            currentText = currentText.Replace("\r\n", "\n").Replace("\r", "\n");
                                            var normalizedCaptionCheck = (caption ?? "").Replace("\r\n", "\n").Replace("\r", "\n");
                                            typed = !string.IsNullOrWhiteSpace(currentText) &&
                                                    (currentText.Contains(normalizedCaptionCheck.Split('\n')[0]) || currentText.Length >= Math.Min(20, normalizedCaptionCheck.Length));
                                        }
                                        catch (Exception)
                                        {
                                            typed = true;
                                        }
                                    }
                                    catch (Exception) { }
                                }

                                if (typed)
                                {
                                    isWorking = true;
                                    captionInserted = true;
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                    if (!isPhoto)
                    {
                        try
                        {
                            Thread.Sleep(1000);
                            driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[2]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br")).SendKeys(caption);
                            isWorking = true;
                        }
                        catch (Exception) { }

                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[2]/div[1]/div[1]/div[1]/div/div/div[1]/p/br")).SendKeys(caption);
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                    }
                    else
                    {
                        if (!isWorking)
                        {
                            try
                            {
                                // Newer Facebook create-post dialog editor (works for photo/video posts)
                                var editor = driver.FindElement(By.XPath(
                                    "//div[@role='dialog']//div[@role='textbox' and (" +
                                    "contains(@aria-label,\"What's on your mind\") or " +
                                    "contains(@aria-label,'Write something') or " +
                                    "contains(@aria-label,'Create post') or " +
                                    "@contenteditable='true')]"));
                                editor.Click();
                                Thread.Sleep(300);
                                editor.SendKeys(caption);
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div[2]/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[2]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br")).SendKeys(caption);
                            isWorking = true;
                        }
                        catch (Exception) { }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[2]/div[1]/div[1]/div[1]/div/div/div[1]/p/br")).SendKeys(caption);
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[2]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br")).SendKeys(caption);
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//br[@data-text='true']")).SendKeys(caption);
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[2]/div[1]/div[1]/div[1]/div/div/div[1]/p/br")).SendKeys(caption);
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[2]/div[1]/div[1]/div[1]/div/div/div[1]/p/br")).SendKeys(caption);
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                    }
                } while (!isWorking && counter-- > 0);

                // Do not submit file-only post when caption was requested but editor input failed.
                if (!captionInserted)
                {
                    // If media exists, continue with media-only post instead of aborting.
                    if (string.IsNullOrEmpty(file))
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//*[@aria-label='Close']")).Click();
                            Thread.Sleep(300);
                        }
                        catch (Exception) { }
                        try
                        {
                            driver.FindElement(By.XPath("//div[@aria-label='Close']")).Click();
                            Thread.Sleep(300);
                        }
                        catch (Exception) { }
                        return false;
                    }
                }
            }
            Thread.Sleep(5000);
            isWorking = SubmitPost(driver);

            if (isWorking)
            {
                try
                {
                    Thread.Sleep(1500);
                    try
                    {
                        driver.FindElement(By.XPath("//*[@aria-label='Close']")).Click();
                        Thread.Sleep(500);
                        driver.FindElement(By.XPath("//div[@aria-label='Close']")).Click();
                    }
                    catch (Exception) { }
                    Thread.Sleep(submitWaiting);
                }
                catch (Exception) { }
            }
            return isWorking;
        }
        public static bool SubmitPost(IWebDriver driver)
        {
            bool isWorking = false;
            int counter = 25;
            Thread.Sleep(1000);
            do
            {
                Thread.Sleep(1000);
                try
                {
                    var el = driver.FindElement(By.XPath("//div[@aria-label='Switch back']"));
                    //var el = driver.FindElement(By.XPath("//span[text() = 'Switch back for all future videos']"));
                    WebFBTool.ClickElement(driver, el);
                    Thread.Sleep(2000);
                }
                catch (Exception) { }
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Next']")).Click();
                    Thread.Sleep(2000);
                }
                catch (Exception) { }
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Post']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div[2]/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[3]/div[2]/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[3]/div[2]/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[3]/div[2]/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        string[] selectors = new[]
                        {
                            "//div[@role='dialog']//div[@role='button' and (@aria-label='Post' or @aria-label='Share' or @aria-label='Publish')]",
                            "//div[@role='dialog']//*[self::div or self::span][normalize-space(.)='Post']/ancestor::div[@role='button'][1]",
                            "//div[@role='dialog']//*[self::div or self::span][normalize-space(.)='Share']/ancestor::div[@role='button'][1]",
                            "//div[@role='dialog']//*[self::div or self::span][normalize-space(.)='Publish']/ancestor::div[@role='button'][1]"
                        };

                        foreach (var xp in selectors)
                        {
                            var btn = driver.FindElements(By.XPath(xp)).FirstOrDefault(e =>
                            {
                                try { return e.Displayed && e.Enabled; } catch (Exception) { return false; }
                            });
                            if (btn == null) continue;
                            try { btn.Click(); }
                            catch (Exception) { ClickElement(driver, btn); }
                            isWorking = true;
                            break;
                        }
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);

            return isWorking;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool LikePage(IWebDriver driver)
        {
            bool isWorking = false;
            int counter = 4;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div/div[3]/div/div/div/div[2]/div/div/div/div[1]/div/div")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Follow']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//a[@aria-label='Follow']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[2]/div/div/div[3]/div/div/div/div[2]/div/div/div[1]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Like']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div[1]/div/div[1]/div/div[2]/div/div/div[3]/div/div[1]/div/div/div[2]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div[1]/div/div[1]/div/div[2]/div/div/div[3]/div/div[1]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='like button']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Like']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[text() = 'Like']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div[1]/div/div[1]/div/div[3]/div/div[1]/a")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div/div/div[3]/div/div[1]/div[1]/div[3]/table/tbody/tr/td[2]/a")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//a[text() = 'Follow']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (isWorking)
            {
                Thread.Sleep(2000);
            }

            return isWorking;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool AddFriend(IWebDriver driver)
        {
            int counter = 3;
            bool isWorking = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//*[@aria-label='Close']")).Click();
                }
                catch (Exception) { }
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Close']")).Click();
                }
                catch (Exception) { }
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Add Friend']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Add friend']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div/div[3]/div/div/div/div[2]/div/div/div/div[1]/div/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (isWorking)
            {
                Thread.Sleep(2000);
            }

            return isWorking;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool IsJoinGroup(IWebDriver driver)
        {
            bool isWorking = false;
            try
            {
                driver.FindElement(By.XPath("//div[@aria-label='Join Group']"));
                isWorking = true;
            }
            catch (Exception) { }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Join group']"));
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("//span[@aria-label='Join Group']"));
                    isWorking = true;
                }
                catch (Exception) { }
            }

            return isWorking;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool JoinGroup(IWebDriver driver, string joinAnswer = "Okay")
        {
            int counter = 4;
            bool isWorking = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//*[@aria-label='Close']")).Click();
                }
                catch (Exception) { }
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Close']")).Click();
                }
                catch (Exception) { }
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Join Group']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Join Group']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Join group']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Follow Group']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Follow group']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[1]/div[2]/div/div/div/div/div[2]/div/div[1]/div/div/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    FBTool.Scroll(driver, 500, false);
                }
            } while (!isWorking && counter-- > 0);
            if (isWorking)
            {
                bool isAnwser = false;
                counter = 3;
                Thread.Sleep(1500);
                if (string.IsNullOrEmpty(joinAnswer))
                {
                    joinAnswer = "Okay";
                }
                do
                {
                    FBTool.Scroll(driver, 1000, false);
                    try
                    {
                        driver.FindElement(By.XPath("//span[text() = 'Answer Questions']")).Click();
                        isAnwser = true;
                    }
                    catch (Exception) { }
                    if (!isAnwser)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//span[text() = 'Answer questions']")).Click();
                            isAnwser = true;
                        }
                        catch (Exception) { }
                    }
                    if (isAnwser)
                    {
                        isWorking = false;
                        try
                        {
                            driver.FindElement(By.XPath("//textarea[@placeholder='Write an answer...']")).SendKeys(joinAnswer);
                            isWorking = true;
                        }
                        catch (Exception) { }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//input[@type='checkbox']")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//input[@type='radio']")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        Thread.Sleep(1000);
                        try
                        {
                            driver.FindElement(By.XPath("//div[@aria-label='Submit']")).Click();
                        }
                        catch (Exception) { }
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div[3]/div[3]/div[3]/div")).Click();
                        }
                        catch (Exception) { }
                    }
                } while (!isAnwser && counter-- > 0);
            }
            if (isWorking)
            {
                Thread.Sleep(2000);
            }

            return isWorking;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void GoToGroup(IWebDriver driver, string groupId)
        {
            try
            {
                driver.Navigate().GoToUrl(FBTool.GetSafeGroupUrl(Constant.FB_WEB_URL, groupId));
            }
            catch (Exception) { }

            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            CloseAllPopup(driver);
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool PostInGroup_bb(IWebDriver driver, string caption = "", string file = "", int postWaiting = 2000, int submitWaiting = 2000, bool isAutoLeave = false, bool isPostIA = false)
        {
            bool isWorking = false, isFile = false;
            int counter = 4;
            FBTool.Scroll(driver, 1000, false);

            do
            {
                try
                {
                    driver.FindElement(By.XPath("//span[contains(text(),'Create a public post…')]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[4]/div/div/div/div/div[1]/div[1]/div/div/div/div[1]/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[contains(text(),'Write something...')]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[text() = 'Discussion']")).Click();
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            counter = 5;
            isWorking = false;
            string xPathCaption1 = "/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br";
            string xPathCaption2 = "/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div/div/div/div/div/div/div[2]/div/div/div/div/span/br";
            string xPathCaption = "";
            do
            {
                Thread.Sleep(1000);
                try
                {
                    driver.FindElement(By.XPath(xPathCaption1));
                    xPathCaption = xPathCaption1;
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath(xPathCaption2));
                        xPathCaption = xPathCaption2;
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (isPostIA)
            {
                IWebElement element;
                try
                {
                    element = driver.FindElement(By.XPath(xPathCaption));
                    element.SendKeys(file);
                    Thread.Sleep(postWaiting);
                    isWorking = true;
                }
                catch (Exception) { }
                if (isWorking)
                {
                    element = null;
                    try
                    {

                        element = driver.FindElement(By.XPath("//span[@data-text='true']"));
                    }
                    catch (Exception) { }
                    if (element != null)
                    {
                        try
                        {
                            element.SendKeys(OpenQA.Selenium.Keys.Control + "a");
                            Thread.Sleep(1000);
                            element.SendKeys(OpenQA.Selenium.Keys.Delete);
                            //element.Clear();
                            Thread.Sleep(1000);
                        }
                        catch (Exception) { }
                        bool isCaption = false;
                        try
                        {
                            driver.FindElement(By.XPath("//br[@data-text='true']")).SendKeys(caption + " ");
                            isCaption = true;
                        }
                        catch (Exception) { }
                        if (isCaption)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }
            }
            else
            {
                Thread.Sleep(1000);
                if (!string.IsNullOrEmpty(file))
                {
                    try
                    {
                        IWebElement upload_file = driver.FindElement(By.XPath("//input[@type='file']"));
                        upload_file.SendKeys(file);
                        Thread.Sleep(postWaiting);
                        isFile = true;
                    }
                    catch (Exception) { }
                }
                if (!string.IsNullOrEmpty(caption))
                {
                    isWorking = false;
                    counter = 1;
                    do
                    {
                        Thread.Sleep(1500);
                        if (!isFile)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div/div/div/div/div/div/div[2]/div/div/div/div/span/br")).SendKeys(caption + " ");
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        else
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//br[@data-text='true']")).SendKeys(caption + " ");
                            }
                            catch (Exception) { }
                        }
                    } while (!isWorking && counter-- > 0);
                    Thread.Sleep(1000);
                }
            }
            isWorking = false;
            counter = 6;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div[2]/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[3]/div[2]/div/div")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Post']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (isWorking)
            {
                bool isPost = true;
                counter = 10;
                do
                {
                    Thread.Sleep(500);
                    try
                    {
                        driver.FindElement(By.XPath("//*[text() = 'Your post has been submitted and is pending approval by an admin or moderator.']"));
                        isPost = false;
                    }
                    catch (Exception) { }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[10]/ul/li/div/div/div[1]/span/span"));
                            isPost = false;
                        }
                        catch (Exception) { }
                    }
                } while (isPost && counter-- > 0);
                if (isAutoLeave && !isPost)
                {
                    FBTool.Close(driver);
                    Thread.Sleep(2000);
                    LeaveGroup(driver);
                    Thread.Sleep(1000);
                    isWorking = false;
                }
                else
                {
                    try
                    {
                        Thread.Sleep(submitWaiting);
                    }
                    catch (Exception) { }
                    FBTool.Close(driver);
                }
            }

            return isWorking;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool PostInGroup(IWebDriver driver, string caption = "", string file = "", int postWaiting = 2000, int submitWaiting = 2000, bool isAutoLeave = false, bool isPostIA = false, bool isMemberGroup = true)
        {
            bool isWorking = false, isFile = false;
            int counter = 4;
            do
            {
                try
                {
                    driver.FindElement(By.XPath("//span[contains(text(),'Create a public post…')]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[contains(text(),'Write something...')]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div/div[1]/div[1]/div/div[3]/div/div/div[4]/div/div[2]/div/div/div/div[1]/div/div/div/div/div[1]/div/div[1]/span")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[4]/div/div/div/div/div[1]/div[1]/div/div/div/div[1]/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[contains(text(),'Write something...')]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[text() = 'Discussion']")).Click();
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            counter = 5;
            isWorking = false;
            string xPathCaption1 = "/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br";
            string xPathCaption2 = "/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div/div/div/div/div/div/div[2]/div/div/div/div/span/br";
            string xPathCaption3 = "/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br";
            string xPathCaption4 = "/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br";
            string xPathCaption5 = "/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br";
            string xPathCaption = "";
            do
            {
                Thread.Sleep(1000);
                try
                {
                    driver.FindElement(By.XPath(xPathCaption1));
                    xPathCaption = xPathCaption1;
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath(xPathCaption2));
                        xPathCaption = xPathCaption2;
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath(xPathCaption3));
                        xPathCaption = xPathCaption3;
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath(xPathCaption4));
                        xPathCaption = xPathCaption4;
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath(xPathCaption5));
                        xPathCaption = xPathCaption5;
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (isPostIA)
            {
                IWebElement element;
                try
                {
                    element = driver.FindElement(By.XPath(xPathCaption));
                    element.SendKeys(file);
                    Thread.Sleep(postWaiting);
                    isWorking = true;
                }
                catch (Exception) { }
                if (isWorking)
                {
                    element = null;
                    try
                    {

                        element = driver.FindElement(By.XPath("//span[@data-text='true']"));
                    }
                    catch (Exception) { }
                    if (element != null)
                    {
                        try
                        {
                            element.SendKeys(OpenQA.Selenium.Keys.Control + "a");
                            Thread.Sleep(1000);
                            element.SendKeys(OpenQA.Selenium.Keys.Delete);
                            //element.Clear();
                            Thread.Sleep(1000);
                        }
                        catch (Exception) { }
                        bool isCaption = false;
                        try
                        {
                            driver.FindElement(By.XPath("//br[@data-text='true']")).SendKeys(caption + " ");
                            isCaption = true;
                        }
                        catch (Exception) { }
                        if (isCaption)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }
            }
            else
            {
                Thread.Sleep(1000);

                if (!string.IsNullOrEmpty(file))
                {
                    try
                    {
                        IWebElement upload_file = driver.FindElement(By.XPath("//input[@type='file']"));
                        upload_file.SendKeys(file);
                        Thread.Sleep(postWaiting);
                        isFile = true;
                    }
                    catch (Exception) { }
                }
                if (!string.IsNullOrEmpty(caption))
                {
                    isWorking = false;
                    counter = 1;
                    do
                    {
                        Thread.Sleep(1500);
                        if (!isFile)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div/div/div/div/div/div/div[2]/div/div/div/div/span/br")).SendKeys(caption + " ");
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        else
                        {
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div[2]/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br")).SendKeys(caption + " ");
                                isWorking = true;
                            }
                            catch (Exception) { }
                            if (!isWorking)
                            {
                                try
                                {
                                    driver.FindElement(By.XPath("//span[@data-text='true']")).SendKeys(caption + " ");
                                    isWorking = true;
                                }
                                catch (Exception) { }
                            }
                            if (!isWorking)
                            {
                                try
                                {
                                    driver.FindElement(By.XPath("//br[@data-text='true']")).SendKeys(caption + " ");
                                    isWorking = true;
                                }
                                catch (Exception) { }
                            }
                        }
                    } while (!isWorking && counter-- > 0);
                    Thread.Sleep(1000);
                }
            }

            ClickButtonPost(driver);

            if (isWorking)
            {
                bool isPost = true;
                if (isMemberGroup)
                {
                    counter = 10;
                    do
                    {
                        Thread.Sleep(500);
                        try
                        {
                            driver.FindElement(By.XPath("//*[text() = 'Your post has been submitted and is pending approval by an admin or moderator.']"));
                            isPost = false;
                        }
                        catch (Exception) { }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div[10]/ul/li/div/div/div[1]/span/span"));
                                isPost = false;
                            }
                            catch (Exception) { }
                        }
                    } while (isPost && counter-- > 0);
                }

                if (isAutoLeave && !isPost)
                {
                    FBTool.Close(driver);
                    Thread.Sleep(2000);
                    LeaveGroup(driver);
                    Thread.Sleep(1000);
                    isWorking = false;
                }
                else
                {
                    try
                    {
                        Thread.Sleep(submitWaiting);
                    }
                    catch (Exception) { }
                    FBTool.Close(driver);
                }
            }

            return isWorking;
        }
        public static void ClickButtonPost(IWebDriver driver)
        {
            bool isWorking = false;
            int counter = 10;
            do
            {
                Thread.Sleep(500);
                IWebElement element = null;
                if (element == null)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[3]/div[3]/div/div"));
                    }
                    catch (Exception) { }
                }
                if (element == null)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[3]/div[4]/div"));
                    }
                    catch (Exception) { }
                }
                if (element == null)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[4]/div/div[2]/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[3]/div[3]/div/div"));
                    }
                    catch (Exception) { }
                }
                if (element == null)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div[2]/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[3]/div[3]/div/div"));
                    }
                    catch (Exception) { }
                }
                if (element == null)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div[2]/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[3]/div[2]/div/div"));
                    }
                    catch (Exception) { }
                }
                if (element == null)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//div[@aria-label='Post']"));
                    }
                    catch (Exception) { }
                }
                if (element == null)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//*[text() = 'Post']"));
                    }
                    catch (Exception) { }
                }
                if (element != null)
                {
                    try
                    {
                        var disabled = element.GetAttribute("aria-disabled");
                        if (disabled != "true")
                        {
                            isWorking = true;
                        }
                    }
                    catch (Exception) { }
                }
                if (isWorking)
                {
                    try
                    {
                        element.Click();
                        Thread.Sleep(3000);
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool LeaveGroupInvite(IWebDriver driver)
        {
            bool isWorking = false;
            if (!isWorking)
            {
                try
                {

                    driver.FindElement(By.XPath("//div[@aria-label='Decline Invite']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Decline invite']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Decline Invitation']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Decline invitation']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Decline invitation']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (isWorking)
            {
                isWorking = false;
                int counter = 6;
                IWebElement element = null;
                do
                {
                    Thread.Sleep(1000);
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div[3]/div/div/div[2]/div[2]/div/div/div[1]"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div[3]/div/div/div[2]/div[2]/div/div/div[1]/div/span/span"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("//*[text() = 'Decline']"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
                if (element != null)
                {
                    ClickElement(driver, element);
                    Thread.Sleep(1000);
                }
            }

            return isWorking;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void LeaveGroup(IWebDriver driver, bool leavePendingOnly = false)
        {
            bool isWorking = false;
            int counter = 5;
            do
            {
                Thread.Sleep(1000);
                isWorking = LeaveGroupInvite(driver);
                if (leavePendingOnly)
                {
                    try
                    {
                        var source = driver.PageSource.ToLower().ToString();
                        if (source.Contains("pending") && source.Contains("manage posts"))
                        {

                        }
                        else
                        {
                            return;
                        }
                    }
                    catch (Exception) { }
                }
                IWebElement element = null;
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//div[@aria-label='Following']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//div[@aria-label='Joined']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[1]/div[2]/div/div/div/div/div[2]/div/div[2]/div/span/span[2]/div"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div[1]/div[1]/div/div[3]/div/div/div/div[1]/div[1]/div/div[3]/div/div/div[1]/div[2]/div/div/div/div/div[2]/div/div[1]"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div/div[1]/div[1]/div/div[3]/div/div/div[1]/div[2]/div/div/div/div/div[2]/div/div[1]/div/span/span[2]/div"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[2]/div/div/div[1]/div[2]/div/div/div/div/div[2]/div/div[1]/span/span[1]/div"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[5]/div/div/div[3]/div/div/div[1]/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/div/div[4]/div/div/div/div/div[1]/div/span/span/div"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//div[@aria-label='More']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//div[@aria-label='more']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[3]/div/div/div[2]/div[2]/div/div/div[2]/div"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[3]/div/div/div[2]/div[2]/div/div[2]/div"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (isWorking)
                {
                    ClickElement(driver, element);
                }
            } while (!isWorking && counter-- > 0);
            if (isWorking)
            {
                Thread.Sleep(1000);
                isWorking = false;
                counter = 10;
                do
                {
                    Thread.Sleep(500);
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Leave Group']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//span[contains(text(),'Leave group')]")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//span[contains(text(),'Unfollow group')]")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
                if (isWorking)
                {
                    Thread.Sleep(1000);
                    isWorking = false;
                    counter = 5;
                    do
                    {
                        Thread.Sleep(500);
                        IWebElement el = null;
                        try
                        {
                            el = driver.FindElement(By.XPath("//div[@aria-label='Leave Group']"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                        if (!isWorking)
                        {
                            try
                            {
                                el = driver.FindElement(By.XPath("//span[contains(text(),'Leave Group')]"));
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        try
                        {
                            el = driver.FindElement(By.XPath("//div[@aria-label='Leave group']"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                        if (!isWorking)
                        {
                            try
                            {
                                el = driver.FindElement(By.XPath("//span[contains(text(),'Leave group')]"));
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        try
                        {
                            el = driver.FindElement(By.XPath("//div[@aria-label='Unfollow group']"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                        if (!isWorking)
                        {
                            try
                            {
                                el = driver.FindElement(By.XPath("//span[contains(text(),'Unfollow group')]"));
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (isWorking)
                        {
                            ClickElement(driver, el);
                            Thread.Sleep(1000);
                        }
                    } while (!isWorking && counter-- > 0);
                }
            }
            Thread.Sleep(2000);
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool InviteFriendsToGroup(IWebDriver driver, int maxInvite = 10)
        {
            if (driver == null || maxInvite <= 0)
            {
                return false;
            }
            Func<IWebElement> findInviteDialog = () =>
            {
                try
                {
                    return driver.FindElements(By.XPath(
                        "//div[@role='dialog' and (" +
                        ".//*[contains(text(),'Invite friends to this group')]" +
                        " or .//*[contains(text(),'friends selected')]" +
                        " or .//input[contains(@placeholder,'Search for friends')]" +
                        ")]")).FirstOrDefault();
                }
                catch (Exception) { }
                return null;
            };

            Action<IWebElement> cancelInviteDialog = (dlg) =>
            {
                try
                {
                    if (dlg == null)
                    {
                        dlg = findInviteDialog();
                    }
                }
                catch (Exception) { }

                string[] cancelXPaths = new string[]
                {
                    ".//span[normalize-space()='Cancel']/ancestor::div[@role='button'][1]",
                    ".//button[normalize-space()='Cancel']",
                    ".//button[@aria-label='Cancel' or @aria-label='Close']",
                    ".//div[@role='button' and (@aria-label='Cancel' or @aria-label='Close')]",
                    ".//div[@aria-label='Close']",
                    ".//div[@aria-label='Cancel']",
                    ".//div[@role='button'][.//span[normalize-space()='Cancel']]",
                    ".//div[@role='button'][.//span[normalize-space()='×']]"
                };

                if (dlg != null)
                {
                    foreach (var xPath in cancelXPaths)
                    {
                        try
                        {
                            var btn = dlg.FindElements(By.XPath(xPath)).FirstOrDefault();
                            if (btn != null)
                            {
                                ClickElement(driver, btn);
                                Thread.Sleep(400);
                                try
                                {
                                    btn.Click();
                                    Thread.Sleep(200);
                                }
                                catch (Exception) { }
                                try { log.Info("[InviteFriendsToGroup] Cancel dialog clicked."); } catch (Exception) { }
                                return;
                            }
                        }
                        catch (Exception) { }
                    }
                }

                try
                {
                    var closeBtn = driver.FindElements(By.XPath(
                        "//div[@role='dialog']//div[@aria-label='Close']" +
                        " | //div[@role='dialog']//span[normalize-space()='Cancel']/ancestor::div[@role='button'][1]" +
                        " | //div[@role='dialog']//button[normalize-space()='Cancel']" +
                        " | //div[@role='dialog']//button[@aria-label='Close']")).FirstOrDefault();
                    if (closeBtn != null)
                    {
                        ClickElement(driver, closeBtn);
                        Thread.Sleep(400);
                        try
                        {
                            closeBtn.Click();
                            Thread.Sleep(200);
                        }
                        catch (Exception) { }
                        try { log.Info("[InviteFriendsToGroup] Cancel dialog clicked (global fallback)."); } catch (Exception) { }
                    }
                }
                catch (Exception) { }

                try
                {
                    new Actions(driver).SendKeys(OpenQA.Selenium.Keys.Escape).Perform();
                    Thread.Sleep(250);
                    log.Info("[InviteFriendsToGroup] Sent ESC to close invite dialog.");
                }
                catch (Exception) { }
            };

            // If account is not a member, join group first before opening invite dialog.
            try
            {
                if (IsJoinGroup(driver))
                {
                    JoinGroup(driver, "Okay");
                    Thread.Sleep(1200);
                    FBTool.WaitingPageLoading(driver);
                    Thread.Sleep(700);
                }
            }
            catch (Exception) { }

            var dialog = findInviteDialog();
            if (dialog == null)
            {
                int openCounter = 8;
                string[] openInviteXPaths = new string[]
                {
                    "//div[@aria-label='Invite friends']",
                    "//div[@aria-label='Invite']",
                    "//div[@role='button'][.//span[normalize-space()='Invite']]",
                    "//span[normalize-space()='Invite']/ancestor::div[@role='button'][1]",
                    "//div[@role='button'][.//span[contains(text(),'Invite friends')]]",
                    "//span[contains(text(),'Invite friends')]/ancestor::div[@role='button'][1]"
                };

                do
                {
                    Thread.Sleep(700);
                    foreach (var xPath in openInviteXPaths)
                    {
                        if (dialog != null)
                        {
                            break;
                        }
                        try
                        {
                            var element = driver.FindElements(By.XPath(xPath)).FirstOrDefault();
                            if (element == null)
                            {
                                continue;
                            }

                            ClickElement(driver, element);
                            Thread.Sleep(1200);
                            dialog = findInviteDialog();
                        }
                        catch (Exception) { }
                    }
                } while (dialog == null && openCounter-- > 0);
            }

            if (dialog == null)
            {
                return false;
            }

            try
            {
                var dialogText = (dialog.Text ?? "").ToLowerInvariant();
                if (dialogText.Contains("no friends to invite"))
                {
                    cancelInviteDialog(dialog);
                    return false;
                }
            }
            catch (Exception) { }

            int selectedCount = 0;
            int selectRound = 10;
            Func<IWebElement, int> getUiSelectedCount = (dlg) =>
            {
                if (dlg == null)
                {
                    return -1;
                }

                try
                {
                    var m = Regex.Match((dlg.Text ?? "").ToLowerInvariant(), @"(\d+)\s+friends?\s+selected");
                    if (m.Success)
                    {
                        return Convert.ToInt32(m.Groups[1].Value);
                    }
                }
                catch (Exception) { }

                return -1;
            };

            Func<IWebElement, bool> hasEnabledSendButton = (dlg) =>
            {
                if (dlg == null)
                {
                    return false;
                }

                try
                {
                    var send = dlg.FindElements(By.XPath(
                        ".//div[@role='button' and not(@aria-disabled='true') and ((@aria-label='Send invites' or @aria-label='Send invite') or .//span[contains(translate(normalize-space(.),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'send')])]" +
                        " | .//button[not(@disabled) and ((@aria-label='Send invites' or @aria-label='Send invite') or contains(translate(normalize-space(.),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'send') or .//span[contains(translate(normalize-space(.),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'send')])]"))
                        .FirstOrDefault();
                    return send != null;
                }
                catch (Exception) { }

                return false;
            };

            Func<IWebElement, bool> isInviteCheckboxChecked = (checkbox) =>
            {
                if (checkbox == null)
                {
                    return false;
                }

                try
                {
                    var ariaChecked = (checkbox.GetAttribute("aria-checked") ?? "").Trim();
                    if (ariaChecked.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                        ariaChecked.Equals("mixed", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                catch (Exception) { }

                try
                {
                    if (checkbox.Selected)
                    {
                        return true;
                    }
                }
                catch (Exception) { }

                try
                {
                    IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
                    var result = executor.ExecuteScript(
                        "var el=arguments[0];" +
                        "if(!el){return false;}" +
                        "if(el.getAttribute && (el.getAttribute('aria-checked')==='true' || el.getAttribute('aria-checked')==='mixed')){return true;}" +
                        "if(typeof el.checked !== 'undefined'){return !!el.checked;}" +
                        "return false;",
                        checkbox);
                    if (result is bool checkedState)
                    {
                        return checkedState;
                    }
                }
                catch (Exception) { }

                return false;
            };

            Func<IWebElement, bool> trySelectInviteCheckbox = (checkbox) =>
            {
                if (checkbox == null || isInviteCheckboxChecked(checkbox))
                {
                    return false;
                }

                var clickTargets = new List<IWebElement>();
                clickTargets.Add(checkbox);

                try
                {
                    var label = checkbox.FindElements(By.XPath("./ancestor::label[1]")).FirstOrDefault();
                    if (label != null)
                    {
                        clickTargets.Add(label);
                    }
                }
                catch (Exception) { }

                try
                {
                    var row = checkbox.FindElements(By.XPath("./ancestor::div[.//input[@type='checkbox'] or .//*[@role='checkbox']][1]")).FirstOrDefault();
                    if (row != null)
                    {
                        clickTargets.Add(row);
                    }
                }
                catch (Exception) { }

                foreach (var target in clickTargets)
                {
                    if (target == null)
                    {
                        continue;
                    }

                    try
                    {
                        ClickElement(driver, target);
                    }
                    catch (Exception) { }

                    Thread.Sleep(140);
                    if (isInviteCheckboxChecked(checkbox))
                    {
                        return true;
                    }

                    try
                    {
                        target.Click();
                    }
                    catch (Exception) { }

                    Thread.Sleep(140);
                    if (isInviteCheckboxChecked(checkbox))
                    {
                        return true;
                    }
                }

                return false;
            };

            do
            {
                Thread.Sleep(500);
                try
                {
                    var dialogText = (dialog.Text ?? "").ToLowerInvariant();
                    if (dialogText.Contains("no friends to invite"))
                    {
                        cancelInviteDialog(dialog);
                        return false;
                    }
                }
                catch (Exception) { }

                IReadOnlyCollection<IWebElement> checkboxElements = null;
                try
                {
                    checkboxElements = dialog.FindElements(By.XPath(
                        ".//div[@role='checkbox' and not(@aria-checked='true')]" +
                        " | .//input[@type='checkbox' and not(@disabled)]"));
                }
                catch (Exception) { }

                if (checkboxElements != null && checkboxElements.Count > 0)
                {
                    foreach (var checkbox in checkboxElements)
                    {
                        if (selectedCount >= maxInvite)
                        {
                            break;
                        }

                        try
                        {
                            bool shouldSkip = false;
                            var rowText = "";
                            try
                            {
                                var row = checkbox.FindElements(By.XPath("./ancestor::div[@role='row' or @role='listitem' or .//input[@type='checkbox'] or .//*[@role='checkbox']][1]")).FirstOrDefault();
                                rowText = (row?.Text ?? "");
                            }
                            catch (Exception) { }

                            var text = (((checkbox.GetAttribute("aria-label") ?? "") + " " + (checkbox.Text ?? "") + " " + rowText)).ToLowerInvariant();
                            if (text.Contains("all"))
                            {
                                shouldSkip = true;
                            }
                            if (text.Contains("already invited") || text.Contains("already a member"))
                            {
                                shouldSkip = true;
                            }
                            if (shouldSkip)
                            {
                                continue;
                            }

                            if (trySelectInviteCheckbox(checkbox))
                            {
                                selectedCount++;
                            }
                            Thread.Sleep(180);
                        }
                        catch (Exception) { }
                    }
                }

                if (selectedCount < maxInvite)
                {
                    try
                    {
                        IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
                        executor.ExecuteScript(
                            "var root=arguments[0];" +
                            "var nodes=root.querySelectorAll('div');" +
                            "for(var i=0;i<nodes.length;i++){" +
                            " if(nodes[i].scrollHeight>nodes[i].clientHeight+20){nodes[i].scrollTop=nodes[i].scrollTop+500;break;}" +
                            "}",
                            dialog);
                        Thread.Sleep(300);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            new Actions(driver).MoveToElement(dialog).Click().SendKeys(OpenQA.Selenium.Keys.PageDown).Perform();
                            Thread.Sleep(300);
                        }
                        catch (Exception) { }
                    }
                }
            } while (selectedCount < maxInvite && selectRound-- > 0);

            dialog = findInviteDialog() ?? dialog;
            int uiSelectedCount = getUiSelectedCount(dialog);
            bool canSend = hasEnabledSendButton(dialog);
            if (!canSend)
            {
                for (int i = 0; i < 6 && !canSend; i++)
                {
                    Thread.Sleep(220);
                    dialog = findInviteDialog() ?? dialog;
                    uiSelectedCount = getUiSelectedCount(dialog);
                    canSend = hasEnabledSendButton(dialog);
                }
            }

            try
            {
                log.Info("[InviteFriendsToGroup] Post-select check: selectedCount=" + selectedCount + ", uiSelectedCount=" + uiSelectedCount + ", canSend=" + canSend);
            }
            catch (Exception) { }

            if (selectedCount <= 0 || !canSend)
            {
                try
                {
                    log.Info("[InviteFriendsToGroup] No selectable friends or send disabled. Click Cancel.");
                }
                catch (Exception) { }
                cancelInviteDialog(dialog);
                return false;
            }

            bool isSent = false;
            bool attemptedSendClick = false;
            try
            {
                log.Info("[InviteFriendsToGroup] Selected friends count: " + selectedCount + ". Trying to click Send invites.");
            }
            catch (Exception) { }

            string[] sendButtonXPaths = new string[]
            {
                ".//div[@role='button' and (@aria-label='Send invites' or @aria-label='Send invite') and not(@aria-disabled='true')]",
                ".//span[normalize-space()='Send invites']/ancestor::div[@role='button'][1]",
                ".//span[contains(text(),'Send invite')]/ancestor::div[@role='button'][1]",
                ".//div[@role='button' and not(@aria-disabled='true') and .//span[contains(text(),'Send')]]",
                ".//button[not(@disabled) and (@aria-label='Send invites' or @aria-label='Send invite')]",
                ".//button[not(@disabled)][.//span[normalize-space()='Send invites'] or .//span[contains(text(),'Send invite')]]",
                ".//button[not(@disabled)][contains(translate(normalize-space(.),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'send')]"
            };

            int sendRound = 12;
            do
            {
                if (isSent)
                {
                    break;
                }

                Thread.Sleep(350);

                var currentDialog = findInviteDialog();
                if (currentDialog == null)
                {
                    if (attemptedSendClick)
                    {
                        isSent = true;
                        try { log.Info("[InviteFriendsToGroup] Dialog closed after send click attempt."); } catch (Exception) { }
                    }
                    else
                    {
                        try { log.Warn("[InviteFriendsToGroup] Invite dialog disappeared before any send click attempt."); } catch (Exception) { }
                    }
                    break;
                }

                for (int i = 0; i < sendButtonXPaths.Length; i++)
                {
                    if (isSent)
                    {
                        break;
                    }

                    try
                    {
                        var xPath = sendButtonXPaths[i];
                        var sendBtn = currentDialog.FindElements(By.XPath(xPath)).FirstOrDefault();
                        if (sendBtn == null)
                        {
                            continue;
                        }

                        try
                        {
                            var sendText = (sendBtn.Text ?? "").Trim();
                            var ariaLabel = (sendBtn.GetAttribute("aria-label") ?? "").Trim();
                            log.Info("[InviteFriendsToGroup] Found send button via xpath #" + (i + 1) + " | text='" + sendText + "' | aria-label='" + ariaLabel + "'");
                        }
                        catch (Exception) { }

                        try
                        {
                            ClickElement(driver, sendBtn);
                            attemptedSendClick = true;
                            log.Info("[InviteFriendsToGroup] Clicked send button via JS click.");
                        }
                        catch (Exception) { }

                        Thread.Sleep(250);
                        if (findInviteDialog() == null)
                        {
                            try { log.Info("[InviteFriendsToGroup] Send invite success: dialog closed after JS click."); } catch (Exception) { }
                            isSent = true;
                            break;
                        }

                        try
                        {
                            sendBtn.Click();
                            attemptedSendClick = true;
                            log.Info("[InviteFriendsToGroup] Clicked send button via native click.");
                        }
                        catch (Exception) { }

                        Thread.Sleep(250);
                        if (findInviteDialog() == null)
                        {
                            try { log.Info("[InviteFriendsToGroup] Send invite success: dialog closed after native click."); } catch (Exception) { }
                            isSent = true;
                            break;
                        }
                    }
                    catch (Exception) { }
                }
            } while (!isSent && sendRound-- > 0);

            if (!isSent)
            {
                try
                {
                    var currentDialog = findInviteDialog();
                    if (currentDialog != null)
                    {
                        var footerButtons = currentDialog.FindElements(By.XPath(".//button[not(@disabled)] | .//div[@role='button' and not(@aria-disabled='true')]"));
                        foreach (var btn in footerButtons.Reverse())
                        {
                            var t = (btn.Text ?? "").Trim().ToLowerInvariant();
                            if (string.IsNullOrEmpty(t) || t.Contains("cancel") || t.Contains("close"))
                            {
                                continue;
                            }

                            try
                            {
                                ClickElement(driver, btn);
                                attemptedSendClick = true;
                                log.Info("[InviteFriendsToGroup] Fallback footer click via JS.");
                            }
                            catch (Exception) { }
                            Thread.Sleep(250);
                            if (findInviteDialog() == null)
                            {
                                try { log.Info("[InviteFriendsToGroup] Send invite success: dialog closed after fallback JS click."); } catch (Exception) { }
                                isSent = true;
                                break;
                            }

                            try
                            {
                                btn.Click();
                                attemptedSendClick = true;
                                log.Info("[InviteFriendsToGroup] Fallback footer click via native click.");
                            }
                            catch (Exception) { }
                            Thread.Sleep(250);
                            if (findInviteDialog() == null)
                            {
                                try { log.Info("[InviteFriendsToGroup] Send invite success: dialog closed after fallback native click."); } catch (Exception) { }
                                isSent = true;
                                break;
                            }
                        }
                    }
                }
                catch (Exception) { }
            }

            if (isSent)
            {
                try
                {
                    log.Info("[InviteFriendsToGroup] Waiting 3 seconds after Send invite.");
                }
                catch (Exception) { }
                Thread.Sleep(3000);
            }

            try
            {
                if (isSent)
                {
                    log.Info("[InviteFriendsToGroup] Send invite flow completed.");
                }
                else
                {
                    log.Warn("[InviteFriendsToGroup] Send invite flow failed: dialog still open or send button not clicked.");
                }
            }
            catch (Exception) { }

            return isSent;
        }
        public static void ClickElement(IWebDriver driver, IWebElement element)
        {
            try
            {
                IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
                executor.ExecuteScript("arguments[0].click();", element);
            }
            catch (Exception) { }
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void LeaveGroupII(IWebDriver driver)
        {
            bool isWorking = false;
            int counter = 0;
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/groups/feed/");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            CloseAllPopup(driver);

            try
            {
                int w = driver.Manage().Window.Size.Width;
                int h = driver.Manage().Window.Size.Height;
                driver.Manage().Window.Size = new System.Drawing.Size(w, h + 150);
            }
            catch (Exception) { }

            //isWorking = false;
            //counter = 4;
            //do
            //{
            //    Thread.Sleep(500);
            //    try
            //    {
            //        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[1]/div/div[1]/div/div/div[1]/div/div/span/h1")).Click();
            //        isWorking = true;
            //    }
            //    catch (Exception) { }
            //} while (!isWorking && counter-- > 0);

            isWorking = false;
            counter = 6;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Edit Groups Settings']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Edit Groups settings']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Edit groups settings']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//html/body/div[1]/div/div[1]/div/div[3]/div/div/div/div[1]/div[1]/div[1]/div/div[1]/div/div/div[2]/div/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);

            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[1]/div/div[1]/div/div/div[2]/div/div")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (isWorking)
            {
                isWorking = false;
                counter = 5;
                do
                {
                    Thread.Sleep(1000);
                    try
                    {
                        driver.FindElement(By.XPath("//span[text() = 'Membership']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div[1]/div[1]/div/div/div/div/div/div/div[1]/div[8]/div[2]/div/div[1]/span")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div[1]/div[1]/div/div/div/div/div/div/div[1]/div[8]/div[2]/div/div[1]/span")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            }
            if (isWorking)
            {
                string membership_path = "/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div[3]/div[3]/div[1]/div";
                string membership_path1 = "/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div";
                string membership_path2 = "/html/body/div[1]/div/div[1]/div/div[6]/div/div/div[1]/div/div[2]/div/div/div";
                string membership_path3 = "/html/body/div[1]/div/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div[3]/div[2]/div[1]/div[1]";

                isWorking = false;
                counter = 4;
                do
                {
                    Thread.Sleep(1000);
                    try
                    {
                        driver.FindElement(By.XPath(membership_path));
                        isWorking = true;
                    }
                    catch (Exception) { }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(membership_path1));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(membership_path2));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(membership_path3));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);

                if (isWorking)
                {
                    int countErrorLeaveGroup = 0;
                    counter = 30;
                    do
                    {
                        IReadOnlyCollection<IWebElement> elements = null;
                        try
                        {
                            elements = driver.FindElements(By.XPath("//div[@aria-label='Leave']"));
                        }
                        catch (Exception) { }
                        if (elements != null)
                        {
                            foreach (IWebElement element in elements)
                            {
                                try
                                {
                                    element.Click();
                                    Thread.Sleep(3000);
                                }
                                catch (Exception) { }
                                bool isLeave = false;
                                try
                                {
                                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div[2]/div[1]/div/div[2]/div/div/div/div[3]/div/div[3]/div/div[2]/div")).Click();
                                    countErrorLeaveGroup = 0;
                                    isLeave = true;
                                    Thread.Sleep(1000);
                                }
                                catch (Exception) { }
                                if (!isLeave)
                                {
                                    try
                                    {
                                        driver.FindElement(By.XPath("//div[@aria-label='Leave group']")).Click();
                                        isLeave = true;
                                    }
                                    catch (Exception) { }
                                }
                                if (!isLeave)
                                {
                                    try
                                    {
                                        driver.FindElement(By.XPath("//div[@aria-label='Leave Group']")).Click();
                                        isLeave = true;
                                    }
                                    catch (Exception) { }
                                }
                                if (!isLeave)
                                {
                                    try
                                    {
                                        driver.FindElement(By.XPath("//div[@aria-label='Unfollow group']")).Click();
                                        isLeave = true;
                                    }
                                    catch (Exception) { }
                                }
                                if (!isLeave)
                                {
                                    try
                                    {
                                        driver.FindElement(By.XPath("//div[@aria-label='Unfollow Group']")).Click();
                                        isLeave = true;
                                    }
                                    catch (Exception) { }
                                }
                                if (!isLeave)
                                {
                                    try
                                    {
                                        driver.FindElement(By.XPath("/html/body/div[5]/div[1]/div/div[2]/div/div/div/div[3]/div/div[3]/div/div[2]/div")).Click();
                                        isLeave = true;
                                    }
                                    catch (Exception) { }
                                }
                                if (!isLeave)
                                {
                                    countErrorLeaveGroup++;
                                }
                                else
                                {
                                    Thread.Sleep(2000);
                                    // new update
                                    // find support or report group
                                    var source = driver.PageSource.ToLower().ToString();
                                    if (source.Contains("find support or report group"))
                                    {
                                        bool isClose = false;
                                        if (!isClose)
                                        {
                                            try
                                            {
                                                driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[4]/div/div[3]/div[2]/div/div/div[1]/div/div[2]/div/div/div/div[2]/div")).Click();
                                                isClose = true;
                                            }
                                            catch (Exception) { }
                                        }
                                        if (!isClose)
                                        {
                                            try
                                            {
                                                driver.FindElement(By.XPath("//div[@aria-label='Close']")).Click();
                                                isClose = true;
                                            }
                                            catch (Exception) { }
                                        }
                                        if (!isClose)
                                        {
                                            try
                                            {
                                                driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div[3]/div[2]/div/div/div[1]/div/div[2]/div/div/div/div[2]/div")).Click();
                                                isClose = true;
                                            }
                                            catch (Exception) { }
                                        }
                                        Thread.Sleep(2000);
                                    }
                                }
                            }
                        }
                        else
                        {
                            isWorking = false;
                        }
                        if (countErrorLeaveGroup >= 5)
                        {
                            isWorking = false;
                        }
                    } while (isWorking && counter-- > 0);
                }
            }
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool PostComment(IWebDriver driver, string comment)
        {
            int counter = 4;
            bool isWorking = false, isComment = false;
            IReadOnlyCollection<IWebElement> elements = null;
            do
            {
                Thread.Sleep(1000);

                if (!isComment && !isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Leave a comment' or @aria-label='Comment' or @aria-label='Bình luận' or @aria-label='បញ្ចេញមតិ']")).Click();
                        isComment = true;
                        Thread.Sleep(2500);
                    }
                    catch (Exception) { }
                    if (!isComment)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//div[@aria-label='Write a public comment…']")).Click();
                            isComment = true;
                            Thread.Sleep(2500);
                        }
                        catch (Exception) { }
                    }
                }
                if (!isWorking)
                {
                    try
                    {
                        elements = driver.FindElements(By.XPath("//div[@data-lexical-editor='true']"));
                        isWorking = elements != null && elements.Count > 0;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        elements = driver.FindElements(By.XPath(
                            "//div[@aria-label='Write a comment' or @aria-label='Write a comment…' or @role='textbox']" +
                            " | //div[@contenteditable='true' and (@role='textbox' or @data-lexical-editor='true')]"));
                        isWorking = elements != null && elements.Count > 0;
                    }
                    catch (Exception) { }
                }
                if (!isComment)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[text() = 'Comment' or text() = 'Bình luận' or text() = 'បញ្ចេញមតិ']")).Click();
                        isComment = true;
                        Thread.Sleep(1000);
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (elements != null)
            {
                isWorking = false;
                try
                {
                    if (elements != null && elements.Count > 0)
                    {
                        var target = elements.ElementAt(elements.Count - 1);
                        try { target.Click(); Thread.Sleep(300); } catch (Exception) { }

                        // Prefer direct send to editor; many layouts do not expose a <br> child reliably.
                        try
                        {
                            target.SendKeys(comment + OpenQA.Selenium.Keys.Enter);
                            isWorking = true;
                        }
                        catch (Exception) { }

                        if (!isWorking)
                        {
                            try
                            {
                                var br = target.FindElement(By.TagName("br"));
                                br.SendKeys(comment + OpenQA.Selenium.Keys.Enter);
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }

                        if (!isWorking)
                        {
                            try
                            {
                                new Actions(driver)
                                    .SendKeys(target, comment)
                                    .SendKeys(OpenQA.Selenium.Keys.Enter)
                                    .Build()
                                    .Perform();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                    }
                }
                catch (Exception) { }
                //if(!isWorking)
                //{
                //    try
                //    {
                //        elements.ElementAt(elements.Count - 1).FindElement(By.TagName("br")).SendKeys(comment + OpenQA.Selenium.Keys.Enter);
                //        isWorking = true;
                //    }
                //    catch (Exception) { }
                //}
                if (isWorking)
                {
                    Thread.Sleep(2000);
                }
            }

            return isWorking;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void ChangeLanguage(IWebDriver driver, bool isCheckCleanBrowser = true)
        {
            int counter = 3;
            bool isWorking = false;
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/settings?tab=language&section=account&view");
            }
            catch (Exception) { }

            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);

            if (isCheckCleanBrowser)
            {
                Thread.Sleep(1000);
                CloseAllPopup(driver);
            }
            else
            {
                counter = 4;
                do
                {
                    try
                    {
                        Thread.Sleep(500);
                        isWorking = StepCloseIntroductionAfterLogin(driver);
                    }
                    catch (Exception) { }
                } while (!isWorking && counter-- > 0);
            }
            counter = 4;
            isWorking = false;
            do
            {
                Thread.Sleep(500);
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[2]/div/div/div/div/div[2]/div/div/div[1]/div[2]/div/div[3]/div")).Click();
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[2]/div/div/div/div/div/div/div[2]/div/div/div[2]/div/div/div/div[1]/div/div/div/div/div/div[2]/div/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking) { return; }

            counter = 6;
            isWorking = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-haspopup='listbox']")).Click();
                }
                catch (Exception) { }
                try
                {
                    driver.FindElement(By.XPath("//span[text() = 'English (US)']")).Click();
                    Thread.Sleep(1000);
                    isWorking = true;
                }
                catch (Exception) { }
                if (isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[4]/div/div[2]/div[1]/div/div[2]/div/div/div/div/div/div/div[3]/div/div/div/div/div[1]/div")).Click();
                    }
                    catch (Exception) { }
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[2]/div/div/div/div/div[2]/div/div/div[1]/div[2]/div/div/div[3]/div/div[2]/div[1]")).Click();
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (isWorking)
            {
                try
                {
                    Thread.Sleep(2000);
                }
                catch (Exception) { }
            }
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void ChangeLanguage_____Backup(IWebDriver driver, bool isCheckCleanBrowser = true)
        {
            int counter = 3;
            bool isWorking = false;
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/settings?tab=language&section=account&view");
            }
            catch (Exception) { }

            FBTool.WaitingPageLoading(driver);

            if (isCheckCleanBrowser)
            {
                Thread.Sleep(1000);
                CloseAllPopup(driver);
            }
            else
            {
                counter = 3;
                do
                {
                    try
                    {
                        Thread.Sleep(1000);
                        isWorking = StepCloseIntroductionAfterLogin(driver);
                    }
                    catch (Exception) { }
                } while (!isWorking && counter-- > 0);
            }
            try
            {
                driver.SwitchTo().Frame(0);
            }
            catch (Exception) { }
            try
            {
                Thread.Sleep(3000);
                driver.FindElement(By.XPath("//select[@name='new_language']")).Click();
                Thread.Sleep(1500);
                driver.FindElement(By.XPath("//option[@value='en_US']")).Click();
                Thread.Sleep(1000);
                driver.FindElement(By.XPath("/html/body/div[1]/div[1]/div[1]/div/div[2]/div[2]/div[2]/div/ul/li[1]/div/div/ul/li/div/div/form/div/div[2]/div/label[1]/input")).Click();
            }
            catch (Exception) { }
            try
            {
                driver.SwitchTo().DefaultContent();
                Thread.Sleep(2000);
            }
            catch (Exception) { }
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool LikePage(IWebDriver driver, string url)
        {
            bool isWorking = false;
            int counter = 5;
            do
            {
                Thread.Sleep(1000);
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div/div[3]/div/div/div/div[2]/div/div/div/div[1]/div/div")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Follow']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[2]/div/div/div[3]/div/div/div/div[2]/div/div/div[1]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Like']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (isWorking)
            {
                Thread.Sleep(2000);
            }

            return isWorking;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void LikePost(IWebDriver driver)
        {
            try
            {
                driver.FindElement(By.XPath("//div[@aria-label='Like' or @aria-label='Thích' or @aria-label='ចូលចិត្ត']")).Click();
                Thread.Sleep(1000);
            }
            catch (Exception) { }
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool LikeLastedPost(IWebDriver driver)
        {
            bool isWorking = false;
            IReadOnlyCollection<IWebElement> elements = null;
            try
            {
                elements = driver.FindElements(By.XPath("//div[@aria-label='Like']"));
                isWorking = true;
            }
            catch (Exception) { }
            if (elements != null)
            {
                try
                {
                    elements.ElementAt(elements.Count - 1).Click();
                    isWorking = true;
                    Thread.Sleep(1000);
                }
                catch (Exception) { }
            }

            return isWorking;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static IWebDriver LoginYandex(IWebDriver driver, string email, string pass)
        {
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("window.open();");
                driver.SwitchTo().Window(driver.WindowHandles.Last());
                driver.Navigate().GoToUrl("https://passport.yandex.com/auth/welcome?from=mail&origin=hostroot_homer_auth_L_com&retpath=https%3A%2F%2Fmail.yandex.com%2F&backpath=https%3A%2F%2Fmail.yandex.com%3Fnoretpath%3D1");
            }
            catch (Exception) { }
            try
            {
                Thread.Sleep(2500);
                driver.FindElement(By.Id("passp-field-login")).SendKeys(email + OpenQA.Selenium.Keys.Enter);
            }
            catch (Exception) { }
            try
            {
                Thread.Sleep(2500);
                driver.FindElement(By.Id("passp-field-passwd")).SendKeys(pass + OpenQA.Selenium.Keys.Enter);
            }
            catch (Exception) { }

            return driver;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static string GetVerifyPrimaryContactFromYandex(IWebDriver driver, string email = "", string protocol = "", bool isFirst = true)
        {
            string code = "";
            bool isWorking;
            int counter;

            var source1 = "";
            try
            {
                source1 = driver.PageSource.ToLower();
            }
            catch (Exception) { }
            if (source1.Contains("robot are"))
            {
                try
                {
                    driver.FindElement(By.Id("js-button")).Click();
                    Thread.Sleep(2500);
                }
                catch (Exception) { }
            }
            if (!string.IsNullOrEmpty(protocol))
            {
                try
                {
                    driver.Navigate().GoToUrl(protocol);
                }
                catch (Exception) { }
                if (!isFirst)
                {
                    FBTool.WaitingPageLoading(driver);
                    Thread.Sleep(1000);
                    try
                    {
                        driver.Navigate().Refresh();
                        Thread.Sleep(1000);
                    }
                    catch (Exception) { }
                }
            }
            else
            {
                try
                {
                    driver.FindElement(By.XPath("//a[@data-title='Social']")).Click();
                }
                catch (Exception) { }
                try
                {
                    driver.Navigate().Refresh();
                }
                catch (Exception) { }
            }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);

            isWorking = false;
            counter = 10;
            do
            {
                Thread.Sleep(600);
                IWebElement element = null;
                try
                {
                    element = driver.FindElement(By.XPath("//span[text() = 'Facebook']"));
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//span[text() = 'Confirm email']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//span[text() = 'Confirm Email']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.PartialLinkText("New email address added on Facebook"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//span[text() = 'New email address added on Facebook']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//span[text() = 'Confirm your new email address']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (isWorking)
                {
                    try
                    {
                        Actions actions = new Actions(driver);
                        actions.MoveToElement(element).Click().Perform();
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return "";
            }
            Thread.Sleep(2000);

            isWorking = false;
            counter = 7;
            do
            {
                Thread.Sleep(1000);
                try
                {
                    //driver.FindElement(By.LinkText(email));
                    driver.FindElement(By.XPath("//a[@href='mailto:" + email.ToString().ToLower() + "']"));
                    isWorking = true;
                }
                catch (Exception) { }
                if (isWorking)
                {
                    var source = "";
                    try
                    {
                        source = driver.PageSource.ToLower();
                    }
                    catch (Exception) { }
                    if (!source.Contains("confirm email"))
                    {
                        isWorking = false;
                    }
                }
                if (!isWorking)
                {
                    bool isNext = false;
                    IWebElement element = null;
                    try
                    {
                        element = driver.FindElement(By.XPath("//div[text() = 'next']"));
                        isNext = true;
                    }
                    catch (Exception) { }
                    if (!isNext)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("//*[@href='#message/*']"));
                            isNext = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isNext)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("//div[text() = 'Next']"));
                            isNext = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isNext)
                    {
                        try
                        {
                            element = driver.FindElement(By.ClassName("PrevNext__label--iNM4a"));
                            isNext = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isNext)
                    {
                        try
                        {
                            element = driver.FindElement(By.ClassName("PrevNext__label--2fGPJ"));
                            isNext = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isNext)
                    {
                        try
                        {
                            element = driver.FindElement(By.Id("arrow"));
                            isNext = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isNext)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("//div[contains(text(),'Next email')]"));
                            isNext = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isNext)
                    {
                        try
                        {
                            element = driver.FindElement(By.ClassName("mail-Message-PrevNext-DirectionTitle"));
                            isNext = true;
                        }
                        catch (Exception) { }
                    }
                    if (isNext)
                    {
                        try
                        {
                            Actions actions = new Actions(driver);
                            actions.MoveToElement(element).Click().Perform();
                        }
                        catch (Exception) { }
                    }
                }
            } while (!isWorking && counter-- > 0);

            if (isWorking)
            {
                Thread.Sleep(1500);
                IWebElement element;
                string textCode = "";
                try
                {
                    element = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div[7]/div/div[3]/div[3]/div[3]/div/div[5]/div[1]/div/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                    textCode = element.GetAttribute("innerHTML");
                }
                catch (Exception) { }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[3]/div[2]/div[7]/div/div[3]/div/div[2]/div/main/div[7]/div/div/div/div/div/div[2]/div/article/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div[7]/div/div[3]/div[2]/div[3]/main/div[5]/div/div/div/div/div/div[2]/div/article/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div[7]/div/div[3]/div[3]/div[3]/div/div[5]/div/div[1]/div/div/div[1]/div[5]/div/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div[8]/div/div[3]/div[3]/div[3]/div/div[5]/div/div[1]/div/div/div[1]/div[5]/div/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[7]/div/div[3]/div[3]/div[3]/div/div[5]/div/div[1]/div/div[1]/div[5]/div/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div[8]/div/div[3]/div[3]/div[3]/div/div[5]/div[1]/div/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[7]/div/div[3]/div[3]/div[3]/div/div[5]/div[1]/div/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[7]/div/div[3]/div[3]/div[2]/div[5]/div/div[1]/div/div[1]/div[5]/div/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[6]/div/div[3]/div[3]/div[2]/div[5]/div[1]/div/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[7]/div/div[3]/div[3]/div[2]/div/div[5]/div/div[1]/div/div[1]/div[5]/div/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[7]/div/div[3]/div[3]/div[2]/div/div[5]/div[1]/div/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[3]/div[2]/div[7]/div/div[2]/div/div[2]/div/main/div[7]/div/div/div/div/div/div[2]/div/article/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[3]/div[2]/div[7]/div/div[2]/div/div[2]/div/main/div[8]/div/div/div/div/div/div/div[2]/div/article/div/table/tbody/tr/td/table[2]/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span/center"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[3]/div[2]/div[7]/div/div[2]/div/div[2]/div/main/div[9]/div/div/div/div/div/div/div[2]/div/article/div/table/tbody/tr/td/table[2]/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span/center"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[3]/div[2]/div[7]/div/div[2]/div/div[2]/div/main/div[9]/div/div/div/div/div/div/div[2]/div/article/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[4]/td/span/div/table/tbody/tr/td/span/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (!string.IsNullOrEmpty(textCode))
                {
                    //You may be asked to enter this confirmation code: xxxxxs
                    if (textCode.Length <= 6)
                    {
                        code = textCode;
                    }
                    else
                    {
                        code = textCode.Substring(50, 5);
                    }
                }
                else
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[5]/div/div[3]/div[3]/div[2]/div[5]/div[1]/div/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }

                    if (!string.IsNullOrEmpty(textCode))
                    {
                        //You may be asked to enter this confirmation code: xxxxx
                        code = textCode.Substring(50, 5);
                    }
                    else
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div[2]/div[7]/div/div[3]/div[3]/div[2]/div[5]/div[1]/div/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                            textCode = element.GetAttribute("innerHTML");
                        }
                        catch (Exception) { }
                        if (!string.IsNullOrEmpty(textCode))
                        {
                            //You may be asked to enter this confirmation code: xxxxx
                            code = textCode.Substring(50, 5);
                        }
                    }
                }
                if (string.IsNullOrEmpty(code))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[3]/div[2]/div[7]/div/div[2]/div/div[2]/div/main/div[7]/div/div/div/div/div/div[2]/div/article/div/table/tbody/tr/td/table[2]/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span/center"));
                        code = element.GetAttribute("innerHTML").Trim();
                    }
                    catch (Exception) { }
                }
            }

            return code;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static string GetVerifyPrimaryContactFromYandex_bbdsfdf(IWebDriver driver, string email = "", string protocol = "")
        {
            string code = "";
            bool isWorking;
            int counter;

            if (!string.IsNullOrEmpty(protocol))
            {
                try
                {
                    driver.Navigate().GoToUrl(protocol);
                }
                catch (Exception) { }
            }
            else
            {
                try
                {
                    driver.FindElement(By.XPath("//a[@data-title='Social']")).Click();
                }
                catch (Exception) { }
                try
                {
                    driver.Navigate().Refresh();
                }
                catch (Exception) { }
            }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);

            isWorking = false;
            counter = 10;
            do
            {
                Thread.Sleep(600);
                IWebElement element = null;
                try
                {
                    element = driver.FindElement(By.PartialLinkText("New email address added on Facebook"));
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//span[text() = 'New email address added on Facebook']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (isWorking)
                {
                    try
                    {
                        Actions actions = new Actions(driver);
                        actions.MoveToElement(element).Click().Perform();
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return "";
            }
            Thread.Sleep(2000);

            isWorking = false;
            counter = 7;
            do
            {
                Thread.Sleep(1000);
                try
                {
                    //driver.FindElement(By.LinkText(email));
                    driver.FindElement(By.XPath("//a[@href='mailto:" + email.ToString().ToLower() + "']"));
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    bool isNext = false;
                    IWebElement element = null;
                    try
                    {
                        element = driver.FindElement(By.XPath("//div[text() = 'next']"));
                        isNext = true;
                    }
                    catch (Exception) { }
                    if (!isNext)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("//*[@href='#message/*']"));
                            isNext = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isNext)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("//div[text() = 'Next']"));
                            isNext = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isNext)
                    {
                        try
                        {
                            element = driver.FindElement(By.ClassName("PrevNext__label--iNM4a"));
                            isNext = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isNext)
                    {
                        try
                        {
                            element = driver.FindElement(By.ClassName("PrevNext__label--2fGPJ"));
                            isNext = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isNext)
                    {
                        try
                        {
                            element = driver.FindElement(By.Id("arrow"));
                            isNext = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isNext)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("//div[contains(text(),'Next email')]"));
                            isNext = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isNext)
                    {
                        try
                        {
                            element = driver.FindElement(By.ClassName("mail-Message-PrevNext-DirectionTitle"));
                            isNext = true;
                        }
                        catch (Exception) { }
                    }
                    if (isNext)
                    {
                        try
                        {
                            Actions actions = new Actions(driver);
                            actions.MoveToElement(element).Click().Perform();
                        }
                        catch (Exception) { }
                    }
                }
            } while (!isWorking && counter-- > 0);

            if (isWorking)
            {
                Thread.Sleep(1500);
                IWebElement element;
                string textCode = "";
                try
                {
                    element = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div[7]/div/div[3]/div[3]/div[3]/div/div[5]/div[1]/div/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                    textCode = element.GetAttribute("innerHTML");
                }
                catch (Exception) { }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div[7]/div/div[3]/div[2]/div[3]/main/div[5]/div/div/div/div/div/div[2]/div/article/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div[7]/div/div[3]/div[3]/div[3]/div/div[5]/div/div[1]/div/div/div[1]/div[5]/div/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div[8]/div/div[3]/div[3]/div[3]/div/div[5]/div/div[1]/div/div/div[1]/div[5]/div/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[7]/div/div[3]/div[3]/div[3]/div/div[5]/div/div[1]/div/div[1]/div[5]/div/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div[8]/div/div[3]/div[3]/div[3]/div/div[5]/div[1]/div/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[7]/div/div[3]/div[3]/div[3]/div/div[5]/div[1]/div/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[7]/div/div[3]/div[3]/div[2]/div[5]/div/div[1]/div/div[1]/div[5]/div/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[6]/div/div[3]/div[3]/div[2]/div[5]/div[1]/div/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[7]/div/div[3]/div[3]/div[2]/div/div[5]/div/div[1]/div/div[1]/div[5]/div/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(textCode))
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[7]/div/div[3]/div[3]/div[2]/div/div[5]/div[1]/div/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (!string.IsNullOrEmpty(textCode))
                {
                    //You may be asked to enter this confirmation code: xxxxxs
                    code = textCode.Substring(50, 5);
                }
                else
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[2]/div[5]/div/div[3]/div[3]/div[2]/div[5]/div[1]/div/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        textCode = element.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }

                    if (!string.IsNullOrEmpty(textCode))
                    {
                        //You may be asked to enter this confirmation code: xxxxx
                        code = textCode.Substring(50, 5);
                    }
                    else
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div[2]/div[7]/div/div[3]/div[3]/div[2]/div[5]/div[1]/div/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                            textCode = element.GetAttribute("innerHTML");
                        }
                        catch (Exception) { }
                        if (!string.IsNullOrEmpty(textCode))
                        {
                            //You may be asked to enter this confirmation code: xxxxx
                            code = textCode.Substring(50, 5);
                        }
                    }
                }
            }

            return code;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static string GetPageAsset(IWebDriver driver, string page_id)
        {
            string asset_id = "";
            string strAsset = "delegate_page_id@:@";
            try
            {
                driver.Navigate().GoToUrl("view-source:https://www.facebook.com/profile.php?id=" + page_id);
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);

            var element = driver.FindElement(By.TagName("body"));
            var html_str = element.GetAttribute("innerHTML");

            if (!string.IsNullOrEmpty(html_str))
            {
                html_str = html_str.Replace('"', '@');
                string[] arr = Regex.Split(html_str, strAsset);

                if (arr.Length > 1)
                {
                    try
                    {
                        //Console.WriteLine(arr[1]);
                        string[] a = Regex.Split(arr[1], "@", RegexOptions.Singleline);
                        if (a.Length > 0)
                        {
                            asset_id = a[0].Trim();
                        }
                    }
                    catch (Exception) { }
                }
            }

            return asset_id;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static string GetToken(IWebDriver driver, bool isPublishedPost = false)
        {
            string token = "";
            string strAccessToken = "accessToken@:@EAA";
            try
            {
                if (isPublishedPost)
                {
                    driver.Navigate().GoToUrl("https://www.facebook.com/adsmanager/manage/adsets/edit");
                    strAccessToken = "accessToken=@EAA";
                }
                else
                {
                    driver.Navigate().GoToUrl("https://business.facebook.com/creatorstudio/home?reference=mbs_opt_out");
                }
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);

            try
            {
                string url = driver.Url;
                driver.Navigate().GoToUrl("view-source:" + url);
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);

            var element = driver.FindElement(By.TagName("body"));
            var html_str = element.GetAttribute("innerHTML");

            if (!string.IsNullOrEmpty(html_str))
            {
                html_str = html_str.Replace('"', '@');
                string[] arr = Regex.Split(html_str, strAccessToken);

                if (arr.Length > 1)
                {
                    try
                    {
                        //Console.WriteLine(arr[1]);
                        string[] a = Regex.Split(arr[1], "@", RegexOptions.Singleline);
                        if (a.Length > 0)
                        {
                            token = "EAA" + a[0];
                        }
                    }
                    catch (Exception) { }
                }
            }

            return token;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static string GetTokenLocator(IWebDriver driver, string twofa = "")
        {
            string token = "";
            if (!string.IsNullOrEmpty(twofa))
            {
                LoginTokenLocator(driver, twofa);
            }
            try
            {
                driver.Navigate().GoToUrl("view-source:" +
                    "https://business.facebook.com/business_locations");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(2000);
            try
            {
                var element = driver.FindElement(By.TagName("body"));
                var html_str = element.GetAttribute("innerHTML");

                if (!string.IsNullOrEmpty(html_str))
                {
                    html_str = html_str.Replace('"', '@');
                    token = GetStringBetween(html_str, "@],[@EAA", "@,@");
                    if (!string.IsNullOrEmpty(token))
                    {
                        token = "EAA" + token;
                    }
                }
            }
            catch (Exception) { }

            return token;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void LoginTokenLocator(IWebDriver driver, string twofa)
        {
            try
            {
                driver.Navigate().GoToUrl("https://business.facebook.com/business_locations");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);

            bool isWorking = false;
            int counter = 4;

            do
            {
                Thread.Sleep(1000);
                try
                {
                    driver.FindElement(By.XPath("//input[@placeholder='Enter code']")).SendKeys(TwoFactorRequest.GetPassCode(twofa) + OpenQA.Selenium.Keys.Enter);
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div[1]/div/div[2]/div/div/div/div/div/div[2]/div[1]/div[4]/span/span/div/div[2]/div/div/div/div[1]/div[2]/div/div/input")).SendKeys(TwoFactorRequest.GetPassCode(twofa) + OpenQA.Selenium.Keys.Enter);
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (isWorking)
            {
                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(3000);
            }
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static string GetStringBetween(string text, string start, string end)
        {
            int p1 = text.IndexOf(start) + start.Length;
            int p2 = text.IndexOf(end, p1);

            if (end == "")
            {
                return (text.Substring(p1));
            }
            else
            {
                return text.Substring(p1, p2 - p1);
            }

            return "";
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static void CreatePage(IWebDriver driver, string pageName, string categories, string bio = "")
        {
            IWebElement FindVisibleElement(params string[] xpaths)
            {
                foreach (var xp in xpaths)
                {
                    try
                    {
                        var els = driver.FindElements(By.XPath(xp));
                        foreach (var el in els)
                        {
                            try
                            {
                                if (el != null && el.Displayed && el.Enabled) return el;
                            }
                            catch (Exception) { }
                        }
                    }
                    catch (Exception) { }
                }
                return null;
            }

            bool TryType(IWebElement el, string text)
            {
                if (el == null) return false;
                if (text == null) text = "";

                try { ClickElement(driver, el); Thread.Sleep(300); } catch (Exception) { }

                try
                {
                    var tag = (el.TagName ?? "").ToLower();
                    if (tag == "input" || tag == "textarea")
                    {
                        try { el.Clear(); } catch (Exception) { }
                    }
                }
                catch (Exception) { }

                try
                {
                    el.SendKeys(OpenQA.Selenium.Keys.Control + "a");
                    el.SendKeys(OpenQA.Selenium.Keys.Backspace);
                }
                catch (Exception) { }

                try
                {
                    el.SendKeys(text);
                    return true;
                }
                catch (Exception) { }

                // Fallback for contenteditable/React inputs
                try
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript(@"
                        var el = arguments[0], txt = arguments[1];
                        try { el.focus(); } catch(e) {}
                        if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA') {
                            el.value = txt;
                        } else {
                            el.textContent = txt;
                            if (el.innerText !== undefined) el.innerText = txt;
                        }
                        try { el.dispatchEvent(new Event('input', { bubbles: true })); } catch(e) {}
                        try { el.dispatchEvent(new Event('change', { bubbles: true })); } catch(e) {}
                    ", el, text);
                    return true;
                }
                catch (Exception) { }

                return false;
            }

            bool TryClick(params string[] xpaths)
            {
                var el = FindVisibleElement(xpaths);
                if (el == null) return false;
                try { ClickElement(driver, el); return true; } catch (Exception) { }
                try { el.Click(); return true; } catch (Exception) { }
                return false;
            }

            try
            {
                driver.Navigate().GoToUrl("https://facebook.com/pages/creation?ref_type=launch_point");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1500);

            bool isWorking = false;
            int counter = 8;
            IWebElement nameE = null;
            do
            {
                Thread.Sleep(700);
                nameE = FindVisibleElement(
                    "//input[contains(@aria-label,'Page name')]",
                    "//input[contains(@placeholder,'Page name')]",
                    "//label[contains(@aria-label,'Page name')]//input",
                    "//label[.//*[contains(text(),'Page name')]]//input",
                    "//div[@role='textbox' and contains(@aria-label,'Page name')]",
                    "//div[@contenteditable='true' and (@role='textbox' or @data-lexical-editor='true')][contains(@aria-label,'Page name')]"
                );

                if (nameE != null)
                    isWorking = TryType(nameE, pageName);
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return;
            }
            Thread.Sleep(1000);

            IWebElement catE = FindVisibleElement(
                "//input[contains(@aria-label,'Category')]",
                "//input[contains(@placeholder,'Category')]",
                "//label[contains(@aria-label,'Category')]//input",
                "//label[.//*[contains(text(),'Category')]]//input",
                "//div[@role='combobox']//input[contains(@aria-label,'Category')]",
                "//div[@role='textbox' and contains(@aria-label,'Category')]",
                "//div[@contenteditable='true' and (@role='textbox' or @data-lexical-editor='true')][contains(@aria-label,'Category')]"
            );
            if (catE != null)
            {
                TryType(catE, categories);
                Thread.Sleep(1200);
                try { catE.SendKeys(OpenQA.Selenium.Keys.ArrowDown); } catch (Exception) { }
                Thread.Sleep(600);
                try { catE.SendKeys(OpenQA.Selenium.Keys.Enter); } catch (Exception) { }

                // Fallback: press Enter on active element in case FB swapped the input after typing.
                try
                {
                    var active = driver.SwitchTo().ActiveElement();
                    active.SendKeys(OpenQA.Selenium.Keys.ArrowDown);
                    Thread.Sleep(300);
                    active.SendKeys(OpenQA.Selenium.Keys.Enter);
                }
                catch (Exception) { }
            }
            Thread.Sleep(1000);
            if (!string.IsNullOrEmpty(bio))
            {
                IWebElement bioE = FindVisibleElement(
                    "//textarea[contains(@aria-label,'Bio')]",
                    "//textarea[contains(@placeholder,'Bio')]",
                    "//label[contains(@aria-label,'Bio')]//textarea",
                    "//label[.//*[contains(text(),'Bio')]]//textarea",
                    "//div[@role='textbox' and contains(@aria-label,'Bio')]",
                    "//div[@contenteditable='true' and (@role='textbox' or @data-lexical-editor='true')][contains(@aria-label,'Bio')]"
                );

                TryType(bioE, bio);
            }
            Thread.Sleep(1000);
            isWorking = false;
            isWorking = TryClick(
                "//div[@role='button' and .//span[normalize-space()='Create Page' or normalize-space()='Create page']]",
                "//span[normalize-space()='Create Page' or normalize-space()='Create page']/ancestor::div[@role='button'][1]",
                "//div[@aria-label='Create Page' or @aria-label='Create page']",
                "//div[@role='button' and .//span[normalize-space()='Create']]",
                "//div[@aria-label='Create']"
            );
            if (isWorking)
            {
                int c = 10;
                bool b = false;
                do
                {
                    Thread.Sleep(1000);
                    var source = "";
                    try
                    {
                        source = driver.PageSource.ToLower();
                    }
                    catch (Exception) { }
                    if (!source.Contains("create a page"))
                    {
                        b = true;
                    }
                } while (!b && c-- > 0);
                Thread.Sleep(1000);
            }
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static string GetPageIDs(IWebDriver driver, string token, string uid = "")
        {
            string pageIds = "";
            try
            {
                driver.Navigate().GoToUrl("https://graph.facebook.com/v14.0/me?fields=accounts.limit(1000){id,name,access_token}&access_token=" + token);
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(2000);
            IWebElement element = null;
            try
            {
                element = driver.FindElement(By.TagName("pre"));
            }
            catch (Exception) { }
            if (element == null)
            {
                try
                {
                    element = driver.FindElement(By.XPath("//div[@role='textbox']"));
                }
                catch (Exception) { }
            }
            if (element == null)
            {
                try
                {
                    element = driver.FindElement(By.XPath("//body"));
                }
                catch (Exception) { }
            }
            if (element != null)
            {
                try
                {
                    //string a = (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].previousSibling.textContent.trim();", element);

                    string a = element.Text;//.GetAttribute("innerHTML");
                    if (string.IsNullOrEmpty(a))
                    {
                        return "";
                    }
                    var b = JsonConvert.DeserializeObject<FacebookAccounts>(a.ToString());
                    if (b.accounts == null)
                    {
                        return "";
                    }
                    foreach (var d in b.accounts.data)
                    {
                        var p = JsonConvert.DeserializeObject<Pages>(d.ToString());
                        if (p.id.Trim().Contains(uid.Trim()))
                        {
                            continue;
                        }
                        if (!string.IsNullOrEmpty(pageIds))
                        {
                            pageIds += ',';
                        }
                        pageIds += p.id + "|" + p.access_token;
                    }
                }
                catch (Exception) { }
            }

            return pageIds;
        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool SwitchToProfilePage(IWebDriver driver, string profile_page_id)
        {
            // Step 1: Delete i_user cookie
            try { driver.Manage().Cookies.DeleteCookieNamed("i_user"); } catch { }
            Thread.Sleep(500);

            // Step 2: Navigate directly to the page URL
            try
            {
                driver.Navigate().GoToUrl("https://www.facebook.com/" + profile_page_id);
            }
            catch { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(3000);

            // Step 3: Click "Switch Now" button visible in the banner
            bool switched = false;
            int counter = 10;
            do
            {
                Thread.Sleep(1000);

                // ✅ EXACT match from your screenshot - blue "Switch Now" button in top banner
                if (!switched)
                {
                    try
                    {
                        var el = driver.FindElement(By.XPath(
                            "//div[@aria-label='Switch Now']"));
                        if (el.Displayed) { el.Click(); switched = true; }
                    }
                    catch { }
                }
                if (!switched)
                {
                    try
                    {
                        var el = driver.FindElement(By.XPath(
                            "//div[@aria-label='Switch now']"));
                        if (el.Displayed) { el.Click(); switched = true; }
                    }
                    catch { }
                }
                // ✅ From screenshot: the banner text contains "Switch Now" as a span
                if (!switched)
                {
                    try
                    {
                        var el = driver.FindElement(By.XPath(
                            "//span[text()='Switch Now']"));
                        if (el.Displayed) { el.Click(); switched = true; }
                    }
                    catch { }
                }
                if (!switched)
                {
                    try
                    {
                        var el = driver.FindElement(By.XPath(
                            "//span[text()='Switch now']"));
                        if (el.Displayed) { el.Click(); switched = true; }
                    }
                    catch { }
                }
                // ✅ Bottom-left banner: "Switch into [Page] to take more actions"
                if (!switched)
                {
                    try
                    {
                        var el = driver.FindElement(By.XPath(
                            "//div[contains(@class,'x1') and .//span[contains(text(),'Switch into')]]" +
                            "//div[@role='button']"));
                        if (el.Displayed) { el.Click(); switched = true; }
                    }
                    catch { }
                }
                // ✅ Any role=button containing text Switch
                if (!switched)
                {
                    try
                    {
                        var els = driver.FindElements(By.XPath(
                            "//div[@role='button' and .//span[contains(text(),'Switch')]]"));
                        foreach (var el in els)
                        {
                            if (el.Displayed)
                            {
                                el.Click();
                                switched = true;
                                break;
                            }
                        }
                    }
                    catch { }
                }
                // ✅ Check if already switched via i_user cookie
                if (!switched)
                {
                    try
                    {
                        var cookie = driver.Manage().Cookies.GetCookieNamed("i_user");
                        if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
                            switched = true;
                    }
                    catch { }
                }

            } while (!switched && counter-- > 0);

            if (!switched) return false;

            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(2000);
            FBTool.Close(driver);

            // Verify via i_user cookie
            try
            {
                var cookie = driver.Manage().Cookies.GetCookieNamed("i_user");
                if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
                {
                    UsePage(driver);
                    return true;
                }
            }
            catch { }

            var i_user = FBTool.GetPageUserId(driver);
            if (!string.IsNullOrEmpty(i_user))
            {
                UsePage(driver);
                return true;
            }

            return false;
        }
        //public static bool SwitchToProfilePage(IWebDriver driver, string profile_page_id)
        //{
        //    try
        //    {
        //        driver.Manage().Cookies.DeleteCookieNamed("i_user");
        //    }
        //    catch (Exception) { }
        //    Thread.Sleep(1000);
        //    try
        //    {
        //        Thread.Sleep(1000);
        //        driver.FindElement(By.XPath("//button[@data-cookiebanner='accept_button']")).Click();
        //        Thread.Sleep(1000);
        //    }
        //    catch (Exception) { }

        //    try
        //    {
        //        driver.Navigate().GoToUrl("https://web.facebook.com/" + profile_page_id + "/?show_switched_toast=0&show_invite_to_follow=0&show_switched_tooltip=0&show_podcast_settings=0&show_community_transition=0&show_community_review_changes=0&show_follower_visibility_disclosure=0");
        //    }
        //    catch (Exception) { }
        //    FBTool.WaitingPageLoading(driver);
        //    Thread.Sleep(1000);

        //    bool isWorking = false;
        //    int counter = 4;
        //    do
        //    {
        //        Thread.Sleep(500);
        //        try
        //        {
        //            driver.FindElement(By.XPath("//div[@aria-label='Switch Now']")).Click();
        //            isWorking = true;
        //        }
        //        catch (Exception) { }
        //        if (!isWorking)
        //        {
        //            try
        //            {
        //                driver.FindElement(By.XPath("//div[@aria-label='Switch now'']")).Click();
        //                isWorking = true;
        //            }
        //            catch (Exception) { }
        //        }
        //        if (!isWorking)
        //        {
        //            try
        //            {
        //                driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div/div[1]/div[1]/div[2]/div/div/div/div/div[4]/div[1]/div/div/div/div/div/div/div/div[2]/div/div")).Click();
        //                isWorking = true;
        //            }
        //            catch (Exception) { }
        //        }
        //        if (!isWorking)
        //        {
        //            try
        //            {
        //                driver.FindElement(By.XPath("//div[@aria-label='Get started']")).Click();
        //                isWorking = true;
        //            }
        //            catch (Exception) { }
        //        }
        //        if (!isWorking)
        //        {
        //            try
        //            {
        //                driver.FindElement(By.XPath("//div[@aria-label='Get Started']")).Click();
        //                isWorking = true;
        //            }
        //            catch (Exception) { }
        //        }
        //        if (!isWorking)
        //        {
        //            try
        //            {
        //                driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[2]/div/div/div[1]/div/div[2]/div/div/div/div/div/div[3]/div[2]/div/div/div[2]/div/div/div[2]/div")).Click();
        //                isWorking = true;
        //            }
        //            catch (Exception) { }
        //        }
        //    } while (!isWorking && counter-- > 0);
        //    if (!isWorking)
        //    {
        //        return false;
        //    }
        //    //Thread.Sleep(1000);
        //    //try
        //    //{
        //    //    driver.Navigate().GoToUrl("https://www.facebook.com");
        //    //}
        //    //catch (Exception) { }
        //    FBTool.WaitingPageLoading(driver);
        //    Thread.Sleep(1000);
        //    FBTool.Close(driver);

        //    var i_user = FBTool.GetPageUserId(driver);
        //    if (string.IsNullOrEmpty(i_user))
        //    {
        //        isWorking = false;
        //    } else
        //    {
        //        UsePage(driver);
        //    }

        //    return isWorking;
        //}
        public static void UsePage(IWebDriver driver, int counter = 2, int delaytime = 1000)
        {
            bool isWorking = false;
            do
            {
                Thread.Sleep(delaytime);
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div[2]/div/div[4]/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Use Page']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//*[@aria-label='Use Page']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[contains(text(),'Use Page')]")).Click();
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
        public static string GetPageFollow(IWebDriver driver)
        {
            string follow = "";
            string url = "";
            try
            {
                url = driver.Url;
            }
            catch (Exception) { }
            try
            {
                follow = driver.FindElement(By.XPath("//a[@href='" + url + "&sk=followers']")).GetAttribute("innerHTML").Trim();
            }
            catch (Exception) { }

            return follow;
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static int UnlockCheckpoint(IWebDriver driver, string newPassword)
        {
            bool isWorking = false;
            string url = "";
            try
            {
                url = driver.Url;
            }
            catch (Exception) { }
            if (url.Contains("956"))
            {
                // Get start step #1
                IWebElement element = null;
                try
                {
                    element = driver.FindElement(By.XPath("//div[@aria-label='Get Started']"));
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//div[@aria-label='Get started']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//span[contains(text(),'Get Started')]"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//span[contains(text(),'Get started')]"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[3]/div/div[1]/div/div[3]/div/div/button"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div/div/div[2]/div/table/tbody/tr/td/div/div[7]/a"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div/div/div/div/table/tbody/tr/td/div/div[7]/a"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div/div/div[2]/div/table/tbody/tr/td/div/div[7]/a"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[3]/div/div[1]/div/div[3]/div/div/button"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div/div/div/div/div/div/div/div/div[2]/div/div/div/div[1]/div/div/div[1]/div/div/div/div/div/div/div/div/div/div/div/div/div[5]/div"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div[2]/div/div/div[1]/div/div/div/div/div/div/div/div/div/div/div/div/div/div/div/div[5]/div"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    return -1;
                }
                try
                {
                    Actions actions = new Actions(driver);
                    actions.MoveToElement(element).Click().Perform();
                }
                catch (Exception) { }

                int counter = 10;
                isWorking = false;
                Thread.Sleep(2500);
                do
                {
                    // Next steps to unlock your account
                    Thread.Sleep(500);
                    try
                    {
                        element = driver.FindElement(By.XPath("//div[@aria-label='Next']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                    if (!isWorking)
                    {
                        try
                        {

                            element = driver.FindElement(By.XPath("//button[@value='Next']"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div/div/div/div/table/tbody/tr/td/div/div[3]/a"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div/div/div[2]/div/table/tbody/tr/td/div/div[3]/a"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[3]/div/div[1]/div/div[3]/div/div/button"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div/div[2]/div/div/div/div[1]/div/div/div[1]/div/div/div/div/div/div/div/div/div/div/div/div/div[4]/div/div/div"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div[2]/div/div/div[1]/div/div/div/div/div/div/div/div/div/div/div/div/div/div/div/div[4]/div/div/div"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div/div[2]/div/div/div/div[1]/div/div/div[1]/div/div/div/div/div/div/div/div/div/div/div/div/div[4]/div/div/div"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
                if (!isWorking)
                {
                    return -1;
                }
                try
                {
                    Actions actions = new Actions(driver);
                    actions.MoveToElement(element).Click().Perform();
                }
                catch (Exception) { }

                counter = 8;
                isWorking = false;
                Thread.Sleep(2000);
                do
                {
                    // Secure your login details
                    Thread.Sleep(500);
                    try
                    {
                        element = driver.FindElement(By.XPath("//div[@aria-label='Next']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("//button[@value='Next']"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div/div/div[2]/div/table/tbody/tr/td/div/div[4]/a"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[3]/div/div[1]/div/div[2]/div/div[2]/button"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div/div/div/div/table/tbody/tr/td/div/div[4]/a"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div/div[2]/div/div/div/div[1]/div/div/div[1]/div/div/div/div/div/div/div/div/div/div/div/div/div[4]/div/div/div"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div[2]/div/div/div[1]/div/div/div/div/div/div/div/div/div/div/div/div/div/div/div/div[5]/div/div[2]/div"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div/div[2]/div/div/div/div[1]/div/div/div[1]/div/div/div/div/div/div/div/div/div/div/div/div/div[5]/div/div[2]/div"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div/div[2]/div/div/div/div[1]/div/div/div[1]/div/div/div/div/div/div/div/div/div/div/div/div/div[4]/div/div[2]/div"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
                if (isWorking)
                {
                    try
                    {
                        Actions actions = new Actions(driver);
                        actions.MoveToElement(element).Click().Perform();
                    }
                    catch (Exception) { }
                }
                counter = 8;
                isWorking = false;
                IWebElement input = null;
                Thread.Sleep(2000);
                do
                {
                    Thread.Sleep(500);
                    if (!isWorking)
                    {
                        // Select emails and phone numbers to remove
                        bool b = false;
                        try
                        {
                            element = driver.FindElement(By.XPath("//div[@aria-label=`Don't remove anything`]"));
                            b = true;
                        }
                        catch (Exception) { }
                        if (!isWorking && !b)
                        {
                            try
                            {
                                element = driver.FindElement(By.XPath("//div[@aria-label=`Don't Remove Anything`]"));
                                b = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking && !b)
                        {
                            try
                            {
                                element = driver.FindElement(By.XPath("//*[@aria-label=`Don't remove anything`]"));
                                b = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking && !b)
                        {
                            try
                            {
                                element = driver.FindElement(By.XPath("//*[@aria-label=`Don't Remove Anything`]"));
                                b = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking && !b)
                        {
                            try
                            {
                                element = driver.FindElement(By.XPath("//*[@value=`Don't remove anything`]"));
                                b = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking && !b)
                        {
                            try
                            {
                                element = driver.FindElement(By.XPath("//*[@value=`Don't Remove Anything`]"));
                                b = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking && !b)
                        {
                            try
                            {
                                element = driver.FindElement(By.XPath("//*[@value=`Don't Remove anything`]"));
                                b = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking && !b)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//*[contains(text(),`Don't Remove Anything`)]")).Click();
                                b = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking && !b)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//*[contains(text(),`Don't remove anything`)]")).Click();
                                b = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking && !b)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//*[contains(text(),`Don't Remove anything`)]")).Click();
                                b = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking && !b)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//*[contains(text(),`Don't remove Anything`)]")).Click();
                                b = true;
                            }
                            catch (Exception) { }
                        }
                        if (!b)
                        {
                            try
                            {
                                element = driver.FindElement(By.XPath("/html/body/div/div/div[2]/div/table/tbody/tr/td/form/div/div[5]/input"));
                                b = true;
                            }
                            catch (Exception) { }
                        }
                        if (!b)
                        {
                            try
                            {
                                element = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div/div[2]/div/div/div/div[1]/div/div/div[1]/div/div/div/div/div/div/div/div/div/div/div/div/div[4]/div/div/div"));
                                b = true;
                            }
                            catch (Exception) { }
                        }
                        if (!b)
                        {
                            try
                            {
                                element = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div[2]/div/div/div[1]/div/div/div/div/div/div/div/div/div/div/div/div/div/div/div/div[4]/div/div/div"));
                                b = true;
                            }
                            catch (Exception) { }
                        }
                        if (!b)
                        {
                            try
                            {
                                element = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div[2]/div/div/div[1]/div/div/div/div/div/div/div/div/div/div/div/div/div/div/div/div[4]/div/div/div"));
                                b = true;
                            }
                            catch (Exception) { }
                        }
                        if (!b)
                        {
                            try
                            {
                                element = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div/div[2]/div/div/div/div[1]/div/div/div[1]/div/div/div/div/div/div/div/div/div/div/div/div/div[4]/div/div/div"));
                                b = true;
                            }
                            catch (Exception) { }
                        }
                        if (b)
                        {
                            try
                            {
                                Actions actions = new Actions(driver);
                                actions.MoveToElement(element).Click().Perform();
                            }
                            catch (Exception) { }
                            Thread.Sleep(3000);
                        }
                    }

                    // Your new login details
                    try
                    {
                        input = driver.FindElement(By.XPath("//input[@type='password']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("//button[@value='Next']"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("//div[@aria-label='Next']"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[3]/div/div[1]/div/div[2]/div/div[2]/button"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div/div/div[2]/div/table/tbody/tr/td/div/div[4]/a"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div/div/div/div/table/tbody/tr/td/div/div[4]/a"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div/div[2]/div/div/div/div[1]/div/div/div[1]/div/div/div/div/div/div/div/div/div/div/div/div/div[4]/div"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div/div[2]/div/div/div/div[1]/div/div/div[1]/div/div/div/div/div/div/div/div/div/div/div/div/div[4]/div"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
                if (input != null)
                {
                    Thread.Sleep(2000);
                    try
                    {
                        input.Clear();
                        input.SendKeys(newPassword);
                        isWorking = true;
                    }
                    catch (Exception) { }
                    if (isWorking)
                    {
                        Thread.Sleep(2000);
                        isWorking = false;
                        try
                        {
                            element = driver.FindElement(By.XPath("//div[@aria-label='Save changes']"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                        if (!isWorking)
                        {
                            try
                            {
                                element = driver.FindElement(By.XPath("//button[@aria-label='Save Changes']"));
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                element = driver.FindElement(By.XPath("//button[@aria-label='Save changes']"));
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                element = driver.FindElement(By.XPath("//div[@aria-label='Save Changes']"));
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                element = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div/div/div/div/div[2]/div/div/div[1]/div/div/div/div/div/div/div/div/div/div/div/div/div/div/div/div[4]/div/div/div"));
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (isWorking)
                        {
                            try
                            {
                                Actions actions = new Actions(driver);
                                actions.MoveToElement(element).Click().Perform();
                            }
                            catch (Exception) { }
                        }
                    }
                    if (isWorking)
                    {
                        Thread.Sleep(2500);
                        try
                        {
                            driver.Navigate().GoToUrl("https://facebook.com");
                        }
                        catch (Exception) { }
                        Thread.Sleep(2000);

                        return 1;
                    }
                }

                isWorking = FBTool.CheckLiveUID(FBTool.GetUserId(driver));
                if (isWorking)
                {
                    try
                    {
                        driver.Navigate().GoToUrl("https://facebook.com");
                    }
                    catch (Exception) { }
                    FBTool.WaitingPageLoading(driver);
                    Thread.Sleep(1000);

                    return 2;
                }
            }

            return 0;
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool ChangePassword(IWebDriver driver, string oldPassword, string newPassword, bool isLogic1 = true)
        {
            try
            {
                driver.Navigate().GoToUrl("https://facebook.com/privacy/checkup/?source=settings_and_privacy");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1500);
            CloseAllPopup(driver);
            Thread.Sleep(500);

            try
            {
                driver.FindElement(By.XPath("//div[@aria-label='More You Can Do']")).Click();
                Thread.Sleep(500);
            }
            catch (Exception) { }

            bool isWorking = false;
            int counter = 8;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//span[contains(text(),'How to keep your account secure')]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div/div/div/div[2]/div/div[2]/div/div/div/div/div[1]/div/img")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[5]/div/div/div[3]/div/div/div[1]/div[1]/div/div/div/div/div[2]/div/div[2]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div[1]/div[1]/div/div[3]/div/div/div/div[1]/div[1]/div/div/div/div/div[2]/div/div[2]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return false;
            }
            counter = 8;
            isWorking = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[2]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/div[2]/div[2]/div/div")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/div[2]/div[2]/div/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[6]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/div[2]/div[2]/div/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Continue']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return false;
            }
            Thread.Sleep(1000);
            counter = 8;
            isWorking = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[2]/div/div/div[1]/div/div[2]/div/div/div/div/div[2]/div/div[4]/div[1]/div[1]/div/div[3]/div/div")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[2]/div/div[4]/div[1]/div[1]/div/div[3]/div/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[6]/div/div/div[1]/div/div[2]/div/div/div/div/div[2]/div/div[4]/div[1]/div[1]/div/div[3]/div/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Change Password']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Change password']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return false;
            }
            counter = 8;
            isWorking = false;

            if (isLogic1)
            {
                do
                {
                    isWorking = false;
                    Thread.Sleep(500);
                    IReadOnlyCollection<IWebElement> element = driver.FindElements(By.XPath("//input[@type='password']"));

                    try
                    {
                        element.ElementAt(0).SendKeys(oldPassword);
                        Thread.Sleep(500);
                    }
                    catch (Exception) { }
                    try
                    {
                        element.ElementAt(1).SendKeys(newPassword);
                        Thread.Sleep(500);
                    }
                    catch (Exception) { }
                    try
                    {
                        element.ElementAt(02).SendKeys(newPassword);
                        Thread.Sleep(500);
                        isWorking = true;
                    }
                    catch (Exception) { }
                } while (!isWorking && counter-- > 0);
            }
            else
            {
                do
                {
                    isWorking = false;
                    Thread.Sleep(500);
                    try
                    {
                        driver.FindElement(By.XPath("//input[@aria-label='Current password']")).SendKeys(oldPassword);
                        Thread.Sleep(500);
                    }
                    catch (Exception) { }
                    try
                    {
                        driver.FindElement(By.XPath("//input[@aria-label='New password']")).SendKeys(newPassword);
                        Thread.Sleep(500);
                    }
                    catch (Exception) { }
                    try
                    {
                        driver.FindElement(By.XPath("//input[@aria-label='Re-type new password']")).SendKeys(newPassword);
                        isWorking = true;
                    }
                    catch (Exception) { }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//input[@aria-label='Retype new password']")).SendKeys(newPassword);
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
            }
            if (!isWorking)
            {
                return false;
            }
            Thread.Sleep(1000);
            isWorking = false;
            try
            {
                driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[2]/div/div/div[1]/div/div[2]/div/div/div/div/div[2]/div/div[4]/div[1]/div[1]/div/div[3]/div[4]/div")).Click();
                isWorking = true;
            }
            catch (Exception) { }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[2]/div/div[4]/div[1]/div[1]/div/div[3]/div[4]/div")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[6]/div/div/div[1]/div/div[2]/div/div/div/div/div[2]/div/div[4]/div[1]/div[1]/div/div[3]/div[4]/div")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Save Changes']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Save changes']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (isWorking)
            {
                Thread.Sleep(5000);
                //try
                //{
                //    driver.FindElement(By.XPath("//input[@aria-label='Current password']"));
                //    isWorking = false;
                //}
                //catch (Exception) { }
            }

            return isWorking;
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static bool SlugHacked(IWebDriver driver, string oldPassword, string newPassword)
        {
            bool isWorking = false;
            try
            {
                driver.Navigate().GoToUrl("https://facebook.com/hacked");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            int counter = 4;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//span[contains(text(),'Someone else got into my account without my permission')]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div[1]/div/div/form/div[1]/div[3]/label[2]/span")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return false;
            }
            try
            {
                driver.FindElement(By.Name("confirmed")).Click();
                isWorking = true;
            }
            catch (Exception) { }
            counter = 4;
            isWorking = false;
            IWebElement elementGetStart = null;
            do
            {
                Thread.Sleep(500);
                try
                {
                    elementGetStart = driver.FindElement(By.XPath("//a[@href='#scan']"));
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        elementGetStart = driver.FindElement(By.XPath("//a[@name='submit[Get Started]']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        elementGetStart = driver.FindElement(By.XPath("//a[@name='submit[Get started]']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        elementGetStart = driver.FindElement(By.XPath("//a[contains(text(),'Get Started')]"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        elementGetStart = driver.FindElement(By.XPath("//a[contains(text(),'Get started')]"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        elementGetStart = driver.FindElement(By.Id("checkpointButtonGetStarted"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        elementGetStart = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div[1]/div/form/div/div[2]/div[1]/a"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return true;
            }
            try
            {
                Actions actions = new Actions(driver);
                actions.MoveToElement(elementGetStart).Click().Perform();
            }
            catch (Exception) { }
            isWorking = false;
            counter = 30;
            do
            {
                Thread.Sleep(600);
                try
                {
                    driver.FindElement(By.Id("checkpointSubmitButton")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//a[@name='submit[Continue]']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div[1]/div/form/div/div[3]/div[1]/button")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return false;
            }
            isWorking = false;
            counter = 4;

            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.Name("password_new"));
                    isWorking = true;
                }
                catch (Exception) { }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return false;
            }
            try
            {
                driver.FindElement(By.Name("password_old")).SendKeys(oldPassword);
                Thread.Sleep(500);
            }
            catch (Exception) { }
            try
            {
                driver.FindElement(By.Name("password_new")).SendKeys(newPassword);
                Thread.Sleep(500);
            }
            catch (Exception) { }
            try
            {
                driver.FindElement(By.Name("password_confirm")).SendKeys(newPassword);
                Thread.Sleep(500);
            }
            catch (Exception) { }
            isWorking = false;
            try
            {
                driver.FindElement(By.Id("checkpointSubmitButton")).Click();
                isWorking = true;
            }
            catch (Exception) { }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("//*[@name='submit[Continue]']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div[1]/div/form/div/div[3]/div[1]/button")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                return false;
            }
            counter = 8;
            do
            {
                Thread.Sleep(1000);
                try
                {
                    driver.FindElement(By.Name("password_new"));
                    isWorking = false;
                }
                catch (Exception) { }
            } while (isWorking && counter-- > 0);
            if (!isWorking)
            {
                Thread.Sleep(2000);
                return true;
            }

            return false;
        }

        public static int ShareWebsiteToGroup(IWebDriver driver, string groupId, bool isPastLink, string link, string caption, string multipleImageStr)
        {
            int counter = 6;
            bool isWorking = false;
            IWebElement elementCaption = null;
            do
            {
                Thread.Sleep(1000);
                if (!isWorking)
                {
                    try
                    {
                        elementCaption = driver.FindElement(By.XPath("//*[contains(text(),'Write something...')]"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        elementCaption = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[4]/div/div/div[2]/div/div/div/div[1]/div/div/div/div[1]/div"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                try
                {
                    var source = driver.PageSource.ToString().ToLower();
                    if (source.Contains("invited you to join this group") || source.Contains("decline invite"))
                    {
                        return -1;
                    }
                }
                catch (Exception) { }

                NotNowButton(driver);

            } while (!isWorking && counter-- > 0);
            if (!isWorking) { return 0; }

            if (!isPastLink)
            {
                // click element write something....
                counter = 4;
                isWorking = false;
                do
                {
                    Thread.Sleep(1000);
                    if (!isWorking)
                    {
                        try
                        {
                            elementCaption = driver.FindElement(By.XPath("//*[contains(text(),'Photo/video')]"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
                if (!isWorking) { return 0; }

                ClickElement(driver, elementCaption);
                Thread.Sleep(2000);
                //SendCaption(driver, caption);

                // comment link
                Thread.Sleep(2000);
                isWorking = false;
                if (!string.IsNullOrEmpty(multipleImageStr))
                {
                    counter = 4;
                    IWebElement upload_file = null;
                    do
                    {
                        Thread.Sleep(1000);
                        if (!isWorking)
                        {
                            try
                            {
                                upload_file = driver.FindElement(By.XPath("//input[@accept='image/*,image/heif,image/heic,video/*,video/mp4,video/x-m4v,video/x-matroska,.mkv']"));
                                //upload_file.SendKeys(multipleImageStr);
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                upload_file = driver.FindElement(By.XPath("//input[@type='file']"));
                                //upload_file.SendKeys(multipleImageStr);
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                    } while (!isWorking && counter-- > 0);
                    if (isWorking)
                    {
                        //try
                        //{
                        //    Actions actions = new Actions(driver);
                        //    actions
                        //        .SendKeys(upload_file, multipleImageStr)
                        //        .Build()
                        //        .Perform();
                        //}
                        //catch (Exception) { }
                        try
                        {
                            upload_file.SendKeys(multipleImageStr);
                        }
                        catch (Exception) { }
                    }
                }
                if (!isWorking) { return 0; }


                Thread.Sleep(4000);
                isWorking = false;
                counter = 6;
                elementCaption = null;
                do
                {
                    Thread.Sleep(1000);

                    if (!isWorking)
                    {
                        try
                        {
                            elementCaption = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementCaption = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div[2]/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementCaption = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementCaption = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementCaption = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[4]/div/div[2]/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
                if (isWorking)
                {
                    try
                    {
                        Actions actions = new Actions(driver);
                        actions
                            .SendKeys(elementCaption, caption)
                            .Build()
                            .Perform();

                        Thread.Sleep(1000);
                    }
                    catch (Exception) { }
                }

                ClickButtonPost(driver);
                Thread.Sleep(2000);
                isWorking = false;
                counter = 6;
                do
                {
                    isWorking = true;
                    Thread.Sleep(1000);
                    var souces = driver.PageSource.ToLower().Trim();
                    if (souces.Contains("posting"))
                    {
                        isWorking = false;
                    }
                } while (!isWorking && counter-- > 0);
                Thread.Sleep(1000);
                try
                {
                    driver.Navigate().GoToUrl("https://www.facebook.com/groups/" + groupId + "/my_posted_content");
                }
                catch (Exception) { }
                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(1000);
                counter = 8;
                isWorking = false;
                do
                {
                    Thread.Sleep(500);
                    if (!isWorking)
                    {
                        try
                        {
                            elementCaption = driver.FindElement(By.XPath("//*[contains(text(),'View in group')]"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementCaption = driver.FindElement(By.XPath("//*[contains(text(),'View in Group')]"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementCaption = driver.FindElement(By.XPath("//*[contains(text(),'View In Group')]"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementCaption = driver.FindElement(By.XPath("//*[@aria-label='View in group']"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementCaption = driver.FindElement(By.XPath("//*[@aria-label='View in Group']"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementCaption = driver.FindElement(By.XPath("//*[@aria-label='View In Group']"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking & counter-- > 0);
                if (!isWorking) { return 0; }

                //try
                //{
                //    var sor = driver.PageSource.Trim();
                //    if (sor.Contains("just now"))
                //    {

                //    }
                //    else
                //    {
                //        return false;
                //    }
                //}
                //catch (Exception) { }
                if (!string.IsNullOrEmpty(caption))
                {
                    int len = 20;
                    if (caption.Length <= len)
                    {
                        len = caption.Length / 2;
                    }
                    string captionShort = caption.Substring(0, len);
                    try
                    {
                        var sor = driver.PageSource.ToLower().Trim();
                        if (!sor.Contains(captionShort.ToLower().Trim()))
                        {
                            return 0;
                        }
                    }
                    catch (Exception) { }
                }

                ClickElement(driver, elementCaption);
                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(1000);

                isWorking = false;
                counter = 5;
                IWebElement elementComment = null;
                do
                {
                    Thread.Sleep(1000);
                    if (!isWorking)
                    {
                        try
                        {
                            elementComment = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[4]/div/div[2]/div/div/div/div[2]/div[1]/div[2]/div/div/div/div/div/div/div/div[1]/div/div[13]/div/div/div[4]/div/div/div[2]/div[2]/div/div/div/div[2]/div/div[2]/form/div/div[1]/div[1]/div/div[1]/div/div[1]/p/br"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementComment = driver.FindElement(By.XPath("//div[@data-lexical-editor='true']"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking & counter-- > 0);
                if (!isWorking)
                {
                    return 1;// post success
                }

                try
                {
                    elementComment.SendKeys(link + OpenQA.Selenium.Keys.Enter);
                }
                catch (Exception) { }
                Thread.Sleep(2000);
                isWorking = false;
                counter = 6;
                do
                {
                    isWorking = true;
                    Thread.Sleep(1000);
                    var souces = driver.PageSource.ToLower().Trim();
                    if (souces.Contains("posting"))
                    {
                        isWorking = false;
                    }
                } while (!isWorking && counter-- > 0);
                Thread.Sleep(1000);

                return 1;
            }
            else
            {   // past link

                // click element write something....
                ClickElement(driver, elementCaption);

                SendCaption(driver, link);

                counter = 6;
                isWorking = false;
                IWebElement elementPastCaption = null;
                do
                {
                    Thread.Sleep(1000);
                    if (!isWorking)
                    {
                        try
                        {
                            elementPastCaption = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div[1]/div/div/div/div/div/div/div/div/div/span/span/span"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementPastCaption = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div[1]/div/div/div/div/div/div/div/div/div/span/span/span"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
                if (!isWorking) { return 0; }
                try
                {
                    Thread.Sleep(5000);
                    elementPastCaption.SendKeys(OpenQA.Selenium.Keys.Control + "a");
                    Thread.Sleep(1000);
                    elementPastCaption.SendKeys(OpenQA.Selenium.Keys.Delete);
                    Thread.Sleep(500);
                }
                catch (Exception) { }

                counter = 6;
                isWorking = false;
                do
                {
                    Thread.Sleep(1000);
                    if (!isWorking)
                    {
                        try
                        {
                            elementPastCaption = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementPastCaption = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
                if (!isWorking) { return 0; }
                try
                {
                    Actions actions = new Actions(driver);
                    actions
                        .SendKeys(elementPastCaption, caption)
                        .Build()
                        .Perform();

                    Thread.Sleep(1000);
                }
                catch (Exception) { }

                ClickButtonPost(driver);
                Thread.Sleep(4000);
            }

            return 1;
        }
        public static void ShareWebsiteToPage(IWebDriver driver, bool isPastLink, string link, string caption, string multipleImageStr)
        {
            int counter = 6;
            bool isWorking = false;
            IWebElement elementCaption = null;
            do
            {
                Thread.Sleep(1000);
                if (!isWorking)
                {
                    try
                    {
                        elementCaption = driver.FindElement(By.XPath("//*[contains(text(),'Photo/video')]"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }

                NotNowButton(driver);

            } while (!isWorking && counter-- > 0);
            if (!isWorking) { return; }

            if (!isPastLink)
            {
                // click element write something....
                ClickElement(driver, elementCaption);
                Thread.Sleep(3000);
                //SendCaption(driver, caption);

                // comment link
                if (!string.IsNullOrEmpty(multipleImageStr))
                {
                    isWorking = false;
                    counter = 6;
                    do
                    {
                        Thread.Sleep(1000);
                        if (!isWorking)
                        {
                            try
                            {
                                IWebElement upload_file = driver.FindElement(By.XPath("//input[@accept='image/*,image/heif,image/heic,video/*,video/mp4,video/x-m4v,video/x-matroska,.mkv']"));
                                upload_file.SendKeys(multipleImageStr);
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                IWebElement upload_file = driver.FindElement(By.XPath("//input[@type='file']"));
                                upload_file.SendKeys(multipleImageStr);
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                    } while (!isWorking && counter-- > 0);

                    Thread.Sleep(4500);
                }

                isWorking = false;
                counter = 6;
                elementCaption = null;
                do
                {
                    Thread.Sleep(1000);

                    if (!isWorking)
                    {
                        try
                        {
                            elementCaption = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[2]/div[1]/div[1]/div[1]/div/div/div[1]/p/br"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementCaption = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[2]/div[1]/div[1]/div[1]/div/div/div[1]/p/br"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }

                } while (!isWorking && counter-- > 0);

                try
                {
                    Actions actions = new Actions(driver);
                    actions
                        .SendKeys(elementCaption, caption)
                        .Build()
                        .Perform();

                    Thread.Sleep(1000);
                }
                catch (Exception) { }

                ClickButtonPost(driver);
                Thread.Sleep(3000);
                isWorking = false;
                counter = 12;
                do
                {
                    isWorking = true;
                    Thread.Sleep(1000);
                    var souces = driver.PageSource.ToLower().Trim();
                    if (souces.Contains("chat directly with customers"))
                    {
                        NotNowButton(driver);
                    }
                    else if (souces.Contains("posting"))
                    {
                        isWorking = false;
                    }
                } while (!isWorking && counter-- > 0);
                Thread.Sleep(1000);

                try
                {
                    driver.Navigate().GoToUrl("https://facebook.com/me");
                }
                catch (Exception) { }
                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(1000);

                isWorking = false;
                counter = 6;
                IWebElement elementComment = null;
                do
                {
                    Thread.Sleep(500);
                    if (!isWorking)
                    {
                        try
                        {
                            elementComment = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div[2]/div/div/div/div[3]/div/div[3]/div/div[2]/div[1]/div/div/div/div/div/div/div/div/div/div/div/div/div/div[13]/div/div/div[5]/div/div/div[1]/div/div/div/div[2]/div"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementComment = driver.FindElement(By.XPath("//div[@aria-label='Leave a comment']"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking & counter-- > 0);
                if (!isWorking) { return; }
                try
                {
                    ClickElement(driver, elementComment);
                    Thread.Sleep(2000);
                }
                catch (Exception) { }
                isWorking = false;
                counter = 6;
                elementComment = null;
                do
                {
                    Thread.Sleep(500);
                    if (!isWorking)
                    {
                        try
                        {
                            elementComment = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div[2]/div/div/div/div[3]/div/div[3]/div/div[2]/div[1]/div/div/div/div/div/div/div/div/div/div/div/div/div/div[13]/div/div/div[5]/div/div/div[2]/div[2]/div/div/div/div[2]/div/div[2]/form/div/div[1]/div[1]/div/div[1]/div/div[1]/p/br"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementComment = driver.FindElement(By.XPath("//div[@contenteditable='true']"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking & counter-- > 0);
                try
                {
                    elementComment.SendKeys(link + OpenQA.Selenium.Keys.Enter);
                }
                catch (Exception) { }
                Thread.Sleep(2000);
                isWorking = false;
                counter = 6;
                do
                {
                    isWorking = true;
                    Thread.Sleep(1000);
                    var souces = driver.PageSource.ToLower().Trim();
                    if (souces.Contains("chat directly with customers"))
                    {
                        NotNowButton(driver);
                    }
                    else if (souces.Contains("posting"))
                    {
                        isWorking = false;
                    }
                } while (!isWorking && counter-- > 0);
                Thread.Sleep(1000);
            }
            else
            {
                // past link
                counter = 6;
                isWorking = false;
                elementCaption = null;
                do
                {
                    Thread.Sleep(1000);
                    if (!isWorking)
                    {
                        try
                        {
                            elementCaption = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div[2]/div/div/div/div[3]/div/div[2]/div/div/div/div[1]/div/div[1]/span"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementCaption = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div[2]/div/div/div/div[3]/div/div[2]/div/div/div/div[1]/div/div[1]/span"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);

                // click element write something....
                ClickElement(driver, elementCaption);

                SendCaption(driver, link);

                counter = 6;
                isWorking = false;
                IWebElement elementPastCaption = null;
                do
                {
                    Thread.Sleep(1000);
                    if (!isWorking)
                    {
                        try
                        {
                            elementPastCaption = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[2]/div[1]/div[1]/div[1]/div/div/div/p/a/span"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementPastCaption = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[2]/div[1]/div[1]/div[1]/div/div/div/p/a/span"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
                if (!isWorking) { return; }
                try
                {
                    Thread.Sleep(5000);
                    elementPastCaption.SendKeys(OpenQA.Selenium.Keys.Control + "a");
                    Thread.Sleep(1000);
                    elementPastCaption.SendKeys(OpenQA.Selenium.Keys.Delete);
                    Thread.Sleep(500);
                }
                catch (Exception) { }

                counter = 6;
                isWorking = false;
                do
                {
                    Thread.Sleep(1000);
                    if (!isWorking)
                    {
                        try
                        {
                            elementPastCaption = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[2]/div[1]/div[1]/div[1]/div/div/div[1]/p/br"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementPastCaption = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[2]/div[1]/div[1]/div[1]/div/div/div[1]/p/br"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
                if (!isWorking) { return; }
                try
                {
                    Actions actions = new Actions(driver);
                    actions
                        .SendKeys(elementPastCaption, caption)
                        .Build()
                        .Perform();

                    Thread.Sleep(1000);
                }
                catch (Exception) { }

                ClickButtonPost(driver);
                counter = 6;
                isWorking = false;
                do
                {
                    Thread.Sleep(1000);
                    var souce = driver.PageSource.ToLower().Trim();
                    if (souce.Contains("publish original"))
                    {
                        counter += 1;
                        isWorking = false;
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//div[@aria-label='Publish Original Post']")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//div[@aria-label='Publish original post']")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//*[contains(text(),'Publish Original Post')]")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//*[contains(text(),'Publish original post')]")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                    }
                } while (counter-- > 0);
                Thread.Sleep(1000);
            }
        }
        public static void NotNowButton(IWebDriver driver)
        {
            bool notNowButton = false;
            IWebElement element = null;
            if (!notNowButton)
            {
                try
                {
                    element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div[2]/div[1]/div/div[2]/div/div/div/div[4]/div/div[1]/div[2]"));
                    notNowButton = true;
                }
                catch (Exception) { }
            }
            if (!notNowButton)
            {
                try
                {
                    element = driver.FindElement(By.XPath("//*[@aria-label='Not now']"));
                    notNowButton = true;
                }
                catch (Exception) { }
            }
            if (!notNowButton)
            {
                try
                {
                    element = driver.FindElement(By.XPath("//*[@aria-label='Not Now']"));
                    notNowButton = true;
                }
                catch (Exception) { }
            }
            if (!notNowButton)
            {
                try
                {
                    element = driver.FindElement(By.XPath("//*[contains(text(),'Not now')]"));
                    notNowButton = true;
                }
                catch (Exception) { }
            }
            if (!notNowButton)
            {
                try
                {
                    element = driver.FindElement(By.XPath("//*[contains(text(),'Not Now')]"));
                    notNowButton = true;
                }
                catch (Exception) { }
            }
            if (notNowButton)
            {
                ClickElement(driver, element);
                Thread.Sleep(1000);
            }
        }
        public static void SendCaption(IWebDriver driver, string caption)
        {
            bool isWorking = false;
            int counter = 6;
            IWebElement elementCreatePost = null;
            do
            {
                Thread.Sleep(1000);
                if (!isWorking)
                {
                    try
                    {
                        elementCreatePost = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        elementCreatePost = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[2]/div[1]/div[1]/div[1]/div/div/div[1]/p/br"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        elementCreatePost = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        elementCreatePost = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[2]/div[1]/div[1]/div[1]/div/div/div[1]/p/br"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking) { return; }

            try
            {
                elementCreatePost.SendKeys(caption);
                Thread.Sleep(1000);
            }
            catch (Exception) { }
        }
    }
}
