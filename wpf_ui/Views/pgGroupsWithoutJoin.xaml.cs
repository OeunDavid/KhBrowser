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
using WpfUI;
using WpfUI.ViewModels;
using WpfUI.Views;

namespace ToolKHBrowser.Views
{
    /// <summary>
    /// Interaction logic for pgGroups.xaml
    /// </summary>
    public partial class pgGroupsWithoutJoin : Page
    {
        ICacheViewModel cacheViewModel = DIConfig.Get<ICacheViewModel>();
        public frmMain parentMain;
        public pgGroupsWithoutJoin(frmMain frmMain)
        {
            this.parentMain = frmMain;

            InitializeComponent();
            LoadData();
        }
        public void LoadData()
        {
            try
            {
                var str = cacheViewModel.GetCacheDao().Get("group:without:join").Value.ToString();
                txtGroupIDs.Text = str;
            }
            catch (Exception) { }
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string groupIds = txtGroupIDs.Text.Trim();
            int num = Int32.Parse(txtGroupJoinOfNumber.Value.ToString());
            try
            {
                this.parentMain.AddGroup(groupIds, num);
            }
            catch (Exception) { }

            cacheViewModel.GetCacheDao().Set("group:without:join", groupIds);

            MessageBox.Show("Group ID has been save successfully.");
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
