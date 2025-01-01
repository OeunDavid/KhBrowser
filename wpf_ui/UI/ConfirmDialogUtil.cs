using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using WpfUI.Views;
using static WpfUI.Views.wdConfirmDialog;

namespace WpfUI.UI
{
    public class ConfirmDialogUtil
    {
        public static wdConfirmDialog ShowDialog(string title, string text, BtnClick OkClick, BtnClick CancelClick = null)
        {
            var dialog = new wdConfirmDialog();
            dialog.OkClick = OkClick;
            dialog.CancelClick = CancelClick;
            dialog.Text(text);
            if( title != null)
            {
                dialog.DialogTitle(title);
            }
            dialog.Show();
            return dialog;
        }
        public static wdConfirmDialog ShowDialog(string title, Page page, BtnClick OkClick, BtnClick CancelClick = null)
        {
            var dialog = new wdConfirmDialog();
            dialog.OkClick = OkClick;
            dialog.CancelClick = CancelClick;
            if (title != null)
            {
                dialog.DialogTitle(title);
            }
            dialog.Page(page);
            dialog.Show();
            return dialog;
        }
        public static wdConfirmDialog ShowDialog(string text, BtnClick OkClick,BtnClick CancelClick = null)
        {
            return ShowDialog(null, text, OkClick, CancelClick);
        }
    }
}
