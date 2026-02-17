using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolKHBrowser.ViewModels;
using WpfUI;
using WpfUI.ViewModels;

namespace ToolKHBrowser.ToolLib.Tool
{
    public static class PHelper
    {
        static ICacheViewModel cacheViewModel = DIConfig.Get<ICacheViewModel>();
        public static Sharer GetShareConfig()
        {
            var cache = cacheViewModel.GetCacheDao().Get("share:config");
            if (cache != null && cache.Value != null)
            {
                var str = cache.Value.ToString();
                if (!string.IsNullOrEmpty(str))
                {
                    return JsonConvert.DeserializeObject<Sharer>(str);
                }
            }

            return new Sharer();
        }
    }
}
