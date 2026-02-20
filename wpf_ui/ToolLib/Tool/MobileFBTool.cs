using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolKHBrowser.Helper;
using ToolKHBrowser.ToolLib.Data;
using ToolKHBrowser.ViewModels;
using ToolKHBrowser.ViewModels;
using ToolLib.Tool;

namespace ToolKHBrowser.ToolLib.Tool
{
    public static class MobileFBTool
    {
        public static string LoggedIn(IWebDriver driver, FbAccount data, bool isLoginByCookie = false, string url = "")
        {
            if (string.IsNullOrEmpty(url))
            {
                FBTool.GoToFacebook(driver, Constant.FB_MOBILE_URL);
            }
            else
            {
                FBTool.GoToFacebook(driver, url);
                FBTool.WaitingPageLoading(driver);
                if (string.IsNullOrEmpty(FBTool.GetUserId(driver)))
                {
                    FBTool.GoToFacebook(driver, Constant.FB_MOBILE_URL);
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

            return FBTool.GetResults(driver);
        }
        public static int Login(IWebDriver driver, FbAccount data, bool isLoginByCookie = false, string url = "")
        {
            if (string.IsNullOrEmpty(url))
            {
                FBTool.GoToFacebook(driver, Constant.FB_MOBILE_URL);
            }
            else
            {
                FBTool.GoToFacebook(driver, url);
                FBTool.WaitingPageLoading(driver);
                if (string.IsNullOrEmpty(FBTool.GetUserId(driver)))
                {
                    FBTool.GoToFacebook(driver, Constant.FB_MOBILE_URL);
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
        public static void GoToGroup(IWebDriver driver, string groupId)
        {
            try
            {
                driver.Navigate().GoToUrl(FBTool.GetSafeGroupUrl(Constant.FB_MOBILE_URL, groupId));
            }
            catch (Exception) { }

            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
        }
        public static void GoToMobile(IWebDriver driver)
        {
            string url = "";
            try
            {
                //driver.Navigate().GoToUrl("https://mobile.facebook.com/mobile/zero/carrier_page/settings_page/");
                driver.Navigate().GoToUrl("https://m.facebook.com/zero/policy/optin?_rdc=1&_rdr");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            try
            {
                url = driver.Url;
            }
            catch (Exception) { }
            int dataMode = IsDataMode(driver);
            if (dataMode==1)
            {
                UseDataMode(driver);
            } else if(dataMode==2)
            {
                UseDataMode1(driver);
            }          
        }
        public static int IsDataMode(IWebDriver driver)
        {
            int dataMode = 0;
            string url = "";
            try
            {
                url = driver.Url;
            }
            catch (Exception) { }
            if (url.Contains("/zero/policy"))
            {
                dataMode = 1;
            } else if (url.Contains("/zero/carrier_page"))
            {
                dataMode = 2;
            }

            return dataMode;
        }
        public static void UseDataMode1(IWebDriver driver)
        {
            bool isWorking = false;
            int counter = 6;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//a[@data-sigil='no_mpc']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div/div/div[2]/div[2]/a")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if(!isWorking)
            {
                return;
            }
            isWorking = false;
            counter = 8;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//button[@value='Decline']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if(!isWorking)
                {
                    try {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div[1]/div/div/div[4]/form/button")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if(!isWorking)
                {
                    isWorking = UseDataMode(driver);
                }
            } while (!isWorking && counter-- > 0);
        }
        public static bool UseDataMode(IWebDriver driver)
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
                    driver.FindElement(By.XPath("//span[contains(text(),'No Thanks')]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            // ok, use data
            if (isWorking)
            {
                Thread.Sleep(3000);
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
        public static bool VerifyTwoFactorAuthentication(IWebDriver driver, FbAccount data)
        {
            // If it’s push approval, wait for phone approval
            if (IsWaitingForApproval(driver))
            {
                return WaitForPushApproval(driver, 180);
            }

            // If it’s not push approval, do nothing here (or return false)
            // because Option B = push only
            return false;
        }
        public static bool IsWaitingForApproval(IWebDriver driver)
        {
            if (driver == null) return false;

            try
            {
                // URL check (strong indicator)
                string url = (driver.Url ?? "").ToLower();
                if (!url.Contains("two_factor") && !url.Contains("two_step_verification"))
                    return false;

                // Text detection (multiple variations)
                return driver.FindElements(By.XPath(
                    "//*[contains(text(),'Waiting for approval') " +
                    "or contains(text(),'Check your notifications') " +
                    "or contains(text(),'another device')]"))
                    .Any(e => e.Displayed);
            }
            catch
            {
                return false;
            }
        }

        public static bool WaitForPushApproval(IWebDriver driver, int timeoutSeconds = 180)
        {
            if (driver == null) return false;

            var end = DateTime.UtcNow.AddSeconds(timeoutSeconds);

            while (DateTime.UtcNow < end)
            {
                // 1) If already logged in => done
                try
                {
                    var uid = FBTool.GetUserId(driver);
                    if (!string.IsNullOrEmpty(uid))
                        return true;
                }
                catch { }

                // 2) If code input appears => push step ended, now you can fill code
                if (TwoFaHelper.IsCodeInputScreenStrong(driver))
                    return true;

                // 3) If still waiting approval => keep waiting
                if (IsWaitingForApproval(driver))
                {
                    Thread.Sleep(1000);
                    continue;
                }

                // 4) Check URL AFTER waiting check
                string url = "";
                try { url = (driver.Url ?? "").ToLower(); } catch { }

                // If we left two_factor pages => likely approved
                if (!url.Contains("two_step_verification") && !url.Contains("two_factor"))
                {
                    // not always instantly logged in, but push step is finished
                    return true;
                }

                Thread.Sleep(1000);
            }

            return false;
        }
        public static bool LoginByUID(IWebDriver driver, FbAccount data)
        {
            bool isWorking = false;
            string uid = data.UID.Trim();
            string pass = data.Password.Trim();
            try
            {
                driver.FindElement(By.XPath("//input[@name='email']")).Clear();
                Thread.Sleep(500);
                driver.FindElement(By.XPath("//input[@name='email']")).SendKeys(uid);
                Thread.Sleep(1000);
            } catch (Exception)  { }
            try
            {
                driver.FindElement(By.XPath("//input[@name='pass']")).Clear();
                Thread.Sleep(500);
                driver.FindElement(By.XPath("//input[@name='pass']")).SendKeys(pass);
                Thread.Sleep(1000);
            }
            catch (Exception) { }
            try
            {
                driver.FindElement(By.XPath("//button[@name='login']")).Click();
                isWorking = true;
                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(1000);
            }
            catch (Exception) { }

            return isWorking;
        }
        public static bool SaveInfo(IWebDriver driver, FbAccount data)
        {
            bool isWorking = false;
            int counter = 4;
            string url = "";
            do
            {
                Thread.Sleep(1000);
                try
                {
                    url = driver.Url;
                    if (url.Contains("/save-device"))
                    {
                        isWorking = true;
                    }
                }
                catch (Exception) { }
            } while (!isWorking && counter-- > 0);
            if (isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div/div[1]/div/div/div[3]/div[2]/form/div/button")).Click();
                }
                catch (Exception) { }
                try
                {
                    driver.FindElement(By.XPath("//span[contains(text(),'OK')]")).Click();
                }
                catch (Exception) { }
            }

            return isWorking;
        }
        public static bool Is2FA(IWebDriver driver)
        {
            if (driver == null) return false;

            string url = "";
            try { url = (driver.Url ?? "").ToLower(); } catch { }

            // Must be on 2FA-related page
            bool urlLooks2FA =
                url.Contains("two_step_verification") ||
                url.Contains("two_factor");

            if (!urlLooks2FA)
                return false;

            // Check for actual 2FA inputs or push text
            return
                SeleniumX.Exists(driver, By.XPath("//input[@aria-label='Login code']"), 1) ||
                SeleniumX.Exists(driver, By.XPath("//input[@name='approvals_code']"), 1) ||
                SeleniumX.Exists(driver, By.XPath("//input[@placeholder='Code']"), 1) ||
                SeleniumX.Exists(driver, By.XPath("//*[contains(.,'Waiting for approval')]"), 1) ||
                SeleniumX.Exists(driver, By.XPath("//*[contains(.,'Check your notifications on another device')]"), 1);
        }
        public static string GetName(IWebDriver driver)
        {
            string name = "";
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_MOBILE_URL + "/profile.php");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);

            try
            {
                name = driver.Title.Trim();
            }
            catch (Exception) { }

            return name;
        }
        public static int GetFriends(IWebDriver driver)
        {
            string friends = "";
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_MOBILE_URL + "/profile.php");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);

            try
            {
                friends = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div/div[2]/div[1]/div/div/div[1]/div[2]")).GetAttribute("innerHTML").Trim();
            }
            catch (Exception) { }
            if (!string.IsNullOrEmpty(friends))
            {
                string[] arr = friends.Split(' ');
                try
                {
                    friends = arr[0].Replace('.',' ').Trim();
                }
                catch (Exception) { }
            }
            try
            {
                return Int32.Parse(friends);
            }
            catch (Exception) { }

            return 0;
        }
        public static string GetGender(IWebDriver driver)
        {
            string gender = "";
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_MOBILE_URL + "/me/about");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            IWebElement elm = null;
            try
            {
                gender = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div[1]/div/div[1]/div[4]/div/div[2]/div/div[1]")).GetAttribute("innerHTML").Trim();
            }
            catch (Exception) { }

            return gender;
        }
        public static int GetFriendsRequest(IWebDriver driver)
        {
            string str = "";
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_MOBILE_URL + "/friends/requests");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);

            try
            {
                str = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div[2]/div[1]/header/h3/span")).GetAttribute("innerHTML").Trim();
            }
            catch (Exception) { }
            if (!string.IsNullOrEmpty(str))
            {
                string[] arr = str.Split(' ');
                try
                {
                    return Int32.Parse(arr[0].Replace('.',' ').Trim());
                }
                catch (Exception) { }
            }

            return 0;
        }
        public static string GetBirthday(IWebDriver driver)
        {
            string birthday = "";
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_MOBILE_URL + "/profile/edit/infotab/section/forms/?section=basic-info");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            IWebElement d_e = null;
            IWebElement m_e = null;
            IWebElement y_e = null;
            try
            {
                d_e = driver.FindElement(By.XPath("//select[@name='birthday_day']"));
            }
            catch (Exception) { }
            try
            {
                m_e = driver.FindElement(By.XPath("//select[@name='birthday_month']"));
            }
            catch (Exception) { }
            try
            {
                y_e = driver.FindElement(By.XPath("//select[@name='birthday_year']"));
            }
            catch (Exception) { }
            // get date of birth
            string m = "", d = "", y = "";
            try
            {
                d = d_e.FindElement(By.XPath("//option[@selected='1']")).GetAttribute("innerHTML").Trim();
                m = m_e.FindElement(By.XPath("//option[@selected='1']")).GetAttribute("innerHTML").Trim();
                y = y_e.FindElement(By.XPath("//option[@selected='1']")).GetAttribute("innerHTML").Trim();
            }
            catch (Exception) { }
            if (!string.IsNullOrEmpty(d) && !string.IsNullOrEmpty(m) && !string.IsNullOrEmpty(y))
            {
                m = GetMonthFromString(m);

                birthday = d + "/" + m + "/" + y;
            }

            return birthday;
        }
        public static void PrivacyPublicPost(IWebDriver driver)
        {
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_MOBILE_URL + "/privacy/touch/composer/selector/?logging_source=composer_options");
            }
            catch { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            Boolean is_working = false;
            try
            {
                driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div/div/div/div[4]/form/div/fieldset/label[1]")).Click();
                is_working = true;
            }
            catch { }
            if (!is_working)
            {
                try
                {
                    driver.FindElement(By.XPath("//input[@name='privacyx']")).Click();
                    is_working = true;
                }
                catch { }
            }
            if (!is_working)
            {
                try
                {
                    driver.FindElement(By.XPath("//a[@aria-label='Public']")).Click();
                    is_working = true;
                }
                catch { }
            }
            if (!is_working)
            {
                try
                {
                    driver.FindElement(By.XPath("//span[contains(text(),'Public')]")).Click();
                    is_working = true;
                }
                catch (Exception) { }
            }
            if (!is_working)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div[1]/div/div/div[3]/form/div/fieldset/label[1]/div/div[1]")).Click();
                    is_working = true;
                }
                catch { }
            }
        }
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
        public static void LeaveGroup(IWebDriver driver, bool isBack = false)
        {
            bool isWorking = false;
            int counter = 10;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div[1]/div[2]/div/div[1]/div/div[1]/a")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//a[@aria-label='Joined']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[contains(text(),'Joined')]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (isWorking)
            {
                Thread.Sleep(2000);
                isWorking = false;
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div[3]/div/div/div[2]/a[4]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div[2]/div/div/div[2]/a[4]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//*[contains(text(),'Leave Group')]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[contains(text(),'Leave group')]")).Click();
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
                            driver.FindElement(By.XPath("//button[@value='Leave group']")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/form/article/div/div/div[4]/div/button")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//*[contains(text(),'Leave group')]")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                    } while (!isWorking && counter-- > 0);
                    if (isWorking)
                    {
                        counter = 7;
                        isWorking = false;
                        do
                        {
                            Thread.Sleep(1000);
                            try
                            {
                                driver.FindElement(By.XPath("//button[contains(text(),'Join Group')]"));
                                isWorking = true;
                            }
                            catch (Exception) { }
                            if (!isWorking)
                            {
                                try
                                {
                                    driver.FindElement(By.XPath("//*[contains(text(),'Join Group')]"));
                                    isWorking = true;
                                }
                                catch (Exception) { }
                            }
                            if (!isWorking)
                            {
                                try
                                {
                                    driver.FindElement(By.XPath("//*[contains(text(),'Join group')]"));
                                    isWorking = true;
                                }
                                catch (Exception) { }
                            }
                        } while (!isWorking && counter-- > 0);
                    }
                }
                if (isBack)
                {
                    driver.Navigate().Back();
                }
            }
        }
        public static void Profile(IWebDriver driver, string profilePath)
        {
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_MOBILE_URL + "/profile.php");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);

            WebFBTool.UsePage(driver,2,500);

            bool isWorking = false;
            int counter = 8;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//a[@aria-label='Edit Profile Picture']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//a[@aria-label='Edit profile picture']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//a[text() = 'Edit Profile Picture']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//a[text() = 'Add Profile Picture']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Update profile picture']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Update Profile Picture']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return;
            }
            isWorking = false;
            counter = 8;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.Id("nuxPicFileInput")).SendKeys(profilePath);
                    Thread.Sleep(1500);
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.Id("nuxPicFileInput")).SendKeys(profilePath);
                        Thread.Sleep(1500);
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div[3]/div[1]/div[1]/input")).SendKeys(profilePath);
                        Thread.Sleep(1500);
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div[3]/div[1]/div[1]/input")).SendKeys(profilePath);
                        Thread.Sleep(1500);
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//input[@type='file']")).SendKeys(profilePath);
                        Thread.Sleep(1500);
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            isWorking = false;
            counter = 6;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.Id("nuxUploadPhotoButton")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//input[@value='Upload']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//input[@value='Save']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Save']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (isWorking)
            {
                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(4000);
            }
        }
        public static void Cover(IWebDriver driver, string coverPath)
        {
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_MOBILE_URL + "/profile.php");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            bool isWorking = false;
            int counter = 8;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//span[text() = 'Add Photo']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if(!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[text() = 'Add photo']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//a[@aria-label='Edit Cover Photo']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//a[text() = 'Edit Cover Photo']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[text() = 'Edit Cover Photo']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Edit Cover Photo']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Edit cover photo']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//a[text() = 'Add Cover Photo']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[text() = 'Add Cover Photo']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[text() = 'Add Cover Photo']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return;
            }
            isWorking = false;
            counter = 10;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.Id("nuxPicFileInput")).SendKeys(coverPath);
                    Thread.Sleep(1500);
                    isWorking = true;
                }
                catch (Exception) { }
                if(!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[2]/div[2]/div[2]/div/div[1]")).Click();
                    }
                    catch (Exception) { }
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[2]/div/div[1]")).Click();
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[text() = 'Add photo']")).Click();
                        Thread.Sleep(1500);
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//a[text() = 'Add photo']")).Click();
                        Thread.Sleep(1500);
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//a[@href='/photos/upload/?cover_photo']")).Click();
                        Thread.Sleep(1500);
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//input[@type='file']")).SendKeys(coverPath);
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if(!isWorking)
            {
                return;
            }
            isWorking = false;
            counter = 8;
            do
            {
                Thread.Sleep(500);
                IWebElement element = null;
                try
                {
                    element= driver.FindElement(By.Id("nuxUploadPhotoButton"));
                    isWorking = true;
                }
                catch (Exception) { }
                if(!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//input[@value='Upload']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div/div[1]/div[1]/div/div/div/div[2]/div/div[2]/div[2]/div[1]"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div/div[1]/div[1]/div/div/div/div[2]/div/div[2]/div[2]/div[1]"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//input[@value='Save']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//div[@aria-label='Save changes']"));
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
                        element = driver.FindElement(By.XPath("//span[text() = 'Save Changes']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//span[text() = 'Save changes']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if(isWorking)
                {
                    WebFBTool.ClickElement(driver, element);
                }
            } while (!isWorking && counter-- > 0);
            if(isWorking)
            {
                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(2000);
            }
        }
        public static void Bio(IWebDriver driver, string bio)
        {
            int counter;
            bool isWorking = false;
            try
            {
                driver.Navigate().GoToUrl("https://mobile.facebook.com/profile/intro/edit/public");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            counter = 6;
            isWorking = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div[1]/div/div/div/div/div[5]/div[1]/div[2]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[text() = 'Describe yourself...']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            counter = 8;
            isWorking = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//textarea[@name='bio']")).SendKeys(bio);
                    isWorking = true;
                }
                catch (Exception) { }
            } while (!isWorking && counter-- > 0);
            Thread.Sleep(1000);
            try
            {
                driver.FindElement(By.XPath("//button[@value='SAVE']")).Click();
                Thread.Sleep(1500);
            }
            catch (Exception) { }
        }
        public static bool School(IWebDriver driver, string text)
        {
            bool isWorking = true;
            int counter = 8;
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_MOBILE_URL + "/profile/intro/edit/about/");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            FBTool.Scroll(driver, 1000, false);
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Add a high school']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if(!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Add high school']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Add High School']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[text() = 'Add high school']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if(isWorking)
            {
                SetInfo(driver, "hs_school_text", text);
            }

            return isWorking;
        }
        public static bool College(IWebDriver driver, string text)
        {
            bool isWorking = true;
            int counter = 8;
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_MOBILE_URL + "/profile/intro/edit/about/");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            FBTool.Scroll(driver, 2000, false);
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Add a college']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if(!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Add college']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Add College']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[text() = 'Add college']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (isWorking)
            {
                SetInfo(driver, "college_school_text", text);
            }

            return isWorking;
        }
        public static bool City(IWebDriver driver, string text)
        {
            bool isWorking = true;
            int counter = 8;
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_MOBILE_URL + "/profile/intro/edit/about/");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            FBTool.Scroll(driver, 2000, false);
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Add current city']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[text() = 'Add current city']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (isWorking)
            {
                SetInfo(driver, "current_city_text", text);
            }

            return isWorking;
        }
        public static bool Hometown(IWebDriver driver, string text)
        {
            bool isWorking;
            int counter;
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_MOBILE_URL + "/profile/intro/edit/about/");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            FBTool.Scroll(driver, 2000, false);
            isWorking = false;
            counter = 8;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Add hometown']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking) 
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Add home town']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (isWorking)
            {
                SetInfo(driver, "hometown_text", text);
            }

            return isWorking;
        }
        public static bool SetInfo(IWebDriver driver, string tagName, string text)
        {
            IWebElement element = null;
            int counter = 8;
            bool isWorking = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    element = driver.FindElement(By.XPath("//input[@name='" + tagName + "']"));
                    isWorking = true;
                }
                catch (Exception) { }
            } while (element == null && counter-- > 0);
            if (!isWorking)
            {
                return isWorking;
            }
            try
            {
                element.SendKeys(text);
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
            try
            {
                driver.FindElement(By.XPath("//button[@name='save']")).Click();
                isWorking = true;
            }
            catch (Exception) { }
            if (isWorking)
            {
                counter = 4;
                isWorking = false;
                do
                {
                    Thread.Sleep(500);
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div/div[3]/button")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                } while (!isWorking && counter-- > 0);

                FBTool.WaitingPageLoading(driver);
            }

            return isWorking;
        }
        public static bool AddFriend(IWebDriver driver)
        {
            int counter = 3;
            bool isWorking = false;
            do
            {
                Thread.Sleep(500);
                FBTool.Close(driver);
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
                        driver.FindElement(By.XPath("//*[contains(text(),'Add Friend')]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//*[contains(text(),'Add friend')]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                //if (!isWorking)
                //{
                //    try
                //    {
                //        driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div[1]/div/div[1]/div/div[3]/div/div[1]/a")).Click();
                //        isWorking = true;
                //    }
                //    catch (Exception) { }
                //}
            } while (!isWorking && counter-- > 0);
            if (isWorking)
            {
                Thread.Sleep(2000);
            }

            return isWorking;
        }
        public static bool JoinGroup(IWebDriver driver,string answer = "Okay")
        {
            int counter;
            bool isWorking = false;
            try
            {
                driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div/div[2]/div/div[1]/div/button")).Click();
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
                    driver.FindElement(By.XPath("//div[@aria-label='Join Group']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            //if (!isWorking)
            //{
            //    try
            //    {
            //        driver.FindElement(By.XPath("//div[@aria-label='Follow Comu']")).Click();
            //        isWorking = true;
            //    }
            //    catch (Exception) { }
            //}
            //if (!isWorking)
            //{
            //    try
            //    {
            //        driver.FindElement(By.XPath("//div[@aria-label='Join Group']")).Click();
            //        isWorking = true;
            //    }
            //    catch (Exception) { }
            //}
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("//button[contains(text(),'Join Group')]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("//input[@value='Join Group']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            bool success = isWorking;
            if (isWorking)
            {
                isWorking = false;
                counter = 10;
                if(string.IsNullOrEmpty(answer))
                {
                    answer = "Okay";
                }

                do
                {
                    Thread.Sleep(500);
                    try
                    {
                        driver.FindElement(By.TagName("textarea")).SendKeys(answer);
                        isWorking = true;
                    }
                    catch (Exception) { }
                } while (!isWorking && counter-- > 0);
                if (isWorking)
                {
                    isWorking = false;
                    try
                    {
                        Thread.Sleep(1000);
                        driver.FindElement(By.XPath("//button[@type='submit']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                    if (!isWorking)
                    {
                        try
                        {
                            Thread.Sleep(1000);
                            driver.FindElement(By.XPath("//input[@value='Send Response']")).Click();
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
                    }
                    Thread.Sleep(2000);
                }
            }

            return success;
        }
        public static bool LikePage(IWebDriver driver)
        {
            bool isWorking = false;
            int counter = 3;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div[1]/div/div[1]/div/div[2]/div/div/div[3]/div/div[1]/div/div/div[2]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
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
                        driver.FindElement(By.XPath("//div[@aria-label='Follow']")).Click();
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
        public static void ChangeLanguage(IWebDriver driver)
        {
            try
            {
                driver.Navigate().GoToUrl("https://mbasic.facebook.com/language.php");
            }
            catch (Exception) { }
            bool isWorking = false;
            int counter = 6;
            do
            {
                Thread.Sleep(1000);
                try
                {
                    driver.FindElement(By.XPath("//span[@value='en_US']")).Click();
                    isWorking=true;
                }
                catch (Exception) { }
                if(!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//input[@value='English (US)']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if(!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//*[contains(text(),'English')]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//*[contains(text(),'English (US)')]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter -- > 0);
            if(isWorking)
            {
                Thread.Sleep(2500);
            }
        }
        public static void MobileRemove(IWebDriver driver, string password)
        {
            try
            {
                driver.Navigate().GoToUrl("https://m.facebook.com/settings/sms");
            }
            catch (Exception) { }

            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1500);

            bool isWorking = false;
            try
            {
                driver.FindElement(By.XPath("//a[contains(text(),'Remove')]")).Click();
                isWorking = true;
            }
            catch (Exception) { }
            if (!isWorking)
            {
                // free
                try
                {
                    driver.FindElement(By.XPath("/html/body/div/div/div[3]/div/div[1]/div/div[1]/div[2]/div[2]/a")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                // mobile
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div[1]/div/div[1]/div/div[1]/div[2]/a")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (isWorking)
            {
                Thread.Sleep(2000);
                isWorking = false;
                try
                {
                    driver.FindElement(By.XPath("//input[@name='remove_phone_warning_acknwoledged']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//button[@value='Remove Number']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[contains(text(),'Remove Number')]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    // free
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div/div/div[3]/div[2]/form/div/div[3]/table/tbody/tr/td/input")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    // mobile
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/form/div/div[3]/div/label/input")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            }
            if (isWorking)
            {
                Thread.Sleep(2000);
                try
                {
                    driver.FindElement(By.XPath("//input[@name='save_password']")).SendKeys(password + OpenQA.Selenium.Keys.Enter);
                    isWorking = true;
                }
                catch (Exception) { }
                Thread.Sleep(3000);
            }
            if(!isWorking)
            {
                Thread.Sleep(2500);
            }
        }
        public static void MobileLogOutDevice(IWebDriver driver)
        {
            bool isWorking = false;
            try
            {
                driver.Navigate().GoToUrl("https://m.facebook.com/settings/security_login/sessions/log_out_all/confirm/");
            }
            catch (Exception) { }

            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1500);

            try
            {
                driver.FindElement(By.XPath("//a[@data-sigil='touchable']")).Click();
                isWorking = true;
            }
            catch (Exception) { }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div/div/div[3]/div/table/tbody/tr/td/div[2]/a[1]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("//span[contains(text(),'Log Out')]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            }
            Thread.Sleep(2000);
        }
        public static int GetTotalFriends(IWebDriver driver)
        {
            int total = 0;
            try
            {
                driver.Navigate().GoToUrl("https://mobile.facebook.com/friends/center/friends");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            try
            {
                string friend = driver.FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div[1]/div[1]/header/h3")).GetAttribute("innerHTML");

                string[] arr = friend.Split(' ');
                if (arr.Length > 0)
                {
                    char slug = ',';
                    if(arr[0].Contains("."))
                    {
                        slug = '.';
                    }
                    string[] a = arr[0].Split(slug);
                    string s = a[0];
                    if (a.Length > 1)
                    {
                        s = s + a[1];
                    }
                    total = Int32.Parse(s.Trim());
                }
            }
            catch (Exception) { }

            return total;
        }

        public static void ChangeTime(IWebDriver driver)
        {
            try
            {
                driver.Navigate().GoToUrl("https://m.facebook.com/settings/sms");
            }
            catch (Exception) { }

            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            int counter = 6;
            bool isWorking = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.Id("sms_start")).Click();
                    Thread.Sleep(1500);
                    isWorking = true;
                }
                catch (Exception) { }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return;
            }
            isWorking = false;
            counter = 6;
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
            isWorking = false;
            counter = 6;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.Id("sms_stop")).Click();
                    Thread.Sleep(1500);
                    isWorking = true;
                }
                catch (Exception) { }
            } while (!isWorking && counter-- > 0);
            if (isWorking)
            {
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
            Thread.Sleep(2000);
        }
    }
}
