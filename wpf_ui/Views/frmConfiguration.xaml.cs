using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using ToolLib;
using WpfUI.ToolLib.Data;
using WpfUI.ViewModels;

namespace WpfUI.Views
{
    /// <summary>
    /// Interaction logic for frmConfiguration.xaml
    /// </summary>
    public partial class frmConfiguration : UserControl
    {
        private ICacheViewModel cacheViewModel;
        public frmConfiguration()
        {
            InitializeComponent();

            cacheViewModel = DIConfig.Get<ICacheViewModel>();

            loadData();
            //renameAPK();
            //splitFile();
        }
        private void loadData()
        {
            var cacheEdge = cacheViewModel.GetCacheDao().Get("config:microsoftEdgeProfile");
            txtMicrosoftEdgeProfile.Text = cacheEdge?.Value?.ToString() ?? "";

            var cacheLicense = cacheViewModel.GetCacheDao().Get("config:licenseKey");
            txtLicenseKey.Text = cacheLicense?.Value?.ToString() ?? "";

            var cacheModem = cacheViewModel.GetCacheDao().Get("config:configModem");
            txtConfigModem.Text = cacheModem?.Value?.ToString() ?? "";

            int showScreen = ConfigData.GetShowScreen();
            if(showScreen==3)
            {
                chbShowScreen3.IsChecked = true;
            } else if(showScreen==2)
            {
                chbShowScreen2.IsChecked = true;
            } else if(showScreen==1)
            {
                chbShowScreen1.IsChecked = true;
            }

            if(txtMicrosoftEdgeProfile.Text=="")
            {
                txtMicrosoftEdgeProfile.Text= Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\Microsoft\\Edge\\User Data";
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
                if(h<=0)
                {
                    h = Int32.Parse(System.Windows.SystemParameters.VirtualScreenHeight.ToString());
                }
                txtScreenHeight.Value = h;
            }
            catch (Exception) { }

            string runType= ConfigData.GetRunType();
            if(runType=="mobile")
            {
                chbDefaultRunTypeMobile.IsChecked = true;
            } else if(runType=="mbasic")
            {
                chbDefaultRunTypeMbasic.IsChecked = true;
            }

            string dataMode = cacheViewModel.GetCacheDao().Get("config:dataMode")?.Value?.ToString() ?? "";
            if (dataMode == "data")
            {
                chbDataModeUseData.IsChecked = true;
            }
            else if (dataMode == "free")
            {
                chbDataModeFree.IsChecked = true;
            }
            string browserType = cacheViewModel.GetCacheDao().Get("config:browserType")?.Value?.ToString() ?? "";
            if (browserType == "chrome")
            {
                chbBrowserChrome.IsChecked = true;
            }
            string deviceType = cacheViewModel.GetCacheDao().Get("config:deviceType")?.Value?.ToString() ?? "";
            if (deviceType == "app")
            {
                chbDeviceTypeApp.IsChecked = true;
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
            cacheViewModel.GetCacheDao().SetTotal("config:showScreen", GetShowScreen());
            cacheViewModel.GetCacheDao().Set("config:runType", GetDefaultRunType());
            cacheViewModel.GetCacheDao().Set("config:dataMode", GetDataMode());
            cacheViewModel.GetCacheDao().Set("config:browserType", GetBrowserType());
            cacheViewModel.GetCacheDao().Set("config:deviceType", GetDeviceType());

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
            if(chbDefaultRunTypeMobile.IsChecked.Value)
            {
                runType = "mobile";
            } else if(chbDefaultRunTypeMbasic.IsChecked.Value)
            {
                runType = "mbasic";
            }

            return runType;
        }
        public int GetShowScreen()
        {
            int n = 6;
            if(chbShowScreen3.IsChecked.Value)
            {
                n = 3;
            } else if(chbShowScreen2.IsChecked.Value)
            {
                n = 2;
            } else if(chbShowScreen1.IsChecked.Value)
            {
                n = 1;
            }

            return n;
        }
        private void btnConfigResetIP_Click(object sender, RoutedEventArgs e)
        {
            Internet.ResetIP();
        }
        private void rename()
        {
            string path = @"C:\Users\LM Tecnology\Desktop\User A 1K-0001-1000\Lenovo";
            DirectoryInfo dInfo = new DirectoryInfo(path);
            DirectoryInfo[] subdirs = dInfo.GetDirectories();

            for (int i = 0; i < subdirs.Length; i++)
            {
                string sub = subdirs[i].ToString();
                if (sub.StartsWith("Honor"))
                {
                    string[] arr = sub.Split(' ');
                    string fName = arr[1] + " Honor";
                    Directory.Move(path + "\\" + sub, path + "\\" + fName);
                }
            }
        }
        private void moveAPP()
        {
            string path = @"C:\Users\LM Tecnology\Desktop\User A 1K-0001-1000\Mix Model\A1-1-500";
            DirectoryInfo dInfo = new DirectoryInfo(path);
            DirectoryInfo[] subdirs = dInfo.GetDirectories();

            for(int i=0;i<subdirs.Length;i++)
            {
                string sub = path+"\\"+subdirs[i].ToString();
                string[] file = Directory.GetFiles(sub);
                for(int j=0;j<file.Length;j++)
                {
                    string fileName = System.IO.Path.GetFileName(file[j]);
                    string sou = sub + "\\" + fileName;
                    string des = @"C:\Users\LM Tecnology\Desktop\User A 1K-0001-1000\Mix Model\B1-1-500\" + fileName;

                    try
                    {
                        File.Move(sou, des);
                    }
                    catch (Exception) { }
                }               
            }
        }
        private void splitFile()
        {
            return;
            string path = @"C:\Users\LM Tecnology\Desktop\User A 1K-0001-1000\new\8";
            string[] file = Directory.GetFiles(path);
            int fileIndex = 0;
            for (int i = 1; i <= 4; i++)
            {
                string folder = path + "\\H #" + i;
                try
                {
                    Directory.CreateDirectory(folder);
                }
                catch (Exception) { }
                for(int j=1;j<=500;)
                {
                    string fileName = System.IO.Path.GetFileName(file[fileIndex]);
                    fileIndex++;
                    if (fileName.Contains(".zip") || fileName.Contains(".ara"))
                    {
                        continue;
                    }
                    j++;
                    string sou = path + "\\"+ fileName;
                    string des = folder + "\\"+ fileName;

                    try
                    {
                        File.Move(sou, des);
                    }
                    catch(Exception) { }
                }
            }
        }
        private void renameAPK()
        {
            string path = @"G:\Phone Farm KH\fblites";// DeviceInfo.FB_LITE_SOURCE;
            string[] apks = Directory.GetFiles(path);
            string des;
            int apk_id = 1;

            foreach (var sou in apks)
            {
                string package = apk_id + "";
                if (apk_id < 10)
                {
                    package = "000" + apk_id;
                }
                else if (apk_id > 9 && apk_id < 100)
                {
                    package = "00" + apk_id;
                }
                else if (apk_id > 99 && apk_id < 1000)
                {
                    package = "0" + apk_id;
                }
                string fileName = "Lite " + package + "-" + apk_id;
                des = path + "/" + fileName + ".apk";
                try
                {
                    File.Move(sou, des);
                    Thread.Sleep(1000);
                    File.Delete(sou);
                }
                catch (Exception) { }

                apk_id++;
            }
        }
    }
}
