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
using ToolKHBrowser.Views;
using static Emgu.CV.Stitching.Stitcher;

namespace ToolKHBrowser.Views
{
    /// <summary>
    /// Interaction logic for pgChangeStore.xaml
    /// </summary>
    public partial class pgChangeStore : Page
    {
        public frmMain form;
        public pgChangeStore(frmMain form)
        {
            InitializeComponent();
            this.form = form;

            ddlStore.ItemsSource = this.form.storeViewModel.listDataForGrid();
            ddlStore.SelectedIndex = 0;
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
        public void btnStartChange_Click(object sender, RoutedEventArgs e)
        {
            var sId = GetStoreId();
            var pId = this.form.GetStoreId();
            if(sId != pId)
            {
                string uids = "";
                foreach (FbAccount acc in this.form.dgAccounts.SelectedItems)
                {
                    if (!string.IsNullOrEmpty(uids))
                    {
                        uids += ",";
                    }
                    uids += acc.UID;
                }
                if (!string.IsNullOrEmpty(uids))
                {
                    var store = this.form.storeViewModel.getGroupDevicesDao().Get(sId);
                    if (store.IsTemp == 1)
                    {
                        this.form.fbAccountViewModel.getAccountDao().tempStore(uids, sId);
                    }
                    else
                    {
                        this.form.fbAccountViewModel.getAccountDao().transfer(uids, sId);
                    }
                    MessageBox.Show("Account has been change store successfully.");
                    this.form.loadDataToGrid();
                }
            }
        }
    }
}
