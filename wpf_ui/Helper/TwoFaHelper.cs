using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using OtpNet;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using ToolKHBrowser.ViewModels;

namespace ToolKHBrowser.Helper
{
    public static class TwoFaHelper
    {
        // =========================
        // PUBLIC MAIN ENTRY (AUTO)
        // =========================
        // Call this when your bot detects 2FA page.
        // Returns true if code submitted (or approved).
        public static bool AutoSolve2FA(IWebDriver driver, string twoFaSecretOrOtpAuth, int totalTimeoutSeconds = 120)
        {
            if (driver == null) return false;
            if (string.IsNullOrWhiteSpace(twoFaSecretOrOtpAuth)) return false;

            DateTime end = DateTime.UtcNow.AddSeconds(totalTimeoutSeconds);

            // Loop until solved or timeout
            while (DateTime.UtcNow < end)
            {
                // 1) If push approval screen -> try switch to authenticator app
                if (IsPushApprovalScreen(driver))
                {
                    bool switched = GoToAuthenticatorOptionStrong(driver);

                    // If cannot switch, then Selenium cannot approve push.
                    // We keep waiting a bit in case user approves on phone.
                    if (!switched)
                    {
                        if (WaitForLeaving2FAPages(driver, 10))
                            return true;

                        Thread.Sleep(1000);
                        continue;
                    }

                    // after switching, continue loop to detect input screen
                    Thread.Sleep(800);
                    continue;
                }

                // 2) If code input screen -> generate TOTP & fill
                if (IsCodeInputScreenStrong(driver))
                {
                    string code = GenerateCode(twoFaSecretOrOtpAuth);
                    if (string.IsNullOrWhiteSpace(code)) return false;

                    bool filled = FillCodeAndSubmitStrong(driver, code);
                    if (!filled)
                    {
                        Thread.Sleep(800);
                        continue;
                    }

                    // 3) Wait for navigation away from 2FA page
                    if (WaitForLeaving2FAPages(driver, 60))
                        return true;

                    // sometimes FB reloads and asks again
                    Thread.Sleep(1000);
                    continue;
                }

                // 3) Not push + not code input yet -> wait
                Thread.Sleep(500);
            }

            return false;
        }

        // =========================
        // DETECTION
        // =========================
        private static bool IsPushApprovalScreen(IWebDriver driver)
        {
            try
            {
                return driver.PageSource.Contains("Check your notifications on another device");
            }
            catch { return false; }
        }

        public static bool IsCodeInputScreenStrong(IWebDriver driver)
        {
            if (driver == null) return false;

            return Exists(driver, By.XPath("//input[@placeholder='Code']"), 1)
                || Exists(driver, By.XPath("//input[@autocomplete='one-time-code']"), 1)
                || Exists(driver, By.XPath("//input[@name='approvals_code']"), 1)
                || Exists(driver, By.XPath("//input[contains(@name,'code') or contains(@id,'code')]"), 1);
        }

        private static bool IsOn2FAPageByUrl(IWebDriver driver)
        {
            try
            {
                string url = (driver.Url ?? "").ToLower();
                return url.Contains("two_step_verification") || url.Contains("two_factor") || url.Contains("/checkpoint/");
            }
            catch { return false; }
        }

        // Wait until we LEAVE 2FA-related pages (means success / continued)
        public static bool WaitForLeaving2FAPages(IWebDriver driver, int timeoutSeconds = 60)
        {
            if (driver == null) return false;

            DateTime end = DateTime.UtcNow.AddSeconds(timeoutSeconds);
            while (DateTime.UtcNow < end)
            {
                if (!IsOn2FAPageByUrl(driver))
                    return true;

                // also treat "login page" as NOT success
                if (IsOnLoginPage(driver))
                    return false;

                Thread.Sleep(1000);
            }
            return false;
        }

        // =========================
        // SWITCH PUSH -> AUTH APP
        // =========================
        private static bool GoToAuthenticatorOptionStrong(IWebDriver driver)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

                // 1️⃣ Click "Try another way"
                var tryAnother = wait.Until(d =>
                    d.FindElements(By.XPath("//div[@role='button']//span[contains(text(),'Try another way')]"))
                     .FirstOrDefault());

                if (tryAnother != null)
                {
                    tryAnother.Click();
                }

                // 2️⃣ WAIT until Authentication app option appears
                wait.Until(d => d.PageSource.Contains("Authentication app"));

