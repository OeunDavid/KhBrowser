using CsQuery.StringScanner.Patterns;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToolKHBrowser.ToolLib.Data;
using ToolKHBrowser.ToolLib.Tool;
using ToolLib.Data;
using ToolLib.Tool;
using WpfUI.ViewModels;
using WpfUI.Views;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace ToolKHBrowser.ViewModels
{
    public interface IPageViewModel
    {
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void Start(frmMain form, IWebDriver driver, FbAccount data);
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void Create();
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void Follow();
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void Backup();
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void CreateReel(bool isNoSwitchPage = false);
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        IPagesDao GetPagesDao();
    }
    public class PageViewModel : IPageViewModel
    {
        private IAccountDao accountDao;
        private ICacheDao cacheDao;
        private IPagesDao pageDao;
        private frmMain form;
        private FbAccount data;
        private IWebDriver driver;
        private ProcessActions processActionData;

        private string[] pageUrlArr;
        private string[] pageNameArr;
        private string[] pageCategoriesArr;
        private string[] pageBioArr;

        private int pageUrlIndex = 0;
        private int createNameIndex = 0;
        private int createCategoryIndex =0;
        private int createBioIndex = 0;

        public PageViewModel(IAccountDao accountDao, ICacheDao cacheDao, IPagesDao pageDao)
        {
            this.accountDao = accountDao;
            this.cacheDao = cacheDao;
            this.pageDao = pageDao;
        }
        public IPagesDao GetPagesDao()
        {
            return this.pageDao;
        }
        public void Start(frmMain form, IWebDriver driver, FbAccount data)
        {
            this.form = form;
            this.data = data;
            this.driver = driver;
            this.processActionData = this.form.processActionsData;

            try
            {
                createNameIndex = Int32.Parse(this.form.cacheViewModel.GetCacheDao().Get("page:config:name_index").Value.ToString());
            }
            catch (Exception) { }
            try
            {
                createCategoryIndex = Int32.Parse(this.form.cacheViewModel.GetCacheDao().Get("page:config:category_index").Value.ToString());
            }
            catch (Exception) { }
            try
            {
                createBioIndex = Int32.Parse(this.form.cacheViewModel.GetCacheDao().Get("page:config:bio_index").Value.ToString());
            }
            catch (Exception) { }
            try
            {
                pageUrlArr = this.processActionData.PageConfig.PageUrls.Split('\n');
            }
            catch (Exception) { }
            try
            {
                pageNameArr = this.processActionData.PageConfig.CreatePage.Names.Split('\n');
            }
            catch (Exception) { }
            try
            {
                pageCategoriesArr = this.processActionData.PageConfig.CreatePage.Categies.Split('\n');
            }
            catch (Exception) { }
            try
            {
                pageBioArr = this.processActionData.PageConfig.CreatePage.Bio.Split('\n');
            }
            catch (Exception) { }
        }
        public void CreateReel(bool isNoSwitchPage = false)
        {
            int create = processActionData.PageConfig.CreateReel.CreateNumber;
            if(create > 0)
            {
                data.Description = "Create Reel";
                var des = data.Description;
                int totalCreate = 0;

                bool muliplePage = true;// new update switch page

                if (isNoSwitchPage || muliplePage)
                {
                    for (int j = 0; j < create; j++)
                    {
                        if (IsStop())
                        {
                            break;
                        }
                        CreateTab();
                        var source = this.form.GetSourceReelVideo(data.ReelSourceVideo);
                        source = source.Replace('\\', '/').Trim();
                        if (string.IsNullOrEmpty(source) || !File.Exists(source))
                        {
                            break;
                        }
                        StartCreateReel(source);
                        if (File.Exists(source))
                        {
                            LocalData.DeleteFile(source);
                        }

                        totalCreate++;
                        data.Description = des + " " + totalCreate;
                    }
                }
                else
                {
                    string[] pageArr = data.PageIds.Split(',');
                    bool isBreak = false;
                    for (int i = 0; i < pageArr.Length; i++)
                    {
                        int number = create;
                        if (IsStop() || isBreak)
                        {
                            break;
                        }
                        try
                        {
                            string[] p = pageArr[i].Split('|');

                            string pageId = p[0].Trim();
                            var isSwitch = WebFBTool.SwitchToProfilePage(driver, pageId);

                            if (!isSwitch) { continue; }

                            string follower = WebFBTool.GetPageFollow(driver);
                            if (!string.IsNullOrEmpty(follower))
                            {
                                try
                                {
                                    string note = data.Note;
                                    if (!string.IsNullOrEmpty(note))
                                    {
                                        string[] arr = note.Split(':');
                                        note = arr[0].Trim();
                                    }
                                    note += ":" + follower;
                                    accountDao.addNote(data.UID, note);
                                    data.Note = note;
                                }
                                catch (Exception) { }
                            }
                            for (int j = 0; j < number; j++)
                            {
                                if (IsStop())
                                {
                                    break;
                                }
                                CreateTab();
                                var source = this.form.GetSourceReelVideo(data.ReelSourceVideo);
                                source = source.Replace('\\', '/').Trim();
                                if (string.IsNullOrEmpty(source) || !File.Exists(source))
                                {
                                    isBreak = true;
                                    break;
                                }
                                StartCreateReel(source);
                                if (File.Exists(source))
                                {
                                    LocalData.DeleteFile(source);
                                }

                                totalCreate++;
                                data.Description = des + " " + totalCreate;
                            }
                        }
                        catch (Exception) { }

                        CloseTab();
                    }
                }

                CloseTab();
            }
        }
        public void CloseTab()
        {
            try
            {
                var tabs = driver.WindowHandles;
                foreach (var tab in tabs)
                {
                    // "tab" is a string like "CDwindow-6E793DA3E15E2AB5D6AE36A05344C68"
                    if (tabs[0] != tab)
                    {
                        driver.SwitchTo().Window(tab);
                        driver.Close();
                    }
                }
                driver.SwitchTo().Window(driver.WindowHandles[0]);
            }
            catch (Exception) { }
        }
        public void StartCreateReel(string source)
        {
            try
            {
                driver.Navigate().GoToUrl("https://www.facebook.com/reels/create/?surface=ADDL_PROFILE_PLUS");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            FBTool.Close(driver);
            Thread.Sleep(3000);
            int counter = 6;
            bool isWorking = false;
            IWebElement input = null;
            do
            {
                Thread.Sleep(1000);
                try
                {
                    input = driver.FindElement(By.XPath("//input[@accept='video/*,video/mp4,video/x-m4v,video/x-matroska,.mkv']"));
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        input = driver.FindElement(By.XPath("//input[@type='file']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!IsStop() && !isWorking && counter-- > 0);
            if(!isWorking) { return; }
            if (IsStop())
            {
                return;
            }
            try
            {
                input.SendKeys(source);
            }
            catch (Exception) { }
            counter = 60;
            isWorking = false;
            do
            {
                Thread.Sleep(500);
                if(driver.PageSource.Contains("Replace video") || driver.PageSource.Contains("Replace Video") || driver.PageSource.Contains("replace video"))
                {
                    isWorking = true;
                }
            } while (!IsStop() && !isWorking && counter-- > 0);
            if(IsStop())
            {
                return;
            }
            Thread.Sleep(1000);
            counter = 20;
            isWorking = false;
            do
            { 
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Next']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div/div[1]/form/div/div/div[1]/div/div[3]/div[2]/div/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[5]/div/div/div[3]/form/div/div/div[1]/div/div[4]/div[2]/div[2]/div[1]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!IsStop() && !isWorking && counter-- > 0);
            if (IsStop())
            {
                return;
            }
            if (isWorking)
            {
                isWorking = false;
                counter = 10;
                IWebElement elementNext2 = null;
                do
                { 
                    Thread.Sleep(500);
                    if(!isWorking)
                    {
                        try
                        {
                            elementNext2 = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[3]/div/div/div[1]/form/div/div/div[1]/div/div[3]/div[2]/div[2]/div[1]"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementNext2 = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/form/div/div/div[1]/div/div[3]/div[2]/div[2]/div[1]"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementNext2 = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div/div[1]/form/div/div/div[1]/div/div[3]/div[2]/div[2]/div[1]"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementNext2 = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[5]/div/div/div[3]/div/div/div[1]/form/div/div/div[1]/div/div[3]/div[2]/div[2]/div[1]"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementNext2 = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/form/div/div/div[1]/div/div[3]/div[2]/div[2]/div[1]"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementNext2 = driver.FindElement(By.XPath("//span[contains(text(),'Next')]"));

                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementNext2 = driver.FindElement(By.XPath("//div[@aria-label='Next']"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
                if (IsStop())
                {
                    return;
                }
                if(!isWorking)
                {
                    return;
                }
                try
                {
                    WebFBTool.ClickElement(driver, elementNext2);
                } catch(Exception) { }
                var caption = this.form.GetReelCaption();
                var hashtag = processActionData.PageConfig.CreateReel.Hashtag;
                if(string.IsNullOrEmpty(caption))
                { 
                    string fileName = Path.GetFileName(source);
                    string fileNameOnly = fileName.Substring(0, fileName.LastIndexOf("."));
                    caption = fileNameOnly;
                }
                if(!string.IsNullOrEmpty(hashtag))
                {
                    caption += "\n" + hashtag;
                }
                if(!string.IsNullOrEmpty(caption))
                {
                    Thread.Sleep(2000);

                    isWorking = false;
                    counter = 6;
                    IWebElement webElement = null;
                    do
                    {
                        Thread.Sleep(500);
                        try
                        {
                            webElement = driver.FindElement(By.XPath("//div[@aria-label='Describe your reel...']/p/br"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                        if (!isWorking)
                        {
                            try
                            {
                                webElement = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div/div[1]/form/div/div/div[1]/div/div[2]/div[1]/div[2]/div/div/div/div/div[1]/div[1]/div[1]/div[1]/p/br"));
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isWorking)
                        {
                            try
                            {
                                webElement = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[3]/div/div/div[1]/form/div/div/div[1]/div/div[2]/div[1]/div[2]/div/div/div/div/div[1]/div[1]/div[1]/div[1]/p/br"));
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                    } while (!isWorking && counter-- > 0);
                    if(webElement != null)
                    {
                        try
                        {
                            Actions actions = new Actions(driver);
                            actions
                                .SendKeys(webElement, caption)
                                .Build()
                                .Perform();

                            Thread.Sleep(1000);
                        }
                        catch (Exception) { }
                    }
                }

                isWorking = false;
                counter = 15;
                IWebElement elementPublish = null;
                do
                {
                    Thread.Sleep(500);
                    //try
                    //{
                    //    elementPublish = driver.FindElement(By.XPath("//div[@aria-label='Publish']"));
                    //    isWorking = true;
                    //}
                    //catch (Exception) { }
                    if (!isWorking)
                    {
                        try
                        {
                            elementPublish = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[3]/div/div/div[1]/form/div/div/div[1]/div/div[3]/div[2]/div[2]/div[1]"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementPublish = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div/div[1]/form/div/div/div[1]/div/div[3]/div[2]/div[2]/div[1]"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementPublish = driver.FindElement(By.XPath("/html/b/html/body/div[1]/div/div[1]/div/div[5]/div/div/div[3]/div/div/div[1]/form/div/div/div[1]/div/div[3]/div[2]/div[2]/div[1]"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementPublish = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[5]/div/div/div[3]/div/div/div[1]/form/div/div/div[1]/div/div[3]/div[2]/div[2]/div[1]"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementPublish = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[5]/div/div/div[3]/form/div/div/div[1]/div/div[4]/div[2]/div[2]/div[1]"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementPublish = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/form/div/div/div[1]/div/div[3]/div[2]/div[2]/div[1]/div"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            elementPublish = driver.FindElement(By.XPath("//span[contains(text(),'Publish')]"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
                if (isWorking && elementPublish != null)
                {
                    try
                    {
                        //Actions actions = new Actions(driver);
                        //actions.MoveToElement(elementPublish).Click().Perform();

                        elementPublish.Click();
                    }
                    catch (Exception) { }
                    Thread.Sleep(5000);
                }
            }
        }
        public void Backup()
        {
            try
            {

                var graph = new FBGraph();
                graph.GetCookieContainerFromDriver(driver);

                data.PageIds = graph.GetBackupPage(data.Token).Trim();
                //data.PageIds = WebFBTool.GetPageIDs(driver, this.data.Token,data.UID);

                //if (data.PageIds.Contains(data.UID))
                //{
                //    data.PageIds = data.PageIds.Replace(data.UID, " ");
                //    data.PageIds = data.PageIds.Replace(" ,", "");
                //}
                if (!string.IsNullOrEmpty(data.PageIds))
                {
                    string[] pageArr = data.PageIds.Split(',');
                    data.TotalPage = pageArr.Length;
                } else
                {
                    data.TotalPage = 0;
                }
                this.form.fbAccountViewModel.getAccountDao().UpdatePage(data.UID, data.TotalPage,data.PageIds);
            }
            catch (Exception) { }
        }
        public void Create()
        {
            data.Description = "Create Page";
            for (int j = 1; j <= processActionData.PageConfig.CreatePage.CreateNumber; j++)
            {
                if (IsStop())
                {
                    break;
                }
                CreateTab();

                var mame = GetName();
                var category = GetCategory();
                var bio = GetBio();

                WebFBTool.CreatePage(driver, mame, category, bio);                
                Thread.Sleep(6000);
            }
            try
            {
                driver.SwitchTo().Window(driver.WindowHandles[0]);
                Thread.Sleep(500);
            }
            catch (Exception) { }
        }
        public void Follow()
        {
            data.Description = "Follow Page";
            for(int i = 0; i < pageUrlArr.Length; i++)
            {
                if (IsStop())
                {
                    break;
                }
                var url = "";
                try
                {
                    url = pageUrlArr[i];
                }
                catch (Exception) { }
                if(string.IsNullOrEmpty(url))
                {
                    continue;
                }
                FBTool.GoToFacebook(driver, url);

                WebFBTool.LikePage(driver);
                Thread.Sleep(1000);
            }
        }
        public void CreateTab()
        {
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("window.open();", Array.Empty<object>());
                Thread.Sleep(500); 
                driver.SwitchTo().Window(driver.WindowHandles[driver.WindowHandles.Count-1]);
            }
            catch (Exception) { }
        }
        public Random random = new Random();
        public string GetName()
        {
            string str = "";
            try
            {
                str = pageNameArr[random.Next(0,pageNameArr.Length-1)];
                //if (createNameIndex >= pageNameArr.Length)
                //{
                //    createNameIndex = 0;
                //}
                //str = pageNameArr[createNameIndex];
                //createNameIndex++;

                //this.form.cacheViewModel.GetCacheDao().Set("page:config:name_index", createNameIndex + "");
            }
            catch (Exception) { }

            return str;
        }
        public string GetCategory()
        {
            string str = "";
            try
            {
                str = pageCategoriesArr[random.Next(0, pageCategoriesArr.Length - 1)];
                //if (createCategoryIndex >= pageCategoriesArr.Length)
                //{
                //    createCategoryIndex = 0;
                //}
                //str = pageCategoriesArr[createCategoryIndex];
                //createCategoryIndex++;

                //this.form.cacheViewModel.GetCacheDao().Set("page:config:category_index", createCategoryIndex + "");
            }
            catch (Exception) { }

            return str;
        }
        public string GetBio()
        {
            string str = "";
            try
            {
                str = pageBioArr[random.Next(0, pageBioArr.Length - 1)];
                //if (createBioIndex >= pageBioArr.Length)
                //{
                //    createBioIndex = 0;
                //}
                //str = pageBioArr[createBioIndex];
                //createBioIndex++;

                //this.form.cacheViewModel.GetCacheDao().Set("page:config:bio_index", createBioIndex + "");
            }
            catch (Exception) { }

            return str;
        }
        public string GetUrl()
        {
            string str = "";
            try
            {
                if (pageUrlIndex < pageUrlArr.Length)
                {
                    str = pageUrlArr[pageUrlIndex];
                }
                pageUrlIndex++;
            }
            catch (Exception) { }

            return str;
        }
        public bool IsStop()
        {
            return this.form.IsStop();
        }
    }
}
