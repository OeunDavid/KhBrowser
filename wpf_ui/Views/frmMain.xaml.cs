using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using CsQuery.Engine.PseudoClassSelectors;
using CsQuery.EquationParser.Implementation.Functions;
using Emgu.CV;
using Gu.Wpf.NumericInput;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using ToolKHBrowser.ToolLib.Data;
using ToolKHBrowser.ToolLib.Tool;
using ToolKHBrowser.ViewModels;
using ToolKHBrowser.Views;
using ToolLib;
using ToolLib.Data;
using ToolLib.Tool;
using WpfUI.ToolLib.Data;
using WpfUI.ToolLib.Mail;
using WpfUI.ToolLib.Tool;
using WpfUI.UI;
using WpfUI.ViewModels;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using Application = System.Windows.Application;

namespace WpfUI.Views
{

    public partial class frmMain : UserControl
    {
        public ObservableCollection<FbAccount> fbAccounts;

        public ObservableCollection<Store> storeData;

        public IFbAccountViewModel fbAccountViewModel;

        public IStoreViewModel storeViewModel;

        public ICacheViewModel cacheViewModel;
        public LDPlayerTool ldPlayerTool;

        public bool isFirstLoading;

        public Random random;

        public int tempStoreId;

        public int statusId;

        public int storeId;

        public int storeIndex = 0;

        private int openBrowser;

        public ObservableCollection<FbAccount> accountsCheckLive;

        private bool checkLiveStop;

        public Dictionary<int, FbAccount> dataAccounts;

        public ProcessActions processActionsData;

        public bool isStop;

        public int runningRequest;

        public int accountLastIndex;

        public int running;

        public string[] shareUrlArr;

        public string[] shareCommentArr;

        public string[] shareCaptionArr;

        public string[] profileArr = null;

        public string[] coverArr = null;

        public string[] timelineArr = null;

        public int shareUrlIndex = 0;

        public string[] sourceReelVideoArr = null;

        public int reelVideoIndex = 0;
        public int timelineIndex = 0;

        public string[] reelCaptionArr = null;

        public string[] joinGroupIDArr = null;
        public int joinGroupIDIndex = 0;

        public string[] hotmailListArr = null;
        public int hotmailListIndex = 0;

        public IWebDriver driverYandex;

        public Dictionary<int, YandexVerify> yandexVerifyArr;

        public frmMain()
        {
            InitializeComponent();
            statusId = -1;
            storeId = 0;
            isFirstLoading = true;
            tempStoreId = 0;
            fbAccountViewModel = DIConfig.Get<IFbAccountViewModel>();
            ldPlayerTool = DIConfig.Get<LDPlayerTool>();
            ldPlayerTool.SetStopFunc(() => isStop);
            cacheViewModel = DIConfig.Get<ICacheViewModel>();
            storeViewModel = DIConfig.Get<IStoreViewModel>();
            random = new Random();
            LoadStoreData();
            ObservableCollection<FbGroupDevices> observableCollection = new ObservableCollection<FbGroupDevices>
            {
                new FbGroupDevices
                {
                    Id = 1,
                    Name = "UID|Password"
                },
                new FbGroupDevices
                {
                    Id = 2,
                    Name = "UID|Password|2FA"
                },
                new FbGroupDevices
                {
                    Id = 3,
                    Name = "UID|Password|Cookie"
                },
                new FbGroupDevices
                {
                    Id = 4,
                    Name = "UID|Password|2FA|Cookie"
                }
            };
            ObservableCollection<FbGroupDevices> itemsSource = new ObservableCollection<FbGroupDevices>
            {
                new FbGroupDevices
                {
                    Id = -1,
                    Name = "All"
                },
                new FbGroupDevices
                {
                    Id = 1,
                    Name = "Live"
                },
                new FbGroupDevices
                {
                    Id = 0,
                    Name = "Die"
                }
            };
            ddlStatus.ItemsSource = itemsSource;
            ddlStatus.SelectedIndex = 0;
            isFirstLoading = false;

            //string pathFile = @"D:\\Videos\\Short\\s\\1 (4).mp4";
            //string pathFile1 = @"D:\\Videos\\Short\\s\\tmp\\1 (4).mp4";
            //string fileName = pathFile.Substring(pathFile.LastIndexOf(((char)92)) + 1);
            //string FileNameOnly = fileName.Substring(0, fileName.LastIndexOf("."));
            //MessageBox.Show(FileNameOnly);

            //File.Move(pathFile, pathFile1);
            //Thread.Sleep(2000);
            //File.Delete(pathFile);

            //LocalData.CatchGroupNoPending("12werw3");
            //LocalData.CatchGroupNoPending("323422");
        }

        public void LoadStoreData(int storeId = 0)
        {
            storeData = storeViewModel.listDataForGrid(1);
            isFirstLoading = true;
            ddlStore.ItemsSource = storeData;
            if (storeId == 0)
            {
                ddlStore.SelectedIndex = 0;
                return;
            }
            ddlStore.SelectedValue = storeId;
            isFirstLoading = false;
        }

        public int GetScreen()
        {
            return ConfigData.GetShowScreen();
        }

        public bool IsLoginByCookie()
        {
            return ConfigData.IsLoginByCookie();
        }

        public bool IsUseImage()
        {
            return ConfigData.IsUseImage();
        }

        public string GetDataMode()
        {
            try
            {
                var cache = cacheViewModel.GetCacheDao().Get("config:dataMode");
                if (cache != null && cache.Value != null)
                {
                    return cache.Value.ToString();
                }
            }
            catch (Exception) { }
            return "";
        }

        private T GetCacheConfig<T>(string key) where T : class, new()
        {
            try
            {
                var cache = cacheViewModel.GetCacheDao().Get(key);
                if (cache != null && cache.Value != null)
                {
                    string json = cache.Value.ToString();
                    if (!string.IsNullOrEmpty(json))
                    {
                        return JsonConvert.DeserializeObject<T>(json) ?? new T();
                    }
                }
            }
            catch (Exception) { }
            return new T();
        }

        public void loadDataToGrid()
        {
            string text = txtKeyword.Text;
            bool isTempStore = false;
            if (tempStoreId > 0)
            {
                isTempStore = true;
            }
            text = text.Replace('\n', ',');
            fbAccounts = fbAccountViewModel.fbAccounts(storeId, text, isTempStore, statusId);
            dgAccounts.ItemsSource = fbAccounts;
            try
            {
                lblAccountTotal.Text = fbAccountViewModel.GetTotalAccount().ToString() ?? "";
                lblAccountLive.Text = fbAccountViewModel.GetTotalAccountLive().ToString() ?? "";
                lblAccountDie.Text = fbAccountViewModel.GetTotalAccountDie().ToString() ?? "";
                int totalAccountLive = fbAccountViewModel.getAccountDao().getTotalAccountLive();
                int totalAccountDie = fbAccountViewModel.getAccountDao().getTotalAccountDie();
                lblAllAccountTotal.Text = (totalAccountLive + totalAccountDie).ToString() ?? "";
                lblAllAccountLive.Text = totalAccountLive.ToString() ?? "";
                lblAllAccountDie.Text = totalAccountDie.ToString() ?? "";
            }
            catch (Exception)
            {
            }
        }

        private void txtKeyword_KeyUp(object sender, RoutedEventArgs e)
        {
            if (e.Source is TextBox)
            {
                loadDataToGrid();
            }
        }

