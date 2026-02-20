using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolKHBrowser.Helper
{
    public static class SeleniumX
    {
        public static WebDriverWait Wait(IWebDriver driver, int seconds = 15)
            => new WebDriverWait(new SystemClock(), driver, TimeSpan.FromSeconds(seconds), TimeSpan.FromMilliseconds(200));

        public static bool Exists(IWebDriver driver, By by, int seconds = 2)
        {
            try
            {
                Wait(driver, seconds).Until(d =>
                {
                    var els = d.FindElements(by);
                    return els != null && els.Count > 0;
                });
                return true;
            }
            catch { return false; }
        }

        public static IWebElement WaitVisible(IWebDriver driver, By by, int seconds = 15)
            => Wait(driver, seconds).Until(ExpectedConditions.ElementIsVisible(by));

        public static IWebElement WaitClickable(IWebDriver driver, By by, int seconds = 15)
            => Wait(driver, seconds).Until(ExpectedConditions.ElementToBeClickable(by));

        public static bool SafeClick(IWebDriver driver, By by, int seconds = 10)
        {
            try
            {
                var el = WaitClickable(driver, by, seconds);
                try { el.Click(); }
                catch
                {
                    // fallback JS click
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", el);
                }
                return true;
            }
            catch { return false; }
        }

        // Click a button/span/div by visible text (Facebook changes classes a lot, text is more stable)
        public static bool ClickByText(IWebDriver driver, string text, int seconds = 10)
        {
            try
            {
                string xp =
                    "//*[self::button or self::a or self::div or self::span]" +
                    "[normalize-space(.)='" + EscapeXPath(text) + "']";

                var el = Wait(driver, seconds).Until(d =>
                {
                    var found = d.FindElements(By.XPath(xp)).FirstOrDefault(e => e.Displayed && e.Enabled);
                    return found;
                });

                if (el == null) return false;

                try { el.Click(); }
                catch { ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", el); }
                return true;
            }
            catch { return false; }
        }

        private static string EscapeXPath(string s)
        {
            // minimal escape for single quotes
            if (!s.Contains("'")) return s;
            // concat('a',"'",'b')
            var parts = s.Split('\'');
            return "concat(" + string.Join(",\"'\",", parts.Select(p => "'" + p + "'")) + ")";
        }
    }
}
