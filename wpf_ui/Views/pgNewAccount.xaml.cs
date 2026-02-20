using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using ToolKHBrowser.ViewModels;
using ToolLib.Data;
using ToolLib.Tool;
using ToolKHBrowser.Views;

namespace ToolKHBrowser.Views
{
    /// <summary>
    /// Interaction logic for pgNewAccount.xaml
    /// </summary>
    public partial class pgNewAccount : Page
    {
        private IFbAccountViewModel fbAccountViewModel;
        private IStoreViewModel storeViewModel;

        private ObservableCollection<Store> storeData;
        private ObservableCollection<FbAccount> accountsToSave = new ObservableCollection<FbAccount>();

        private frmMain parentForm;
        private int parentStoreId;

        public pgNewAccount(frmMain parentForm, int storeId)
        {
            InitializeComponent();

            this.parentForm = parentForm;
            this.parentStoreId = storeId;

            fbAccountViewModel = DIConfig.Get<IFbAccountViewModel>();
            storeViewModel = DIConfig.Get<IStoreViewModel>();
            storeData = storeViewModel.listDataForGrid(1);

            ddlStore.ItemsSource = storeData;
            ddlStore.SelectedIndex = 0;

            gdNewAccount.ItemsSource = accountsToSave;

            // Initial visibility
            UpdateFieldVisibility();
        }

        private void ddlFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFieldVisibility();
        }

        private void UpdateFieldVisibility()
        {
            if (ddlFormat == null || pnl2FA == null) return;

            string selectedFormat = (ddlFormat.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "UID|Password";

            // Always show UID and Password
            pnlUID.Visibility = Visibility.Visible;
            pnlPassword.Visibility = Visibility.Visible;

            // Show others based on format
            pnl2FA.Visibility = selectedFormat.Contains("2FA") ? Visibility.Visible : Visibility.Collapsed;
            pnlCookie.Visibility = selectedFormat.Contains("Cookie") ? Visibility.Visible : Visibility.Collapsed;
            pnlMail.Visibility = selectedFormat.Contains("Mail") ? Visibility.Visible : Visibility.Collapsed;
            pnlPassMail.Visibility = selectedFormat.Contains("PassMail") ? Visibility.Visible : Visibility.Collapsed;
        }

        private void btnAddManual_Click(object sender, RoutedEventArgs e)
        {
            string uid = txtUID.Text.Trim();
            string pass = txtPassword.Text.Trim();
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("UID and Password are required!");
                return;
            }

            AddAccountToList(uid, pass, txt2FA.Text.Trim(), txtCookie.Text.Trim(), txtMail.Text.Trim(), txtPassMail.Text.Trim());

            // Clear inputs
            txtUID.Text = "";
            txtPassword.Text = "";
            txt2FA.Text = "";
            txtCookie.Text = "";
            txtMail.Text = "";
            txtPassMail.Text = "";
            txtUID.Focus();
        }

        private void AddAccountToList(string uid, string pass, string twofa, string cookie, string mail, string passmail)
        {
            string mailPass = mail;
            if (!string.IsNullOrEmpty(mail) && !string.IsNullOrEmpty(passmail))
                mailPass = mail + "|" + passmail;

            accountsToSave.Add(new FbAccount()
            {
                Key = accountsToSave.Count + 1,
                UID = uid,
                Password = pass,
                TwoFA = twofa,
                Cookie = cookie,
                MailPass = mailPass
            });
        }

