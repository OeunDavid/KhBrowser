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
using WpfUI.Views;

namespace ToolKHBrowser.Views
{
    /// <summary>
    /// Interaction logic for pgNote.xaml
    /// </summary>
    public partial class pgTimelineSource : Page
    {
        public frmMain parentMain;
        public pgTimelineSource(frmMain frmMain)
        {
            this.parentMain = frmMain;

            InitializeComponent();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string source = txtTimelineSource.Text;
            if(string.IsNullOrEmpty(source))
            {
                return;
            }
            this.parentMain.AddTimelineSource(source);
            txtTimelineSource.Text = "";
        }
    }
}
