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
using ToolLib.Data;
using ToolKHBrowser.UI;
using ToolKHBrowser.ViewModels;
using ToolKHBrowser.Views;
using ToolKHBrowser;
using System.Collections.ObjectModel;

namespace ToolKHBrowser.Views
{
    /// <summary>
    /// Interaction logic for pgStore.xaml
    /// </summary>
    public partial class pgStore : Page
    {
        private IStoreViewModel storeViewModel;
        private ObservableCollection<Store> _groupDevice;
        private bool isFirstLoading;
        public frmMain parentMain;
        public int parentStoreId;
        public pgStore(frmMain frmMain, int storeId)
        {
            InitializeComponent();

            this.parentMain = frmMain;
            this.parentStoreId = storeId;

            isFirstLoading = true;
            loadDataToGrid();
        }
        public void loadDataToGrid()
        {
            storeViewModel = DIConfig.Get<IStoreViewModel>();
            _groupDevice = storeViewModel.listDataForGrid();
            dgStore.ItemsSource = _groupDevice;
            if(!isFirstLoading)
            {
                this.parentMain.LoadStoreData(this.parentStoreId);
            }
            isFirstLoading = false;
        }
        private void btnAddNew_Click(object sender, EventArgs e)
        {
            var page = new pgNewStore(this);
            var dialog = UIUtil.Dialog(page, "Store");
            dialog.ShowDialog();
        }
        private void dgStore_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgStore.SelectedItem == null) return;
            var store = dgStore.SelectedItem as Store;

            var page = new pgNewStore(this, store);
            var dialog = UIUtil.Dialog(page, "Store");
            dialog.ShowDialog();
        }
    }
}
