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
    public partial class pgRecovery : Page
    {
        ICacheViewModel cacheViewModel = DIConfig.Get<ICacheViewModel>();
        public pgRecovery()
        {
            InitializeComponent();

            LoadData();
        }
        public void LoadData()
        {
            try
            {
                var str = cacheViewModel.GetCacheDao().Get("recovery:config").Value.ToString();
                if (!string.IsNullOrEmpty(str))
                {
                    RecoveryConfig recoveryObj = JsonConvert.DeserializeObject<RecoveryConfig>(str);

                    txt5SimApiKey.Password = recoveryObj.PhoneNumber.FiveSim.APIKey;
                    txt5SimCountry.Text = recoveryObj.PhoneNumber.FiveSim.Country;
                    txt5SimOperator.Text = recoveryObj.PhoneNumber.FiveSim.Opterator;
                    txt5SimProduct.Text = recoveryObj.PhoneNumber.FiveSim.Product;

                    chb5Sim.IsChecked = recoveryObj.FiveSime;
                    txtNewPassword.Text = recoveryObj.Password;
                }
            } catch(Exception) { }
        }

        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            RecoveryConfig recoveryObj = new RecoveryConfig();
            RecoveryPhoneNumber phoneNumberObj = new RecoveryPhoneNumber();
            FiveSim fiveSimObj = new FiveSim();

            recoveryObj.FiveSime = chb5Sim.IsChecked.Value;
            recoveryObj.Password= txtNewPassword.Text;

            fiveSimObj.APIKey = txt5SimApiKey.Password;
            fiveSimObj.Country = txt5SimCountry.Text;
            fiveSimObj.Opterator = txt5SimOperator.Text;
            fiveSimObj.Product = txt5SimProduct.Text;

            phoneNumberObj.FiveSim= fiveSimObj;
            recoveryObj.PhoneNumber = phoneNumberObj;

            string output = JsonConvert.SerializeObject(recoveryObj);

            cacheViewModel.GetCacheDao().Set("recovery:config", output);

            MessageBox.Show("Your config has been save successfully.");
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
