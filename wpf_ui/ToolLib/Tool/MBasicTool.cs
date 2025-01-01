using OpenQA.Selenium;
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
using WpfUI.ViewModels;

namespace ToolKHBrowser.ToolLib.Tool
{
    public static class MBasicTool
    {
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        public static string LoggedIn(IWebDriver driver, FbAccount data, bool isLoginByCookie = false, string url = "")
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
                    //FBTool.LoginByCookie(driver, data.Cookie);
                    FBTool.LoginFbByCookie(driver, data.Cookie);

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

            return FBTool.GetResults(driver);
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
            bool isWorking = false;
            string code = "";
            int counter = 4;

            do
            {
                Thread.Sleep(500);
                code = TwoFactorRequest.GetPassCode(data.TwoFA);
            } while (string.IsNullOrEmpty(code) && counter-- > 0);
            try
            {
                Thread.Sleep(1000);
                driver.FindElement(By.XPath("//input[@name='approvals_code']")).SendKeys(code + OpenQA.Selenium.Keys.Enter);
                isWorking = true;
            }
            catch (Exception) { }
            if (!isWorking)
            {
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[3]/form/div/article/div[1]/table/tbody/tr/td/button")).SendKeys(code + OpenQA.Selenium.Keys.Enter);
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (isWorking)
            {
                Boolean b = true;
                try
                {
                    Thread.Sleep(1500);
                    driver.FindElement(By.Id("checkpointSubmitButton-actual-button")).Click();
                }
                catch (Exception) { b = false; }
                if (b)
                {
                    try
                    {
                        Thread.Sleep(1000);
                        driver.FindElement(By.Id("checkpointSubmitButton-actual-button")).Click();
                    }
                    catch (Exception) { }
                    try
                    {
                        Thread.Sleep(1000);
                        driver.FindElement(By.Id("checkpointSubmitButton-actual-button")).Click();
                    }
                    catch (Exception) { }
                    try
                    {
                        Thread.Sleep(1000);
                        driver.FindElement(By.Id("checkpointSubmitButton-actual-button")).Click();
                    }
                    catch (Exception) { }
                    try
                    {
                        Thread.Sleep(1000);
                        driver.FindElement(By.Id("checkpointSubmitButton-actual-button")).Click();
                    }
                    catch (Exception) { }
                }
            }

            return isWorking;
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
