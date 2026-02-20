using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolKHBrowser.ToolLib.Data;
using ToolLib.Data;

namespace ToolKHBrowser.ViewModels
{
    public interface ICacheViewModel
    {
        ICacheDao GetCacheDao();
    }
    public class CacheViewModel : ICacheViewModel
    {
        private ICacheDao cacheDao;
        public CacheViewModel(ICacheDao cacheDao)
        {
            this.cacheDao = cacheDao;
        }
        public ICacheDao GetCacheDao()
        {
            return this.cacheDao;
        }
    }
}
