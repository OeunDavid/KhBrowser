using OpenPop.Mime;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Markup;
using ToolKHBrowser.ToolLib.Data;
using ToolKHBrowser.ToolLib.Tool;
using ToolLib.Data;
using ToolLib.Tool;
using WpfUI.ViewModels;
using WpfUI.Views;
using static Emgu.CV.Stitching.Stitcher;

namespace ToolKHBrowser.ViewModels
{
    public interface IProfileViewModel
    {
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void Start(frmMain form, IWebDriver driver, FbAccount data);
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void Delete();
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void PublicPost();
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void TwoFA();
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void Info();
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void Password();
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void LockTime();
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void LogoutAllDevice();
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void ActivityLog();
    }
    public class ProfileViewModel : IProfileViewModel
    {
        private IAccountDao accountDao;
        private ICacheDao cacheDao;
        private frmMain form;
        private FbAccount data;
        private IWebDriver driver;
        private ProcessActions processActionData;

        private string[] bioArr;
        private string[] cityArr;
        private string[] hometownArr;
        private string[] schoolArr;
        private string[] collegeArr;

        private Random random;
        private int profileIndex;
        private int coverIndex;

        public ProfileViewModel(IAccountDao accountDao, ICacheDao cacheDao)
        {
            this.accountDao = accountDao;
            this.cacheDao = cacheDao;
        }
        public void Start(frmMain form, IWebDriver driver, FbAccount data)
        {
            this.form = form;
            this.data = data;
            this.driver = driver;
            this.processActionData = this.form.processActionsData;

            try
            {
                var cacheP = this.form.cacheViewModel.GetCacheDao().Get("profile:config:profile_index");
                if (cacheP != null && cacheP.Value != null)
                {
                    profileIndex = Int32.Parse(cacheP.Value.ToString());
                }
            }
            catch (Exception) { }
            try
            {
                var cacheC = this.form.cacheViewModel.GetCacheDao().Get("profile:config:cover_index");
                if (cacheC != null && cacheC.Value != null)
                {
                    coverIndex = Int32.Parse(cacheC.Value.ToString());
                }
            }
            catch (Exception) { }
            this.processActionData = this.form.processActionsData;

            bioArr = Array.Empty<string>();
            cityArr = Array.Empty<string>();
            hometownArr = Array.Empty<string>();
            schoolArr = Array.Empty<string>();
            collegeArr = Array.Empty<string>();

            if (processActionData != null && processActionData.ProfileConfig != null && processActionData.ProfileConfig.NewInfo != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(processActionData.ProfileConfig.NewInfo.Bio))
                        bioArr = processActionData.ProfileConfig.NewInfo.Bio.Split('\n');
                }
                catch (Exception) { }
                try
                {
                    if (!string.IsNullOrEmpty(processActionData.ProfileConfig.NewInfo.City))
                        cityArr = processActionData.ProfileConfig.NewInfo.City.Split('\n');
                }
                catch (Exception) { }
                try
                {
                    if (!string.IsNullOrEmpty(processActionData.ProfileConfig.NewInfo.Hometown))
                        hometownArr = processActionData.ProfileConfig.NewInfo.Hometown.Split('\n');
                }
                catch (Exception) { }
                try
                {
                    if (!string.IsNullOrEmpty(processActionData.ProfileConfig.NewInfo.School))
                        schoolArr = processActionData.ProfileConfig.NewInfo.School.Split('\n');
                }
                catch (Exception) { }
                try
                {
                    if (!string.IsNullOrEmpty(processActionData.ProfileConfig.NewInfo.College))
                        collegeArr = processActionData.ProfileConfig.NewInfo.College.Split('\n');
                }
                catch (Exception) { }
            }