                // 3️⃣ Click Authentication app
                var authApp = driver.FindElements(By.XPath("//div[@role='button']//span[contains(text(),'Authentication app')]"))
                                    .FirstOrDefault();

                if (authApp != null)
                {
                    authApp.Click();
                    return true;
                }
            }
            catch (WebDriverTimeoutException)
            {
                // Option not found
            }
            catch { }

            return false;
        }

        // =========================
        // FILL CODE (STRONG)
        // =========================
        public static bool FillCodeAndSubmitStrong(IWebDriver driver, string code)
        {
            try
            {
                if (driver == null) return false;

                code = new string((code ?? "").Where(char.IsDigit).ToArray());
                if (code.Length < 6) return false;

                IWebElement input = Find2FAInput(driver);
                if (input == null) return false;

                // scroll + focus
                try
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", input);
                    Thread.Sleep(150);
                }
                catch { }

                try { input.Click(); } catch { }

                // clear hard
                try
                {
                    input.SendKeys(Keys.Control + "a");
                    input.SendKeys(Keys.Backspace);
                }
                catch { }

                // type with Actions (more reliable)
                try
                {
                    var act = new Actions(driver);
                    act.Click(input).SendKeys(code).Perform();
                }
                catch
                {
                    try { input.SendKeys(code); } catch { }
                }

                // dispatch events so button becomes enabled
                try
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript(@"
                        const el = arguments[0];
                        el.dispatchEvent(new Event('input', { bubbles: true }));
                        el.dispatchEvent(new Event('change', { bubbles: true }));
                        el.dispatchEvent(new KeyboardEvent('keyup', { bubbles: true }));
                    ", input);
                }
                catch { }

                Thread.Sleep(400);

                // click Continue
                if (JsClickByTextStrong(driver, "Continue"))
                    return true;

                // fallback: click any enabled button in form
                try
                {
                    var btn = driver.FindElements(By.XPath("//form//*[self::button or @role='button'][not(@disabled)]"))
                                    .FirstOrDefault(b => b.Displayed && b.Enabled);
                    if (btn != null)
                    {
                        try { btn.Click(); return true; } catch { }
                        try
                        {
                            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btn);
                            return true;
                        }
                        catch { }
                    }
                }
                catch { }

                return true; // code filled even if click failed (FB sometimes auto-submits)
            }
            catch
            {
                return false;
            }
        }

        private static IWebElement Find2FAInput(IWebDriver driver)
        {
            By[] locators = new[]
            {
                By.XPath("//input[@placeholder='Code']"),
                By.XPath("//input[@autocomplete='one-time-code']"),
                By.XPath("//input[@name='approvals_code']"),
                By.XPath("//input[contains(@name,'code') or contains(@id,'code')]"),
                By.XPath("//form//input[(@type='text' or @type='tel' or @type='number') and not(@disabled)]")
            };

            foreach (var by in locators)
            {
                try
                {
                    var el = WaitVisible(driver, by, 8);
                    if (el != null && el.Displayed && el.Enabled)
                        return el;
                }
                catch { }
            }

            // last fallback: first visible input
            try
            {
                return driver.FindElements(By.TagName("input"))
                             .FirstOrDefault(i => i.Displayed && i.Enabled);
            }
            catch { }

            return null;
        }

        // =========================
        // TOTP GENERATION
        // =========================
        public static string GenerateCode(string secretOrOtpAuth, int digits = 6, int periodSeconds = 30, int driftSteps = 1)
        {
            if (string.IsNullOrWhiteSpace(secretOrOtpAuth))
                return "";

            string secret = ExtractSecret(secretOrOtpAuth);
            if (string.IsNullOrWhiteSpace(secret))
                return "";

            secret = NormalizeBase32(secret);

            try
            {
                byte[] secretBytes = Base32Encoding.ToBytes(secret);
                var totp = new Totp(secretBytes, step: periodSeconds, totpSize: digits, mode: OtpHashMode.Sha1);

                DateTime now = DateTime.UtcNow;

                // current
                string code = totp.ComputeTotp(now);
                if (IsValidDigits(code, digits)) return code;

                // drift
                for (int i = 1; i <= driftSteps; i++)
                {
                    code = totp.ComputeTotp(now.AddSeconds(-periodSeconds * i));
                    if (IsValidDigits(code, digits)) return code;

                    code = totp.ComputeTotp(now.AddSeconds(periodSeconds * i));
                    if (IsValidDigits(code, digits)) return code;
                }

                return totp.ComputeTotp(now);
            }
            catch
            {
                return "";
            }
        }

        private static bool IsValidDigits(string code, int digits)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            if (code.Length != digits) return false;
            return code.All(char.IsDigit);
        }

        private static string ExtractSecret(string input)
        {
            input = (input ?? "").Trim();

            if (input.StartsWith("otpauth://", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(input, @"[?&]secret=([^&]+)", RegexOptions.IgnoreCase);
                if (match.Success)
                    return Uri.UnescapeDataString(match.Groups[1].Value);
                return "";
            }

            return input;
        }

        private static string NormalizeBase32(string secret)
        {
            secret = (secret ?? "").Replace(" ", "").Replace("\r", "").Replace("\n", "").Trim();
            secret = Regex.Replace(secret.ToUpperInvariant(), @"[^A-Z2-7=]", "");
            return secret;
        }

        // =========================
        // SMALL SELENIUM HELPERS (NO SeleniumX dependency)
        // =========================
        private static bool Exists(IWebDriver driver, By by, int timeoutSeconds)
        {
            try
            {
                DateTime end = DateTime.UtcNow.AddSeconds(timeoutSeconds);
                while (DateTime.UtcNow < end)
                {
                    var el = driver.FindElements(by).FirstOrDefault(e => e.Displayed);
                    if (el != null) return true;
                    Thread.Sleep(200);
                }
            }
            catch { }
            return false;
        }

        private static IWebElement WaitVisible(IWebDriver driver, By by, int timeoutSeconds)
        {
            try
            {
                DateTime end = DateTime.UtcNow.AddSeconds(timeoutSeconds);
                while (DateTime.UtcNow < end)
                {
                    var el = driver.FindElements(by).FirstOrDefault(e => e.Displayed);
                    if (el != null) return el;
                    Thread.Sleep(200);
                }
            }
            catch { }
            return null;
        }

        public static bool JsClickByTextStrong(IWebDriver driver, params string[] needles)
        {
            try
            {
                var js = (IJavaScriptExecutor)driver;

                // Pass needles into JS
                var ok = (bool)js.ExecuteScript(@"
                    const needles = arguments[0].map(x => (x||'').toLowerCase());
                    const candidates = Array.from(document.querySelectorAll(
                      'div[role=""button""], button, a, [role=""link""], [role=""menuitem""], label, [role=""radio""]'
                    ));

                    function txt(el){
                      return ((el.innerText || el.textContent || '') + '').trim().toLowerCase();
                    }

                    for (const el of candidates) {
                      const t = txt(el);
                      if (!t) continue;
                      if (needles.some(n => n && t.includes(n))) {
                        try {
                          el.scrollIntoView({block:'center', inline:'center'});
                          el.click();
                          return true;
                        } catch (e) {}
                      }
                    }
                    return false;
                ", needles);

                return ok;
            }
            catch { return false; }
        }

        // =========================
        // LOGIN PAGE CHECK (so you know it failed)
        // =========================
        public static bool IsOnLoginPage(IWebDriver driver)
        {
            try
            {
                bool hasEmail = driver.FindElements(By.Name("email")).Any(e => e.Displayed);
                bool hasPass = driver.FindElements(By.Name("pass")).Any(e => e.Displayed);
                if (hasEmail && hasPass) return true;

                string url = driver.Url ?? "";
                return url.IndexOf("login", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch { return false; }
        }
        public static bool IsWaitingForApproval(IWebDriver driver)
        {
            return IsPushApprovalScreen(driver);
        }
        public static bool ForceSwitchFromPushToAuthenticator(IWebDriver driver, int timeoutSeconds = 20)
        {
            if (driver == null) return false;

            DateTime end = DateTime.UtcNow.AddSeconds(timeoutSeconds);

            while (DateTime.UtcNow < end)
            {
                // already on code screen?
                if (IsCodeInputScreenStrong(driver))
                    return true;

                // if we are on push approval screen, try to switch
                if (IsWaitingForApproval(driver) || IsPushApprovalScreen(driver))
                {
                    // Click "Try another way"
                    SeleniumX.SafeClick(driver,
                        By.XPath("//*[self::button or @role='button' or self::a][contains(.,'Try another way')]"), 3);

                    Thread.Sleep(700);

                    // Choose "Authentication app"
                    // (sometimes it’s inside a modal / list item)
                    SeleniumX.SafeClick(driver,
                        By.XPath("//*[self::div or self::span or self::a or self::button][contains(.,'Authentication app')]"), 3);

                    Thread.Sleep(700);

                    // Click Continue
                    SeleniumX.SafeClick(driver,
                        By.XPath("//*[self::button or @role='button'][contains(.,'Continue')]"), 3);

                    Thread.Sleep(1200);

                    // check again
                    if (IsCodeInputScreenStrong(driver))
                        return true;
                }
                else
                {
                    // not push, not code => just wait a bit
                    Thread.Sleep(500);
                }
            }

            return IsCodeInputScreenStrong(driver);
        }
        public static bool AutoFillTotp(IWebDriver driver, string secretOrOtpAuth)
        {
            if (driver == null) return false;

            string code = GenerateCode(secretOrOtpAuth);
            if (string.IsNullOrWhiteSpace(code)) return false;

            // Use the method that exists in this class
            return FillCodeAndSubmitStrong(driver, code);
        }
        public static bool HandleRememberBrowser_AlwaysConfirm(IWebDriver driver, int tries = 6)
        {
            if (driver == null) return false;

            for (int i = 0; i < tries; i++)
            {
                string url = "";
                try { url = (driver.Url ?? "").ToLowerInvariant(); } catch { }
                if (!url.Contains("remember_browser") && !url.Contains("remember-browser"))
                    return true; // already left the page

                try
                {
                    // A) Direct XPath by visible text (works when Text is readable)
                    var el = driver.FindElements(By.XPath(
                        "//*[@role='button' or self::button][.//span[contains(translate(normalize-space(.),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'always confirm')]" +
                        " or contains(translate(normalize-space(.),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'always confirm')]"
                    )).FirstOrDefault();

                    if (el != null)
                    {
                        ForceClick(driver, el);
                        Thread.Sleep(800);
                        continue;
                    }
                }
                catch { }

                try
                {
                    // B) JS scan innerText/textContent (works when Selenium Text is empty)
                    bool jsClicked = (bool)((IJavaScriptExecutor)driver).ExecuteScript(@"
                const nodes = Array.from(document.querySelectorAll('div[role=""button""],button'));
                const txt = el => ((el.innerText || el.textContent || '')+'').trim().toLowerCase();
                for (const el of nodes) {
                  const t = txt(el);
                  if (t && t.includes('always confirm')) {
                    el.scrollIntoView({block:'center'});
                    el.click();
                    return true;
                  }
                }
                return false;
            ");
                    if (jsClicked)
                    {
                        Thread.Sleep(800);
                        continue;
                    }
                }
                catch { }

                try
                {
                    // C) LAST RESORT: click the SECOND big button (your screenshot shows 2 buttons)
                    var buttons = driver.FindElements(By.XPath("//div[@role='button'] | //button"))
                                        .Where(x => x.Displayed)
                                        .ToList();

                    if (buttons.Count >= 2)
                    {
                        ForceClick(driver, buttons[1]); // 0=Trust this device, 1=Always confirm it's me
                        Thread.Sleep(800);
                        continue;
                    }
                }
                catch { }

                Thread.Sleep(600);
            }

            // Still on remember_browser -> failed
            return false;
        }

        private static void ForceClick(IWebDriver driver, IWebElement el)
        {
            try { ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", el); } catch { }

            try { el.Click(); return; } catch { }

            try { new Actions(driver).MoveToElement(el).Click().Perform(); return; } catch { }

            try { ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", el); } catch { }
        }

        public static void DebugAuthCookies(IWebDriver driver, string tag)
        {
            try
            {
                string url = "";
                try { url = driver.Url ?? ""; } catch { }

                string cUser = "", xs = "";
                try { cUser = driver.Manage().Cookies.GetCookieNamed("c_user")?.Value ?? ""; } catch { }
                try { xs = driver.Manage().Cookies.GetCookieNamed("xs")?.Value ?? ""; } catch { }

                Console.WriteLine($"[{tag}] url={url} c_user={(string.IsNullOrEmpty(cUser) ? "NULL" : cUser)} xs={(string.IsNullOrEmpty(xs) ? "NULL" : "OK")}");
            }
            catch { }
        }
        public static string FinalizeLoginFlow_SafeWait(IWebDriver driver, FbAccount account, int seconds = 240)
        {
            if (driver == null) return "Driver null";

            DateTime end = DateTime.UtcNow.AddSeconds(seconds);

            // Require login page to be stable for a few seconds before deciding "Back to login"
            int loginSeenTicks = 0; // counts consecutive checks where login form exists

            while (DateTime.UtcNow < end)
            {
                string url = "";
                string html = "";

                try { url = (driver.Url ?? "").ToLowerInvariant(); } catch { url = ""; }
                try { html = (driver.PageSource ?? "").ToLowerInvariant(); } catch { html = ""; }

                // ✅ 1) Success if c_user exists
                string cUser = "";
                try { cUser = GetUserIdSafe(driver); } catch { cUser = ""; }
                if (!string.IsNullOrEmpty(cUser))
                    return "success";

                // ✅ 2) Detect 2FA pages (so you can proceed to fill code)
                // URL based
                if (url.Contains("two_factor") || url.Contains("two_step_verification") || url.Contains("/checkpoint/"))
                    return "Need 2FA";

                // HTML based (code input / approvals)
                if (html.Contains("approvals_code") || html.Contains("authentication app") || html.Contains("two-factor")
                    || html.Contains("two-step verification") || html.Contains("enter the code") || html.Contains("6-digit"))
                    return "Need 2FA";

                // ✅ 3) Detect CAPTCHA / human verification (manual required)
                if (html.Contains("captcha") || html.Contains("verify you are human") || html.Contains("select all squares"))
                    return "Captcha/Verify required (manual)";

                // ✅ 4) Back to login (session restarted) — only if it stays like that for a while
                if (HasLoginForm(driver) || url.Contains("/login"))
                {
                    loginSeenTicks++;

                    // If it's stable for ~3 seconds (3 ticks), then it's truly back to login
                    if (loginSeenTicks >= 3)
                        return "Back to login (session not created)";

                    Thread.Sleep(1000);
                    continue;
                }
                else
                {
                    loginSeenTicks = 0; // reset if we left login page
                }

                // ✅ 5) Auth flow page? just wait (do not navigate / refresh)
                if (IsAuthFlow(url, html))
                {
                    Thread.Sleep(1000);
                    continue;
                }

                // Otherwise wait a bit more (FB redirect)
                Thread.Sleep(1000);
            }

            // timeout diagnosis
            try
            {
                if (!string.IsNullOrEmpty(GetUserIdSafe(driver))) return "success";
                if (HasLoginForm(driver)) return "Timeout: back to login";
            }
            catch { }

            return "Login not finalized (timeout)";
        }
        private static string GetUserIdSafe(IWebDriver driver)
        {
            try
            {
                var c = driver.Manage().Cookies.GetCookieNamed("c_user");
                return c?.Value ?? "";
            }
            catch { return ""; }
        }
        public static bool HasLoginForm(IWebDriver driver)
        {
            try
            {
                return driver.FindElements(By.Name("email")).Any()
                    || driver.FindElements(By.Name("pass")).Any()
                    || driver.FindElements(By.CssSelector("input[name='email'], input[name='pass']")).Any();
            }
            catch { return false; }
        }
        private static bool IsAuthFlow(string url, string html)
        {
            if (string.IsNullOrEmpty(url)) url = "";
            if (string.IsNullOrEmpty(html)) html = "";

            // login / checkpoint / 2fa / verification / remember browser
            if (url.Contains("/login")
                || url.Contains("two_factor")
                || url.Contains("two_step_verification")
                || url.Contains("/checkpoint")
                || url.Contains("remember_browser")
                || url.Contains("authentication"))
                return true;

            // page content hints (don’t try to automate, just detect)
            if (html.Contains("check your notifications")
                || html.Contains("waiting for approval")
                || html.Contains("try another way")
                || html.Contains("approvals_code")
                || html.Contains("captcha")
                || html.Contains("verify you are human")
                || html.Contains("select all squares"))
                return true;

            return false;
        }
        public static bool IsCaptchaPage(IWebDriver driver)
        {
            try
            {
                string url = (driver.Url ?? "").ToLowerInvariant();
                if (url.Contains("captcha")) return true;

                // Check for common captcha indicators that are actually VISIBLE
                string[] xpaths = {
                    "//*[contains(text(), 'select all squares')]",
                    "//*[contains(text(), 'verify you are human') or contains(text(), 'Verify you are human')]",
                    "//iframe[contains(@src, 'recaptcha') or contains(@src, 'captcha')]",
                    "//div[contains(@id, 'captcha')]",
                    "//img[contains(@src, 'captcha')]"
                };

                foreach (var xpath in xpaths)
                {
                    try
                    {
                        var els = driver.FindElements(By.XPath(xpath));
                        if (els.Any(e => e.Displayed)) return true;
                    }
                    catch { }
                }

                return false;
            }
            catch { return false; }
        }

    }
}