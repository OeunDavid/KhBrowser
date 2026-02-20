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

namespace ToolKHBrowser.Views
{
    /// <summary>
    /// Interaction logic for pgProxy.xaml
    /// </summary>
    public partial class pgProxy : Page
    {
        public frmMain parentMain;
        public pgProxy(frmMain frmMain)
        {
            this.parentMain = frmMain;

            InitializeComponent();
        }

        private void btnSaveProxy_Click(object sender, RoutedEventArgs e)
        {
            string proxyListIds = txtProxyList.Text.Trim();

            this.parentMain.AddProxy(proxyListIds);

            txtProxyList.Text = "";
        }
    }
}
