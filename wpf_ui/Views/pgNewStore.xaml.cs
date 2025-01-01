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
using ToolKHBrowser.Views;
using ToolLib.Data;
using WpfUI.ViewModels;

namespace WpfUI.Views
{
    /// <summary>
    /// Interaction logic for pgNewGroupDevices.xaml
    /// </summary>
    public partial class pgNewStore : Page
    {
        private Store store;
        private IStoreViewModel storeViewModel;
        private pgStore parentPGStore;

        public pgNewStore(pgStore parentPGStore, Store store=null)
        {
            InitializeComponent();

            this.parentPGStore = parentPGStore;
            this.store = store;

            storeViewModel = DIConfig.Get<IStoreViewModel>();
            renderData();
        }
        public void renderData()
        {
            if(store == null)
            {
                store = new Store();
                return;
            }
            try
            {
                txtName.Text = store.Name;
                txtDescription.Text = store.Description;
                if (store.Status != 1)
                {
                    chbStatus.IsChecked = false;
                }
                if (store.IsTemp == 1)
                {
                    chbTemp.IsChecked = true;
                }
                chbTemp.IsEnabled = false;
            }
            catch { }
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            string name = txtName.Text;
            string description = txtDescription.Text;
            bool isStatus = chbStatus.IsChecked.Value;
            bool isTemp = chbTemp.IsChecked.Value;
            if(!string.IsNullOrEmpty(name))
            {
                Store data = new Store();
                data.Name = name;
                data.Status = (isStatus) ? 1 : 0 ;
                data.IsTemp = (isTemp) ? 1 : 0;
                data.Description = description;

                if (store.Id>0)
                {
                    data.Id = store.Id;
                    store.Name = name;
                    store.Description = description;
                    store.Status = data.Status;
                    store.IsTemp = data.IsTemp;
                    if (store.Status==1)
                    {
                        store.TextStatus = "Active";
                    } else
                    {
                        store.TextStatus = "Inactive";
                    }

                    storeViewModel.getGroupDevicesDao().update(data);
                } else
                {
                    storeViewModel.getGroupDevicesDao().add(data);
                }

                txtName.Text = "";
                txtDescription.Text = "";
                store.Id = 0;

                parentPGStore.loadDataToGrid();
            } else
            {
                MessageBox.Show("Name is require!");
            }
        }
    }
}
