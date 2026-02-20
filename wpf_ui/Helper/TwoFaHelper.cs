using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OtpNet;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

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
        public static bool IsPushApprovalScreen(IWebDriver driver)
        {
            if (driver == null) return false;

            return Exists(driver, By.XPath("//*[contains(.,'Waiting for approval')]"), 1)
                || Exists(driver, By.XPath("//*[contains(.,'Check your notifications on another device')]"), 1);
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
        public static bool GoToAuthenticatorOptionStrong(IWebDriver driver)
        {
            if (driver == null) return false;

            // Click "Try another way"
            if (!ClickByTextStrong(driver, "Try another way", 10))
                return false;

            Thread.Sleep(800);

            // Choose "Authentication app"
            if (!ClickByTextStrong(driver, "Authentication app", 10))
                return false;

            Thread.Sleep(400);

            // Click Continue
            ClickByTextStrong(driver, "Continue", 10);

            Thread.Sleep(800);
            return true;
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
                if (ClickByTextStrong(driver, "Continue", 8))
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

        private static bool ClickByTextStrong(IWebDriver driver, string text, int timeoutSeconds)
        {
            try
            {
                DateTime end = DateTime.UtcNow.AddSeconds(timeoutSeconds);

                // normalize-space contains works better on FB
                string xp =
                    $"//div[@role='button' and contains(normalize-space(.),'{text}')]" +
                    $"|//button[contains(normalize-space(.),'{text}')]" +
                    $"|//*[@role='button' and contains(normalize-space(.),'{text}')]";

                while (DateTime.UtcNow < end)
                {
                    var el = driver.FindElements(By.XPath(xp)).FirstOrDefault(e => e.Displayed);
                    if (el != null)
                    {
                        try
                        {
                            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", el);
                            Thread.Sleep(150);
                        }
                        catch { }

                        try { el.Click(); return true; } catch { }

                        try
                        {
                            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", el);
                            return true;
                        }
                        catch { }
                    }

                    Thread.Sleep(250);
                }
            }
            catch { }

            return false;
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
    }
}