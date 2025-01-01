using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using ToolKHBrowser.ToolLib.Tool;
using WpfUI;
using WpfUI.ToolLib.Data;
using WpfUI.ViewModels;

namespace ToolKHBrowser.Views
{
    /// <summary>
    /// Interaction logic for pgSetting.xaml
    /// </summary>
    public partial class pgSetting : Page
    {
        private ICacheViewModel cacheViewModel;
        public pgSetting()
        {
            InitializeComponent();

            cacheViewModel = DIConfig.Get<ICacheViewModel>();

            loadData();
        }
        private void loadData()
        {
            txtMicrosoftEdgeProfile.Text = cacheViewModel.GetCacheDao().Get("config:microsoftEdgeProfile").Value.ToString();
            txtLicenseKey.Text = cacheViewModel.GetCacheDao().Get("config:licenseKey").Value.ToString();
            txtConfigModem.Text = cacheViewModel.GetCacheDao().Get("config:configModem").Value.ToString();

            int showScreen = ConfigData.GetShowScreen();
            if (showScreen == 3)
            {
                chbShowScreen3.IsChecked = true;
            }
            else if (showScreen == 2)
            {
                chbShowScreen2.IsChecked = true;
            }
            else if (showScreen == 1)
            {
                chbShowScreen1.IsChecked = true;
            }

            if (txtMicrosoftEdgeProfile.Text == "")
            {
                txtMicrosoftEdgeProfile.Text = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\Microsoft\\Edge\\User Data";
            }
            try
            {
                int w = cacheViewModel.GetCacheDao().Get("screen:width").Total;
                if (w <= 0)
                {
                    w = Int32.Parse(System.Windows.SystemParameters.VirtualScreenWidth.ToString());
                }
                txtScreenWidth.Value = w;
            }
            catch (Exception) { }
            try
            {
                int h = cacheViewModel.GetCacheDao().Get("screen:height").Total;
                if (h <= 0)
                {
                    h = Int32.Parse(System.Windows.SystemParameters.VirtualScreenHeight.ToString());
                }
                txtScreenHeight.Value = h;
            }
            catch (Exception) { }

            string runType = ConfigData.GetRunType();
            if (runType == "mobile")
            {
                chbDefaultRunTypeMobile.IsChecked = true;
            }
            else if (runType == "mbasic")
            {
                chbDefaultRunTypeMbasic.IsChecked = true;
            }

            string dataMode = cacheViewModel.GetCacheDao().Get("config:dataMode").Value.ToString();
            if (dataMode == "data")
            {
                chbDataModeUseData.IsChecked = true;
            }
            else if (dataMode == "free")
            {
                chbDataModeFree.IsChecked = true;
            }
            string browserType = cacheViewModel.GetCacheDao().Get("config:browserType").Value.ToString();
            if (browserType == "chrome")
            {
                chbBrowserChrome.IsChecked = true;
            }
            string deviceType = cacheViewModel.GetCacheDao().Get("config:deviceType").Value.ToString();
            if (deviceType == "app")
            {
                chbDeviceTypeApp.IsChecked = true;
            }

            string loginType = cacheViewModel.GetCacheDao().Get("config:loginType").Value.ToString();
            if (loginType == "cookie")
            {
                chbLoginTypeByCookie.IsChecked = true;
            }
            string useImage = cacheViewModel.GetCacheDao().Get("config:useImage").Value.ToString();
            if (useImage == "no")
            {
                chbUseImageNo.IsChecked = true;
            }
            string cache = cacheViewModel.GetCacheDao().Get("config:no_clear_cache").Value.ToString();
            if (cache == "True")
            {
                chbAutoClearCacheNo.IsChecked = true;
            }
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void btnConfigSave_Click(object sender, RoutedEventArgs e)
        {
            cacheViewModel.GetCacheDao().Set("config:microsoftEdgeProfile", txtMicrosoftEdgeProfile.Text);
            cacheViewModel.GetCacheDao().Set("config:licenseKey", txtLicenseKey.Text);
            cacheViewModel.GetCacheDao().Set("config:configModem", txtConfigModem.Text);

            cacheViewModel.GetCacheDao().Set("config:no_clear_cache", chbAutoClearCacheNo.IsChecked.Value+"");

            cacheViewModel.GetCacheDao().SetTotal("config:showScreen", GetShowScreen());
            cacheViewModel.GetCacheDao().Set("config:runType", GetDefaultRunType());
            cacheViewModel.GetCacheDao().Set("config:dataMode", GetDataMode());
            cacheViewModel.GetCacheDao().Set("config:browserType", GetBrowserType());
            cacheViewModel.GetCacheDao().Set("config:deviceType", GetDeviceType());

            cacheViewModel.GetCacheDao().Set("config:loginType", GetLoginType());
            cacheViewModel.GetCacheDao().Set("config:useImage", GetUseImage());

            try
            {
                cacheViewModel.GetCacheDao().SetTotal("screen:width", Int32.Parse(txtScreenWidth.Value.ToString()));
            }
            catch (Exception) { }
            try
            {
                cacheViewModel.GetCacheDao().SetTotal("screen:height", Int32.Parse(txtScreenHeight.Value.ToString()));
            }
            catch (Exception) { }

            MessageBox.Show("Your data has been save successfully.");
        }
        public string GetLoginType()
        {
            string loginType = "uid";
            if (chbLoginTypeByCookie.IsChecked.Value)
            {
                loginType = "cookie";
            }

            return loginType;
        }
        public string GetUseImage()
        {
            string userimage = "yes";
            if (chbUseImageNo.IsChecked.Value)
            {
                userimage = "no";
            }

            return userimage;
        }
        public string GetDeviceType()
        {
            string deviceType = "defaul";
            if (chbDeviceTypeApp.IsChecked.Value)
            {
                deviceType = "app";
            }

            return deviceType;
        }
        public string GetBrowserType()
        {
            string browserType = "edge";
            if (chbBrowserChrome.IsChecked.Value)
            {
                browserType = "chrome";
            }

            return browserType;
        }
        public string GetDataMode()
        {
            string runType = "none";
            if (chbDataModeUseData.IsChecked.Value)
            {
                runType = "data";
            }
            else if (chbDataModeFree.IsChecked.Value)
            {
                runType = "free";
            }

            return runType;
        }
        public string GetDefaultRunType()
        {
            string runType = "web";
            if (chbDefaultRunTypeMobile.IsChecked.Value)
            {
                runType = "mobile";
            }
            else if (chbDefaultRunTypeMbasic.IsChecked.Value)
            {
                runType = "mbasic";
            }

            return runType;
        }
        public int GetShowScreen()
        {
            int n = 6;
            if (chbShowScreen3.IsChecked.Value)
            {
                n = 3;
            }
            else if (chbShowScreen2.IsChecked.Value)
            {
                n = 2;
            }
            else if (chbShowScreen1.IsChecked.Value)
            {
                n = 1;
            }

            return n;
        }
        private void btnConfigResetIP_Click(object sender, RoutedEventArgs e)
        {
            Internet.ResetIP();
        }
    }
}
