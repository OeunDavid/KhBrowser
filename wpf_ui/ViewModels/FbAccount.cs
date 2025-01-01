using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfUI.ViewModels
{
    public class FbAccount: INotifyPropertyChanged
    {
        private long _id;
        private int _group_device_id;
        private string _uid;
        private string _name;
        private string _password;
        private string _twofa;
        private string _token;
        private DateTime _birthday;
        private string _email;
        private string _gender;

        private int _total_friend;
        private int _total_group;
        private int _key;

        private string _status;
        private string _join;
        private string _leave;
        private string _description;
        private string _device_id;
        private string _device_name;
        private string _lastUpdate;
        private string _proxy;
        private int _share_type_id;
        private string _share;
        private int _is_active;
        private int _is_leave;
        private int _is_share;

        private string _pending_join;
        private string _cookie;
        private string _browser_key;
        private string _user_agent;
        private int _is_login;
        private int _friends_request;
        public string _dob;
        public string _active;
        public string _group_ids;
        public string _login;
        public int _total_share_group;
        public int _total_share_timeline;

        public int _total_page;
        public string _page_ids;

        private int _is_twofa;

        private int _is_verify;
        public string _verify;
        public string _temp_name;

        public string _url;
        public string _caption;
        public string _comment;

        public string _note;
        public string _reel_source_video;
        public string _mailPass;
        public string _primaryLocation;
        public string _timeline_source;
        public string _creation_date;
        public string _old_group_ids;
        public int TotalPage
        {
            get { return _total_page; }
            set
            {
                _total_page = value;
                RaiseProperChanged();
            }
        }
        public string OldGroupIds
        {
            get { return _old_group_ids; }
            set
            {
                _old_group_ids = value;
                RaiseProperChanged();
            }
        }
        public string TimelineSource
        {
            get { return _timeline_source; }
            set
            {
                _timeline_source = value;
                RaiseProperChanged();
            }
        }
        public string ReelSourceVideo
        {
            get { return _reel_source_video; }
            set
            {
                _reel_source_video = value;
                RaiseProperChanged();
            }
        }
        public string PrimaryLocation
        {
            get { return _primaryLocation; }
            set
            {
                _primaryLocation = value;
                RaiseProperChanged();
            }
        }
        public string PageIds
        {
            get { return _page_ids; }
            set
            {
                _page_ids = value;
                RaiseProperChanged();
            }
        }
        public string MailPass
        {
            get { return _mailPass; }
            set
            {
                _mailPass = value;
                RaiseProperChanged();
            }
        }
        public string CreationDate
        {
            get { return _creation_date; }
            set
            {
                _creation_date = value;
                RaiseProperChanged();
            }
        }
        public string Note
        {
            get { return _note; }
            set
            {
                _note = value;
                RaiseProperChanged();
            }
        }
        public string URL
        {
            get { return _url; }
            set
            {
                _url = value;
            }
        }
        public string Caption
        {
            get { return _caption; }
            set
            {
                _caption = value;
            }
        }
        public string Comment
        {
            get { return _comment; }
            set
            {
                _comment = value;
            }
        }
        public string TempName
        {
            get { return _temp_name; }
            set
            {
                _temp_name = value;
                RaiseProperChanged();
            }
        }
        public string Verify
        {
            get { return _verify; }
            set
            {
                _verify = value;
                RaiseProperChanged();
            }
        }
        public int IsVerify
        {
            get { return _is_verify; }
            set
            {
                _is_verify = value;
                RaiseProperChanged();
            }
        }
        public int IsTwoFA
        {
            get { return _is_twofa; }
            set
            {
                _is_twofa = value;
                RaiseProperChanged();
            }
        }
        public int TotalShareGroup
        {
            get { return _total_share_group; }
            set
            {
                _total_share_group = value;
                RaiseProperChanged();
            }
        }
        public int TotalShareTimeline
        {
            get { return _total_share_timeline; }
            set
            {
                _total_share_timeline = value;
                RaiseProperChanged();
            }
        }
        public string Login
        {
            get { return _login; }
            set
            {
                _login = value;
                RaiseProperChanged();
            }
        }
        public string GroupIDs
        {
            get { return _group_ids; }
            set
            {
                _group_ids = value;
                RaiseProperChanged();
            }
        }
        public string Active
        {
            get { return _active; }
            set
            {
                _active = value;
                RaiseProperChanged();
            }
        }
        public int IsActive
        {
            get { return _is_active; }
            set
            {
                _is_active = value;
                RaiseProperChanged();
            }
        }
        public int IsShare
        {
            get { return _is_share; }
            set
            {
                _is_share = value;
                RaiseProperChanged();
            }
        }
        public int IsLeave
        {
            get { return _is_leave; }
            set
            {
                _is_leave = value;
                RaiseProperChanged();
            }
        }
        public int FriendsRequest
        {
            get { return _friends_request; }
            set
            {
                _friends_request = value;
                RaiseProperChanged();
            }
        }
        public string DOB
        {
            get { return _dob; }
            set
            {
                _dob = value;
                RaiseProperChanged();
            }
        }
        public int ShareTypeId { get { return _share_type_id; } set {
                _share_type_id = value;
                RaiseProperChanged();
            }
        }
        public int IsLogin
        {
            get { return _is_login; }
            set
            {
                _is_login = value;
                RaiseProperChanged();
            }
        }
        public string UserAgent
        {
            get { return _user_agent; }
            set
            {
                _user_agent = value;
                RaiseProperChanged();
            }
        }
        public string BrowserKey
        {
            get { return _browser_key; }
            set
            {
                _browser_key = value;
                RaiseProperChanged();
            }
        }
        public string Cookie
        {
            get { return _cookie; }
            set
            {
                _cookie = value;
                RaiseProperChanged();
            }
        }
        public string Share
        {
            get { return _share; }
            set
            {
                _share = value;
                RaiseProperChanged();
            }
        }
        public long Id
        {
            get { return _id; }
            set
            {
                _id = value;
                RaiseProperChanged();
            }
        }
        public string PendingJoin
        {
            get { return _pending_join; }
            set
            {
                _pending_join = value;
                RaiseProperChanged();
            }
        }
        public string DeviceId
        {
            get { return _device_id; }
            set
            {
                _device_id = value;
                RaiseProperChanged();
            }
        }
        public string DeviceName
        {
            get { return _device_name; }
            set
            {
                _device_name = value;
                RaiseProperChanged();
            }
        }
        public string Proxy
        {
            get { return _proxy; }
            set
            {
                _proxy = value;
                RaiseProperChanged();
            }
        }
        public int TotalFriend
        {
            get { return _total_friend; }
            set
            {
                _total_friend = value;
                RaiseProperChanged();
            }
        }
        public int TotalGroup
        {
            get { return _total_group; }
            set
            {
                _total_group = value;
                RaiseProperChanged();
            }
        }
        public string Password { get { return _password; } set {
                _password = value;
                RaiseProperChanged();
            } }
        public string Status { get { return _status; } set {
                _status = value;
                RaiseProperChanged();
            } }
        public string LastUpdate { get {
                return _lastUpdate;

            } set { _lastUpdate = value;
                RaiseProperChanged();
            } }
        public string Email
        {
            get { return _email; }
            set { 
                _email = value;
                RaiseProperChanged();
            }
        }
        public string UID
        {
            get { return _uid; }
            set { 
                _uid = value;
                RaiseProperChanged();
            }
        }
        public string Name
        {
            get { return _name; }
            set { 
                _name = value;
                RaiseProperChanged();
            }
        }
        public string TwoFA
        {
            get { return _twofa; }
            set { 
                _twofa = value;
                RaiseProperChanged();
            }
        }
        public string Token
        {
            get { return _token; }
            set { 
                _token = value;
                RaiseProperChanged();
            }
        }
        public string Gender
        {
            get { return _gender; }
            set { 
                _gender = value;
                RaiseProperChanged();
            }
        }
        public DateTime Birthday
        {
            get { return _birthday; }
            set { 
                _birthday = value;
                RaiseProperChanged();
            }
        }
        public int Key
        {
            get { return _key; }
            set {
                _key = value;
                //RaiseProperChanged();
            }
        }
        public int GroupDeviceId
        {
            get { return _group_device_id; }
            set { 
                _group_device_id = value;
                RaiseProperChanged();
            }
        }
        public string Description
        {
            get { return _description; }
            set { 
                _description = value;
                RaiseProperChanged();
            }
        }
        public string Join
        {
            get { return _join; }
            set { 
                _join = value;
                RaiseProperChanged();
            }
        }
        public string Leave
        {
            get { return _leave; }
            set { 
                _leave = value;
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
