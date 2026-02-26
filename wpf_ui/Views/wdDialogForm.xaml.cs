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
using System.Windows.Threading;
using ToolKHBrowser.UI;

namespace ToolKHBrowser.Views
{
    /// <summary>
    /// Interaction logic for wdDialogForm.xaml
    /// </summary>
    public partial class wdDialogForm : Window
    {
        private Page _page;
        private double _pageDesignWidth = double.NaN;
        private double _pageDesignHeight = double.NaN;
        public object Data { get; set; }
        public wdDialogForm()
        {
            InitializeComponent();
            Loaded += wdDialogForm_Loaded;
            SizeChanged += wdDialogForm_SizeChanged;
        }
        public void Page(Page page)
        {
            _page = page;
            _pageDesignWidth = page?.Width ?? double.NaN;
            _pageDesignHeight = page?.Height ?? double.NaN;

            // Avoid nested scrolling in Frame + Page root ScrollViewer.
            if (_page != null)
            {
                _page.Width = double.NaN;
                _page.Height = double.NaN;
                _page.HorizontalAlignment = HorizontalAlignment.Stretch;
                _page.VerticalAlignment = VerticalAlignment.Stretch;
            }

            frameMain.Content = _page;
            Dispatcher.BeginInvoke(new Action(UpdateFooterButtonsState), DispatcherPriority.Loaded);
            Dispatcher.BeginInvoke(new Action(ApplyDialogSizeConstraints), DispatcherPriority.Loaded);
        }

        private void wdDialogForm_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyDialogSizeConstraints();
        }

        private void wdDialogForm_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplyDialogSizeConstraints();
        }

        private void btnPower_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnDialogSave_Click(object sender, RoutedEventArgs e)
        {
            if (_page == null)
            {
                return;
            }

            var saveButton = FindChildByName<Button>(_page, "btnSaveConfig");
            if (saveButton != null)
            {
                saveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void UpdateFooterButtonsState()
        {
            bool hasSaveButton = _page != null && FindChildByName<Button>(_page, "btnSaveConfig") != null;
            if (dialogFooterBar != null)
            {
                dialogFooterBar.Visibility = hasSaveButton ? Visibility.Visible : Visibility.Collapsed;
            }
            ApplyDialogSizeConstraints();
        }

        private void ApplyDialogSizeConstraints()
        {
            if (gContent == null)
            {
                return;
            }

            double maxW = Math.Max(320, SystemParameters.WorkArea.Width - 56);
            double maxH = Math.Min(700, Math.Max(320, SystemParameters.WorkArea.Height - 56));

            gContent.MaxWidth = maxW;
            gContent.MaxHeight = maxH;

            if (_page == null)
            {
                return;
            }

            double pageWidth = (!double.IsNaN(_pageDesignWidth) && _pageDesignWidth > 0) ? _pageDesignWidth : 900;
            double pageHeight = (!double.IsNaN(_pageDesignHeight) && _pageDesignHeight > 0) ? _pageDesignHeight : 700;

            double footerHeight = (dialogFooterBar != null && dialogFooterBar.Visibility == Visibility.Visible) ? 86 : 0;
            double chromeHeight = 72 + footerHeight;

            double desiredWidth = pageWidth + 48;
            double desiredHeight = pageHeight + chromeHeight + 16;

            gContent.Width = Math.Min(desiredWidth, maxW);
            gContent.Height = Math.Min(desiredHeight, maxH);

            if (frameMain != null)
            {
                frameMain.MaxHeight = Math.Max(120, gContent.Height - chromeHeight);
            }
        }

        private T FindChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            if (parent == null)
            {
                return null;
            }

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typed && string.Equals(typed.Name, name, StringComparison.Ordinal))
                {
                    return typed;
                }

                T nested = FindChildByName<T>(child, name);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
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
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}
