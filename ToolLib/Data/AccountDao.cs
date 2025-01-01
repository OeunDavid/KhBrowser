using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ToolLib.Data
{
    public interface IAccountDao
    {
        int add(Account account);
        int update(Account account);
        int delete(Account account);
        int update(List<Account> accounts);
        int add(List<Account> accounts);
        int resetDevice(string uid);
        List<Account> listAccount();
        List<Account> listAccount(int groupDeviceId);
        List<Account> listAccount(int limit, int offset);
        List<Account> listAccount(List<string> accountIDs);
        int storeBatch(List<Account> accounts);
        void suffleAccount(List<Device>devices, bool forceAll);
        List<Account> listAccountNoDevices(int limit, int offset);
        List<Account> listByDevices(string deviceId);
        //List<AccountGroup> accountGroupsByDeviceId(string deviceId);
        Account get(string id);
        bool isExist(string uid);
        int clearProxy(int groupDeviceId);
        int addProxy(string uid, string proxy);
        int clearProxyByUID(string uid);
        int updateStatus(string uid, string description, int status);
        string updateAccountAction(string uid, int actionResult, string actionType);
        int updatePendingJoin(string uid, string groupIds);
        int updateGroupDevice(string uid, int groupDeviceId);
        int delete(string uid);
        int leaveGroup(string uid,int isLeave);
        int shareType(string uid,int shareTypeId);
        int updateSecurity(string uid, string password, string twofa);
        int getTotalAccountDie();
        int getTotalAccountLive();
    }
    public class AccountDao:IAccountDao
    {
        private IDataDao _dataDao;
        private IDeviceDao _deviceDao;
        public AccountDao( IDataDao dataDao, IDeviceDao deviceDao)
        {
            _dataDao = dataDao;
            _deviceDao = deviceDao;
        }
        public int getTotalAccountDie()
        {
            int total = 0;
            var dataTable = _dataDao.query(SQLConstant.TABLE_ACCOUNT_TOTAL_DIE);
            foreach (DataRow row in dataTable.Rows)
            {
                total = Int32.Parse(row["total_account_die"].ToString().Trim());
                break;
            }

            return total;
        }
        public int getTotalAccountLive()
        {
            int total = 0;
            var dataTable = _dataDao.query(SQLConstant.TABLE_ACCOUNT_TOTAL_LIVE);
            foreach (DataRow row in dataTable.Rows)
            {
                total = Int32.Parse(row["total_account_live"].ToString().Trim());
                break;
            }

            return total;
        }
        private Dictionary<string,object>to(Account account)
        {
            var p = new Dictionary<string, object>() {
                {"@id", account.Id },
                {"@uid", account.UID },
                {"@name", account.Name },
                {"@password", account.Password },
                {"@twofa", account.TwoFA },
                {"@token", account.Token },
                {"@description", account.Description },
                {"@updated_at", account.UpdatedAt },
                {"@status", account.Status },
                {"@device_id", account.DeviceId },
                {"@group_device_id", account.GroupDeviceId },
                {"@share_type_id", account.ShareTypeId },
                {"@total_group", account.TotalGroup }
            };

            return p;
        }
        public int add(Account account)
        {
            var p = to(account);

            return _dataDao.execute(SQLConstant.TABLE_ACCOUNT_INSERT, p);
        }
        public int delete(string uid)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid }
            };

            return _dataDao.execute(SQLConstant.TABLE_ACCOUNT_DELETE_BY_UID, p);
        }
        public int shareType(string uid, int shareTypeId)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid },
                { "@share_type_id", shareTypeId }
            };

            return _dataDao.execute(SQLConstant.TABLE_ACCOUNT_UPDATE_SHARE_TYPE_BY_UID, p);
        }
        public int leaveGroup(string uid, int isLeave)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid },
                { "@is_leave", isLeave }
            };

            return _dataDao.execute(SQLConstant.TABLE_ACCOUNT_UPDATE_LEAVE_GROUP_BY_UID, p);
        }
        public int updateSecurity(string uid, string password, string twofa)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid },
                { "@password", password },
                { "@twofa", twofa }
            };

            return _dataDao.execute(SQLConstant.TABLE_ACCOUNT_UPDATE_SECURITY, p);
        }
        public int updatePendingJoin(string uid, string groupIds)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid },
                { "@pending_join", groupIds }
            };

            return _dataDao.execute(SQLConstant.TABLE_ACCOUNT_PENDING_JOIN_BY_UID, p);
        }
        public int updateGroupDevice(string uid, int groupDeviceId)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid },
                { "@group_device_id", groupDeviceId }
            };

            return _dataDao.execute(SQLConstant.TABLE_ACCOUNT_UPDATE_GROUP_DEVICE_BY_UID, p);
        }
        public int updateStatus(string uid, string description, int status)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid },
                { "@description", description },
                { "@status", status },
                { "@updated_at", DateTime.Now.ToFileTime() }
            };

            return _dataDao.execute(SQLConstant.TABLE_ACCOUNT_UPDATE_STATUS, p);
        }
        public string updateAccountAction(string uid, int actionResult, string actionType)
        {
            string description = "";
            if (actionResult == 2)
            {
                // account disabled
                description = "Account disabled";
            }
            else if (actionResult == 3)
            {
                // account locked
                description = "Account Locked";
            }
            else if (actionResult == 4)
            {
                // incorrect uid or password
                description = "Incorrect UID or Password";
                actionResult = 1;
            }
            else if (actionResult == 5)
            {
                // need login
                description = "Need Login";
                actionResult = 1;
            }
            else if (actionResult == 6)
            {
                // logout
                description = "Account Logout";
                actionResult = 1;
            }
            else if (actionResult == 0)
            {
                // failed
                description = "Failed"; 
                actionResult = 1;
            }
            else
            {
                actionResult = 1;
                if (actionType == "login")
                {
                    description = "Login";
                }
                else if (actionType == "active")
                {
                    description = "Active";
                }
                else if (actionType == "join")
                {
                    description = "Join Groups";
                }
                else if (actionType == "leave")
                {
                    description = "Leave Groups";
                }
                else
                {
                    description = "Unknow";
                }
            }
            updateStatus(uid, description, actionResult);

            return description;
        }
        public int update(Account account)
        {
            var p = to(account);

            return _dataDao.execute(SQLConstant.TABLE_ACCOUNT_UPDATE, p);
        }
        public int addProxy(string uid, string proxy)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid },
                { "@proxy", proxy }
            };

            return _dataDao.execute(SQLConstant.TABLE_ACCOUNT_ADD_PROXY, p);
        }
        public int clearProxy(int groupDeviceId)
        {
            var p = new Dictionary<string, object>()
            {
                { "@group_device_id", groupDeviceId }
            };

            return _dataDao.execute(SQLConstant.TABLE_ACCOUNT_CLEAR_PROXY, p);
        }
        public int clearProxyByUID(string uid)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid }
            };

            return _dataDao.execute(SQLConstant.TABLE_ACCOUNT_CLEAR_PROXY_BY_UID, p);
        }
        public int delete(Account account)
        {
            var p = new Dictionary<string, object>()
            {
                { "@id", account.Id }
            };

            return _dataDao.execute(SQLConstant.TABLE_ACCOUNT_DELETE, p);
        }
        public List<Account> listAccount()
        {
            List<Account> result = new List<Account>();
            DataTable table = _dataDao.query(SQLConstant.TABLE_ACCOUNT_SELEC_ALL);
            foreach(DataRow row in table.Rows)
            {
                var acc = Account.from(row);
                result.Add(acc);
            }

            return result;
        }
        public List<Account> listAccount(List<string> accountIDs)
        {
            List<Account> result = new List<Account>();
            if( accountIDs.Count == 0)
            {
                return result;
            }
            string idList = "'" + String.Join("','", accountIDs.ToArray()) +"'";
            var p = new Dictionary<string, object>() {
                {"@uid", idList }
            };
            //Console.WriteLine("UID: " + idList);
            DataTable table = _dataDao.query(SQLConstant.TABLE_ACCOUNT_SELECT_BY_UID, p);
            foreach (DataRow row in table.Rows)
            {
                var acc = Account.from(row);
                result.Add(acc);
            }

            return result;
        }
        public int storeBatch(List<Account> accounts)
        {
            var idList = new List<string>();
            foreach(var acc in accounts)
            {
                idList.Add(acc.UID);
            }
            List<Account> existList = listAccount(idList);

            var dic = new Dictionary<string, Account>();
            foreach(var a in existList)
            {
                dic[a.UID] = a;
            }
            var updateList = new List<Dictionary<string,object>>();
            var insertList = new List<Dictionary<string,object>>();
            foreach( var acc in accounts)
            {
                if(dic.ContainsKey(acc.UID))
                {
                    //updateList.Add(to(acc));
                }
                else
                {
                    insertList.Add(to(acc));
                }
            }
            int countInsert = 0;
            if( insertList.Count > 0)
            {
                countInsert = _dataDao.executeBatch(SQLConstant.TABLE_ACCOUNT_INSERT, insertList);
            }
            int countUpdate = 0;
            if( updateList.Count > 0)
            {
                //countUpdate = _dataDao.executeBatch(SQLConstant.TABLE_ACCOUNT_UPDATE, updateList);
            }

            return countInsert + countUpdate;
        }
        public int update(List<Account> accounts)
        {
            List<Dictionary<string, object>> pList = new List<Dictionary<string, object>>();
            foreach( var ac in accounts)
            {
                var p = to(ac);
                pList.Add(p);
            }

            return _dataDao.executeBatch(SQLConstant.TABLE_ACCOUNT_UPDATE, pList);
        }
        public int add(List<Account> accounts)
        {
            List<Dictionary<string, object>> pList = new List<Dictionary<string, object>>();
            foreach (var ac in accounts)
            {
                var p = to(ac);
                pList.Add(p);
            }

            return _dataDao.executeBatch(SQLConstant.TABLE_ACCOUNT_INSERT, pList);
        }
        public List<Account> listAccount(int group_device_id)
        {
            List<Account> items = new List<Account>();
            var p = new Dictionary<string, object> {
                {"@group_device_id",group_device_id }
            };
            var dataTable = _dataDao.query(SQLConstant.TABLE_ACCOUNT_SELECT_ALL_BY_STORE, p);
            
            foreach (DataRow row in dataTable.Rows)
            {
                var acc = Account.from(row);
                items.Add(acc);
            }

            return items;
        }
        public List<Account> listAccount(int limit, int offset)
        {
            List<Account> items = new List<Account>();
            var p = new Dictionary<string, object> {
                {"@limit",limit },
                {"@offset", offset }
            };
            var dataTable = _dataDao.query(SQLConstant.TABLE_ACCOUNT_SELECT_ALL_LIMIT, p);
            foreach(DataRow row in dataTable.Rows)
            {
                var acc = Account.from(row);
                items.Add(acc);
            }

            return items;
        }
        public void suffleAccount(List<Device> devices, bool forceAll)
        {
            var deviceList = new ItemList<Device>(devices);
            var limit = 2000;
            var offset = 0;
            List<Account> items;
            while (true)
            {
                 if(forceAll)
                 {
                    items = listAccount(limit, offset);
                 }
                 else
                 {
                    items = listAccountNoDevices(limit, offset);
                 }
                 if( items.Count == 0)
                 {
                    break;
                 }
                 offset += limit;
                foreach (var acc in items)
                {
                    var d = deviceList.Next();
                    acc.DeviceId = d.DeviceID;
                    acc.UpdatedAt = DateTime.Now.ToFileTimeUtc();
                }
                update(items);

                Thread.Sleep(100);
            }
        }
        public List<Account> listAccountNoDevices(int limit, int offset)
        {
            List<Account> items = new List<Account>();
            var p = new Dictionary<string, object> {
                {"@limit",limit },
                {"@offset", offset }
            };
            var dataTable = _dataDao.query(SQLConstant.TABLE_ACCOUNT_SELECT_ALL_NO_DEVICE, p);
            foreach (DataRow row in dataTable.Rows)
            {
                var acc = Account.from(row);
                items.Add(acc);
            }

            return items;
        }
        public List<Account> listByDevices(string deviceId)
        {
            List<Account> items = new List<Account>();
            var p = new Dictionary<string, object>() {
                {"@device_id", deviceId }
            };
            var dataTable = _dataDao.query(SQLConstant.TABLE_ACCOUNT_SELECT_BY_DEVICE, p);
            foreach(DataRow row in dataTable.Rows)
            {
                var item = Account.from(row);
                items.Add(item);
            }

            return items;
        }
        public int resetDevice(string uid)
        {
            var p = new Dictionary<string, object>() {
                {"@uid", uid }
            };

            return _dataDao.execute(SQLConstant.TABLE_ACCOUNT_RESET_DEVICE_BY_UID, p);
        }
        public Account get(string id)
        {
            var p = new Dictionary<string, object> {
                {"@id", id }
            };
            var dataTable = _dataDao.query(SQLConstant.TABLE_ACCOUNT_SELECT_ONE, p);
            foreach(DataRow row in dataTable.Rows)
            {
                return Account.from(row);
            }

            return null;
        }
        public bool isExist(string uid)
        {
            bool b= false;
            var p = new Dictionary<string, object> {
                {"@uid", uid }
            };
            var dataTable = _dataDao.query(SQLConstant.TABLE_ACCOUNT_SELECT_ONE_BY_UID, p);
            foreach (DataRow row in dataTable.Rows)
            {
                b = true;
                break;
            }

            return b;
        }
    }
}
