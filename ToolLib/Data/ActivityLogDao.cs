using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ToolLib.Data
{
    public interface IActivityLogDao
    {
        int add(ActivityLog activityLog);
        int update(ActivityLog activityLog);
        ObservableCollection<ActivityLog> list(long fromDate, long toDate);
        int getTotalShareTimeline();
        int getTotalShareGroup();
    }
    public class ActivityLogDao:IActivityLogDao
    {
        private IDataDao _dataDao;
        public ActivityLogDao( IDataDao dataDao)
        {
            _dataDao = dataDao;
        }
        public int getTotalShareGroup()
        {
            int total = 0;
            var dataTable = _dataDao.query(SQLConstant.TABLE_ACTIVITY_LOG_TOTAL_SHARE_GROUP);
            foreach (DataRow row in dataTable.Rows)
            {
                try
                {
                    total = Int32.Parse(row["total_share_group"].ToString().Trim());
                }
                catch { }
                break;
            }

            return total;
        }
        public int getTotalShareTimeline()
        {
            int total = 0;
            var dataTable = _dataDao.query(SQLConstant.TABLE_ACTIVITY_LOG_TOTAL_SHARE_TIMELINE);
            foreach (DataRow row in dataTable.Rows)
            {
                try
                {
                    total = Int32.Parse(row["total_share_timeline"].ToString().Trim());
                }
                catch { }
                break;
            }

            return total;
        }
        public ObservableCollection<ActivityLog> list(long fromDate, long toDate)
        {
            ObservableCollection<ActivityLog> activityLogs = new ObservableCollection<ActivityLog>();
            var p = new Dictionary<string, object> {
                { "@from_date",fromDate },
                { "@to_date",toDate }
            };
            var dataTable = _dataDao.query(SQLConstant.TABLE_ACTIVITY_LOG_SELECT_ALL, p);
            int key = 1;
            foreach (DataRow r in dataTable.Rows)
            {
                var id = (long)r["id"];
                var deviceId = r["device_id"] + "";
                var uid = r["uid"] + "";
                var action_name = r["action_name"] + "";
                var description = r["description"] + "";
                var textActionDate = DateTimeOffset.FromFileTime((long)r["action_date"]).ToString("dd/MM/yyyy HH:mm:ss");
                var actionDate = (long)r["action_date"];

                var item = new ActivityLog()
                {
                    Key = key,
                    Id = id,
                    DeviceId = deviceId,
                    UID = uid,
                    ActionName = action_name,
                    Description = description,
                    TextActionDate = textActionDate,
                    ActionDate = actionDate
                };

                key++;
                activityLogs.Add(item);
            }

            return activityLogs;
        }
        public int add(ActivityLog activityLog)
        {
            var p = new Dictionary<string, object>() {
                {"@device_id", activityLog.DeviceId },
                {"@uid", activityLog.UID },
                {"@url", activityLog.UID },
                {"@share_timeline", activityLog.ShareTimeline },
                {"@share_groups", activityLog.ShareGroups },
                {"@action_name", activityLog.ActionName },
                {"@description", activityLog.Description },
                {"@action_date", activityLog.ActionDate },
            };

            return _dataDao.execute(SQLConstant.TABLE_ACTIVITY_LOG_INSERT, p);
        }
        public int update(ActivityLog activityLog)
        {
            var p = new Dictionary<string, object>() {
                {"@device_id", activityLog.DeviceId },
                {"@url", activityLog.UID },
                {"@share_timeline", activityLog.ShareTimeline },
                {"@share_groups", activityLog.ShareGroups },
                {"@description", activityLog.Description }
            };

            return _dataDao.execute(SQLConstant.TABLE_ACTIVITY_LOG_UPDATE, p);
        }
    }
    public class ActivityLog : INotifyPropertyChanged
    {
        public int _key;
        public long _id;
        public string _device_id;
        public string _uid;
        public string _action_name;
        public string _description;
        public string _text_action_date;
        public long _action_date;
        public int _share_timeline;
        public int _share_groups;
        public string _url;

        public int Key
        {
            get { return _key; }
            set
            {
                _key = value;
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
        public int ShareGroups
        {
            get { return _share_groups; }
            set
            {
                _share_groups = value;
                RaiseProperChanged();
            }
        }
        public int ShareTimeline
        {
            get { return _share_timeline; }
            set
            {
                _share_timeline = value;
                RaiseProperChanged();
            }
        }
        public string URL
        {
            get { return _url; }
            set
            {
                _url = value;
                RaiseProperChanged();
            }
        }
        public string ActionName
        {
            get { return _action_name; }
            set
            {
                _action_name = value;
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
        public string UID
        {
            get { return _uid; }
            set
            {
                _uid = value;
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
        public string TextActionDate
        {
            get { return _text_action_date; }
            set
            {
                _text_action_date = value;
                RaiseProperChanged();
            }
        }
        public long ActionDate
        {
            get { return _action_date; }
            set
            {
                _action_date = value;
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
