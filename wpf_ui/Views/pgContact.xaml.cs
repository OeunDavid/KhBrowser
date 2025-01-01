using Newtonsoft.Json;
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
using ToolKHBrowser.ViewModels;
using WpfUI.ViewModels;
using WpfUI;
using Emgu.CV.Flann;
using ToolKHBrowser.ToolLib.Data;

namespace ToolKHBrowser.Views
{
    /// <summary>
    /// Interaction logic for pgFriends.xaml
    /// </summary>
    public partial class pgContact : Page
    {
        ICacheViewModel cacheViewModel = DIConfig.Get<ICacheViewModel>();
        public pgContact()
        {
            InitializeComponent();

            LoadData();
        }
        public void LoadData()
        {
            try
            {
                var str = cacheViewModel.GetCacheDao().Get("contact:config").Value.ToString();
                if (!string.IsNullOrEmpty(str))
                {
                    ContactConfig contactObj = JsonConvert.DeserializeObject<ContactConfig>(str);
                    int index = Int32.Parse(cacheViewModel.GetCacheDao().Get("contact:config:index").Value.ToString());

                    txtYandexIndex.Value = index;
                    txtYandexMail.Text = contactObj.YandexConfig.Mail;
                    txtYandexPassword.Password = contactObj.YandexConfig.Password;
                    txtYandexProtocol.Text = contactObj.YandexConfig.Protocol;
                    try
                    {
                        txtYandexFix.Text = contactObj.YandexConfig.TextFix;
                    } catch(Exception) { }

                    txtHotmailApiKey.Password = contactObj.HotmailConfig.ApiKey;
                    txtHotmailDomain.Text = contactObj.HotmailConfig.DomainName;

                    chbYandex.IsChecked = contactObj.Yandex;
                    chbHotmail.IsChecked = contactObj.Hotmail;
                    chbPrimaryNewLayout.IsChecked = contactObj.NewLayout;

                    var mailList = cacheViewModel.GetCacheDao().Get("contact:config:mail_list").Value.ToString();
                    txtMailList.Text = mailList;
                }
            } catch(Exception) { }
        }

        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            ContactConfig contactObj = new ContactConfig();
            YandexConfig yandexObj = new YandexConfig();
            HotmailConfig hotmailObj = new HotmailConfig();

            int index = Int32.Parse(txtYandexIndex.Value.ToString());

            yandexObj.Index = index;
            yandexObj.Mail = txtYandexMail.Text;
            yandexObj.Password = txtYandexPassword.Password;
            yandexObj.Protocol = txtYandexProtocol.Text;
            yandexObj.TextFix = txtYandexFix.Text;

            hotmailObj.ApiKey= txtHotmailApiKey.Password;
            hotmailObj.DomainName= txtHotmailDomain.Text;

            contactObj.Hotmail = chbHotmail.IsChecked.Value;
            contactObj.Yandex = chbYandex.IsChecked.Value;
            contactObj.NewLayout = chbPrimaryNewLayout.IsChecked.Value;

            contactObj.MailList= txtMailList.Text;

            contactObj.HotmailConfig = hotmailObj;
            contactObj.YandexConfig = yandexObj;

            string output = JsonConvert.SerializeObject(contactObj);

            cacheViewModel.GetCacheDao().Set("contact:config", output);
            cacheViewModel.GetCacheDao().Set("contact:config:index", index+"");

            cacheViewModel.GetCacheDao().Set("contact:config:mail_list", contactObj.MailList);

            MessageBox.Show("Your config has been save successfully.");
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
