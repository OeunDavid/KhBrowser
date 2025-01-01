using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfUI.Views;

namespace WpfUI.UI
{
    public class UIUtil
    {
        public static void CloseParent(Page page)
        {
            var wnd = Window.GetWindow( page);
            wnd.Close();
        }
        public static void SetParentData(Page page, object data)
        {
            var wnd = Window.GetWindow(page);
            if ( wnd is wdDialogForm)
            {
                var dialog = (wdDialogForm)wnd;
                dialog.Data = data;
            }
        }
        public static wdDialogForm Dialog( Page page, string title = "Dialog")
        {
            var dialogWindow = new wdDialogForm();
            dialogWindow.Page(page);
            dialogWindow.WinTitle(title);

            return dialogWindow;
        }
        public static bool isPointInSide(Point point , double startX, double endX , double startY , double endY)
        {
            double x =  point.X;
            double y = (int)point.Y;
            return x >= startX && x <= endX && y >= startY && y <= endY;
        }
        public static SolidColorBrush BrushFromHex(string hexColorString)
        {
            return (SolidColorBrush)(new BrushConverter().ConvertFrom(hexColorString));
        }
    }
}
