using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ToolKHBrowser.ToolLib.Data;
using ToolKHBrowser.ToolLib.Tool;
using ToolLib.Data;
using ToolLib.Tool;
using WpfUI;
using WpfUI.ToolLib.Data;
using WpfUI.ToolLib.Tool;
using WpfUI.ViewModels;
using WpfUI.Views;

namespace ToolKHBrowser.ViewModels
{
    public interface IActiveViewModel
    {

    }
    public class ActiveViewModel : IActiveViewModel
    {
        public ActiveViewModel(IAccountDao accountDao, ICacheDao cacheDao)
        {
            //this.accountDao = accountDao;
            //this.cacheDao = cacheDao;
        }
    }
}
