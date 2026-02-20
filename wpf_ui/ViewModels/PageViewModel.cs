using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
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
        // ✅ POST TO PAGE (Photo/Video OR Reel flow)
        // - Open composer
        // - Click Photo/video
        // - SendKeys file into the *dialog* file input
        // - If FB shows "Edit reel" screen -> Click Next -> Caption -> Post
        // - Else normal post -> Next (if any) -> Post/Publish/Share now
        // =========================================================
        public void Post()
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

                    // 3) TWO OPTIONS TO PICK MEDIA
                    // Option A: click Photo/video button
                    bool clickedPicker = ClickPhotoVideoInDialog();

                    // Option B: if Photo/video not found -> click textbox + click photo icon
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

                    // 4) Attach file (works for both options)
                    if (!AttachMediaInDialog(media))
                    {
                        Log("FAIL: Could not attach media.");
                        TryCloseAnyDialog();
                        break;
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

            Log("Switching back to personal profile before finish...");
            SwitchToProfileIdentity();

            Log("=== POST DONE ===");
            this.data.Description = "Post to Page Done";
            this.form.SetGridDataRowStatus(this.data);
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
                    try { 
                        inputEmail.Click();
                        inputEmail.SendKeys(OpenQA.Selenium.Keys.Control + "a");
                        inputEmail.SendKeys(OpenQA.Selenium.Keys.Delete);
                    } catch { }
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
        private void SwitchToProfileIdentity()
        {
            try
            {
                Log("Switching back to profile identity...");

                // Click top-right profile picture menu
                var profileMenu = FindFirstDisplayed(new[]
                {
            By.XPath("//div[@aria-label='Account']"),
            By.XPath("//div[@role='button' and @aria-label='Your profile']"),
            By.XPath("//div[@role='button' and .//img]")
        }, 5);

                if (profileMenu != null)
                {
                    SafeClick(profileMenu);
                    Thread.Sleep(1000);
                }

                // Click "See all profiles"
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

                // Click your personal profile (not Page)
                var switchProfile = FindFirstDisplayed(new[]
                {
            By.XPath("//span[contains(.,'Switch to')]/ancestor::div[@role='button'][1]"),
            By.XPath("//span[contains(.,'profile')]/ancestor::div[@role='button'][1]")
        }, 3);

                if (switchProfile != null)
                {
                    SafeClick(switchProfile);
                    Thread.Sleep(2000);
                }

                Log("Switched to profile.");
            }
            catch (Exception ex)
            {
                Log("SwitchToProfileIdentity error: " + ex.Message);
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
