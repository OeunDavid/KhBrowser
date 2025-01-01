using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLib.Data
{
    public interface IDeviceDao
    {
        List<Device> listDataForGrid(int groupDeviceId);
        List<Device> listDevice();
        int add(Device device);
        int update(Device device);
        int delete(Device device);
        int updateFBLite(Device device);
        int updateAvailable(Device device);
        int updateName(Device device);
        int updateAccount(Device device);
        int updateRemoveFBLite(Device device);
        int updateDataMode(string deviceId, int dataMode);
        int updateGroup(Device device);
        int updateInternet(Device device);
        int truncate();
        Device get(int id);
        Device get(string deviceId);
        int updateIsBusy(int id, int is_busy);
        int updateExpire(string deviceId, int isExpire);
        int updateStatus(string deviceId, int status);
        int updateStatusToDisconnect();
        int updateActive(int id, long active_date, string description);
        int getTotalDevice(int groupDeviceId, int internetId, int isBusy);
        DataTable listDataForActions(int groupDeviceId, string internetIds, int limit, string orderBy, int status = 1);
        int getTotalDevice();
    }
   public class DeviceDao:IDeviceDao
   {
        private IDataDao _dataDao;
        public DeviceDao( IDataDao dataDao)
        {
            _dataDao = dataDao;
        }
        public int getTotalDevice()
        {
            int total = 0;
            var dataTable = _dataDao.query(SQLConstant.TABLE_DEVICE_TOTAL_ALL);
            foreach (DataRow row in dataTable.Rows)
            {
                total = Int32.Parse(row["total_device"].ToString().Trim());
                break;
            }

            return total;
        }
        public int updateExpire(string deviceId, int isExpire)
        {
            var p = new Dictionary<string, object>() {
                {"@device_id", deviceId },
                {"@is_expire", isExpire }
            };

            return _dataDao.execute(SQLConstant.TABLE_DEVICE_UPDATE_EXPIRE, p);
        }
        public int updateActive(int id, long active_date, string description)
        {
            var p = new Dictionary<string, object>() {
                {"@id", id },
                {"@active_date", active_date },
                {"@description", description }
            };

            return _dataDao.execute(SQLConstant.TABLE_DEVICE_UPDATE_ACTIVE, p);
        }
        public List<Device> listDataForGrid(int groupDeviceId)
        {
            List<Device> devices = new List<Device>();
            var p = new Dictionary<string, object>() {
                {"@group_device_id", groupDeviceId}
            };
            DataTable table = _dataDao.query(SQLConstant.TABLE_DEVICE_SELECT_ALL_GROUP,p);
            if (table != null)
            {
                int key = 1;
                foreach (DataRow row in table.Rows)
                {
                    int id = Int32.Parse(row["id"].ToString().Trim());
                    int status = Int32.Parse(row["status"].ToString());
                    int totalAccount = Int32.Parse(row["total_account"].ToString());
                    int totalAvailable = Int32.Parse(row["total_available"].ToString());
                    int totalFBLite = Int32.Parse(row["total_fblite"].ToString());
                    int isBusy = Int32.Parse(row["is_busy"].ToString());
                    int internetId = Int32.Parse(row["internet_id"].ToString());
                    int dataMode = Int32.Parse(row["data_mode"].ToString());
                    int accountLive = Int32.Parse(row["account_live"].ToString());

                    string name = row["name"] + "";
                    string deviceID = row["device_id"] + "";
                    string description = row["description"] + "";
                    long updatedAt = (long)row["updated_at"];
                    var keyPress = Convert.ToInt32(row["key_press_for_text"]);

                    var d = new Device()
                    {
                        Key = key,
                        Id = id,
                        DeviceID = deviceID,
                        Name = name,
                        TotalAccount = totalAccount,
                        TotalAvailable = totalAvailable,
                        TotalFBLite = totalFBLite,
                        Status = status,
                        IsBusy = isBusy,
                        InternetId = internetId,
                        Description = description,
                        AccountLive = accountLive,
                        GroupDeviceId = groupDeviceId,
                        UpdatedAt = updatedAt,
                        DataMode = dataMode,
                        KeyPressForText = keyPress
                    };
                    try
                    {
                        d.TextActiveDate = DateTimeOffset.FromFileTime((long)row["active_date"]).ToString("dd/MM/yyyy HH:mm:ss");
                        d.ActiveDate = (long)row["active_date"];
                    }
                    catch { }
                    devices.Add(d);

                    key++;
                }
            }

            return devices;
        }
        public List<Device> listDevice()
        {
            List<Device> devices = new List<Device>();
            DataTable table = _dataDao.query(SQLConstant.TABLE_DEVICE_SELECT_ALL);
            if( table !=null)
            {
                int key = 1;
                foreach (DataRow  row in table.Rows)
                {
                    int id = Int32.Parse(row["id"].ToString()) ;
                    int status = Int32.Parse(row["status"].ToString());
                    int totalAccount = Int32.Parse(row["total_account"].ToString());
                    int totalAvailable = Int32.Parse(row["total_available"].ToString());
                    int totalFBLite = Int32.Parse(row["total_fblite"].ToString());
                    int isBusy = Int32.Parse(row["is_busy"].ToString());
                    int dataMode = Int32.Parse(row["data_mode"].ToString());
                    string name = row["name"] + "";
                    string deviceID = row["device_id"] + "";
                    string description = row["description"] + "";
                    long updatedAt = (long)row["updated_at"];
                    var keyPress = Convert.ToInt32(row["key_press_for_text"]);

                    var d = new Device() { 
                        Key = key, 
                        Id = id,
                        DeviceID = deviceID, 
                        Name = name,
                        TotalAccount = totalAccount, 
                        TotalAvailable = totalAvailable,
                        TotalFBLite = totalFBLite,
                        Status = status,
                        IsBusy = isBusy,
                        Description = description,
                        DataMode = dataMode,
                        UpdatedAt = updatedAt ,
                    KeyPressForText=keyPress};
                    devices.Add(d);

                    key++;
                }
            }
            
            return devices;
        }
        public int add(Device device)
        {
            var p = new Dictionary<string, object>() {
                {"@device_id", device.DeviceID},
                {"@name", device.Name},
                {"@description", device.Description},
                {"@internet_id", device.InternetId},
                {"@group_device_id", device.GroupDeviceId},
                {"@total_available", device.TotalAvailable},
                {"@total_account", device.TotalAccount},
                {"@data_mode", device.DataMode},
                {"@key_press_for_text", device.KeyPressForText }
            };

            for(int i = 1; i <= device.TotalAccount; i++)
            {
                var a= new Dictionary<string, object>() {
                    {"@device_id", device.DeviceID},
                    {"@apk_id", i}
                };
                _dataDao.execute(SQLConstant.TABLE_DEVICE_ACCOUNT_INSERT_APK, a);
            }

            return _dataDao.execute(SQLConstant.TABLE_DEVICE_INSERT, p);
        }
        public int updateGroup(Device device)
        {
            var p = new Dictionary<string, object>() {
                {"@device_id", device.DeviceID },
                {"@group_device_id", device.GroupDeviceId }
            };

            _dataDao.execute(SQLConstant.TABLE_ACCOUNT_UPDATE_GROUP_DEVICE, p);
            return _dataDao.execute(SQLConstant.TABLE_DEVICE_UPDATE_GROUP, p);
        }
        public int updateInternet(Device device)
        {
            var p = new Dictionary<string, object>() {
                {"@device_id", device.DeviceID },
                {"@internet_id", device.InternetId }
            };

            return _dataDao.execute(SQLConstant.TABLE_DEVICE_UPDATE_INTERNET, p);
        }
        public int update(Device device)
        {
            var p = new Dictionary<string, object>() {
                //{"@id", device.Id },
                {"@status", device.Status },
                {"@description", device.Description },
                {"@updated_at", device.UpdatedAt },
                {"@key_press_for_text", device.KeyPressForText }
            };
            return _dataDao.execute(SQLConstant.TABLE_DEVICE_UPDATE, p);
        }
        public int updateRemoveFBLite(Device device)
        {
            var p = new Dictionary<string, object>() {
                {"@device_id", device.DeviceID },
                {"@total_fblite", device.TotalFBLite },
                {"@total_available", device.TotalAvailable }
            };

            return _dataDao.execute(SQLConstant.TABLE_DEVICE_UPDATE_REMOVE_FBLITE, p);
        }
        public int updateFBLite(Device device)
        {
            var p = new Dictionary<string, object>() {
                {"@device_id", device.DeviceID },
                {"@total_fblite", device.TotalFBLite }
            };

            return _dataDao.execute(SQLConstant.TABLE_DEVICE_UPDATE_FBLITE, p);
        }
        public int updateName(Device device)
        {
            var p = new Dictionary<string, object>() {
                {"@device_id", device.DeviceID },
                {"@name", device.Name }
            };

            return _dataDao.execute(SQLConstant.TABLE_DEVICE_UPDATE_NAME, p);
        }
        public int updateAccount(Device device)
        {
            var p = new Dictionary<string, object>() {
                {"@device_id", device.DeviceID },
                {"@total_account", device.TotalAccount },
                {"@total_available", device.TotalAvailable }
            };

            return _dataDao.execute(SQLConstant.TABLE_DEVICE_UPDATE_ACCOUNT, p);
        }
        public int updateIsBusy(int id, int is_busy)
        {
            var p = new Dictionary<string, object>() {
                {"@id", id },
                {"@is_busy", is_busy }
            };

            return _dataDao.execute(SQLConstant.TABLE_DEVICE_UPDATE_BUSY, p);
        }
        public int updateAvailable(Device device)
        {
            var p = new Dictionary<string, object>() {
                {"@id", device.Id },
                {"@total_available", device.TotalAvailable }
            };

            return _dataDao.execute(SQLConstant.TABLE_DEVICE_UPDATE_AVAILABLE, p);
        }
        public int updateDataMode(string deviceId, int dataMode)
        {
            var p = new Dictionary<string, object>() {
                {"@device_id", deviceId },
                {"@data_mode", dataMode }
            };

            return _dataDao.execute(SQLConstant.TABLE_DEVICE_UPDATE_DATA_MODE, p);
        }
        public int delete(Device device)
        {
            var p = new Dictionary<string, object> {
                  {"@id", device.Id },
            };
            return _dataDao.execute(SQLConstant.TABLE_DEVICE_DELETE, p);
        }

        public int truncate()
        {
            return _dataDao.execute(SQLConstant.TABLE_DEVICE_TRUNCATE);
        }
        public Device get(int id)
        {
            var p = new Dictionary<string, object> {
                {"@id", id }
            };
            var dataTable = _dataDao.query(SQLConstant.TABLE_DEVICE_SELECT_ONE, p);
            foreach (DataRow row in dataTable.Rows)
            {
                return Device.from(row);
            }

            return new Device();
        }
        public Device get(string deviceId)
        {
            var p = new Dictionary<string, object> {
                {"@device_id", deviceId }
            };
            var dataTable = _dataDao.query(SQLConstant.TABLE_DEVICE_SELECT_ONE_BY_DEVICE_ID, p);
            foreach (DataRow row in dataTable.Rows)
            {
                return Device.from(row);
            }

            return new Device();
        }
        public int getTotalDevice(int groupDeviceId, int internetId, int isBusy)
        {
            int total = 0;
            var p = new Dictionary<string, object> {
                {"@group_device_id", groupDeviceId },
                {"@internet_id", internetId },
                {"@is_busy", isBusy }
            };
            string sql = SQLConstant.TABLE_DEVICE_TOTAL_DEVICE;
            if(internetId==0)
            {
                sql = SQLConstant.TABLE_DEVICE_ALL_TOTAL_DEVICE;
            }
            var dataTable = _dataDao.query(sql, p);
            foreach (DataRow row in dataTable.Rows)
            {
                try
                {
                    total = Int32.Parse(row["total_device"].ToString());
                } catch { }
                break;
            }

            return total;
        }
        public int updateStatus(string deviceId, int status)
        {
            var p = new Dictionary<string, object>() {
                {"@device_id", deviceId },
                {"@status", status }
            };

            return _dataDao.execute(SQLConstant.TABLE_DEVICE_UPDATE_STATUS, p);
        }
        public int updateStatusToDisconnect()
        {
            return _dataDao.execute(SQLConstant.TABLE_DEVICE_UPDATE_STATUS_TO_DISCONNECT);
        }
        public DataTable listDataForActions(int groupDeviceId, string internetIds, int limit, string orderBy, int status = 1)
        {
            
            var p = new Dictionary<string, object>() {
                {"@group_device_id", groupDeviceId },              
                {"@limit", limit },
                {"@status", status }
            };
            string sql = SQLConstant.TABLE_DEVICE_SELECT_ALL_ACTION.Replace("{order_by}", orderBy);
            sql = sql.Replace("{internets}", internetIds);

            return _dataDao.query( sql, p);
        }
    }
}