        private void btnPasteClipboard_Click(object sender, RoutedEventArgs e)
        {
            string str = Clipboard.GetText();
            if (string.IsNullOrEmpty(str)) return;

            string selectedFormat = (ddlFormat.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "UID|Password";
            string[] lines = str.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string cleanLine = line.Trim();
                if (string.IsNullOrEmpty(cleanLine)) continue;

                char separator = '|';
                if (!cleanLine.Contains("|") && cleanLine.Contains("\t")) separator = '\t';
                else if (!cleanLine.Contains("|") && !cleanLine.Contains("\t") && cleanLine.Contains(":")) separator = ':';

                string[] arr = cleanLine.Split(separator);
                if (arr.Length < 2) continue;

                string uid = arr[0].Trim();
                string pass = arr[1].Trim();
                string twofa = "", cookie = "", mail = "", passmail = "";

                try
                {
                    switch (selectedFormat)
                    {
                        case "UID|Password|2FA":
                            if (arr.Length > 2) twofa = arr[2].Trim();
                            break;
                        case "UID|Password|Cookie":
                            if (arr.Length > 2) cookie = arr[2].Trim();
                            break;
                        case "UID|Password|2FA|Cookie":
                            if (arr.Length > 2) twofa = arr[2].Trim();
                            if (arr.Length > 3) cookie = arr[3].Trim();
                            break;
                        case "UID|Password|Mail|PassMail":
                            if (arr.Length > 2) mail = arr[2].Trim();
                            if (arr.Length > 3) passmail = arr[3].Trim();
                            break;
                        case "UID|Password|2FA|Mail|PassMail":
                            if (arr.Length > 2) twofa = arr[2].Trim();
                            if (arr.Length > 3) mail = arr[3].Trim();
                            if (arr.Length > 4) passmail = arr[4].Trim();
                            break;
                        case "UID|Password|Cookie|Mail|PassMail":
                            if (arr.Length > 2) cookie = arr[2].Trim();
                            if (arr.Length > 3) mail = arr[3].Trim();
                            if (arr.Length > 4) passmail = arr[4].Trim();
                            break;
                        case "UID|Password|2FA|Cookie|Mail|PassMail":
                            if (arr.Length > 2) twofa = arr[2].Trim();
                            if (arr.Length > 3) cookie = arr[3].Trim();
                            if (arr.Length > 4) mail = arr[4].Trim();
                            if (arr.Length > 5) passmail = arr[5].Trim();
                            break;
                    }
                } catch { }

                AddAccountToList(uid, pass, twofa, cookie, mail, passmail);
            }
        }

        private void btnClearGrid_Click(object sender, RoutedEventArgs e)
        {
            accountsToSave.Clear();
        }

        private void btnDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as FbAccount;
            if (item != null)
            {
                accountsToSave.Remove(item);
                // Re-index
                for (int i = 0; i < accountsToSave.Count; i++)
                    accountsToSave[i].Key = i + 1;
            }
        }

        public int GetStoreId()
        {
            try { return Convert.ToInt32(ddlStore.SelectedValue); } catch { return 0; }
        }

        private void btnStartSaveAccount_Click(object sender, RoutedEventArgs e)
        {
            if (accountsToSave.Count == 0)
            {
                MessageBox.Show("No accounts to save!");
                return;
            }

            int storeId = GetStoreId();
            if (storeId == 0)
            {
                MessageBox.Show("Please choose store!");
                return;
            }

            List<Account> existingAccs = fbAccountViewModel.getAccountDao().listAccount();
            HashSet<string> accExist = new HashSet<string>(existingAccs.Select(a => a.UID));
            
            string sql = "";
            bool isNew = false, isUpdate = false;
            bool noNeedUpdateExist = chbNoNeedUpdateExistData.IsChecked == true;

            foreach (var fbAcc in accountsToSave)
            {
                string uid = fbAcc.UID;
                string password = fbAcc.Password;
                string twoFA = Regex.Replace(fbAcc.TwoFA ?? "", @"\s+", "");
                string cookie = fbAcc.Cookie ?? "";
                string mailPass = fbAcc.MailPass ?? "";

                if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(password)) continue;

                if (accExist.Contains(uid))
                {
                    if (!noNeedUpdateExist)
                    {
                        fbAccountViewModel.getAccountDao().updateSecurity(uid, password, twoFA, storeId, mailPass);
                        isUpdate = true;
                    }
                    else
                    {
                        string pathUpdateStore = LocalData.GetPath() + "\\update_store.txt";
                        if (File.Exists(pathUpdateStore))
                        {
                            fbAccountViewModel.getAccountDao().updateStore(uid, storeId);
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(sql)) sql += ",";
                    sql += $"('{uid}','{password}','{twoFA}','{cookie}',{storeId},'{mailPass}')";
                    accExist.Add(uid);
                    isNew = true;
                }
            }

            if (!string.IsNullOrEmpty(sql))
            {
                sql = "INSERT INTO accounts(uid,password,twofa,cookie,store_id,mailPass) VALUES " + sql;
                fbAccountViewModel.getAccountDao().RunSQL(sql);
            }

            MessageBox.Show("Your data has been saved successfully.");
            accountsToSave.Clear();

            if (isUpdate || (isNew && storeId == this.parentStoreId))
            {
                this.parentForm.loadDataToGrid();
            }
        }
    }
}
