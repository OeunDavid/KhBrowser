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
using System.Windows.Shapes;
using ToolKHBrowser.UI;

namespace ToolKHBrowser.Views
{
    /// <summary>
    /// Interaction logic for wdConfirmDialog.xaml
    /// </summary>
    public partial class wdConfirmDialog : Window
    {
       
        public BtnClick OkClick { get; set; }
        public BtnClick CancelClick { get; set; }
        public void DialogTitle(string title)
        {
            lblTitle.Text = title;
        }
        public void Text(string content)
        {
            lblContent.Text = content;
        }
        public wdConfirmDialog()
        {
            InitializeComponent();
        }
        public void Page(Page page)
        {
            lblContent.Visibility = Visibility.Hidden;
            frameMain.Content = page;
            frameMain.Visibility = Visibility.Visible;

        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
           
           
            this.Close();
            if (OkClick != null)
            {
                OkClick.Invoke(sender, e);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
        
            if( CancelClick != null)
            {
                CancelClick.Invoke(sender, e);
            }
            this.Close();
        }

        private void btnPower_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}
