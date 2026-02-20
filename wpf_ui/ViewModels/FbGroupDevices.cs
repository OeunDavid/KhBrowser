using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ToolKHBrowser.ViewModels
{
    public class FbGroupDevices : INotifyPropertyChanged
    {
        private int _key;
        private int _id;
        private string _name;
        private int _status;
        private string _textStatus;
        private string _description;

        public int Key
        {
            get { return _key; }
            set
            {
                _key = value;
            }
        }
        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
            }
        }
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaiseProperChanged();
            }
        }
        public int Status
        {
            get { return _status; }
            set
            {
                _status = value;
                RaiseProperChanged();
            }
        }
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                RaiseProperChanged();
            }
        }
        public string TextStatus
        {
            get { return _textStatus; }
            set
            {
                _textStatus = value;
                RaiseProperChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaiseProperChanged([CallerMemberName] string caller = "")
        {

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }
    }
}
