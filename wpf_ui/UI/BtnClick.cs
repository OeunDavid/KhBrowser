using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfUI.UI
{
    public delegate void BtnClick(object sender, RoutedEventArgs e);
    public interface DialogContent
    {
        object Content { get; }
    }
}
