using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using ToolKHBrowser.ToolLib.Data;
using ToolKHBrowser.ToolLib.Tool;
using ToolLib.Data;
using ToolLib.Tool;
using WpfUI.ToolLib.Data;
using WpfUI.ViewModels;
using WpfUI.Views;

namespace ToolKHBrowser.ViewModels
{
    public interface INewsFeedViewModel
    {
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void Start(frmMain form, IWebDriver driver, FbAccount data);
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void ReadNotification();
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void Messenger();
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void Play();
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void Timeline();
    }
    public class NewsFeedViewModel : INewsFeedViewModel
    {
        private IAccountDao accountDao;
        private ICacheDao cacheDao;
        private frmMain form;
        private FbAccount data;
        private IWebDriver driver;
        private ProcessActions processActionData;
        private string[] captionArr;
        private string[] soureFolderFileArr;
        private Random random;
        private int sourceFolderFileIndex;
        private int captionIndex;

        public NewsFeedViewModel(IAccountDao accountDao, ICacheDao cacheDao)
        {
            this.accountDao = accountDao;
            this.cacheDao = cacheDao;
        }
        public void Start(frmMain form, IWebDriver driver, FbAccount data)
        {
            this.form = form ?? throw new ArgumentNullException(nameof(form));
            this.data = data;
            this.driver = driver;

            // safe defaults
            sourceFolderFileIndex = 0;
            captionIndex = 0;
            captionArr = Array.Empty<string>();
            random = new Random();

            // processActionsData can be null
            this.processActionData = this.form.processActionsData;

            // ✅ SAFE: cache dao might be null
            var cacheDao = this.form.cacheViewModel?.GetCacheDao();

            // ✅ SAFE: Get(...) might return null, Value might be null
            try
            {
                var item = cacheDao?.Get("newsfeed:source_index");
                if (item?.Value != null && int.TryParse(item.Value.ToString(), out var idx))
                    sourceFolderFileIndex = idx;
            }
            catch { }

            try
            {
                var item = cacheDao?.Get("newsfeed:caption_index");
                if (item?.Value != null && int.TryParse(item.Value.ToString(), out var idx))
                    captionIndex = idx;
            }
            catch { }

            // ✅ SAFE: nested objects might be null
            try
            {
                var captions = this.processActionData?.NewsFeed?.Timeline?.Captions;
                if (!string.IsNullOrWhiteSpace(captions))
                {
                    captionArr = captions
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            catch { }
        }

        public void ReadNotification()
        {
            bool isWorking = false;
            IWebElement element = null;
            try
            {
                element = driver.FindElement(By.XPath("//a[@href='/notifications/']"));
                isWorking = true;
            }
            catch (Exception) { }
            if (!isWorking)
            {
                try
                {
                    element = driver.FindElement(By.Id("notifications_jewel"));
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if(isWorking)
            {
                try
                {
                    Actions actions = new Actions(driver);
                    actions.MoveToElement(element)
                        .Click()
                        .Build()
                        .Perform();
                    Thread.Sleep(1000);
                }
                catch (Exception) { }
                FBTool.Scroll(driver, 3000, false);
            }
        }
        public void Play()
        {
            try
            {
                driver.Navigate().GoToUrl(Constant.FB_WEB_URL);
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);

            int sT = this.form.processActionsData.NewsFeed.NewsFeed.PlayTime.NumberStart;
            int eT = this.form.processActionsData.NewsFeed.NewsFeed.PlayTime.NumberEnd;
            int time = GetRankNumber(sT, eT);

            if (time > 0)
            {
                bool isLike = false;
                do
                {
                    if (!isLike)
                    {
                        isLike = true;
                        if (processActionData.NewsFeed.NewsFeed.React.Like)
                        {
                            WebFBTool.LikePost(driver);
                        }
                        else if (processActionData.NewsFeed.NewsFeed.React.Random && random.Next(0, 2) == 1)
                        {
                            WebFBTool.LikePost(driver);
                        }
                    }
                    time--;
                    FBTool.Scroll(driver, 1000, false);
                } while (!IsStop() && time > 0);
            }
        }
        public void Messenger()
        {
            bool isWorking = ReadMessenger();
            
            if(isWorking)
            {
                isWorking = false;
                if (!processActionData.NewsFeed.Messenger.MessageSound.None)
                {
                    MessengerOption();

                    IWebElement element = null;
                    try
                    {
                        element = driver.FindElement(By.XPath("//input[@aria-label='Message sounds']"));
                    }
                    catch (Exception) { }
                    if (element != null)
                    {
                        string isChecked = "false";
                        try
                        {
                            isChecked = element.GetAttribute("aria-checked");
                        }
                        catch (Exception) { }
                        if ((processActionData.NewsFeed.Messenger.MessageSound.On && isChecked == "false") || (processActionData.NewsFeed.Messenger.MessageSound.Off && isChecked == "true"))
                        {
                            try
                            {
                                element.Click();
                                Thread.Sleep(1000);
                            }
                            catch (Exception) { }
                        }
                    }
                }
                if (!processActionData.NewsFeed.Messenger.MessagePopup.None)
                {
                    MessengerOption();
                    IWebElement element = null;
                    try
                    {
                        element = driver.FindElement(By.XPath("//input[@aria-label='Pop-up new messages']"));
                    }
                    catch (Exception) { }
                    if (element != null)
                    {
                        string isChecked = "false";
                        try
                        {
                            isChecked = element.GetAttribute("aria-checked");
                        }
                        catch (Exception) { }
                        if ((processActionData.NewsFeed.Messenger.MessagePopup.On && isChecked == "false") || (processActionData.NewsFeed.Messenger.MessagePopup.Off && isChecked == "true"))
                        {
                            try
                            {
                                element.Click();
                                Thread.Sleep(1000);
                            }
                            catch (Exception) { }
                        }
                    }
                }
                if (!processActionData.NewsFeed.Messenger.MessageCallSound.None)
                {
                    MessengerOption();

                    IWebElement element = null;
                    try
                    {
                        element = driver.FindElement(By.XPath("//input[@aria-label='Incoming call sounds']"));
                    }
                    catch (Exception) { }
                    if (element != null)
                    {
                        string isChecked = "false";
                        try
                        {
                            isChecked = element.GetAttribute("aria-checked");
                        }
                        catch (Exception) { }
                        if (processActionData.NewsFeed.Messenger.MessageCallSound.On && isChecked == "false")
                        {
                            try
                            {
                                element.Click();
                                Thread.Sleep(1000);
                            }
                            catch (Exception) { }
                        }
                        else if (processActionData.NewsFeed.Messenger.MessageCallSound.Off && isChecked == "true")
                        {
                            try
                            {
                                element.Click();
                                Thread.Sleep(1000);
                            }
                            catch (Exception) { }
                            int counter = 4;
                            isWorking = false;
                            do
                            {
                                Thread.Sleep(500);
                                try
                                {
                                    driver.FindElement(By.XPath("//span[text() = 'Until I turn it back on']")).Click();
                                    isWorking = true;
                                    Thread.Sleep(1000);
                                }
                                catch (Exception) { }
                                if (!isWorking)
                                {
                                    try
                                    {
                                        driver.FindElement(By.XPath("/html/body/div[5]/div[1]/div/div[2]/div/div/div/div[4]/div/div[3]")).Click();
                                        isWorking = true;
                                        Thread.Sleep(1000);
                                    }
                                    catch (Exception) { }
                                }
                            } while (counter-- > 0 && !isWorking);
                            if (isWorking)
                            {
                                isWorking = false;
                                try
                                {
                                    driver.FindElement(By.XPath("//div[@aria-label='Disable']")).Click();
                                    Thread.Sleep(1000);
                                    isWorking = true;
                                }
                                catch (Exception) { }
                                if (!isWorking)
                                {
                                    try
                                    {
                                        driver.FindElement(By.XPath("//span[text() = 'Disable']")).Click();
                                        isWorking = true;
                                        Thread.Sleep(1000);
                                    }
                                    catch (Exception) { }
                                }
                                if (!isWorking)
                                {
                                    try
                                    {
                                        driver.FindElement(By.XPath("/html/body/div[5]/div[1]/div/div[2]/div/div/div/div[5]/div/div/div")).Click();
                                        isWorking = true;
                                        Thread.Sleep(1000);
                                    }
                                    catch (Exception) { }
                                }
                            }
                        }
                    }
                }
                if(!processActionData.NewsFeed.Messenger.ActiveStatus.None)
                {
                    MessengerOption();
                    isWorking = false;

                    if(processActionData.NewsFeed.Messenger.ActiveStatus.On)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//span[text() = 'Active Status: OFF']")).Click();
                            isWorking = true;
                            Thread.Sleep(1500);
                        }
                        catch (Exception) { }
                    } 
                    else if(processActionData.NewsFeed.Messenger.ActiveStatus.Off)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//span[text() = 'Active Status: ON']")).Click();
                            isWorking = true;
                            Thread.Sleep(1500);
                        }
                        catch (Exception) { }
                    }
                    if (isWorking)
                    {
                        isWorking = false;
                        try
                        {
                            driver.FindElement(By.XPath("//input[@aria-label='Web Visibility']")).Click();
                            Thread.Sleep(1000);
                            isWorking = true;
                        }
                        catch (Exception) { }
                        if(isWorking)
                        {
                            isWorking = false;
                            IWebElement element = null;
                            try
                            {
                                element = driver.FindElement(By.XPath("/html/body/div[7]/div[1]/div/div[2]/div/div/div/div/div/div[1]/div[3]/div[4]/div[2]/div[2]/div[1]/div/div[1]"));
                                isWorking = true;
                            }
                            catch (Exception) { }
                            if(!isWorking)
                            {
                                try
                                {
                                    element = driver.FindElement(By.XPath("/html/body/div[5]/div[1]/div/div[2]/div/div/div/div/div/div[1]/div[3]/div[3]/div[2]/div[2]/div[1]/div/div[1]"));
                                    isWorking = true;
                                }
                                catch (Exception) { }
                            }
                            if (!isWorking)
                            {
                                try
                                {
                                    element = driver.FindElement(By.XPath("/html/body/div[5]/div[1]/div/div[2]/div/div/div/div/div/div[1]/div[3]/div[4]/div[2]/div[2]/div[1]"));
                                    isWorking = true;
                                }
                                catch (Exception) { }
                            }
                            if (!isWorking)
                            {
                                try
                                {
                                    element = driver.FindElement(By.XPath("/html/body/div[6]/div[1]/div/div[2]/div/div/div/div/div/div[1]/div[3]/div[3]/div[2]/div[2]/div[1]"));
                                    isWorking = true;
                                }
                                catch (Exception) { }
                            }
                            if (isWorking)
                            {
                                try
                                {
                                    Actions actions = new Actions(driver);
                                    actions.MoveToElement(element)
                                        .Click()
                                        .Build()
                                        .Perform();
                                    
                                    Thread.Sleep(1500);
                                }
                                catch (Exception) { }
                            }
                        }
                    }
                }
            }
        }
        public bool ReadMessenger()
        {
            bool isWorking = false;
            int counter = 2;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Messenger']")).Click();
                    isWorking = true;
                    Thread.Sleep(2000);
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.Id("messages_jewel")).Click();
                        Thread.Sleep(2000);
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);

