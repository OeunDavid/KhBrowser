using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfUI.ViewModels
{
    public class FBDeviceAccount : INotifyPropertyChanged
    {
        private int _key;
        private long _id;
        private string _device_id;
        private string _uid;
        private string _description;
        private DateTime _lastUpdate;
        private int _status;
        private string _text_status;
        private string _fblite;
        private string _package_id;
        private string _text_updated_at;
        private int _apk_id;
        private int _dark_mode;
        private string _text_account_status;
        public int Key
        {
            get
            {
                return _key;
            }
            set
            {
                _key = value;
            }
        }
        public long Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }
        public string TextAccountStatus
        {
            get { return _text_account_status; }
            set
            {
                _text_account_status = value;
                RaiseProperChanged();
            }
        }
        public string DeviceID
        {
            get { return _device_id; }
            set
            {
                _device_id = value;
                RaiseProperChanged();
            }
        }
        public int DarkMode
        {
            get { return _dark_mode; }
            set
            {
                _dark_mode = value;
                RaiseProperChanged();
            }
        }
        public string TextUpdatedAt
        {
            get { return _text_updated_at; }
            set
            {
                _text_updated_at = value;
                RaiseProperChanged();
            }
        }
        public string FBLite
        {
            get { return _fblite; }
            set
            {
                _fblite = value;
                RaiseProperChanged();
            }
        }
        public int APKID
        {
            get { return _apk_id; }
            set
            {
                _apk_id = value;
                RaiseProperChanged();
            }
        }
        public string PackageID
        {
            get { return _package_id; }
            set
            {
                _package_id = value;
                RaiseProperChanged();
            }
        }
        public string UID
        {
            get { return _uid; }
            set
            {
                _uid = value;
                RaiseProperChanged();
            }
        }
        public DateTime LastUpdate
        {
            get { return _lastUpdate; }
            set
            {
                _lastUpdate = value;
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
            get { return _text_status; }
            set
            {
                _text_status = value;
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
