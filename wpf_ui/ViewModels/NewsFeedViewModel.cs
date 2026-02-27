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
using ToolKHBrowser.ToolLib.Data;
using ToolKHBrowser.ViewModels;
using ToolKHBrowser.Views;

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
        void CommentPostByUrl();
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

            // ? SAFE: cache dao might be null
            var cacheDao = this.form.cacheViewModel?.GetCacheDao();

            // ? SAFE: Get(...) might return null, Value might be null
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

            // ? SAFE: nested objects might be null
            try
            {
                var captions = this.processActionData?.NewsFeed?.Timeline?.Captions;
                if (!string.IsNullOrWhiteSpace(captions))
                {
                    // NewsFeed -> Post Timeline should post exactly what user typed in Caption(s).
                    // Keep all lines and blank lines.
                    captionArr = new[] { captions };
                }
            }
            catch { }
        }

        public void ReadNotification()
        {
            bool opened = false;
            try
            {
                // Make sure top nav is available before trying to click the notification icon.
                FBTool.GoToFacebook(driver, Constant.FB_WEB_URL);
                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(1000);
            }
            catch (Exception) { }

            try
            {
                string[] selectors =
                {
                    "//a[contains(@href,'/notifications')]",
                    "//*[@role='link' and contains(@href,'/notifications')]",
                    "//*[@aria-label='Notifications']",
                    "//*[@aria-label='Your notifications']",
                    "//*[@role='button' and @aria-label='Notifications']",
                    "//*[@role='button' and @aria-label='Your notifications']",
                    "//*[@id='notifications_jewel']"
                };

                foreach (string selector in selectors)
                {
                    IWebElement element = null;
                    try
                    {
                        element = driver.FindElements(By.XPath(selector))
                            .FirstOrDefault(e => e.Displayed && e.Enabled);
                    }
                    catch (Exception) { }

                    if (element == null)
                    {
                        continue;
                    }

                    if (TryClickElement(element))
                    {
                        opened = true;
                        break;
                    }
                }
            }
            catch (Exception) { }

            try
            {
                string currentUrl = string.Empty;
                try { currentUrl = driver.Url ?? string.Empty; } catch (Exception) { }

                // Always normalize to the notifications page (icon click can open only a popup).
                if (currentUrl.IndexOf("/notifications", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    FBTool.GoToFacebook(driver, Constant.FB_WEB_URL + "notifications/");
                    FBTool.WaitingPageLoading(driver);
                    Thread.Sleep(1000);
                    opened = true;
                }
            }
            catch (Exception) { }

            if (opened)
            {
                try { FBTool.Scroll(driver, 3000, false); } catch (Exception) { }
            }
        }

        private bool TryClickElement(IWebElement element)
        {
            if (element == null)
            {
                return false;
            }

            try
            {
                Actions actions = new Actions(driver);
                actions.MoveToElement(element).Click().Build().Perform();
                Thread.Sleep(1000);
                return true;
            }
            catch (Exception) { }

            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
                Thread.Sleep(1000);
                return true;
            }
            catch (Exception) { }

            return false;
        }
        public void Play()
        {
            if (processActionData == null || processActionData.NewsFeed == null || processActionData.NewsFeed.NewsFeed == null)
                return;

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
            var reactCfg = processActionData.NewsFeed.NewsFeed.React;

            bool doLike = reactCfg != null && reactCfg.Like;
            bool doComment = reactCfg != null && reactCfg.Comment;
            if (reactCfg != null && reactCfg.Random)
            {
                doLike = random.Next(0, 2) == 1;
                doComment = random.Next(0, 2) == 1;
            }

            string[] commentLines = Array.Empty<string>();
            try
            {
                commentLines = (processActionData.NewsFeed.NewsFeed.Comments ?? "")
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();
            }
            catch (Exception) { }

            if (time > 0)
            {
                bool isLike = !doLike;
                bool isComment = !doComment;
                do
                {
                    if (!isLike || !isComment)
                    {
                        if (!isLike && doLike)
                        {
                            isLike = true;
                            try { WebFBTool.LikePost(driver); } catch (Exception) { }
                        }

                        if (!isComment && doComment)
                        {
                            isComment = true;
                            if (commentLines.Length > 0)
                            {
                                string comment = commentLines[random.Next(commentLines.Length)];
                                if (!string.IsNullOrWhiteSpace(comment))
                                {
                                    Thread.Sleep(500);
                                    try { WebFBTool.PostComment(driver, comment); } catch (Exception) { }
                                }
                            }
                        }
                    }
                    time--;
                    FBTool.Scroll(driver, 1000, false);
                } while (!IsStop() && time > 0);
            }
        }
        public void CommentPostByUrl()
        {
            string[] commentLines = Array.Empty<string>();
            try
            {
                var raw = processActionData?.NewsFeed?.NewsFeed?.CommentPost?.Comments;
                if (string.IsNullOrWhiteSpace(raw))
                {
                    raw = processActionData?.NewsFeed?.NewsFeed?.Comments ?? "";
                }

                commentLines = raw
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();
            }
            catch (Exception) { }

            RunCommentPostByUrl(commentLines);
        }
        private void RunCommentPostByUrl(string[] commentLines)
        {
            try
            {
                var cfg = processActionData?.NewsFeed?.NewsFeed?.CommentPost;
                if (cfg == null) return;

                var react = cfg.React;
                bool doLike = react?.Like == true;
                bool doComment = react?.Comment == true;
                if (!doLike && !doComment) return;

                var urls = (cfg.VideoUrls ?? "")
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToArray();

                if (urls.Length == 0) return;

                int minComments = Math.Max(1, processActionData.NewsFeed.NewsFeed.MinComments);
                int maxComments = Math.Max(minComments, processActionData.NewsFeed.NewsFeed.MaxComments);

                foreach (var url in urls)
                {
                    if (IsStop()) break;

                    try
                    {
                        FBTool.GoToFacebook(driver, url);
                    }
                    catch (Exception)
                    {
                        try { driver.Navigate().GoToUrl(url); } catch (Exception) { }
                    }

                    FBTool.WaitingPageLoading(driver);
                    Thread.Sleep(1500);

                    if (doLike)
                    {
                        try
                        {
                            TryLikeCommentPostWithoutToggle();
                        }
                        catch (Exception) { }
                        Thread.Sleep(700);
                    }

                    if (doComment && commentLines != null && commentLines.Length > 0)
                    {
                        int totalComments = GetRankNumber(minComments, maxComments);
                        if (totalComments <= 0) totalComments = 1;

                        for (int i = 0; i < totalComments; i++)
                        {
                            if (IsStop()) break;
                            try
                            {
                                string comment = GetNextCommentPostComment(commentLines);
                                if (!string.IsNullOrWhiteSpace(comment))
                                {
                                    bool commented = false;
                                    try
                                    {
                                        commented = WebFBTool.PostComment(driver, comment);
                                    }
                                    catch (Exception) { }

                                    if (!commented)
                                    {
                                        Thread.Sleep(1200);
                                        try
                                        {
                                            // Retry comment on the same page after like UI settles (no refresh).
                                            commented = WebFBTool.PostComment(driver, comment);
                                        }
                                        catch (Exception) { }
                                    }

                                    Thread.Sleep(commented ? 900 : 600);
                                }
                            }
                            catch (Exception) { }
                        }
                    }

                    Thread.Sleep(800);
                }

                try
                {
                    driver.Navigate().GoToUrl(Constant.FB_WEB_URL);
                    FBTool.WaitingPageLoading(driver);
                    Thread.Sleep(1000);
                }
                catch (Exception) { }
            }
            catch (Exception) { }
        }
        private bool TryLikeCommentPostWithoutToggle()
        {
            try
            {
                // If the visible Like control is already pressed, skip clicking so we can still comment.
                var likeButtons = driver.FindElements(By.XPath(
                    "//div[(@aria-label='Like' or @aria-label='Thích' or @aria-label='ចូលចិត្ត') and (@role='button' or @tabindex)]"));

                foreach (var el in likeButtons)
                {
                    try
                    {
                        if (!el.Displayed) continue;

                        var pressed = (el.GetAttribute("aria-pressed") ?? "").Trim().ToLowerInvariant();
                        if (pressed == "true")
                        {
                            return false; // already liked -> skip click, continue comment
                        }

                        WebFBTool.ClickElement(driver, el);
                        return true;
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception) { }

            try
            {
                // Fallback to existing logic (may no-op if no Like button found).
                WebFBTool.LikePost(driver);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private string GetNextCommentPostComment(string[] commentLines)
        {
            try
            {
                if (commentLines == null || commentLines.Length == 0)
                {
                    return "";
                }
                if (commentLines.Length == 1)
                {
                    return commentLines[0];
                }

                const string orderKey = "newsfeed:commentpost:comment_order";
                const string indexKey = "newsfeed:commentpost:comment_index";

                int[] order = null;
                int index = 0;

                try
                {
                    var rawOrder = cacheDao?.Get(orderKey)?.Value?.ToString();
                    if (!string.IsNullOrWhiteSpace(rawOrder))
                    {
                        var parsed = rawOrder
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x =>
                            {
                                int.TryParse(x, out var n);
                                return n;
                            })
                            .ToArray();

                        if (parsed.Length == commentLines.Length
                            && parsed.Distinct().Count() == commentLines.Length
                            && parsed.All(x => x >= 0 && x < commentLines.Length))
                        {
                            order = parsed;
                        }
                    }
                }
                catch (Exception) { }

                try
                {
                    int.TryParse(cacheDao?.Get(indexKey)?.Value?.ToString(), out index);
                }
                catch (Exception) { index = 0; }

                int[] Shuffle()
                {
                    return Enumerable.Range(0, commentLines.Length)
                        .OrderBy(_ => random.Next())
                        .ToArray();
                }

                if (order == null || order.Length != commentLines.Length)
                {
                    order = Shuffle();
                    index = 0;
                }

                if (index < 0 || index >= order.Length)
                {
                    order = Shuffle();
                    index = 0;
                }

                string result = commentLines[order[index]];

                index++;
                if (index >= order.Length)
                {
                    order = Shuffle();
                    index = 0;
                }

                try
                {
                    cacheDao?.Set(orderKey, string.Join(",", order));
                    cacheDao?.Set(indexKey, index.ToString());
                }
                catch (Exception) { }

                return result;
            }
            catch (Exception)
            {
                try
                {
                    return commentLines[random.Next(commentLines.Length)];
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }
        public void Messenger()
        {
            if (processActionData == null || processActionData.NewsFeed == null || processActionData.NewsFeed.Messenger == null)
                return;

            bool isWorking = ReadMessenger();

            if (isWorking)
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
                if (!processActionData.NewsFeed.Messenger.ActiveStatus.None)
                {
                    MessengerOption();
                    isWorking = false;

                    if (processActionData.NewsFeed.Messenger.ActiveStatus.On)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//span[text() = 'Active Status: OFF']")).Click();
                            isWorking = true;
                            Thread.Sleep(1500);
                        }
                        catch (Exception) { }
                    }
                    else if (processActionData.NewsFeed.Messenger.ActiveStatus.Off)
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
                        if (isWorking)
                        {
                            isWorking = false;
                            IWebElement element = null;
                            try
                            {
                                element = driver.FindElement(By.XPath("/html/body/div[7]/div[1]/div/div[2]/div/div/div/div/div/div[1]/div[3]/div[4]/div[2]/div[2]/div[1]/div/div[1]"));
                                isWorking = true;
                            }
                            catch (Exception) { }
                            if (!isWorking)
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
            if (processActionData == null || processActionData.NewsFeed == null || processActionData.NewsFeed.Timeline == null)
                return;

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
                if (IsStop())
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
                var source = this.form.GetSourceTimeline(data.TimelineSource, processActionData.NewsFeed.Timeline.DeleteAfterPost);
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
                    if (!string.IsNullOrEmpty(source) && File.Exists(source) && processActionData.NewsFeed.Timeline.DeleteAfterPost)
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
                if (captionArr == null || captionArr.Length == 0)
                {
                    return "";
                }
                if (captionIndex >= captionArr.Length)
                {
                    captionIndex = 0;
                }
                str = captionArr[captionIndex];
                captionIndex++;

                this.form.cacheViewModel.GetCacheDao().Set("newsfeed:caption_index", captionIndex + "");
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
            min = Math.Max(0, min);
            max = Math.Max(min, max);
            if (max == min)
            {
                return min;
            }
            return new Random().Next(min, max + 1);
        }
        public bool IsStop()
        {
            return this.form.IsStop();
        }
    }
}
