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
    /// Interaction logic for wdDialogForm.xaml
    /// </summary>
    public partial class wdDialogForm : Window
    {
        private Page _page;
        public object Data { get; set; }
        public wdDialogForm()
        {
            InitializeComponent();
        }
        public void Page(Page page)
        {
            _page = page;
            frameMain.Content = _page;
        }

        private void btnPower_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        public void WinTitle(string title)
        {
            lblWinTitle.Text = title;
        }


        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //var position = e.GetPosition(this);
            ////var gPos = gContent.ReadLocalValue
            //Point relativePoint = gContent.TransformToAncestor(this)
            //              .Transform(new Point(0, 0));
            //var startX = relativePoint.X;
            //var endX = relativePoint.X + gContent.Width;
            //var startY = relativePoint.Y;
            //var endY = relativePoint.Y + gContent.Height;
            //if( !UIUtil.isPointInSide(position, startX , endX, startY , endY))
            //{
            //    this.Close();
            //}
            


        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if( e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}
