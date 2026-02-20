using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Xml.Linq;
using ToolKHBrowser.ToolLib.Data;
using ToolKHBrowser.ToolLib.Tool;
using ToolKHBrowser.Views;
using ToolLib.Data;
using ToolLib.Tool;
using ToolKHBrowser.ViewModels;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace ToolKHBrowser.ViewModels
{
    public interface IShareViewModel
    {
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void Start(frmMain form, FbAccount data);
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void ShareWatchDelay(IWebDriver driver, int timeDelay);
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void ShareWatchVideo(IWebDriver driver, string url, string comment = "");
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        bool ShareToTimeline(IWebDriver driver, string caption = "");
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void ShareToGroup(IWebDriver driver, bool shareByGraph = false);
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void ShareProfilePage(IWebDriver driver);
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void ShareWebsite(IWebDriver driver);
    }
    public class ShareViewModel : IShareViewModel
    {
        private IAccountDao accountDao;
        private ICacheDao cacheDao;
        private frmMain form;
        private FbAccount data;
        private int shareGroupIndex;
        private string[] shareArrGroupIDs;
        private ProcessActions processActionData;
        private Random random = new Random();

        public ShareViewModel(IAccountDao accountDao, ICacheDao cacheDao)
        {
            this.accountDao = accountDao;
            this.cacheDao = cacheDao;
        }
        public void Start(frmMain form, FbAccount data)
        {
            this.form = form;
            this.data = data;
            this.processActionData = this.form.processActionsData;

            shareGroupIndex = 0;
            shareArrGroupIDs = Array.Empty<string>();

            try
            {
                var cache = this.form.cacheViewModel.GetCacheDao().Get("share:groupIndex");
                if (cache != null && cache.Value != null)
                {
                    shareGroupIndex = Int32.Parse(cache.Value.ToString());
                }
            }
            catch (Exception) { }
            try
            {
                if (processActionData != null && processActionData.Share != null && processActionData.Share.ProfilePage != null && !string.IsNullOrEmpty(processActionData.Share.ProfilePage.GroupIDs))
                {
                    shareArrGroupIDs = this.form.processActionsData.Share.ProfilePage.GroupIDs.Split('\n');
                }
            }
            catch (Exception) { }
        }
        public string GetRanEmoji(bool emojiUrl = true)
        {
            string str = "";
            if (emojiUrl)
            {
                var rgch = "\U0001F642".ToCharArray();
                str = rgch[0] + "" + rgch[random.Next(1, rgch.Length - 1)];
            } else
            {
                char[] emojiStr = "⭐☹️✋✌️☝️✊✍️❤️‍❣️☘️".ToCharArray();
                if (emojiStr != null && emojiStr.Length > 0)
                {
                    str = emojiStr[random.Next(0, emojiStr.Length)].ToString();
                }
            }

            return str;
        }
        public string GetRanNumber(int digit = 1)
        {
            string str = "";
            string strMax = "9";
            string strMin = "1";
            for(int i = 1;i <= digit; i++)
            {
                strMin += "1";
                strMax += "9";
            }

            return random.Next(Int32.Parse(strMin),Int32.Parse(strMax)).ToString();
        }
        public string GetWord()
        {
            string[] str = "Wise,thoughts,parables,quotes,aphorisms,about,love,about,life,Puerto,Ricans,United,Davenport,Kissimmee,Orlando,Fraud,Scam,Travel,Agencies,Buying and selling in Valle de,Pedregal,Puebla and Monarca,Maestro,Autoparts,Atlanta,Entrepreneurs,Advice,Guidance,Networking for the Elite".Split(',');

            if (str.Length == 0) return "";
            return str[random.Next(0, str.Length)].ToString().ToLower();
        }
        public string GetHashTag()
        {
            string[] str = "life,fbviral,good,wow,omg,easy,lower,swag,low,high,love,wear,car,viral,like,virals,viral,watch,much,video,watching,videos,lover,liker,lifer,follower,follow,maper,reel,reels,music".Split(',');

            if (str.Length == 0) return "";
            return str[random.Next(0, str.Length)].ToString().ToLower();
        }
        public string GetMixCaption(string caption = "", bool emojiUrl = true)
        {
            string str = caption;

            if (string.IsNullOrEmpty(str))
            {
                for (int i = 1; i <= random.Next(2, 10); i++)
                {
                    str += GetWord() + " ";
                    //if (random.Next(0, 8) == 1)
                    //{
                    //    str += " " + GetRanNumber(1);
                    //}
                }
            }

            str += " #" + GetHashTag();
            if (random.Next(0, 4) == 1 || true)
            {
                str += GetRanNumber();
            }
            str += " " + GetRanEmoji(emojiUrl);
            if (random.Next(0, 2) == 1 || true)
            {
                str += GetRanEmoji(emojiUrl);
            }
            str += " #" + GetHashTag();
            if (random.Next(0, 4) == 1 || true)
            {
                str += GetRanNumber();
            }
            if (random.Next(0, 2) == 1 || true)
            {
                str += " " + GetRanEmoji(emojiUrl);
            }

            return str;
            return HttpUtility.UrlEncode(str); ;
        }
        public void APIShareToGroup(IWebDriver driver)
        {
            if (processActionData == null || processActionData.Share == null || processActionData.Share.GroupNumber == null || processActionData.Share.DelayEachShare == null)
                return;

            //
            data.Description = "API Share to Groups ";

            var graph = new FBGraph();

            graph.GetCookieContainerFromDriver(driver);

            //if (processActionData.IsBackupGroup || string.IsNullOrEmpty(data.GroupIDs))
            //{
            //    data.GroupIDs = WebFBTool.GetGroupIDs(driver, this.data.Token);
            //}

            // quit browser
            //string profile = ConfigData.GetBrowserKey(data.UID);
            //FBTool.QuitBrowser(driver, profile, this.data.UID);

            string[] arrGroupId = data.GroupIDs.Split(',');

            string des = data.Description;

            var url = this.form.GetURL();
            int sGN = this.form.processActionsData.Share.GroupNumber.NumberStart;
            int eGN = this.form.processActionsData.Share.GroupNumber.NumberEnd;
            int sES = this.form.processActionsData.Share.DelayEachShare.DelayStart;
            int eES = this.form.processActionsData.Share.DelayEachShare.DelayEnd;

            int delayEachOfShare = GetRankNumber(sES, eES);
            int gNumber = GetRankNumber(sGN, eGN);
            bool isCheckGroupPending = this.form.processActionsData.Share.CheckPending;

            int totalShare = 1, groupIndex = 0 ;

            for (int j = 1; j <= gNumber; j++)
            {
                if (IsStop())
                {
                    break;
                }
                var groupId = "" ;
                try
                {
                    groupId = arrGroupId[groupIndex].Trim();
                    groupIndex++;
                } catch(Exception) { }
                var isPending = 0;
                int countPending = 20;
                if (isCheckGroupPending)
                {
                    do
                    {
                        if (IsStop())
                        {
                            break;
                        }
                        isPending = graph.IsGroupPending(groupId);

                        if (isPending == 1)
                        {
                            try
                            {
                                groupId = arrGroupId[groupIndex].Trim();
                                groupIndex++;
                            }
                            catch (Exception) { }
                        }
                    } while (isPending == 1 && countPending-- > 0);
                    if(isPending == 1)
                    {
                        groupId = "";
                    }
                }
                if (string.IsNullOrEmpty(groupId))
                {
                    break;
                }
                string caption = GetMixCaption();
                if (!string.IsNullOrEmpty(caption))
                {
                    //if (new Random().Next(0, 4) == 1)
                    //{
                    //    caption = caption + " " + GetRandCaption(6);
                    //}
                    caption= HttpUtility.UrlEncode(caption);
                }

                string param = "access_token=" + data.Token.Trim() + "&message=" + caption + "&link=" + url.Trim();

                string respone = graph.HTTPWebRequest("POST", "https://graph.facebook.com/v10.0/" + groupId.Trim() + "/feed", param);
                Thread.Sleep(delayEachOfShare);

                totalShare++;
                data.Description = des + " " + totalShare;
            }

            // update message
            accountDao.updateStatus(data.UID, data.Description, 1);
        }
        public void ShareToGroup(IWebDriver driver, bool shareByGraph = false)
        {
            if (processActionData == null || processActionData.Share == null || processActionData.Share.GroupNumber == null || processActionData.Share.DelayEachShare == null)
                return;

            bool isWorking = false;
            if(shareByGraph)
            {
                APIShareToGroup(driver);

                return;
            }
            // update message
            data.Description += ", Share to Groups ";
            accountDao.updateStatus(data.UID, data.Description, 1);

            string str_path = "/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/div[2]/div/div/div[2]/div/div[1]/div/div/div[2]/div/div";
            string str_path1 = "/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[2]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/div[2]/div/div/div[2]/div/div[1]/div/div/div[2]/div/div";
            string str_path2 = "/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/div[2]/div[2]/div/div[2]/div/div[1]/div/div/div[2]/div/div";
            string str_path3 = "/html/body/div[1]/div/div[1]/div/div[6]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/div[2]/div/div/div[2]/div/div[1]/div/div/div[2]/div/div";
            string str_path4 = "/html/body/div[6]/div[1]/div/div[2]/div/div/div/div/div[1]/div[2]/div/div/div[2]/div/div[1]/div/div/div/div[2]/div/div";
            string str_path5 = "/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/div[2]/div/div/div[2]/div/div[1]/div/div/div/div[2]/div/div";
            string str_path6 = "/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[2]/div/div[2]/div[2]/div/div/div/div[2]/div/div[1]/div/div/div[2]/div/div";
            string str_path7 = "/html/body/div[6]/div[1]/div/div[2]/div/div/div/div/div[2]/div/div[2]/div[2]/div/div/div/div[2]/div/div[1]/div/div/div[2]/div/div";
            string str_path8 = "/html/body/div[1]/div/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[2]/div/div[2]/div[2]/div/div/div/div[2]/div/div[1]/div/div/div[2]/div/div";
            string str_path9 = "/html/body/div[1]/div/div[1]/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[3]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br";

            string des = data.Description;
            bool isContinue, isShare = true;
            int delayEachShare = GetEachDelayOfShare(this.form.processActionsData.Share.DelayEachShare.DelayStart, this.form.processActionsData.Share.DelayEachShare.DelayEnd);
            int groupNumber = GetShareGroupNumber(this.form.processActionsData.Share.GroupNumber.NumberStart, this.form.processActionsData.Share.GroupNumber.NumberEnd);
            int counter = 0, countShare = 0;

            for (int i = 1; i <= groupNumber && isShare; i++)
            {
                if (i > 1)
                {
                    Thread.Sleep(delayEachShare);
                }
                bool isClose = FBTool.Close(driver);
                if (isClose)
                {
                    Thread.Sleep(1000);
                }
                if (i > 1)
                {
                    FBTool.ScrollUp(driver, 3000);
                }
                isContinue = false;
                if (IsStop())
                {
                    break;
                }
                isShare = false;
                // goto share button
                isWorking = GotoShare(driver);
                if (!isWorking)
                {
                    break;
                }
                if (IsStop())
                {
                    break;
                }
                string path = str_path + "[" + i + "]";
                string path1 = str_path1 + "[" + i + "]";
                string path2 = str_path2 + "[" + i + "]";
                string path6 = str_path3 + "[" + i + "]";
                string path8= str_path4 + "[" + i + "]";
                string path9= str_path5 + "[" + i + "]";
                string path6_1= str_path6 + "[" + i + "]";
                string path7_1= str_path7 + "[" + i + "]";
                string path8_1= str_path8 + "[" + i + "]";
                string path9_1= str_path8 + "[" + i + "]";

                // for test
                string path3 = str_path + "[" + (i + 1) + "]";
                string path4 = str_path1 + "[" + (i + 1) + "]";
                string path5 = str_path2 + "[" + (i + 1) + "]";

                string path7 = str_path3 + "[" + (i + 1) + "]";
                string path10 = str_path4 + "[" + (i + 1) + "]";
                string path11 = str_path5 + "[" + (i + 1) + "]";
                string path6_2 = str_path6 + "[" + (i + 1) + "]";
                string path7_2 = str_path7 + "[" + (i + 1) + "]";
                string path8_2 = str_path8 + "[" + (i + 1) + "]";
                string path9_2 = str_path8 + "[" + (i + 1) + "]";

                Thread.Sleep(1000);
                isWorking = false;
                counter = 8;
                do
                {
                    if (IsStop())
                    {
                        break;
                    }
                    Thread.Sleep(1000);

                    // test for continue
                    try
                    {
                        driver.FindElement(By.XPath(path3));
                        isContinue = true;
                    }
                    catch (Exception) { }
                    if (!isContinue)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path4));
                            isContinue = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isContinue)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path5));
                            isContinue = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isContinue)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path7));
                            isContinue = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isContinue)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path10));
                            isContinue = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isContinue)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path11));
                            isContinue = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isContinue)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path6_2));
                            isContinue = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isContinue)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path7_2));
                            isContinue = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isContinue)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path8_2));
                            isContinue = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isContinue)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path9_2));
                            isContinue = true;
                        }
                        catch (Exception) { }
                    }
                    // end test for continue

                    try
                    {
                        driver.FindElement(By.XPath(path)).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path1)).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path2)).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path6)).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path8)).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path9)).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path6_1)).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path7_1)).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path8_1)).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path9_1)).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
                if (isWorking)
                {
                    Thread.Sleep(4000);
                    SendCaption(driver);
                }
                if (!isWorking)
                {
                    break;
                }
                if (IsStop())
                {
                    break;
                }
                isShare = false;
                if (isWorking)
                {
                    counter = 8;
                    Thread.Sleep(1000);
                    do
                    {
                        if (IsStop())
                        {
                            break;
                        }
                        Thread.Sleep(1000);
                        try
                        {
                            driver.FindElement(By.XPath("//div[@aria-label='Post']")).Click();
                            countShare++;
                            isShare = true;
                            Thread.Sleep(4000);
                        }
                        catch (Exception) { }
                    } while (!isShare && counter-- > 0);
                }
                if (isShare)
                {
                    this.data.Description = des + " " + countShare;
                }
                if (!isContinue)
                {
                    break;
                }
            }

            if (countShare > 0)
            {
                long total = 0;
                try
                {
                    total = long.Parse(cacheDao.Get("share:shareToGroup").Total.ToString());
                }
                catch (Exception) { }
                total = total + countShare;
                cacheDao.SetTotal("share:shareToGroup", total);

                // update share to groups
                accountDao.updateShareToGroup(this.data.UID, countShare);

                this.data.TotalShareGroup = this.data.TotalShareGroup + countShare;
            }
        }
        public void SendCaption(IWebDriver driver)
        {
            string caption = form.GetCaption();
            if (processActionData.Share.MixContent)
            {
                caption = GetMixCaption(caption,false);
            }

            if (!string.IsNullOrEmpty(caption))
            {
                bool isCap = false;
                IWebElement element = null;

                try
                {
                    element = driver.FindElement(By.XPath("//br[@data-text='true']"));
                    isCap = true;
                }
                catch (Exception) { }
                if (!isCap)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[2]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br"));
                        isCap = true;
                    }
                    catch (Exception) { }
                }
                if (!isCap)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[2]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div/div/div/div/div/div/div[2]/div/div/div/div/span/br"));
                        isCap = true;
                    }
                    catch (Exception) { }
                }
                if (!isCap)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[2]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br"));
                        isCap = true;
                    }
                    catch (Exception) { }
                }
                if (!isCap)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[2]/div[1]/div[1]/div[1]/div/div/div[1]/p/br"));
                        isCap = true;
                    }
                    catch (Exception) { }
                }
                if (!isCap)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[3]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br"));
                        isCap = true;
                    }
                    catch (Exception) { }
                }
                if (!isCap)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div/div/div[2]/div[2]/div/div/div/div[1]/form/div/div/div[2]/div/div/div/div[1]/div/div[1]/div[1]/p/br"));
                        isCap = true;
                    }
                    catch (Exception) { }
                }
                if (!isCap)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div/div/div[2]/div[2]/div/div/div/div[1]/form/div/div/div[2]/div/div/div/div[1]/div/div[1]/div[1]/p/br"));
                        isCap = true;
                    }
                    catch (Exception) { }
                }
                if (!isCap)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[3]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br"));
                        isCap = true;
                    }
                    catch (Exception) { }
                }
                if (isCap)
                {
                    //caption = "⭐☹️✋✌️☝️✊✍️❤️‍❣️☘️";
                    //caption = "😀";
                    try
                    {
                        Actions actions = new Actions(driver);
                        actions
                            .SendKeys(element,caption)
                            .Build()
                            .Perform();

                        //PopulateElementJs(driver,element,caption);
                    }
                    catch (Exception) { }
                }
            }
        }
        public string EncodeEmojiToUnicode(string emojiString)
        {
            string unicodeString = "";
            foreach (char c in emojiString)
            {
                unicodeString += "\\u" + ((int)c).ToString("X4");
            }
            return unicodeString;
        }
        public string GetEmoji(int numberOfEmoji)
        {
            // Define an array of emojis
            string[] emojis = { "😊", "😂", "😍", "🥰", "😎" };
            // Initialize an empty string to store the pattern
            string pattern = "";
            Random ran = new Random();

            // Generate the emoji pattern
            for (int i = 0; i < numberOfEmoji; i++)
            {
                pattern += emojis[ran.Next(0, emojis.Length - 1)] + " ";
            }

            return pattern;
        }
        public static void PopulateElementJs(IWebDriver driver, IWebElement element, string text)
        {
            var script = "arguments[0].innerHTML=' " + text + " ';";
            ((IJavaScriptExecutor)driver).ExecuteScript(script, element);
        }
        public void ShareWatchVideo(IWebDriver driver, string url, string comment = "")
        {
            FBTool.GoToFacebook(driver, url);
            // watch video before share

            // like video
            bool isLike = this.form.processActionsData.Share.Groups.React.Like;
            if (!isLike && this.form.processActionsData.Share.Groups.React.Random)
            {
                if (this.form.random.Next(0, 4) == 2)
                {
                    isLike = true;
                }
            }
            if (isLike)
            {
                WebFBTool.LikePost(driver);
            }
            // comment on video
            if (!string.IsNullOrEmpty(comment))
            {
                WebFBTool.PostComment(driver, comment);
            }
            ShareWatchDelay(driver, this.form.processActionsData.Share.Groups.WatchBeforeShare);
        }
        public void ShareWatchDelay(IWebDriver driver, int timeDelay)
        {
            do
            {
                if (this.form.IsStop())
                {
                    break;
                }
                timeDelay--;
                Thread.Sleep(1000);
            } while (timeDelay-- > 0);
        }
        public bool ShareToTimeline(IWebDriver driver, string caption = "")
        {
            bool isWorking = false;
            int counter;

            data.Description = data.Description+ ", Share Timeline ";

            // goto share button
            isWorking = ClickButtonShare(driver);
            if (!isWorking)
            {
                return false;
            }
            if (isWorking)
            {
                isWorking = false;
                counter = 10;
                IWebElement element = null;
                do
                {
                    Thread.Sleep(500);
                    try
                    {
                        element = driver.FindElement(By.XPath("//*[text() = 'Share now (Public)']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("//*[text() = 'Share now (Friends)']"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("//*[text() = 'Share now (Friends of Friends)']"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("//*[text() = 'Share now (Friends of Friends)']"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            element = driver.FindElement(By.XPath("//*[text() = 'Share to Feed']"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (isWorking)
                    {
                        WebFBTool.ClickElement(driver, element);
                    } else
                    {
                        IWebElement eShare = GetElementShareButton(driver);
                        if(eShare != null)
                        {
                            isWorking = true;
                        }
                    }
                } while (!isWorking && counter-- > 0);
            }
            if(!isWorking)
            {
                return false;
            }
            isWorking = false;
            counter = 10;
            IWebElement elementShareButton = null;
            do
            {
                Thread.Sleep(500);
                elementShareButton = GetElementShareButton(driver);

                if (elementShareButton != null)
                {
                    isWorking = true;
                    SendCaption(driver);
                    Thread.Sleep(1000);
                    //try
                    //{
                    //    Actions actions = new Actions(driver);
                    //    actions.MoveToElement(elementShareButton).Click().Perform();
                    //    Thread.Sleep(3000);
                    //}
                    //catch (Exception) { }
                    WebFBTool.ClickElement(driver, elementShareButton);
                    Thread.Sleep(5000);
                }
            } while (!isWorking && counter-- > 0);
            int share = 0;
            if (isWorking)
            {
                long total = 0;
                try
                {
                    total = long.Parse(cacheDao.Get("share:shareToTimeline").Total.ToString());
                }
                catch (Exception) { }
                total = total + 1;
                cacheDao.SetTotal("share:shareToTimeline", total);
                share = 1;
            }
            else
            {
                // check for Reels share now / share to feed
                isWorking = false;
                counter = 10;
                do
                {
                    Thread.Sleep(1000);
                    try
                    {
                        // Reels share button often has different label or just a specific icon
                        IWebElement reelShare = driver.FindElement(By.XPath("//div[@aria-label='Share now' or @aria-label='Share to Feed' or @aria-label='Post']"));
                        WebFBTool.ClickElement(driver, reelShare);
                        isWorking = true;
                        share = 1;
                        Thread.Sleep(5000);
                    }
                    catch (Exception) { }
                } while (!isWorking && counter-- > 0);
            }
            data.Description += share;
            accountDao.updateStatus(data.UID, data.Description, 1);

            return isWorking;
        }
        public IWebElement GetElementShareButton(IWebDriver driver)
        {
            IWebElement elementShareButton = null;
            bool isWorking = false;

            if (!isWorking)
            {
                try
                {
                    elementShareButton = driver.FindElement(By.XPath("//span[text() = 'Share now']"));
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    elementShareButton = driver.FindElement(By.XPath("//div[@aria-label='Share']"));
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    elementShareButton = driver.FindElement(By.XPath("//span[text() = 'Share']"));
                    isWorking = true;
                }
                catch (Exception) { }
            }

            if (!isWorking)
            {
                try
                {
                    elementShareButton = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div/div/div[2]/div[2]/div/div/div/div[2]/form/div/div/div[3]/div/div/div/div[1]"));
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    elementShareButton = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/form/div/div[1]/div/div/div/div[3]/div[2]/div/div"));
                    isWorking = true;
                }
                catch (Exception) { }
            }
            if (!isWorking)
            {
                try
                {
                    elementShareButton = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div/div/div[2]/div[2]/div/div/div/div[1]/form/div/div/div[3]/div/div/div/div[1]/div"));
                    isWorking = true;
                }
                catch (Exception) { }
            }

            return elementShareButton;
        }
        public int GetEachDelayOfShare(int min, int max)
        {
            return this.form.random.Next(min, max);
        }
        public int GetShareGroupNumber(int min, int max)
        {
            return this.form.random.Next(min, max);
        }
        public void ShareToGroup(IWebDriver driver, FbAccount data, string caption = "", string comment = "")
        {
            bool isWorking = true;

            // update message
            data.Description += ", Share to Groups ";
            accountDao.updateStatus(data.UID, data.Description, 1);

            string str_path = "/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/div[2]/div/div/div[2]/div/div[1]/div/div/div[2]/div/div";
            string str_path1 = "/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[2]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/div[2]/div/div/div[2]/div/div[1]/div/div/div[2]/div/div";
            string str_path2 = "/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[1]/div[2]/div[2]/div/div[2]/div/div[1]/div/div/div[2]/div/div";

            string des = data.Description;
            bool isContinue;

            int sGN = this.form.processActionsData.Share.GroupNumber.NumberStart;
            int eGN = this.form.processActionsData.Share.GroupNumber.NumberEnd;
            int sES = this.form.processActionsData.Share.DelayEachShare.DelayStart;
            int eES = this.form.processActionsData.Share.DelayEachShare.DelayEnd;

            int delayEachOfShare = GetRankNumber(sES, eES);
            int gNumber = GetRankNumber(sGN, eGN);
            bool isShare = true;
            int counter = 0, countShare = 0;

            for (int i = 1; i <= gNumber && isShare; i++)
            {
                if(i > 1)
                {
                    ShareWatchDelay(driver, delayEachOfShare);
                }
                bool isClose = FBTool.Close(driver);
                if (isClose)
                {
                    Thread.Sleep(1000);
                }
                if (i > 1)
                {
                    FBTool.ScrollUp(driver, 3000);
                }
                isContinue = false;
                if (IsStop())
                {
                    break;
                }
                isShare = false;
                // goto share button
                isWorking = GotoShare(driver);
                if (!isWorking)
                {
                    break;
                }
                if (IsStop())
                {
                    break;
                }
                string path = str_path + "[" + i + "]";
                string path1 = str_path1 + "[" + i + "]";
                string path2 = str_path2 + "[" + i + "]";
                // for test
                string path3 = str_path + "[" + (i + 1) + "]";
                string path4 = str_path1 + "[" + (i + 1) + "]";
                string path5 = str_path2 + "[" + (i + 1) + "]";

                Thread.Sleep(1000);
                isWorking = false;
                counter = 8;
                do
                {
                    if (IsStop())
                    {
                        break;
                    }
                    Thread.Sleep(1000);

                    // test for continue
                    try
                    {
                        driver.FindElement(By.XPath(path3));
                        isContinue = true;
                    }
                    catch (Exception) { }
                    if (!isContinue)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path4));
                            isContinue = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isContinue)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path5));
                            isContinue = true;
                        }
                        catch (Exception) { }
                    }
                    // end test for continue

                    try
                    {
                        driver.FindElement(By.XPath(path)).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path1)).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath(path2)).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
                if (isWorking)
                {
                    Thread.Sleep(4000);
                    if (!string.IsNullOrEmpty(caption))
                    {
                        bool isCap = false;

                        try
                        {
                            driver.FindElement(By.XPath("//br[@data-text='true']")).SendKeys(caption + " ");
                            isCap = true;
                        }
                        catch (Exception) { }
                        if (!isCap)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[2]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div/div/div/div/div[2]/div/div/div/div/span/br")).SendKeys(caption);
                                isCap = true;
                            }
                            catch (Exception) { }
                        }
                        if (!isCap)
                        {
                            try
                            {
                                driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div[2]/form/div/div[1]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[1]/div/div/div/div/div/div/div[2]/div/div/div/div/span/br")).SendKeys(caption);
                                isCap = true;
                            }
                            catch (Exception) { }
                        }
                    }
                }
                if (!isWorking)
                {
                    break;
                }
                if (IsStop())
                {
                    break;
                }
                isShare = false;
                if (isWorking)
                {
                    counter = 8;
                    Thread.Sleep(1000);
                    do
                    {
                        if (IsStop())
                        {
                            break;
                        }
                        Thread.Sleep(1000);
                        try
                        {
                            driver.FindElement(By.XPath("//div[@aria-label='Post']")).Click();
                            countShare++;
                            isShare = true;
                            Thread.Sleep(4000);
                        }
                        catch (Exception) { }
                    } while (!isShare && counter-- > 0);
                }
                if (isShare)
                {
                    data.Description = des + " " + countShare;
                }
                if (!isContinue)
                {
                    break;
                }
            }
            
            if (countShare > 0)
            {
                long total = 0;
                try
                {
                    total = long.Parse(cacheDao.Get("share:shareToGroup").Total.ToString());
                }
                catch (Exception) { }
                total = total + countShare;
                cacheDao.SetTotal("share:shareToGroup", total);

                // update share to groups
                accountDao.updateShareToGroup(data.UID, countShare);

                data.TotalShareGroup = data.TotalShareGroup + countShare;
            }
            // update message
            accountDao.updateStatus(data.UID, data.Description, 1);
        }
        public void ShareProfilePage(IWebDriver driver)
        {
            try
            {
                driver.Manage().Cookies.DeleteCookieNamed("i_user");
                //driver.Navigate().Refresh();
                Thread.Sleep(1000);
            }
            catch (Exception) { }

            //
            data.Description += ", Share Profile Page ";

             var graph = new FBGraph();

            graph.GetCookieContainerFromDriver(driver);

            if (processActionData.BackupPage || string.IsNullOrEmpty(data.PageIds))
            {
                data.PageIds = WebFBTool.GetPageIDs(driver, this.data.Token);
            }
            // quit browser
            string profile = ConfigData.GetBrowserKey(data.UID);
            FBTool.QuitBrowser(driver, profile, this.data.UID);

            int maxCommentOnPost = 100;
            int commentOnPost = 0, totalShare = 0;
            string des = data.Description;

            string[] pageArr = data.PageIds.Split(',');
            data.TotalPage = pageArr.Length;
            this.form.fbAccountViewModel.getAccountDao().UpdatePage(data.UID, data.TotalPage, data.PageIds);

            for (int i = 0; i < pageArr.Length; i++)
            {
                if (IsStop())
                {
                    break;
                }

                string[] p = pageArr[i].Split('|');

                if (p.Length < 2) { continue; }
                string pageId = p[0].Trim();

                var access_token = p[1].Trim();

                if (i == 0 && commentOnPost < maxCommentOnPost)
                {
                    string comment = this.form.GetComment() + " " + GetRandCaption(6);
                    if (!string.IsNullOrEmpty(comment))
                    {
                        string obj_id = this.form.processActionsData.Share.ProfilePage.CommentObject;
                        if (!string.IsNullOrEmpty(obj_id))
                        {
                            var res = graph.CommentOnPost(obj_id.Trim(), access_token, comment);

                            commentOnPost++;
                        }
                    }
                }

                var url = this.form.GetURL();
                int sGN = this.form.processActionsData.Share.GroupNumber.NumberStart;
                int eGN = this.form.processActionsData.Share.GroupNumber.NumberEnd;
                int sES = this.form.processActionsData.Share.DelayEachShare.DelayStart;
                int eES = this.form.processActionsData.Share.DelayEachShare.DelayEnd;

                int delayEachOfShare = GetRankNumber(sES, eES);
                int gNumber = GetRankNumber(sGN, eGN);
                bool isCheckGroupPending = this.form.processActionsData.Share.CheckPending;

                for (int j = 1; j <= gNumber; j++)
                {
                    if (IsStop())
                    {
                        break;
                    }
                    var groupId = GetGroupID().Trim();
                    string caption = GetMixCaption();// this.form.GetCaption();
                    var isPending = 0;
                    int countPending = 20;
                    if (isCheckGroupPending)
                    {
                        do
                        {
                            if (IsStop())
                            {
                                break;
                            }
                            isPending = graph.IsGroupPending(groupId);

                            if (isPending == 1)
                            {
                                groupId = GetGroupID();
                            }
                        } while (isPending == 1 && countPending-- > 0);
                    }

                    if (!string.IsNullOrEmpty(caption))
                    {
                        //if (new Random().Next(0, 4) == 1)
                        //{
                        //    caption = caption + " " + GetRandCaption(6);
                        //}
                        caption = HttpUtility.UrlEncode(caption);
                    }

                    string param = "access_token=" + access_token.Trim() + "&message=" + caption + "&link=" + url.Trim();

                    string respone = graph.HTTPWebRequest("POST", "https://graph.facebook.com/v10.0/" + groupId.Trim() + "/feed", param);
                    Thread.Sleep(delayEachOfShare);

                    totalShare++;
                    data.Description = des + " " + totalShare;
                }
            }
            // update message
            accountDao.updateStatus(data.UID, data.Description, 1);
        }
        public string GetGroupID()
        {
            string groupID = "";
            try
            {
                if (shareGroupIndex >= shareArrGroupIDs.Length)
                {
                    shareGroupIndex = 0;
                }
                groupID = shareArrGroupIDs[shareGroupIndex];
                shareGroupIndex++;

                this.form.cacheViewModel.GetCacheDao().Set("share:groupIndex", shareGroupIndex + "");
            }
            catch (Exception) { }

            return groupID;
        }
        public bool IsStop()
        {
            return this.form.IsStop();
        }
        public int GetRankNumber(int min, int max)
        {
            return new Random().Next(min, max);
        }
        public string GetRandCaption(int digit)
        {
            int min = 100000, max = 999999;

            return new Random().Next(min, max) + "";
        }

        public string GetPictureSoure(string[] arr, string key)
        {
            string path = "";
            int index = 0;
            try
            {
                var cache = this.form.cacheViewModel.GetCacheDao().Get(key);
                if (cache != null && cache.Value != null)
                {
                    index = Int32.Parse(cache.Value.ToString());
                }
            }
            catch (Exception) { }
            try
            {
                if (index >= arr.Length)
                {
                    index = 0;
                }

                if (arr.Length > 0)
                {
                    path = arr[index];
                }

                index++;

                this.form.cacheViewModel.GetCacheDao().Set(key, index + "");
            }
            catch (Exception) { }

            return path;
        }
        public string GetShareWebsiteLink(string[] arr, string key)
        {
            string link = "";
            int index = 0;
            key = key + "link";
            try
            {
                var cache = this.form.cacheViewModel.GetCacheDao().Get(key);
                if (cache != null && cache.Value != null)
                {
                    index = Int32.Parse(cache.Value.ToString());
                }
            }
            catch (Exception) { }
            try
            {
                if (index >= arr.Length)
                {
                    index = 0;
                }

                if (arr.Length > 0)
                {
                    link = arr[index];
                }

                index++;

                this.form.cacheViewModel.GetCacheDao().Set(key, index + "");
            }
            catch (Exception) { }

            return link;
        }
        public void ShareWebsite(IWebDriver driver)
        {
            int sGN = this.form.processActionsData.Share.GroupNumber.NumberStart;
            int eGN = this.form.processActionsData.Share.GroupNumber.NumberEnd;
            int sES = this.form.processActionsData.Share.DelayEachShare.DelayStart;
            int eES = this.form.processActionsData.Share.DelayEachShare.DelayEnd;

            int delayEachOfShare = GetRankNumber(sES, eES);
            int gNumber = GetRankNumber(sGN, eGN);
                        
            string folder = processActionData.Share.Website.Folder.Trim();
            bool pastLink = processActionData.Share.Website.PastLink;

            int maxPhoto = 10;
            string multipleImageStr = "";
            string caption = "";
            string link = "";
            if(processActionData.Share.Website.RandomContent && !string.IsNullOrEmpty(folder))
            {
                string[] folders = System.IO.Directory.GetDirectories(@folder);
                if(folders != null && folders.Length > 0)
                {
                    folder = folders[random.Next(0, folders.Length)];

                    string pathContent = folder+"\\caption.txt";
                    string pathLink = folder+"\\link.txt";
                    
                    caption = LocalData.ReadTextFromTextFile(@pathContent,true);
                    link = LocalData.ReadTextFromTextFile(pathLink,true).Trim();
                }
            } else
            {
                caption = processActionData.Share.Captions;
                link = processActionData.Share.Urls;
            }

            string[] fileArr = LocalData.GetFiles(folder);
            if (!string.IsNullOrEmpty(folder))
            {
                if (fileArr != null)
                {
                    if (processActionData.Share.Website.RandomPicture)
                    {
                        int counter = 6;
                        int fileLength = fileArr.Length;
                        //do
                        //{
                            multipleImageStr = GetPictureSoure(fileArr, folder);
                            
                            if (multipleImageStr.Contains(".txt"))
                            {
                                multipleImageStr = "";
                                if (fileLength > 1)
                                {
                                    multipleImageStr = GetPictureSoure(fileArr, folder);
                                    if (multipleImageStr.Contains(".txt"))
                                    {
                                        multipleImageStr = GetPictureSoure(fileArr, folder);
                                    }
                                }
                            }
                            if (multipleImageStr.Contains(".txt"))
                            {
                                multipleImageStr = "";
                            }
                        //} while (!string.IsNullOrEmpty(multipleImageStr) && counter-- > 0);
                    }
                    else
                    {
                        for (int i = 0; i < fileArr.Length; i++)
                        {
                            if (maxPhoto <= 0)
                            {
                                break;
                            }
                            string file = fileArr[i].Trim();
                            if (file.ToLower().Trim().EndsWith(".txt"))
                            {
                                continue;
                            }
                            if (!string.IsNullOrEmpty(multipleImageStr))
                            {
                                multipleImageStr += " \n ";
                            }
                            multipleImageStr+=file;
                            maxPhoto--;
                        }
                    }
                }
            }

            caption = GetFinalCaption(caption);

            string[] urlArr = link.Split('\n');
            //string[] captionArr = caption.Split('\n');

            if (processActionData.Share.Website.Group)
            {
                try
                {
                    if (processActionData.Share.Website.GroupWithoutJoin)
                    {
                        string gIds = "";
                        for(int i = 0; i < gNumber; i++)
                        {
                            string gId = this.form.GetGroupWithoutJoin();
                            if(gIds.Contains(gId)) { continue; }

                            if(!string.IsNullOrEmpty(gIds))
                            {
                                gIds += ",";
                            }
                            gIds += gId;
                        }
                        if (!string.IsNullOrEmpty(gIds))
                        {
                            data.GroupIDs = gIds;
                        }
                    }
                }
                catch (Exception) { }
                var groupArr = data.GroupIDs.Split(',');
                string des = data.Description;
                int shareSuccess= 0, shareFail= 0;

                for(int i = 0; i < groupArr.Length; i ++)
                {
                    if(gNumber < 0)
                    {
                        break;
                    }
                    if (this.form.IsStop())
                    {
                        break;
                    }
                    if(delayEachOfShare > 0 && i > 0)
                    {
                        Thread.Sleep(delayEachOfShare);
                    }
                    //if(i > 0 && !string.IsNullOrEmpty(folder))
                    //{
                    //    multipleImageStr = fileArr[random.Next(0, fileArr.Length - 1)];
                    //}
                    var groupId = groupArr[i].Trim();
                    string cap = caption;
                    if (cap.Contains("[t]"))
                    {
                        string t = DateTime.Now.ToString("HH:mm:ss");

                        cap = cap.Replace("[t]", " " + t + " ");
                    }
                    string link1 = "";
                    if (cap.StartsWith("[c]"))
                    {
                        cap = cap.Replace("[c]", "");
                        link1 = GetShareWebsiteLink(urlArr, folder);
                    }
                    else
                    {
                        if (urlArr != null && urlArr.Length > 0)
                        {
                            link1 = urlArr[random.Next(0, urlArr.Length)];
                        }
                    }
                    cap = cap.TrimStart();

                    string pathDebuglink = LocalData.GetPath() + "\\debug_link.txt";
                    if (File.Exists(pathDebuglink))
                    {
                        try
                        {
                            driver.Navigate().GoToUrl("https://developers.facebook.com/tools/debug/");
                        } catch(Exception) { }
                        FBTool.WaitingPageLoading(driver);
                        Thread.Sleep(1000);
                        try
                        {
                            driver.FindElement(By.ClassName("inputtext")).SendKeys(link1 + OpenQA.Selenium.Keys.Enter);
                        }
                        catch (Exception) { }
                        int counter = 6;
                        bool isWorking = false;
                        do
                        {
                            Thread.Sleep(1000);
                            var source = driver.PageSource.Trim().ToLower();
                            if(source.Contains("this url hasn"))
                            {
                                try
                                {
                                    driver.FindElement(By.XPath("/html/body/div[1]/div[5]/div[3]/div/div[2]/div/div/div/div/span/form/button")).Click();
                                    Thread.Sleep(2000);
                                } catch(Exception) { }
                            } else if(source.Contains("time scraped"))
                            {
                                isWorking = true;
                            }
                        } while (!isWorking && counter-- > 0);
                        Thread.Sleep(1000);
                    }

                    try
                    {
                        string safeUrl = FBTool.GetSafeGroupUrl(Constant.FB_WEB_URL, groupId);
                        if (!safeUrl.EndsWith("/")) safeUrl += "/";
                        driver.Navigate().GoToUrl(safeUrl + "buy_sell_discussion");
                    }
                    catch (Exception) { }
                    FBTool.WaitingPageLoading(driver);
                    Thread.Sleep(1000);
                    int isSuccess = WebFBTool.ShareWebsiteToGroup(driver,groupId,pastLink,link1, cap, multipleImageStr);
                    if(isSuccess == 1 && !pastLink)
                    {
                        string source = "";
                        try
                        {
                            source = driver.FindElement(By.TagName("body")).Text.Trim().ToLower();
                        }
                        catch (Exception) { }
                        if(source.Contains("use this feature right now"))
                        {
                            data.Description = "FB block comment";
                            data.Status = "Die";
                            return;
                        }

                        if (processActionData.Share.Website.GroupWithoutJoin)
                        {
                            // for page profile
                            LocalData.CatchGroupNoPending(groupId);
                        } else
                        {
                            LocalData.CatchGroupNoPending(groupId,"catch_group_no_pending.txt");
                        }
                        shareSuccess++;


                        
                    } else if(isSuccess == 0)
                    {
                        shareFail++;
                    } else
                    {
                        // group invite

                    }
                    if (isSuccess != -1)
                    {
                        gNumber--;
                        data.Description = des + " Share (success: " + shareSuccess + ", Fail: " + shareFail + ") ";
                    }
                }
            } else
            {
                try
                {
                    driver.Navigate().GoToUrl(Constant.FB_WEB_URL);
                }
                catch (Exception) { }
                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(1000);

                string link1 = "";
                if (urlArr != null && urlArr.Length > 0)
                {
                    link1 = urlArr[random.Next(0, urlArr.Length)];
                }

                WebFBTool.ShareWebsiteToPage(driver, pastLink, link1, caption, multipleImageStr);
            }

            //accountDao.updateDescription(data.UID, data.Description);
        }
        public string GetFinalCaption(string caption) 
        { 
            if(!string.IsNullOrEmpty(processActionData.Share.Hashtag) && caption.Contains("#") )
            {
                string[] hashtagArr = processActionData.Share.Hashtag.Split(',');
                string[] captionArr = caption.Split('#');
                caption = "";
                for (int i = 0; i < captionArr.Count(); i++)
                {
                    if (i == 0)
                    {
                        caption += captionArr[i] + " " + hashtagArr[random.Next(0, hashtagArr.Count() - 1)];
                    }
                    else if ((i + 1) < captionArr.Count())
                    {
                        caption += " " + hashtagArr[random.Next(0, hashtagArr.Count() - 1)] + "" + captionArr[i];
                    } else
                    {
                        caption += " " + captionArr[i];
                    }
                }
            }

            return caption; 
        }
        public bool ClickButtonShare(IWebDriver driver)
        {
            int counter = 20;
            bool isWorking = false;
            do
            {
                FBTool.Close(driver);
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Send this to friends or post it on your timeline.']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Send this to friends or post it on your Timeline.']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='Share']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='share']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div/div[1]/div/div[2]/div/div/div/div[1]/div/div/div[2]/div[2]/div/div/div/div[4]/div/div/div/div[1]/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[text() = 'Share']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//span[text() = 'share']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div[2]/div/div/div/div[1]/div[2]/div/div[2]/div/div[3]/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        // Targeting the SVG icon for sharing which is common on Reels/Mobile layouts
                        driver.FindElement(By.XPath("//div[i[contains(@style, 'background-image')] or @role='button']//i[contains(@class, 'x1b0p6m6')]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        // Another common selector for the share icon in newer layouts
                        driver.FindElement(By.XPath("//div[@aria-label='Share' or @aria-label='share']//i")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    FBTool.Scroll(driver, 500, false);
                }
            } while (!isWorking && counter-- > 0);

            return isWorking;
        }
        public bool GotoShare(IWebDriver driver)
        {
            bool isWorking;
            int counter = 2;
            do
            {
                Thread.Sleep(500);
                isWorking = FBTool.Close(driver);
            } while (!isWorking && counter-- > 0);

            isWorking = ClickButtonShare(driver);
            if (!isWorking)
            {
                return false;
            }
            counter = 30;
            isWorking = false;
            bool isShareToGroup = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//*[text() = 'More Options']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//*[text() = 'More options']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//*[text() = 'Share to a group']")).Click();
                        isWorking = true;
                        isShareToGroup = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//*[text() = 'Group']")).Click();
                        isWorking = true;
                        isShareToGroup = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return false;
            }
            if(isShareToGroup)
            {
                return true;
            }
            if (!IsGoToShare(driver))
            {
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Back']")).Click();
                    Thread.Sleep(2000);
                }
                catch (Exception) { }
                try
                {
                    driver.FindElement(By.XPath("//*[text() = 'More Options']")).Click();
                }
                catch (Exception) { }
                try
                {
                    // check again
                    driver.FindElement(By.XPath("//*[text() = 'More options']")).Click();
                }
                catch (Exception) { }
            }
            counter = 10;
            isWorking = false;
            do
            {
                Thread.Sleep(500);
                IReadOnlyCollection<IWebElement> elements = null;
                try
                {
                    elements = driver.FindElements(By.XPath("//*[text() = 'Share to a group']"));
                }
                catch (Exception) { }
                if (elements != null)
                {
                    string url = "";
                    try
                    {
                        url = driver.Url;
                    }
                    catch (Exception) { }
                    if (url.Contains("posts"))
                    {
                        try
                        {
                            elements.ElementAt(1).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                        if (!isWorking)
                        {
                            try
                            {
                                elements.ElementAt(0).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                    }
                    else
                    {
                        try
                        {
                            elements.ElementAt(0).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                }
            } while (!isWorking && counter-- > 0);

            return isWorking;
        }
        public bool IsGoToShare(IWebDriver driver)
        {
            bool isWorking = false;
            int counter = 6;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//*[text() = 'Share to a group']"));
                    isWorking = true;
                }
                catch (Exception) { }
            } while (!isWorking && counter-- > 0);

            return isWorking;
        }
    }
}