            random = new Random();
        }
        public void ActivityLog()
        {
            if (processActionData == null || processActionData.ProfileConfig == null || processActionData.ProfileConfig.ActivityLog == null)
                return;

            int num = 0;
            try
            {
                num = processActionData.ProfileConfig.ActivityLog.GroupPostsAndComments;
            } catch (Exception) { }
            if(num <= 0)
            {
                return;
            }
            // update message
            data.Description += ", Delete Activity Logs";

            int counter;
            bool isWorking = false;
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/" + data.UID + "/allactivity?activity_history=false&category_key=GROUPPOSTS&manage_mode=false&should_load_landing_page=false");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);

            int counterError = 4, index = 0, totalDelete = 0;
            string des = data.Description;

            for (int i = 1; i <= num; i++)
            {
                isWorking = false;
                counter = 4;
                FBTool.Close(driver);
                do
                {
                    if (this.form.IsStop())
                    {
                        break;
                    }
                    Thread.Sleep(500);
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Action options']")).Click();
                        isWorking = true;
                        counterError = 10;
                    }
                    catch (Exception) { }
                    if(!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//div[@aria-label='Action Options']")).Click();
                            isWorking = true;
                            counterError = 10;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
                index++;
                if (isWorking)
                {
                    isWorking = false;
                    counter = 6;
                    do
                    {
                        if (this.form.IsStop())
                        {
                            break;
                        }
                        Thread.Sleep(500);
                        try
                        {
                            driver.FindElement(By.XPath("//span[contains(text(),'Delete')]")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//div[contains(text(),'Delete')]")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//span[contains(text(),'Unlike')]")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//span[contains(text(),'Remove Tag')]")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//span[contains(text(),'Remove tag')]")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//span[contains(text(),'Move to trash')]")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//span[contains(text(),'Move to Trash')]")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//span[contains(text(),'Move to archive')]")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//span[contains(text(),'Move to Archive')]")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//span[contains(text(),'Remove Reaction')]")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//span[contains(text(),'Remove reaction')]")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                    } while (!isWorking && counter-- > 0);

                    if (isWorking)
                    {
                        isWorking = false;
                        counter = 6;
                        do
                        {
                            if (this.form.IsStop())
                            {
                                break;
                            }
                            Thread.Sleep(500);
                            try
                            {
                                driver.FindElement(By.XPath("//div[@aria-label='Delete']")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                            if (!isWorking)
                            {
                                try
                                {
                                    driver.FindElement(By.XPath("//div[@aria-label='Remove']")).Click();
                                    isWorking = true;
                                }
                                catch (Exception) { }
                            }
                            if (!isWorking)
                            {
                                try
                                {
                                    driver.FindElement(By.XPath("//div[@aria-label='Move to Trash']")).Click();
                                    isWorking = true;
                                }
                                catch (Exception) { }
                            }
                        } while (!isWorking && counter-- > 0);
                        if (isWorking)
                        {
                            totalDelete++;
                            data.Description = des + " " + totalDelete;
                        }
                    }
                }
                else
                {
                    counterError--;
                }
                if (counterError <= 0)
                {
                    break;
                }
                else
                {
                    if (num > i && !isWorking)
                    {
                        if (this.form.IsStop())
                        {
                            break;
                        }
                        index = -1;
                        driver.Navigate().Refresh();
                        if (this.form.IsStop())
                        {
                            break;
                        }
                        FBTool.WaitingPageLoading(driver);
                        Thread.Sleep(1000);
                    }
                }
                Thread.Sleep(1000);
            }

            // update message
            accountDao.updateStatus(data.UID, data.Description, 1);
        }
        public void LockTime()
        {
            MobileFBTool.ChangeTime(driver);
        }
        public void LogoutAllDevice()
        {
            MobileFBTool.MobileLogOutDevice(driver);
        }
        public void PublicPost()
        {
            MobileFBTool.PrivacyPublicPost(driver);
        }
        public void TwoFA()
        {
            if (processActionData == null || processActionData.ProfileConfig == null || processActionData.ProfileConfig.TwoFA == null)
                return;

            if (data.TwoFA.Length < 20)
            {
                data.Description = "Turn on 2FA";
                if (!IsStop() && processActionData.ProfileConfig.TwoFA.Web)
                {
                    TurnOn2FAOnWeb();
                }
                else if(!IsStop() && processActionData.ProfileConfig.TwoFA.WebII)
                {
                    TurnOn2FAOnWebII();
                }
                else if (!IsStop() && processActionData.ProfileConfig.TwoFA.Mbasic)
                {
                    TurnOn2FAOnMobile();
                }
                accountDao.updateStatus(data.UID, data.DeviceId, 1);
            }
        }
        public void Password()
        {
            data.Description = "New Password";
            SlugHacked();
            accountDao.updateStatus(data.UID, data.DeviceId, 1);
        }
        public void Info()
        {
            if (processActionData == null || processActionData.ProfileConfig == null || processActionData.ProfileConfig.NewInfo == null)
                return;

            data.Description = "Update Info";
            if(!string.IsNullOrEmpty(processActionData.ProfileConfig.NewInfo.SourceProfile))
            {
                string source = GetProfileFullPath();
                MobileFBTool.Profile(driver, source);
            }
            if (!string.IsNullOrEmpty(processActionData.ProfileConfig.NewInfo.SourceCover))
            {
                string source = GetCoverFullPath();
                MobileFBTool.Cover(driver, source);
            }
            if (bioArr.Length > 0)
            {
                string str = GetRankValueFromArr(bioArr);
                if (!string.IsNullOrEmpty(str))
                {
                    MobileFBTool.Bio(driver, str);
                }
            }
            if (cityArr.Length > 0)
            {
                string str = GetRankValueFromArr(cityArr);
                if (!string.IsNullOrEmpty(str))
                {
                    MobileFBTool.City(driver, str);
                }
            }
            if (hometownArr.Length > 0)
            {
                string str = GetRankValueFromArr(hometownArr);
                if (!string.IsNullOrEmpty(str))
                {
                    MobileFBTool.Hometown(driver, str);
                }
            }
            if (schoolArr.Length > 0)
            {
                string str = GetRankValueFromArr(schoolArr);
                if (!string.IsNullOrEmpty(str))
                {
                    MobileFBTool.School(driver, str);
                }
            }
            if (collegeArr.Length > 0)
            {
                string str = GetRankValueFromArr(collegeArr);
                if (!string.IsNullOrEmpty(str))
                {
                    MobileFBTool.College(driver, str);
                }
            }
            accountDao.updateStatus(data.UID, data.DeviceId, 1);
        }
        public string GetProfileFullPath()
        {
            return this.form.GetSourceProfile() ;
        }
        public string GetCoverFullPath()
        {
            return this.form.GetSourceCover() ;
        }
        public void Delete()
        {
            if (processActionData == null || processActionData.ProfileConfig == null || processActionData.ProfileConfig.DeleteData == null)
                return;

            data.Description = "Delete Data";
            if(!IsStop() && processActionData.ProfileConfig.DeleteData.Unfriend > 0)
            {
                Unfriends(processActionData.ProfileConfig.DeleteData.Unfriend);
            }
            if(!IsStop() && processActionData.ProfileConfig.DeleteData.Suggest > 0)
            {
                DeleteFriendsSuggest(processActionData.ProfileConfig.DeleteData.Suggest);
            }
            if(!IsStop() && processActionData.ProfileConfig.DeleteData.Request > 0)
            {
                DeleteFriendsRequest(processActionData.ProfileConfig.DeleteData.Request);
            }
            if(!IsStop() && processActionData.ProfileConfig.DeleteData.Profile)
            {
                DeleteProfilePictures();
            }
            if(!IsStop() && processActionData.ProfileConfig.DeleteData.Cover)
            {
                DeleteCoverPictures();
            }
            if(!IsStop() && processActionData.ProfileConfig.DeleteData.Picture)
            {
                DeleteAllPictures();
            }
            if(!IsStop() && processActionData.ProfileConfig.DeleteData.Tag)
            {
                DeleteAllPost();
            }
            if(!IsStop() && processActionData.ProfileConfig.DeleteData.Phone)
            {
                MobileFBTool.MobileRemove(driver, data.Password);
            }

            accountDao.updateStatus(data.UID, data.DeviceId, 1);
        }
        public string GetRankValueFromArr(string[] arr)
        {
            string str = "";
            try
            {
                if (arr != null && arr.Length > 0)
                {
                    str = arr[random.Next(0, arr.Length)];
                }
            }
            catch (Exception) { }

            return str;
        }
        public void TurnOn2FAOnWebII()
        {
            try
            {
                driver.Navigate().GoToUrl("https://accountscenter.facebook.com/password_and_security/two_factor");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            bool isWorking = false;
            int counter = 8;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[3]/div[2]/div[4]/div/div/div[2]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@role='img']")).Click() ;
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[5]/div[2]/div[1]/div/div/div[2]/div/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0 && !IsStop());
            if(!isWorking || IsStop())
            {
                return;
            }
            isWorking = false;
            counter = 8;
            bool isEnterPassword = false;
            do
            {
                Thread.Sleep(500);
                if (processActionData.ProfileConfig.TwoFA.EnterPassword && !isEnterPassword)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//input[@type='password']")).SendKeys(data.Password);
                        Thread.Sleep(500);
                        driver.FindElement(By.XPath("//span[contains(text(),'Submit')]")).Click();
                        Thread.Sleep(2000);
                        isEnterPassword = true;
                    }
                    catch (Exception) { }
                }
                try
                {
                    driver.FindElement(By.XPath("//span[contains(text(),'Next')]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if(!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div[2]/div/div[4]/div[3]/div/div/div/div/div/div/div/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0 && !IsStop());
            isWorking = false;
            counter = 10;
            string twofa = "";
            do
            {
                if (IsStop())
                {
                    break;
                }
                Thread.Sleep(500);
                try
                {
                    var element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div[3]/div/div[3]/div[2]/div[4]/div/div/div[4]/div/div[2]/div/div/div[1]/span"));
                    var text_code = element.GetAttribute("innerHTML");
                    if (!string.IsNullOrEmpty(text_code) && text_code.Length > 20)
                    {
                        twofa = text_code.Trim();
                    } else
                    {
                        twofa = "";
                    }
                }
                catch (Exception) { }
                if(string.IsNullOrEmpty(twofa))
                {
                    try
                    {
                        var element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div[3]/div/div[5]/div[2]/div[1]/div/div/div[4]/div/div[2]/div/div/div[1]/span"));
                        var text_code = element.GetAttribute("innerHTML");
                        if (!string.IsNullOrEmpty(text_code) && text_code.Length > 20)
                        {
                            twofa = text_code.Trim();
                        } else
                        {
                            twofa = "";
                        }
                    }
                    catch (Exception) { }
                }
            } while (string.IsNullOrEmpty(twofa) && counter-- > 0);
            if(!string.IsNullOrEmpty(twofa))
            {
                if(IsStop())
                {
                    return;
                }
                isWorking = false;
                try
                {
                    driver.FindElement(By.XPath("//span[contains(text(),'Next')]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if(!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div[3]/div/div[6]/div[3]/div/div/div/div/div/div/div/div/div[1]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if(!isWorking || IsStop())
                {
                    return;
                }
                twofa = Regex.Replace(twofa, @"\s+", "");
                string code = "";
                counter = 5;
                do
                {
                    code = TwoFactorRequest.GetPassCode(twofa);
                } while (string.IsNullOrEmpty(code) && counter-- > 0 && !IsStop());

                if (!string.IsNullOrEmpty(code))
                {
                    isWorking = false;
                    counter = 4;
                    do
                    {
                        Thread.Sleep(500);
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div[4]/div/div[3]/div[2]/div[4]/div/div/div[2]/div/div/input")).SendKeys(code);
                            isWorking = true;
                        }
                        catch (Exception) { }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div[4]/div/div[5]/div[2]/div[1]/div/div/div[2]/div/div/input")).SendKeys(code);
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//input[@maxlength='6']")).SendKeys(code);
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if(isWorking)
                        {
                            Thread.Sleep(1000);
                            isWorking = false;
                            try
                            {
                                driver.FindElement(By.XPath("//span[contains(text(),'Next')]")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                            if (!isWorking)
                            {
                                try
                                {
                                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div[4]/div/div[6]/div[3]/div/div/div/div/div/div/div/div/div[1]/div")).Click();
                                    isWorking = true;
                                }
                                catch (Exception) { }
                            }
                        }
                    } while (!isWorking && counter-- > 0);

                    if (isWorking)
                    {
                        Thread.Sleep(1000);
                        data.TwoFA = twofa;
                        accountDao.TwoFA(data.UID, twofa);
                        Thread.Sleep(2500);
                    }
                }
            }
        }
        public void TurnOn2FAOnWeb()
        {
            try
            {
                driver.Navigate().GoToUrl("https://facebook.com/privacy/checkup/?source=settings_and_privacy");
            }
            catch (Exception) { }

            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            WebFBTool.CloseAllPopup(driver);

            bool isWorking = false;
            int counter = 8;
            do
            {
                if (IsStop())
                {
                    break;
                }
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
            } while (!isWorking && counter-- > 0);
            if (IsStop())
            {
                return;
            }
            if (isWorking)
            {
                counter = 8;
                isWorking = false;
                do
                {
                    if (IsStop())
                    {
                        break;
                    }
                    Thread.Sleep(500);
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/div[2]/div[2]/div/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
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
            }
            if (IsStop())
            {
                return;
            }
            if (isWorking)
            {
                // step password updated
                isWorking = false;
                Thread.Sleep(2000);
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[2]/div/div[4]/div[2]/div[2]/div[1]/div")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[2]/div/div/div[1]/div/div[2]/div/div/div/div/div[2]/div/div[4]/div[2]/div[2]/div[1]/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Next']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            }
            if (IsStop() || !isWorking)
            {
                return;
            }
            if (isWorking)
            {
                isWorking = false;
                counter = 10;
                do
                {
                    Thread.Sleep(500);
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[3]/div/div[4]/div[1]/div[1]/div/div[3]/div/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//div[@aria-label='Get Started']")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
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
                            driver.FindElement(By.XPath("//a[@aria-label='Get Started']")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//a[@aria-label='Get started']")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
            }
            if (isWorking)
            {
                bool b = false;
                counter = 5;
                do
                {
                    Thread.Sleep(500);
                    try
                    {
                        driver.FindElement(By.XPath("//a[@href='/security/2fac/setup/qrcode/generate/']")).Click();
                        b = true;
                    }
                    catch (Exception) { }
                } while (!b && counter-- > 0);
            }
            if (IsStop() || !isWorking)
            {
                return;
            }
            if (isWorking)
            {
                //counter = 15;
                //isWorking = false;
                //do
                //{
                //    if (IsStop())
                //    {
                //        break;
                //    }
                //    Thread.Sleep(500);
                //    try
                //    {
                //        driver.SwitchTo().Frame(0);
                //        isWorking = true;
                //    }
                //    catch (Exception) { }
                //} while (!isWorking && counter-- > 0);
                //if (IsStop() || !isWorking)
                //{
                //    return;
                //}
                //step 2fa
                isWorking = false;
                counter = 10;
                do
                {
                    if (IsStop())
                    {
                        break;
                    }
                    Thread.Sleep(500);
                    if (processActionData.ProfileConfig.TwoFA.EnterPassword)
                    {
                        try
                        {
                            driver.FindElement(By.Id("ajax_password")).SendKeys(data.Password+OpenQA.Selenium.Keys.Enter);
                            Thread.Sleep(2000);
                        }
                        catch (Exception) { }
                    }
                    try
                    {
                        driver.FindElement(By.XPath("//div[contains(text(),'Two-factor authentication')]"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[3]/div[2]/div/div/div/div/div/div[2]/span[2]/div/div/button/div/div")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//span[contains(text(),'Get Started')]")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//span[contains(text(),'Get started')]")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
                if (IsStop() || !isWorking)
                {
                    return;
                }
                if (isWorking)
                {
                    isWorking = false;
                    counter = 10;
                    do
                    {
                        if (IsStop())
                        {
                            break;
                        }
                        Thread.Sleep(500);
                        try
                        {
                            driver.FindElement(By.XPath("//div[contains(text(),'Two-factor authentication')]"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div[3]/div[2]/div/div/div/div/div/div[3]/span[2]/div/div[2]/button")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//*[contains(text(),'Continue')]")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                    } while (!isWorking && counter-- > 0);
                }
                if (IsStop() || !isWorking)
                {
                    return;
                }
                if (isWorking)
                {
                    counter = 6;
                    string twofa = "";
                    do
                    {
                        if (IsStop())
                        {
                            break;
                        }
                        Thread.Sleep(500);
                        try
                        {
                            var element = driver.FindElement(By.XPath("/html/body/div[6]/div[2]/div/div/div/div/div/div/div[2]/div/div/div[2]/div[2]/span[2]"));
                            var text_code = element.GetAttribute("innerHTML");
                            if (!string.IsNullOrEmpty(text_code))
                            {
                                twofa = text_code.Trim();
                            }
                        }
                        catch (Exception) { }
                        if (string.IsNullOrEmpty(twofa))
                        {
                            try
                            {
                                var element1 = driver.FindElement(By.XPath("/html/body/div[5]/div[2]/div/div/div/div/div/div/div[2]/div/div/div[2]/div[2]/span[2]"));
                                var text_code1 = element1.GetAttribute("innerHTML");
                                if (!string.IsNullOrEmpty(text_code1))
                                {
                                    twofa = text_code1.Trim();
                                }
                            }
                            catch (Exception) { }
                        }
                        if (string.IsNullOrEmpty(twofa))
                        {
                            try
                            {
                                var element2 = driver.FindElement(By.XPath("/html/body/div[3]/div[2]/div/div/div/div/div/div[2]/div/div/div[2]/div[2]/span[2]"));
                                var text_code2 = element2.GetAttribute("innerHTML");
                                if (!string.IsNullOrEmpty(text_code2))
                                {
                                    twofa = text_code2.Trim();
                                }
                            }
                            catch (Exception) { }
                        }
                        if (string.IsNullOrEmpty(twofa))
                        {
                            try
                            {
                                var element3 = driver.FindElement(By.XPath("/html/body/div[8]/div[2]/div/div/div/div/div/div/div[2]/div/div/div[2]/div[2]/span[2]"));
                                var text_code3 = element3.GetAttribute("innerHTML");
                                if (!string.IsNullOrEmpty(text_code3))
                                {
                                    twofa = text_code3.Trim();
                                }
                            }
                            catch (Exception) { }
                        }
                    } while (string.IsNullOrEmpty(twofa) && counter-- > 0);
                    if (IsStop())
                    {
                        return;
                    }
                    if (!string.IsNullOrEmpty(twofa))
                    {
                        bool b1 = false;
                        try
                        {
                            Thread.Sleep(2000);
                            driver.FindElement(By.XPath("/html/body/div[6]/div[2]/div/div/div/div/div/div/div[3]/span[2]/div/div[2]/button")).Click();
                            b1 = true;
                        }
                        catch (Exception) { }
                        if (!b1)
                        {
                            try
                            {
                                Thread.Sleep(2000);
                                driver.FindElement(By.XPath("/html/body/div[5]/div[2]/div/div/div/div/div/div/div[3]/span[2]/div/div[2]/button")).Click();
                                b1 = true;
                            }
                            catch (Exception) { }
                        }
                        if (!b1)
                        {
                            try
                            {
                                driver.FindElement(By.XPath(".//div[contains(text(),'Continue')]")).Click();
                            }
                            catch (Exception) { }
                        }

                        twofa = Regex.Replace(twofa, @"\s+", "");
                        string code = "";
                        counter = 5;
                        do
                        {
                            code = TwoFactorRequest.GetPassCode(twofa);
                        } while (string.IsNullOrEmpty(code) && counter-- > 0);

                        if (!string.IsNullOrEmpty(code))
                        {
                            isWorking = false;
                            try
                            {
                                Thread.Sleep(2000);
                                driver.FindElement(By.XPath(".//input[@data-key='0']")).SendKeys(code);
                                isWorking = true;
                            }
                            catch (Exception) { }
                            if (!isWorking)
                            {
                                try
                                {
                                    driver.FindElement(By.XPath("/html/body/div[6]/div[2]/div/div/div/div/div/div/div[2]/div/div/div/div[2]/div/div/form/input[1]")).SendKeys(code);
                                    isWorking = true;
                                }
                                catch (Exception) { }
                            }
                            if (isWorking)
                            {
                                data.TwoFA = twofa;
                                accountDao.TwoFA(data.UID, twofa);
                                Thread.Sleep(2500);
                            }
                        }
                    }
                }

                try
                {
                    Thread.Sleep(1000);
                    driver.SwitchTo().DefaultContent();
                }
                catch (Exception) { }
            }
        }
        public void TurnOn2FAOnMobile()
        {
            try
            {
                driver.Navigate().GoToUrl("https://mbasic.facebook.com/privacy");
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
                    driver.FindElement(By.XPath("//*[text() = 'Use two-factor authentication']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (IsStop())
                {
                    break;
                }
            } while (!isWorking && counter-- > 0);
            if (IsStop())
            {
                return;
            }
            if (isWorking)
            {
                counter = 4;
                isWorking = false;
                IWebElement element = null;
                do
                {
                    if (IsStop())
                    {
                        break;
                    }
                    Thread.Sleep(500);
                    try
                    {
                        element = driver.FindElement(By.XPath("//input[@type='password']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                } while (!isWorking && counter-- > 0);
                if (IsStop())
                {
                    return;
                }
                if (element != null)
                {
                    if (processActionData.ProfileConfig.TwoFA.EnterPassword)
                    {
                        element.SendKeys(data.Password + OpenQA.Selenium.Keys.Enter);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {
                return;
            }
            counter = 6;
            isWorking = false;
            do
            {
                IWebElement element = null;
                if (IsStop())
                {
                    break;
                }
                Thread.Sleep(500);
                try
                {
                    element = driver.FindElement(By.XPath("//*[text() = 'Use Authentication App']"));
                }
                catch (Exception) { }
                if (element == null)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//*[text() = 'Use authentication app']"));
                    }
                    catch (Exception) { }
                }
                if (element == null)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div/div/div[2]/div/table/tbody/tr/td/div/div/div/div/div[1]/div/table/tbody/tr/td[2]/div/div[3]/a"));
                    }
                    catch (Exception) { }
                }
                if (element == null)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div/div/div[3]/div/table/tbody/tr/td/div/div/div/div/div[1]/div/table/tbody/tr/td[2]/div/div[3]/a"));
                    }
                    catch (Exception) { }
                }
                if (element != null)
                {
                    try
                    {
                        Actions actions = new Actions(driver);
                        actions.MoveToElement(element).Click().Perform();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return;
            }
            counter = 4;
            isWorking = false;
            IWebElement element1 = null;
            do
            {
                if (IsStop())
                {
                    break;
                }
                Thread.Sleep(500);
                try
                {
                    element1 = driver.FindElement(By.XPath("//input[@type='password']"));
                    isWorking = true;
                }
                catch (Exception) { }
            } while (!isWorking && counter-- > 0);
            if (IsStop())
            {
                return;
            }
            if (element1 != null)
            {
                if (processActionData.ProfileConfig.TwoFA.EnterPassword)
                {
                    element1.SendKeys(data.Password + OpenQA.Selenium.Keys.Enter);
                }
                else
                {
                    return;
                }
            }
            isWorking = false;
            counter = 10;
            string twofa = "";
            do
            {
                if (IsStop())
                {
                    break;
                }
                Thread.Sleep(500);
                try
                {
                    var element = driver.FindElement(By.XPath("/html/body/div/div/div[2]/div/table/tbody/tr/td/form/div[2]/div/table/tbody/tr/td/div/div[2]/div[2]"));
                    var textCode = element.GetAttribute("innerHTML");
                    if (!string.IsNullOrEmpty(textCode))
                    {
                        twofa = textCode.Trim();
                    }
                }
                catch (Exception) { }
                if (string.IsNullOrEmpty(twofa))
                {
                    try
                    {
                        var element2 = driver.FindElement(By.XPath("/html/body/div/div/div[3]/div/table/tbody/tr/td/form/div[2]/div/table/tbody/tr/td/div/div[2]/div[2]"));
                        var textCode1 = element2.GetAttribute("innerHTML");
                        if (!string.IsNullOrEmpty(textCode1))
                        {
                            twofa = textCode1.Trim();
                        }
                    }
                    catch (Exception) { }
                }
            } while (string.IsNullOrEmpty(twofa) && counter-- > 0);
            if (!string.IsNullOrEmpty(twofa))
            {
                isWorking = false;
                try
                {
                    driver.FindElement(By.XPath("//input[@name='confirmButton']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.Id("qr_confirm_button")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                twofa = Regex.Replace(twofa, @"\s+", "");
                string code = "";
                counter = 5;
                do
                {
                    code = TwoFactorRequest.GetPassCode(twofa);
                } while (string.IsNullOrEmpty(code) && counter-- > 0);
                if (!string.IsNullOrEmpty(code))
                {
                    isWorking = false;
                    counter = 6;
                    do
                    {
                        Thread.Sleep(500);
                        try
                        {
                            driver.FindElement(By.XPath("//input[@name='code']")).SendKeys(code + OpenQA.Selenium.Keys.Enter);
                            isWorking = true;
                        }
                        catch (Exception) { }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div/div/div[2]/div/table/tbody/tr/td/div[2]/form[2]/div[1]/div[1]/div/input")).SendKeys(code + OpenQA.Selenium.Keys.Enter);
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                    } while (!isWorking && counter-- > 0);
                    if (isWorking)
                    {
                        Thread.Sleep(2000);

                        data.TwoFA = twofa;
                        accountDao.TwoFA(data.UID, twofa);
                    }
                }
            }
        }
        public void SlugHacked()
        {
            if (processActionData.ProfileConfig.Password.RunOnWeb)
            {
                bool isChange= WebFBTool.ChangePassword(driver,data.Password, processActionData.ProfileConfig.Password.Value);
                if(isChange)
                {
                    data.Password = processActionData.ProfileConfig.Password.Value;
                    accountDao.Password(data.UID, processActionData.ProfileConfig.Password.Value);
                }
            }
            else if(processActionData.ProfileConfig.Password.RunOnWeb2)
            {
                bool isChange = WebFBTool.SlugHacked(driver, data.Password, processActionData.ProfileConfig.Password.Value);
                if (isChange)
                {
                    data.Password = processActionData.ProfileConfig.Password.Value;
                    accountDao.Password(data.UID, processActionData.ProfileConfig.Password.Value);
                }
            }
            else if(processActionData.ProfileConfig.Password.RunOnMbasic)
            {
                try
                {
                    driver.Navigate().GoToUrl("https://mbasic.facebook.com/hacked");
                }
                catch (Exception) { }
                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(1000);
                string url = "";
                try
                {
                    url = driver.Url;
                }
                catch (Exception) { }

                if (url.Contains("mbasic") || url.Contains("free."))
                {
                    SlugHackedMBASIC();
                }
                else
                {
                    SlugHackedMobile();
                }
            } else if(processActionData.ProfileConfig.Password.RunOnNewLayOut)
            {
                try
                {
                    driver.Navigate().GoToUrl("https://accountscenter.facebook.com/password_and_security/password/change");
                }
                catch (Exception) { }
                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(1000);
                int counter = 4;
                bool isWorking = false;
                IWebElement chElement = null;
                do
                {
                    Thread.Sleep(500);
                    try
                    {
                        chElement= driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[3]/div[2]/div[4]/div/div/div[2]/div/div/div[1]"));
                        
                        isWorking = true;
                    }
                    catch (Exception) { }
                    if (!isWorking)
                    {
                        try
                        {
                            chElement = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[5]/div[2]/div[1]/div/div/div[2]/div/div/div[1]"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            chElement = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[5]/div[2]/div[1]/div/div/div[2]/div/div/div[1]/div"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            chElement = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[3]/div[2]/div[4]/div/div/div[2]/div/div/div[1]/div"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                   {
                        try
                        {
                            chElement = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div[1]/div/div[3]/div[2]/div[4]/div/div/div[2]/div/div[1]"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            chElement = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div[1]/div/div[5]/div[2]/div[1]/div/div/div[2]/div/div[1]/div[1]/div"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
                if(!isWorking)
                {
                    return;
                }
                try
                {
                    Actions actions = new Actions(driver);
                    actions.MoveToElement(chElement).Click().Perform();
                }
                catch (Exception) { }
                Thread.Sleep(1000);
                isWorking = false;
                counter = 4;
                do
                {
                    Thread.Sleep(500);
                    try
                    {
                        driver.FindElement(By.XPath("//input[@type='password']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                } while(!isWorking && counter-- > 0);
                if (!isWorking)
                {
                    return;
                }
                isWorking = false;
                try
                {
                    IReadOnlyCollection<IWebElement> element = driver.FindElements(By.XPath("//input[@type='password']"));
                    int index = 0;
                    try
                    {
                        element.ElementAt(index).SendKeys(data.Password);
                        Thread.Sleep(500);
                        isWorking = true;
                        index++;
                    }
                    catch (Exception) { }
                    try
                    {
                        element.ElementAt(index).SendKeys(processActionData.ProfileConfig.Password.Value);
                        Thread.Sleep(500);
                        isWorking = true;
                        index++;
                    }
                    catch (Exception) { }
                    try
                    {
                        element.ElementAt(index).SendKeys(processActionData.ProfileConfig.Password.Value);
                        Thread.Sleep(500);
                        isWorking = true;
                        index++;
                    }
                    catch (Exception) { }
                } catch(Exception) { }
                if(!isWorking)
                {
                    return;
                }
                isWorking = false;
                try
                {
                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div[2]/div/div[4]/div[3]/div/div/div/div/div/div/div")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div[2]/div/div[6]/div[3]/div/div/div/div/div/div/div/div/div[1]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[contains(text(),'Change password')]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[contains(text(),'Change Password')]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                Thread.Sleep(1000);
                if(isWorking)
                {
                    isWorking = false;
                    counter = 10;
                    do
                    {
                        Thread.Sleep(500);
                        try
                        {
                            driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div[2]/div/div[4]/div[3]/div/div/div/div/div/div/div"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div[2]/div/div[6]/div[3]/div/div/div/div/div/div/div/div/div[1]"));
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//span[contains(text(),'Change password')]"));
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//span[contains(text(),'Change Password')]"));
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                    } while (!isWorking && counter-- > 0);

                    if (isWorking)
                    {
                        data.Password = processActionData.ProfileConfig.Password.Value;
                        accountDao.Password(data.UID, processActionData.ProfileConfig.Password.Value);
                    }
                }
            }
        }
        public void SlugHackedMobile()
        {
            bool isWorking = false;
            int counter = 5;

            // step 1
            do
            {
                if (IsStop())
                {
                    break;
                }
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//input[@value='someone_accessed']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div/div/div/div/table/tbody/tr/td/form/div[1]/fieldset/label[2]/div/table/tbody/tr/td[2]/input")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div/div/div[2]/div/table/tbody/tr/td/form/div[1]/fieldset/label[2]/div/table/tbody/tr/td[2]/input")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (isWorking)
            {
                isWorking = false;
                counter = 5;
                do
                {
                    if (IsStop())
                    {
                        break;
                    }
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div/div/div[2]/div/table/tbody/tr/td/form/div[3]/button")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//button[contains(text(),'Continue')]")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
            }

            // step 2
            if (isWorking)
            {
                isWorking = false;
                counter = 6;
                Thread.Sleep(1000);
                do
                {
                    if (IsStop())
                    {
                        break;
                    }
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
                            driver.FindElement(By.XPath("/html/body/div/div/div[3]/div/form/div/article/div[1]/table/tbody/tr/td/input")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.Id("checkpointButtonContinue-actual-button")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
            }
            // step 3
            if (isWorking)
            {
                Thread.Sleep(1000);
                isWorking = false;
                counter = 4;
                do
                {
                    if (IsStop())
                    {
                        break;
                    }
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
                            driver.FindElement(By.XPath("/html/body/div/div/div[3]/div/form/div/article/div[1]/div/input")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.Id("checkpointSubmitButton-actual-button")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
            }
            if (!isWorking)
            {
                return;
            }
            Thread.Sleep(2000);
            if (IsStop())
            {
                return;
            }
            try
            {
                driver.FindElement(By.XPath("//input[@name='password_old']")).SendKeys(data.Password);
                Thread.Sleep(1000);
            }
            catch (Exception) { }
            try
            {
                driver.FindElement(By.XPath("//input[@name='password_new']")).SendKeys(processActionData.ProfileConfig.Password.Value);
                Thread.Sleep(1000);
                isWorking = true;
            }
            catch (Exception) { }
            try
            {
                driver.FindElement(By.XPath("//input[@name='password_confirm']")).SendKeys(processActionData.ProfileConfig.Password.Value);
            }
            catch (Exception) { }
            if (isWorking)
            {
                Thread.Sleep(1000);
                isWorking = false;
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
                        driver.FindElement(By.XPath("/html/body/div/div/div[3]/div/form/div/article/div[1]/div/input")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.Id("checkpointSubmitButton-actual-button")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (isWorking)
                {
                    data.Password = processActionData.ProfileConfig.Password.Value;
                    accountDao.Password(data.UID, processActionData.ProfileConfig.Password.Value);
                }
            }
            if (isWorking)
            {
                try
                {
                    Thread.Sleep(1000);
                    driver.FindElement(By.XPath("//input[@value='Continue']")).Click();
                }
                catch (Exception) { }
                try
                {
                    Thread.Sleep(1000);
                    driver.FindElement(By.XPath("/html/body/div/div/div[3]/div/form/div/article/div[1]/div/input")).Click();
                }
                catch (Exception) { }
                try
                {
                    Thread.Sleep(1000);
                    driver.FindElement(By.Id("checkpointButtonContinue-actual-button")).Click();
                }
                catch (Exception) { }
            }
        }
        public void SlugHackedMBASIC()
        {
            int counter = 4;
            bool isWorking = false;

            // step 1
            do
            {
                if (IsStop())
                {
                    break;
                }
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//input[@value='someone_accessed']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return;
            }
            Thread.Sleep(1000);
            isWorking = false;
            IWebElement conEle = null;
            if (!isWorking)
            {
                try
                {
                    conEle= driver.FindElement(By.XPath("//button[contains(text(),'Continue')]"));
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    conEle= driver.FindElement(By.XPath("//*[contains(text(),'Continue')]"));
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    conEle = driver.FindElement(By.XPath("/html/body/div/div/div/div/table/tbody/tr/td/form/div[3]/button"));
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    conEle = driver.FindElement(By.XPath("/html/body/div/div/div[2]/div/table/tbody/tr/td/form/div[3]/button"));
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    conEle = driver.FindElement(By.XPath("//button[@type='submit']"));
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                return;
            }
            if(conEle != null)
            {
                try
                {
                    Actions actions = new Actions(driver);
                    actions.MoveToElement(conEle).Click().Perform();
                }
                catch (Exception) { }
            }

            // step 2
            counter = 8;
            isWorking = false;
            do
            {
                if (IsStop())
                {
                    break;
                }
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.Id("checkpointButtonContinue-actual-button")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[contains(text(),'Continue')]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div/div/div[2]/div/form/div/article/div[1]/table/tbody/tr/td/input")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div/div/div[2]/div/form/div/div[2]/div[2]/table/tbody/tr/td/input")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div/div/div[3]/div/form/div/div[2]/div[2]/table/tbody/tr/td/input")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//button[@type='submit']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return;
            }

            // step 3
            isWorking = false;
            counter = 8;
            do
            {
                if (IsStop())
                {
                    break;
                }
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.Id("checkpointSubmitButton-actual-button")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.Id("checkpointButtonContinue-actual-button")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div/div/div[2]/div/form/div/div[2]/div[2]/div/input")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div/div/div[3]/div/form/div/div[2]/div[2]/div/input")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div/div/div[2]/div/form/div/article/div[1]/div/input")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//button[@type='submit']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//*[contains(text(),'Continue')]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return;
            }
            // step 4
            if (IsStop())
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
                    driver.FindElement(By.XPath("//input[@name='password_old']")).SendKeys(data.Password);
                    Thread.Sleep(1000);
                }
                catch (Exception) { }
                try
                {
                    driver.FindElement(By.XPath("//input[@name='password_new']")).SendKeys(processActionData.ProfileConfig.Password.Value);
                    Thread.Sleep(1000);
                    isWorking = true;
                }
                catch (Exception) { }
                try
                {
                    driver.FindElement(By.XPath("//input[@name='password_confirm']")).SendKeys(processActionData.ProfileConfig.Password.Value);
                }
                catch (Exception) { }
            } while (!isWorking && counter-- > 0);
            if (isWorking)
            {
                isWorking = false;
                if (IsStop())
                {
                    return;
                }
                Thread.Sleep(2000);
                try
                {
                    driver.FindElement(By.Id("checkpointSubmitButton-actual-button")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//input[@value='Continue']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.Id("/html/body/div/div/div[2]/div/form/div/div[2]/div[2]/div/input")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div/div/div[2]/div/form/div/article/div[1]/div/input")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            }
            if (isWorking)
            {
                data.Password = processActionData.ProfileConfig.Password.Value;
                accountDao.Password(data.UID, processActionData.ProfileConfig.Password.Value);

                try
                {
                    Thread.Sleep(1000);
                    driver.FindElement(By.XPath("//input[@value='Continue']")).Click();
                }
                catch (Exception) { }
                try
                {
                    Thread.Sleep(1000);
                    driver.FindElement(By.XPath("checkpointButtonContinue-actual-button")).Click();
                }
                catch (Exception) { }
                try
                {
                    Thread.Sleep(1000);
                    driver.FindElement(By.XPath("/html/body/div/div/div[3]/div/form/div/article/div[1]/div/input")).Click();
                }
                catch (Exception) { }
            }
        }
        public bool IsStop()
        {
            return this.form.IsStop();
        }
        public void DeleteFriendsRequest(int num)
        {
            // update message
            data.Description = "Delete Friends Request";

            int counter;
            bool isWorking = false;
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/friends/requests");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);

            isWorking = false;
            counter = 4;
            do
            {
                if (IsStop())
                {
                    break;
                }
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//*[contains(text(),'Suggestions')]")).Click();
                    isWorking = false;
                    FBTool.WaitingPageLoading(driver);
                    Thread.Sleep(1000);
                }
                catch (Exception) { }
            } while (!isWorking && counter-- > 0);
            int errorCount = 0, totalDelete = 0;
            ReadOnlyCollection<IWebElement> deleteElements = null;
            string des = data.Description;
            do
            {
                if (IsStop())
                {
                    break;
                }
                counter = 4;
                isWorking = false;
                do
                {
                    if (IsStop())
                    {
                        break;
                    }
                    Thread.Sleep(500);
                    try
                    {
                        deleteElements = driver.FindElements(By.XPath("//div[@aria-label='Delete']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                } while (!isWorking && counter-- > 0);
                if (!isWorking)
                {
                    break;
                }
                isWorking = false;
                foreach (IWebElement element in deleteElements)
                {
                    if (IsStop())
                    {
                        break;
                    }
                    isWorking = true;
                    FBTool.Close(driver);

                    try
                    {
                        Thread.Sleep(500);
                        element.Click();
                        num--;
                        totalDelete++;
                        errorCount = 0;
                        data.Description = des + " " + totalDelete;
                        FBTool.Scroll(driver, 1000, false);
                    }
                    catch (Exception) { errorCount++; }
                    if (num <= 0 || errorCount >= 5)
                    {
                        break;
                    }
                }
                if (num > 0 && isWorking)
                {
                    if (IsStop())
                    {
                        break;
                    }
                    driver.Navigate().Refresh();
                    FBTool.WaitingPageLoading(driver);
                    if (IsStop())
                    {
                        break;
                    }
                    Thread.Sleep(1500);
                }
            } while (isWorking && num > 0);

            // update message
            accountDao.updateStatus(data.UID, data.Description, 1);
        }
        public void DeleteFriendsSuggest(int num)
        {
            // update message
            data.Description = "Delete Friends Suggest";

            int counter;
            bool isWorking = false;
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/friends");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);

            isWorking = false;
            counter = 4;
            do
            {
                if (IsStop())
                {
                    break;
                }
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//*[contains(text(),'Suggestions')]")).Click();
                    isWorking = true;
                    FBTool.WaitingPageLoading(driver);
                    Thread.Sleep(1000);
                }
                catch (Exception) { }
            } while (!isWorking && counter-- > 0);

            int deleteFriends = 0, errorCount = 0;
            string des = data.Description;
            ReadOnlyCollection<IWebElement> removeElements = null;
            do
            {
                if (IsStop())
                {
                    break;
                }
                counter = 6;
                isWorking = false;
                do
                {
                    if (IsStop())
                    {
                        break;
                    }
                    Thread.Sleep(500);
                    try
                    {
                        removeElements = driver.FindElements(By.XPath("//div[@aria-label='Remove']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                } while (!isWorking && counter-- > 0);
                if (!isWorking)
                {
                    break;
                }
                isWorking = false;
                foreach (IWebElement element in removeElements)
                {
                    if (IsStop())
                    {
                        break;
                    }
                    isWorking = true;
                    FBTool.Close(driver);

                    try
                    {
                        Thread.Sleep(500);
                        element.Click();
                        num--;
                        deleteFriends++;
                        errorCount = 0;
                        data.Description = des + " " + deleteFriends;
                        FBTool.Scroll(driver, 1000, false);
                    }
                    catch (Exception) { errorCount++; }
                    if (num <= 0)
                    {
                        break;
                    }
                }
                if (errorCount > 5)
                {
                    break;
                }
                if (num > 0 && isWorking)
                {
                    if (IsStop())
                    {
                        break;
                    }
                    driver.Navigate().Refresh();
                    if (IsStop())
                    {
                        break;
                    }
                    FBTool.WaitingPageLoading(driver);
                    if (IsStop())
                    {
                        break;
                    }
                    Thread.Sleep(1000);
                }
            } while (isWorking && num > 0);

            // update message
            accountDao.updateStatus(data.UID, data.Description, 1);
        }
        public void Unfriends(int num)
        {
            // update message
            data.Description = "Unfriends";

            int counter;
            bool isWorking = false;
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/" + data.UID + "/friends");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);

            string des = data.Description;
            int unfriends = 0, errorCount = 0;
            ReadOnlyCollection<IWebElement> deleteElements = null;
            FBTool.Scroll(driver, 1000, false);
            do
            {
                if (IsStop())
                {
                    break;
                }
                counter = 4;
                isWorking = false;
                do
                {
                    if (IsStop())
                    {
                        break;
                    }
                    Thread.Sleep(500);
                    try
                    {
                        deleteElements = driver.FindElements(By.XPath("//div[@aria-label='Friends']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                } while (!isWorking && counter-- > 0);
                if (!isWorking)
                {
                    break;
                }
                foreach (IWebElement element in deleteElements)
                {
                    if (IsStop())
                    {
                        break;
                    }
                    FBTool.Close(driver);
                    isWorking = false;
                    counter = 4;
                    do
                    {
                        if (IsStop())
                        {
                            break;
                        }
                        Thread.Sleep(500);
                        try
                        {
                            element.Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    } while (!isWorking && counter-- > 0);
                    isWorking = false;
                    counter = 6;
                    do
                    {
                        if (IsStop())
                        {
                            break;
                        }
                        Thread.Sleep(500);
                        try
                        {
                            driver.FindElement(By.XPath("//span[contains(text(),'Unfriend')]")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    } while (!isWorking && counter-- > 0);
                    isWorking = false;
                    counter = 6;
                    do
                    {
                        if (IsStop())
                        {
                            break;
                        }
                        Thread.Sleep(500);
                        try
                        {
                            driver.FindElement(By.XPath("//div[@aria-label='Confirm']")).Click();
                            isWorking = true;
                            unfriends++;
                            errorCount = 0;
                            num--;
                            data.Description = des + " " + unfriends;
                        }
                        catch (Exception) { }
                    } while (!isWorking && counter-- > 0);
                    if (num <= 0)
                    {
                        break;
                    }
                    if (!isWorking)
                    {
                        errorCount++;
                    }
                }
                if (num > 0 && isWorking)
                {
                    driver.Navigate().Refresh();
                    FBTool.WaitingPageLoading(driver);
                    Thread.Sleep(1000);
                    FBTool.Scroll(driver, 1000, false);
                }
            } while (isWorking && num > 0);

            // update message
            accountDao.updateStatus(data.UID, data.Description, 1);
        }
        public void DeleteAllPost()
        {
            // update message
            data.Description = "Delete post & tag";

            int counter;
            bool isWorking = false;
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/" + data.UID);
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            WebFBTool.CloseAllPopup(driver);
            if (IsStop())
            {
                return;
            }
            FBTool.Scroll(driver, 1000, false);
            bool isMoreSelectAll;
            for (int i = 0; i < 100; i++)
            {
                if (IsStop())
                {
                    break;
                }
                counter = 20;
                isWorking = false;
                isMoreSelectAll = false;
                do
                {
                    if (IsStop())
                    {
                        break;
                    }
                    Thread.Sleep(500);
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Manage Posts']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//div[@aria-label='Manage posts']")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);

                // select all post step by step
                if (isWorking)
                {
                    isWorking = false;
                    counter = 10;
                    do
                    {
                        if (IsStop())
                        {
                            break;
                        }
                        Thread.Sleep(500);
                        try
                        {
                            driver.FindElement(By.XPath("//span[contains(text(),'Select All')]")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//span[contains(text(),'Select all')]")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                    } while (!isWorking && counter-- > 0);
                    if (isWorking)
                    {
                        counter = 6;
                        do
                        {
                            if (IsStop())
                            {
                                break;
                            }
                            Thread.Sleep(500);
                            try
                            {
                                driver.FindElement(By.XPath("//span[contains(text(),'Select all')]"));
                                isMoreSelectAll = true;
                            }
                            catch (Exception) { }
                            if (!isMoreSelectAll)
                            {
                                try
                                {
                                    driver.FindElement(By.XPath("//span[contains(text(),'Select All')]"));
                                    isMoreSelectAll = true;
                                }
                                catch (Exception) { }
                            }
                        } while (!isMoreSelectAll && counter-- > 0);
                    }
                }
                // click on button next
                if (isWorking)
                {
                    isWorking = false;
                    counter = 4;
                    do
                    {
                        if (IsStop())
                        {
                            break;
                        }
                        Thread.Sleep(500);
                        try
                        {
                            driver.FindElement(By.XPath("//div[@aria-label='Next']")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    } while (!isWorking && counter-- > 0);
                }
                if (isWorking)
                {
                    IWebElement element = null;
                    ReadOnlyCollection<IWebElement> elements = null;
                    isWorking = false;
                    counter = 10;
                    do
                    {
                        if (IsStop())
                        {
                            break;
                        }
                        Thread.Sleep(500);
                        try
                        {
                            elements = driver.FindElements(By.XPath("//div[@aria-label='Manage Posts']"));
                            element = elements.ElementAt(1);
                            isWorking = true;
                        }
                        catch (Exception) { }
                        if (!isWorking)
                        {
                            try
                            {
                                elements = driver.FindElements(By.XPath("//div[@aria-label='Manage posts']"));
                                element = elements.ElementAt(1);
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div"));
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                    } while (!isWorking && counter-- > 0);
                    if (isWorking)
                    {
                        ReadOnlyCollection<IWebElement> options = null;
                        isWorking = false;
                        counter = 4;
                        do
                        {
                            if (IsStop())
                            {
                                break;
                            }
                            Thread.Sleep(500);
                            try
                            {
                                options = element.FindElements(By.XPath("//div[@aria-checked='false']"));
                                isWorking = true;
                            }
                            catch (Exception) { }
                        } while (!isWorking && counter-- > 0);
                        if (isWorking)
                        {
                            for (int index = 2; index >= 0; index--)
                            {
                                if (IsStop())
                                {
                                    break;
                                }
                                Thread.Sleep(1000);
                                isWorking = false;
                                try
                                {
                                    options.ElementAt(index).Click();
                                }
                                catch (Exception) { }
                                try
                                {
                                    Thread.Sleep(1000);
                                    element.FindElement(By.XPath("//div[@aria-checked='true']"));
                                    isWorking = true;
                                    index = -1;
                                }
                                catch (Exception) { }
                                if (isWorking)
                                {
                                    try
                                    {
                                        element.FindElement(By.XPath("//div[@aria-label='Done']")).Click();
                                        Thread.Sleep(2000);
                                    }
                                    catch (Exception) { }
                                }
                            }
                        }
                    }
                }
                else
                {
                    break;
                }
                if (!isMoreSelectAll)
                {
                    Thread.Sleep(2000);
                    break;
                }
            }
        }
        public void DeleteProfilePictures()
        {
            // update message
            data.Description = "Delete profile pictures";

            int counter;
            bool isWorking = false;
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/" + data.UID + "?sk=photos_albums");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            WebFBTool.CloseAllPopup(driver);
            counter = 10;
            do
            {
                if (IsStop())
                {
                    return;
                }
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//span[text() = 'Profile Pictures']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[text() = 'Profile pictures']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);

            FBTool.WaitingPageLoading(driver);
            IReadOnlyCollection<IWebElement> elements = null;
            counter = 5;
            do
            {
                if (IsStop())
                {
                    return;
                }
                Thread.Sleep(500);
                try
                {
                    elements = driver.FindElements(By.XPath("//div[@aria-label='Edit']"));
                }
                catch (Exception) { }
            } while (elements == null && counter-- > 0);
            if (elements == null)
            {
                return;
            }
            foreach (IWebElement element in elements)
            {
                FBTool.Close(driver);

                if (IsStop())
                {
                    break;
                }
                isWorking = false;
                try
                {
                    element.Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (isWorking)
                {
                    isWorking = false;
                    counter = 6;
                    do
                    {
                        if (IsStop())
                        {
                            break;
                        }
                        Thread.Sleep(500);
                        try
                        {
                            driver.FindElement(By.XPath("//span[text() = 'Delete Photo']")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//span[text() = 'Delete photo']")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                    } while (!isWorking && counter-- > 0);
                    if (IsStop())
                    {
                        break;
                    }
                    if (isWorking)
                    {
                        counter = 8;
                        isWorking = false;
                        do
                        {
                            if (IsStop())
                            {
                                break;
                            }
                            Thread.Sleep(500);
                            try
                            {
                                driver.FindElement(By.XPath("//div[@aria-label='Delete']")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                            if (!isWorking)
                            {
                                try
                                {
                                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div[4]/div/div[1]/div[1]")).Click();
                                    isWorking = true;
                                }
                                catch (Exception) { }
                            }
                        } while (!isWorking && counter-- > 0);
                        if (isWorking)
                        {
                            Thread.Sleep(2000);
                        }
                    }
                }
            }
        }
        public void DeleteCoverPictures()
        {
            // update message
            data.Description = "Delete cover pictures";

            int counter;
            bool isWorking = false;
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/" + data.UID + "?sk=photos_albums");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            if (IsStop())
            {
                return;
            }
            Thread.Sleep(1000);
            WebFBTool.CloseAllPopup(driver);
            counter = 10;
            do
            {
                if (IsStop())
                {
                    break;
                }
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//span[text() = 'Cover Pictures']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[text() = 'Cover pictures']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[text() = 'Cover Photos']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[text() = 'Cover photos']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return;
            }
            FBTool.WaitingPageLoading(driver);
            IReadOnlyCollection<IWebElement> elements = null;
            counter = 8;
            do
            {
                if (IsStop())
                {
                    break;
                }
                Thread.Sleep(500);
                try
                {
                    elements = driver.FindElements(By.XPath("//div[@aria-label='Edit']"));
                }
                catch (Exception) { }
            } while (elements == null && counter-- > 0);
            if (elements == null)
            {
                return;
            }
            foreach (IWebElement element in elements)
            {
                FBTool.Close(driver);

                if (IsStop())
                {
                    break;
                }
                isWorking = false;
                try
                {
                    element.Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (isWorking)
                {
                    isWorking = false;
                    counter = 5;
                    do
                    {
                        if (IsStop())
                        {
                            break;
                        }
                        Thread.Sleep(500);
                        try
                        {
                            driver.FindElement(By.XPath("//span[text() = 'Delete Photo']")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//span[text() = 'Delete photo']")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                    } while (!isWorking && counter-- > 0);
                    if (isWorking)
                    {
                        counter = 10;
                        isWorking = false;
                        do
                        {
                            if (IsStop())
                            {
                                break;
                            }
                            Thread.Sleep(500);
                            try
                            {
                                driver.FindElement(By.XPath("//div[@aria-label='Delete']")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                            if (!isWorking)
                            {
                                try
                                {
                                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div[4]/div/div[1]/div[1]")).Click();
                                    isWorking = true;
                                }
                                catch (Exception) { }
                            }
                        } while (!isWorking && counter-- > 0);
                        if (isWorking)
                        {
                            Thread.Sleep(2000);
                        }
                    }
                }
            }
        }
        public void DeleteAllPictures()
        {
            // update message
            data.Description = "Delete all pictures";

            int counter;
            bool isWorking = false;
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/" + data.UID + "?sk=photos");
            }
            catch { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            WebFBTool.CloseAllPopup(driver);

            counter = 10;
            isWorking = false;
            do
            {
                if (IsStop())
                {
                    break;
                }
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//span[text() = 'Your Photos']")).Click();
                    isWorking = true;
                }
                catch { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[text() = 'Your photos']")).Click();
                        isWorking = true;
                    }
                    catch { }
                }
            } while (!isWorking && counter-- > 0);

            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            do
            {
                if (IsStop())
                {
                    break;
                }
                FBTool.Close(driver);
                isWorking = false;
                counter = 10;
                do
                {
                    if (IsStop())
                    {
                        break;
                    }
                    Thread.Sleep(500);
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Edit']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                } while (!isWorking && counter-- > 0);

                if (isWorking)
                {
                    isWorking = false;
                    counter = 4;
                    do
                    {
                        if (IsStop())
                        {
                            break;
                        }
                        Thread.Sleep(500);
                        try
                        {
                            driver.FindElement(By.XPath("//span[text() = 'Delete Photo']")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                        if (!isWorking)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("//span[text() = 'Delete photo']")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                    } while (!isWorking && counter-- > 0);

                    if (isWorking)
                    {
                        counter = 8;
                        isWorking = false;
                        do
                        {
                            if (IsStop())
                            {
                                break;
                            }
                            Thread.Sleep(500);
                            try
                            {
                                driver.FindElement(By.XPath("//div[@aria-label='Delete']")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                            if (!isWorking)
                            {
                                try
                                {
                                    driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div[4]/div/div[1]/div[1]")).Click();
                                    isWorking = true;
                                }
                                catch (Exception) { }
                            }
                        } while (!isWorking && counter-- > 0);
                        if (isWorking)
                        {
                            Thread.Sleep(2000);
                        }
                    }
                }
            } while (isWorking);
        }
    }
}
