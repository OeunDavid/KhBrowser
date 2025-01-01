using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLib.Data
{
    public interface IDeviceAccountDao
    {
        Dictionary<string,DeviceAccount> list(string deviceID);
        IDataDao getIDataDao();
        void mapDevice(string deviceID, int groupDeviceId, string shareTypeIds, int limit);
        DataTable listDataForGrid(string deviceID);
        DeviceAccount get(long id);
        int delete(long id);
        void add(string deviceID,int num);
        int updateStatus(int id,int status,string description="");
        int updateDarkMode(long id,int darkMode);
        int action(int id, int status, string fieldAction, string description = "");
        DataTable listNoFBLite(string deviceID, int offset, int limit);
        DataTable listData(string deviceId, int limit, int offset, string where, string orderBy, int status = 1);
    }
    public class DeviceAccountDao : IDeviceAccountDao
    {
        private IDataDao _dataDao;
        public DeviceAccountDao(IDataDao dataDao)
        {
            _dataDao = dataDao;
        }
        public IDataDao getIDataDao()
        {
            return _dataDao;
        }
        public int updateDarkMode(long id, int darkMode)
        {
            var p = new Dictionary<string, object>() {
                        {"@id", id },
                        {"@dark_mode", darkMode }
                    };

            return _dataDao.execute(SQLConstant.TABLE_DEVICE_ACCOUNT_UPDATE_DARK_MODE, p);
        }
        public int updateStatus(int id, int status, string description = "")
        {
            var p = new Dictionary<string, object>() {
                        {"@id", id },
                        {"@status", status },
                        {"@description", description },
                        {"@updated_at", DateTime.Now.ToFileTime()}
                    };

            return _dataDao.execute(SQLConstant.TABLE_DEVICE_ACCOUNT_UPDATE_STATUS, p);
        }
        public int action(int id, int status, string fieldAction, string description = "")
        {
            var p = new Dictionary<string, object>() {
                        {"@id", id },
                        {"@status", status },
                        {"@description", description },
                        {"@updated_at", DateTime.Now.ToFileTime()}
                    };
            string sql = SQLConstant.TABLE_DEVICE_ACCOUNT_UPDATE_ACTIVE.Replace("{customFieldUpdate}", ","+fieldAction+"="+ DateTime.Now.ToFileTime());

            return _dataDao.execute(sql, p);
        }
        public DataTable listData(string deviceId, int limit, int offset, string where, string orderBy, int status = 1)
        {
            var p = new Dictionary<string, object>() {
                {"@device_id", deviceId },
                {"@offset", offset },
                {"@limit", limit },
                {"@status", status }
            };

            string sql = SQLConstant.TABLE_DEVICE_ACCOUNT_SELECT_ALL.Replace("{order_by}", orderBy);
            sql = sql.Replace("{where}", where);

            DataTable table = _dataDao.query(sql, p);

            return table;
        }
        public DataTable listNoFBLite(string deviceID, int offset, int limit)
        {
            var p = new Dictionary<string, object>() {
                {"@device_id", deviceID },
                {"@offset", offset },
                {"@limit", limit }
            };
            DataTable table = _dataDao.query(SQLConstant.TABLE_DEVICE_ACCOUNT_NO_FBLITE, p);

            return table;
        }
        public Dictionary<string,DeviceAccount> list(string deviceID)
        {
            Dictionary<string,DeviceAccount> items = new Dictionary<string,DeviceAccount>();
            var p = new Dictionary<string, object>() {
                {"@device_id", deviceID }
            };
            DataTable table = _dataDao.query(SQLConstant.TABLE_DEVICE_ACCOUNT_BY_DEVICE,p);
            if (table != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    long id = Int64.Parse(row["id"].ToString());
                    int apkID = Int32.Parse(row["apk_id"].ToString());
                    int status = Int32.Parse(row["status"].ToString());
                    //long updated_at = (long)row["updated_at"];
                    string uid = row["uid"].ToString().Trim();
                    string packageID = row["package_id"].ToString().Trim();
                    string description = row["description"].ToString().Trim();

                    var d = new DeviceAccount()
                    {
                        Id = id,
                        DeviceID = deviceID,
                        APKID = apkID,
                        UID = uid,
                        PackageID = packageID,
                        Status = status,
                        Description = description,
                        //UpdatedAt= DateTimeOffset.FromFileTime(updated_at).UtcDateTime
                    };

                    items[packageID] = d;
                }
            }

            return items;
        }
        public DataTable listDataForGrid(string deviceID)
        {
            var items = new ObservableCollection<DeviceAccount>();
            var p = new Dictionary<string, object>() {
                {"@device_id", deviceID }
            };
            DataTable table = _dataDao.query(SQLConstant.TABLE_DEVICE_ACCOUNT_BY_DEVICE, p);

            return table;
        }
        public int add(DeviceAccount data)
        {
            var p = new Dictionary<string, object>() {
                {"@device_id", data.DeviceID},
                {"@apk_id", data.APKID }
            };

            return _dataDao.execute(SQLConstant.TABLE_DEVICE_ACCOUNT_INSERT_APK, p);
        }
        public int delete(long id)
        {
            var p = new Dictionary<string, object>()
            {
                { "@id", id }
            };

            return _dataDao.execute(SQLConstant.TABLE_DEVICE_ACCOUNT_DELETE, p);
        }
        public void mapDevice(string deviceID, int groupDeviceId, string shareTypeIds, int limit)
        {
            var p = new Dictionary<string, object> {
                {"@device_id",deviceID },
                {"@limit",limit },
                {"@group_device_id",groupDeviceId }
            };

            string sql = SQLConstant.TABLE_ACCOUNT_SELECT_ALL_NO_DEVICE.Replace("{shareTypeIds}", shareTypeIds);
            var dataTable = _dataDao.query(sql, p);
            foreach (DataRow row in dataTable.Rows)
            {
                long id = (long)row["id"];
                string uid = row["uid"]+"";

                var a = new Dictionary<string, object> {
                    {"@device_id",deviceID },
                    {"@id", id}
                };
                var b = new Dictionary<string, object> {
                    {"@device_id",deviceID },
                    {"@uid", uid }
                };

                _dataDao.execute(SQLConstant.TABLE_ACCOUNT_UPDATE_DEVICE, a);
                _dataDao.execute(SQLConstant.TABLE_DEVICE_ACCOUNT_UPDATE_UID, b);
            }
        }
        public DeviceAccount get(long id)
        {
            var p = new Dictionary<string, object> {
                {"@id", id }
            };
            var dataTable = _dataDao.query(SQLConstant.TABLE_DEVICE_ACCOUNT_SELECT_ONE, p);
            foreach (DataRow row in dataTable.Rows)
            {
                return DeviceAccount.from(row);
            }

            return new DeviceAccount();
        }
        public DeviceAccount last(string deviceID)
        {
            var p = new Dictionary<string, object> {
                {"@device_id", deviceID }
            };
            var dataTable = _dataDao.query(SQLConstant.TABLE_DEVICE_ACCOUNT_SELECT_ONE_LAST_BY_DEVICE, p);
            foreach (DataRow row in dataTable.Rows)
            {
                return DeviceAccount.from(row);
            }

            return new DeviceAccount();
        }
        public void add(string deviceID, int num)
        {
            DeviceAccount d = last(deviceID);
            int apkID = 1;
            if(!string.IsNullOrEmpty(d.APKID.ToString()))
            {
                apkID = d.APKID + 1;
            }
            string path = DeviceInfo.FB_LITE_SOURCE;
            for (int i = 1; i <= num; i++)
            {
                int counter = 20;
                bool exist = false;
                do
                {
                    if(File.Exists(path+"/"+apkID+".apk"))
                    {
                        exist = true;
                    } else
                    {
                        apkID++;
                    }
                } while (!exist && counter-- > 0 );
                if (exist)
                {
                    var a = new Dictionary<string, object>() {
                        {"@device_id", deviceID},
                        {"@apk_id", apkID}
                    };
                    _dataDao.execute(SQLConstant.TABLE_DEVICE_ACCOUNT_INSERT_APK, a);

                    apkID++;
                }
            }
        }
    }
}