        private void ddlStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isFirstLoading)
            {
                statusId = GetStatusId();
                loadDataToGrid();
            }
        }

        private void ddlStore_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            storeId = GetStoreId();
            Store store = storeViewModel.getGroupDevicesDao().Get(storeId);
            tempStoreId = 0;
            if (store.IsTemp == 1)
            {
                tempStoreId = store.Id;
            }
            loadDataToGrid();
        }

        public int GetStoreId()
        {
            int result = 0;
            try
            {
                result = Convert.ToInt32(ddlStore.SelectedValue.ToString());
            }
            catch (Exception)
            {
            }
            return result;
        }

        public int GetStatusId()
        {
            int result = -1;
            try
            {
                result = Convert.ToInt32(ddlStatus.SelectedValue.ToString());
                storeIndex = Convert.ToInt32(ddlStatus.SelectedIndex.ToString());
            }
            catch (Exception)
            {
            }
            return result;
        }

        private void dgAccounts_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            FbAccount fbAccount = (FbAccount)e.Row.DataContext;
            if (fbAccount != null)
            {
                if (fbAccount.Status == "Die")
                {
                    e.Row.Background = new SolidColorBrush(Colors.IndianRed);
                }
                else
                {
                    e.Row.Background = new SolidColorBrush(Colors.Transparent);
                }
                e.Row.BorderBrush = null;
            }
        }

        public void SetGridDataRowStatus(FbAccount data)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                if (dgAccounts.ItemsSource is IList && dgAccounts.ItemContainerGenerator.ContainerFromItem(data) is DataGridRow dataGridRow)
                {
                    if (data.Status == "Die")
                    {
                        dataGridRow.Background = new SolidColorBrush(Colors.IndianRed);
                    }
                    else
                    {
                        dataGridRow.Background = new SolidColorBrush(Colors.Transparent);
                    }
                    dataGridRow.BorderBrush = null;
                }
            });
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void copyUID_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("UID"));
        }

        private void copyPass_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("PASS"));
        }

        private void copyCookie_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("Cookie"));
        }

        private void copyUP_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("UID|PASS"));
        }

        private void copyUP2FA_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("UID|PASS|2FA"));
        }

        private void copyUPCookie_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("UID|PASS|COOKIE"));
        }

        private void copy2FA_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("2FA"));
        }

        private void copyUP2FACookie_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("UID|PASS|2FA|Cookie"));
        }

        private void copyUPMail_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("UID|PASS|Mail"));
        }

        private void copyUP2FAMail_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("UID|PASS|2FA|Mail"));
        }

        private void copyUPCookieMail_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("UID|PASS|Cookie|Mail"));
        }

        private void copyUP2FACookieMail_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("UID|PASS|2FA|Cookie|Mail"));
        }

        private void copyUPDOB_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("UID|PASS|DOB"));
        }

        private void copyUP2FADOB_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("UID|PASS|2FA|DOB"));
        }

        private void copyUPCookieDOB_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("UID|PASS|Cookie|DOB"));
        }

        private void copyUP2FACookieDOB_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("UID|PASS|2FA|Cookie|DOB"));
        }

        private void copyUPMailDOB_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("UID|PASS|Mail|DOB"));
        }

        private void copyUP2FAMailDOB_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("UID|PASS|2FA|Mail|DOB"));
        }

        private void copyUPCookieMailDOB_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("UID|PASS|Cookie|Mail|DOB"));
        }

        private void copyUP2FACookieMailDOB_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("UID|PASS|2FA|Cookie|Mail|DOB"));
        }

        private void copyGroupIDs_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("GROUPIDS"));
        }

        private void copyGroupIDsfromSuggest_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("GROUPIDS|SUGGEST"));
        }

        private void copyUIDGroupIDs_Click(object sender, RoutedEventArgs e)
        {
            CopyClipboard(getCopyData("UID|GROUPIDS"));
        }
        private void CopyClipboard(string str)
        {
            try
            {
                Clipboard.SetText(str);
            }
            catch (Exception) { }
        }
        public string GetProfileSize(string uid)
        {
            string strSize = "";
            string browserKey = ConfigData.GetBrowserKey(uid);
            string dirPath = ConfigData.GetBrowserDataDirectory() + "\\" + browserKey;

            strSize = GetMB(GetDirectorySize2(dirPath));


            return strSize;
        }
        private string GetMB(long bytes)
        {
            return Math.Round((bytes / 1024f) / 1024f,2) + " MB";
        }
        private long GetDirectorySize2(string dirPath)
        {
            if (Directory.Exists(dirPath) == false)
            {
                return 0;
            }

            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            long size = 0;

            // Add file sizes.
            FileInfo[] fis = dirInfo.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }

            // Add subdirectory sizes.
            DirectoryInfo[] dis = dirInfo.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += GetDirectorySize2(di.FullName);
            }

            return size;
        }
        private void CheckSize_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(StartCheckSize);
            thread.Start();
        }

        private void StartCheckSize()
        {
            ObservableCollection<FbAccount> data = null;
            Application.Current.Dispatcher.Invoke(delegate
            {
                data = GetSelectData();
            });
            foreach (FbAccount item in data)
            {

                var size = GetProfileSize(item.UID);

                item.Description = "Size: " + size;
                int status = 0;
                if(item.Status == "Live")
                {
                    status= 1;
                }

                fbAccountViewModel.getAccountDao().updateStatus(item.UID, item.Description,status);
            }
        }
        private void deleteProfile_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want delete account profile?", "Confirm", MessageBoxButton.YesNo);
            if (messageBoxResult != MessageBoxResult.Yes)
            {
                return;
            }
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                FBTool.DeleteProfile(selectedItem.UID);
            }
        }

        private void clearCache_Click(object sender, RoutedEventArgs e)
        {
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                string browserKey = ConfigData.GetBrowserKey(selectedItem.UID);
                try
                {
                    LocalData.ClearCache(browserKey);
                }
                catch (Exception)
                {
                }
            }
        }
        private void btnClearAllCache_Click(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(ClearAllCache);
            t.Start();
        }
        public void ClearAllCache()
        {
            var cache = cacheViewModel.GetCacheDao().Get("config:microsoftEdgeProfile");
            if (cache == null || cache.Value == null) return;

            string dir = cache.Value.ToString();
            string[] folders = System.IO.Directory.GetDirectories(@dir);

            for (int i = 0; i < folders.Count(); i++)
            {
                string folder = folders[i];
                try
                {
                    LocalData.ClearCache(folder);
                }
                catch (Exception) { }
            }
        }

        private void removeAccount_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want delete?", "Confirm", MessageBoxButton.YesNo);
            if (messageBoxResult != MessageBoxResult.Yes)
            {
                return;
            }
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID;
                FBTool.DeleteProfile(selectedItem.UID);
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().delete(text);
                loadDataToGrid();
            }
        }

        private void changeStore_Click(object sender, RoutedEventArgs e)
        {
            pgChangeStore page = new pgChangeStore(this);
            wdDialogForm wdDialogForm2 = UIUtil.Dialog(page, "Change Store");
            wdDialogForm2.ShowDialog();
        }

        private void liveAccount_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID;
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().updateStatus(text, "Move to Live", 1);
                loadDataToGrid();
            }
        }

        private void dieAccount_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID;
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().updateStatus(text, "Move to Die", 0);
                loadDataToGrid();
            }
        }

        private void blockActive_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.Active = "Block";
                selectedItem.IsActive = 0;
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID;
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().active(text, 0);
            }
        }

        private void allowActive_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.Active = "Allow";
                selectedItem.IsActive = 1;
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID;
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().active(text, 1);
            }
        }

        private void blockVerify_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.Verify = "Block";
                selectedItem.IsVerify = 0;
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID;
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().active(text, 0);
            }
        }

        private void allowVerify_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.Verify = "Allow";
                selectedItem.IsVerify = 1;
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID;
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().Verify(text, 1);
            }
        }

        private void block2FA_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                if (selectedItem.TwoFA.Length < 10)
                {
                    selectedItem.TwoFA = "Block";
                    selectedItem.IsTwoFA = 0;
                    if (!string.IsNullOrEmpty(text))
                    {
                        text += ",";
                    }
                    text += selectedItem.UID;
                }
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().IsTwoFA(text, 0);
            }
        }

        private void allow2FA_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                if (selectedItem.TwoFA.Length < 10)
                {
                    selectedItem.TwoFA = "Allow";
                    selectedItem.IsTwoFA = 1;
                    if (!string.IsNullOrEmpty(text))
                    {
                        text += ",";
                    }
                    text += selectedItem.UID;
                }
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().IsTwoFA(text, 1);
            }
        }

        private void blockLogin_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.Login = "Block";
                selectedItem.IsLogin = 0;
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID;
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().UpdateLogin(text, 1);
            }
        }

        private void allowLogin_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.Login = "Allow";
                selectedItem.IsLogin = 1;
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID;
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().UpdateLogin(text, 0);
            }
        }

        private void blockShare_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.Share = "Block";
                selectedItem.IsShare = 0;
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID;
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().share(text, 0);
                loadDataToGrid();
            }
        }

        private void unblockShare_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.Share = "Allow";
                selectedItem.IsShare = 1;
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID;
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().share(text, 1);
                loadDataToGrid();
            }
        }

        private void removeFixedGroup_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.GroupIDs = "";
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID;
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().UpdateGroupIds(text);
            }
        }

        private void blockLeaveGroup_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.Leave = "Block";
                selectedItem.IsLeave = 0;
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID;
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().leaveGroup(text, 0);
                loadDataToGrid();
            }
        }

        private void unblockLeaveGroup_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.Leave = "Allow";
                selectedItem.IsLeave = 1;
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID;
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().leaveGroup(text, 1);
                loadDataToGrid();
            }
        }

        private void removeProxy_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.Proxy = "";
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID;
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().clearProxyByUID(text);
            }
        }

        private void addProxy_Click(object sender, RoutedEventArgs e)
        {
            pgProxy page = new pgProxy(this);
            wdDialogForm wdDialogForm2 = UIUtil.Dialog(page, "Proxy");
            wdDialogForm2.ShowDialog();
        }

        public void AddProxy(string proxyListIds)
        {
            string[] array = proxyListIds.Trim().Split('\n');
            if (array.Length == 0)
            {
                return;
            }
            int num = 0;
            cacheViewModel.GetCacheDao().Set("proxy:list", proxyListIds);
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                string text = "";
                if (num >= array.Length)
                {
                    num = 0;
                }
                try
                {
                    text = array[num].Trim();
                    num++;
                }
                catch (Exception)
                {
                }
                if (!string.IsNullOrEmpty(text))
                {
                    selectedItem.Proxy = text;
                    fbAccountViewModel.getAccountDao().addProxy(selectedItem.UID, text);
                }
            }
        }

        private void removeUserAgent_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.UserAgent = "";
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID;
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().clearUserAgentByUID(text);
            }
        }

        private void addUserAgent_Click(object sender, RoutedEventArgs e)
        {
            pgUserAgent page = new pgUserAgent(this);
            wdDialogForm wdDialogForm2 = UIUtil.Dialog(page, "User Agent");
            wdDialogForm2.ShowDialog();
        }

        public void AddUserAgent(string userAgentListIds)
        {
            string[] array = userAgentListIds.Trim().Split('\n');
            if (array.Length == 0)
            {
                return;
            }
            int num = 0;
            cacheViewModel.GetCacheDao().Set("user_agent:list", userAgentListIds);
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                string text = "";
                if (num >= array.Length)
                {
                    num = 0;
                }
                try
                {
                    text = array[num].Trim();
                    num++;
                }
                catch (Exception)
                {
                }
                if (!string.IsNullOrEmpty(text))
                {
                    selectedItem.UserAgent = text;
                    fbAccountViewModel.getAccountDao().addUserAgent(selectedItem.UID, text);
                }
            }
        }
        private void removeGroup_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.GroupIDs = "";
                selectedItem.TotalGroup = 0;
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID;
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().clearGroupID(text);
            }
        }

        private void addGroup_Click(object sender, RoutedEventArgs e)
        {
            pgGroupsWithoutJoin page = new pgGroupsWithoutJoin(this);
            wdDialogForm wdDialogForm2 = UIUtil.Dialog(page, "Add Group without Join");
            wdDialogForm2.ShowDialog();
        }

        public void AddGroup(string listGroupIDs, int numGroup)
        {
            if(string.IsNullOrEmpty(listGroupIDs))
            {                
                return;
            }
            string[] arrGroup = listGroupIDs.Trim().Split('\n');
            int groupIndex = 0;
            int gLeng = arrGroup.Count();
            if(gLeng < numGroup)
            {
                numGroup = gLeng;
            }
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                string groupIds = "";
                int total = 0,i = 0;
                do
                {
                    i++;
                    if(groupIndex >= gLeng)
                    {
                        groupIndex = 0;
                    }
                    if(!string.IsNullOrEmpty(groupIds))
                    {
                        groupIds += ",";
                    }
                    groupIds += arrGroup[groupIndex].Trim();
                    groupIndex++;
                    total++;
                } while (i < numGroup);

                selectedItem.GroupIDs = groupIds;
                selectedItem.TotalGroup = total;
                fbAccountViewModel.getAccountDao().UpdateGroup(selectedItem.UID,total,groupIds);
            }
        }
        private void removeNote_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.Note = "";
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID;
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().clearNote(text);
            }
        }

        private void addNote_Click(object sender, RoutedEventArgs e)
        {
            pgNote page = new pgNote(this);
            wdDialogForm wdDialogForm2 = UIUtil.Dialog(page, "Note");
            wdDialogForm2.ShowDialog();
        }

        public void AddNote(string note)
        {
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.Note = note;
                fbAccountViewModel.getAccountDao().addNote(selectedItem.UID, note);
            }
        }
        private void removeReelSourceVideo_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.ReelSourceVideo = "";
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID;
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().clearReelSourceVideo(text);
            }
        }
        private void reelCountVideos_Click(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(ReelCountVideo);
            t.Start();
        }
        private void ReelCountVideo()
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                foreach (FbAccount data in dgAccounts.SelectedItems)
                {
                    string mes = "Reel Video: ";
                    int total = 0;
                    if (!string.IsNullOrEmpty(data.ReelSourceVideo))
                    {
                        if (Directory.Exists(data.ReelSourceVideo))
                        {
                            string[] arr = LocalData.GetFiles(data.ReelSourceVideo);
                            if (arr != null)
                            {
                                total = arr.Length;
                            }
                        }
                    }
                    data.Description = mes + total;
                    int active = 0;
                    if (data.Status == "Live")
                    {
                        active = 1;
                    }

                    fbAccountViewModel.getAccountDao().updateStatus(data.UID, data.Description, active);
                }
            });
        }
        private void addTimelineSource_Click(object sender, RoutedEventArgs e)
        {
            pgTimelineSource page = new pgTimelineSource(this);
            wdDialogForm wdDialogForm2 = UIUtil.Dialog(page, "Timeline Source");
            wdDialogForm2.ShowDialog();
        }

        public void AddTimelineSource(string timeline_source)
        {
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.TimelineSource = timeline_source;
                fbAccountViewModel.getAccountDao().addTimelineSource(selectedItem.UID, timeline_source);
            }
        }
        private void removeTimelineSource_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.TimelineSource = "";
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID;
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().clearTimelineSource(text);
            }
        }
        private void addReelSourceVideo_Click(object sender, RoutedEventArgs e)
        {
            pgReelSourceVideo page = new pgReelSourceVideo(this);
            wdDialogForm wdDialogForm2 = UIUtil.Dialog(page, "Reel Source Video");
            wdDialogForm2.ShowDialog();
        }

        public void AddReelSourceVideo(string reel_source_video)
        {
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.ReelSourceVideo = reel_source_video;
                fbAccountViewModel.getAccountDao().addReelSourceVideo(selectedItem.UID, reel_source_video);
            }
        }
        private void removeSuggestion_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.PendingJoin = "";
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID.Trim();
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().updatePendingJoin(text, "");
            }
        }

        private void removeTempStore_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    text += ",";
                }
                text += selectedItem.UID.Trim();
            }
            if (!string.IsNullOrEmpty(text))
            {
                fbAccountViewModel.getAccountDao().tempStore(text, 0);
                loadDataToGrid();
            }
        }

        private string getCopyData(string type)
        {
            string text = "";
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                try
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        text += Environment.NewLine;
                    }
                    switch (type)
                    {
                        case "UID":
                            text += selectedItem.UID;
                            break;
                        case "PASS":
                            text += selectedItem.Password;
                            break;
                        case "2FA":
                            text += selectedItem.TwoFA;
                            break;
                        case "Cookie":
                            text += selectedItem.Cookie;
                            break;
                        case "UID|PASS":
                            text = text + selectedItem.UID + "|" + selectedItem.Password;
                            break;
                        case "UID|PASS|2FA":
                            text = text + selectedItem.UID + "|" + selectedItem.Password + "|" + selectedItem.TwoFA;
                            break;
                        case "UID|PASS|COOKIE":
                            text = text + selectedItem.UID + "|" + selectedItem.Password + "|" + selectedItem.Cookie;
                            break;
                        case "UID|PASS|2FA|Cookie":
                            text = text + selectedItem.UID + "|" + selectedItem.Password + "|" + selectedItem.TwoFA + "|" + selectedItem.Cookie;
                            break;
                        case "UID|PASS|Mail":
                            text = text + selectedItem.UID + "|" + selectedItem.Password + "|" + selectedItem.MailPass;
                            break;
                        case "UID|PASS|2FA|Mail":
                            text = text + selectedItem.UID + "|" + selectedItem.Password + "|" + selectedItem.TwoFA + "|" + selectedItem.MailPass;
                            break;
                        case "UID|PASS|Cookie|Mail":
                            text = text + selectedItem.UID + "|" + selectedItem.Password + "|" + selectedItem.Cookie + "|" + selectedItem.MailPass;
                            break;
                        case "UID|PASS|2FA|Cookie|Mail":
                            text = text + selectedItem.UID + "|" + selectedItem.Password + "|" + selectedItem.TwoFA + "|" + selectedItem.Cookie + "|" + selectedItem.MailPass;
                            break;
                        case "UID|PASS|DOB":
                            text = text + selectedItem.UID + "|" + selectedItem.Password + "|" + selectedItem.DOB;
                            break;
                        case "UID|PASS|2FA|DOB":
                            text = text + selectedItem.UID + "|" + selectedItem.Password + "|" + selectedItem.TwoFA + "|" + selectedItem.DOB;
                            break;
                        case "UID|PASS|Cookie|DOB":
                            text = text + selectedItem.UID + "|" + selectedItem.Password + "|" + selectedItem.Cookie + "|" + selectedItem.DOB;
                            break;
                        case "UID|PASS|2FA|Cookie|DOB":
                            text = text + selectedItem.UID + "|" + selectedItem.Password + "|" + selectedItem.TwoFA + "|" + selectedItem.Cookie + "|" + selectedItem.DOB;
                            break;
                        case "UID|PASS|Mail|DOB":
                            text = text + selectedItem.UID + "|" + selectedItem.Password + "|" + selectedItem.MailPass + "|" + selectedItem.DOB;
                            break;
                        case "UID|PASS|2FA|Mail|DOB":
                            text = text + selectedItem.UID + "|" + selectedItem.Password + "|" + selectedItem.TwoFA + "|" + selectedItem.MailPass + "|" + selectedItem.DOB;
                            break;
                        case "UID|PASS|Cookie|Mail|DOB":
                            text = text + selectedItem.UID + "|" + selectedItem.Password + "|" + selectedItem.Cookie + "|" + selectedItem.MailPass + "|" + selectedItem.DOB;
                            break;
                        case "UID|PASS|2FA|Cookie|Mail|DOB":
                            text = text + selectedItem.UID + "|" + selectedItem.Password + "|" + selectedItem.TwoFA + "|" + selectedItem.Cookie + "|" + selectedItem.MailPass + "|" + selectedItem.DOB;
                            break;
                        case "GROUPIDS":
                            {
                                if (string.IsNullOrEmpty(selectedItem.GroupIDs))
                                {
                                    break;
                                }
                                string[] array2 = selectedItem.GroupIDs.Split(',');
                                for (int j = 0; j < array2.Length; j++)
                                {
                                    if (j > 0)
                                    {
                                        text += Environment.NewLine;
                                    }
                                    text += array2[j];
                                }
                                break;
                            }
                        case "UID|GROUPIDS":
                            text = text + selectedItem.UID + "|" + selectedItem.GroupIDs;
                            break;
                        case "GROUPIDS|SUGGEST":
                            if (string.IsNullOrEmpty(selectedItem.PendingJoin))
                            {
                                break;
                            }
                            string[] array = selectedItem.PendingJoin.Split(',');
                            for (int i = 0; i < array.Length; i++)
                            {
                                if (i > 0)
                                {
                                    text += Environment.NewLine;
                                }
                                text += array[i];
                            }
                            break;
                    }
                }
                catch (Exception) { }
            }
            return text;
        }

        public void clearProfile()
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
                {
                    string browserKey = ConfigData.GetBrowserKey(selectedItem.UID);
                    try
                    {
                        LocalData.DeleteProfile(browserKey);
                    }
                    catch (Exception)
                    {
                    }
                }
            });
        }

        public void clearCache()
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
                {
                    string browserKey = ConfigData.GetBrowserKey(selectedItem.UID);
                    try
                    {
                        LocalData.ClearCache(browserKey);
                    }
                    catch (Exception)
                    {
                    }
                }
            });
        }

        private void open_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<FbAccount> selectData = GetSelectData();
            openBrowser = 0;
            foreach (FbAccount item in selectData)
            {
                openBrowser++;
                item.Key = openBrowser;
                Thread thread = new Thread(OpenBrowser);
                thread.Start(item);
                Thread.Sleep(1000);
            }
        }

        private void checkLive_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(StartCheckLive);
            thread.Start();
        }

        private void StartCheckLive()
        {
            ObservableCollection<FbAccount> data = null;
            Application.Current.Dispatcher.Invoke(delegate
            {
                data = GetSelectData();
            });
            foreach (FbAccount item in data)
            {
                Thread thread = new Thread(CheckLive);
                thread.Start(item);
                Thread.Sleep(2000);
            }
        }

        private void CheckLive(object obj)
        {
            FbAccount fbAccount = (FbAccount)obj;
            if (chbUseLDPlayer.IsChecked.Value)
            {
                ldPlayerTool.Connect();
                ldPlayerTool.OpenApp("com.facebook.katana");
                if (ldPlayerTool.WaitText("Log in", 10))
                {
                    fbAccount.Status = "LDPlayer: Account Logout/Die";
                }
                else
                {
                    fbAccount.Status = "LDPlayer: Account Live";
                }
                SetGridDataRowStatus(fbAccount);
                return;
            }

            bool useImage = IsUseImage();
            bool isLoginByCookie = IsLoginByCookie();
            string browserKey = ConfigData.GetBrowserKey(fbAccount.UID);
            IWebDriver webDriver = new MyWebDriver().GetWebDriver(browserKey, 1, GetScreen(), fbAccount.UserAgent, fbAccount.Proxy, useImage);
            int num = WebFBTool.Login(webDriver, fbAccount, isLoginByCookie, isCloseAllPopup: false);
            switch (num)
            {
                case 1:
                    fbAccount.Status = "Live";
                    fbAccount.Description = "Check Live";
                    break;
                case -1:
                    fbAccount.Status = "Die";
                    num = 0;
                    break;
                case -2:
                    fbAccount.Status = "Die";
                    num = 0;
                    break;
                default:
                    fbAccount.Status = "Die";
                    num = 0;
                    fbAccount.Description = "Unknow Error";
                    break;
            }
            SetGridDataRowStatus(fbAccount);
            fbAccountViewModel.getAccountDao().updateStatus(fbAccount.UID, fbAccount.Description, num);
            FBTool.QuitBrowser(webDriver);
        }

        private void OpenBrowser(object obj)
        {
            FbAccount fbAccount = (FbAccount)obj;
            bool useImage = IsUseImage();
            bool isLoginByCookie = IsLoginByCookie();
            int screen = GetScreen();
            if (fbAccount.Key > screen)
            {
                fbAccount.Key = 1;
            }
            string browserKey = ConfigData.GetBrowserKey(fbAccount.UID);
            IWebDriver webDriver = new MyWebDriver().GetWebDriver(browserKey, fbAccount.Key, screen, fbAccount.UserAgent, fbAccount.Proxy, useImage);
            int num = WebFBTool.Login(webDriver, fbAccount, isLoginByCookie);
            switch (num)
            {
                case 1:
                    fbAccount.Status = "Live";
                    fbAccount.Description = "Open Account";
                    break;
                case -1:
                    fbAccount.Status = "Die";
                    num = 0;
                    break;
                case -2:
                    fbAccount.Status = "Die";
                    num = 0;
                    break;
                default:
                    fbAccount.Status = "Die";
                    num = 0;
                    fbAccount.Description = "Unknow Error";
                    break;
            }
            SetGridDataRowStatus(fbAccount);
            fbAccountViewModel.getAccountDao().updateStatus(fbAccount.UID, fbAccount.Description, num);
        }

        public ObservableCollection<FbAccount> GetSelectData()
        {
            ObservableCollection<FbAccount> observableCollection = new ObservableCollection<FbAccount>();
            int num = 1;
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                selectedItem.Key = num;
                observableCollection.Add(selectedItem);
                num++;
            }
            return observableCollection;
        }

        public void startCheckLive_Click(object sender, RoutedEventArgs e)
        {
            CheckLiveStop();
            accountsCheckLive = new ObservableCollection<FbAccount>();
            foreach (FbAccount selectedItem in dgAccounts.SelectedItems)
            {
                if (checkLiveStop)
                {
                    break;
                }
                accountsCheckLive.Add(selectedItem);
            }
            if (!checkLiveStop)
            {
                Thread thread = new Thread(StartAccountCheckLive);
                thread.Start();
            }
        }

        public void stopCheckLive_Click(object sender, EventArgs e)
        {
            CheckLiveStop(stop: true);
        }

        public void CheckLiveStop(bool stop = false)
        {
            checkLiveStop = stop;
        }

        public void StartAccountCheckLive()
        {
            foreach (FbAccount item in accountsCheckLive)
            {
                if (checkLiveStop)
                {
                    break;
                }
                Thread thread = new Thread(StartThreadCheckLive);
                thread.Start(item);
                Thread.Sleep(300);
            }
            accountsCheckLive = null;
        }

        public void StartThreadCheckLive(object obj)
        {
            FbAccount fbAccount = (FbAccount)obj;
            try
            {
                bool flag = FBTool.CheckLiveUID(fbAccount.UID);
                int status = 0;
                string text = "Die";
                if (flag)
                {
                    status = 1;
                    text = "Live";
                }
                fbAccount.Status = text;
                fbAccount.Description = "Check Live (" + text + ")";
                SetGridDataRowStatus(fbAccount);
                fbAccountViewModel.getAccountDao().updateStatus(fbAccount.UID, fbAccount.Description, status);
            }
            catch (Exception)
            {
            }
        }

        private void btnAddNewAccount_Click(object sender, RoutedEventArgs e)
        {
            pgNewAccount page = new pgNewAccount(this, storeId);
            wdDialogForm wdDialogForm2 = UIUtil.Dialog(page, "New Accounts");
            wdDialogForm2.ShowDialog();
        }

        private void btnStore_Click(object sender, RoutedEventArgs e)
        {
            pgStore page = new pgStore(this, storeId);
            wdDialogForm wdDialogForm2 = UIUtil.Dialog(page, "Store");
            wdDialogForm2.ShowDialog();
        }

        public void chbShareConfig_Click(object sender, RoutedEventArgs e)
        {
            pgShare page = new pgShare();
            wdDialogForm wdDialogForm2 = UIUtil.Dialog(page, "Share Config");
            wdDialogForm2.ShowDialog();
        }

        public void chbGroupConfig_Click(object sender, RoutedEventArgs e)
        {
            pgGroups page = new pgGroups();
            wdDialogForm wdDialogForm2 = UIUtil.Dialog(page, "Group Config");
            wdDialogForm2.ShowDialog();
        }

        public void chbNewsFeedConfig_Click(object sender, RoutedEventArgs e)
        {
            pgNewsFeed page = new pgNewsFeed();
            wdDialogForm wdDialogForm2 = UIUtil.Dialog(page, "News Feed Config");
            wdDialogForm2.ShowDialog();
        }

        public void chbPageConfig_Click(object sender, RoutedEventArgs e)
        {
            pgPages page = new pgPages();
            wdDialogForm wdDialogForm2 = UIUtil.Dialog(page, "Page Config");
            wdDialogForm2.ShowDialog();
        }

        public void chbProfileConfig_Click(object sender, RoutedEventArgs e)
        {
            pgProfile page = new pgProfile();
            wdDialogForm wdDialogForm2 = UIUtil.Dialog(page, "Profile Config");
            wdDialogForm2.ShowDialog();
        }

        public void chbFriendsConfig_Click(object sender, RoutedEventArgs e)
        {
            pgFriends page = new pgFriends();
            wdDialogForm wdDialogForm2 = UIUtil.Dialog(page, "Friends Config");
            wdDialogForm2.ShowDialog();
        }

        public void chbContactConfig_Click(object sender, RoutedEventArgs e)
        {
            pgContact page = new pgContact();
            wdDialogForm wdDialogForm2 = UIUtil.Dialog(page, "Contact Config");
            wdDialogForm2.ShowDialog();
        }
        public void chbRecoveryConfig_Click(object sender, RoutedEventArgs e)
        {
            pgRecovery page = new pgRecovery();
            wdDialogForm wdDialogForm2 = UIUtil.Dialog(page, "Recovery Config");
            wdDialogForm2.ShowDialog();
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            pgSetting page = new pgSetting();
            wdDialogForm wdDialogForm2 = UIUtil.Dialog(page, "Setting Config");
            wdDialogForm2.ShowDialog();
        }

        public Dictionary<int, FbAccount> GetAccounts(int num = 2000000000)
        {
            Dictionary<int, FbAccount> dictionary = new Dictionary<int, FbAccount>();
            int num2 = 1;
            int num3 = 0;

            List<FbAccount> selectedAccounts = new List<FbAccount>();
            if (fbAccounts != null)
            {
                foreach (var acc in fbAccounts)
                {
                    if (acc.IsSelected)
                    {
                        selectedAccounts.Add(acc);
                    }
                }
            }

            if (selectedAccounts.Count == 0)
            {
                foreach (FbAccount item in dgAccounts.SelectedItems)
                {
                    selectedAccounts.Add(item);
                }
            }

            foreach (FbAccount selectedItem in selectedAccounts)
            {
                bool flag = true;
                num--;
                selectedItem.Key = num2;
                dictionary.Add(num3, selectedItem);
                num2++;
                num3++;
                if (num <= 0)
                {
                    break;
                }
            }
            return dictionary;
        }

        private bool isCleanTMP = false, isCleanChrom = false, isCleanEDGE = false;
        public string[] groupWithoutJoinArr = null; 


        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            running = 0;
            runningRequest = 0;
            accountLastIndex = 0;
            shareUrlIndex = 0;
            reelVideoIndex = 0;
            timelineIndex = 0;
            sourceReelVideoArr = null;
            joinGroupIDIndex = 0;
            joinGroupIDArr = GetJoinGroupIDArr();
            dataAccounts = GetAccounts();

            hotmailListIndex = 0;
            hotmailListArr = GetHotmailFromListArr();

            processActionsData = GetProcessActionsData();

            isStop = false;
            isCleanTMP = chbCleanTemp.IsChecked.Value;
            isCleanChrom = chbCleanChrome.IsChecked.Value;
            isCleanEDGE = chbCleanEdge.IsChecked.Value;

            Stop(isStop, false);

            InitShare();

            Thread thread = new Thread(StartProcess);
            thread.Start();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            isStop = true;
            Stop(isStop);
        }

        public void InitShare()
        {
            if (true || processActionsData.IsShareProfilePage || processActionsData.IsShareToGroup || processActionsData.IsShareWebsite || processActionsData.IsShareToTimeline)
            {
                processActionsData.Share = GetCacheConfig<Sharer>("share:config");

                if (processActionsData.Share != null)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(processActionsData.Share.Urls))
                            shareUrlArr = processActionsData.Share.Urls.Split('\n');
                    }
                    catch (Exception) { }
                    try
                    {
                        if (!string.IsNullOrEmpty(processActionsData.Share.Captions))
                            shareCaptionArr = processActionsData.Share.Captions.Split('\n');
                    }
                    catch (Exception) { }
                    try
                    {
                        if (!string.IsNullOrEmpty(processActionsData.Share.Comments))
                            shareCommentArr = processActionsData.Share.Comments.Split('\n');
                    }
                    catch (Exception) { }
                    try
                    {
                        if (processActionsData.Share.ProfilePage != null && !string.IsNullOrEmpty(processActionsData.Share.ProfilePage.GroupIDs))
                            groupWithoutJoinArr = processActionsData.Share.ProfilePage.GroupIDs.Split('\n');
                    }
                    catch (Exception) { }
                }
            }
        }
        public string GetGroupWithoutJoin()
        {
            string groupID = "";
            int index = 0;
            try
            {
                var cache = cacheViewModel.GetCacheDao().Get("share:groupIndex");
                if (cache != null && cache.Value != null)
                {
                    index = Int32.Parse(cache.Value.ToString());
                }
            }
            catch (Exception) { }
            try
            {
                if (index >= groupWithoutJoinArr.Length)
                {
                    index = 0;
                }
                groupID = groupWithoutJoinArr[index];
                index++;

                cacheViewModel.GetCacheDao().Set("share:groupIndex", index + "");
            }
            catch (Exception) { }

            return groupID.ToString().Trim();
        }
        public void Stop(bool isStop = false, bool isCleanPC = true)
        {
            if (isCleanPC)
            {
                CleanPC();
                Thread.Sleep(3000);
            }

            Application.Current.Dispatcher.Invoke(delegate
            {
                try
                {
                    btnStart.IsEnabled = isStop;
                    btnStop.IsEnabled = !isStop;
                }
                catch (Exception)
                {
                }
            });
        }

        public bool IsStop()
        {
            return isStop;
        }

        public void StopWorking(IWebDriver driver = null, string uid = "")
        {
            string browserKey = ConfigData.GetBrowserKey(uid);
            FBTool.QuitBrowser(driver, browserKey, uid);
        }

        public string GetURL()
        {
            string result = "";
            if (shareUrlArr == null)
            {
                return "";
            }
            try
            {
                if (shareUrlIndex >= shareUrlArr.Length)
                {
                    shareUrlIndex = 0;
                }
                result = shareUrlArr[shareUrlIndex];
                shareUrlIndex++;
            }
            catch (Exception)
            {
            }
            return result;
        }

        public string GetComment()
        {
            return GetSingleStringFromArr(shareCommentArr);
        }

        public string GetCaption()
        {
            if (processActionsData.Share.OneContent)
            {
                return processActionsData.Share.Captions;
            }
            return GetSingleStringFromArr(shareCaptionArr);
        }

        public string GetSingleStringFromArr(string[] arr)
        {
            string result = "";
            try
            {
                if (arr == null)
                {
                    return "";
                }
                try
                {
                    int maxValue = arr.Length - 1;
                    result = arr[random.Next(0, maxValue)];
                }
                catch (Exception)
                {
                }
            }
            catch (Exception) { }

            return result;
        }

        public FbAccount GetAccount()
        {
            FbAccount result = null;
            try
            {
                result = dataAccounts[accountLastIndex];
                accountLastIndex++;
            }
            catch (Exception)
            {
            }
            return result;
        }

        public int GetThread()
        {
            try
            {
                return int.Parse(((NumericBox<int>)(object)txtThread).Value.ToString());
            }
            catch (Exception)
            {
            }
            return 1;
        }

        public int GetResetIP()
        {
            try
            {
                return int.Parse(((NumericBox<int>)(object)txtResetIP).Value.ToString());
            }
            catch (Exception)
            {
            }
            return 0;
        }

        public ProcessActions GetProcessActionsData()
        {
            ProcessActions processActions = new ProcessActions();
            processActions.Thread = GetThread();
            processActions.ResetIP = GetResetIP();
            processActions.ContactPrimary = chbPrimary.IsChecked.Value;
            processActions.ContactRemoveInstragram = chbRemoveInstragram.IsChecked.Value;
            processActions.ContactRemovePhone = chbPrimaryRemovePhone.IsChecked.Value;

            processActions.EnglishUS = chbOtherChangeLanguage.IsChecked.Value;
            processActions.LockTime = chbLockTime.IsChecked.Value;
            processActions.Token = chbToken.IsChecked.Value;
            processActions.LogoutAlDevices = chbLogOutAllDevices.IsChecked.Value;
            processActions.AutoUnlockCheckpoint = chbAutoUnlockCheckpoint.IsChecked.Value;
            processActions.UserData = chbUseUserData.IsChecked.Value;
            processActions.PrimaryLocation = chbPrimaryLocation.IsChecked.Value;

            processActions.IsShareToTimeline = chbShareToTimeline.IsChecked.Value;
            processActions.IsShareToGroup = chbShareToGroups.IsChecked.Value;
            processActions.IsShareProfilePage = chbSharePageProfile.IsChecked.Value;
            processActions.IsShareWebsite = chbShareWebsite.IsChecked.Value;

            processActions.IsLeaveGroup = chbGroupLeave.IsChecked.Value;
            processActions.IsJoinGroup = chbGroupJoin.IsChecked.Value;
            processActions.IsViewGroup = chbGroupView.IsChecked.Value;
            processActions.AutoScrollGroup = chbAutoScrollGroup.IsChecked.Value;
            processActions.IsBackupGroup = chbGroupBackup.IsChecked.Value;
            processActions.ReadMessenger = chbNewsFeedReadMessenger.IsChecked.Value;
            processActions.ReadNotification = chbNewsFeedReadNotification.IsChecked.Value;
            processActions.PostTimeline = chbNewsFeedPostTimeline.IsChecked.Value;
            processActions.PlayNewsFeed = chbNewsFeedPlay.IsChecked.Value;
            processActions.AutoScroll = chbAutoScroll.IsChecked.Value;
            processActions.CreatePage = chbPageCreate.IsChecked.Value;
            processActions.FollowPage = chbPageFollow.IsChecked.Value;
            processActions.BackupPage = chbPageBackup.IsChecked.Value;
            processActions.AutoScrollPage = chbAutoScrollPage.IsChecked.Value;
            processActions.PageCreateReel = chbPageCreateReel.IsChecked.Value;
            processActions.ProfileDeleteData = chbProfileDeleteData.IsChecked.Value;
            processActions.ActivtiyLog = chbDeleteActivity.IsChecked.Value;
            processActions.NewInfo = chbProfileNewInfo.IsChecked.Value;

            processActions.CheckIn = chbProfileCheckIn.IsChecked.Value;
            processActions.Marketplace = chbProfileMarketplace.IsChecked.Value;
            processActions.TurnOnPM = chbProfileTurnOnPM.IsChecked.Value;

            processActions.PublicPost = chbProfilePublicPost.IsChecked.Value;
            processActions.TurnOnTwoFA = chbProfileTurnOn2FA.IsChecked.Value;
            processActions.NewPassword = chbProfileNewPassword.IsChecked.Value;
            processActions.GetInfo = chbGetInfo.IsChecked.Value;
            processActions.CreationDate = chbCreationDate.IsChecked.Value;
            processActions.AddFriends = chbFriendsAdd.IsChecked.Value;
            processActions.AcceptFriends = chbFriendsAccept.IsChecked.Value;
            processActions.AddFriendsByUID = chbFriendsByUID.IsChecked.Value;
            processActions.BackupFriends = chbFriendsBackup.IsChecked.Value;

            processActions.RecoveryPhoneNumber = chbRecoveryPhone.IsChecked.Value;

            processActions.OtherConfig = new OtherConfig();
            processActions.OtherConfig.ChangeLanguage = chbOtherChangeLanguage.IsChecked.Value;
            processActions.OtherConfig.WatchTime = chbOtherWatchTime.IsChecked.Value;
            processActions.OtherConfig.ReelPlay = chbOtherReelPlay.IsChecked.Value;
            processActions.EnglishUS = processActions.OtherConfig.ChangeLanguage;

            processActions.IsCheckReelInvite = chbCheckReelInvite.IsChecked.Value;
            processActions.NoSwitchPage = chbNoSwitchPage.IsChecked.Value;
            processActions.IsShareByGraph = chbShareByGraph.IsChecked.Value;
            processActions.IsPageInvite = chbPageInvite.IsChecked.Value;
            processActions.IsPageRemoveAdmin = chbPageRemoveAdmin.IsChecked.Value;


            return processActions;
        }

        //public void StartProcess()
        //{
        //    running = processActionsData.Thread;
        //    if (processActionsData.ResetIP <= running && running != processActionsData.Thread)
        //    {
        //        processActionsData.Thread = running;
        //        Application.Current.Dispatcher.Invoke(delegate
        //        {
        //            try
        //            {
        //                ((NumericBox<int>)(object)txtThread).Value = running;
        //            }
        //            catch (Exception)
        //            {
        //            }
        //        });
        //    }
        //    if (processActionsData.ContactPrimary || processActionsData.ContactRemovePhone || processActionsData.ContactRemoveInstragram)
        //    {
        //        yandexVerifyArr = new Dictionary<int, YandexVerify>();
        //        processActionsData.ContactConfig = GetCacheConfig<ContactConfig>("contact:config");

        //        if (processActionsData.ContactPrimary && processActionsData.ContactConfig.Yandex)
        //        {
        //            YandexLogin(processActionsData.ContactConfig.YandexConfig.Mail, processActionsData.ContactConfig.YandexConfig.Password);
        //            for (int i = 0; i < processActionsData.Thread; i++)
        //            {
        //                yandexVerifyArr.Add(i, new YandexVerify()
        //                {
        //                    Code = "",
        //                    Status = false,
        //                    MailPrimary = ""
        //                });
        //            }

        //            // auto get code from mail.yandex

        //            Thread thread1 = new Thread(GetYandexCodeFromDriver);
        //            thread1.Start();
        //        }
        //    }
        //    for (int i = 0; i < processActionsData.Thread; i++)
        //    {
        //        if (IsStop())
        //        {
        //            break;
        //        }
        //        Thread thread = new Thread(StartRunAccount);
        //        thread.Start();
        //        Thread.Sleep(1000);
        //    }
        //}

        public void StartProcess()
        {
            // ✅ read UI toggle safely on UI thread
            bool autoScroll = false;
            Application.Current.Dispatcher.Invoke(delegate
            {
                try
                {
                    autoScroll = chbAutoScroll.IsChecked.HasValue && chbAutoScroll.IsChecked.Value;
                }
                catch { autoScroll = false; }
            });

            // ✅ store into your process config
            processActionsData.AutoScroll = autoScroll;

            running = processActionsData.Thread;

            if (processActionsData.ResetIP <= running && running != processActionsData.Thread)
            {
                processActionsData.Thread = running;
                Application.Current.Dispatcher.Invoke(delegate
                {
                    try { ((NumericBox<int>)(object)txtThread).Value = running; } catch { }
                });
            }

            if (processActionsData.ContactPrimary || processActionsData.ContactRemovePhone || processActionsData.ContactRemoveInstragram)
            {
                yandexVerifyArr = new Dictionary<int, YandexVerify>();
                processActionsData.ContactConfig = GetCacheConfig<ContactConfig>("contact:config");

                if (processActionsData.ContactPrimary && processActionsData.ContactConfig != null && processActionsData.ContactConfig.Yandex)
                {
                    YandexLogin(processActionsData.ContactConfig.YandexConfig.Mail, processActionsData.ContactConfig.YandexConfig.Password);

                    for (int i = 0; i < processActionsData.Thread; i++)
                    {
                        yandexVerifyArr.Add(i, new YandexVerify()
                        {
                            Code = "",
                            Status = false,
                            MailPrimary = ""
                        });
                    }

                    Thread thread1 = new Thread(GetYandexCodeFromDriver);
                    thread1.Start();
                }
            }

            for (int i = 0; i < processActionsData.Thread; i++)
            {
                if (IsStop()) break;

                Thread thread = new Thread(StartRunAccount);
                thread.IsBackground = true;
                thread.Start();

                Thread.Sleep(1000);
            }
        }


        public void RunOneMoreThread()
        {
            running++;
            Thread thread = new Thread(StartRunAccount);
            thread.Start();
        }

        //public void StartRunAccount()
        //{
        //    FbAccount account = GetAccount();
        //    if (account == null || string.IsNullOrEmpty(account.UID))
        //        return;

        //    // ✅ Read UI values on UI thread (very important)
        //    bool useLDPlayer = false;
        //    bool isNoSwitchPage_UI = false;
        //    bool isCheckReelInvite_UI = false;

        //    Application.Current.Dispatcher.Invoke(() =>
        //    {
        //        useLDPlayer = chbUseLDPlayer.IsChecked == true;

        //        // If you need these later you can also cache them here
        //        isNoSwitchPage_UI = chbNoSwitchPage.IsChecked == true;
        //        isCheckReelInvite_UI = chbCheckReelInvite.IsChecked == true;
        //    });

        //    runningRequest++;
        //    int num = runningRequest;
        //    int screen = GetScreen();

        //    try
        //    {
        //        num %= screen;
        //        if (num == 0) num = screen;
        //    }
        //    catch { }

        //    string text = "";
        //    if (processActionsData.UserData)
        //        text = ConfigData.GetBrowserKey(account.UID);

        //    // ✅ Now safe: use bool, not UI control
        //    if (useLDPlayer)
        //    {
        //        StartRunAccountLDPlayer(account);
        //        return;
        //    }

        //    IWebDriver webDriver = new MyWebDriver()
        //        .GetWebDriver(text, num, screen, account.UserAgent, account.Proxy, IsUseImage());

        //    int num2 = 0;
        //    string text2 = "";

        //    if (processActionsData.RecoveryPhoneNumber)
        //    {
        //        RecoveryPhoneNumber(webDriver, account);
        //        text2 = FBTool.GetResults(webDriver);
        //    }
        //    else
        //    {
        //        bool isLoginByCookie = IsLoginByCookie();
        //        text2 = FBTool.LoggedIn(webDriver, account, isLoginByCookie);
        //    }

        //    if (text2 == "success")
        //    {
        //        num2 = 1;
        //        account.Status = "Live";
        //    }
        //    else
        //    {
        //        account.Status = "Die";
        //    }

        //    // ✅ If SetGridDataRowStatus touches UI, do it on UI thread
        //    Application.Current.Dispatcher.Invoke(() =>
        //    {
        //        SetGridDataRowStatus(account);
        //    });

        //    fbAccountViewModel.getAccountDao().updateStatus(account.UID, text2, num2);
        //    account.Description = text2;

        //    bool flag = false;
        //    if (text2.Contains("Lock 956") && processActionsData.AutoUnlockCheckpoint)
        //    {
        //        string text5 = "";
        //        if (processActionsData.NewPassword)
        //        {
        //            try
        //            {
        //                string text6 = cacheViewModel.GetCacheDao().Get("profile:config").Value.ToString();
        //                ProfileConfig profileConfig = JsonConvert.DeserializeObject<ProfileConfig>(text6);
        //                processActionsData.ProfileConfig = profileConfig;
        //                text5 = processActionsData.ProfileConfig.Password.Value;
        //            }
        //            catch { }
        //        }

        //        if (string.IsNullOrEmpty(text5))
        //            text5 = "ph" + GetRandomString(8);

        //        switch (WebFBTool.UnlockCheckpoint(webDriver, text5))
        //        {
        //            case 1:
        //                account.Password = text5;
        //                fbAccountViewModel.getAccountDao().Password(account.UID, text5);
        //                flag = true;
        //                if (processActionsData.NewPassword) processActionsData.NewPassword = false;
        //                break;

        //            case 2:
        //                flag = true;
        //                break;
        //        }
        //    }

        //    if (num2 == 1 || flag)
        //    {
        //        if (flag)
        //        {
        //            account.Description = "Unlock checkpoint";
        //            fbAccountViewModel.getAccountDao().updateStatus(account.UID, account.Description, 1);
        //            num2 = 1;
        //            account.Status = "Live";

        //            Application.Current.Dispatcher.Invoke(() =>
        //            {
        //                SetGridDataRowStatus(account);
        //            });
        //        }

        //        if (IsStop())
        //        {
        //            StopWorking(webDriver, account.UID);
        //            return;
        //        }

        //        if (!DetectStopProcess(webDriver, account))
        //        {
        //            // ✅ Use cached UI values (no UI access here)
        //            bool isNoSwitchPage = isNoSwitchPage_UI;
        //            bool isCheckReelInvite = isCheckReelInvite_UI;

        //            if (!processActionsData.WorkingOnPage)
        //            {
        //                if (!isNoSwitchPage)
        //                    RemovePageCookie(webDriver);
        //            }
        //            else
        //            {
        //                RemovePageCookie(webDriver);
        //            }

        //            string dataMode = GetDataMode();
        //            FBTool.UseData(webDriver, dataMode);

        //            if (!DetectStopProcess(webDriver, account))
        //            {
        //                string userId = FBTool.GetUserId(webDriver);
        //                string cookie = FBTool.GetCookie(webDriver);
        //                fbAccountViewModel.getAccountDao().updateCookie(account.UID, cookie);

        //                if (text.Contains('@'))
        //                {
        //                    fbAccountViewModel.getAccountDao().updateEmail(account.UID, account.UID);
        //                    fbAccountViewModel.getAccountDao().updateUID(account.UID, userId);
        //                    account.UID = userId;
        //                }
        //                else if (text.Contains("+") || text.StartsWith("0"))
        //                {
        //                    fbAccountViewModel.getAccountDao().updateUID(account.UID, userId);
        //                    account.UID = userId;
        //                }

        //                if (IsStop())
        //                {
        //                    StopWorking(webDriver, account.UID);
        //                    return;
        //                }

        //                WebFBTool.CloseAllPopup(webDriver);

        //                if (!DetectStopProcess(webDriver, account))
        //                {
        //                    ChangeLanguage(webDriver);

        //                    if (!DetectStopProcess(webDriver, account))
        //                    {
        //                        if (IsStop())
        //                        {
        //                            StopWorking(webDriver, account.UID);
        //                            return;
        //                        }

        //                        if (processActionsData.WorkingOnPage)
        //                        {
        //                            string[] pageArr = account.PageIds.Split(',');
        //                            bool isBreak = false;

        //                            for (int i = 0; i < pageArr.Length; i++)
        //                            {
        //                                if (IsStop() || isBreak) break;

        //                                try
        //                                {
        //                                    string[] p = pageArr[i].Split('|');
        //                                    string pageId = p[0].Trim();
        //                                    var isSwitch = WebFBTool.SwitchToProfilePage(webDriver, pageId);
        //                                    if (!isSwitch) continue;
        //                                }
        //                                catch { }

        //                                isNoSwitchPage = true;
        //                                WorkingProcess(webDriver, account, isNoSwitchPage, isCheckReelInvite);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            WorkingProcess(webDriver, account, isNoSwitchPage, isCheckReelInvite);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    FBTool.QuitBrowser(webDriver, text, account.UID);

        //    running--;
        //    if (IsStop()) return;

        //    int resetIP = processActionsData.ResetIP;
        //    if (resetIP > 0 && runningRequest % resetIP == 0)
        //    {
        //        if (running <= 0)
        //        {
        //            Internet.ResetIP();
        //            new Thread(StartProcess).Start();
        //        }
        //    }
        //    else
        //    {
        //        new Thread(RunOneMoreThread).Start();
        //    }
        //}

        public void StartRunAccount()
        {
            FbAccount account = GetAccount();
            if (account == null || string.IsNullOrEmpty(account.UID))
                return;

            bool useLDPlayer = false;
            bool isNoSwitchPage_UI = false;
            bool isCheckReelInvite_UI = false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                useLDPlayer = chbUseLDPlayer.IsChecked == true;
                isNoSwitchPage_UI = chbNoSwitchPage.IsChecked == true;
                isCheckReelInvite_UI = chbCheckReelInvite.IsChecked == true;
            });

            runningRequest++;
            int num = runningRequest;
            int screen = GetScreen();

            try
            {
                num %= screen;
                if (num == 0) num = screen;
            }
            catch { }

            string browserKey = "";
            if (processActionsData.UserData)
                browserKey = ConfigData.GetBrowserKey(account.UID);

            if (useLDPlayer)
            {
                StartRunAccountLDPlayer(account);
                return;
            }

            IWebDriver webDriver = new MyWebDriver()
                .GetWebDriver(browserKey, num, screen, account.UserAgent, account.Proxy, IsUseImage());

            int num2 = 0;
            string text2 = "";

            bool isLoginByCookie = IsLoginByCookie();
            text2 = FBTool.LoggedIn(webDriver, account, isLoginByCookie);

            // ============================
            // ✅ HANDLE 2FA CORRECTLY
            // ============================
            if (text2 == "Need 2FA")
            {
                account.Status = "Need 2FA";
                account.Description = "Waiting for 2FA code...";

                Application.Current.Dispatcher.Invoke(() =>
                {
                    SetGridDataRowStatus(account);
                });

                bool success = FBTool.WaitForLoginSuccess(webDriver, 180);

                if (success)
                {
                    text2 = "success";
                    num2 = 1;
                    account.Status = "Live";
                    account.Description = "Login success after 2FA";
                }
                else
                {
                    text2 = "2FA Timeout";
                    account.Status = "Die";
                    account.Description = "2FA not completed";
                }
            }
            else if (text2 == "success")
            {
                num2 = 1;
                account.Status = "Live";
            }
            else
            {
                account.Status = "Die";
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                SetGridDataRowStatus(account);
            });

            fbAccountViewModel.getAccountDao().updateStatus(account.UID, text2, num2);
            account.Description = text2;

            // ============================
            // STOP HERE IF LOGIN FAILED
            // ============================
            if (num2 != 1)
            {
                FBTool.QuitBrowser(webDriver, browserKey, account.UID);
                running--;
                return;
            }

            // ============================
            // CONTINUE WORKING PROCESS
            // ============================

            if (IsStop())
            {
                StopWorking(webDriver, account.UID);
                return;
            }

            string dataMode = GetDataMode();
            FBTool.UseData(webDriver, dataMode);

            string userId = FBTool.GetUserId(webDriver);
            string cookie = FBTool.GetCookie(webDriver);

            fbAccountViewModel.getAccountDao().updateCookie(account.UID, cookie);

            WebFBTool.CloseAllPopup(webDriver);
            ChangeLanguage(webDriver);

            bool isNoSwitchPage = isNoSwitchPage_UI;
            bool isCheckReelInvite = isCheckReelInvite_UI;

            WorkingProcess(webDriver, account, isNoSwitchPage, isCheckReelInvite);

            FBTool.QuitBrowser(webDriver, browserKey, account.UID);

            running--;
        }



        //public void StartRunAccount()
        //{
        //    FbAccount account = GetAccount();
        //    if (account == null || string.IsNullOrEmpty(account.UID))
        //    {
        //        //Stop(true);
        //        return;
        //    }
        //    runningRequest++;
        //    int num = runningRequest;
        //    int screen = GetScreen();

        //    try
        //    {
        //        num %= screen;
        //        if (num == 0)
        //        {
        //            num = screen;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //    string text = "";
        //    if (processActionsData.UserData)
        //    {
        //        text = ConfigData.GetBrowserKey(account.UID);
        //    }
        //    if (chbUseLDPlayer.IsChecked.Value)
        //    {
        //        StartRunAccountLDPlayer(account);
        //        return;
        //    }

        //    IWebDriver webDriver = new MyWebDriver().GetWebDriver(text, num, screen, account.UserAgent, account.Proxy, IsUseImage());

        //    int num2 = 0;
        //    string text2 = "";
        //    if (processActionsData.RecoveryPhoneNumber)
        //    {
        //        RecoveryPhoneNumber(webDriver, account);

        //        text2 = FBTool.GetResults(webDriver);
        //    }
        //    else
        //    {
        //        bool isLoginByCookie = IsLoginByCookie();
        //        text2 = FBTool.LoggedIn(webDriver, account, isLoginByCookie);
        //    }

        //    string text3 = text2;
        //    string text4 = text3;
        //    if (text4 == "success")
        //    {
        //        num2 = 1;
        //        account.Status = "Live";
        //    }
        //    else
        //    {
        //        account.Status = "Die";
        //    }
        //    //string userId23 = FBTool.GetCookie(webDriver);
        //    //MessageBox.Show(userId23);
        //    SetGridDataRowStatus(account);
        //    fbAccountViewModel.getAccountDao().updateStatus(account.UID, text2, num2);
        //    account.Description = text2;
        //    bool flag = false;
        //    if (text2.Contains("Lock 956") && processActionsData.AutoUnlockCheckpoint)
        //    {
        //        string text5 = "";
        //        if (processActionsData.NewPassword)
        //        {
        //            try
        //            {
        //                string text6 = cacheViewModel.GetCacheDao().Get("profile:config").Value.ToString();
        //                ProfileConfig profileConfig = JsonConvert.DeserializeObject<ProfileConfig>(text6);
        //                processActionsData.ProfileConfig = profileConfig;
        //                text5 = processActionsData.ProfileConfig.Password.Value;
        //            }
        //            catch (Exception)
        //            {
        //            }
        //        }
        //        if (string.IsNullOrEmpty(text5))
        //        {
        //            text5 = "ph" + GetRandomString(8);
        //        }
        //        switch (WebFBTool.UnlockCheckpoint(webDriver, text5))
        //        {
        //            case 1:
        //                account.Password = text5;
        //                fbAccountViewModel.getAccountDao().Password(account.UID, text5);
        //                flag = true;
        //                if (processActionsData.NewPassword)
        //                {
        //                    processActionsData.NewPassword = false;
        //                }
        //                break;
        //            case 2:
        //                flag = true;
        //                break;
        //        }
        //    }
        //    if (num2 == 1 || flag)
        //    {
        //        if (flag)
        //        {
        //            account.Description = "Unlock checkpoint";
        //            fbAccountViewModel.getAccountDao().updateStatus(account.UID, account.Description, 1);
        //            num2 = 1;
        //            account.Status = "Live";
        //            SetGridDataRowStatus(account);
        //        }
        //        if (IsStop())
        //        {
        //            StopWorking(webDriver, account.UID);
        //            return;
        //        }
        //        if (!DetectStopProcess(webDriver, account))
        //        {
        //            bool isNoSwitchPage = false, isCheckReelInvite = false;
        //            Application.Current.Dispatcher.Invoke(delegate
        //            {
        //                try
        //                {
        //                    isNoSwitchPage = chbNoSwitchPage.IsChecked.Value;
        //                    isCheckReelInvite = chbCheckReelInvite.IsChecked.Value;
        //                }
        //                catch (Exception) { }
        //            });
        //            if (!processActionsData.WorkingOnPage)
        //            {
        //                if (!isNoSwitchPage)
        //                {
        //                    RemovePageCookie(webDriver);
        //                }
        //            } else
        //            {
        //                RemovePageCookie(webDriver);
        //            }

        //            string dataMode = GetDataMode();
        //            FBTool.UseData(webDriver, dataMode);
        //            if (!DetectStopProcess(webDriver, account))
        //            {
        //                string userId = FBTool.GetUserId(webDriver);
        //                string cookie = FBTool.GetCookie(webDriver);
        //                fbAccountViewModel.getAccountDao().updateCookie(account.UID, cookie);
        //                if (text.Contains('@'))
        //                {
        //                    fbAccountViewModel.getAccountDao().updateEmail(account.UID, account.UID);
        //                    fbAccountViewModel.getAccountDao().updateUID(account.UID, userId);
        //                    account.UID = userId;
        //                }
        //                else if (text.Contains("+") || text.StartsWith("0"))
        //                {
        //                    fbAccountViewModel.getAccountDao().updateUID(account.UID, userId);
        //                    account.UID = userId;
        //                }
        //                if (IsStop())
        //                {
        //                    StopWorking(webDriver, account.UID);
        //                    return;
        //                }
        //                WebFBTool.CloseAllPopup(webDriver);
        //                if (!DetectStopProcess(webDriver, account))
        //                {
        //                    ChangeLanguage(webDriver);
        //                    if (!DetectStopProcess(webDriver, account))
        //                    {
        //                        if (IsStop())
        //                        {
        //                            StopWorking(webDriver, account.UID);
        //                            return;
        //                        }
        //                        if(processActionsData.WorkingOnPage)
        //                        {
        //                            string[] pageArr = account.PageIds.Split(',');
        //                            bool isBreak = false;
        //                            for (int i = 0; i < pageArr.Length; i++)
        //                            {
        //                                if (IsStop() || isBreak)
        //                                {
        //                                    break;
        //                                }
        //                                try
        //                                {
        //                                    string[] p = pageArr[i].Split('|');

        //                                    string pageId = p[0].Trim();
        //                                    var isSwitch = WebFBTool.SwitchToProfilePage(webDriver, pageId);

        //                                    if (!isSwitch) { continue; }
        //                                }
        //                                catch (Exception) { }

        //                                isNoSwitchPage = true;
        //                                WorkingProcess(webDriver, account, isNoSwitchPage, isCheckReelInvite);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            WorkingProcess(webDriver, account, isNoSwitchPage, isCheckReelInvite);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    FBTool.QuitBrowser(webDriver, text, account.UID);
        //    running--;
        //    if (IsStop())
        //    {
        //        return;
        //    }
        //    int resetIP = processActionsData.ResetIP;
        //    if (resetIP > 0 && runningRequest % resetIP == 0)
        //    {
        //        if (running <= 0)
        //        {
        //            Internet.ResetIP();
        //            Thread thread = new Thread(StartProcess);
        //            thread.Start();
        //        }
        //    }
        //    else
        //    {
        //        Thread thread2 = new Thread(RunOneMoreThread);
        //        thread2.Start();
        //    }
        //}

        public void StartRunAccountLDPlayer(FbAccount account)
        {
            try
            {
                if (IsStop()) return;

                ldPlayerTool.Connect();

                if (!string.IsNullOrEmpty(account.Proxy))
                {
                    account.Status = "LDPlayer: Setting Proxy...";
                    SetGridDataRowStatus(account);
                    ldPlayerTool.SetProxy(account.Proxy);
                }
                else
                {
                    ldPlayerTool.ClearProxy();
                }

                ldPlayerTool.OpenApp("com.facebook.katana");
                account.Status = "LDPlayer Started";
                SetGridDataRowStatus(account);

                if (processActionsData.EnglishUS)
                {
                    account.Status = "LDPlayer: Checking Language...";
                    SetGridDataRowStatus(account);

                    if (!ldPlayerTool.IsEnglish())
                    {
                        account.Status = "LDPlayer: Normalizing Language to English...";
                        SetGridDataRowStatus(account);
                        ldPlayerTool.ChangeLanguage();
                    }
                }

                if (!ldPlayerTool.WaitText("What's on your mind?", 20))
                {
                    if (!ldPlayerTool.IsLoggedIn())
                    {
                        account.Status = "LDPlayer Error: Not Logged In";
                        SetGridDataRowStatus(account);
                    }

                    if (!ldPlayerTool.WaitText("What's on your mind?", 20))
                    {
                        account.Status = "LDPlayer: What's on your mind not found";
                        SetGridDataRowStatus(account);
                        return;
                    }
                }

                // 0. Checkpoint / Lock / Contact
                if (processActionsData.AutoUnlockCheckpoint)
                {
                    account.Status = "LDPlayer: Unlocking Checkpoint...";
                    SetGridDataRowStatus(account);
                    ldPlayerTool.UnlockCheckpoint();
                }

                if (processActionsData.LockTime)
                {
                    account.Status = "LDPlayer: Locking Profile...";
                    SetGridDataRowStatus(account);
                    ldPlayerTool.LockProfile();
                }

                if (processActionsData.ContactPrimary || processActionsData.ContactRemovePhone || processActionsData.ContactRemoveInstragram)
                {
                    account.Status = "LDPlayer: Managing Contact Info...";
                    SetGridDataRowStatus(account);

                    // You pass only phone/instagram to your helper currently
                    ldPlayerTool.RemoveContactInfo(processActionsData.ContactRemovePhone, processActionsData.ContactRemoveInstragram);
                }

                // GetInfo
                if (processActionsData.GetInfo)
                {
                    account.Status = "LDPlayer: Getting Info...";
                    SetGridDataRowStatus(account);

                    ldPlayerTool.GetInfo(account);

                    if (processActionsData.CreationDate)
                    {
                        var cDate = ldPlayerTool.GetCreationDate();
                        account.Description += $", Created: {cDate}";
                    }

                    if (processActionsData.Token)
                    {
                        string token = ldPlayerTool.GetAccessToken();
                        account.Token = token;

                        // NOTE: if your IAccountDao doesn't contain updateToken -> you must add it or rename it
                        fbAccountViewModel.getAccountDao().updateToken(account.UID, account.Token);
                    }
                }

                // Working on Page (Switch page)
                if (processActionsData.WorkingOnPage && !processActionsData.NoSwitchPage)
                {
                    account.Status = "LDPlayer: Switching to Page...";
                    SetGridDataRowStatus(account);

                    // Your PageConfig uses PageUrls (not Follow.Value)
                    var pageVal = processActionsData.PageConfig?.PageUrls;
                    if (string.IsNullOrWhiteSpace(pageVal))
                        pageVal = "My Page";

                    ldPlayerTool.SwitchToPage(pageVal);
                }

                // OtherConfig actions
                if (processActionsData.OtherConfig?.ChangeLanguage ?? false)
                {
                    account.Status = "LDPlayer: Changing Language...";
                    SetGridDataRowStatus(account);
                    ldPlayerTool.ChangeLanguage();
                }

                if (processActionsData.OtherConfig?.WatchTime ?? false)
                {
                    account.Status = "LDPlayer: Watching Videos...";
                    SetGridDataRowStatus(account);
                    ldPlayerTool.WatchTime(5);
                }

                if (processActionsData.OtherConfig?.ReelPlay ?? false)
                {
                    account.Status = "LDPlayer: Playing Reels...";
                    SetGridDataRowStatus(account);
                    ldPlayerTool.ReelPlay(5);
                }

                // Backup groups
                if (processActionsData.GroupConfig?.Backup?.IsBrowser ?? false)
                {
                    account.Status = "LDPlayer: Backup Groups...";
                    SetGridDataRowStatus(account);

                    string gids = ldPlayerTool.GetGroupIDs();
                    account.GroupIDs = gids;

                    fbAccountViewModel.getAccountDao().UpdateGroup(account.UID, account.TotalGroup, account.GroupIDs, account.OldGroupIds);
                }

                // 1. NewsFeed
                if (processActionsData.PlayNewsFeed || processActionsData.PostTimeline)
                {
                    account.Status = "LDPlayer: NewsFeed...";
                    SetGridDataRowStatus(account);
                    ldPlayerTool.InteractWithNewsFeed(3, processActionsData.PlayNewsFeed);
                }

                if (processActionsData.TurnOnPM)
                {
                    account.Status = "LDPlayer: Turning on Professional Mode...";
                    SetGridDataRowStatus(account);
                    ldPlayerTool.TurnOnPM();
                }

                // Delete Data (Clear Cache) - your toggle is ProcessActions.ProfileDeleteData
                if (processActionsData.ProfileDeleteData)
                {
                    account.Status = "LDPlayer: Clearing App Cache...";
                    SetGridDataRowStatus(account);
                    ldPlayerTool.ClearAppCache("com.facebook.katana");
                    Thread.Sleep(5000);
                }

                // New Password - your toggle is ProcessActions.NewPassword
                if (processActionsData.NewPassword)
                {
                    account.Status = "LDPlayer: Changing Password...";
                    SetGridDataRowStatus(account);

                    var newPass = processActionsData.ProfileConfig?.NewInfo?.Password;
                    if (string.IsNullOrWhiteSpace(newPass))
                        newPass = "NewPass123!";

                    ldPlayerTool.ChangePassword(account.Password, newPass);
                    account.Password = newPass;

                    // NOTE: if your IAccountDao doesn't contain updatePassword -> you must add it or rename it
                    fbAccountViewModel.getAccountDao().updatePassword(account.UID, account.Password);
                }

                // Primary Location
                if (processActionsData.PrimaryLocation)
                {
                    account.Status = "LDPlayer: Setting Primary Location...";
                    SetGridDataRowStatus(account);

                    var loc = processActionsData.ProfileConfig?.NewInfo?.City;
                    if (string.IsNullOrWhiteSpace(loc))
                        loc = "London";

                    ldPlayerTool.SetPrimaryLocation(loc);
                }

                // 2. Profile Info
                if (processActionsData.NewInfo)
                {
                    account.Status = "LDPlayer: Updating Profile...";
                    SetGridDataRowStatus(account);

                    processActionsData.ProfileConfig = GetCacheConfig<ProfileConfig>("profile:config");

                    var city = processActionsData.ProfileConfig?.NewInfo?.City;
                    var hometown = processActionsData.ProfileConfig?.NewInfo?.Hometown;
                    var bio = processActionsData.ProfileConfig?.NewInfo?.Bio;

                    ldPlayerTool.UpdateProfileInfo(city, hometown, bio);
                }

                // 3. Friends
                if (processActionsData.AcceptFriends)
                {
                    account.Status = "LDPlayer: Accepting Friends...";
                    SetGridDataRowStatus(account);
                    ldPlayerTool.AcceptFriendRequests(processActionsData.FriendsConfig?.AcceptNumber ?? 5);
                }

                if (processActionsData.AddFriends)
                {
                    account.Status = "LDPlayer: Adding Suggest Friends...";
                    SetGridDataRowStatus(account);
                    ldPlayerTool.AddFriendsBySuggest(processActionsData.FriendsConfig?.AddNumber ?? 5);
                }

                if (processActionsData.AddFriendsByUID)
                {
                    account.Status = "LDPlayer: Adding UID Friends...";
                    SetGridDataRowStatus(account);

                    string uidsText = processActionsData.FriendsConfig?.FriendsByUID?.UIDs;
                    if (!string.IsNullOrEmpty(uidsText))
                    {
                        string[] uids = uidsText.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        ldPlayerTool.AddFriendsByUID(uids, processActionsData.FriendsConfig?.FriendsByUID?.AddNumber ?? 5);
                    }
                }

                // 4. Groups
                if (processActionsData.IsJoinGroup)
                {
                    account.Status = "LDPlayer: Joining Groups...";
                    SetGridDataRowStatus(account);

                    string[] groupIds = joinGroupIDArr;
                    if (groupIds != null)
                    {
                        foreach (var gid in groupIds)
                        {
                            if (IsStop()) break;

                            var answers = processActionsData.GroupConfig?.Join?.Answers ?? "";
                            ldPlayerTool.JoinGroup(gid, answers);
                            Thread.Sleep(2000);
                        }
                    }
                }

                if (processActionsData.IsViewGroup)
                {
                    account.Status = "LDPlayer: Viewing Group notifications...";
                    SetGridDataRowStatus(account);
                    ldPlayerTool.ReadNotifications();
                }

                if (processActionsData.ReadMessenger)
                {
                    account.Status = "LDPlayer: Reading Messenger...";
                    SetGridDataRowStatus(account);
                    ldPlayerTool.ReadMessenger();
                }

                // Page interaction: Create Page (your config uses CreatePage.Names/Categies)
                if (processActionsData.CreatePage)
                {
                    account.Status = "LDPlayer: Creating Page...";
                    SetGridDataRowStatus(account);

                    var createCfg = processActionsData.PageConfig?.CreatePage;
                    if (createCfg != null)
                    {
                        string name = (createCfg.Names ?? "")
                            .Split(new[] { '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .FirstOrDefault()?.Trim();

                        string category = (createCfg.Categies ?? "")
                            .Split(new[] { '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .FirstOrDefault()?.Trim();

                        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(category))
                        {
                            ldPlayerTool.CreatePage(name, category);
                        }
                        else
                        {
                            account.Status = "LDPlayer: CreatePage missing Names/Categies";
                            SetGridDataRowStatus(account);
                        }
                    }
                }

                // Create Reel (your config uses CreateReel.Captions)
                if (processActionsData.PageCreateReel)
                {
                    account.Status = "LDPlayer: Creating Reel...";
                    SetGridDataRowStatus(account);

                    var caption = processActionsData.PageConfig?.CreateReel?.Captions;
                    if (string.IsNullOrWhiteSpace(caption))
                        caption = "New Reel";

                    ldPlayerTool.CreateReel("", caption);
                }

                // Backup Pages
                if (processActionsData.BackupPage)
                {
                    account.Status = "LDPlayer: Backup Pages...";
                    SetGridDataRowStatus(account);

                    string pages = ldPlayerTool.BackupPages();
                    account.TotalPage = string.IsNullOrWhiteSpace(pages) ? 0 : pages.Split(',').Length;

                    fbAccountViewModel.getAccountDao().UpdatePage(account.UID, account.TotalPage, pages);
                }

                // Follow Page / Invite / Remove Admin (your config uses PageUrls)
                if (processActionsData.FollowPage)
                {
                    account.Status = "LDPlayer: Following Page...";
                    SetGridDataRowStatus(account);

                    var pageVal = processActionsData.PageConfig?.PageUrls ?? "";
                    if (!string.IsNullOrWhiteSpace(pageVal))
                        ldPlayerTool.FollowPage(pageVal);
                }

                if (processActionsData.IsPageInvite)
                {
                    account.Status = "LDPlayer: Inviting Friends to Page...";
                    SetGridDataRowStatus(account);

                    var pageVal = processActionsData.PageConfig?.PageUrls ?? "";
                    if (!string.IsNullOrWhiteSpace(pageVal))
                        ldPlayerTool.InviteFriendsToPage(pageVal);
                }

                if (processActionsData.IsPageRemoveAdmin)
                {
                    account.Status = "LDPlayer: Removing Admin from Page...";
                    SetGridDataRowStatus(account);

                    var pageVal = processActionsData.PageConfig?.PageUrls ?? "";
                    if (!string.IsNullOrWhiteSpace(pageVal))
                        ldPlayerTool.RemoveAdminFromPage(pageVal);
                }

                // 5. Warming (Marketplace/CheckIn)
                if (processActionsData.Marketplace)
                {
                    account.Status = "LDPlayer: Marketplace...";
                    SetGridDataRowStatus(account);

                    var location = processActionsData.ProfileConfig?.NewInfo?.Marketplace;
                    if (string.IsNullOrWhiteSpace(location))
                        location = "London";

                    ldPlayerTool.Marketplace(location);
                }

                if (processActionsData.CheckIn)
                {
                    account.Status = "LDPlayer: CheckIn...";
                    SetGridDataRowStatus(account);

                    var location = processActionsData.ProfileConfig?.NewInfo?.CheckIn;
                    if (string.IsNullOrWhiteSpace(location))
                        location = "London";

                    ldPlayerTool.CheckIn(location);
                }

                if (processActionsData.LogoutAlDevices)
                {
                    account.Status = "LDPlayer: Logging out all devices...";
                    SetGridDataRowStatus(account);
                    ldPlayerTool.LogoutAllDevices();
                }

                if (processActionsData.IsCheckReelInvite)
                {
                    account.Status = "LDPlayer: Checking Reel Invite...";
                    SetGridDataRowStatus(account);
                    ldPlayerTool.CheckReelInvite();
                }

                if (processActionsData.IsLeaveGroup)
                {
                    account.Status = "LDPlayer: Leaving Groups...";
                    SetGridDataRowStatus(account);

                    var leaveGroupIdsText = cacheViewModel.GetCacheDao().Get("group:config:group_ids")?.Value?.ToString();
                    if (!string.IsNullOrEmpty(leaveGroupIdsText))
                    {
                        string[] lGids = leaveGroupIdsText.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var gid in lGids)
                        {
                            if (IsStop()) break;
                            ldPlayerTool.LeaveGroup(gid);
                            Thread.Sleep(2000);
                        }
                    }
                }

                if (processActionsData.BackupFriends)
                {
                    account.Status = "LDPlayer: Backup Friends...";
                    SetGridDataRowStatus(account);

                    string friends = ldPlayerTool.BackupFriends();
                    account.TotalFriend = string.IsNullOrWhiteSpace(friends) ? 0 : friends.Split(',').Length;

                    // NOTE: if your IAccountDao doesn't contain updateTotalFriend -> you must add it or rename it
                    fbAccountViewModel.getAccountDao().updateTotalFriend(account.UID, account.TotalFriend);
                }

                if (processActionsData.ActivtiyLog)
                {
                    account.Status = "LDPlayer: Deleting Activity...";
                    SetGridDataRowStatus(account);
                    ldPlayerTool.DeleteActivity();
                }

                if (processActionsData.PublicPost)
                {
                    account.Status = "LDPlayer: Setting Public Post...";
                    SetGridDataRowStatus(account);
                    ldPlayerTool.PublicPost();
                }

                if (processActionsData.TurnOnTwoFA)
                {
                    account.Status = "LDPlayer: Turning on 2FA...";
                    SetGridDataRowStatus(account);
                    ldPlayerTool.TurnOnTwoFA();
                }

                // 6. Sharing
                if (processActionsData.IsShareToTimeline || processActionsData.IsShareToGroup || processActionsData.IsShareByGraph)
                {
                    account.Status = "LDPlayer: Sharing...";
                    SetGridDataRowStatus(account);
                    StartShareVideoLDPlayer(account);
                }

                account.Status = "LDPlayer Success";
                SetGridDataRowStatus(account);
            }
            catch (Exception ex)
            {
                account.Status = "LDPlayer Error: " + ex.Message;
                SetGridDataRowStatus(account);
            }
        }


        //public void StartRunAccountLDPlayer(FbAccount account)
        //{
        //    try
        //    {
        //        if (IsStop()) return;
        //        ldPlayerTool.Connect();

        //        // Optional: Clear cache if specified (matching web behavior if desired)
        //        // ldPlayerTool.ClearAppCache("com.facebook.katana"); 

        //        if (!string.IsNullOrEmpty(account.Proxy))
        //        {
        //            account.Status = "LDPlayer: Setting Proxy...";
        //            SetGridDataRowStatus(account);
        //            ldPlayerTool.SetProxy(account.Proxy);
        //        }
        //        else
        //        {
        //            ldPlayerTool.ClearProxy();
        //        }

        //        ldPlayerTool.OpenApp("com.facebook.katana");
        //        account.Status = "LDPlayer Started";
        //        SetGridDataRowStatus(account);

        //        if (processActionsData.EnglishUS)
        //        {
        //            account.Status = "LDPlayer: Checking Language...";
        //            SetGridDataRowStatus(account);
        //            if (!ldPlayerTool.IsEnglish())
        //            {
        //                account.Status = "LDPlayer: Normalizing Language to English...";
        //                SetGridDataRowStatus(account);
        //                ldPlayerTool.ChangeLanguage();
        //            }
        //        }

        //        if (!ldPlayerTool.WaitText("What's on your mind?", 20))
        //        {
        //            if (!ldPlayerTool.IsLoggedIn())
        //            {
        //                account.Status = "LDPlayer Error: Not Logged In";
        //            }
        //           if (!ldPlayerTool.WaitText("What's on your mind?", 20))
        //            {
        //                account.Status = "LDPlayer: What's on your mind not found";
        //                SetGridDataRowStatus(account);
        //                return;
        //            }
        //        }

        //        // 0. GetInfo
        //        if (processActionsData.AutoUnlockCheckpoint)
        //        {
        //            account.Status = "LDPlayer: Unlocking Checkpoint...";
        //            SetGridDataRowStatus(account);
        //            ldPlayerTool.UnlockCheckpoint();
        //        }

        //        if (processActionsData.LockTime)
        //        {
        //            account.Status = "LDPlayer: Locking Profile...";
        //            SetGridDataRowStatus(account);
        //            ldPlayerTool.LockProfile();
        //        }

        //        if (processActionsData.ContactPrimary || processActionsData.ContactRemovePhone || processActionsData.ContactRemoveInstragram)
        //        {
        //            account.Status = "LDPlayer: Managing Contact Info...";
        //            SetGridDataRowStatus(account);
        //            ldPlayerTool.RemoveContactInfo(processActionsData.ContactRemovePhone, processActionsData.ContactRemoveInstragram);
        //        }

        //        if (processActionsData.GetInfo)
        //        {
        //            account.Status = "LDPlayer: Getting Info...";
        //            SetGridDataRowStatus(account);
        //            ldPlayerTool.GetInfo(account);

        //            if (processActionsData.CreationDate)
        //            {
        //                var cDate = ldPlayerTool.GetCreationDate();
        //                account.Description += $", Created: {cDate}";
        //                // Optional: update DB if field exists
        //            }

        //            if (processActionsData.Token)
        //            {
        //                string token = ldPlayerTool.GetAccessToken();
        //                account.Token = token;
        //                fbAccountViewModel.getAccountDao().updateToken(account.UID, account.Token);
        //            }
        //        }

        //        if (processActionsData.WorkingOnPage)
        //        {
        //            if (!processActionsData.NoSwitchPage)
        //            {
        //                account.Status = "LDPlayer: Switching to Page...";
        //                SetGridDataRowStatus(account);
        //                var pageVal = processActionsData.PageConfig?.Follow?.Value ?? "My Page";
        //                ldPlayerTool.SwitchToPage(pageVal);
        //            }
        //        }

        //        if (processActionsData.OtherConfig?.ChangeLanguage ?? false)
        //        {
        //            account.Status = "LDPlayer: Changing Language...";
        //            SetGridDataRowStatus(account);
        //            ldPlayerTool.ChangeLanguage();
        //        }

        //        if (processActionsData.OtherConfig?.WatchTime ?? false)
        //        {
        //            account.Status = "LDPlayer: Watching Videos...";
        //            SetGridDataRowStatus(account);
        //            ldPlayerTool.WatchTime(5); // Default 5 mins
        //        }

        //        if (processActionsData.OtherConfig?.ReelPlay ?? false)
        //        {
        //            account.Status = "LDPlayer: Playing Reels...";
        //            SetGridDataRowStatus(account);
        //            ldPlayerTool.ReelPlay(5); // Default 5 reels
        //        }

        //        if (processActionsData.GroupConfig?.Backup?.IsBrowser ?? false)
        //        {
        //            account.Status = "LDPlayer: Backup Groups...";
        //            SetGridDataRowStatus(account);
        //            string gids = ldPlayerTool.GetGroupIDs();
        //            account.GroupIDs = gids;
        //            // Update DB 
        //            fbAccountViewModel.getAccountDao().UpdateGroup(account.UID, account.TotalGroup, account.GroupIDs, account.OldGroupIds);
        //        }

        //        // 1. NewsFeed
        //        if (processActionsData.PlayNewsFeed || processActionsData.PostTimeline)
        //        {
        //            account.Status = "LDPlayer: NewsFeed...";
        //            SetGridDataRowStatus(account);
        //            ldPlayerTool.InteractWithNewsFeed(3, processActionsData.PlayNewsFeed);
        //        }

        //        if (processActionsData.TurnOnPM)
        //        {
        //            account.Status = "LDPlayer: Turning on Professional Mode...";
        //            SetGridDataRowStatus(account);
        //            ldPlayerTool.TurnOnPM();
        //        }

        //        //if (processActionsData.ProfileConfig?.DeleteData ?? false)
        //        //{
        //        //    account.Status = "LDPlayer: Clearing App Cache...";
        //        //    SetGridDataRowStatus(account);
        //        //    ldPlayerTool.ClearAppCache("com.facebook.katana");
        //        //    Thread.Sleep(5000); // Wait for reset
        //        //}

        //        //if (processActionsData.ProfileConfig?.NewPassword ?? false)
        //        //{
        //        //    account.Status = "LDPlayer: Changing Password...";
        //        //    SetGridDataRowStatus(account);
        //        //    var newPass = processActionsData.ProfileConfig?.NewInfo?.Password ?? "NewPass123!";
        //        //    ldPlayerTool.ChangePassword(account.Password, newPass);
        //        //    account.Password = newPass;
        //        //    fbAccountViewModel.getAccountDao().updatePassword(account.UID, account.Password);
        //        //}

        //        // Delete Data (Clear Cache)
        //        if (processActionsData.ProfileDeleteData || processActionsData.ProfileConfig?.DeleteData != null)
        //        {
        //            account.Status = "LDPlayer: Clearing App Cache...";
        //            SetGridDataRowStatus(account);
        //            ldPlayerTool.ClearAppCache("com.facebook.katana");
        //            Thread.Sleep(5000);
        //        }

        //        // New Password
        //        if (processActionsData.NewPassword)
        //        {
        //            account.Status = "LDPlayer: Changing Password...";
        //            SetGridDataRowStatus(account);

        //            var newPass = processActionsData.ProfileConfig?.NewInfo?.Password;
        //            if (string.IsNullOrWhiteSpace(newPass))
        //                newPass = "NewPass123!";

        //            ldPlayerTool.ChangePassword(account.Password, newPass);
        //            account.Password = newPass;

        //            fbAccountViewModel.getAccountDao().updatePassword(account.UID, account.Password);
        //        }


        //        if (processActionsData.PrimaryLocation)
        //        {
        //            account.Status = "LDPlayer: Setting Primary Location...";
        //            SetGridDataRowStatus(account);
        //            var loc = processActionsData.ProfileConfig?.NewInfo?.City ?? "London";
        //            ldPlayerTool.SetPrimaryLocation(loc);
        //        }

        //        // 2. Profile Info
        //        if (processActionsData.NewInfo)
        //        {
        //            account.Status = "LDPlayer: Updating Profile...";
        //            SetGridDataRowStatus(account);
        //            processActionsData.ProfileConfig = GetCacheConfig<ProfileConfig>("profile:config");
        //            var city = processActionsData.ProfileConfig?.NewInfo?.City;
        //            var hometown = processActionsData.ProfileConfig?.NewInfo?.Hometown;
        //            var bio = processActionsData.ProfileConfig?.NewInfo?.Bio;
        //            ldPlayerTool.UpdateProfileInfo(city, hometown, bio);
        //        }

        //        // 3. Friends
        //        if (processActionsData.AcceptFriends)
        //        {
        //            account.Status = "LDPlayer: Accepting Friends...";
        //            SetGridDataRowStatus(account);
        //            ldPlayerTool.AcceptFriendRequests(processActionsData.FriendsConfig?.AcceptNumber ?? 5);
        //        }

        //        if (processActionsData.AddFriends)
        //        {
        //            account.Status = "LDPlayer: Adding Suggest Friends...";
        //            SetGridDataRowStatus(account);
        //            ldPlayerTool.AddFriendsBySuggest(processActionsData.FriendsConfig?.AddNumber ?? 5);
        //        }

        //        if (processActionsData.AddFriendsByUID)
        //        {
        //            account.Status = "LDPlayer: Adding UID Friends...";
        //            SetGridDataRowStatus(account);
        //            string uidsText = processActionsData.FriendsConfig?.FriendsByUID?.UIDs;
        //            if (!string.IsNullOrEmpty(uidsText))
        //            {
        //                string[] uids = uidsText.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        //                ldPlayerTool.AddFriendsByUID(uids, processActionsData.FriendsConfig?.FriendsByUID?.AddNumber ?? 5);
        //            }
        //        }

        //        // 4. Groups
        //        if (processActionsData.IsJoinGroup)
        //        {
        //            account.Status = "LDPlayer: Joining Groups...";
        //            SetGridDataRowStatus(account);
        //            string[] groupIds = joinGroupIDArr; // Use existing array
        //            if (groupIds != null)
        //            {
        //                foreach (var gid in groupIds)
        //                {
        //                    if (IsStop()) break;
        //                    var answers = processActionsData.GroupConfig?.Join?.Answers ?? "";
        //                    ldPlayerTool.JoinGroup(gid, answers);
        //                    Thread.Sleep(2000);
        //                }
        //            }
        //        }

        //        // 4. Notifications
        //        if (processActionsData.IsViewGroup)
        //        {
        //            account.Status = "LDPlayer: Viewing Group notifications...";
        //            SetGridDataRowStatus(account);
        //            ldPlayerTool.ReadNotifications();
        //        }

        //        if (processActionsData.ReadMessenger)
        //        {
        //            account.Status = "LDPlayer: Reading Messenger...";
        //            SetGridDataRowStatus(account);
        //            ldPlayerTool.ReadMessenger();
        //        }

        //        // Page interaction
        //        if (processActionsData.CreatePage)
        //        {
        //            account.Status = "LDPlayer: Creating Page...";
        //            SetGridDataRowStatus(account);
        //            var pageConfig = processActionsData.PageConfig?.Create;
        //            if (pageConfig != null)
        //            {
        //                ldPlayerTool.CreatePage(pageConfig.Name, pageConfig.Category);
        //            }
        //        }

        //        if (processActionsData.PageCreateReel)
        //        {
        //            account.Status = "LDPlayer: Creating Reel...";
        //            SetGridDataRowStatus(account);
        //            // caption from config
        //            var caption = processActionsData.PageConfig?.CreateReel?.Caption ?? "New Reel";
        //            ldPlayerTool.CreateReel("", caption);
        //        }
        //        if (processActionsData.BackupPage)
        //        {
        //            account.Status = "LDPlayer: Backup Pages...";
        //            SetGridDataRowStatus(account);
        //            string pages = ldPlayerTool.BackupPages();
        //            account.TotalPage = pages.Split(',').Length;
        //            fbAccountViewModel.getAccountDao().UpdatePage(account.UID, account.TotalPage, pages);
        //        }

        //        // Page interaction
        //        if (processActionsData.FollowPage)
        //        {
        //            account.Status = "LDPlayer: Following Page...";
        //            SetGridDataRowStatus(account);
        //            var pageVal = processActionsData.PageConfig?.Follow?.Value ?? "";
        //            if (!string.IsNullOrEmpty(pageVal))
        //            {
        //                ldPlayerTool.FollowPage(pageVal);
        //            }
        //        }

        //        if (processActionsData.IsPageInvite)
        //        {
        //            account.Status = "LDPlayer: Inviting Friends to Page...";
        //            SetGridDataRowStatus(account);
        //            var pageVal = processActionsData.PageConfig?.Follow?.Value ?? "";
        //            if (!string.IsNullOrEmpty(pageVal))
        //            {
        //                ldPlayerTool.InviteFriendsToPage(pageVal);
        //            }
        //        }

        //        if (processActionsData.IsPageRemoveAdmin)
        //        {
        //            account.Status = "LDPlayer: Removing Admin from Page...";
        //            SetGridDataRowStatus(account);
        //            var pageVal = processActionsData.PageConfig?.Follow?.Value ?? "";
        //            if (!string.IsNullOrEmpty(pageVal))
        //            {
        //                ldPlayerTool.RemoveAdminFromPage(pageVal);
        //            }
        //        }

        //        // 5. Warming (Marketplace/CheckIn)
        //        if (processActionsData.Marketplace)
        //        {
        //            account.Status = "LDPlayer: Marketplace...";
        //            SetGridDataRowStatus(account);
        //            var location = processActionsData.ProfileConfig?.NewInfo?.Marketplace ?? "London";
        //            ldPlayerTool.Marketplace(location);
        //        }

        //        if (processActionsData.CheckIn)
        //        {
        //            account.Status = "LDPlayer: CheckIn...";
        //            SetGridDataRowStatus(account);
        //            var location = processActionsData.ProfileConfig?.NewInfo?.CheckIn ?? "London";
        //            ldPlayerTool.CheckIn(location);
        //        }

        //        if (processActionsData.LogoutAlDevices)
        //        {
        //            account.Status = "LDPlayer: Logging out all devices...";
        //            SetGridDataRowStatus(account);
        //            ldPlayerTool.LogoutAllDevices();
        //        }

        //        if (processActionsData.IsCheckReelInvite)
        //        {
        //            account.Status = "LDPlayer: Checking Reel Invite...";
        //            SetGridDataRowStatus(account);
        //            ldPlayerTool.CheckReelInvite();
        //        }

        //        if (processActionsData.IsLeaveGroup)
        //        {
        //            account.Status = "LDPlayer: Leaving Groups...";
        //            SetGridDataRowStatus(account);
        //            var leaveGroupIdsText = cacheViewModel.GetCacheDao().Get("group:config:group_ids")?.Value?.ToString();
        //            if (!string.IsNullOrEmpty(leaveGroupIdsText))
        //            {
        //                string[] lGids = leaveGroupIdsText.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        //                foreach (var gid in lGids)
        //                {
        //                    if (IsStop()) break;
        //                    ldPlayerTool.LeaveGroup(gid);
        //                    Thread.Sleep(2000);
        //                }
        //            }
        //        }

        //        if (processActionsData.BackupFriends)
        //        {
        //            account.Status = "LDPlayer: Backup Friends...";
        //            SetGridDataRowStatus(account);
        //            string friends = ldPlayerTool.BackupFriends();
        //            account.TotalFriend = friends.Split(',').Length;
        //            fbAccountViewModel.getAccountDao().updateTotalFriend(account.UID, account.TotalFriend);
        //        }

        //        if (processActionsData.ActivtiyLog)
        //        {
        //            account.Status = "LDPlayer: Deleting Activity...";
        //            SetGridDataRowStatus(account);
        //            ldPlayerTool.DeleteActivity();
        //        }

        //        if (processActionsData.PublicPost)
        //        {
        //            account.Status = "LDPlayer: Setting Public Post...";
        //            SetGridDataRowStatus(account);
        //            ldPlayerTool.PublicPost();
        //        }

        //        if (processActionsData.TurnOnTwoFA)
        //        {
        //            account.Status = "LDPlayer: Turning on 2FA...";
        //            SetGridDataRowStatus(account);
        //            ldPlayerTool.TurnOnTwoFA();
        //        }

        //        // 6. Sharing
        //        if (processActionsData.IsShareToTimeline || processActionsData.IsShareToGroup || processActionsData.IsShareByGraph)
        //        {
        //            account.Status = "LDPlayer: Sharing...";
        //            SetGridDataRowStatus(account);
        //            StartShareVideoLDPlayer(account);
        //        }

        //        account.Status = "LDPlayer Success";
        //        SetGridDataRowStatus(account);
        //    }
        //    catch (Exception ex)
        //    {
        //        account.Status = "LDPlayer Error: " + ex.Message;
        //        SetGridDataRowStatus(account);
        //    }
        //}
        public void WorkingProcess(IWebDriver webDriver, FbAccount account, bool isNoSwitchPage, bool isCheckReelInvite)
        {
            if (processActionsData.PrimaryLocation)
            {
                PrimaryLocation(webDriver, account);
            }
            if (IsStop())
            {
                StopWorking(webDriver, account.UID);
                return;
            }
            if (processActionsData.Token)
            {
                Token(webDriver, account);
            }
            if (!DetectStopProcess(webDriver, account))
            {
                if (IsStop())
                {
                    StopWorking(webDriver, account.UID);
                    return;
                }
                GetInfo(webDriver, account);
                if (!DetectStopProcess(webDriver, account))
                {
                    if (IsStop())
                    {
                        StopWorking(webDriver, account.UID);
                        return;
                    }
                    StartNewsFeedConfig(webDriver, account);
                    if (!DetectStopProcess(webDriver, account))
                    {
                        if (IsStop())
                        {
                            StopWorking(webDriver, account.UID);
                            return;
                        }
                        if (!DetectStopProcess(webDriver, account))
                        {
                            if (IsStop())
                            {
                                StopWorking(webDriver, account.UID);
                                return;
                            }
                            StartProfile(webDriver, account);
                            if (!DetectStopProcess(webDriver, account))
                            {
                                if (IsStop())
                                {
                                    StopWorking(webDriver, account.UID);
                                    return;
                                }
                                StartFriends(webDriver, account);
                                if (!DetectStopProcess(webDriver, account))
                                {
                                    if (IsStop())
                                    {
                                        StopWorking(webDriver, account.UID);
                                        return;
                                    }
                                    if (!isNoSwitchPage)
                                    {
                                        RemovePageCookie(webDriver);
                                    }
                                    StartPage(webDriver, account, isNoSwitchPage);
                                    if (!DetectStopProcess(webDriver, account))
                                    {
                                        if (IsStop())
                                        {
                                            StopWorking(webDriver, account.UID);
                                            return;
                                        }
                                        if (!isNoSwitchPage)
                                        {
                                            RemovePageCookie(webDriver);
                                        }
                                        StartGroup(webDriver, account);
                                        if (!DetectStopProcess(webDriver, account))
                                        {
                                            if (IsStop())
                                            {
                                                StopWorking(webDriver, account.UID);
                                                return;
                                            }
                                            if (!isNoSwitchPage)
                                            {
                                                RemovePageCookie(webDriver);
                                            }
                                            StartShareVideo(webDriver, account);
                                            //if (!DetectStopProcess(webDriver, account) && IsStop())
                                            //{
                                            //    StopWorking(webDriver, account.UID);
                                            //    return;
                                            //}
                                            if (isCheckReelInvite)
                                            {
                                                CheckReelInvite(webDriver, account);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public void CheckReelInvite(IWebDriver driver, FbAccount account)
        {
            string[] pageArr = (account.PageIds ?? "").Split(',');
            for (int i = 0; i < pageArr.Length; i++)
            {
                string[] page = pageArr[i].Trim().Split('|');
                string page_id = "";
                try
                {
                    page_id = page[0].Trim();
                }
                catch (Exception) { }
                if (string.IsNullOrEmpty(page_id))
                {
                    continue;
                }
                string asset_id = WebFBTool.GetPageAsset(driver, page_id).Trim();
                if (string.IsNullOrEmpty(asset_id))
                {
                    continue;
                }
                try
                {
                    driver.Navigate().GoToUrl("https://www.facebook.com/creatorstudio/?collection_id=free_form_collection&mode=facebook&selected_single_page_id=" + asset_id + "&tab=monetization_eligibility");
                }
                catch (Exception) { }

                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(2000);

                //int counter = 10;
                //bool isWorking = false; x
                //do
                //{
                //    Thread.Sleep(500);
                //    try
                //    {
                //        driver.FindElement(By.XPath("//div[@aria-label='Expand']")).Click();
                //        Thread.Sleep(1000);
                //    } catch(Exception) { }
                //    try
                //    {
                //        driver.FindElement(By.XPath("//span[text() = 'Meta Business Suite']")).Click();
                //        isWorking= true;
                //    }
                //    catch (Exception) { }
                //    if(!isWorking)
                //    {
                //        try
                //        {
                //            driver.FindElement(By.XPath("//span[text() = 'Meta business suite']")).Click();
                //            isWorking = true;
                //        }
                //        catch (Exception) { }
                //    }
                //} while (!isWorking && counter-- > 0);
                //if(!isWorking)
                //{
                //    continue;
                //}
                try
                {
                    driver.Navigate().GoToUrl("https://business.facebook.com/latest/monetization");
                }
                catch (Exception) { }

                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(2000);
                int counter = 5;
                bool isWorking = false;
                do
                {
                    Thread.Sleep(500);
                    try
                    {
                        driver.FindElement(By.XPath("//*[text() = 'Ads on Reels']"));
                        isWorking = true;
                    }
                    catch (Exception) { }
                    if (!isWorking)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//*[text() = 'Ads on reels']"));
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }
                } while (!isWorking && counter-- > 0);
                if (isWorking)
                {
                    account.Description = "Reel Invite";
                    // update message
                    fbAccountViewModel.getAccountDao().updateStatus(account.UID, account.Description, 1);
                    break;
                }
            }
        }
        public void RecoveryPhoneNumber(IWebDriver driver, FbAccount account)
        {
            try
            {
                var cache = cacheViewModel.GetCacheDao().Get("recovery:config");
                if (cache != null && cache.Value != null)
                {
                    string text = cache.Value.ToString();
                    processActionsData.RecoveryConfig = JsonConvert.DeserializeObject<RecoveryConfig>(text);
                }
            }
            catch (Exception)
            {
            }
            try
            {
                driver.Navigate().GoToUrl("https://web.facebook.com/login/identify/?ctx=recover&ars=facebook_login&from_login_screen=0");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(2000);
            int counter = 1;
            bool isWorking = false;
            string fiveSimBuyer = "";
            do
            {
                Thread.Sleep(500);
                string phoneNumber = "", orderId = "";
                try
                {
                    if (processActionsData.RecoveryConfig.FiveSime)
                    {
                        fiveSimBuyer = FiveSimBuyPhone();
                        if (string.IsNullOrEmpty(fiveSimBuyer))
                        {
                            break;
                        }
                        string[] arr = fiveSimBuyer.Split('|');
                        phoneNumber = arr[0];
                        orderId = arr[1];
                    }
                }
                catch (Exception) { }
                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    // search account by phone
                    try
                    {
                        driver.FindElement(By.Id("identify_email")).SendKeys(phoneNumber + OpenQA.Selenium.Keys.Enter);
                        isWorking = true;
                    }
                    catch (Exception) { }
                    // Identify your account
                    Thread.Sleep(2000);
                    bool myAccount = false;
                    try
                    {
                        driver.FindElement(By.XPath("//*[text() = 'This Is My Account']")).Click();
                        myAccount = true;
                    }
                    catch (Exception) { }
                    if (!myAccount)
                    {
                        try
                        {
                            driver.FindElement(By.XPath("//*[text() = 'This is my account']")).Click();
                            myAccount = true;
                        }
                        catch (Exception) { }
                    }
                    //We'll send you a code to your mobile number
                    if (isWorking)
                    {
                        Thread.Sleep(1000);
                        isWorking = false;
                        counter = 8;
                        do
                        {
                            Thread.Sleep(500);
                            try
                            {
                                driver.FindElement(By.XPath("//*[text() = 'Continue']")).Click();
                                isWorking = true;
                            }
                            catch (Exception) { }

                            if (!isWorking)
                            {
                                try
                                {
                                    driver.FindElement(By.XPath("//*[text() = 'Try another way']")).Click();
                                }
                                catch (Exception) { }
                            }
                            if (!isWorking)
                            {
                                try
                                {
                                    driver.FindElement(By.XPath("//*[text() = 'Try Another Way']")).Click();
                                }
                                catch (Exception) { }
                            }
                        } while (!isWorking && counter-- > 0);
                    }

                    if (isWorking)
                    {
                        // verify code
                        isWorking = false;
                        Thread.Sleep(4000);
                        IWebElement elementInputCode = null;
                        try
                        {
                            elementInputCode = driver.FindElement(By.Name("n"));
                        }
                        catch (Exception) { }
                        if (elementInputCode != null)
                        {
                            string code = FiveSimGetCode(orderId);
                            if (string.IsNullOrEmpty(code))
                            {
                                break;
                            }
                            try
                            {
                                elementInputCode.SendKeys(code + OpenQA.Selenium.Keys.Enter);
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                    }

                    if (isWorking)
                    {
                        // new password
                        Thread.Sleep(2000);
                        string newPass = processActionsData.RecoveryConfig.Password;
                        if (string.IsNullOrEmpty(newPass))
                        {
                            newPass = GetRandomString(10);
                        }
                        try
                        {
                            driver.FindElement(By.Name("password_new")).SendKeys(newPass + OpenQA.Selenium.Keys.Enter);
                        }
                        catch (Exception) { }

                        FBTool.WaitingPageLoading(driver);
                        Thread.Sleep(2000);
                        counter = 8;
                        bool isPass = false;
                        do
                        {
                            Thread.Sleep(500);
                            if (!IsStop())
                            {
                                isPass = true;
                            }
                            try
                            {
                                driver.FindElement(By.Name("password_new"));
                                isPass = true;
                            }
                            catch (Exception) { }
                        } while (!isPass && counter-- > 0);

                        Thread.Sleep(2000);
                        try
                        {
                            driver.Navigate().GoToUrl("https://facebook.com");
                        }
                        catch (Exception) { }
                        FBTool.WaitingPageLoading(driver);
                        Thread.Sleep(1000);

                        var uid = FBTool.GetUserId(driver);
                        if (string.IsNullOrEmpty(uid))
                        {
                            uid = phoneNumber;
                        }
                        string browserKey = ConfigData.GetBrowserKey(account.UID);
                        FBTool.CloneDataProfile(browserKey, uid);

                        fbAccountViewModel.getAccountDao().Password(account.UID, newPass);
                        fbAccountViewModel.getAccountDao().updateUID(account.UID, uid);

                        account.UID = uid;
                        account.Password = newPass;
                    }
                }
            } while (!isWorking && counter-- > 0);

        }
        public void MbasicRecoveryPhoneNumber(IWebDriver driver, FbAccount account)
        {
            try
            {
                var cache = cacheViewModel.GetCacheDao().Get("recovery:config");
                if (cache != null && cache.Value != null)
                {
                    string text = cache.Value.ToString();
                    processActionsData.RecoveryConfig = JsonConvert.DeserializeObject<RecoveryConfig>(text);
                }
            }
            catch (Exception)
            {
            }
            try
            {
                driver.Navigate().GoToUrl("https://mbasic.facebook.com/login/identify/?ctx=recover&c=https%3A%2F%2Fmbasic.facebook.com%2F&multiple_results=0&ars=facebook_login&from_login_screen=0&lwv=100&_rdr");
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            int counter = 6;
            bool isWorking = false;
            string fiveSimBuyer = "";
            do
            {
                Thread.Sleep(500);
                string phoneNumber = "", orderId = "";
                try
                {
                    if (processActionsData.RecoveryConfig.FiveSime)
                    {
                        fiveSimBuyer = FiveSimBuyPhone();
                        if (string.IsNullOrEmpty(fiveSimBuyer))
                        {
                            break;
                        }
                        string[] arr = fiveSimBuyer.Split('|');
                        phoneNumber = arr[0];
                        orderId = arr[1];
                    }
                }
                catch (Exception) { }
                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    // search account by phone
                    try
                    {
                        driver.FindElement(By.Id("identify_search_text_input")).SendKeys(phoneNumber + OpenQA.Selenium.Keys.Enter);
                        isWorking = true;
                    }
                    catch (Exception) { }
                    // Choose your account
                    Thread.Sleep(2000);
                    try
                    {
                        driver.FindElement(By.ClassName("ba")).Click();
                    }
                    catch (Exception) { }
                    //Send code via SMS
                    if (isWorking)
                    {
                        Thread.Sleep(1500);
                        try
                        {
                            driver.FindElement(By.XPath("//div[text() = 'Send code via SMS']")).Click();
                            Thread.Sleep(500);
                        }
                        catch (Exception) { }
                        isWorking = false;
                        try
                        {
                            driver.FindElement(By.Name("reset_action")).Click();
                            isWorking = true;
                        }
                        catch (Exception) { }
                    }

                    if (isWorking)
                    {
                        // verify code
                        isWorking = false;
                        Thread.Sleep(2000);
                        IWebElement elementInputCode = null;
                        try
                        {
                            elementInputCode = driver.FindElement(By.Name("n"));
                        }
                        catch (Exception) { }
                        if (elementInputCode != null)
                        {
                            string code = FiveSimGetCode(orderId);
                            if (string.IsNullOrEmpty(code))
                            {
                                break;
                            }
                            try
                            {
                                elementInputCode.SendKeys(code + OpenQA.Selenium.Keys.Enter);
                                isWorking = true;
                            }
                            catch (Exception) { }
                        }
                    }

                    if (isWorking)
                    {
                        // new password
                        try
                        {
                            Thread.Sleep(2000);
                            string newPass = processActionsData.RecoveryConfig.Password;
                            if (string.IsNullOrEmpty(newPass))
                            {
                                newPass = GetRandomString(10);
                            }
                            driver.FindElement(By.Name("password_new")).SendKeys(newPass + OpenQA.Selenium.Keys.Enter);
                            FBTool.WaitingPageLoading(driver);
                            Thread.Sleep(2000);



                            driver.Navigate().GoToUrl("facebook.com");
                            FBTool.WaitingPageLoading(driver);
                            Thread.Sleep(1000);

                            var uid = FBTool.GetUserId(driver);

                            fbAccountViewModel.getAccountDao().Password(account.UID, newPass);
                            fbAccountViewModel.getAccountDao().updateUID(account.UID, uid);

                            account.UID = uid;
                            account.Password = newPass;
                        }
                        catch (Exception) { }
                    }
                }
            } while (!isWorking && counter-- > 0);

        }
        public string FiveSimGetCode(string order_id)
        {
            int counter = 30;
            string code = "";
            var fiveSim = processActionsData.RecoveryConfig.PhoneNumber.FiveSim;
            do
            {
                Thread.Sleep(500);
                code = LocalData.GetFiveSimCodeVerify(fiveSim.APIKey, order_id);
            } while (string.IsNullOrEmpty(code) && counter-- > 0);

            return code;
        }
        public string FiveSimBuyPhone()
        {
            int counter = 30;
            string fiveSimBuyer = "";
            var fiveSim = processActionsData.RecoveryConfig.PhoneNumber.FiveSim;
            do
            {
                Thread.Sleep(500);
                fiveSimBuyer = LocalData.FiveSimBuyPhoneNumber(fiveSim.APIKey, fiveSim.Country, fiveSim.Opterator, fiveSim.Product);
            } while (string.IsNullOrEmpty(fiveSimBuyer) && counter-- > 0);

            return fiveSimBuyer;
        }
        public void PrimaryLocation(IWebDriver driver, FbAccount account)
        {
            account.Description += ", Primary Location";

            try
            {
                driver.Navigate().GoToUrl("https://www.facebook.com/primary_location/info");
            }
            catch (Exception)
            {
            }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);

            string primaryLocation = "";
            IWebElement element = null;
            int counter = 10;
            do
            {
                Thread.Sleep(1000);
                try
                {
                    element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div/div[1]/div[1]/div/div/div/div/div/div[2]/div/div[2]/div/div/div/div[2]/div/div/span"));
                }
                catch (Exception) { }
                if (element == null)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div/div/div/div/div[2]/div/div[2]/div/div/div/div[2]/div/div/span"));
                    }
                    catch (Exception) { }
                }
                if (element == null)
                {
                    try
                    {
                        element = driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div/div/div/div/div/div[2]/div/div[2]/div/div/div/div[2]/div/div/span"));
                    }
                    catch (Exception) { }
                }
            }while (element==null && counter-- > 0);
            if (element != null)
            {
                try
                {
                    var str = element.GetAttribute("innerHTML");
                    if (!string.IsNullOrEmpty(str))
                    {
                        primaryLocation = str.Trim();
                    }
                }
                catch (Exception) { }
            }
            if (string.IsNullOrEmpty(primaryLocation))
            {
                primaryLocation = "";
            }

            account.PrimaryLocation = primaryLocation;
            account.Description += " " + primaryLocation;

            fbAccountViewModel.getAccountDao().UpdatePrimaryLocation(account.UID.Trim(), primaryLocation);
        }
        public bool DetectStopProcess(IWebDriver driver, FbAccount data)
        {
            bool result = false;
            string results = FBTool.GetResults(driver, 0);
            if (results != "success")
            {
                result = true;
                data.Status = "Die";
                data.Description = results;
                SetGridDataRowStatus(data);
                fbAccountViewModel.getAccountDao().updateStatus(data.UID, results, 0);
            }
            return result;
        }
        public int GetVerifyYandexIndex()
        {
            int num = 0;
            Application.Current.Dispatcher.Invoke(delegate
            {
                try
                {
                    var cache = cacheViewModel.GetCacheDao().Get("contact:config:index");
                    if (cache != null && cache.Value != null)
                    {
                        num = int.Parse(cache.Value.ToString());
                    }
                    cacheViewModel.GetCacheDao().Set("contact:config:index", (num + 1).ToString() ?? "");
                }
                catch (Exception)
                {
                }
            });

            return num;
        }
        public void YandexLogin(string email, string password)
        {
            try
            {
                driverYandex = new MyWebDriver().GetWebDriver(email, 1, 3);
                driverYandex = WebFBTool.LoginYandex(driverYandex, email, password);
            }
            catch (Exception)
            {
            }
        }
        public void CleanPC()
        {
            if (isCleanTMP)
            {
                LocalData.RunCMD("rmdir /s /q %temp%");
            }
            if (isCleanChrom)
            {
                LocalData.RunCMD("Taskkill /F /IM chrome.exe");
                Thread.Sleep(500);
                LocalData.RunCMD("Taskkill /F /IM chromedriver.exe");
            }
            if (isCleanEDGE)
            {
                LocalData.RunCMD("Taskkill /F /IM msedge.exe");
                Thread.Sleep(500);
                LocalData.RunCMD("Taskkill /F /IM microsoftedgedriver.exe");
            }
        }
        public void RemovePageCookie(IWebDriver driver)
        {
            try
            {
                driver.Manage().Cookies.DeleteCookieNamed("i_user");
                Thread.Sleep(1000);
            }
            catch (Exception)
            {
            }
        }
        public void GetInfo(IWebDriver driver, FbAccount data)
        {
            if (IsStop() || !processActionsData.GetInfo)
            {
                return;
            }
            data.Description += ", Get Info";
            if (string.IsNullOrEmpty(data.Token))
            {
                try
                {
                    Token(driver, data);
                }
                catch { }
            }
            if (!string.IsNullOrEmpty(data.Token))
            {
                FBGraph fBGraph = new FBGraph();
                fBGraph.GetCookieContainerFromDriver(driver);
                FBProfile info = fBGraph.GetInfo(data.Token);
                try
                {
                    data.Name = info.name;
                    data.Gender = info.gender;
                    data.Email = info.email;
                    data.DOB = info.birthday;
                    fbAccountViewModel.getAccountDao().UpdateInfo(data);
                }
                catch (Exception)
                {
                }
            }
            int status = 1;
            if (data.Status == "Die")
            {
                status = 0;
            }
            fbAccountViewModel.getAccountDao().updateStatus(data.UID, data.Description, status);
        }
        public void Token(IWebDriver driver, FbAccount data)
        {
            if (!IsStop())
            {
                data.Description = "Get Token";
                string twoFA = (data.TwoFA ?? "").Trim();
                data.Token = WebFBTool.GetTokenLocator(driver, twoFA);
                //data.Token = WebFBTool.GetToken(driver,false);
                fbAccountViewModel.getAccountDao().UpdateToken(data.UID, data.Token);
            }
        }
        public void StartProfile(IWebDriver driver, FbAccount data)
        {
            bool flag = false;
            if (processActionsData.TurnOnPM)
            {
                TurnOnPM(driver, data);
            }
            if (
                processActionsData.ProfileDeleteData ||
                processActionsData.LogoutAlDevices ||
                processActionsData.LockTime ||
                processActionsData.NewInfo ||
                processActionsData.PublicPost ||
                processActionsData.TurnOnTwoFA ||
                processActionsData.NewPassword ||
                processActionsData.CheckIn ||
                processActionsData.Marketplace ||
                processActionsData.ActivtiyLog
            )
            {
                processActionsData.ProfileConfig = GetCacheConfig<ProfileConfig>("profile:config");
                flag = true;
            }
            else if (!processActionsData.ContactRemovePhone && !processActionsData.ContactRemoveMail && !processActionsData.ContactRemoveInstragram && !processActionsData.ContactPrimary)
            {
                return;
            }
            IProfileViewModel profileViewModel = DIConfig.Get<IProfileViewModel>();
            if (flag)
            {
                profileViewModel.Start(this, driver, data);
            }
            if (!IsStop() && processActionsData.CheckIn && processActionsData.ProfileConfig != null && processActionsData.ProfileConfig.NewInfo != null)
            {
                WebFBTool.Checkin(driver, processActionsData.ProfileConfig.NewInfo.CheckIn);
            }
            if (!IsStop() && processActionsData.Marketplace && processActionsData.ProfileConfig != null && processActionsData.ProfileConfig.NewInfo != null)
            {
                WebFBTool.Marketplace(driver, processActionsData.ProfileConfig.NewInfo.Marketplace);
            }
            if (!IsStop() && processActionsData.LockTime)
            {
                profileViewModel.LockTime();
            }
            if (!IsStop() && processActionsData.ProfileDeleteData)
            {
                profileViewModel.Delete();
            }
            if (!IsStop() && processActionsData.ActivtiyLog)
            {
                profileViewModel.ActivityLog();
            }
            if (!IsStop() && processActionsData.TurnOnTwoFA)
            {
                profileViewModel.TwoFA();
            }
            if (processActionsData.ContactRemoveMail || processActionsData.ContactRemovePhone || processActionsData.ContactPrimary || processActionsData.ContactRemoveInstragram)
            {
                Contact(driver, data);
            }
            if (!IsStop() && processActionsData.NewInfo)
            {
                profileViewModel.Info();
            }
            if (!IsStop() && processActionsData.PublicPost)
            {
                profileViewModel.PublicPost();
            }
            if (!IsStop() && processActionsData.NewPassword)
            {
                profileViewModel.Password();
            }
            if (!IsStop() && processActionsData.LogoutAlDevices)
            {
                profileViewModel.LogoutAllDevice();
            }
            int status = 1;
            if (data.Status == "Die")
            {
                status = 0;
            }
            fbAccountViewModel.getAccountDao().updateStatus(data.UID, data.Description, status);
        }

        public void TurnOnPM(IWebDriver driver, FbAccount data)
        {
            try
            {
                driver.Navigate().GoToUrl("https://facebook.com/" + data.UID);
            }
            catch (Exception) { }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            int counter = 6;
            bool isWorking = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='See options']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-label='See Options']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("//div[@aria-haspopup='menu']")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return;
            }
            Thread.Sleep(1000);
            counter = 6;
            isWorking = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//span[contains(text(),'Turn on professional mode')]")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div[1]/div[1]/div/div/div/div/div[1]/div/div[1]/div/div[2]")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            Thread.Sleep(1000);
            counter = 6;
            isWorking = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    driver.FindElement(By.XPath("//div[@aria-label='Turn on']")).Click();
                    isWorking = true;
                }
                catch (Exception) { }
                if (!isWorking)
                {
                    try
                    {
                        driver.FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[4]/div/div/div[1]/div/div[2]/div/div/div/div/div/div/div[2]/div[2]/div/div/div[1]/div[3]/div/div[2]/div")).Click();
                        isWorking = true;
                    }
                    catch (Exception) { }
                }
            } while (!isWorking && counter-- > 0);
            if (!isWorking)
            {
                return;
            }
            Thread.Sleep(3000);
        }
        public void Contact(IWebDriver driver, FbAccount data)
        {
            if (processActionsData.ContactPrimary && processActionsData.ContactConfig != null)
            {
                if (processActionsData.ContactConfig.Hotmail && processActionsData.ContactConfig.HotmailConfig != null)
                {
                    if (!string.IsNullOrEmpty(processActionsData.ContactConfig.HotmailConfig.ApiKey) || hotmailListArr.Length > 0)
                    {
                        bool flag = true;
                        if (processActionsData.ContactConfig.NewLayout)
                        {
                            flag = IsNewLayout(driver);
                        }
                        string text = "";
                        int num = 4;
                        if (flag)
                        {
                            text = GetHotmailFromList();
                            if (string.IsNullOrEmpty(text))
                            {
                                do
                                {
                                    Thread.Sleep(500);
                                    text = LocalData.GetHotmail(processActionsData.ContactConfig.HotmailConfig.ApiKey, processActionsData.ContactConfig.HotmailConfig.DomainName);
                                }
                                while (string.IsNullOrEmpty(text) && !IsStop() && num-- > 0);
                            }
                        }
                        if (!string.IsNullOrEmpty(text))
                        {
                            string[] array = text.Split('|');
                            string text2 = array[0].Trim();
                            string password = array[1].Trim();
                            bool flag2 = false;
                            if ((!processActionsData.ContactConfig.NewLayout) ? MobileChangePrimary(driver, text2, data.Password) : ChangePrimary(driver, text2, data.Password))
                            {
                                string text3 = "";
                                num = 5;
                                do
                                {
                                    text3 = LocalData.GetHotmailCodeVerify(text2, password);
                                }
                                while (string.IsNullOrEmpty(text3) && !IsStop() && num-- > 0);
                                if (!string.IsNullOrEmpty(text3))
                                {
                                    bool flag3 = false;
                                    if ((!processActionsData.ContactConfig.NewLayout) ? MobileVerifyCodeEmailPrimary(driver, text2, text3) : MailVerifyCod(driver, text2, text3))
                                    {
                                        data.Email = text2;
                                        data.MailPass = text;
                                        fbAccountViewModel.getAccountDao().updateEmail(data.UID, text2);
                                        fbAccountViewModel.getAccountDao().updateEmailPass(data.UID, text);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (processActionsData.ContactConfig.Yandex && processActionsData.ContactConfig.YandexConfig != null)
                {
                    bool flag = true;
                    if (processActionsData.ContactConfig.NewLayout)
                    {
                        flag = IsNewLayout(driver);
                    }
                    if (flag)
                    {
                        var mailPrimary = GetYandexPrimary();
                        if (!string.IsNullOrEmpty(mailPrimary))
                        {
                            if ((!processActionsData.ContactConfig.NewLayout) ? MobileChangePrimary(driver, mailPrimary, data.Password) : ChangePrimary(driver, mailPrimary, data.Password))
                            {
                                AddYandexToWaitingCode(mailPrimary);
                                string code = "";
                                int counter = 20;
                                do
                                {
                                    if (IsStop())
                                    {
                                        break;
                                    }
                                    Thread.Sleep(1000);
                                    code = GetYandexCodeVerifyFromArr(mailPrimary);
                                } while (string.IsNullOrEmpty(code) && counter-- > 0);
                                if (!string.IsNullOrEmpty(code))
                                {
                                    if (processActionsData.ContactConfig.NewLayout)
                                    {
                                        MailVerifyCod(driver, mailPrimary, code);
                                    }
                                    else
                                    {
                                        MobileVerifyCodeEmailPrimary(driver, mailPrimary, code);
                                    }
                                    data.Email = mailPrimary;
                                    data.MailPass = "";

                                    fbAccountViewModel.getAccountDao().updateEmail(data.UID, mailPrimary);
                                }
                            }
                        }
                    }
                }
            }
            if (processActionsData.ContactRemoveInstragram)
            {
                NewLayOutRemoveInstragram(driver, data.UID, data.Password);
            }
            if (processActionsData.ContactRemovePhone)
            {
                if (processActionsData.ContactConfig != null && processActionsData.ContactConfig.NewLayout)
                {
                    NewLayOutRemovePhone(driver, data.Password);
                }
                else
                {
                    MobileFBTool.MobileRemove(driver, data.Password);
                }
            }
            if (!processActionsData.ContactRemoveMail)
            {
            }
        }
        public void GetYandexCodeFromDriver()
        {
            do
            {
                Thread.Sleep(1000);
                for (int i = 0; i < yandexVerifyArr.Count; i++)
                {
                    string yandexMail = yandexVerifyArr[i].MailPrimary;
                    if (!string.IsNullOrEmpty(yandexMail) && !yandexVerifyArr[i].Status)
                    {
                        string code = GetYandexCodeForVerify(yandexMail);
                        if (!string.IsNullOrEmpty(code))
                        {
                            yandexVerifyArr[i].Code = code;
                        }
                        yandexVerifyArr[i].Status = true;
                        break;
                    }
                }
            } while (!IsStop());
        }
        public string GetYandexCodeForVerify(string primaryEmail)
        {
            int counter = 15;
            string code = "";
            bool isFirst = true;
            do
            {
                if (IsStop())
                {
                    break;
                }
                Thread.Sleep(1000);
                code = WebFBTool.GetVerifyPrimaryContactFromYandex(driverYandex, primaryEmail, processActionsData.ContactConfig.YandexConfig.Protocol, isFirst);

                isFirst = false;
            } while (string.IsNullOrEmpty(code) && counter-- > 0);

            return code;
        }
        public string GetYandexCodeVerifyFromArr(string mailYandex)
        {
            string code = "";
            for (int i = 0; i < yandexVerifyArr.Count; i++)
            {
                if (yandexVerifyArr[i].MailPrimary.ToLower().Trim().Contains(mailYandex.ToLower().Trim()))
                {
                    code = yandexVerifyArr[i].Code;
                    if (!string.IsNullOrEmpty(code))
                    {
                        yandexVerifyArr[i].Code = "";
                        yandexVerifyArr[i].MailPrimary = "";
                        yandexVerifyArr[i].Status = false;
                    }
                    else
                    {
                        Thread.Sleep(2000);
                    }
                    break;
                }
            }

            return code;
        }
        public void AddYandexToWaitingCode(string mailYandex)
        {
            for (int i = 0; i < yandexVerifyArr.Count; i++)
            {
                if (yandexVerifyArr[i].Status == false && string.IsNullOrEmpty(yandexVerifyArr[i].MailPrimary))
                {
                    yandexVerifyArr[i].Code = "";
                    yandexVerifyArr[i].Status = false;
                    yandexVerifyArr[i].MailPrimary = mailYandex;
                    break;
                }
            }
        }
        public string GetYandexPrimary()
        {
            int index = GetVerifyYandexIndex();
            string email = processActionsData.ContactConfig.YandexConfig.Mail;
            string prefix = processActionsData.ContactConfig.YandexConfig.TextFix;
            string s = email.Replace("@", "+" + index + prefix + "@");

            return s;
        }
        public void StartFriends(IWebDriver driver, FbAccount data)
        {
            if (processActionsData.AddFriends || processActionsData.AcceptFriends || processActionsData.AddFriendsByUID || processActionsData.BackupFriends)
            {
                processActionsData.FriendsConfig = GetCacheConfig<FriendsConfig>("friend:config");
                IFriendsViewModel friendsViewModel = DIConfig.Get<IFriendsViewModel>();
                friendsViewModel.Start(this, driver, data);
                if (!IsStop() && processActionsData.AddFriends)
                {
                    friendsViewModel.Add();
                }
                if (!IsStop() && processActionsData.AcceptFriends)
                {
                    friendsViewModel.Accept();
                }
                if (!IsStop() && processActionsData.AddFriendsByUID)
                {
                    friendsViewModel.AddByUID();
                }
                if (!IsStop() && processActionsData.BackupFriends)
                {
                    friendsViewModel.Backup();
                }
                int status = 1;
                if (data.Status == "Die")
                {
                    status = 0;
                }
                fbAccountViewModel.getAccountDao().updateStatus(data.UID, data.Description, status);
            }
        }

        public void StartPage(IWebDriver driver, FbAccount data, bool isNoSwitchPage = false)
        {
            if (!processActionsData.CreatePage && !processActionsData.FollowPage && !processActionsData.BackupPage && !processActionsData.PageCreateReel && !processActionsData.AutoScrollPage)
            {
                return;
            }
            processActionsData.PageConfig = GetCacheConfig<PageConfig>("page:config");
            IPageViewModel pageViewModel = DIConfig.Get<IPageViewModel>();
            pageViewModel.Start(this, driver, data);
            if (!IsStop() && processActionsData.CreatePage)
            {
                pageViewModel.Create();
            }
            if (!IsStop() && processActionsData.BackupPage)
            {
                if (string.IsNullOrEmpty(data.Token))
                {
                    Token(driver, data);
                }
                pageViewModel.Backup();
            }
            if (!IsStop() && processActionsData.FollowPage)
            {
                pageViewModel.Follow();
            }
            if (!IsStop() && processActionsData.PageCreateReel)
            {
                pageViewModel.CreateReel(isNoSwitchPage);
            }
            if (!IsStop() && processActionsData.AutoScrollPage)
            {
                StartAutoScroll(driver, data);
            }
            int status = 1;
            if (data.Status == "Die")
            {
                status = 0;
            }
            fbAccountViewModel.getAccountDao().updateStatus(data.UID, data.Description, status);
        }

        public void StartAutoScroll(IWebDriver driver, FbAccount account)
        {
            try
            {
                account.Status = "Auto Scroll";
                SetGridDataRowStatus(account);
                fbAccountViewModel.getAccountDao().updateStatus(account.UID, account.Description, 1);

                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                Random rnd = new Random();

                while (!IsStop())
                {
                    try
                    {
                        // Check if Comment button is visible on the page (Primary condition)
                        bool hasComment = false;
                        try
                        {
                            // Using broad XPaths for Comment buttons commonly found on Facebook
                            hasComment = driver.FindElements(By.XPath("//div[@aria-label='Leave a comment' or @aria-label='Bình luận' or @aria-label='Comment' or @aria-label='បញ្ចេញមតិ']")).Any(e => e.Displayed);
                        }
                        catch { }

                        if (!hasComment)
                        {
                            // If Comment button is not visible, scroll down to find next post
                            int scrollAmount = rnd.Next(300, 600);
                            js.ExecuteScript($"window.scrollBy(0, {scrollAmount});");

                            // Quick wait to allow content to load
                            Thread.Sleep(rnd.Next(1000, 2000));
                            continue; // Skip and check again
                        }

                        // Interaction Logic: Like and Comment if configured
                        try
                        {
                            var cache = cacheViewModel.GetCacheDao().Get("newsfeed:config");
                            if (cache != null && cache.Value != null)
                            {
                                var confStr = cache.Value.ToString();
                                if (!string.IsNullOrEmpty(confStr))
                                {
                                    var newsfeedObj = JsonConvert.DeserializeObject<NewsFeedConfig>(confStr);
                                    if (newsfeedObj != null && newsfeedObj.NewsFeed != null)
                                    {
                                        bool doLike = newsfeedObj.NewsFeed.React.Like;
                                        bool doComment = newsfeedObj.NewsFeed.React.Comment;
                                        bool doRandom = newsfeedObj.NewsFeed.React.Random;

                                        if (doRandom)
                                        {
                                            doLike = rnd.Next(0, 2) == 0;
                                            doComment = rnd.Next(0, 2) == 0;
                                        }

                                        // Like interaction
                                        if (doLike)
                                        {
                                            WebFBTool.LikePost(driver);
                                            Thread.Sleep(rnd.Next(1000, 2000));
                                        }

                                        // Comment interaction - One comment per post
                                        if (doComment && !string.IsNullOrEmpty(newsfeedObj.NewsFeed.Comments))
                                        {
                                            var comments = newsfeedObj.NewsFeed.Comments.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                            if (comments.Length > 0)
                                            {
                                                string randomComment = comments[rnd.Next(comments.Length)];
                                                bool success = WebFBTool.PostComment(driver, randomComment);
                                                if (success)
                                                {
                                                    Thread.Sleep(rnd.Next(2000, 4000));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch { }

                        // After interacting with ONE post, scroll significantly to move to the NEXT post
                        int nextPostScroll = rnd.Next(800, 1200);
                        js.ExecuteScript($"window.scrollBy(0, {nextPostScroll});");

                        // Simulate reading time before looking for next interaction
                        Thread.Sleep(rnd.Next(3000, 5000));
                    }
                    catch { }
                }
            }
            catch (Exception) { }
        }

        public void StartNewsFeedConfig(IWebDriver driver, FbAccount data)
        {
            if (processActionsData.ReadMessenger || processActionsData.PostTimeline || processActionsData.PlayNewsFeed || processActionsData.ReadNotification || processActionsData.AutoScroll)
            {
                processActionsData.NewsFeed = GetCacheConfig<NewsFeedConfig>("newsfeed:config");
                INewsFeedViewModel newsFeedViewModel = DIConfig.Get<INewsFeedViewModel>();
                newsFeedViewModel.Start(this, driver, data);
                if (processActionsData.ReadNotification && !IsStop())
                {
                    newsFeedViewModel.ReadNotification();
                }
                if (processActionsData.ReadMessenger && !IsStop())
                {
                    newsFeedViewModel.Messenger();
                }
                if (processActionsData.PostTimeline && !IsStop())
                {
                    newsFeedViewModel.Timeline();
                }
                if (processActionsData.PlayNewsFeed && !IsStop())
                {
                    newsFeedViewModel.Play();
                }
                if (processActionsData.AutoScroll && !IsStop())
                {
                    StartAutoScroll(driver, data);
                }
                int status = 1;
                if (data.Status == "Die")
                {
                    status = 0;
                }
                fbAccountViewModel.getAccountDao().updateStatus(data.UID, data.Description, status);
            }
        }

        public void StartGroup(IWebDriver driver, FbAccount data)
        {
            if (processActionsData.IsLeaveGroup || processActionsData.IsJoinGroup || processActionsData.IsBackupGroup || processActionsData.IsViewGroup || processActionsData.AutoScrollGroup)
            {
                processActionsData.GroupConfig = GetCacheConfig<GroupConfig>("group:config");
                IGroupViewModel groupViewModel = DIConfig.Get<IGroupViewModel>();
                groupViewModel.Start(this, driver, data);
                if (processActionsData.IsLeaveGroup && !IsStop())
                {
                    data.Description += ", Leave Groups";
                    groupViewModel.LeaveGroup();
                }
                if (processActionsData.IsJoinGroup && !IsStop())
                {
                    data.Description += ", Join Groups";
                    groupViewModel.JoinGroup();
                }
                if (processActionsData.IsBackupGroup && !IsStop())
                {
                    data.Description += ", Backup Groups";
                    groupViewModel.BackupGroup();
                }
                if (processActionsData.IsViewGroup && !IsStop())
                {
                    data.Description += ", View Groups";
                    groupViewModel.ViewGroup();
                }
                if (processActionsData.AutoScrollGroup && !IsStop())
                {
                    StartAutoScroll(driver, data);
                }
                int status = 1;
                if (data.Status == "Die")
                {
                    status = 0;
                }
                fbAccountViewModel.getAccountDao().updateStatus(data.UID, data.Description, status);
            }
        }

        public void ChangeLanguage(IWebDriver driver)
        {
            if (processActionsData.EnglishUS && !FBTool.IsEnglish(driver))
            {
                //MobileFBTool.ChangeLanguage(driver);
                WebFBTool.ChangeLanguage(driver,false);
                Thread.Sleep(1000);
            }
        }

        public void StartShareVideo(IWebDriver driver, FbAccount data)
        {
            IShareViewModel shareViewModel = DIConfig.Get<IShareViewModel>();
            shareViewModel.Start(this, data);
            shareUrlIndex = 0;
            if (processActionsData.IsShareToGroup || processActionsData.IsShareToTimeline)
            {
                try
                {
                    int width = driver.Manage().Window.Size.Width;
                    int height = driver.Manage().Window.Size.Height;
                    driver.Manage().Window.Size = new System.Drawing.Size(width, height + 150);
                }
                catch (Exception)
                {
                }
                
                if (shareUrlArr != null)
                {
                    foreach (var url in shareUrlArr)
                    {
                        if (string.IsNullOrEmpty(url)) continue;
                        if (IsStop()) break;

                        if (!IsStop())
                        {
                            string comment = GetComment();
                            shareViewModel.ShareWatchVideo(driver, url, comment);
                            if (processActionsData.Share.Groups != null)
                            {
                                shareViewModel.ShareWatchDelay(driver, processActionsData.Share.Groups.WatchBeforeShare);
                            }
                        }
                        if (processActionsData.IsShareToTimeline && !IsStop())
                        {
                            string caption = GetCaption();
                            shareViewModel.ShareToTimeline(driver, caption);
                        }
                        if (processActionsData.IsShareToGroup && !IsStop())
                        {
                            bool isAPIShare = false;
                            Application.Current.Dispatcher.Invoke(delegate
                            {
                                try
                                {
                                    isAPIShare = chbShareByGraph.IsChecked.Value;
                                }
                                catch (Exception) { }
                            });
                            shareViewModel.ShareToGroup(driver, isAPIShare);
                        }
                        if (!IsStop())
                        {
                            if (processActionsData.Share.Groups != null)
                            {
                                shareViewModel.ShareWatchDelay(driver, processActionsData.Share.Groups.WatchAfterShare);
                            }
                        }
                    }
                }
            }
            if (processActionsData.IsShareWebsite && !IsStop())
            {
                shareViewModel.ShareWebsite(driver);
            }
            if (processActionsData.IsShareProfilePage && !IsStop())
            {
                if (string.IsNullOrEmpty(data.Token))
                {
                    Token(driver, data);
                }
                shareViewModel.ShareProfilePage(driver);
            }
            int status = 1;
            if (data.Status == "Die")
            {
                status = 0;
            }
            fbAccountViewModel.getAccountDao().updateStatus(data.UID, data.Description, status);
        }

        //public void StartShareVideoLDPlayer(FbAccount data)
        //{
        //    if (shareUrlArr != null)
        //    {
        //        foreach (var url in shareUrlArr)
        //        {
        //            if (string.IsNullOrEmpty(url)) continue;
        //            if (IsStop()) break;

        //            string caption = GetCaption();

        //            if (processActionsData.IsShareToTimeline || processActionsData.IsShareByGraph)
        //            {
        //                if (processActionsData.IsShareByGraph)
        //                {
        //                    ldPlayerTool.ShareByGraphLink(url, caption);
        //                }
        //                else
        //                {
        //                    ldPlayerTool.ShareLink(url, caption);
        //                }
        //                Thread.Sleep(5000);
        //            }

        //            if (processActionsData.IsShareToGroup)
        //            {
        //                if (shareGroupIDArr != null)
        //                {
        //                    foreach (var gid in shareGroupIDArr)
        //                    {
        //                        if (IsStop()) break;
        //                        ldPlayerTool.ShareToGroup(gid, url, caption);
        //                        Thread.Sleep(5000);
        //                    }
        //                }
        //            }

        //            if (processActionsData.IsShareProfilePage || processActionsData.IsShareWebsite)
        //            {
        //                // Similar to timeline but maybe with different interaction
        //                ldPlayerTool.ShareLink(url, caption);
        //                Thread.Sleep(5000);
        //            }
        //        }
        //    }
        //}

        public void StartShareVideoLDPlayer(FbAccount data)
        {
            // 1) URLs: use your existing shareUrlArr if it is already prepared,
            //    otherwise build it from processActionsData.Share.Urls
            var urls = shareUrlArr;

            if (urls == null || urls.Length == 0)
            {
                var urlsText = processActionsData?.Share?.Urls;
                if (!string.IsNullOrWhiteSpace(urlsText))
                {
                    urls = urlsText
                        .Split(new[] { '\n', '\r', ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToArray();
                }
            }

            if (urls == null || urls.Length == 0)
                return;

            // 2) Group IDs (for ShareToGroup)
            string groupIdsText = processActionsData?.Share?.ProfilePage?.GroupIDs;
            string[] shareGroupIDArr = null;

            if (!string.IsNullOrWhiteSpace(groupIdsText))
            {
                shareGroupIDArr = groupIdsText
                    .Split(new[] { '\n', '\r', ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();
            }

            // 3) Main loop
            foreach (var url in urls)
            {
                if (IsStop()) break;
                if (string.IsNullOrWhiteSpace(url)) continue;

                string caption = GetCaption(); // keep your existing method

                // Share to Timeline / Graph
                if (processActionsData.IsShareToTimeline || processActionsData.IsShareByGraph)
                {
                    if (processActionsData.IsShareByGraph)
                        ldPlayerTool.ShareByGraphLink(url, caption);
                    else
                        ldPlayerTool.ShareLink(url, caption);

                    Thread.Sleep(5000);
                }

                // Share to Groups
                if (processActionsData.IsShareToGroup)
                {
                    if (shareGroupIDArr != null && shareGroupIDArr.Length > 0)
                    {
                        foreach (var gid in shareGroupIDArr)
                        {
                            if (IsStop()) break;
                            if (string.IsNullOrWhiteSpace(gid)) continue;

                            ldPlayerTool.ShareToGroup(gid, url, caption);
                            Thread.Sleep(5000);
                        }
                    }
                    else
                    {
                        // Optional: helpful status if config missing
                        data.Status = "ShareToGroup enabled but Share.ProfilePage.GroupIDs is empty";
                        SetGridDataRowStatus(data);
                    }
                }

                // Share to Profile/Page or Website (your current behavior shares link normally)
                if (processActionsData.IsShareProfilePage || processActionsData.IsShareWebsite)
                {
                    ldPlayerTool.ShareLink(url, caption);
                    Thread.Sleep(5000);
                }
            }
        }


        public void BackupGroups(IWebDriver driver, FbAccount data)
        {
            IGroupsDao groupsDao = DIConfig.Get<IGroupViewModel>().GetGroupsDao();
            if (processActionsData.GroupConfig.Backup.IsBrowser)
            {
                WebFBTool.GetTotalGroup(driver);

                string groupIds = WebFBTool.GetGroupIDs(driver);
                if(!string.IsNullOrEmpty(data.OldGroupIds) && !string.IsNullOrEmpty(data.GroupIDs))
                {
                    data.OldGroupIds += ",";
                }
                data.OldGroupIds += data.GroupIDs;
                bool isBackupNewGroup = false;
                try
                {
                    isBackupNewGroup = processActionsData.GroupConfig.Backup.BackupNewGroup;
                }
                catch (Exception) { }
                if(isBackupNewGroup)
                {
                    string newGroupIds = "";
                    string[] gArr = groupIds.Split(',');
                    for(int i = 0; i < gArr.Count(); i ++)
                    {
                        string g_id = gArr[i].ToString().Trim();
                        if (data.OldGroupIds.Contains(g_id))
                        {
                            continue;
                        }
                        if(!string.IsNullOrEmpty(newGroupIds))
                        {
                            newGroupIds += ",";
                        }
                        newGroupIds += g_id;
                    }
                    data.GroupIDs = newGroupIds;
                } else
                {
                    data.OldGroupIds = "";
                    data.GroupIDs = groupIds;
                }

                int num = 0;
                string[] array = data.GroupIDs.Split(',');

                if (string.IsNullOrEmpty(data.GroupIDs))
                {
                    data.TotalGroup = 0;
                } else
                {
                    num = (data.TotalGroup = array.Length);
                }
                fbAccountViewModel.getAccountDao().UpdateGroup(data.UID, data.TotalGroup, data.GroupIDs,data.OldGroupIds);
                groupsDao.deleteByUID(data.UID);

                for (int i = 0; i < num; i++)
                {
                    int pending = 0;
                    string groupId = array[i].Trim();
                    Groups data2 = new Groups
                    {
                        UID = data.UID,
                        GroupId = groupId,
                        Name = "",
                        Member = 0,
                        Pending = pending,
                        Status = 1,
                        Check_Pending = 0
                    };
                    groupsDao.add(data2);
                }
            }
            else
            {
                if (!processActionsData.GroupConfig.Backup.IsToken)
                {
                    return;
                }
                data.Token = WebFBTool.GetTokenLocator(driver, data.TwoFA);
                if (string.IsNullOrEmpty(data.Token) || !data.Token.StartsWith("EAA"))
                {
                    return;
                }
                FBGraph fBGraph = new FBGraph();
                fBGraph.GetCookieContainerFromDriver(driver);
                ObservableCollection<FBGroupNodes> groupByUID = fBGraph.GetGroupByUID(data.UID, data.Token);
                try
                {
                    data.TotalGroup = groupByUID.Count;
                    fbAccountViewModel.getAccountDao().UpdateGroup(data.UID, data.TotalGroup);
                    if (data.TotalGroup <= 0)
                    {
                        return;
                    }
                    groupsDao.deleteByUID(data.UID);
                    foreach (FBGroupNodes item in groupByUID)
                    {
                        string viewer_post_status = item.viewer_post_status;
                        int pending2 = 0;
                        string id = item.id;
                        if (viewer_post_status.Contains("CAN_POST_AFTER_APPROVAL"))
                        {
                            pending2 = 1;
                        }
                        Groups data3 = new Groups
                        {
                            UID = data.UID,
                            GroupId = item.id,
                            Name = item.name,
                            Member = item.group_member_profiles.count,
                            Pending = pending2,
                            Status = 1,
                            Check_Pending = 0
                        };
                        groupsDao.add(data3);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public void TimeDelay(int timeDelay)
        {
            while (!IsStop())
            {
                timeDelay--;
                Thread.Sleep(1000);
                if (timeDelay-- <= 0)
                {
                    break;
                }
            }
        }

        public string GetSourceProfile()
        {
            string result = "";
            try
            {
                if (profileArr == null)
                {
                    profileArr = LocalData.GetFiles(processActionsData.ProfileConfig.NewInfo.SourceProfile);
                    if (profileArr.Length == 0)
                    {
                        return "";
                    }
                }
            }
            catch (Exception)
            {
            }
            int num = 0;
            try
            {
                var cache = cacheViewModel.GetCacheDao().Get("profile:config:profile_index");
                if (cache != null && cache.Value != null)
                {
                    num = int.Parse(cache.Value.ToString());
                }
            }
            catch (Exception)
            {
            }
            try
            {
                if (num >= profileArr.Length)
                {
                    num = 0;
                }
                result = profileArr[num];
                cacheViewModel.GetCacheDao().Set("profile:config:profile_index", (num + 1).ToString() ?? "");
            }
            catch (Exception)
            {
            }
            return result;
        }

        public string GetSourceCover()
        {
            string result = "";
            try
            {
                if (coverArr == null)
                {
                    coverArr = LocalData.GetFiles(processActionsData.ProfileConfig.NewInfo.SourceCover);
                    if (coverArr.Length == 0)
                    {
                        return "";
                    }
                }
            }
            catch (Exception)
            {
            }
            int num = 0;
            try
            {
                var cache = cacheViewModel.GetCacheDao().Get("profile:config:cover_index");
                if (cache != null && cache.Value != null)
                {
                    num = int.Parse(cache.Value.ToString());
                }
            }
            catch (Exception)
            {
            }
            try
            {
                if (num >= coverArr.Length)
                {
                    num = 0;
                }
                result = coverArr[num];
                cacheViewModel.GetCacheDao().Set("profile:config:cover_index", (num + 1).ToString() ?? "");
            }
            catch (Exception)
            {
            }
            return result;
        }
        public string GetSourceTimeline(string source = "", bool deleteAfterPost = true)
        {
            string result = "";
            if (!string.IsNullOrEmpty(source))
            {
                if (System.IO.File.Exists(source)) return source;

                if (Directory.Exists(source))
                {
                    try
                    {
                        string[] arr = LocalData.GetFile(source);
                        if (arr.Length > 0)
                        {
                            result = arr[random.Next(0, arr.Length)];
                        }
                    }
                    catch (Exception) { }

                    return result;
                }
            }
            try
            {
                if (System.IO.File.Exists(processActionsData.NewsFeed.Timeline.SourceFolder))
                {
                    return processActionsData.NewsFeed.Timeline.SourceFolder;
                }

                if (timelineArr == null)
                {
                    timelineArr = LocalData.GetFiles(processActionsData.NewsFeed.Timeline.SourceFolder);
                    if (timelineArr.Length == 0)
                    {
                        return "";
                    }
                }
            }
            catch (Exception)
            {
            }
            if (timelineIndex >= timelineArr.Length)
            {
                return "";
            }
            try
            {
                result = timelineArr[timelineIndex];
                timelineIndex++;
            }
            catch (Exception)
            {
            }
            return result;
        }
        public string GetPhotoPostTimeline_b()
        {
            string result = "";
            try
            {
                if (timelineArr == null)
                {
                    timelineArr = LocalData.GetFiles(processActionsData.NewsFeed.Timeline.SourceFolder);
                    if (timelineArr.Length == 0)
                    {
                        return "";
                    }
                }
            }
            catch (Exception)
            {
            }
            int num = 0;
            try
            {
                var cache = cacheViewModel.GetCacheDao().Get("newsfeed:config:timeline_index");
                if (cache != null && cache.Value != null)
                {
                    int.TryParse(cache.Value.ToString(), out num);
                }
            }
            catch (Exception)
            {
            }
            try
            {
                if (num >= timelineArr.Length)
                {
                    num = 0;
                }
                result = timelineArr[num];
                cacheViewModel.GetCacheDao().Set("newsfeed:config:timeline_index", (num + 1).ToString() ?? "");
            }
            catch (Exception)
            {
            }
            return result;
        }

        public string GetSourceReelVideo(string sourceFolder = "")
        {
            string result = "";
            if (!string.IsNullOrEmpty(sourceFolder))
            {
                if (System.IO.File.Exists(sourceFolder)) return sourceFolder;
                if (Directory.Exists(sourceFolder))
                {

                    try
                    {
                        string[] arr = LocalData.GetFile(sourceFolder);
                        if (arr.Length > 0)
                        {
                            result = arr[random.Next(0, arr.Length)];
                        }
                    }
                    catch (Exception) { }

                    return result;
                }
            }
            try
            {
                if (System.IO.File.Exists(processActionsData.PageConfig.CreateReel.SourceFolder))
                {
                    return processActionsData.PageConfig.CreateReel.SourceFolder;
                }

                if (sourceReelVideoArr == null)
                {
                    sourceReelVideoArr = LocalData.GetFiles(processActionsData.PageConfig.CreateReel.SourceFolder);
                    if (sourceReelVideoArr.Length == 0)
                    {
                        return "";
                    }
                }
            }
            catch (Exception)
            {
            }
            if (reelVideoIndex >= sourceReelVideoArr.Length)
            {
                return "";
            }
            try
            {
                result = sourceReelVideoArr[reelVideoIndex];
                reelVideoIndex++;
            }
            catch (Exception)
            {
            }
            return result;
        }

        public string GetReelCaption()
        {
            string result = "";
            try
            {
                if (reelCaptionArr == null)
                {
                    reelCaptionArr = processActionsData.PageConfig.CreateReel.Captions.Split('\n');
                    if (reelCaptionArr.Length == 0)
                    {
                        return "";
                    }
                }
            }
            catch (Exception)
            {
            }
            int num = 0;
            try
            {
                var cache = cacheViewModel.GetCacheDao().Get("page:config:reel_caption_index");
                if (cache != null && cache.Value != null)
                {
                    int.TryParse(cache.Value.ToString(), out num);
                }
            }
            catch (Exception)
            {
            }
            try
            {
                if (num >= reelCaptionArr.Length)
                {
                    num = 0;
                }
                result = reelCaptionArr[num];
                cacheViewModel.GetCacheDao().Set("page:config:reel_caption_index", (num + 1).ToString() ?? "");
            }
            catch (Exception)
            {
            }
            return result;
        }

        //public string[] GetJoinGroupIDArr()
        //{
        //    string[] result = null;
        //    var cache = cacheViewModel.GetCacheDao().Get("group:config:group_ids");
        //    string text = cache?.Value?.ToString() ?? "";
        //    if (!string.IsNullOrEmpty(text))
        //    {
        //        try
        //        {
        //            result = text.Split('\n');
        //        }
        //        catch (Exception)
        //        {
        //        }
        //    }
        //    return result;
        //}
        public string[] GetJoinGroupIDArr()
        {
            var cache = cacheViewModel.GetCacheDao().Get("group:config:group_ids");
            var text = cache != null && cache.Value != null ? cache.Value.ToString() : "";

            if (string.IsNullOrWhiteSpace(text))
                return new string[0];

            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var list = new List<string>();
            foreach (var line in lines)
            {
                var url = NormalizeFacebookGroupUrl(line);
                if (!string.IsNullOrWhiteSpace(url))
                    list.Add(url);
            }

            return list.ToArray();
        }

        private string NormalizeFacebookGroupUrl(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            var s = input.Trim();

            // Fix weird pasted: "https://www.facebook.com/groups/https://www.facebook.com/groups/123"
            var idx = s.LastIndexOf("/groups/", StringComparison.OrdinalIgnoreCase);
            if (idx > 0)
                s = "https://www.facebook.com" + s.Substring(idx);

            // Ensure protocol for cases like "www.facebook.com/groups/123"
            if (s.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                s = "https://" + s;

            // Remove query + fragment early
            var q = s.IndexOf("?", StringComparison.Ordinal);
            if (q >= 0) s = s.Substring(0, q);

            var hash = s.IndexOf("#", StringComparison.Ordinal);
            if (hash >= 0) s = s.Substring(0, hash);

            s = s.Trim();

            // If it's only "/groups/123" or "groups/123"
            if (s.StartsWith("/")) s = s.Substring(1);
            if (s.StartsWith("groups/", StringComparison.OrdinalIgnoreCase))
                s = "https://www.facebook.com/" + s;

            // If user pasted only ID/slug (no http)
            if (!s.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !s.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return "https://www.facebook.com/groups/" + s.Trim().Trim('/');
            }
            var m = System.Text.RegularExpressions.Regex.Match(
                s,
                @"facebook\.com\/groups\/([^\/\?\#]+)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (m.Success)
            {
                var groupKey = m.Groups[1].Value.Trim().Trim('/');
                return "https://www.facebook.com/groups/" + groupKey;
            }

            // fallback: return cleaned input
            return s.TrimEnd('/');
        }

        public string[] GetHotmailFromListArr()
        {
            string[] result = null;
            var cache = cacheViewModel.GetCacheDao().Get("contact:config:mail_list");
            string text = cache?.Value?.ToString() ?? "";
            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    result = text.Split('\n');
                }
                catch (Exception)
                {
                }
            }
            return result;
        }

        public string GetJoinGroupID()
        {
            string result = "";
            try
            {
                if (joinGroupIDArr != null)
                {
                    try
                    {
                        string value = "";
                        if (joinGroupIDArr.Length != 0 && joinGroupIDIndex < joinGroupIDArr.Length)
                        {
                            result = joinGroupIDArr[joinGroupIDIndex];
                            joinGroupIDIndex++;
                            value = string.Join("\n", joinGroupIDArr.Skip(joinGroupIDIndex));
                        }
                        cacheViewModel.GetCacheDao().Set("group:config:group_ids", value);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception)
            {
            }
            return result;
        }
        public string GetHotmailFromList()
        {
            string result = "";
            try
            {
                if (hotmailListArr != null)
                {
                    try
                    {
                        string value = "";
                        if (hotmailListArr.Length != 0 && hotmailListIndex < hotmailListArr.Length)
                        {
                            result = hotmailListArr[hotmailListIndex];
                            hotmailListIndex++;
                            value = string.Join("\n", hotmailListArr.Skip(hotmailListIndex));
                        }
                        cacheViewModel.GetCacheDao().Set("contact:config:mail_list", value);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception)
            {
            }

            return result;
        }

        public string GetRandomString(int digit = 10)
        {
            string text = "";
            char[] array = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrtuvwxyz0123456789@#$".ToCharArray();
            int maxValue = array.Length - 1;
            Random random = new Random();
            for (int i = 0; i < digit; i++)
            {
                text += array[random.Next(0, maxValue)];
            }
            return text;
        }

        public void ProcessWaitingGetCodeFromYandex()
        {
            int num = 10;
            do
            {
                Thread.Sleep(1000);
                if (num > 0)
                {
                }
            }
            while (!IsStop());
        }

        public bool MobileVerifyCodeEmailPrimary(IWebDriver driver, string primaryEmail, string code)
        {
            bool flag = false;
            bool result = false;
            int num = 6;
            while (!IsStop())
            {
                Thread.Sleep(500);
                try
                {
                    ((ISearchContext)driver).FindElement(By.XPath("//input[@name='code']")).SendKeys(code + Keys.Enter);
                    flag = true;
                    result = true;
                }
                catch (Exception)
                {
                }
                if (flag || num-- <= 0)
                {
                    break;
                }
            }
            if (!flag)
            {
                return result;
            }
            num = 10;
            flag = false;
            while (!IsStop())
            {
                Thread.Sleep(500);
                try
                {
                    ((ISearchContext)driver).FindElement(By.XPath("//div[contains(text(),'" + primaryEmail + "')]")).Click();
                    flag = true;
                }
                catch (Exception)
                {
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div/div/div/div[3]/div/div[2]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (flag || num-- <= 0)
                {
                    break;
                }
            }
            if (!flag)
            {
                return result;
            }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            num = 6;
            flag = false;
            while (!IsStop())
            {
                Thread.Sleep(500);
                try
                {
                    ((ISearchContext)driver).FindElement(By.XPath("//button[@value='Make Primary']")).Click();
                    flag = true;
                }
                catch (Exception)
                {
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//button[@value='Make primary']")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div[1]/div/div/div[2]/div/button")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (flag || num-- <= 0)
                {
                    break;
                }
            }
            if (flag)
            {
                Thread.Sleep(2000);
            }
            return result;
        }

        public string GetVerifyPrimaryContactFromYandex(IWebDriver driver, bool isFirst = true)
        {
            string result = "";
            string protocol = processActionsData.ContactConfig.YandexConfig.Protocol;
            if (!string.IsNullOrEmpty(protocol))
            {
                try
                {
                    driver.Navigate().GoToUrl(protocol);
                }
                catch (Exception)
                {
                }
                if (!isFirst)
                {
                    try
                    {
                        Thread.Sleep(500);
                        driver.Navigate().Refresh();
                        Thread.Sleep(1000);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            else
            {
                try
                {
                    ((ISearchContext)driver).FindElement(By.XPath("//a[@data-title='Social']")).Click();
                }
                catch (Exception)
                {
                }
                try
                {
                    driver.Navigate().Refresh();
                }
                catch (Exception)
                {
                }
            }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            bool flag = false;
            int num = 10;
            do
            {
                Thread.Sleep(600);
                IWebElement val = null;
                try
                {
                    val = ((ISearchContext)driver).FindElement(By.PartialLinkText("New email address added on Facebook"));
                    flag = true;
                }
                catch (Exception)
                {
                }
                if (!flag)
                {
                    try
                    {
                        val = ((ISearchContext)driver).FindElement(By.XPath("//span[text() = 'New email address added on Facebook']"));
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (flag)
                {
                    try
                    {
                        Actions val2 = new Actions(driver);
                        val2.MoveToElement(val).Click().Perform();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            while (!flag && num-- > 0);
            if (!flag)
            {
                return "";
            }
            Thread.Sleep(2000);
            flag = false;
            num = 7;
            do
            {
                Thread.Sleep(1000);
                try
                {
                    flag = true;
                }
                catch (Exception)
                {
                }
                if (flag)
                {
                    continue;
                }
                bool flag2 = false;
                IWebElement val3 = null;
                try
                {
                    val3 = ((ISearchContext)driver).FindElement(By.XPath("//div[text() = 'next']"));
                    flag2 = true;
                }
                catch (Exception)
                {
                }
                if (!flag2)
                {
                    try
                    {
                        val3 = ((ISearchContext)driver).FindElement(By.XPath("//*[@href='#message/*']"));
                        flag2 = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag2)
                {
                    try
                    {
                        val3 = ((ISearchContext)driver).FindElement(By.XPath("//div[text() = 'Next']"));
                        flag2 = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag2)
                {
                    try
                    {
                        val3 = ((ISearchContext)driver).FindElement(By.ClassName("PrevNext__label--iNM4a"));
                        flag2 = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag2)
                {
                    try
                    {
                        val3 = ((ISearchContext)driver).FindElement(By.ClassName("PrevNext__label--2fGPJ"));
                        flag2 = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag2)
                {
                    try
                    {
                        val3 = ((ISearchContext)driver).FindElement(By.Id("arrow"));
                        flag2 = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag2)
                {
                    try
                    {
                        val3 = ((ISearchContext)driver).FindElement(By.XPath("//div[contains(text(),'Next email')]"));
                        flag2 = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag2)
                {
                    try
                    {
                        val3 = ((ISearchContext)driver).FindElement(By.ClassName("mail-Message-PrevNext-DirectionTitle"));
                        flag2 = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (flag2)
                {
                    try
                    {
                        Actions val4 = new Actions(driver);
                        val4.MoveToElement(val3).Click().Perform();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            while (!flag && num-- > 0);
            if (flag)
            {
                Thread.Sleep(1500);
                string text = "";
                try
                {
                    IWebElement val5 = ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[2]/div[2]/div[7]/div/div[3]/div[3]/div[3]/div/div[5]/div[1]/div/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                    text = val5.GetAttribute("innerHTML");
                }
                catch (Exception) { }
                if (string.IsNullOrEmpty(text))
                {
                    try
                    {
                        IWebElement val5 = ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[3]/div[2]/div[7]/div/div[3]/div/div[2]/div/main/div[7]/div/div/div/div/div/div[2]/div/article/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        text = val5.GetAttribute("innerHTML");
                    }
                    catch (Exception)
                    {
                    }
                }
                if (string.IsNullOrEmpty(text))
                {
                    try
                    {
                        IWebElement val5 = ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[3]/div[2]/div[7]/div/div[2]/div/div[2]/div/main/div[7]/div/div/div/div/div/div[2]/div/article/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        text = val5.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(text))
                {
                    try
                    {
                        IWebElement val5 = ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[2]/div[2]/div[7]/div/div[3]/div[2]/div[3]/main/div[5]/div/div/div/div/div/div[2]/div/article/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        text = val5.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(text))
                {
                    try
                    {
                        IWebElement val5 = ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[2]/div[2]/div[7]/div/div[3]/div[3]/div[3]/div/div[5]/div/div[1]/div/div/div[1]/div[5]/div/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        text = val5.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(text))
                {
                    try
                    {
                        IWebElement val5 = ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[2]/div[2]/div[8]/div/div[3]/div[3]/div[3]/div/div[5]/div/div[1]/div/div/div[1]/div[5]/div/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        text = val5.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(text))
                {
                    try
                    {
                        IWebElement val5 = ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[2]/div[7]/div/div[3]/div[3]/div[3]/div/div[5]/div/div[1]/div/div[1]/div[5]/div/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        text = val5.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(text))
                {
                    try
                    {
                        IWebElement val5 = ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[2]/div[2]/div[8]/div/div[3]/div[3]/div[3]/div/div[5]/div[1]/div/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        text = val5.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(text))
                {
                    try
                    {
                        IWebElement val5 = ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[2]/div[7]/div/div[3]/div[3]/div[3]/div/div[5]/div[1]/div/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        text = val5.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(text))
                {
                    try
                    {
                        IWebElement val5 = ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[2]/div[7]/div/div[3]/div[3]/div[2]/div[5]/div/div[1]/div/div[1]/div[5]/div/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        text = val5.GetAttribute("innerHTML");
                    }
                    catch (Exception) { }
                }
                if (string.IsNullOrEmpty(text))
                {
                    try
                    {
                        IWebElement val5 = ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[2]/div[6]/div/div[3]/div[3]/div[2]/div[5]/div[1]/div/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        text = val5.GetAttribute("innerHTML");
                    }
                    catch (Exception)
                    {
                    }
                }
                if (string.IsNullOrEmpty(text))
                {
                    try
                    {
                        IWebElement val5 = ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[2]/div[7]/div/div[3]/div[3]/div[2]/div/div[5]/div/div[1]/div/div[1]/div[5]/div/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        text = val5.GetAttribute("innerHTML");
                    }
                    catch (Exception)
                    {
                    }
                }
                if (string.IsNullOrEmpty(text))
                {
                    try
                    {
                        IWebElement val5 = ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[2]/div[7]/div/div[3]/div[3]/div[2]/div/div[5]/div[1]/div/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        text = val5.GetAttribute("innerHTML");
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!string.IsNullOrEmpty(text))
                {
                    result = text.Substring(50, 5);
                }
                else
                {
                    try
                    {
                        IWebElement val5 = ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[2]/div[5]/div/div[3]/div[3]/div[2]/div[5]/div[1]/div/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                        text = val5.GetAttribute("innerHTML");
                    }
                    catch (Exception)
                    {
                    }
                    if (!string.IsNullOrEmpty(text))
                    {
                        result = text.Substring(50, 5);
                    }
                    else
                    {
                        try
                        {
                            IWebElement val5 = ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[2]/div[7]/div/div[3]/div[3]/div[2]/div[5]/div[1]/div/div[3]/div/div/table/tbody/tr/td/table/tbody/tr[4]/td[2]/table/tbody/tr[8]/td/span"));
                            text = val5.GetAttribute("innerHTML");
                        }
                        catch (Exception)
                        {
                        }
                        if (!string.IsNullOrEmpty(text))
                        {
                            result = text.Substring(50, 5);
                        }
                    }
                }
            }
            return result;
        }

        public bool MobileChangePrimary(IWebDriver driver, string mail, string password)
        {
            bool flag = false;
            bool flag2 = true;
            string text = "";
            try
            {
                text = driver.Url;
            }
            catch (Exception)
            {
            }
            if (text.Contains("free."))
            {
                try
                {
                    driver.Navigate().GoToUrl("https://free.facebook.com/settings/email/?refid=70");
                }
                catch (Exception)
                {
                }
                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(1000);
                flag2 = false;
            }
            else
            {
                try
                {
                    driver.Navigate().GoToUrl("https://m.facebook.com/settings/account/");
                }
                catch (Exception)
                {
                }
                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(1000);
                try
                {
                    ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div/div/div[2]/div[2]/a")).Click();
                }
                catch (Exception)
                {
                }
            }
            if (IsStop())
            {
                return false;
            }
            flag = false;
            try
            {
                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(1000);
                ((ISearchContext)driver).FindElement(By.XPath("//div[contains(text(),'Add Email Address')]")).Click();
                flag = true;
            }
            catch (Exception)
            {
            }
            if (IsStop())
            {
                return false;
            }
            if (!flag)
            {
                try
                {
                    ((ISearchContext)driver).FindElement(By.XPath("//*[contains(text(),'Add Email Address')]")).Click();
                    flag = true;
                }
                catch (Exception) { }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//*[contains(text(),'Add email address')]")).Click();
                        flag = true;
                    }
                    catch (Exception) { }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//*[contains(text(),'Add Email address')]")).Click();
                        flag = true;
                    }
                    catch (Exception) { }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//*[contains(text(),'Change email address')]")).Click();
                        flag = true;
                    }
                    catch (Exception) { }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div/div/div/div[4]/div[3]/div")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("/html/body/div/div/div[3]/div/div[1]/div/div[2]/a[1]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            if (IsStop())
            {
                return false;
            }
            int num = 6;
            flag = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    ((ISearchContext)driver).FindElement(By.XPath("//input[@name='save_password']")).SendKeys(password);
                    flag = true;
                }
                catch (Exception)
                {
                }
            }
            while (!flag && num-- > 0);
            if (IsStop())
            {
                return false;
            }
            num = 4;
            flag = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    ((ISearchContext)driver).FindElement(By.XPath("//input[@name='email']")).SendKeys(mail + Keys.Enter);
                    flag = true;
                    Thread.Sleep(1000);
                }
                catch (Exception)
                {
                }
            }
            while (!flag && num-- > 0);
            if (IsStop())
            {
                return false;
            }
            flag = false;
            if (flag2)
            {
                num = 10;
                flag = false;
                do
                {
                    Thread.Sleep(500);
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//div[contains(text(),'" + mail + "')]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                    if (!flag)
                    {
                        try
                        {
                            ((ISearchContext)driver).FindElement(By.XPath("//button[@value='Confirm']")).Click();
                            flag = true;
                        }
                        catch (Exception) { }
                    }
                    if (!flag)
                    {
                        try
                        {
                            ((ISearchContext)driver).FindElement(By.XPath("//*[contains(text(),'Confirm email address')]")).Click();
                            flag = true;
                        }
                        catch (Exception) { }
                    }
                    if (!flag)
                    {
                        try
                        {
                            ((ISearchContext)driver).FindElement(By.XPath("//*[contains(text(),'Confirm Email Address')]")).Click();
                            flag = true;
                        }
                        catch (Exception) { }
                    }
                }
                while (!flag && num-- > 0);
            }
            else
            {
                num = 10;
                flag = false;
                do
                {
                    Thread.Sleep(500);
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//*[contains(text(),'Confirm Email Address')]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                    if (!flag)
                    {
                        try
                        {
                            ((ISearchContext)driver).FindElement(By.XPath("//*[contains(text(),'Confirm email address')]")).Click();
                            flag = true;
                        }
                        catch (Exception)
                        {
                        }
                    }
                    if (!flag)
                    {
                        try
                        {
                            ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div[2]/div/div/div[1]/div[2]/div/div[1]/span[2]/div/a")).Click();
                            flag = true;
                        }
                        catch (Exception)
                        {
                        }
                    }
                    if (!flag)
                    {
                        try
                        {
                            ((ISearchContext)driver).FindElement(By.XPath("/html/body/div/div/div[3]/div/div[2]/div/div[1]/div[2]/div/div[1]/span[2]/div/a")).Click();
                            flag = true;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                while (!flag && num-- > 0);
            }
            if (IsStop())
            {
                return false;
            }
            if (flag)
            {
                FBTool.WaitingPageLoading(driver);
                Thread.Sleep(1000);
                flag = false;
                try
                {
                    ((ISearchContext)driver).FindElement(By.XPath("//*[contains(text(),'Resend confirmation email')]")).Click();
                    flag = true;
                }
                catch (Exception)
                {
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[4]/div/div[1]/div/div[3]/form/div/button")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                    if (!flag)
                    {
                        try
                        {
                            ((ISearchContext)driver).FindElement(By.XPath("/html/body/div/div/div[3]/div[2]/div[1]/div[3]/a")).Click();
                            flag = true;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                if (flag)
                {
                    Thread.Sleep(5000);
                }
            }
            return flag;
        }

        public bool MailVerifyCod(IWebDriver driver, string primaryEmail, string code)
        {
            bool flag = false;
            int num = 6;
            while (!IsStop())
            {
                Thread.Sleep(500);
                try
                {
                    ((ISearchContext)driver).FindElement(By.XPath("//input[@autocomplete='one-time-code']")).SendKeys(code + Keys.Enter);
                    flag = true;
                }
                catch (Exception)
                {
                }
                if (flag || num-- <= 0)
                {
                    break;
                }
            }
            if (!flag)
            {
                return false;
            }
            try
            {
                code = code.Trim();
            }
            catch (Exception) { }
            if (flag)
            {
                Thread.Sleep(1000);
                flag = false;
                try
                {
                    ((ISearchContext)driver).FindElement(By.XPath("//span[contains(text(),'Next')]")).Click();
                    flag = true;
                }
                catch (Exception)
                {
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//span[contains(text(),'next')]")).Click();
                        flag = true;
                    }
                    catch (Exception) { }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//*[contains(text(),'Next')]")).Click();
                        flag = true;
                    }
                    catch (Exception) { }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//*[contains(text(),'next')]")).Click();
                        flag = true;
                    }
                    catch (Exception) { }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div[2]/div/div[4]/div[3]/div/div/div/div/div/div/div/div/div[1]/div/span/span")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div[2]/div/div[6]/div[3]/div/div/div/div/div/div/div/div/div[1]/div/span")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            if (flag)
            {
                Thread.Sleep(4000);
            }
            return flag;
        }

        public bool IsNewLayout(IWebDriver driver)
        {
            bool flag = false;
            try
            {
                driver.Navigate().GoToUrl("https://accountscenter.facebook.com/personal_info/contact_points/?contact_point_type=email&dialog_type=add_contact_point");
            }
            catch (Exception)
            {
            }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            int num = 2;
            do
            {
                Thread.Sleep(500);
                try
                {
                    ((ISearchContext)driver).FindElement(By.Name("noform"));
                    flag = true;
                }
                catch (Exception)
                {
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[3]/div[2]/div[4]/div[3]/div/div[2]/div/div/div/div/label/div[1]/div/div[3]/div/input"));
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            while (!flag && num-- > 0);
            return flag;
        }

        public bool ChangePrimary(IWebDriver driver, string mail, string password)
        {
            bool flag = false;
            try
            {
                driver.Navigate().GoToUrl("https://accountscenter.facebook.com/personal_info/contact_points");
            }
            catch (Exception)
            {
            }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            int num = 4;
            flag = false;
            do
            {
                Thread.Sleep(1000);
                try
                {
                    ((ISearchContext)driver).FindElement(By.XPath("//span[contains(text(),'Add new contact')]")).Click();
                    flag = true;
                }
                catch (Exception)
                {
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//span[contains(text(),'Add New contact')]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//span[contains(text(),'Add New Contact')]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//div[contains(text(),'Add new contact')]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//div[contains(text(),'Add New contact')]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//div[contains(text(),'Add New Contact')]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[3]/div[2]/div[4]/div/div/div[2]/div/div/div/div/div[3]/div[1]/div")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[5]/div[2]/div[1]/div/div/div[2]/div/div/div/div/div[3]/div[1]/div/div/div/div/span")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            while (!flag && num-- > 0);
            if (!flag)
            {
                return false;
            }
            num = 4;
            flag = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    ((ISearchContext)driver).FindElement(By.XPath("//span[contains(text(),'Add email')]")).Click();
                    flag = true;
                }
                catch (Exception)
                {
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//span[contains(text(),'Add Email')]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//span[contains(text(),'add email')]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[5]/div[2]/div[1]/div/div/div[2]/div[2]/div/div/div/div[2]/div[1]/div/div/div/div/span")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//div[contains(text(),'Add email')]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//div[contains(text(),'Add Email')]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//div[contains(text(),'add email')]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[3]/div[2]/div[4]/div/div/div[2]/div[2]/div/div/div/div[2]/div[1]/div/div/div/div/div")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            while (!flag && num-- > 0);
            if (!flag)
            {
                return false;
            }
            num = 4;
            flag = false;
            do
            {
                Thread.Sleep(500);
                try
                {
                    ((ISearchContext)driver).FindElement(By.Name("noform")).Click();
                    flag = true;
                }
                catch (Exception)
                {
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[3]/div[2]/div[4]/div[3]/div/div[2]/div/div/div/div/label/div[1]/div/div[3]/div/input")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            while (!flag && num-- > 0);
            if (IsStop())
            {
                return false;
            }
            flag = false;
            try
            {
                ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[3]/div[2]/div[4]/div[2]/div/div/div/div/input")).SendKeys(mail);
                flag = true;
            }
            catch (Exception)
            {
            }
            if (!flag)
            {
                try
                {
                    ((ISearchContext)driver).FindElement(By.XPath("//input[@dir='ltr']")).SendKeys(mail);
                    flag = true;
                }
                catch (Exception)
                {
                }
            }
            if (flag)
            {
                Thread.Sleep(1000);
                flag = false;
                try
                {
                    ((ISearchContext)driver).FindElement(By.XPath("//span[contains(text(),'Next')]")).Click();
                    flag = true;
                }
                catch (Exception)
                {
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[4]/div[3]/div/div/div/div/div/div/div/div/div[1]/div/span")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            if (flag)
            {
                Thread.Sleep(5000);
            }
            return flag;
        }

        public bool NewLayOutRemovePhone(IWebDriver driver, string password)
        {
            bool flag = false;
            bool flag2 = false;
            try
            {
                driver.Navigate().GoToUrl("https://accountscenter.facebook.com/personal_info/contact_points");
            }
            catch (Exception)
            {
            }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            int num = 4;
            do
            {
                Thread.Sleep(500);
                string text = "/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[5]/div[2]/div[1]/div/div/div[2]/div/div/div/div";
                try
                {
                    ((ISearchContext)driver).FindElement(By.XPath(text + "/div[2]")).Click();
                    flag = true;
                }
                catch (Exception)
                {
                }
                if (flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath(text + "/div[4]")).Click();
                        flag2 = true;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            while (!flag && !IsStop() && num-- > 0);
            if (!flag || IsStop())
            {
                return false;
            }
            flag = false;
            num = 4;
            do
            {
                Thread.Sleep(1000);
                try
                {
                    ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[3]/div[2]/div[4]/div/div/div[2]/div[1]/div/div/div/div/div[1]/div/div[2]")).Click();
                    flag = true;
                }
                catch (Exception)
                {
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[5]/div[2]/div[1]/div/div/div[2]/div[1]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            while (!flag && !IsStop() && num-- > 0);
            if (IsStop())
            {
                return false;
            }
            flag = false;
            num = 4;
            do
            {
                Thread.Sleep(1000);
                bool flag3 = true;
                try
                {
                    ((ISearchContext)driver).FindElement(By.XPath("//span[contains(., 'Delete From Facebook')]")).Click();
                    flag = true;
                }
                catch (Exception)
                {
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//span[contains(., 'Delete from Facebook')]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//span[contains(., 'Delete from facebook')]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//span[contains(., 'delete from facebook')]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//span[contains(., 'Delete number')]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//span[contains(., 'Delete Number')]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//span[contains(., 'Delete')]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            while (!flag && !IsStop() && num-- > 0);
            if (!flag || IsStop())
            {
                return false;
            }
            flag = false;
            num = 4;
            do
            {
                Thread.Sleep(1000);
                try
                {
                    ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[4]/div[1]/div/div[2]/div/div/div/div[3]/div[2]/div/div")).Click();
                    flag = true;
                }
                catch (Exception)
                {
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//span[contains(., 'Delete')]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[5]/div[1]/div/div[2]/div/div/div/div[3]/div[2]/div/div/div")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            while (!flag && !IsStop() && num-- > 0);
            Thread.Sleep(1000);
            if (flag)
            {
                num = 6;
                IWebElement val = null;
                do
                {
                    Thread.Sleep(500);
                    try
                    {
                        val = ((ISearchContext)driver).FindElement(By.XPath("//input[@type='password']"));
                    }
                    catch (Exception)
                    {
                    }
                }
                while (val == null && !IsStop() && num-- > 0);
                if (val != null)
                {
                    try
                    {
                        val.SendKeys(password);
                        Thread.Sleep(500);
                    }
                    catch (Exception)
                    {
                    }
                    bool isSubmit = false;
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//span[contains(., 'Submit')]")).Click();
                        isSubmit = true;
                    }
                    catch (Exception)
                    {
                    }
                    if (!isSubmit)
                    {
                        try
                        {
                            ((ISearchContext)driver).FindElement(By.XPath("//span[contains(text(), 'Submit')]")).Click();
                            isSubmit = true;
                        }
                        catch (Exception)
                        {
                        }
                    }
                    if (!isSubmit)
                    {
                        try
                        {
                            ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[6]/div[1]/div/div[2]/div/div/div/div/div/div/div[4]/div[3]/div/div/div/div/div/div/div/div")).Click();
                            isSubmit = true;
                        }
                        catch (Exception)
                        {
                        }
                    }
                    if (isSubmit)
                    {

                        Thread.Sleep(2000);
                    }
                }
                Thread.Sleep(2000);
                if (flag2)
                {
                    NewLayOutRemovePhone(driver, password);
                }
            }

            return flag;
        }

        public bool NewLayOutRemoveInstragram(IWebDriver driver, string uid, string password)
        {
            //IL_01b5: Unknown result type (might be due to invalid IL or missing references)
            //IL_01bc: Expected O, but got Unknown
            bool flag = false;
            bool flag2 = false;
            try
            {
                driver.Navigate().GoToUrl("https://accountscenter.facebook.com/accounts/" + uid + "/remove/?accountType=INSTAGRAM");
            }
            catch (Exception)
            {
            }
            FBTool.WaitingPageLoading(driver);
            Thread.Sleep(1000);
            flag = false;
            int num = 4;
            do
            {
                Thread.Sleep(1000);
                try
                {
                    ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[4]/div[3]/div/div/div/div/div/div/div/div/div[1]/div/span/span")).Click();
                    flag = true;
                }
                catch (Exception)
                {
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("//span[contains(text(), 'Continue')]")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            while (!flag && !IsStop() && num-- > 0);
            if (IsStop())
            {
                return false;
            }
            flag = false;
            num = 4;
            IWebElement val = null;
            do
            {
                Thread.Sleep(1000);
                bool flag3 = true;
                try
                {
                    val = ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div[2]/div/div[6]/div[3]/div/div/div/div/div[2]/div[2]/div"));
                    flag = true;
                }
                catch (Exception)
                {
                }
                if (!flag)
                {
                    try
                    {
                        val = ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div[2]/div/div[4]/div[3]/div/div/div/div/div[2]/div[2]/div"));
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        val = ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[6]/div[3]/div/div/div[2]/div/div[1]/div[1]/div"));
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!flag)
                {
                    try
                    {
                        val = ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[4]/div[3]/div/div/div/div/div[1]/div[1]/div"));
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            while (!flag && !IsStop() && num-- > 0);
            if (!flag || IsStop())
            {
                return false;
            }
            try
            {
                Actions val2 = new Actions(driver);
                val2.MoveToElement(val).Click().Perform();
            }
            catch (Exception)
            {
            }
            flag = false;
            num = 4;
            do
            {
                Thread.Sleep(1000);
                try
                {
                    ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[4]/div[3]/div/div/div/div/div[1]/div[1]/div")).Click();
                    flag = true;
                }
                catch (Exception)
                {
                }
                if (!flag)
                {
                    try
                    {
                        ((ISearchContext)driver).FindElement(By.XPath("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[2]/div/div/div/div/div/div/div/div[6]/div[3]/div/div/div/div/div[1]/div[1]/div")).Click();
                        flag = true;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            while (!flag && !IsStop() && num-- > 0);
            if (flag)
            {
                Thread.Sleep(3000);
            }
            Thread.Sleep(1000);
            return flag;
        }
    }
}