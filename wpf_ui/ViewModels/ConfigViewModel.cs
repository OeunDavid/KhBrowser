using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolLib.Data;

namespace ToolKHBrowser.ViewModels
{
    public interface IConfigViewModel
    {
        Boolean useDataMode(string deviceID="");
    }
    public class ConfigViewModel : IConfigViewModel
    {
        private IConfigDao _configDao;
        public ConfigViewModel(IConfigDao configDao)
        {
            _configDao = configDao;
        }
        public Boolean useDataMode(string deviceID = "")
        {
            return true;
        }
    }
}
