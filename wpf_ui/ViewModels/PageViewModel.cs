using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using ToolKHBrowser.ToolLib.Data;
using ToolKHBrowser.ToolLib.Tool;
using ToolLib.Data;
using ToolLib.Tool;
using ToolKHBrowser.ViewModels;
using ToolKHBrowser.Views;

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
        void Post();

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        void AutoScroll();

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        IPagesDao GetPagesDao();
    }

    public class PageViewModel : IPageViewModel
    {
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        private IAccountDao accountDao;
        private ICacheDao cacheDao;
        private IPagesDao pageDao;
        private frmMain form;
        private FbAccount data;
        private IWebDriver driver;
        private ProcessActions processActionData;
        private string mainProfileUrl;
        private string mainProfileName;

        private string[] pageUrlArr;
        private string[] pageNameArr;
        private string[] pageCategoriesArr;
        private string[] pageBioArr;

        private int pageUrlIndex = 0;
        private int createNameIndex = 0;
        private int createCategoryIndex = 0;
        private int createBioIndex = 0;

        public Random random = new Random();

        public PageViewModel(IAccountDao accountDao, ICacheDao cacheDao, IPagesDao pageDao)
        {
            this.accountDao = accountDao;
            this.cacheDao = cacheDao;
            this.pageDao = pageDao;
        }

        public IPagesDao GetPagesDao() => this.pageDao;

        public void Start(frmMain form, IWebDriver driver, FbAccount data)
        {
            this.form = form;
            this.data = data;
            this.driver = driver;
            this.processActionData = this.form.processActionsData;
            this.mainProfileName = (data?.Name ?? "").Trim();
            this.mainProfileUrl = "";

            // Seed the personal profile URL early so later page actions don't "remember"
            // a page identity URL and then switch back to the wrong target.
            try
            {
                var uid = (data?.UID ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(uid))
                {
                    this.mainProfileUrl = "https://www.facebook.com/profile.php?id=" + uid;
                }
            }
            catch { }

            try
            {
                var c = this.form.cacheViewModel.GetCacheDao().Get("page:config:name_index");
                if (c?.Value != null) int.TryParse(c.Value.ToString(), out createNameIndex);
            }
            catch { }

            try
            {
                var c = this.form.cacheViewModel.GetCacheDao().Get("page:config:category_index");
                if (c?.Value != null) int.TryParse(c.Value.ToString(), out createCategoryIndex);
            }
            catch { }

            try
            {
                var c = this.form.cacheViewModel.GetCacheDao().Get("page:config:bio_index");
                if (c?.Value != null) int.TryParse(c.Value.ToString(), out createBioIndex);
            }
            catch { }

            if (this.processActionData?.PageConfig == null) return;

            try
            {
                if (!string.IsNullOrEmpty(this.processActionData.PageConfig.PageUrls))
                    pageUrlArr = this.processActionData.PageConfig.PageUrls.Split('\n');
            }
            catch { }

            if (this.processActionData.PageConfig.CreatePage != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(this.processActionData.PageConfig.CreatePage.Names))
                        pageNameArr = this.processActionData.PageConfig.CreatePage.Names.Split('\n');
                }
                catch { }

                try
                {
                    if (!string.IsNullOrEmpty(this.processActionData.PageConfig.CreatePage.Categies))
                        pageCategoriesArr = this.processActionData.PageConfig.CreatePage.Categies.Split('\n');
                }
                catch { }

                try
                {
                    if (!string.IsNullOrEmpty(this.processActionData.PageConfig.CreatePage.Bio))
                        pageBioArr = this.processActionData.PageConfig.CreatePage.Bio.Split('\n');
                }
                catch { }
            }
        }

        // =========================================================
        // - Open composer
        // - Click Photo/video
        // - SendKeys file into the *dialog* file input
        // - If FB shows "Edit reel" screen -> Click Next -> Caption -> Post
        // - Else normal post -> Next (if any) -> Post/Publish/Share now
        // =========================================================
        public void Post()
        {
            try
            {
                try { File.WriteAllText(@"C:\debug.txt", "=== POST START ===\r\n"); } catch { }

                if (this.processActionData == null) { Log("STOP: processActionData null"); return; }

                var dao = this.form?.cacheViewModel?.GetCacheDao();
                if (dao == null) { Log("STOP: dao null"); return; }

                var cache = dao.Get("newsfeed:config");
                var str = cache?.Value?.ToString();
                if (string.IsNullOrWhiteSpace(str)) { Log("STOP: newsfeed:config empty"); return; }

                NewsFeedConfig newsfeedObj;
                try { newsfeedObj = JsonConvert.DeserializeObject<NewsFeedConfig>(str); }
                catch (Exception ex) { Log($"STOP: JSON error {ex.Message}"); return; }

                var config = newsfeedObj?.PagePost;
                if (config == null) { Log("STOP: PagePost config null"); return; }

                // Prefer Backup(PageIds saved into data.PageIds) else config.PageIds
                string pageIdsSource = !string.IsNullOrWhiteSpace(data.PageIds) ? data.PageIds : config.PageIds;
                if (string.IsNullOrWhiteSpace(pageIdsSource))
                {
                    Log("STOP: BOTH PageIds empty - run Backup first!");
                    return;
                }

                // Captions
                string[] captionArr = new[] { "" };
                if (!string.IsNullOrWhiteSpace(config.Captions))
                {
                    captionArr = config.Captions
                        .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Where(x => x.Length > 0)
                        .ToArray();
                }
                if (captionArr.Length == 0) captionArr = new[] { "" };

                // Media (supports file OR folder)
                var mediaFiles = LoadMediaFiles_FileOrFolder(config.SourceFolder);
                if (mediaFiles.Count == 0)
                {
                    Log("STOP: No media found (SourceFolder is empty / invalid).");
                    return;
                }

                // Parse Page IDs (allow full URL)
                var pageArr = pageIdsSource
                    .Split(new[] { "\r\n", "\n", ",", ";" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(NormalizePageId)
                    .Where(x => x.Length > 0)
                    .ToArray();

                if (pageArr.Length == 0) { Log("STOP: no pages after parse"); return; }

                int postPerPage = (config.MaxPosts > 0) ? config.MaxPosts : 1;

                // Remember the personal profile URL before switching to any Page identity.
                RememberMainProfileUrl();

                foreach (var pageId in pageArr)
                {
                    if (IsStop()) break;

                    Log($"--- PAGE [{pageId}] ---");

                    if (!GoToPage(pageId))
                    {
                        Log($"SKIP: Cannot open page [{pageId}] (redirect/no access).");
                        continue;
                    }

                    if (!CheckSessionAndHandleSecurity())
                    {
                        Log("SKIP: Security screen (2FA/Login) detected and could not be bypassed.");
                        continue;
                    }

                    TryClickSwitchNowIfPresent();

                    for (int i = 0; i < postPerPage; i++)
                    {
                        if (IsStop()) break;

                        string caption = captionArr[random.Next(0, captionArr.Length)];
                        string media = mediaFiles[random.Next(0, mediaFiles.Count)];

                        Log($"POST {i + 1}/{postPerPage} | media={Path.GetFileName(media)}");

                        if (!CheckSessionAndHandleSecurity())
                        {
                            Log("FAIL: Security screen detected inside post loop. Breaking page flow.");
                            break;
                        }

                        // 1) Open Create post dialog
                        if (!OpenCreatePostDialog())
                        {
                            Log("FAIL: Cannot open Create post dialog.");
                            break;
                        }

                        // 2) Type caption
                        TryTypeInDialogCaption(caption);

                        // 3) Prefer direct hidden file-input upload to avoid native OS file dialog.
                        bool attachedDirectly = AttachMediaInDialog(media);
                        if (attachedDirectly)
                        {
                            Log("Media attached via file input (no picker dialog).");
                        }

                        // 4) Fallback path: open picker UI only if direct file-input attach failed.
                        if (!attachedDirectly)
                        {
                            // Option A: click Photo/video button
                            bool clickedPicker = ClickPhotoVideoInDialog();

                            // Option B: if Photo/video not found -> focus textbox + click photo icon
                            if (!clickedPicker)
                            {
                                Log("Option B: Photo/video not found -> focus textbox + click photo icon");

                                if (!ClickWhatsOnYourMindInDialog())
                                {
                                    Log("FAIL: Can't click What's on your mind textbox.");
                                    TryCloseAnyDialog();
                                    break;
                                }

                                if (!ClickPhotoIconInAddToYourPost())
                                {
                                    Log("FAIL: Can't click photo icon in Add to your post.");
                                    TryCloseAnyDialog();
                                    break;
                                }
                            }

                            // Attach file after opening upload UI
                            if (!AttachMediaInDialog(media))
                            {
                                Log("FAIL: Could not attach media.");
                                TryCloseAnyDialog();
                                break;
                            }
                        }

                        // 5) VIDEO/REEL FLOW (Next twice)
                        if (IsCreatePostDialogWithNext() || IsEditReelScreen())
                        {
                            Log("Detected: VIDEO FLOW (Next twice)");

                            // Next #1 (dialog)
                            if (IsCreatePostDialogWithNext())
                            {
                                if (!ClickDialogNext_AndWaitEditReel())
                                {
                                    Log("FAIL: Dialog Next -> Edit reel");
                                    TryShot(@"C:\debug_next1_fail.png");
                                    TryCloseAnyDialog();
                                    break;
                                }
                            }

                            // Next #2 (Edit reel)
                            if (!ClickReelNext_AndWaitSettings())
                            {
                                Log("FAIL: Edit reel Next -> Reel settings");
                                TryShot(@"C:\debug_next2_fail.png");
                                TryCloseAnyDialog();
                                break;
                            }

                            // Reel settings -> caption -> Post
                            TryTypeReelCaption(caption);
                            ClickNotNowIfPresent();

                            if (!ClickReelPost_AndWaitFinish())
                            {
                                Log("FAIL: Reel Post");
                                TryShot(@"C:\debug_reel_post_fail.png");
                                TryCloseAnyDialog();
                                break;
                            }

                            Log("✅ Reel posted.");
                            Thread.Sleep(5000);
                            continue;
                        }

                        // 6) NORMAL PHOTO FLOW (your screenshot: Next -> Post settings -> Post)
                        if (!ClickDialogNext_AndWaitPostSettings())
                        {
                            Log("FAIL: Next -> Post settings not reached");
                            TryShot(@"C:\debug_next_postsettings_fail.png");
                            TryCloseAnyDialog();
                            break;
                        }

                        if (!ClickPostInPostSettings())
                        {
                            Log("FAIL: Post button not clicked in Post settings");
                            TryShot(@"C:\debug_postsettings_post_fail.png");
                            TryCloseAnyDialog();
                            break;
                        }

                        Log("✅ Posted (photo).");
                        Thread.Sleep(6000);
                    }
                }

                Log("=== POST DONE ===");
                this.data.Description = "Post to Page Done";
                this.form.SetGridDataRowStatus(this.data);
            }
            finally
            {
                Log("Switching back to personal profile before finish...");
                SwitchToProfileIdentity();
            }
        }
        private bool ClickWhatsOnYourMindInDialog()
        {
            try
            {
                var box = FindFirstDisplayed(new[]
                {
            By.XPath("//div[@role='dialog']//div[@role='textbox']"),
            By.XPath("//div[@role='dialog']//div[@contenteditable='true']")
        }, 3);

                if (box == null) return false;
                SafeClick(box);
                Thread.Sleep(300);
                return true;
            }
            catch { return false; }
        }

        private bool ClickPhotoIconInAddToYourPost()
        {
            for (int i = 0; i < 10 && !IsStop(); i++)
            {
                try
                {
                    var btn = FindFirstDisplayed(new[]
                    {
                By.XPath("//div[@role='dialog']//*[contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'add to your post')]//div[@role='button' and .//*[name()='svg']]"),
                By.XPath("//div[@role='dialog']//div[@aria-label='Photo/video' or @aria-label='Photo/Video']"),
                By.XPath("//div[@role='dialog']//div[@role='button' and contains(translate(@aria-label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'photo')]"),
            }, 2);

                    if (btn != null)
                    {
                        SafeClick(btn);
                        Thread.Sleep(700);
                        return true;
                    }
                }
                catch { }

                Thread.Sleep(500);
            }
            return false;
        }

        private bool ClickDialogNext_AndWaitPostSettings()
        {
            for (int i = 0; i < 30 && !IsStop(); i++)
            {
                try
                {
                    var nextBtn = FindFirstDisplayed(new[]
                    {
                By.XPath("//div[@role='dialog']//span[normalize-space(.)='Next']/ancestor::div[@role='button'][1]"),
                By.XPath("//div[@role='dialog']//div[@role='button' and .//span[normalize-space(.)='Next']]")
            }, 1);

                    if (nextBtn != null)
                    {
                        var ariaDisabled = (nextBtn.GetAttribute("aria-disabled") ?? "").ToLower();
                        if (ariaDisabled == "true")
                        {
                            Log("Waiting: Next disabled (processing/copyright)...");
                            Thread.Sleep(800);
                            continue;
                        }

                        SafeClick(nextBtn);
                        Thread.Sleep(1200);

                        if (WaitExists(By.XPath("//*[normalize-space(.)='Post settings']"), 10))
                            return true;
                    }
                }
                catch { }

                Thread.Sleep(600);
            }
            return false;
        }

        private bool ClickPostInPostSettings()
        {
            for (int i = 0; i < 25 && !IsStop(); i++)
            {
                try
                {
                    var postBtn = FindFirstDisplayed(new[]
                    {
                By.XPath("//*[normalize-space(.)='Post settings']//span[normalize-space(.)='Post']/ancestor::div[@role='button'][1]"),
                By.XPath("//span[normalize-space(.)='Post']/ancestor::div[@role='button'][1]")
            }, 2);

                    if (postBtn != null)
                    {
                        var ariaDisabled = (postBtn.GetAttribute("aria-disabled") ?? "").ToLower();
                        if (ariaDisabled == "true")
                        {
                            Thread.Sleep(700);
                            continue;
                        }

                        SafeClick(postBtn);
                        Thread.Sleep(1500);
                        ClickPublishOriginalPostIfPresent();
                        return true;
                    }
                }
                catch { }

                Thread.Sleep(700);
            }
            return false;
        }


        private bool ClickDialogNext_AndWaitEditReel()
        {
            for (int i = 0; i < 25 && !IsStop(); i++)
            {
                try
                {
                    var nextBtn = FindFirstDisplayed(new[]
                    {
                By.XPath("//div[@role='dialog']//div[@role='button' and .//span[normalize-space(.)='Next']]"),
                By.XPath("//div[@role='dialog']//span[normalize-space(.)='Next']/ancestor::div[@role='button'][1]")
            }, 1);

                    if (nextBtn != null)
                    {
                        // Wait until enabled
                        var ariaDisabled = (nextBtn.GetAttribute("aria-disabled") ?? "").ToLower();
                        if (ariaDisabled == "true")
                        {
                            Log("Waiting: dialog Next disabled (copyright check/processing)...");
                            Thread.Sleep(800);
                            continue;
                        }

                        SafeClick(nextBtn);
                        Thread.Sleep(1200);

                        if (WaitExists(By.XPath("//*[normalize-space(.)='Edit reel']"), 10))
                            return true;
                    }
                }
                catch { }

                Thread.Sleep(600);
            }
            return false;
        }


        private bool IsCreatePostDialogWithNext()
        {
            try
            {
                return driver.FindElements(By.XPath(
                    "//div[@role='dialog' and .//*[normalize-space(.)='Create post']]"
                )).Any()
                && driver.FindElements(By.XPath(
                    "//div[@role='dialog']//div[@role='button' and .//span[normalize-space(.)='Next']]"
                )).Any();
            }
            catch { return false; }
        }


        private bool ClickReelNext_AndWaitSettings()
        {
            if (!WaitExists(By.XPath("//*[normalize-space(.)='Edit reel']"), 15))
                return false;

            for (int i = 0; i < 35 && !IsStop(); i++)
            {
                try
                {
                    var nextBtn = FindFirstDisplayed(new[]
                    {
                By.XPath("//div[@role='button' and (.//span[normalize-space(.)='Next'] or normalize-space(.)='Next')]")
            }, 1);

                    if (nextBtn != null)
                    {
                        var ariaDisabled = (nextBtn.GetAttribute("aria-disabled") ?? "").ToLower();
                        if (ariaDisabled == "true")
                        {
                            Log("Waiting: edit reel Next disabled (processing)...");
                            Thread.Sleep(800);
                            continue;
                        }

                        SafeClick(nextBtn);
                        Thread.Sleep(1500);

                        // Reel settings indicators
                        if (driver.PageSource.IndexOf("Reel settings", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                        if (driver.PageSource.IndexOf("Describe your reel", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                    }
                }
                catch { }

                Thread.Sleep(700);
            }

            return false;
        }

        private bool ClickReelPost_AndWaitFinish()
        {
            for (int i = 0; i < 30 && !IsStop(); i++)
            {
                // Sometimes "Post", sometimes "Share now"
                var postBtn = FindEnabledButtonByText("Post") ?? FindEnabledButtonByText("Share now") ?? FindEnabledButtonByText("Share Now");

                if (postBtn != null)
                {
                    SafeClickScroll(postBtn);
                    Thread.Sleep(1500);

                    // Popups may appear
                    ClickPublishOriginalPostIfPresent();
                    ClickNotNowIfPresent();

                    // ✅ success conditions:
                    // - editor disappears
                    // - URL changes away from reel editor
                    // - toast/confirmation appears (varies)
                    if (!IsEditReelScreen() && !IsReelSettingsScreen())
                        return true;
                }

                Log("Waiting: Reel Post / Share button...");
                Thread.Sleep(1000);
            }

            return false;
        }
        private void ClickNotNowIfPresent()
        {
            try
            {
                var btn = FindFirstDisplayed(new[]
                {
            By.XPath("//span[normalize-space(.)='Not now']/ancestor::div[@role='button'][1]"),
            By.XPath("//div[@role='button' and .//span[normalize-space(.)='Not now']]"),
        }, 1);

                if (btn != null)
                {
                    SafeClickScroll(btn);
                    Thread.Sleep(800);
                    Log("Clicked: Not now");
                }
            }
            catch { }
        }

        private void ClickPublishOriginalPostIfPresent()
        {
            try
            {
                for (int i = 0; i < 8 && !IsStop(); i++)
                {
                    var btn = FindFirstDisplayed(new[]
                    {
                        By.XPath("//span[normalize-space(.)='Publish Original Post']/ancestor::div[@role='button'][1]"),
                        By.XPath("//div[@role='button' and .//span[normalize-space(.)='Publish Original Post']]"),
                        By.XPath("//span[contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'publish original post')]/ancestor::div[@role='button'][1]")
                    }, 1);

                    if (btn != null)
                    {
                        SafeClickScroll(btn);
                        Thread.Sleep(1200);
                        Log("Clicked: Publish Original Post");
                        return;
                    }

                    // If the upsell is visible but button is not ready yet, wait a little.
                    if (driver.PageSource.IndexOf("Publish Original Post", StringComparison.OrdinalIgnoreCase) >= 0
                        || driver.PageSource.IndexOf("Hosting an event", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Thread.Sleep(600);
                        continue;
                    }

                    break;
                }
            }
            catch { }
        }

        private IWebElement FindEnabledButtonByText(string text)
        {
            try
            {
                var els = driver.FindElements(By.XPath(
                    $"//div[@role='button' and (.//span[normalize-space(.)='{text}'] or normalize-space(.)='{text}')]"
                ));

                foreach (var el in els)
                {
                    try
                    {
                        if (!el.Displayed) continue;

                        var ariaDisabled = (el.GetAttribute("aria-disabled") ?? "").Trim().ToLower();
                        if (ariaDisabled == "true") continue;

                        return el;
                    }
                    catch { }
                }
            }
            catch { }

            return null;
        }

        private void SafeClickScroll(IWebElement el)
        {
            if (el == null) return;

            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", el);
                Thread.Sleep(200);
            }
            catch { }

            try { el.Click(); return; } catch { }

            try { ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", el); } catch { }
        }



        private bool IsReelSettingsScreen()
        {
            try
            {
                // FB variations: "Reel settings" or caption box "Describe your reel..."
                return driver.FindElements(By.XPath("//*[contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'reel settings')]")).Any()
                    || driver.FindElements(By.XPath("//div[contains(translate(@aria-label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'describe your reel')]")).Any()
                    || driver.FindElements(By.XPath("//textarea[contains(translate(@aria-label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'describe')]")).Any();
            }
            catch { return false; }
        }


        private bool CheckSessionAndHandleSecurity()
        {
            try
            {
                string url = driver.Url.ToLower();

                // 1) Handle Login Screen (Initial or session loss)
                bool isLoginScreen = url.Contains("/login/")
                    || (url.Contains("facebook.com") && WaitExists(By.Name("login"), 2))
                    || WaitExists(By.Id("email"), 1)
                    || WaitExists(By.XPath("//input[@name='email']"), 1);

                if (isLoginScreen)
                {
                    Log("Security: Browser at login screen. Attempting robust sign-in...");
                    RobustLogin(driver, data);

                    // Wait for URL to leave login screen (max 10s)
                    for (int i = 0; i < 5; i++)
                    {
                        Thread.Sleep(2000);
                        url = driver.Url.ToLower();
                        // Moved on from login page — but DON'T return yet,
                        // Facebook may now show the push notification screen!
                        if (!url.Contains("/login/") && !WaitExists(By.Name("login"), 1))
                        {
                            Log("Security: Left login screen. Checking for push/2FA...");
                            break; // Fall through to the 2FA check below
                        }
                    }
                }

                // 2) Always check for Push Approval / 2FA screen
                // This runs BOTH after normal navigation AND after bot-verification/login
                if (FBTool.Is2FA(driver))
                {
                    Log("Security: 2FA / Push Approval screen detected. Attempting bypass...");

                    string result = FBTool.AutoFill2FACode(driver, data);
                    if (result == "ok")
                    {
                        Log("Security: 2FA Auto-fill successful.");
                        if (FBTool.WaitForLoginSuccess(driver, 30))
                        {
                            Log("Security: Logged in successfully after 2FA.");
                            return true;
                        }
                    }
                    else
                    {
                        Log("Security: 2FA Auto-fill failed: " + result);
                    }

                    url = driver.Url.ToLower();
                }

                // Final check: are we still at login?
                if (url.Contains("/login/") || (url.Contains("facebook.com") && WaitExists(By.Name("login"), 2)))
                {
                    Log("Security: Still at login screen after recovery attempt.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log("CheckSessionAndHandleSecurity error: " + ex.Message);
                return true;
            }
        }

        private void RobustLogin(IWebDriver driver, FbAccount data)
        {
            try
            {
                Log("Security: Running Robust Login Filler...");
                Thread.Sleep(2000);

                string email = data?.UID?.Trim() ?? "";
                string pass = data?.Password?.Trim() ?? "";

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
                {
                    Log("Security: ABORT: UID or Password in account data is empty.");
                    return;
                }

                // 1) Find Email/UID field
                var inputEmail = FindFirstDisplayed(new[]
                {
                    By.Id("email"),
                    By.Name("email"),
                    By.XPath("//input[@name='email']"),
                    By.XPath("//input[@placeholder='Email or mobile number']"),
                    By.XPath("//input[@placeholder='Email address or phone number']"),
                    By.XPath("//input[contains(@aria-label,'Email')]"),
                    By.XPath("//input[contains(@aria-label,'mobile number')]")
                }, 10);

                if (inputEmail != null)
                {
                    Log("Security: Found Email field. Typing UID...");
                    try
                    {
                        inputEmail.Click();
                        inputEmail.SendKeys(OpenQA.Selenium.Keys.Control + "a");
                        inputEmail.SendKeys(OpenQA.Selenium.Keys.Delete);
                    }
                    catch { }
                    Thread.Sleep(500);
                    inputEmail.SendKeys(email);
                }
                else
                {
                    Log("Security: WARNING: Could not find Email field. (Tried ID, Name, Placeholder, Aria)");
                    TryShot(@"C:\debug_login_email_fail.png");
                }

                // 2) Find Password field
                var inputPass = FindFirstDisplayed(new[]
                {
                    By.Id("pass"),
                    By.Name("pass"),
                    By.XPath("//input[@name='pass']"),
                    By.XPath("//input[@placeholder='Password']"),
                    By.XPath("//input[@type='password']"),
                    By.XPath("//input[contains(@aria-label,'Password')]")
                }, 10);

                if (inputPass != null)
                {
                    Log("Security: Found Password field. Typing Password...");
                    try { inputPass.Click(); inputPass.Clear(); } catch { }
                    Thread.Sleep(500);
                    inputPass.SendKeys(pass);
                }
                else
                {
                    Log("Security: WARNING: Could not find Password field.");
                    TryShot(@"C:\debug_login_pass_fail.png");
                }

                // 3) Find Login Button
                var btnLogin = FindFirstDisplayed(new[]
                {
                    By.Name("login"),
                    By.XPath("//button[@name='login']"),
                    By.XPath("//button[@type='submit' and (contains(.,'Log') or contains(.,'login'))]"),
                    By.XPath("//button[contains(translate(.,'LOGIN','login'),'log in')]"),
                    By.XPath("//div[@role='button' and (contains(translate(.,'LOGIN','login'),'log in'))]"),
                    By.XPath("//button[@type='submit']")
                }, 10);

                if (btnLogin != null)
                {
                    Log("Security: Clicking Login button...");
                    SafeClick(btnLogin);
                    Thread.Sleep(3000);
                }
                else
                {
                    Log("Security: WARNING: Could not find Login button.");
                    TryShot(@"C:\debug_login_button_fail.png");
                }
            }
            catch (Exception ex)
            {
                Log("RobustLogin error: " + ex.Message);
            }
        }

        // =========================
        // Navigation / identity
        // =========================
        private bool GoToPage(string pageId)
        {
            try
            {
                string url1 = "https://www.facebook.com/" + pageId;
                string url2 = "https://www.facebook.com/profile.php?id=" + pageId;

                Log($"Navigating to: {url1}");
                SafeGo(url1, 1500);
                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(2000);

                var cur = SafeGetUrl();
                if (IsBlockedUrl(cur))
                {
                    Log("Navigation blocked. Checking security...");
                    if (CheckSessionAndHandleSecurity())
                    {
                        Log("Security handled. Continuing navigation...");
                        SafeGo(url1, 1500);
                        FBTool.WaitingPageLoading(driver);
                    }
                    else return false;
                }

                if (IsOnTargetPage(cur, pageId)) return true;

                Log($"Fallback navigation to: {url2}");
                SafeGo(url2, 1500);
                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(2000);

                cur = SafeGetUrl();
                if (IsBlockedUrl(cur))
                {
                    if (CheckSessionAndHandleSecurity()) return true; // Let the caller re-verify
                    return false;
                }
                return IsOnTargetPage(cur, pageId);
            }
            catch (Exception ex)
            {
                Log("GoToPage error: " + ex.Message);
                return false;
            }
        }

        private void TryClickSwitchNowIfPresent()
        {
            try
            {
                // “Switch Now” banner appears on managed pages sometimes
                var btn = FindFirstDisplayed(new[]
                {
                    By.XPath("//div[@aria-label='Switch Now' or @aria-label='Switch now']"),
                    By.XPath("//span[normalize-space(text())='Switch Now']/ancestor::div[@role='button'][1]"),
                    By.XPath("//span[normalize-space(text())='Switch now']/ancestor::div[@role='button'][1]"),
                    By.XPath("//div[@role='button' and .//span[contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'switch')]]")
                }, 2);

                if (btn != null)
                {
                    SafeClick(btn);
                    Thread.Sleep(1200);
                    FBTool.WaitingPageLoading(driver);
                    Thread.Sleep(1200);
                    FBTool.Close(driver);
                    Thread.Sleep(800);
                    Log("Clicked Switch Now.");
                }
            }
            catch { }
        }

        // =========================
        // Create Post dialog helpers
        // =========================
        private bool OpenCreatePostDialog()
        {
            // On page timeline (not dialog yet) click “What’s on your mind?” area or composer
            for (int i = 0; i < 6 && !IsStop(); i++)
            {
                try
                {
                    var btn = FindFirstDisplayed(new[]
                    {
                        By.XPath("//div[@role='button' and (@aria-label='Create post' or @aria-label='Create Post')]"),
                        By.XPath("//div[@aria-label='Create post' or @aria-label='Create Post']"),
                        By.XPath("//span[contains(text(),\"What's on your mind\")]/ancestor::div[@role='button'][1]"),
                        By.XPath("//span[contains(text(),'What') and contains(text(),'mind')]/ancestor::div[@role='button'][1]"),
                        By.XPath("//div[@role='textbox']"),
                    }, 2);

                    if (btn != null)
                    {
                        SafeClick(btn);
                        Thread.Sleep(800);

                        // Wait dialog exists
                        if (WaitExists(By.XPath("//div[@role='dialog']"), 6))
                            return true;
                    }
                }
                catch { }

                Thread.Sleep(600);
            }
            return false;
        }

        private void TryTypeInDialogCaption(string caption)
        {
            if (string.IsNullOrWhiteSpace(caption)) return;

            try
            {
                var dialog = FindDialog();
                if (dialog == null) return;

                // Prefer the actual textbox inside dialog
                var box = FindFirstDisplayed(new[]
                {
                    By.XPath("//div[@role='dialog']//div[@role='textbox']"),
                    By.XPath("//div[@role='dialog']//div[@contenteditable='true']")
                }, 2);

                if (box != null)
                {
                    SafeClick(box);
                    Thread.Sleep(200);
                    // Use Actions to reduce issues with contenteditable
                    new Actions(driver).SendKeys(box, caption).Build().Perform();
                    Thread.Sleep(300);
                }
            }
            catch { }
        }

        private bool ClickPhotoVideoInDialog()
        {
            // In the Create post dialog, “Photo/video” is usually a button in “Add to your post”
            for (int i = 0; i < 6 && !IsStop(); i++)
            {
                try
                {
                    var btn = FindFirstDisplayed(new[]
                    {
                        By.XPath("//div[@role='dialog']//*[self::div or self::span][normalize-space(.)='Photo/video']/ancestor::div[@role='button'][1]"),
                        By.XPath("//div[@role='dialog']//div[@aria-label='Photo/video' or @aria-label='Photo/Video']"),
                        By.XPath("//div[@role='dialog']//div[@role='button' and contains(translate(@aria-label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'photo')]"),
                        // fallback: first svg button row under "Add to your post"
                        By.XPath("//div[@role='dialog']//div[contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'add to your post')]//div[@role='button' and .//*[name()='svg']]")
                    }, 2);

                    if (btn != null)
                    {
                        SafeClick(btn);
                        Thread.Sleep(700);
                        return true;
                    }
                }
                catch { }

                Thread.Sleep(500);
            }
            return false;
        }

        private bool AttachMediaInDialog(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return false;

            // Important: choose an input that is inside the dialog (avoid other hidden inputs on the page)
            for (int t = 0; t < 10 && !IsStop(); t++)
            {
                try
                {
                    var inputs = driver.FindElements(By.XPath("//div[@role='dialog']//input[@type='file']")).ToList();

                    // If FB renders input outside dialog but related to it, fallback to any visible-ish input
                    if (inputs.Count == 0)
                        inputs = driver.FindElements(By.XPath("//input[@type='file']")).ToList();

                    foreach (var inp in inputs)
                    {
                        try
                        {
                            // Some inputs are not interactable; try anyway
                            inp.SendKeys(filePath);
                            Thread.Sleep(1200);

                            // Wait until preview/next appears OR reel editor appears
                            if (WaitAny(new[]
                            {
                                By.XPath("//div[@role='dialog']//div[@aria-label='Next' or @aria-label='next']"),
                                By.XPath("//div[@role='dialog']//span[normalize-space(text())='Next']"),
                                By.XPath("//*[contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'edit reel')]"),
                                By.XPath("//*[contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'reel settings')]"),
                                By.XPath("//div[@role='dialog']//div[contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'uploading')]")
                            }, 8))
                            {
                                return true;
                            }

                            // Even if not found, still may have attached; consider success if dialog now shows media thumbnail
                            if (driver.PageSource.IndexOf("Edit reel", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                            if (driver.PageSource.IndexOf("Reel settings", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                            if (driver.PageSource.IndexOf("Next", StringComparison.OrdinalIgnoreCase) >= 0) return true;

                            return true;
                        }
                        catch { }
                    }
                }
                catch { }

                Thread.Sleep(700);
            }
            return false;
        }

        /// <summary>
        /// This button for switch to main account after post successful!
        /// </summary>
        //private void SwitchToProfileIdentity()
        //{
        //    try
        //    {
        //        Log("Switching back to profile identity...");

        //        // First try direct navigation to the remembered personal profile URL.
        //        if (!string.IsNullOrWhiteSpace(mainProfileUrl))
        //        {
        //            Log("Trying remembered main profile URL: " + mainProfileUrl);
        //            try
        //            {
        //                SafeGo(mainProfileUrl, 1000);
        //                FBTool.WaitingPageLoading(driver);
        //                Thread.Sleep(1500);
        //                TryClickSwitchNowIfPresent();
        //                Thread.Sleep(1200);
        //            }
        //            catch { }
        //        }

        //        // Open home so the account/profile menu is in a predictable state.
        //        try
        //        {
        //            SafeGo("https://www.facebook.com/", 1000);
        //            FBTool.WaitingPageLoading(driver);
        //            Thread.Sleep(1200);
        //        }
        //        catch { }

        //        // Click top-right profile picture menu
        //        var profileMenu = FindFirstDisplayed(new[]
        //        {
        //            By.XPath("//div[@aria-label='Account']"),
        //            By.XPath("//div[@role='button' and @aria-label='Your profile']"),
        //            By.XPath("//div[@role='button' and .//img]")
        //        }, 5);

        //        if (profileMenu != null)
        //        {
        //            SafeClick(profileMenu);
        //            Thread.Sleep(1000);
        //        }

        //        // Your popup layout tip: current page is row #1, main profile is row #2.
        //        // Try this first when "Select profile" popup is visible.
        //        if (TryClickSecondProfileRowByIndexPopup())
        //        {
        //            Log("Popup index click: selected 2nd profile row (main profile).");
        //            if (WaitSwitchBackConfirmed(4)) { Log("Switched to profile."); return; }
        //        }

        //        // Primary method for this popup:
        //        // Fixed popup-relative OS click for row 2 (main profile under current page).
        //        if (TryOsClickSecondProfileRowByPopupLayout())
        //        {
        //            Log("OS popup-layout click: selected second profile row.");
        //            if (WaitSwitchBackConfirmed(4)) { Log("Switched to profile."); return; }
        //        }

        //        // Primary method fallback:
        //        // Native Selenium click on the 2nd row (real/trusted browser click).
        //        if (TryNativeClickSecondProfileRowInPopup())
        //        {
        //            Log("Native click: selected second profile row.");
        //            if (WaitSwitchBackConfirmed(4)) { Log("Switched to profile."); return; }
        //        }

        //        // Secondary method:
        //        // Use keyboard navigation instead of DOM click selectors.
        //        // Select profile popup starts on current page row -> Home, ArrowDown, Enter => 2nd row.
        //        if (TrySelectSecondProfileByKeyboard())
        //        {
        //            Log("Keyboard primary: selected second profile row.");
        //            if (WaitSwitchBackConfirmed(4)) { Log("Switched to profile."); return; }
        //        }

        //        // Fallback for your popup layout:
        //        // Click the 2nd visible profile row (top is current page, second is main profile).
        //        if (TryClickSecondProfileRowInPopupJs())
        //        {
        //            Log("Clicked 2nd profile row in Select profile popup.");
        //            if (WaitSwitchBackConfirmed(4)) { Log("Switched to profile."); return; }
        //        }

        //        // Fallback for "Select profile" popup:
        //        // click the profile row immediately after the currently selected row (checked icon).
        //        if (TryClickRowAfterCurrentProfile())
        //        {
        //            Log("Clicked profile row after current selected page.");
        //            if (WaitSwitchBackConfirmed(4)) { Log("Switched to profile."); return; }
        //        }

        //        // Final fallback: name-based click
        //        if (!string.IsNullOrWhiteSpace(mainProfileName))
        //        {
        //            if (TryClickProfileByNameJs(mainProfileName))
        //            {
        //                Log("Clicked personal profile by JS text match: " + mainProfileName);
        //                if (WaitSwitchBackConfirmed(4)) { Log("Switched to profile."); return; }
        //            }

        //            if (TryClickProfileByVisibleName(mainProfileName))
        //            {
        //                Log("Clicked personal profile by name: " + mainProfileName);
        //                if (WaitSelectProfilePopupClosed(4)) { Log("Switched to profile."); return; }
        //            }
        //        }

        //        // Fallback: click the top profile row directly
        //        var topProfileRow = FindFirstDisplayed(new[]
        //        {
        //            By.XPath("(//div[@role='dialog']//div[@role='button'][.//img])[1]"),
        //            By.XPath("(//div[@role='menu']//div[@role='button'][.//img])[1]"),
        //            By.XPath("(//div[@role='button'][.//img and not(.//*[normalize-space(.)='See all profiles'])])[1]")
        //        }, 2);

        //        if (topProfileRow != null)
        //        {
        //            try
        //            {
        //                var rowText = (topProfileRow.Text ?? "").Trim();
        //                if (!string.IsNullOrWhiteSpace(rowText) &&
        //                    rowText.IndexOf("See all profiles", StringComparison.OrdinalIgnoreCase) < 0)
        //                {
        //                    SafeClick(topProfileRow);
        //                    Thread.Sleep(2000);
        //                    Log("Clicked top profile row from account switch popup.");
        //                    if (WaitSwitchBackConfirmed(4)) { Log("Switched to profile."); return; }
        //                }
        //            }
        //            catch { }
        //        }

        //        // Click "See all profiles"
        //        var seeAll = FindFirstDisplayed(new[]
        //        {
        //            By.XPath("//span[normalize-space(.)='See all profiles']/ancestor::div[@role='button'][1]"),
        //            By.XPath("//span[contains(.,'profiles')]/ancestor::div[@role='button'][1]")
        //        }, 3);

        //        if (seeAll != null)
        //        {
        //            SafeClick(seeAll);
        //            Thread.Sleep(1000);
        //        }

        //        // Click your personal profile (not Page)
        //        var switchProfile = FindFirstDisplayed(new[]
        //        {
        //            By.XPath("//span[contains(.,'Switch to')]/ancestor::div[@role='button'][1]"),
        //            By.XPath("//span[contains(.,'profile')]/ancestor::div[@role='button'][1]")
        //        }, 3);

        //        if (switchProfile != null)
        //        {
        //            SafeClick(switchProfile);
        //            Thread.Sleep(2000);
        //            if (WaitSwitchBackConfirmed(4)) { Log("Switched to profile."); return; }
        //        }
        //        else
        //        {
        //            // Fallback: direct menu item text on some FB variants
        //            var personal = FindFirstDisplayed(new[]
        //            {
        //                By.XPath("//span[normalize-space(.)='Switch to profile']/ancestor::div[@role='button'][1]"),
        //                By.XPath("//span[contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'switch to profile')]/ancestor::div[@role='button'][1]")
        //            }, 2);

        //            if (personal != null)
        //            {
        //                SafeClick(personal);
        //                Thread.Sleep(2000);
        //                if (WaitSwitchBackConfirmed(4)) { Log("Switched to profile."); return; }
        //            }
        //        }

        //        if (IsSelectProfilePopupOpen())
        //        {
        //            // Last automatic fallback (outside Selenium): AutoHotkey click on popup row #2.
        //            if (TryAutoHotkeyClickSecondProfileRow())
        //            {
        //                Log("AutoHotkey fallback: clicked second profile row.");
        //                if (WaitSelectProfilePopupClosed(6))
        //                {
        //                    Log("Switched to profile.");
        //                    return;
        //                }
        //            }

        //            Log("Auto switch failed. Waiting manual click on main profile in 'Select profile' popup...");
        //            if (WaitManualSelectProfilePopupClosed(120))
        //            {
        //                Log("Manual switch detected (popup closed).");
        //                return;
        //            }

        //            Log("Switch profile failed: Select profile popup still open after manual wait timeout.");
        //        }
        //        else
        //        {
        //            Log("Switch flow finished (popup not visible).");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log("SwitchToProfileIdentity error: " + ex.Message);
        //    }
        //}

        private void SwitchToProfileIdentity()
        {
            try
            {
                Log("Switching back to profile identity...");

                // First try direct navigation to the remembered personal profile URL.
                if (!string.IsNullOrWhiteSpace(mainProfileUrl))
                {
                    Log("Trying remembered main profile URL: " + mainProfileUrl);
                    try
                    {
                        SafeGo(mainProfileUrl, 1000);
                        FBTool.WaitingPageLoading(driver);
                        Thread.Sleep(1500);
                        TryClickSwitchNowIfPresent();
                        Thread.Sleep(1200);
                        // If "Switch Now" was available and we're now on profile, done.
                        if (WaitSwitchBackConfirmed(3)) { Log("Switched via profile URL + SwitchNow."); return; }
                    }
                    catch { }
                }

                // Open home so the account/profile menu is in a predictable state.
                try
                {
                    SafeGo("https://www.facebook.com/", 1000);
                    FBTool.WaitingPageLoading(driver);
                    Thread.Sleep(1500);
                }
                catch { }

                // When acting as a Page, Facebook shows a different top-right button.
                // Try ALL known variants of the account/profile menu button.
                bool menuOpened = TryOpenAccountMenu();
                if (!menuOpened)
                {
                    Log("WARNING: Could not open account menu — will still attempt popup click.");
                }

                // Your popup layout is stable: current page row is first, main profile is second.
                // Try a physical click on row #2 immediately after popup opens.
                if (EnsureSelectProfileDialogOpen() && TryOsClickSecondProfileRowByPopupLayout())
                {
                    Log("Popup-layout: clicked 2nd profile row (main profile).");
                    if (WaitSwitchBackConfirmed(4)) { Log("Switched to profile."); return; }
                }

                if (EnsureSelectProfileDialogOpen() && TryNativeClickSecondProfileRowInPopup())
                {
                    Log("Native fallback: clicked 2nd profile row in popup.");
                    if (WaitSwitchBackConfirmed(4)) { Log("Switched to profile."); return; }
                }

                // ── Unified method: text-match mainProfileName first, index-1 fallback ──
                if (SwitchBackToMainProfile())
                {
                    Log("SwitchBackToMainProfile: selected main profile row.");
                    if (WaitSwitchBackConfirmed(4)) { Log("Switched to profile."); return; }
                }

                // Secondary: keyboard navigation
                if (TrySelectSecondProfileByKeyboard())
                {
                    Log("Keyboard: selected second profile row.");
                    if (WaitSwitchBackConfirmed(4)) { Log("Switched to profile."); return; }
                }

                // Fallback: 2nd visible profile row via JS
                if (TryClickSecondProfileRowInPopupJs())
                {
                    Log("Clicked 2nd profile row in Select profile popup.");
                    if (WaitSwitchBackConfirmed(4)) { Log("Switched to profile."); return; }
                }

                // Fallback: row after currently-checked profile
                if (TryClickRowAfterCurrentProfile())
                {
                    Log("Clicked profile row after current selected page.");
                    if (WaitSwitchBackConfirmed(4)) { Log("Switched to profile."); return; }
                }

                // Fallback: name-based click
                if (!string.IsNullOrWhiteSpace(mainProfileName))
                {
                    if (TryClickProfileByNameJs(mainProfileName))
                    {
                        Log("Clicked personal profile by JS text match: " + mainProfileName);
                        if (WaitSwitchBackConfirmed(4)) { Log("Switched to profile."); return; }
                    }
                    if (TryClickProfileByVisibleName(mainProfileName))
                    {
                        Log("Clicked personal profile by name: " + mainProfileName);
                        if (WaitSelectProfilePopupClosed(4)) { Log("Switched to profile."); return; }
                    }
                }

                // Fallback: top profile row
                var topProfileRow = FindFirstDisplayed(new[]
                {
            By.XPath("(//div[@role='dialog']//div[@role='button'][.//img])[1]"),
            By.XPath("(//div[@role='menu']//div[@role='button'][.//img])[1]"),
            By.XPath("(//div[@role='button'][.//img and not(.//*[normalize-space(.)='See all profiles'])])[1]")
        }, 2);

                if (topProfileRow != null)
                {
                    try
                    {
                        var rowText = (topProfileRow.Text ?? "").Trim();
                        if (!string.IsNullOrWhiteSpace(rowText) &&
                            rowText.IndexOf("See all profiles", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            SafeClick(topProfileRow);
                            Thread.Sleep(2000);
                            Log("Clicked top profile row.");
                            if (WaitSwitchBackConfirmed(4)) { Log("Switched to profile."); return; }
                        }
                    }
                    catch { }
                }

                // Click "See all profiles" path
                var seeAll = FindFirstDisplayed(new[]
                {
            By.XPath("//span[normalize-space(.)='See all profiles']/ancestor::div[@role='button'][1]"),
            By.XPath("//span[contains(.,'profiles')]/ancestor::div[@role='button'][1]")
        }, 3);

                if (seeAll != null)
                {
                    SafeClick(seeAll);
                    Thread.Sleep(1000);
                }

                var switchProfile = FindFirstDisplayed(new[]
                {
            By.XPath("//span[contains(.,'Switch to')]/ancestor::div[@role='button'][1]"),
            By.XPath("//span[normalize-space(.)='Switch to profile']/ancestor::div[@role='button'][1]"),
            By.XPath("//span[contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'switch to profile')]/ancestor::div[@role='button'][1]")
        }, 3);

                if (switchProfile != null)
                {
                    SafeClick(switchProfile);
                    Thread.Sleep(2000);
                    if (WaitSwitchBackConfirmed(4)) { Log("Switched to profile."); return; }
                }

                if (IsSelectProfilePopupOpen())
                {
                    if (TryAutoHotkeyClickSecondProfileRow())
                    {
                        Log("AutoHotkey fallback: clicked second profile row.");
                        if (WaitSelectProfilePopupClosed(6)) { Log("Switched to profile."); return; }
                    }

                    // Final non-blocking retry: direct profile URL + "Switch Now" once more.
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(data?.UID))
                        {
                            var directProfile = "https://www.facebook.com/profile.php?id=" + data.UID.Trim();
                            Log("Final retry via direct profile URL: " + directProfile);
                            SafeGo(directProfile, 1000);
                            FBTool.WaitingPageLoading(driver);
                            Thread.Sleep(1000);
                            TryClickSwitchNowIfPresent();
                            Thread.Sleep(1000);
                            if (!IsLikelyPageIdentityUi())
                            {
                                Log("Final direct-profile retry switched to main profile.");
                                return;
                            }
                        }
                    }
                    catch { }

                    // Do not block the whole run waiting 120s for manual input.
                    Log("Switch profile failed (popup still open). Skipping manual wait and continuing.");
                    try
                    {
                        // Best effort close popup so following actions are not blocked.
                        driver.FindElement(By.TagName("body")).SendKeys(OpenQA.Selenium.Keys.Escape);
                        Thread.Sleep(400);
                    }
                    catch { }
                }
                else
                {
                    bool pageUiDetected = false;
                    try { pageUiDetected = IsLikelyPageIdentityUi(); } catch { }

                    Log("Switch flow finished (popup not visible). pageUiDetected=" + pageUiDetected);
                    Log("No-popup branch: trying forced profile switch...");

                    try
                    {
                        var candidates = new List<string>();
                        if (!string.IsNullOrWhiteSpace(mainProfileUrl))
                            candidates.Add(mainProfileUrl.Trim());
                        if (!string.IsNullOrWhiteSpace(data?.UID))
                            candidates.Add("https://www.facebook.com/profile.php?id=" + data.UID.Trim());
                        candidates.Add("https://www.facebook.com/me");

                        foreach (var url in candidates)
                        {
                            if (string.IsNullOrWhiteSpace(url)) continue;

                            Log("No-popup forced retry via: " + url);
                            SafeGo(url, 1000);
                            FBTool.WaitingPageLoading(driver);
                            Thread.Sleep(1000);
                            TryClickSwitchNowIfPresent();
                            Thread.Sleep(1000);

                            if (!IsLikelyPageIdentityUi())
                            {
                                Log("No-popup forced retry switched to main profile.");
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("No-popup forced retry error: " + ex.Message);
                    }

                    if (IsLikelyPageIdentityUi())
                        Log("Switch flow finished (popup not visible) and still Page identity.");
                    else
                        Log("Switch flow finished (popup not visible) after forced retries.");
                }
            }
            catch (Exception ex)
            {
                Log("SwitchToProfileIdentity error: " + ex.Message);
            }
        }

        // ── NEW: handles Page-context menu buttons that differ from personal profile ──
        private bool TryOpenAccountMenu()
        {
            // All known top-right button variants across personal + Page contexts
            var candidates = new[]
            {
        // Standard personal profile
        By.XPath("//div[@aria-label='Account']"),
        By.XPath("//div[@role='button' and @aria-label='Your profile']"),
        // Page context: Facebook shows page avatar with different aria-label
        By.XPath("//div[@aria-label='Account controls and Page switcher']"),
        By.XPath("//div[@aria-label='Page controls and notifications']"),
        By.XPath("//div[@aria-label='Your Pages and profiles']"),
        // Generic fallback: top-right area image button
        By.XPath("(//div[@role='navigation']//div[@role='button'][.//img])[last()]"),
        By.XPath("(//div[@role='banner']//div[@role='button'][.//img])[last()]"),
        By.XPath("(//div[@role='button'][.//img])[last()]"),
    };

            var btn = FindFirstDisplayed(candidates, 5);
            if (btn == null)
            {
                Log("TryOpenAccountMenu: no menu button found.");
                return false;
            }

            try
            {
                Log("TryOpenAccountMenu: clicking " + (btn.GetAttribute("aria-label") ?? "unknown"));
                SafeClick(btn);
                Thread.Sleep(1000);

                // If the actual "Select profile" dialog appeared right away, we're done.
                if (IsSelectProfileDialogOpen()) return true;

                // Some Page contexts show an intermediate menu first.
                // Look for "Switch profile" / "See all profiles" / "Select profile" inside that menu.
                var switchItem = FindFirstDisplayed(new[]
                {
            By.XPath("//span[contains(.,'Switch profile')]/ancestor::div[@role='menuitem'][1]"),
            By.XPath("//span[contains(.,'Switch profile')]/ancestor::div[@role='button'][1]"),
            By.XPath("//span[normalize-space(.)='See all profiles']/ancestor::div[@role='menuitem'][1]"),
            By.XPath("//span[normalize-space(.)='See all profiles']/ancestor::div[@role='button'][1]"),
            By.XPath("//span[normalize-space(.)='Select profile']/ancestor::div[@role='button'][1]"),
        }, 3);

                if (switchItem != null)
                {
                    SafeClick(switchItem);
                    Thread.Sleep(1000);
                }

                return EnsureSelectProfileDialogOpen();
            }
            catch (Exception ex)
            {
                Log("TryOpenAccountMenu error: " + ex.Message);
                return false;
            }
        }

        private bool IsSelectProfileDialogOpen()
        {
            try
            {
                var js = (IJavaScriptExecutor)driver;
                var result = js.ExecuteScript(@"
                    function vis(el){
                        if(!el) return false;
                        var r=el.getBoundingClientRect(), s=getComputedStyle(el);
                        return r.width>0 && r.height>0 && s.display!=='none' && s.visibility!=='hidden';
                    }
                    function txt(el){ return ((el&&(el.innerText||el.textContent))||'').trim(); }
                    var nodes = Array.from(document.querySelectorAll('div,span')).filter(vis);
                    var hasTitle = nodes.some(function(n){ return txt(n) === 'Select profile'; });
                    if (!hasTitle) return false;
                    return nodes.some(function(n){
                        var t = txt(n);
                        if (!t) return false;
                        if (t.indexOf('See all Pages') < 0 && t.indexOf('See all profiles') < 0) return false;
                        var r = n.getBoundingClientRect();
                        if (r.width < 220 || r.width > 760 || r.height < 180 || r.height > 980) return false;
                        return n.querySelectorAll && n.querySelectorAll('img').length >= 2;
                    });
                ");
                return result is bool && (bool)result;
            }
            catch { return false; }
        }

        private bool EnsureSelectProfileDialogOpen()
        {
            try
            {
                if (IsSelectProfileDialogOpen()) return true;

                var item = FindFirstDisplayed(new[]
                {
                    By.XPath("//span[normalize-space(.)='See all profiles']/ancestor::div[@role='menuitem'][1]"),
                    By.XPath("//span[normalize-space(.)='See all profiles']/ancestor::div[@role='button'][1]"),
                    By.XPath("//span[contains(.,'Switch profile')]/ancestor::div[@role='menuitem'][1]"),
                    By.XPath("//span[contains(.,'Switch profile')]/ancestor::div[@role='button'][1]"),
                    By.XPath("//span[normalize-space(.)='Select profile']/ancestor::div[@role='button'][1]")
                }, 2);

                if (item != null)
                {
                    SafeClick(item);
                    Thread.Sleep(1000);
                }

                for (int i = 0; i < 8; i++)
                {
                    if (IsSelectProfileDialogOpen()) return true;
                    Thread.Sleep(250);
                }
            }
            catch { }
            return IsSelectProfileDialogOpen();
        }

        private bool IsSelectProfilePopupOpen()
        {
            try
            {
                var js = (IJavaScriptExecutor)driver;
                var result = js.ExecuteScript(@"
                    function vis(el){ 
                        if(!el) return false; 
                        var r=el.getBoundingClientRect(), s=getComputedStyle(el); 
                        return r.width>0 && r.height>0 && s.display!=='none' && s.visibility!=='hidden'; 
                    }
                    function txt(el){ return ((el&&(el.innerText||el.textContent))||'').trim(); }
                    var nodes = Array.from(document.querySelectorAll('div')).filter(vis);
                    for (var i=0; i<nodes.length; i++){
                        var el = nodes[i], t = txt(el);
                        if (!t) continue;
                        // Match English AND other FB UI languages
                        var hasProfileKeyword = 
                            t.indexOf('Select profile') >= 0 ||
                            t.indexOf('See all Pages') >= 0 ||
                            t.indexOf('See all profiles') >= 0 ||
                            t.indexOf('Switch profile') >= 0;
                        if (!hasProfileKeyword) continue;
                        var r = el.getBoundingClientRect();
                        if (r.width < 200 || r.width > 760 || r.height < 150 || r.height > 980) continue;
                        if (!el.querySelectorAll) continue;
                        // Must have at least 2 avatar images
                        var imgs = el.querySelectorAll('img');
                        if (imgs.length < 2) continue;
                        return true;
                    }
                    return false;
                ");
                return result is bool && (bool)result;
            }
            catch { return false; }
        }


        private bool WaitSelectProfilePopupClosed(int seconds)
        {
            for (int i = 0; i < seconds * 5; i++)
            {
                if (!IsSelectProfilePopupOpen()) return true;
                Thread.Sleep(200);
            }
            return false;
        }

        private bool WaitSwitchBackConfirmed(int seconds)
        {
            if (!WaitSelectProfilePopupClosed(seconds))
                return false;

            // Popup can close even when the click missed the intended row.
            // Confirm we are no longer obviously in Page identity UI.
            for (int i = 0; i < 10; i++)
            {
                if (!IsLikelyPageIdentityUi())
                    return true;

                Thread.Sleep(300);
            }

            Log("Popup closed but still appears to be Page identity UI. Trying forced profile switch...");

            try
            {
                // Sometimes FB closes the popup but stays in Page mode.
                // Force personal profile URL and accept "Switch Now" if shown.
                var directProfile = !string.IsNullOrWhiteSpace(mainProfileUrl)
                    ? mainProfileUrl
                    : (!string.IsNullOrWhiteSpace(data?.UID) ? "https://www.facebook.com/profile.php?id=" + data.UID.Trim() : null);

                if (!string.IsNullOrWhiteSpace(directProfile))
                {
                    Log("Forced switch via profile URL: " + directProfile);
                    SafeGo(directProfile, 1000);
                    FBTool.WaitingPageLoading(driver);
                    Thread.Sleep(1000);
                    TryClickSwitchNowIfPresent();

                    for (int i = 0; i < 12; i++)
                    {
                        if (!IsLikelyPageIdentityUi())
                            return true;
                        Thread.Sleep(300);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Forced switch confirm retry error: " + ex.Message);
            }

            Log("Still in Page identity UI after forced switch retry.");
            return false;
        }

        private bool IsLikelyPageIdentityUi()
        {
            try
            {
                try
                {
                    var cur = SafeGetUrl();
                    if (!string.IsNullOrWhiteSpace(cur) &&
                        cur.IndexOf("professional_dashboard", StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
                catch { }

                string src = "";
                try { src = (driver.PageSource ?? "").ToLowerInvariant(); } catch { src = ""; }
                if (string.IsNullOrEmpty(src)) return false;

                // English UI signals seen in your screenshots while acting as Page.
                if (src.Contains("tips for your page")) return true;
                if (src.Contains("professional dashboard")) return true;
                if (src.Contains("meta business suite")) return true;
                if (src.Contains("ads manager") && src.Contains("ad center")) return true;
                if (src.Contains("recommended post") && src.Contains("see insights")) return true;
                if (src.Contains("boost reel")) return true;
                if (src.Contains("boost post") && src.Contains("professional dashboard")) return true;

                return false;
            }
            catch { return false; }
        }

        private bool WaitManualSelectProfilePopupClosed(int seconds)
        {
            // Manual assist fallback:
            // user clicks the main profile row in the popup, then automation resumes.
            for (int i = 0; i < seconds; i++)
            {
                if (!IsSelectProfilePopupOpen()) return true;
                Thread.Sleep(1000);

                // Periodic reminder so the operator knows the bot is intentionally waiting.
                if (i > 0 && i % 10 == 0)
                {
                    Log("Waiting manual switch... click main profile row, then bot will continue.");
                }
            }

            return !IsSelectProfilePopupOpen();
        }

        private bool IsSwitchProfileFailedToast()
        {
            try
            {
                return driver.PageSource.IndexOf("Switch profile failed", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch { return false; }
        }

        private bool TryClickRowAfterCurrentProfile()
        {
            try
            {
                // Common structure in "Select profile": current row contains a check icon.
                // Then the next sibling row is often the personal profile.
                var nextRow = FindFirstDisplayed(new[]
                {
                    By.XPath("(//div[@role='dialog']//div[@role='button'][.//*[name()='svg'] and .//img])[1]/following-sibling::div[@role='button'][1]"),
                    By.XPath("(//div[@role='dialog']//div[@role='button'][.//img])[1]/following-sibling::div[@role='button'][1]"),
                    By.XPath("(//div[@role='menu']//div[@role='button'][.//img])[1]/following-sibling::div[@role='button'][1]")
                }, 2);

                if (nextRow == null) return false;

                var text = (nextRow.Text ?? "").Trim();
                if (text.IndexOf("See all", StringComparison.OrdinalIgnoreCase) >= 0)
                    return false;

                SafeClick(nextRow);
                Thread.Sleep(2000);
                return true;
            }
            catch { return false; }
        }

        private bool TryClickSecondProfileRowByIndexPopup()
        {
            try
            {
                if (!IsSelectProfilePopupOpen()) return false;

                var js = (IJavaScriptExecutor)driver;
                var rowObj = js.ExecuteScript(
                    "function vis(el){ if(!el) return false; var r=el.getBoundingClientRect(); var s=getComputedStyle(el); return r.width>0&&r.height>0&&s.display!=='none'&&s.visibility!=='hidden'; }" +
                    "function txt(el){ return ((el&&(el.innerText||el.textContent))||'').trim(); }" +
                    "function panel(){ " +
                    "  var nodes = Array.from(document.querySelectorAll('div')).filter(vis);" +
                    "  var best = null;" +
                    "  for (var i=0;i<nodes.length;i++){" +
                    "    var el = nodes[i], t = txt(el);" +
                    "    if (!t) continue;" +
                    "    if (t.indexOf('Select profile')<0 && t.indexOf('See all Pages')<0 && t.indexOf('See all profiles')<0) continue;" +
                    "    var r = el.getBoundingClientRect();" +
                    "    if (r.width < 240 || r.width > 760 || r.height < 220 || r.height > 980) continue;" +
                    "    if (!el.querySelectorAll || el.querySelectorAll('img').length < 2) continue;" +
                    "    if (!best) best = el;" +
                    "    else { var br = best.getBoundingClientRect(); if (r.width*r.height < br.width*br.height) best = el; }" +
                    "  }" +
                    "  return best;" +
                    "}" +
                    "var p = panel(); if(!p) return null;" +
                    "var rows = Array.from(p.querySelectorAll(\"div[role='button'], [role='menuitem']\")).filter(function(el){" +
                    "  if(!vis(el)) return false;" +
                    "  var r = el.getBoundingClientRect();" +
                    "  var t = txt(el);" +
                    "  if (!t) return false;" +
                    "  if (t.indexOf('Select profile')>=0) return false;" +
                    "  if (t.indexOf('See all')>=0) return false;" +
                    "  if (r.width < 180 || r.height < 36) return false;" +
                    "  if (!el.querySelector || !el.querySelector('img')) return false;" +
                    "  return true;" +
                    "});" +
                    "rows.sort(function(a,b){ return a.getBoundingClientRect().top - b.getBoundingClientRect().top; });" +
                    "var uniq=[];" +
                    "for (var j=0;j<rows.length;j++){" +
                    "  var rr = rows[j].getBoundingClientRect();" +
                    "  if (uniq.some(function(u){ var ur=u.getBoundingClientRect(); return Math.abs(ur.top-rr.top)<6; })) continue;" +
                    "  uniq.push(rows[j]);" +
                    "}" +
                    "if (uniq.length < 2) return null;" +
                    "return uniq[1];");

                var row = rowObj as IWebElement;
                if (row == null) return false;

                string rowText = "";
                try { rowText = (row.Text ?? "").Trim(); } catch { }
                if (!string.IsNullOrWhiteSpace(rowText))
                    Log("2nd popup profile row text: " + rowText.Replace("\r", " ").Replace("\n", " "));

                try
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", row);
                    Thread.Sleep(150);
                }
                catch { }

                // Prefer left-center click (avatar/text area) rather than center to avoid right-side controls.
                try
                {
                    var clicked = js.ExecuteScript(
                        "var el=arguments[0]; if(!el) return false;" +
                        "var r=el.getBoundingClientRect();" +
                        "var x=Math.floor(r.left + r.width*0.28);" +
                        "var y=Math.floor(r.top + r.height*0.5);" +
                        "var t=document.elementFromPoint(x,y) || el;" +
                        "['mousedown','mouseup','click'].forEach(function(ev){ try{ t.dispatchEvent(new MouseEvent(ev,{bubbles:true,cancelable:true,view:window,clientX:x,clientY:y})); }catch(e){} });" +
                        "try{ el.click(); }catch(e){}" +
                        "return true;", row);
                    if (clicked is bool && (bool)clicked)
                    {
                        Thread.Sleep(1200);
                        return true;
                    }
                }
                catch { }

                SafeClick(row);
                Thread.Sleep(1200);
                return true;
            }
            catch (Exception ex)
            {
                Log("TryClickSecondProfileRowByIndexPopup error: " + ex.Message);
                return false;
            }
        }

        private bool TryOsClickSecondProfileRowByPopupLayout()
        {
            try
            {
                if (!IsSelectProfilePopupOpen()) return false;

                double left, top, width, height, dpr;
                if (!TryGetSelectProfilePopupRect(out left, out top, out width, out height, out dpr))
                    return false;

                if (width <= 0 || height <= 0) return false;

                Log($"Popup rect => left={left:0.0}, top={top:0.0}, w={width:0.0}, h={height:0.0}, dpr={dpr:0.##}");

                // Layout-based row-2 click (from your screenshot):
                // row2 (main profile) is directly under the checked current page row.
                // Aim slightly higher first; older 0.43 clicks can drift into row3 on some popup heights.
                var candidatesCss = new[]
                {
                    // Prefer text hotspot ("Ro Bin") over avatar/left shell.
                    Tuple.Create(left + width * 0.42, top + height * 0.375, "row2-text-main"),
                    Tuple.Create(left + width * 0.48, top + height * 0.375, "row2-text-right"),
                    Tuple.Create(left + width * 0.36, top + height * 0.375, "row2-text-left"),
                    Tuple.Create(left + width * 0.28, top + height * 0.375, "row2-main"),
                    Tuple.Create(left + width * 0.22, top + height * 0.375, "row2-avatar"),
                    Tuple.Create(left + width * 0.42, top + height * 0.39, "row2-text-down"),
                    Tuple.Create(left + width * 0.42, top + height * 0.36, "row2-text-up"),
                    // Legacy ratios kept as fallbacks.
                    Tuple.Create(left + width * 0.28, top + height * 0.40, "row2-legacy-up"),
                    Tuple.Create(left + width * 0.28, top + height * 0.43, "row2-legacy-mid")
                };

                foreach (var c in candidatesCss)
                {
                    var points = new List<Tuple<int, int, string>>
                    {
                        Tuple.Create((int)Math.Round(c.Item1), (int)Math.Round(c.Item2), c.Item3 + "-css"),
                        Tuple.Create((int)Math.Round(c.Item1 * dpr), (int)Math.Round(c.Item2 * dpr), c.Item3 + "-scaled")
                    };

                    foreach (var p in points)
                    {
                        if (!SetCursorPos(p.Item1, p.Item2))
                            continue;

                        Log($"OS popup-layout click => mode={p.Item3}, x={p.Item1}, y={p.Item2}, dpr={dpr:0.##}");

                        Thread.Sleep(120);
                        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
                        Thread.Sleep(60);
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);

                        // Some Facebook rows behave better with a second click on text hotspot.
                        if (p.Item3.IndexOf("text", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Thread.Sleep(120);
                            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
                            Thread.Sleep(50);
                            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
                        }

                        Thread.Sleep(700);

                        if (!IsSelectProfilePopupOpen())
                            return true;

                        if (IsSwitchProfileFailedToast())
                        {
                            Log("Popup-layout click hit wrong target (Switch profile failed). Retrying...");
                            Thread.Sleep(350);
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Log("TryOsClickSecondProfileRowByPopupLayout error: " + ex.Message);
                return false;
            }
        }

        private bool TryGetSelectProfilePopupRect(out double left, out double top, out double width, out double height, out double dpr)
        {
            left = top = width = height = 0;
            dpr = 1.0;
            try
            {
                var js = (IJavaScriptExecutor)driver;
                var rectObj = js.ExecuteScript(
                    "function vis(el){ if(!el) return false; var r=el.getBoundingClientRect(); var s=getComputedStyle(el); return r.width>0&&r.height>0&&s.display!=='none'&&s.visibility!=='hidden'; }" +
                    "function txt(el){ return ((el&&(el.innerText||el.textContent))||'').trim(); }" +
                    "var panel = null;" +
                    "var candidates = Array.from(document.querySelectorAll('div')).filter(vis);" +
                    "for (var i=0;i<candidates.length;i++){" +
                    "  var el = candidates[i]; var t = txt(el); if(!t) continue;" +
                    "  if (t.indexOf('See all Pages')<0 && t.indexOf('See all profiles')<0) continue;" +
                    "  var r = el.getBoundingClientRect();" +
                    "  if (r.width < 240 || r.width > 700 || r.height < 220 || r.height > 900) continue;" +
                    "  if (!el.querySelectorAll || el.querySelectorAll('img').length < 2) continue;" +
                    "  // Prefer panel-like container near top-right (popup), not page sections." +
                    "  if (!panel) panel = el;" +
                    "  else { var pr = panel.getBoundingClientRect(); if (r.width*r.height < pr.width*pr.height) panel = el; }" +
                    "}" +
                    "if(!panel) return null;" +
                    "var r = panel.getBoundingClientRect();" +
                    "var sx = (window.screenX || window.screenLeft || 0);" +
                    "var sy = (window.screenY || window.screenTop || 0);" +
                    "var chromeTop = Math.max(0, (window.outerHeight - window.innerHeight));" +
                    "var chromeLeft = Math.max(0, (window.outerWidth - window.innerWidth) / 2);" +
                    "return { left: sx + chromeLeft + r.left, top: sy + chromeTop + r.top, width: r.width, height: r.height, dpr: (window.devicePixelRatio || 1) };");

                var dict = rectObj as IDictionary<string, object>;
                if (dict == null) return false;

                left = Convert.ToDouble(dict["left"]);
                top = Convert.ToDouble(dict["top"]);
                width = Convert.ToDouble(dict["width"]);
                height = Convert.ToDouble(dict["height"]);
                dpr = dict.ContainsKey("dpr") ? Convert.ToDouble(dict["dpr"]) : 1.0;
                return true;
            }
            catch (Exception ex)
            {
                Log("TryGetSelectProfilePopupRect error: " + ex.Message);
                return false;
            }
        }

        private bool TryAutoHotkeyClickSecondProfileRow()
        {
            try
            {
                if (!IsSelectProfilePopupOpen()) return false;

                string ahkExe = FindAutoHotkeyExe();
                if (string.IsNullOrWhiteSpace(ahkExe))
                {
                    Log("AutoHotkey fallback skipped: AutoHotkey.exe not found.");
                    return false;
                }

                double left, top, width, height, dpr;
                if (!TryGetSelectProfilePopupRect(out left, out top, out width, out height, out dpr))
                    return false;

                // Row #2 (main profile) in popup layout from screenshot: click avatar/text band.
                double clickX = left + (width * 0.28);
                double clickY = top + (height * 0.43);

                // Use unscaled first (AutoHotkey often works in logical coords on Windows).
                int x1 = (int)Math.Round(clickX);
                int y1 = (int)Math.Round(clickY);
                int x2 = (int)Math.Round(clickX * dpr);
                int y2 = (int)Math.Round(clickY * dpr);

                string scriptPath = Path.Combine(Path.GetTempPath(), "khbrowser_switch_profile_row2.ahk");
                string script = BuildAutoHotkeySwitchScript(x1, y1, x2, y2);
                File.WriteAllText(scriptPath, script);

                Log($"AutoHotkey fallback running => x1={x1},y1={y1}, x2={x2},y2={y2}, dpr={dpr:0.##}");

                var p = Process.Start(new ProcessStartInfo
                {
                    FileName = ahkExe,
                    Arguments = "\"" + scriptPath + "\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });

                try { if (p != null) p.WaitForExit(6000); } catch { }
                Thread.Sleep(800);
                return !IsSelectProfilePopupOpen();
            }
            catch (Exception ex)
            {
                Log("TryAutoHotkeyClickSecondProfileRow error: " + ex.Message);
                return false;
            }
        }
        private bool SwitchBackToMainProfile()
        {
            try
            {
                if (!EnsureSelectProfileDialogOpen())
                {
                    Log("SwitchBackToMainProfile: Select profile dialog not open, skipping.");
                    return false;
                }

                var js = (IJavaScriptExecutor)driver;

                // Use the actual mainProfileName from data for logs, but per your confirmed UI layout
                // we prioritize row #2 under the current Page profile.
                string profileNameToFind = (mainProfileName ?? "").Trim();
                Log("SwitchBackToMainProfile: looking for profile name='" + profileNameToFind + "'");

                var result = js.ExecuteScript(@"
            var profileName = arguments[0] || '';
            
            function vis(el){ 
                if(!el) return false;
                var r=el.getBoundingClientRect(), s=getComputedStyle(el); 
                return r.width>0 && r.height>0 && s.display!=='none' && s.visibility!=='hidden'; 
            }
            function txt(el){ return ((el&&(el.innerText||el.textContent))||'').trim(); }

            // Find the popup panel - look for any container with multiple profile avatars
            var panel = null;
            var allDivs = Array.from(document.querySelectorAll('div')).filter(vis);
            for (var i=0; i<allDivs.length; i++){
                var el = allDivs[i];
                if (!el.querySelectorAll) continue;
                var imgs = el.querySelectorAll('img');
                if (imgs.length < 2) continue;
                var r = el.getBoundingClientRect();
                if (r.width < 200 || r.width > 760 || r.height < 150 || r.height > 980) continue;
                var t = txt(el);
                if (!t) continue;
                // Must look like a profile switcher panel
                var isPanel = t.indexOf('Select profile') >= 0 || 
                              t.indexOf('See all') >= 0 || 
                              t.indexOf('Switch profile') >= 0;
                if (!isPanel) continue;
                if (!panel) panel = el;
                else {
                    var pr = panel.getBoundingClientRect();
                    if (r.width*r.height < pr.width*pr.height) panel = el;
                }
            }
            
            if (!panel) return { found: false, error: 'no panel', rows: [] };

            // Build rows from visible avatars (more robust than role='button' on FB variants)
            var avatars = Array.from(panel.querySelectorAll('img')).filter(function(img){
                if (!vis(img)) return false;
                var r = img.getBoundingClientRect();
                return r.width >= 20 && r.height >= 20;
            });
            avatars.sort(function(a,b){ return a.getBoundingClientRect().top - b.getBoundingClientRect().top; });

            var uniqAvatars = [];
            avatars.forEach(function(img){
                var ir = img.getBoundingClientRect();
                if (!uniqAvatars.some(function(u){
                    var ur = u.getBoundingClientRect();
                    return Math.abs(ur.top - ir.top) < 4 && Math.abs(ur.left - ir.left) < 4;
                })) uniqAvatars.push(img);
            });

            var uniq = [];
            uniqAvatars.forEach(function(img){
                var row = img;
                for (var d=0; d<10 && row; d++){
                    var r = row.getBoundingClientRect(), t = txt(row);
                    var imgCount = (row.querySelectorAll ? row.querySelectorAll('img').length : 0);
                    if (vis(row) &&
                        r.width > 180 && r.height > 36 && r.height < 140 &&
                        row.querySelector && row.querySelector('img') &&
                        imgCount >= 1 && imgCount <= 3 &&
                        t && t.indexOf('See all') < 0 && t !== 'Select profile') {
                        break;
                    }
                    row = row.parentElement;
                }
                if (!row) return;
                var rr = row.getBoundingClientRect();
                if (uniq.some(function(u){ var ur=u.getBoundingClientRect(); return Math.abs(ur.top-rr.top) < 6; })) return;
                uniq.push(row);
            });

            uniq.sort(function(a,b){ return a.getBoundingClientRect().top - b.getBoundingClientRect().top; });

            var names = uniq.map(function(el){ return txt(el).replace(/\s+/g,' ').substring(0,40); });

            // Prefer: row immediately after the CURRENT selected row (the one with a check icon).
            // Fallback: visual row #2 from top, then name match.
            var target = null;
            var currentIndex = -1;
            for (var k=0; k<uniq.length; k++){
                var row = uniq[k];
                // Current selected profile row usually has a check icon SVG on the right.
                // Be stricter than has-any-svg because notification rows can also contain icons.
                var hasAvatar = !!(row.querySelector && row.querySelector('img'));
                var ariaChecked = (row.getAttribute && row.getAttribute('aria-checked')) || '';
                var rr0 = row.getBoundingClientRect();
                var hasRightCheckLikeSvg = false;
                try
                {
                    var svgs = Array.from(row.querySelectorAll ? row.querySelectorAll('svg') : []).filter(vis);
                    hasRightCheckLikeSvg = svgs.some(function(s){
                        var sr = s.getBoundingClientRect();
                        return sr.width >= 12 && sr.height >= 12 &&
                               sr.left > (rr0.left + rr0.width * 0.68);
                    });
                }
                catch (e) { }
                if (hasAvatar && (ariaChecked === 'true' || hasRightCheckLikeSvg))
                {
                    currentIndex = k;
                    break;
                }
            }

            if (currentIndex >= 0 && currentIndex + 1 < uniq.length)
            {
                target = uniq[currentIndex + 1];
            }
            if (!target && uniq.length >= 2) target = uniq[1];
            if (!target && profileName)
            {
                target = uniq.find(function(el){
                    return txt(el).toLowerCase().indexOf(profileName.toLowerCase()) >= 0;
                });
            }
            if (!target) return { found: false, error: 'no target', rows: names, currentIndex: currentIndex }
            ;

            var targetName = txt(target).replace(/\s+/g,' ').substring(0,40);
            var tr = target.getBoundingClientRect();
            var sx = (window.screenX || window.screenLeft || 0);
            var sy = (window.screenY || window.screenTop || 0);
            var chromeTop = Math.max(0, (window.outerHeight - window.innerHeight));
            var chromeLeft = Math.max(0, (window.outerWidth - window.innerWidth) / 2);
            var dpr = (window.devicePixelRatio || 1);

            // Click a clickable descendant (role=button/a/tabindex) if present, then target row itself.
            try
            {
                var r = target.getBoundingClientRect();
                var x = Math.floor(r.left + r.width * 0.28);
                var y = Math.floor(r.top + r.height * 0.50);
                var hit = document.elementFromPoint(x, y) || target;
                var clickEl = null;
                try
                {
                    clickEl = (hit && hit.closest) ? (hit.closest('[role=""button""],a,[tabindex]') || hit) : hit;
                }
                catch (e) { clickEl = hit; }
                if (!clickEl || !target.contains(clickEl))
                {
                    try
                    {
                        var innerClickable = target.querySelector('[role=""button""],a,[tabindex]');
                        if (innerClickable) clickEl = innerClickable;
                    }
                    catch (e) { }
                }
                [clickEl, hit, target].forEach(function(el){
                    if (!el) return;
                    ['pointerdown', 'mousedown', 'pointerup', 'mouseup', 'click'].forEach(function(ev){
                        try
                        {
                            var Ctor = ev.indexOf('pointer') === 0 ? PointerEvent : MouseEvent;
                            el.dispatchEvent(new Ctor(ev,{ bubbles:true, cancelable:true, view:window, clientX:x, clientY:y }));
                        }
                        catch (e)
                        {
                            try { el.dispatchEvent(new MouseEvent(ev.replace('pointer', 'mouse'),{ bubbles:true, cancelable:true, view:window, clientX:x, clientY:y })); } catch (e2) { }
                        }
                    });
                    try { el.focus && el.focus(); } catch (e) { }
                    try
                    {
                        el.dispatchEvent(new KeyboardEvent('keydown',{ key:'Enter', code:'Enter', bubbles:true }));
                        el.dispatchEvent(new KeyboardEvent('keyup',{ key:'Enter', code:'Enter', bubbles:true }));
                    }
                    catch (e) { }
                });
            }
            catch (e) { }
            try { target.click(); } catch (e) { }

            return {
            found: true, 
                name: targetName, 
                rows: names, 
                currentIndex: currentIndex,
                targetLeft: (sx + chromeLeft + tr.left),
                targetTop: (sy + chromeTop + tr.top),
                targetWidth: tr.width,
                targetHeight: tr.height,
                targetDpr: dpr
            }
            ;
            ", profileNameToFind);

                var dict = result as IDictionary<string, object>;
                if (dict == null)
                {
                    Log("SwitchBackToMainProfile: JS returned null");
                    return false;
                }

                // Always log the rows found for debugging
                var rowNames = "";
                if (dict.ContainsKey("rows"))
                {
                    var rowList = dict["rows"] as IList<object>;
                    if (rowList != null) rowNames = string.Join(" | ", rowList);
                }
                Log("SwitchBackToMainProfile rows found: [" + rowNames + "]");

                if (dict.ContainsKey("error"))
                    Log("SwitchBackToMainProfile error detail: " + dict["error"]);
                if (dict.ContainsKey("currentIndex"))
                    Log("SwitchBackToMainProfile current row index: " + (dict["currentIndex"] ?? "null"));

                bool found = dict.ContainsKey("found") && dict["found"] is bool && (bool)dict["found"];
                if (!found)
                {
                    Log("SwitchBackToMainProfile: target row not found.");
                    return false;
                }

                var clickedName = dict.ContainsKey("name") ? dict["name"]?.ToString() : "?";
                Log("SwitchBackToMainProfile: clicked row '" + clickedName + "'");

                Thread.Sleep(1500);

                if (!IsSelectProfilePopupOpen())
                {
                    Log("SwitchBackToMainProfile: popup closed — success.");
                    return true;
                }

                // Popup still open — try OS click on the exact JS-selected row first.
                Log("SwitchBackToMainProfile: popup still open, trying exact-row OS click...");
                try
                {
                    if (dict.ContainsKey("targetLeft") && dict.ContainsKey("targetTop"))
                    {
                        double left = Convert.ToDouble(dict["targetLeft"]);
                        double top = Convert.ToDouble(dict["targetTop"]);
                        double width = dict.ContainsKey("targetWidth") ? Convert.ToDouble(dict["targetWidth"]) : 0;
                        double height = dict.ContainsKey("targetHeight") ? Convert.ToDouble(dict["targetHeight"]) : 0;
                        double dpr = dict.ContainsKey("targetDpr") ? Convert.ToDouble(dict["targetDpr"]) : 1.0;

                        if (width > 0 && height > 0)
                        {
                            var cx = left + (width * 0.28);
                            var cy = top + (height * 0.50);
                            var mx = left + (width * 0.50);
                            var my = top + (height * 0.50);

                            var points = new List<Tuple<int, int, string>>
                            {
                                Tuple.Create((int)Math.Round(cx), (int)Math.Round(cy), "exact-css-left"),
                                Tuple.Create((int)Math.Round(mx), (int)Math.Round(my), "exact-css-center"),
                                Tuple.Create((int)Math.Round(cx * dpr), (int)Math.Round(cy * dpr), "exact-scaled-left"),
                                Tuple.Create((int)Math.Round(mx * dpr), (int)Math.Round(my * dpr), "exact-scaled-center")
                            };

                            foreach (var p in points)
                            {
                                if (!SetCursorPos(p.Item1, p.Item2))
                                    continue;

                                Log($"SwitchBackToMainProfile exact OS click => mode={p.Item3}, x={p.Item1}, y={p.Item2}, dpr={dpr:0.##}");
                                Thread.Sleep(120);
                                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
                                Thread.Sleep(60);
                                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);

                                // A second click often helps on Facebook profile switch rows.
                                Thread.Sleep(120);
                                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
                                Thread.Sleep(50);
                                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);

                                Thread.Sleep(800);
                                if (!IsSelectProfilePopupOpen())
                                {
                                    Log("SwitchBackToMainProfile: exact-row OS click succeeded.");
                                    return true;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("SwitchBackToMainProfile exact OS click error: " + ex.Message);
                }

                // Popup still open — try generic layout-based OS click fallback
                Log("SwitchBackToMainProfile: trying popup-layout OS click...");
                if (TryOsClickSecondProfileRowByPopupLayout())
                {
                    Thread.Sleep(1000);
                    if (!IsSelectProfilePopupOpen())
                    {
                        Log("SwitchBackToMainProfile: OS click succeeded.");
                        return true;
                    }
                }

                // Last try: native Selenium Actions click
                Log("SwitchBackToMainProfile: trying native Selenium click...");
                try
                {
                    var rowEl = js.ExecuteScript(@"
                var profileName = arguments[0] || '';
                function vis(el){ var r=el.getBoundingClientRect(),s=getComputedStyle(el); return r.width>0&&r.height>0&&s.display!=='none'&&s.visibility!=='hidden'; }
                function txt(el){ return ((el&&(el.innerText||el.textContent))||'').trim(); }
                var rows = Array.from(document.querySelectorAll('div[role=""button""]')).filter(function(el){
                    if(!vis(el)) return false;
                    if(!el.querySelector('img')) return false;
                    var t=txt(el), r=el.getBoundingClientRect();
                    if(!t||t.indexOf('See all')>=0||r.width<150||r.height<30) return false;
                    return true;
                });
                rows.sort(function(a,b){ return a.getBoundingClientRect().top-b.getBoundingClientRect().top; });
                var uniq=[];
                rows.forEach(function(el){ var top=el.getBoundingClientRect().top; if(!uniq.some(function(u){ return Math.abs(u.getBoundingClientRect().top-top)<8; })) uniq.push(el); });
                if (profileName) {
                    var byName = uniq.find(function(el){ return txt(el).toLowerCase().indexOf(profileName.toLowerCase())>=0; });
                    if (byName) return byName;
                }
                return uniq.length >= 2 ? uniq[1] : null;
            ", profileNameToFind) as IWebElement;

                    if (rowEl != null)
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", rowEl);
                        Thread.Sleep(300);
                        new OpenQA.Selenium.Interactions.Actions(driver)
                            .MoveToElement(rowEl)
                            .Click()
                            .Build()
                            .Perform();
                        Thread.Sleep(1500);
                        if (!IsSelectProfilePopupOpen())
                        {
                            Log("SwitchBackToMainProfile: Selenium Actions click succeeded.");
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("SwitchBackToMainProfile Selenium click error: " + ex.Message);
                }

                return false;
            }
            catch (Exception ex)
            {
                Log("SwitchBackToMainProfile error: " + ex.Message);
                return false;
            }
        }
        private string FindAutoHotkeyExe()
        {
            try
            {
                var candidates = new[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "AutoHotkey", "AutoHotkey.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "AutoHotkey", "v2", "AutoHotkey64.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "AutoHotkey", "v2", "AutoHotkey.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "AutoHotkey", "AutoHotkey.exe"),
                };

                foreach (var p in candidates)
                {
                    try { if (!string.IsNullOrWhiteSpace(p) && File.Exists(p)) return p; } catch { }
                }
            }
            catch { }
            return null;
        }

        private string BuildAutoHotkeySwitchScript(int x1, int y1, int x2, int y2)
        {
            // AutoHotkey v1 script syntax (most common in automation setups).
            // Try logical coords first, then scaled coords as retry.
            return
                "CoordMode, Mouse, Screen\r\n" +
                "SetMouseDelay, 50\r\n" +
                "MouseMove, " + x1 + ", " + y1 + ", 0\r\n" +
                "Click\r\n" +
                "Sleep, 700\r\n" +
                "MouseMove, " + x2 + ", " + y2 + ", 0\r\n" +
                "Click\r\n" +
                "Sleep, 700\r\n" +
                "ExitApp\r\n";
        }

        private bool TryNativeClickSecondProfileRowInPopup()
        {
            try
            {
                if (!IsSelectProfilePopupOpen()) return false;

                var js = (IJavaScriptExecutor)driver;
                var rowObj = js.ExecuteScript(
                    "function vis(el){ if(!el) return false; var r=el.getBoundingClientRect(); var s=getComputedStyle(el); return r.width>0&&r.height>0&&s.display!=='none'&&s.visibility!=='hidden'; }" +
                    "function txt(el){ return ((el&&(el.innerText||el.textContent))||'').trim(); }" +
                    "var marker = Array.from(document.querySelectorAll('div,span')).find(function(n){ if(!vis(n)) return false; var t=txt(n); return t==='See all Pages' || t==='See all profiles' || t==='Select profile'; });" +
                    "if(!marker) return null;" +
                    "var panel = marker;" +
                    "for(var i=0;i<18 && panel; i++){ var t=txt(panel); if((t.indexOf('See all Pages')>=0 || t.indexOf('See all profiles')>=0) && panel.querySelectorAll && panel.querySelectorAll('img').length>=2) break; panel=panel.parentElement; }" +
                    "if(!panel) return null;" +
                    "var imgs = Array.from(panel.querySelectorAll('img')).filter(function(img){ if(!vis(img)) return false; var r=img.getBoundingClientRect(); return r.width>=20 && r.height>=20; });" +
                    "imgs.sort(function(a,b){ return a.getBoundingClientRect().top - b.getBoundingClientRect().top; });" +
                    "var unique=[];" +
                    "for(var j=0;j<imgs.length;j++){ var im=imgs[j], r=im.getBoundingClientRect(); if(unique.some(function(u){ var ur=u.getBoundingClientRect(); return Math.abs(ur.top-r.top)<4 && Math.abs(ur.left-r.left)<4; })) continue; unique.push(im); }" +
                    "if(unique.length < 2) return null;" +
                    "var row = unique[1];" +
                    "for(var d=0; d<8 && row; d++){ var rt=txt(row), rr=row.getBoundingClientRect(); if(vis(row) && rr.width>180 && rr.height>35 && rt && rt.indexOf('See all')<0) break; row=row.parentElement; }" +
                    "return row || null;");

                var row = rowObj as IWebElement;
                if (row == null)
                {
                    Log("Native click fallback: could not resolve second profile row element.");
                    return false;
                }

                try
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", row);
                    Thread.Sleep(250);
                }
                catch { }

                string rowText = "";
                try { rowText = (row.Text ?? "").Trim(); } catch { }

                // First try OS-level mouse click at row center (trusted native click).
                if (TryOsClickElementCenter(row))
                {
                    Thread.Sleep(1200);
                    if (!string.IsNullOrWhiteSpace(rowText))
                        Log("OS clicked row text: " + rowText.Replace("\r", " ").Replace("\n", " "));
                    return true;
                }

                try
                {
                    new Actions(driver)
                        .MoveToElement(row)
                        .Click()
                        .Build()
                        .Perform();
                    Thread.Sleep(1200);
                }
                catch (Exception ex)
                {
                    Log("Native click fallback error: " + ex.Message);
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(rowText))
                    Log("Native clicked row text: " + rowText.Replace("\r", " ").Replace("\n", " "));

                return true;
            }
            catch (Exception ex)
            {
                Log("TryNativeClickSecondProfileRowInPopup error: " + ex.Message);
                return false;
            }
        }

        private bool TryOsClickElementCenter(IWebElement element)
        {
            try
            {
                if (element == null) return false;

                var js = (IJavaScriptExecutor)driver;
                var pointObj = js.ExecuteScript(
                    "var el = arguments[0];" +
                    "if(!el) return null;" +
                    "var r = el.getBoundingClientRect();" +
                    "var sx = (window.screenX || window.screenLeft || 0);" +
                    "var sy = (window.screenY || window.screenTop || 0);" +
                    "var chromeTop = Math.max(0, (window.outerHeight - window.innerHeight));" +
                    "var chromeLeft = Math.max(0, (window.outerWidth - window.innerWidth) / 2);" +
                    "return { x: sx + chromeLeft + r.left, y: sy + chromeTop + r.top, w: r.width, h: r.height, dpr: (window.devicePixelRatio || 1) };",
                    element);

                var dict = pointObj as IDictionary<string, object>;
                if (dict == null || !dict.ContainsKey("x") || !dict.ContainsKey("y"))
                {
                    Log("OS click fallback: could not compute screen point.");
                    return false;
                }

                double left = Convert.ToDouble(dict["x"]);
                double top = Convert.ToDouble(dict["y"]);
                double width = dict.ContainsKey("w") ? Convert.ToDouble(dict["w"]) : 0;
                double height = dict.ContainsKey("h") ? Convert.ToDouble(dict["h"]) : 0;
                double dpr = dict.ContainsKey("dpr") ? Convert.ToDouble(dict["dpr"]) : 1.0;

                if (width <= 0 || height <= 0)
                    return false;

                // Prefer left-center of row (over text/avatar area) over exact center.
                double cx = left + (width * 0.28);
                double cy = top + (height * 0.50);
                double mx = left + (width * 0.50);
                double my = top + (height * 0.50);

                var candidates = new List<Tuple<int, int, string>>
                {
                    Tuple.Create((int)Math.Round(cx), (int)Math.Round(cy), "css-left"),
                    Tuple.Create((int)Math.Round(mx), (int)Math.Round(my), "css-center"),
                    Tuple.Create((int)Math.Round(cx * dpr), (int)Math.Round(cy * dpr), "scaled-left"),
                    Tuple.Create((int)Math.Round(mx * dpr), (int)Math.Round(my * dpr), "scaled-center")
                };

                foreach (var p in candidates)
                {
                    try
                    {
                        Log($"OS click point => mode={p.Item3}, x={p.Item1}, y={p.Item2}, dpr={dpr:0.##}");

                        if (!SetCursorPos(p.Item1, p.Item2))
                            continue;

                        Thread.Sleep(120);
                        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
                        Thread.Sleep(60);
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
                        Thread.Sleep(500);

                        if (!IsSelectProfilePopupOpen())
                            return true;

                        if (IsSwitchProfileFailedToast())
                        {
                            Log("OS click attempt triggered 'Switch profile failed' toast. Retrying point...");
                            Thread.Sleep(400);
                        }
                    }
                    catch { }
                }

                return false;
            }
            catch (Exception ex)
            {
                Log("TryOsClickElementCenter error: " + ex.Message);
                return false;
            }
        }

        private bool TryClickSecondProfileRowInPopupJs()
        {
            try
            {
                var js = (IJavaScriptExecutor)driver;
                var script =
                    "function vis(el){ if(!el) return false; var r=el.getBoundingClientRect(); var s=getComputedStyle(el); return r.width>0&&r.height>0&&s.display!=='none'&&s.visibility!=='hidden'; }\n" +
                    "function txt(el){ return ((el && (el.innerText||el.textContent))||'').trim(); }\n" +
                    "function clickAtCenter(el){\n" +
                    "  if(!el) return false;\n" +
                    "  try{ el.scrollIntoView({block:'center'});}catch(e){}\n" +
                    "  var r = el.getBoundingClientRect();\n" +
                    "  var x = Math.floor(r.left + r.width/2);\n" +
                    "  var y = Math.floor(r.top + r.height/2);\n" +
                    "  try{ var t = document.elementFromPoint(x,y); if(t){ t.dispatchEvent(new MouseEvent('mousedown',{bubbles:true,cancelable:true,view:window,clientX:x,clientY:y})); t.dispatchEvent(new MouseEvent('mouseup',{bubbles:true,cancelable:true,view:window,clientX:x,clientY:y})); t.dispatchEvent(new MouseEvent('click',{bubbles:true,cancelable:true,view:window,clientX:x,clientY:y})); return 'center:' + (t.tagName||''); } }catch(e){}\n" +
                    "  try{ el.click(); return 'row'; }catch(e){}\n" +
                    "  return false;\n" +
                    "}\n" +
                    "var marker = Array.from(document.querySelectorAll('div,span')).find(function(n){ if(!vis(n)) return false; var t=txt(n); return t==='See all Pages' || t==='See all profiles' || t==='Select profile'; });\n" +
                    "if(!marker) return 'ERR:no_marker';\n" +
                    "var panel = marker;\n" +
                    "for(var i=0;i<18 && panel; i++){\n" +
                    "  var t = txt(panel);\n" +
                    "  if((t.indexOf('See all Pages')>=0 || t.indexOf('See all profiles')>=0) && panel.querySelectorAll && panel.querySelectorAll('img').length>=2) break;\n" +
                    "  panel = panel.parentElement;\n" +
                    "}\n" +
                    "if(!panel) return 'ERR:no_panel';\n" +
                    "var imgs = Array.from(panel.querySelectorAll('img')).filter(function(img){ if(!vis(img)) return false; var r=img.getBoundingClientRect(); return r.width>=20 && r.height>=20; });\n" +
                    "imgs.sort(function(a,b){ return a.getBoundingClientRect().top - b.getBoundingClientRect().top; });\n" +
                    "var unique=[];\n" +
                    "for(var j=0;j<imgs.length;j++){\n" +
                    "  var im=imgs[j]; var r=im.getBoundingClientRect();\n" +
                    "  if(unique.some(function(u){ var ur=u.getBoundingClientRect(); return Math.abs(ur.top-r.top)<4 && Math.abs(ur.left-r.left)<4; })) continue;\n" +
                    "  unique.push(im);\n" +
                    "}\n" +
                    "if(unique.length < 2) return 'ERR:avatars=' + unique.length;\n" +
                    "var secondImg = unique[1];\n" +
                    "var row = secondImg;\n" +
                    "for(var d=0; d<8 && row; d++){\n" +
                    "  var rt = txt(row);\n" +
                    "  var rr = row.getBoundingClientRect();\n" +
                    "  if(vis(row) && rr.width > 180 && rr.height > 35 && rt && rt.indexOf('See all') < 0) break;\n" +
                    "  row = row.parentElement;\n" +
                    "}\n" +
                    "if(!row) return 'ERR:no_row';\n" +
                    "var rowText = txt(row).replace(/\\s+/g,' ').slice(0,80);\n" +
                    "var clickRes = clickAtCenter(row);\n" +
                    "if(!clickRes) return 'ERR:click_false:' + rowText;\n" +
                    "return 'OK:' + clickRes + ':' + rowText;";

                var result = js.ExecuteScript(script);
                Log("TryClickSecondProfileRowInPopupJs => " + (result == null ? "null" : result.ToString()));

                var ok = false;
                if (result is bool) ok = (bool)result;
                else if (result != null)
                {
                    var s = result.ToString();
                    ok = s.StartsWith("OK:", StringComparison.OrdinalIgnoreCase);
                }

                if (ok)
                {
                    Thread.Sleep(1800);
                    return true;
                }
            }
            catch { }
            return false;
        }

        private bool TrySelectSecondProfileByKeyboard()
        {
            try
            {
                if (!IsSelectProfilePopupOpen()) return false;

                var target = FindFirstDisplayed(new[]
                {
                    By.XPath("//*[normalize-space(.)='Select profile']"),
                    By.XPath("//div[@role='dialog']"),
                    By.TagName("body")
                }, 1);

                if (target != null)
                {
                    try { SafeClickScroll(target); } catch { }
                    Thread.Sleep(300);
                }

                // Focus position can vary (header/current row/button). Try multiple sequences.
                // Strategy A: reset with Home, then ArrowDown N times.
                int[] downCounts = new[] { 1, 2, 3, 4 };
                foreach (var downCount in downCounts)
                {
                    try
                    {
                        // reset to top
                        new Actions(driver)
                            .SendKeys(OpenQA.Selenium.Keys.Home)
                            .Build()
                            .Perform();
                        Thread.Sleep(250);

                        for (int i = 0; i < downCount; i++)
                        {
                            new Actions(driver)
                                .SendKeys(OpenQA.Selenium.Keys.ArrowDown)
                                .Build()
                                .Perform();
                            Thread.Sleep(200);
                        }

                        new Actions(driver)
                            .SendKeys(OpenQA.Selenium.Keys.Enter)
                            .Build()
                            .Perform();

                        Thread.Sleep(1200);

                        // Success: popup closes
                        if (!IsSelectProfilePopupOpen())
                            return true;

                        // If FB says failed, retry another offset.
                        if (IsSwitchProfileFailedToast())
                        {
                            Log("Keyboard switch attempt failed toast (down=" + downCount + "). Retrying...");
                            Thread.Sleep(600);
                            continue;
                        }
                    }
                    catch { }
                }

                // Strategy B: tab through focusable elements in popup and press Enter.
                int[] tabCounts = new[] { 1, 2, 3, 4, 5, 6 };
                foreach (var tabCount in tabCounts)
                {
                    try
                    {
                        // Try to focus popup header/body again before tabbing.
                        var focusTarget = FindFirstDisplayed(new[]
                        {
                            By.XPath("//*[normalize-space(.)='Select profile']"),
                            By.XPath("//div[@role='dialog']"),
                            By.TagName("body")
                        }, 1);
                        if (focusTarget != null)
                        {
                            try { SafeClickScroll(focusTarget); } catch { }
                            Thread.Sleep(250);
                        }

                        for (int i = 0; i < tabCount; i++)
                        {
                            new Actions(driver)
                                .SendKeys(OpenQA.Selenium.Keys.Tab)
                                .Build()
                                .Perform();
                            Thread.Sleep(180);
                        }

                        new Actions(driver)
                            .SendKeys(OpenQA.Selenium.Keys.Enter)
                            .Build()
                            .Perform();

                        Thread.Sleep(1200);

                        if (!IsSelectProfilePopupOpen())
                            return true;

                        if (IsSwitchProfileFailedToast())
                        {
                            Log("Keyboard switch attempt failed toast (tab=" + tabCount + "). Retrying...");
                            Thread.Sleep(600);
                            continue;
                        }
                    }
                    catch { }
                }

                return false;
            }
            catch { return false; }
        }

        private bool TryClickProfileByVisibleName(string profileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(profileName)) return false;
                string target = profileName.Trim();

                // 1) Exact text node hit (span/div), then climb to clickable row.
                var textNode = FindFirstDisplayed(new[]
                {
                    By.XPath("//div[@role='dialog']//*[self::span or self::div][normalize-space(.)=\"" + EscapeXPathLiteral(target) + "\"]"),
                    By.XPath("//div[@role='menu']//*[self::span or self::div][normalize-space(.)=\"" + EscapeXPathLiteral(target) + "\"]"),
                    By.XPath("//div[@role='dialog']//*[self::span or self::div][contains(normalize-space(.),\"" + EscapeXPathLiteral(target) + "\")]"),
                    By.XPath("//div[@role='menu']//*[self::span or self::div][contains(normalize-space(.),\"" + EscapeXPathLiteral(target) + "\")]")
                }, 2);

                if (textNode != null)
                {
                    if (ClickNearestProfileRow(textNode))
                        return true;
                }

                // 2) Brute-force visible rows with images in popup/menu.
                var rows = driver.FindElements(By.XPath("//div[@role='dialog']//div[.//img] | //div[@role='menu']//div[.//img]"));
                foreach (var row in rows)
                {
                    try
                    {
                        if (!row.Displayed) continue;
                        var t = (row.Text ?? "").Trim();
                        if (string.IsNullOrWhiteSpace(t)) continue;
                        if (t.IndexOf("See all", StringComparison.OrdinalIgnoreCase) >= 0) continue;

                        if (t.IndexOf(target, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            SafeClickScroll(row);
                            Thread.Sleep(2000);
                            return true;
                        }
                    }
                    catch { }
                }
            }
            catch { }

            return false;
        }

        private bool TryClickProfileByNameJs(string profileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(profileName)) return false;

                var js = (IJavaScriptExecutor)driver;
                var script =
                    "var target = (arguments[0] || '').trim().toLowerCase();\n" +
                    "if (!target) return false;\n" +
                    "function isVisible(el) {\n" +
                    "  if (!el) return false;\n" +
                    "  var r = el.getBoundingClientRect();\n" +
                    "  var s = window.getComputedStyle(el);\n" +
                    "  return r.width > 0 && r.height > 0 && s.visibility !== 'hidden' && s.display !== 'none';\n" +
                    "}\n" +
                    "function clickEl(el) {\n" +
                    "  try { el.scrollIntoView({block:'center'}); } catch(e) {}\n" +
                    "  try { el.click(); return true; } catch(e) {}\n" +
                    "  try { el.dispatchEvent(new MouseEvent('click', {bubbles:true, cancelable:true, view:window})); return true; } catch(e) {}\n" +
                    "  return false;\n" +
                    "}\n" +
                    "var nodes = Array.from(document.querySelectorAll('div, span'));\n" +
                    "for (var i = 0; i < nodes.length; i++) {\n" +
                    "  var n = nodes[i];\n" +
                    "  if (!isVisible(n)) continue;\n" +
                    "  var txt = (n.innerText || n.textContent || '').trim();\n" +
                    "  if (!txt) continue;\n" +
                    "  if (txt.toLowerCase() !== target) continue;\n" +
                    "  var cur = n;\n" +
                    "  for (var d = 0; d < 6 && cur; d++) {\n" +
                    "    if (isVisible(cur)) {\n" +
                    "      var rowText = (cur.innerText || '').toLowerCase();\n" +
                    "      if (rowText.indexOf('see all') < 0 && clickEl(cur)) return true;\n" +
                    "    }\n" +
                    "    cur = cur.parentElement;\n" +
                    "  }\n" +
                    "  if (clickEl(n)) return true;\n" +
                    "}\n" +
                    "return false;";
                var clicked = js.ExecuteScript(script, profileName);

                if (clicked is bool && (bool)clicked)
                {
                    Thread.Sleep(1800);
                    return true;
                }
            }
            catch { }

            return false;
        }

        private bool ClickNearestProfileRow(IWebElement el)
        {
            if (el == null) return false;

            try
            {
                var candidates = new List<IWebElement>();

                try
                {
                    var c1 = el.FindElement(By.XPath("./ancestor::div[@role='button'][1]"));
                    if (c1 != null) candidates.Add(c1);
                }
                catch { }

                try
                {
                    var c2 = el.FindElement(By.XPath("./ancestor::div[.//img][1]"));
                    if (c2 != null) candidates.Add(c2);
                }
                catch { }

                candidates.Add(el);

                foreach (var c in candidates)
                {
                    try
                    {
                        if (!c.Displayed) continue;
                        SafeClickScroll(c);
                        Thread.Sleep(1500);
                        return true;
                    }
                    catch
                    {
                        try
                        {
                            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", c);
                            Thread.Sleep(1500);
                            return true;
                        }
                        catch { }
                    }
                }
            }
            catch { }

            return false;
        }

        private string EscapeXPathLiteral(string value)
        {
            // Minimal escape for existing usage; avoid double quotes breaking XPath strings.
            return (value ?? "").Replace("\"", "");
        }

        private void RememberMainProfileUrl()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(mainProfileUrl))
                    return;

                Log("Capturing main profile URL...");

                SafeGo("https://www.facebook.com/me", 1000);
                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(1500);

                var url = SafeGetUrl();
                if (string.IsNullOrWhiteSpace(url) || IsBlockedUrl(url))
                {
                    Log("Capture main profile URL failed (blocked/empty).");
                    return;
                }

                if (url.IndexOf("facebook.com", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    mainProfileUrl = url;
                    Log("Captured main profile URL: " + mainProfileUrl);
                }
            }
            catch (Exception ex)
            {
                Log("RememberMainProfileUrl error: " + ex.Message);
            }
        }

        private void TryCloseAnyDialog()
        {
            try
            {
                var close = FindFirstDisplayed(new[]
                {
                    By.XPath("//div[@role='dialog']//div[@aria-label='Close']"),
                    By.XPath("//div[@role='dialog']//div[@role='button' and @aria-label='Close']"),
                    By.XPath("//div[@role='dialog']//div[@role='button' and .//*[name()='svg']]")
                }, 1);

                if (close != null) SafeClick(close);
            }
            catch { }
        }

        // =========================
        // Reel flow (Edit reel -> Next -> Reel settings -> Post)
        // =========================
        private bool IsEditReelScreen()
        {
            try
            {
                return driver.FindElements(By.XPath(
                    "//div[normalize-space(text())='Edit reel']"
                )).Any();
            }
            catch { return false; }
        }


        private bool ClickReelNext()
        {
            // Wait until Edit reel header exists
            if (!WaitExists(By.XPath("//div[normalize-space(text())='Edit reel']"), 15))
                return false;

            for (int i = 0; i < 20 && !IsStop(); i++)
            {
                try
                {
                    var nextButtons = driver.FindElements(By.XPath(
                        "//div[@role='button' and (.//span[normalize-space(text())='Next'] or normalize-space(text())='Next')]"
                    )).ToList();

                    foreach (var btn in nextButtons)
                    {
                        try
                        {
                            if (!btn.Displayed) continue;

                            // 🔴 VERY IMPORTANT: skip disabled Next
                            var ariaDisabled = btn.GetAttribute("aria-disabled");
                            if (!string.IsNullOrEmpty(ariaDisabled) &&
                                ariaDisabled.ToLower() == "true")
                                continue;

                            // scroll into view
                            ((IJavaScriptExecutor)driver)
                                .ExecuteScript("arguments[0].scrollIntoView({block:'center'});", btn);

                            Thread.Sleep(300);

                            // click
                            try
                            {
                                btn.Click();
                            }
                            catch
                            {
                                ((IJavaScriptExecutor)driver)
                                    .ExecuteScript("arguments[0].click();", btn);
                            }

                            Thread.Sleep(2000);
                            return true;
                        }
                        catch { }
                    }
                }
                catch { }

                // Wait for FB to finish processing video
                Thread.Sleep(1000);
            }

            return false;
        }


        private void TryTypeReelCaption(string caption)
        {
            if (string.IsNullOrWhiteSpace(caption)) return;

            // Reel settings screen has "Describe your reel..."
            for (int i = 0; i < 10 && !IsStop(); i++)
            {
                try
                {
                    var box = FindFirstDisplayed(new[]
                    {
                        By.XPath("//div[@aria-label='Describe your reel...']"),
                        By.XPath("//div[contains(@aria-label,'Describe your reel') ]"),
                        By.XPath("//div[contains(translate(@aria-label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'describe your reel')]"),
                        By.XPath("//textarea[contains(translate(@aria-label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'describe')]")
                    }, 2);

                    if (box != null)
                    {
                        SafeClick(box);
                        Thread.Sleep(200);

                        // contenteditable div often needs Actions
                        try { new Actions(driver).SendKeys(box, caption).Build().Perform(); }
                        catch { try { box.SendKeys(caption); } catch { } }

                        Thread.Sleep(500);
                        Log("Typed reel caption.");
                        return;
                    }
                }
                catch { }

                Thread.Sleep(600);
            }
        }

        private bool ClickReelPost()
        {
            // Reel settings screen bottom button usually "Post" (sometimes "Share now")
            for (int i = 0; i < 14 && !IsStop(); i++)
            {
                try
                {
                    var btn = FindFirstDisplayed(new[]
                    {
                        By.XPath("//div[@aria-label='Post' or @aria-label='Share now' or @aria-label='Share Now']"),
                        By.XPath("//span[normalize-space(text())='Post']/ancestor::div[@role='button'][1]"),
                        By.XPath("//span[normalize-space(text())='Share now']/ancestor::div[@role='button'][1]"),
                        By.XPath("//span[normalize-space(text())='Share Now']/ancestor::div[@role='button'][1]"),
                    }, 2);

                    if (btn != null)
                    {
                        SafeClick(btn);
                        Thread.Sleep(1500);
                        return true;
                    }
                }
                catch { }

                Thread.Sleep(700);
            }
            return false;
        }

        // =========================
        // Existing methods (unchanged from your file)
        // =========================
        public void CreateReel(bool isNoSwitchPage = false)
        {
            // keep your existing implementation
            // (you already have StartCreateReel which works for reel create page)
        }

        public void Backup()
        {
            try
            {
                var graph = new FBGraph();
                graph.GetCookieContainerFromDriver(driver);

                data.PageIds = graph.GetBackupPage(data.Token).Trim();
                if (!string.IsNullOrEmpty(data.PageIds))
                {
                    string[] pageArr = data.PageIds.Split(',');
                    data.TotalPage = pageArr.Length;
                }
                else data.TotalPage = 0;

                this.form.fbAccountViewModel.getAccountDao().UpdatePage(data.UID, data.TotalPage, data.PageIds);
            }
            catch { }
        }

        public void Create()
        {
            if (processActionData?.PageConfig?.CreatePage == null) return;

            data.Description = "Create Page";
            for (int j = 1; j <= processActionData.PageConfig.CreatePage.CreateNumber; j++)
            {
                if (IsStop()) break;
                CreateTab();

                var name = GetName();
                var category = GetCategory();
                var bio = GetBio();

                WebFBTool.CreatePage(driver, name, category, bio);
                Thread.Sleep(6000);
            }

            try
            {
                driver.SwitchTo().Window(driver.WindowHandles[0]);
                Thread.Sleep(500);
            }
            catch { }
        }

        public void Follow()
        {
            data.Description = "Follow Page";
            if (pageUrlArr == null) return;

            for (int i = 0; i < pageUrlArr.Length; i++)
            {
                if (IsStop()) break;

                var url = "";
                try { url = pageUrlArr[i]; } catch { }
                if (string.IsNullOrEmpty(url)) continue;

                FBTool.GoToFacebook(driver, url);
                WebFBTool.LikePage(driver);
                Thread.Sleep(1000);
            }
        }

        public void AutoScroll()
        {
            data.Description = "Page Auto Scroll";
            try { this.form.SetGridDataRowStatus(this.data); } catch { }

            var autoCfg = processActionData?.PageConfig?.AutoScroll;
            var autoReact = autoCfg?.React;
            string[] autoCommentLines = Array.Empty<string>();
            try
            {
                autoCommentLines = (autoCfg?.Comments ?? "")
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();
            }
            catch { }

            var targets = new List<string>();

            try
            {
                if (pageUrlArr != null)
                {
                    targets.AddRange(pageUrlArr
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x.Trim()));
                }
            }
            catch { }

            // Fallback to backed-up page ids when PageUrls config is empty.
            if (targets.Count == 0)
            {
                try
                {
                    var pageIds = (data?.PageIds ?? "")
                        .Split(new[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => NormalizePageId(x))
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct()
                        .ToArray();

                    foreach (var id in pageIds)
                    {
                        targets.Add("https://www.facebook.com/" + id);
                    }
                }
                catch { }
            }

            if (targets.Count == 0)
            {
                // No configured pages: keep scrolling current page until user stops.
                while (!IsStop())
                {
                    try
                    {
                        PageScrollCurrent(autoReact, autoCommentLines);
                    }
                    catch { }
                    Thread.Sleep(random.Next(800, 1500));
                }
                return;
            }

            var uniqueTargets = targets
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct()
                .ToList();

            // Open one target page once, then keep scrolling there until user stops.
            // This avoids the "refresh / reopen page and scroll again" behavior.
            var targetUrl = uniqueTargets.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(targetUrl))
            {
                try
                {
                    FBTool.GoToFacebook(driver, targetUrl);
                    FBTool.WaitingPageLoading(driver);
                    Thread.Sleep(1200);
                }
                catch { }
            }

            while (!IsStop())
            {
                try
                {
                    PageScrollCurrent(autoReact, autoCommentLines);
                }
                catch { }
                Thread.Sleep(random.Next(800, 1500));
            }
        }

        private void PageScrollCurrent(React autoReact = null, string[] autoCommentLines = null)
        {
            try
            {
                int rounds = random.Next(5, 11);
                for (int i = 0; i < rounds && !IsStop(); i++)
                {
                    TryInteractOnPageAutoScroll(autoReact, autoCommentLines);
                    try
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollBy(0, arguments[0]);", random.Next(500, 1100));
                    }
                    catch { }
                    Thread.Sleep(random.Next(900, 1800));
                }

                // small up-scroll to look more natural on page timeline
                try
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollBy(0, arguments[0]);", -random.Next(150, 450));
                }
                catch { }
                Thread.Sleep(random.Next(600, 1200));
            }
            catch { }
        }

        private void TryInteractOnPageAutoScroll(React autoReact, string[] autoCommentLines)
        {
            try
            {
                if (autoReact == null) return;

                bool doLike = autoReact.Like;
                bool doComment = autoReact.Comment;

                if (autoReact.Random)
                {
                    doLike = random.Next(0, 2) == 1;
                    doComment = random.Next(0, 2) == 1;
                }

                if (!doLike && !doComment) return;

                // Don’t try to interact on every scroll tick.
                if (random.Next(0, 100) > 40) return;

                if (doLike)
                {
                    try { WebFBTool.LikePost(driver); } catch { }
                    Thread.Sleep(random.Next(400, 900));
                }

                if (doComment && autoCommentLines != null && autoCommentLines.Length > 0)
                {
                    try
                    {
                        var comment = GetNextPageAutoScrollComment(autoCommentLines);
                        if (!string.IsNullOrWhiteSpace(comment))
                        {
                            WebFBTool.PostComment(driver, comment);
                            Thread.Sleep(random.Next(700, 1200));
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private string GetNextPageAutoScrollComment(string[] comments)
        {
            try
            {
                if (comments == null || comments.Length == 0) return "";
                if (comments.Length == 1) return comments[0];

                const string orderKey = "page:autoscroll:comment_order";
                const string indexKey = "page:autoscroll:comment_index";

                int[] order = null;
                int index = 0;

                try
                {
                    var raw = cacheDao?.Get(orderKey)?.Value?.ToString();
                    if (!string.IsNullOrWhiteSpace(raw))
                    {
                        var parsed = raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x =>
                            {
                                int.TryParse(x, out var n);
                                return n;
                            })
                            .ToArray();

                        if (parsed.Length == comments.Length &&
                            parsed.Distinct().Count() == comments.Length &&
                            parsed.All(x => x >= 0 && x < comments.Length))
                        {
                            order = parsed;
                        }
                    }
                }
                catch { }

                try { int.TryParse(cacheDao?.Get(indexKey)?.Value?.ToString(), out index); } catch { index = 0; }

                int[] Shuffle() => Enumerable.Range(0, comments.Length).OrderBy(_ => random.Next()).ToArray();

                if (order == null || order.Length != comments.Length)
                {
                    order = Shuffle();
                    index = 0;
                }

                if (index < 0 || index >= order.Length)
                {
                    order = Shuffle();
                    index = 0;
                }

                var result = comments[order[index]];

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
                catch { }

                return result;
            }
            catch
            {
                try { return comments[random.Next(0, comments.Length)]; } catch { return ""; }
            }
        }

        // =========================
        // Small helpers
        // =========================
        private List<string> LoadMediaFiles_FileOrFolder(string path)
        {
            var list = new List<string>();
            if (string.IsNullOrWhiteSpace(path)) return list;

            try
            {
                path = path.Trim();

                // user selected a single file
                if (File.Exists(path))
                {
                    if (IsSupportedMedia(path)) list.Add(path);
                    return list;
                }

                // user selected a folder
                if (Directory.Exists(path))
                {
                    list.AddRange(Directory.GetFiles(path).Where(IsSupportedMedia));
                }
            }
            catch { }

            return list;
        }

        private bool IsSupportedMedia(string f)
        {
            if (string.IsNullOrWhiteSpace(f)) return false;
            return f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                   f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                   f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                   f.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                   f.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                   f.EndsWith(".mov", StringComparison.OrdinalIgnoreCase) ||
                   f.EndsWith(".avi", StringComparison.OrdinalIgnoreCase);
        }

        // Allow PageIds field to contain:
        // - plain id: 6157...
        // - username
        // - full url: https://www.facebook.com/profile.php?id=...
        // - full url: https://www.facebook.com/username
        private string NormalizePageId(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            raw = raw.Trim();

            if (raw.Contains("|")) raw = raw.Split('|')[0].Trim();

            if (raw.IndexOf("profile.php", StringComparison.OrdinalIgnoreCase) >= 0 &&
                raw.IndexOf("id=", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                try
                {
                    int idx = raw.IndexOf("id=", StringComparison.OrdinalIgnoreCase);
                    string idPart = raw.Substring(idx + 3);

                    int amp = idPart.IndexOf('&');
                    if (amp >= 0) idPart = idPart.Substring(0, amp);

                    int slash = idPart.IndexOf('/');
                    if (slash >= 0) idPart = idPart.Substring(0, slash);

                    return idPart.Trim();
                }
                catch { return ""; }
            }

            if (raw.IndexOf("facebook.com/", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                try
                {
                    var after = raw.Split(new[] { "facebook.com/" }, StringSplitOptions.None)[1];
                    after = after.Split('?', '&', '/')[0];
                    return after.Trim();
                }
                catch { return ""; }
            }

            return raw;
        }

        private void SafeGo(string url, int sleepMs)
        {
            try { driver.Navigate().GoToUrl(url); } catch { }
            Thread.Sleep(sleepMs);
        }

        private string SafeGetUrl()
        {
            try { return driver.Url ?? ""; } catch { return ""; }
        }

        private bool IsBlockedUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return true;
            url = url.ToLowerInvariant();
            return url.Contains("login") || url.Contains("checkpoint") || url.Contains("recover");
        }

        private bool IsOnTargetPage(string url, string pageId)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            url = url.ToLowerInvariant();
            pageId = (pageId ?? "").ToLowerInvariant();

            if (url.Contains("profile.php?id=" + pageId)) return true;
            if (url.Contains("/" + pageId)) return true;
            if (url.Contains("facebook.com/pages/")) return true;

            return false;
        }

        private IWebElement FindDialog()
        {
            try { return driver.FindElements(By.XPath("//div[@role='dialog']")).FirstOrDefault(); }
            catch { return null; }
        }

        private IWebElement FindFirstDisplayed(IEnumerable<By> byList, int seconds)
        {
            DateTime until = DateTime.Now.AddSeconds(seconds);
            while (DateTime.Now < until && !IsStop())
            {
                foreach (var by in byList)
                {
                    try
                    {
                        var el = driver.FindElements(by)
                            .FirstOrDefault(e =>
                            {
                                try { return e != null && e.Displayed && e.Enabled; }
                                catch { return false; }
                            });
                        if (el != null) return el;
                    }
                    catch { }
                }
                Thread.Sleep(250);
            }
            return null;
        }

        private bool WaitExists(By by, int seconds)
        {
            try
            {
                var wait = new WebDriverWait(new SystemClock(), driver, TimeSpan.FromSeconds(seconds), TimeSpan.FromMilliseconds(250));
                return wait.Until(d => d.FindElements(by).Any());
            }
            catch { return false; }
        }

        private bool WaitAny(By[] byList, int seconds)
        {
            DateTime until = DateTime.Now.AddSeconds(seconds);
            while (DateTime.Now < until && !IsStop())
            {
                foreach (var by in byList)
                {
                    try { if (driver.FindElements(by).Any()) return true; }
                    catch { }
                }
                Thread.Sleep(250);
            }
            return false;
        }

        private void SafeClick(IWebElement el)
        {
            if (el == null) return;
            try
            {
                // normal click
                el.Click();
                return;
            }
            catch { }

            try
            {
                // JS click fallback (FB sometimes blocks normal click)
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", el);
            }
            catch { }
        }

        private void TryShot(string path)
        {
            try { ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(path); }
            catch { }
        }

        private void Log(string msg)
        {
            try
            {
                string line = $"[{DateTime.Now:HH:mm:ss}] {msg}";
                System.Diagnostics.Debug.WriteLine(line);
                File.AppendAllText(@"C:\debug.txt", line + "\r\n");

                if (this.data != null && this.form != null)
                {
                    this.data.Description = msg;
                    this.form.SetGridDataRowStatus(this.data);
                }
            }
            catch { }
        }

        public void CreateTab()
        {
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("window.open();", Array.Empty<object>());
                Thread.Sleep(500);
                driver.SwitchTo().Window(driver.WindowHandles[driver.WindowHandles.Count - 1]);
            }
            catch { }
        }

        public string GetName()
        {
            try
            {
                if (pageNameArr != null && pageNameArr.Length > 0)
                    return pageNameArr[random.Next(0, pageNameArr.Length)];
            }
            catch { }
            return "";
        }

        public string GetCategory()
        {
            try
            {
                if (pageCategoriesArr != null && pageCategoriesArr.Length > 0)
                    return pageCategoriesArr[random.Next(0, pageCategoriesArr.Length)];
            }
            catch { }
            return "";
        }

        public string GetBio()
        {
            try
            {
                if (pageBioArr != null && pageBioArr.Length > 0)
                    return pageBioArr[random.Next(0, pageBioArr.Length)];
            }
            catch { }
            return "";
        }

        public string GetUrl()
        {
            string str = "";
            try
            {
                if (pageUrlArr != null && pageUrlIndex < pageUrlArr.Length)
                    str = pageUrlArr[pageUrlIndex];
                pageUrlIndex++;
            }
            catch { }
            return str;
        }

        public bool IsStop() => this.form != null && this.form.IsStop();
    }
}
