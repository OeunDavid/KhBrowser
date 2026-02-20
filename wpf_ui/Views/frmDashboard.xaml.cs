using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ToolKHBrowser.ViewModels;

namespace ToolKHBrowser.Views
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class frmDashboard : UserControl
    {
        //private IActivityLogsViewModel activityLogsViewModel;
        //private IPhoneViewModel phoneViewModel;
        private IFbAccountViewModel fbAccountViewModel;
        private ICacheViewModel cacheViewModel;
        public frmDashboard()
        {
            InitializeComponent();

            //activityLogsViewModel = DIConfig.Get<IActivityLogsViewModel>();
            //phoneViewModel = DIConfig.Get<IPhoneViewModel>();
            fbAccountViewModel = DIConfig.Get<IFbAccountViewModel>();
            cacheViewModel = DIConfig.Get<ICacheViewModel>();

            renderData();
        }
        public void renderData()
        {
            var cacheTimeline = cacheViewModel.GetCacheDao().Get("share:shareToTimeline");
            var cacheGroup = cacheViewModel.GetCacheDao().Get("share:shareToGroup");

            string totalShareTimeline = (cacheTimeline?.Total ?? 0).ToString("#,###");
            string totalShareGroup = (cacheGroup?.Total ?? 0).ToString("#,###");
            string totalAccountLive = fbAccountViewModel.getAccountDao().getTotalAccountLive().ToString("#,###");
            string totalAccountDie = fbAccountViewModel.getAccountDao().getTotalAccountDie().ToString("#,###");
            
            if(string.IsNullOrEmpty(totalShareTimeline))
            {
                totalShareTimeline = "0";
            }
            if (string.IsNullOrEmpty(totalShareGroup))
            {
                totalShareGroup = "0";
            }
            if (string.IsNullOrEmpty(totalAccountLive))
            {
                totalAccountLive = "0";
            }
            if (string.IsNullOrEmpty(totalAccountDie))
            {
                totalAccountDie = "0";
            }

            lblShareTimeline.Text = totalShareTimeline;
            lblShareGroup.Text = totalShareGroup;
            lblTotalAccountLive.Text = totalAccountLive;
            lblTotalAccountDie.Text = totalAccountDie;
        }
    }
}
