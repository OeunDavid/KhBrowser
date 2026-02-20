using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolKHBrowser.ToolLib.Data;
using ToolKHBrowser.ToolLib.Tool;
using ToolLib.Data;
using ToolKHBrowser.ViewModels;
using ToolKHBrowser.Views;

namespace ToolKHBrowser.ViewModels
{
    public interface IFriendsViewModel
    {
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void Start(frmMain form, IWebDriver driver, FbAccount data); 
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void Add();
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void Accept();
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void AddByUID();
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void Backup();
    }
    public class FriendsViewModel : IFriendsViewModel
    {
        private IAccountDao accountDao;
        private ICacheDao cacheDao;
        private frmMain form;
        private FbAccount data;
        private IWebDriver driver;
        private ProcessActions processActionData;

        public FriendsViewModel(IAccountDao accountDao, ICacheDao cacheDao)
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
        }
        public void Backup()
        {
            data.Description = "Backup Friends";

            data.TotalFriend = WebFBTool.GetFriends(driver);
            accountDao.UpdateFriends(data.UID, data.TotalFriend);
        }
        public void Add()
        {
            if (processActionData == null || processActionData.FriendsConfig == null)
                return;

            int num = processActionData.FriendsConfig.AddNumber;
            if(num > 0)
            {
                AddFriend(num);
            }
        }
        public void AddFriend(int num)
        {
            data.Description = "Add Friends";

            driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/friends");

            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(2000);
            try
            {
                driver.FindElement(By.XPath("//*[contains(text(),'Suggestions')]")).Click();
            }
            catch (Exception) { }
            Thread.Sleep(2500);
            do
            {
                if (IsStop())
                {
                    break;
                }
                num = StartAddFriends(num);
                if (num > 0)
                {
                    driver.Navigate().Refresh();
                    FBTool.WaitingPageLoading(driver);
                    Thread.Sleep(2000);
                }
            } while (num > 0);
        }
        public int StartAddFriends(int num)
        {
            bool isWorking = false, isFirstWorking = false; ;
            do
            {
                isWorking = false;
                IWebElement element = null;
                try
                {
                    element = driver.FindElement(By.XPath("//span[contains(text(),'Add Friend')]"));
                    
                }
                catch (Exception) { }
                if(element == null)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("//span[contains(text(),'Add friend')]"));

                    }
                    catch (Exception) { }
                }
                if(element != null)
                {
                    try
                    {
                        num--;
                        isFirstWorking = true;
                        isWorking = true;
                        FBTool.Scroll(driver, element);
                        Actions actions = new Actions(driver);
                        actions.MoveToElement(element).Click().Perform();
                        Thread.Sleep(1000);
                    } catch(Exception) { }
                }
            } while (!IsStop() && isWorking && num > 0);
            if(!isFirstWorking)
            {
                num = 0;
            }

            return num;
        }
        public void Accept()
        {
            if (processActionData == null || processActionData.FriendsConfig == null)
                return;

            int num = processActionData.FriendsConfig.AcceptNumber;
            if (num > 0)
            {
                data.Description = "Accept Friends";
                ConfirmFriends(num);
            }
        }
        public void AddByUID()
        {
            if (processActionData == null || processActionData.FriendsConfig == null || processActionData.FriendsConfig.FriendsByUID == null)
                return;

            int num = processActionData.FriendsConfig.FriendsByUID.AddNumber;
            if(num > 0)
            {
                data.Description = "Add Friends by UID";
                for (int i = 1; i <= num; i ++ )
                {
                    if(IsStop())
                    {
                        break;
                    }
                    var uid = GetUID();
                    try
                    {
                        driver.Navigate().GoToUrl(Constant.FB_MOBILE_URL + "/" + uid);
                    }
                    catch (Exception) { }
                    FBTool.WaitingPageLoading(driver);
                    FBTool.Scroll(driver, 500, false);
                    if (IsStop())
                    {
                        return;
                    }
                    FBTool.Close(driver);
                    MobileFBTool.AddFriend(driver);
                }
            }
        }
        public string GetUID()
        {
            string str = "";
            var cache = form.cacheViewModel.GetCacheDao().Get("friend:config:uids");
            if (cache == null || cache.Value == null) return "";

            var uids = cache.Value.ToString();
            if (!string.IsNullOrEmpty(uids))
            {
                try
                {
                    string[] lines = uids.Split('\n');
                    if (lines.Length > 0)
                    {
                        str = lines[0].Trim();
                        uids = string.Join("\n", lines.Skip(1));
                    }

                    form.cacheViewModel.GetCacheDao().Set("friend:config:uids", uids);
                }
                catch (Exception) { }
            }

            return str;
        }
        public void ConfirmFriends(int num)
        {
            driver.Navigate().GoToUrl(Constant.FB_WEB_URL + "/friends/requests");
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1500);
            try
            {
                driver.FindElement(By.XPath("//*[contains(text(),'Suggestions')]")).Click();
                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(1000);
            }
            catch (Exception) { }
            do
            {
                if (IsStop())
                {
                    break;
                }
                num = StartConfirmFriends(num);
                if (num > 0)
                {
                    driver.Navigate().Refresh();
                    FBTool.WaitingPageLoading(driver);
                    Thread.Sleep(2000);
                }
            } while (num > 0);
        }
        public int StartConfirmFriends(int num)
        {
            bool isWorking = false, isFirstWorking = false; ;
            do
            {
                isWorking = false;
                try
                {
                    IWebElement element = driver.FindElement(By.XPath("//span[contains(text(),'Confirm')]"));
                    num--;
                    isFirstWorking = true;
                    isWorking = true;
                    FBTool.Scroll(driver, element);
                    Actions actions = new Actions(driver);
                    actions.MoveToElement(element).Click().Perform();
                    Thread.Sleep(1000);
                }
                catch (Exception) { }

            } while (!IsStop() && isWorking && num > 0);
            if (!isFirstWorking)
            {
                num = 0;
            }

            return num;
        }
        public bool IsStop()
        {
            return this.form.IsStop();
        }
    }
}
