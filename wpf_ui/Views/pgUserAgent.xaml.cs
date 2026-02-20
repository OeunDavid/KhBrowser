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
    public partial class pgUserAgent : Page
    {
        public frmMain parentMain;
        public pgUserAgent(frmMain frmMain)
        {
            this.parentMain = frmMain;

            InitializeComponent();
        }

        private void btnSaveUserAgent_Click(object sender, RoutedEventArgs e)
        {
            string userAgentListIds = txtUserAgentList.Text.Trim();

            this.parentMain.AddUserAgent(userAgentListIds);

            txtUserAgentList.Text = "";
        }
    }
}