            return isWorking;
        }
        public void MessengerOption()
        {
            int counter = 4;
            bool isWorking = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//input[@aria-label='Incoming call sounds']"));
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Options']")).Click();
                        Thread.Sleep(1000);
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
        }
        public void Timeline()
        {
            int min = 1, max = 1;
            try
            {
                min = processActionData.NewsFeed.Timeline.MinNumber;
                max = processActionData.NewsFeed.Timeline.MaxNumber;
            }
            catch (Exception) { }
            int numberPost = GetRankNumber(min, max);
            for (int i = 0; i < numberPost; i++)
            {
                if(IsStop())
                {
                    break;
                }
                try
                {
                    driver.Navigate().GoToUrl(Constant.FB_WEB_URL);
                }
                catch (Exception) { }
                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(1000);
                var source = this.form.GetSourceTimeline(data.TimelineSource,processActionData.NewsFeed.Timeline.DeleteAfterPost);
                source = source.Replace('\\', '/').Trim();
                if (string.IsNullOrEmpty(source) || !File.Exists(source))
                {
                    break;
                }

                string caption = GetCaption();
                
                if (!string.IsNullOrEmpty(caption) || !string.IsNullOrEmpty(source))
                {
                    WebFBTool.PostTimeline(driver, caption, source, 3000, 30000);
                    Thread.Sleep(1000);
                    if(!string.IsNullOrEmpty(source) && File.Exists(source) && processActionData.NewsFeed.Timeline.DeleteAfterPost)
                    {
                        LocalData.DeleteFile(source);
                    }
                }
            }
        }
        public string GetCaption()
        {
            string str = "";
            try
            {
                if (captionIndex >= captionArr.Length)
                {
                    captionIndex = 0;
                }
                str = captionArr[sourceFolderFileIndex];
                captionIndex++;

                this.form.cacheViewModel.GetCacheDao().Set("group:view:source_index", captionIndex + "");
            }
            catch (Exception) { }

            return str;
        }
        public string GetSourceFile()
        {
            string str = "";
            try
            {
                if (sourceFolderFileIndex >= soureFolderFileArr.Length)
                {
                    sourceFolderFileIndex = 0;
                }
                str = soureFolderFileArr[sourceFolderFileIndex];
                sourceFolderFileIndex++;

                this.form.cacheViewModel.GetCacheDao().Set("group:view:source_index", sourceFolderFileIndex + "");
            }
            catch (Exception) { }

            return str;
        }
        public int GetRankNumber(int min, int max)
        {
            return new Random().Next(min, max);
        }
        public bool IsStop()
        {
            return this.form.IsStop();
        }
    }
}
