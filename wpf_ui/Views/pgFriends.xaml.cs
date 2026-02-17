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

namespace ToolKHBrowser.Views
{
    /// <summary>
    /// Interaction logic for pgFriends.xaml
    /// </summary>
    public partial class pgFriends : Page
    {
        ICacheViewModel cacheViewModel = DIConfig.Get<ICacheViewModel>();
        public pgFriends()
        {
            InitializeComponent();

            LoadData();
        }
        public void LoadData()
        {
            var cache = cacheViewModel.GetCacheDao().Get("friend:config");
            if (cache != null && cache.Value != null)
            {
                var str = cache.Value.ToString();
                if (!string.IsNullOrEmpty(str))
                {
                    FriendsConfig friendsObj = JsonConvert.DeserializeObject<FriendsConfig>(str);
                    var cacheUids = cacheViewModel.GetCacheDao().Get("friend:config:uids");
                    var uids = cacheUids?.Value?.ToString() ?? "";

                    txtAddFriends.Value = friendsObj.AddNumber;
                    txtAcceptFriends.Value = friendsObj.AcceptNumber;

                    txtAddFriendByUIDNumber.Value = friendsObj.FriendsByUID.AddNumber;
                    txtUID.Text = uids;
                }
            }
        }

        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            FriendsByUID friendsByUIDObj = new FriendsByUID();
            friendsByUIDObj.UIDs = txtUID.Text;
            friendsByUIDObj.AddNumber = Int32.Parse(txtAddFriendByUIDNumber.Value.ToString());

            FriendsConfig friendObj = new FriendsConfig();
            friendObj.AddNumber = Int32.Parse(txtAddFriends.Value.ToString());
            friendObj.AcceptNumber = Int32.Parse(txtAcceptFriends.Value.ToString());
            friendObj.FriendsByUID = friendsByUIDObj;

            string output = JsonConvert.SerializeObject(friendObj);

            cacheViewModel.GetCacheDao().Set("friend:config", output);
            cacheViewModel.GetCacheDao().Set("friend:config:uids", friendsByUIDObj.UIDs);

            MessageBox.Show("Your config has been save successfully.");
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
