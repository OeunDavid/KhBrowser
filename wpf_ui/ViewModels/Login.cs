using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ToolKHBrowser.ViewModels
{
    public class Login
    {
        public List<int> RunTab { get; set; }
        public ObservableCollection<Account> data { get; set; }
        public class Property
        {
            public string BrowserType { get; set; }
            public string LoginType { get; set; }
            public int ResetIP { get; set; }
            public IWebDriver Driver { get; set; }
        }
        public class Account
        {
            public long Id { get; set; }
            public string UID { get; set; }
            public string Password { get; set; }
            public string TwoFA { get; set; }
            public string Proxy { get; set; }
            public string Cookei { get; set; }
        }
    }
}
