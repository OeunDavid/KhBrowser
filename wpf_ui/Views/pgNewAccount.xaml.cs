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
using WpfUI;
using WpfUI.ViewModels;
using WpfUI.Views;

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
        }

        private void passUIDPassword_Click(object sender, RoutedEventArgs e)
        {
            PassAccount("UID|Password");
        }
        private void passUIDPasswordTwoFA_Click(object sender, RoutedEventArgs e)
        {
            PassAccount("UID|Password|2FA");
        }
        private void passUIDPasswordCookie_Click(object sender, RoutedEventArgs e)
        {
            PassAccount("UID|Password|Cookie");
        }
        private void passUIDPasswordTwoFACookie_Click(object sender, RoutedEventArgs e)
        {
            PassAccount("UID|Password|2FA|Cookie");
        }
        private void passUIDPasswordMailMailPass_Click(object sender, RoutedEventArgs e)
        {
            PassAccount("UID|Password|Mail|MailPass");
        }
        private void passUIDPasswordCookieMailMailPass_Click(object sender, RoutedEventArgs e)
        {
            PassAccount("UID|Password|Cookie|Mail|MailPass");
        }
        private void passUIDPassword2FAMailMailPass_Click(object sender, RoutedEventArgs e)
        {
            PassAccount("UID|Password|2FA|Mail|MailPass");
        }
        private void passUIDPassword2FACookieMailMailPass_Click(object sender, RoutedEventArgs e)
        {
            PassAccount("UID|Password|2FA|Cookie|Mail|MailPass");
        }
        public void PassAccount(string format)
        {
            string str = Clipboard.GetText();
            if (string.IsNullOrEmpty(str))
            {
                return;
            }
            string[] arr = str.Split('\n');
            if (arr.Length == 0)
            {
                return;
            }
            ObservableCollection<FbAccount> accounts = new ObservableCollection<FbAccount>();
            for (int i = 0; i < arr.Length; i++)
            {
                string[] arrAcc = arr[i].Trim().Split('|');
                string uid = "", password = "", twoFA = "", cookie = "", mail="", mailPass="";

                try
                {
                    uid = arrAcc[0].Trim();
                    password = arrAcc[1].Trim();
                    switch (format)
                    {
                        case "UID|Password":

                            break;
                        case "UID|Password|2FA":
                            twoFA = arrAcc[2].Trim();
                            break;
                        case "UID|Password|Cookie":
                            cookie = arrAcc[2];
                            break;
                        case "UID|Password|2FA|Cookie":
                            twoFA = arrAcc[2].Trim();
                            cookie = arrAcc[3];
                            break;
                        case "UID|Password|Mail|MailPass":
                            mail = arrAcc[2].Trim();
                            mailPass = arrAcc[3].Trim();
                            break;
                        case "UID|Password|Cookie|Mail|MailPass":
                            cookie = arrAcc[2];
                            mail = arrAcc[3].Trim();
                            mailPass = arrAcc[4].Trim();
                            break;
                        case "UID|Password|2FA|Mail|MailPass":
                            twoFA = arrAcc[2].Trim();
                            mail = arrAcc[3].Trim();
                            mailPass = arrAcc[4].Trim();
                            break;
                        case "UID|Password|2FA|Cookie|Mail|MailPass":
                            twoFA = arrAcc[2].Trim();
                            cookie = arrAcc[3];
                            mail = arrAcc[4].Trim();
                            mailPass = arrAcc[5].Trim();
                            break;
                    }
                }
                catch (Exception) { }
                if (!string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(password))
                {
                    twoFA = Regex.Replace(twoFA, @"\s+", "");
                    if (twoFA.Length != 32)
                    {
                        twoFA = "";
                    }
                    if (!string.IsNullOrEmpty(mail) && !string.IsNullOrEmpty(mailPass))
                    {
                        mailPass = mail + "|" + mailPass;
                    } else
                    {
                        mail = "";
                        mailPass = "";
                    }
                    accounts.Add(new FbAccount()
                    {
                        Key= i+1,
                        UID= uid,
                        Password= password,
                        TwoFA= twoFA,
                        Cookie= cookie,
                        MailPass= mailPass
                    });
                }
            }
            gdNewAccount.ItemsSource = accounts;
        }
        public int GetStoreId()
        {
            int id = 0;
            try
            {
                id = Convert.ToInt32(ddlStore.SelectedValue.ToString());
            }
            catch (Exception) { }

            return id;
        }

        private void btnStartSaveAccount_Click(object sender, RoutedEventArgs e)
        {
            string str = Clipboard.GetText();
            if (string.IsNullOrEmpty(str))
            {
                return;
            }
            string[] arr = str.Split('\n');
            if (arr.Length == 0)
            {
                return;
            }
            List<Account> acc = fbAccountViewModel.getAccountDao().listAccount();
            Dictionary<string, int> accExist = new Dictionary<string, int>();
            Account data;
            foreach (Account a in acc)
            {
                accExist[a.UID] = 1;
            }
            int storeId = GetStoreId();
            if (storeId == 0)
            {
                MessageBox.Show("Please choose store!");
                return;
            }
            string sql = "";
            bool isNew = false, isUpdate= false, noNeedUpdateExist= chbNoNeedUpdateExistData.IsChecked.Value;
            foreach(FbAccount fbAcc in gdNewAccount.ItemsSource)
            {
                string uid = "", password = "", twoFA = "", cookie = "",mailPass= "";
                try
                {
                    uid = fbAcc.UID;
                    password = fbAcc.Password;
                    twoFA = fbAcc.TwoFA;
                    cookie = fbAcc.Cookie;
                    mailPass = fbAcc.MailPass;
                }
                catch (Exception) { }
                if (!string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(password))
                {
                    twoFA = Regex.Replace(twoFA, @"\s+", "");
                    if (accExist.ContainsKey(uid))
                    {
                        if (!noNeedUpdateExist)
                        {
                            fbAccountViewModel.getAccountDao().updateSecurity(uid, password, twoFA, storeId, mailPass);
                            
                            isUpdate = true;
                        } else
                        {
                            // need update store only, I'm don't want design UI (only me use it)
                            string pathUpdateStore = LocalData.GetPath() + "\\update_store.txt";
                            if (File.Exists(pathUpdateStore))
                            {
                                fbAccountViewModel.getAccountDao().updateStore(uid, storeId);
                            }
                        }
                    }
                    else
                    {
                        if(!string.IsNullOrEmpty(sql))
                        {
                            sql += ",";
                        }
                        sql += "('" + uid + "','" + password + "','" + twoFA + "','" + cookie + "'," + storeId + ",'"+ mailPass +"')";
                        accExist.Add(uid, 1);
                    }
                }
            }
            if(!string.IsNullOrEmpty(sql))
            {
                sql = "INSERT INTO accounts(uid,password,twofa,cookie,store_id,mailPass) VALUES " + sql;

                fbAccountViewModel.getAccountDao().RunSQL(sql);

                isNew = true;
            }
            gdNewAccount.ItemsSource = null;
            MessageBox.Show("Your data has been save successfully.");

            if(isUpdate || (isNew && storeId == this.parentStoreId))
            {
                this.parentForm.loadDataToGrid();
            }
        }
    }
}
