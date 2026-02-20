using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolKHBrowser.ViewModels;

namespace ToolLib.Data
{
    public interface IAccountDao
    {
        int add(Account account);
        int update(Account account);
        int delete(Account account);
        int update(List<Account> accounts);
        int add(List<Account> accounts);
        int updatePassword(string uid, string password);
        int updateToken(string uid, string token);
        int updateTotalFriend(string uid, int totalFriend);
        int resetDevice(string uid);
        List<Account> listAccount();
        List<Account> listAccount(int groupDeviceId, string keyword = "", bool isTempStore = false);
        List<Account> listAccount(List<string> accountIDs);
        int storeBatch(List<Account> accounts);
        List<Account> listAccountNoDevices(int limit, int offset);
        List<Account> listByDevices(string deviceId);
        Account get(string id);
        bool isExist(string uid);
        int clearProxy(int groupDeviceId);
        int addProxy(string uid, string proxy);
        int addUserAgent(string uid, string user_agent);
        int clearGroupID(string uid);
        int clearNote(string uid);
        int addNote(string uid, string note);
        int addReelSourceVideo(string uid, string reel_source_video);
        int clearReelSourceVideo(string uid);
        int addTimelineSource(string uid, string source);
        int clearTimelineSource(string uid);
        int clearProxyByUID(string uid);
        int clearUserAgentByUID(string uid);
        int updateStatus(string uid, string description, int status);
        int updateDescription(string uid, string description);
        string updateAccountAction(string uid, int actionResult, string actionType);
        int updatePendingJoin(string uid, string groupIds);
        int updateGroupDevice(string uid, int groupDeviceId);
        int delete(string uid);
        int leaveGroup(string uid,int isLeave);
        int share(string uid, int isShare);
        int updateSecurity(string uid, string password, string twofa, int storeId, string mailPass = "");
        int getTotalAccountDie();
        int getTotalAccountLive();
        ObservableCollection<FbAccount> GetAccountList(string where, int limit, int offset=0);

        int UpdateLogin(string uid, int isLogin);
        int UpdateName(string uid, string name);
        int UpdateGender(string uid, string gender);
        int UpdateBirthday(string uid, string dob);
        int UpdateFriends(string uid, int totalFriends);
        int UpdateFriendsRequest(string uid, int friendRequest);
        int UpdateGroup(string uid, int total_group, string groupId="", string old_group_ids = "");
        int UpdateGroupIds(string uid, string groupIds = null, string old_group_ids = "");
        int active(string uid, int isActive);
        int Verify(string uid, int isVerify);
        int IsTwoFA(string uid, int isTwoFA);
        int TwoFA(string uid, string twofa);
        int updateUID(string uid, string newUID);
        int updateEmail(string uid, string email);
        int updateEmailPass(string uid, string mailPass);
        int updateCookie(string uid, string cookie);
        int updateShareToGroup(string uid, int total);
        int updateShareToTimeline(string uid, int total);
        int transfer(string uid, int storeId);
        int Password(string uid, string password);
        int tempStore(string uid, int storeId);
        int RunSQL(string sql);
        int UpdateToken(string uid, string token);
        int UpdatePrimaryLocation(string uid, string location);
        int UpdatePage(string uid, int totalPage, string pageIds);
        int UpdateInfo(FbAccount data);
        int updateStore(string uid, int storeId);
    }
    public class AccountDao:IAccountDao
    {
        private IDataDao _dataDao;

        public AccountDao( IDataDao dataDao)
        {
            _dataDao = dataDao;
        }

        public int RunSQL(string sql)
        {
            return _dataDao.execute(sql);
        }
        public int updatePassword(string uid, string password)
        {
            var p = new Dictionary<string, object>()
    {
        { "@uid", uid },
        { "@password", password }
    };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_PASSWORD, p);
        }

        public int updateToken(string uid, string token)
        {
            var p = new Dictionary<string, object>()
    {
        { "@uid", uid },
        { "@token", token }
    };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_TOKEN, p);
        }

        public int updateTotalFriend(string uid, int totalFriend)
        {
            var p = new Dictionary<string, object>()
    {
        { "@uid", uid },
        { "@total_friend", totalFriend }
    };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_FRIENDS, p);
        }

        public int UpdatePage(string uid, int totalPage, string pageIds)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid},
                { "@total_page", totalPage},
                { "@page_ids", pageIds}
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_PAGE, p);
        }
        public int UpdatePrimaryLocation(string uid, string primaryLocation)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid},
                { "@primaryLocation", primaryLocation}
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_PRIMARY_LOCATION, p);
        }
        public int UpdateToken(string uid, string token)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid},
                { "@token", token}
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_TOKEN, p);
        }
        public int updateShareToGroup(string uid, int total)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid},
                { "@total", total}
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_SHARE_TO_GROUP, p);
        }
        public int updateShareToTimeline(string uid, int total)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid},
                { "@total", total}
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_SHARE_TO_TIMELINE, p);
        }
        public int UpdateInfo(FbAccount data)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", data.UID},
                { "@gender", data.Gender},
                { "@name", data.Name},
                { "@dob", data.DOB},
                { "@email", data.Email}
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_INFO, p);
        }
        public int updateEmail(string uid, string email)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid},
                { "@email", email}
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_EMAIL, p);
        }
        public int updateEmailPass(string uid, string mailPass)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid},
                { "@mailPass", mailPass}
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_MAIL_PASS, p);
        }
        public int updateCookie(string uid, string cookie)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid},
                { "@cookie", cookie}
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_COOKIE, p);
        }
        public int updateUID(string uid, string newUID)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid},
                { "@new_uid", newUID}
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_UID, p);
        }
        public int TwoFA(string uid, string twofa)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid},
                { "@twofa", twofa}
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_TWOFA_BY_UID, p);
        }
        public int IsTwoFA(string uid, int isTwoFA)
        {
            var p = new Dictionary<string, object>()
            {
                { "@is_twofa", isTwoFA }
            };
            uid = "'" + uid.Replace(",", "','") + "'";
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_IS_TWOFA_BY_UID.Replace("{uid}", uid);

            return _dataDao.execute(sql, p);
        }
        public int Verify(string uid, int isVerify)
        {
            var p = new Dictionary<string, object>()
            {
                { "@is_verify", isVerify }
            };
            uid = "'" + uid.Replace(",", "','") + "'";
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_VERIFY_BY_UID.Replace("{uid}", uid);

            return _dataDao.execute(sql, p);
        }
        public int UpdateLogin(string uid, int isLogin)
        {
            var p = new Dictionary<string, object>()
            {
                { "@is_login", isLogin }
            };
            uid = "'" + uid.Replace(",", "','") + "'";
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_LOGIN_BY_UID.Replace("{uid}", uid);

            return _dataDao.execute(sql, p);
        }
        public int UpdateName(string uid, string name)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid},
                { "@name", name}
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_NAME, p);
        }
        public int UpdateGender(string uid, string gender)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid},
                { "@gender", gender}
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_GENDER, p);
        }
        public int UpdateBirthday(string uid, string dob)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid},
                { "@dob", dob}
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_BIRTHDAY, p);
        }
        public int UpdateFriends(string uid, int total_friends)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid},
                { "@total_friend", total_friends}
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_FRIENDS, p);
        }
        public int UpdateFriendsRequest(string uid, int friends_request)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid},
                { "@friends_request", friends_request}
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_FRIENDS_REQUEST, p);
        }
        public int UpdateGroup(string uid, int total_group, string groupIds="", string old_group_ids = "")
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid},
                { "@total_group", total_group}
            };
            if(!string.IsNullOrEmpty(groupIds))
            {
                UpdateGroupIds(uid, groupIds, old_group_ids);
            }

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_TOTAL_GROUP, p);
        }
        public int UpdateGroupIds(string uid, string groupIds = null, string old_group_ids = "")
        {
            var p = new Dictionary<string, object>()
            {
                { "@group_ids", groupIds},
                { "@old_group_ids", old_group_ids}
            };
            uid = "'" + uid.Replace(",", "','") + "'";
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_GROUP_ID.Replace("{uid}", uid);

            return _dataDao.execute(sql, p);
        }
        public ObservableCollection<FbAccount> GetAccountList(string where, int limit, int offset = 0)
        {
            var items = new ObservableCollection<FbAccount>();
            var p = new Dictionary<string, object>()
            {
                { "@limit", limit },
                { "@offset", offset }
            };
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_SELECT_ALL.Replace("{where}", where);
            var dataTable = _dataDao.query(sql,p);
            int key = 1;
            foreach (DataRow row in dataTable.Rows)
            {
                var a = Account.from(row);
                var acc = new FbAccount()
                {
                    Key= key,
                    Id = Convert.ToInt64(a.Id.ToString()),
                    UID = a.UID,
                    Name = a.Name,
                    Password = a.Password,
                    TwoFA = a.TwoFA,
                    Token = a.Token,

                    PendingJoin = a.PendingJoin,
                    Description = a.Description,
                    IsLogin = a.IsLogin,

                    Cookie = a.Cookie,
                    UserAgent = a.UserAgent,
                    Proxy = a.Proxy,

                    MailPass= a.MailPass
                };

                items.Add(acc);
                key++;
            }

            return items;
        }
        public int getTotalAccountDie()
        {
            int total = 0;
            var dataTable = _dataDao.query(SQLConstant.AccountSQL.TABLE_ACCOUNT_TOTAL_DIE);
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
            var dataTable = _dataDao.query(SQLConstant.AccountSQL.TABLE_ACCOUNT_TOTAL_LIVE);
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
                {"@cookie", account.Cookie },
                {"@description", account.Description },
                {"@status", account.Status },
                {"@store_id", account.StoreId },
                {"@total_group", account.TotalGroup }
            };

            return p;
        }
        public int add(Account account)
        {
            var p = to(account);

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_INSERT, p);
        }
        public int delete(string uid)
        {
            uid = "'" + uid.Replace(",", "','") + "'";
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_DELETE_BY_UID.Replace("{uid}", uid);

            return _dataDao.execute(sql);
        }
        public int transfer(string uid, int storeId)
        {
            var p = new Dictionary<string, object>()
            {
                { "@store_id", storeId }
            };
            uid = "'" + uid.Replace(",", "','") + "'";
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_TRANSFER_BY_UID.Replace("{uid}", uid);

            return _dataDao.execute(sql, p);
        }
        public int tempStore(string uid, int storeId)
        {
            var p = new Dictionary<string, object>()
            {
                { "@temp_store_id", storeId },
                { "@temp_order", DateTime.Now.ToFileTime() }
            };
            uid = "'" + uid.Replace(",", "','") + "'";
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_TEMP_STORE_BY_UID.Replace("{uid}", uid);

            return _dataDao.execute(sql, p);
        }
        public int active(string uid, int isActive)
        {
            var p = new Dictionary<string, object>()
            {
                { "@is_active", isActive }
            };
            uid = "'" + uid.Replace(",", "','") + "'";
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_ACTIVE_BY_UID.Replace("{uid}", uid);

            return _dataDao.execute(sql, p);
        }
        public int Password(string uid, string password)
        {
            var p = new Dictionary<string, object>()
            {
                { "@password", password }
            };
            uid = "'" + uid.Replace(",", "','") + "'";
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_PASSWORD_BY_UID.Replace("{uid}", uid);

            return _dataDao.execute(sql, p);
        }
        public int share(string uid, int isShare)
        {
            var p = new Dictionary<string, object>()
            {
                { "@is_share", isShare }
            };
            uid = "'" + uid.Replace(",","','") + "'";
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_SHARE_BY_UID.Replace("{uid}", uid);

            return _dataDao.execute(sql, p);
        }
        public int leaveGroup(string uid, int isLeave)
        {
            var p = new Dictionary<string, object>()
            {
                { "@is_leave", isLeave }
            };
            uid = "'" + uid.Replace(",", "','") + "'";
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_LEAVE_GROUP_BY_UID.Replace("{uid}", uid);
            return _dataDao.execute(sql,p);
        }
        public int updateSecurity(string uid, string password, string twofa, int storeId, string mailPass="")
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid },
                { "@password", password },
                { "@twofa", twofa },
                { "@store_id", storeId },
                { "@mailPass", mailPass }
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_SECURITY, p);
        }
        public int updateStore(string uid, int storeId)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid },
                { "@store_id", storeId }
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_STORE, p);
        }
        public int updatePendingJoin(string uid, string groupIds)
        {
            var p = new Dictionary<string, object>()
            {
                { "@pending_join", groupIds }
            };

            uid = "'" + uid.Replace(",", "','") + "'";
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_PENDING_JOIN_BY_UID.Replace("{uid}", uid);

            return _dataDao.execute(sql, p);
        }
        public int updateGroupDevice(string uid, int groupDeviceId)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid },
                { "@group_device_id", groupDeviceId }
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_GROUP_DEVICE_BY_UID, p);
        }
        public int updateStatus(string uid, string description, int status)
        {
            var p = new Dictionary<string, object>()
            {
                { "@description", description },
                { "@status", status },
                { "@updated_at", DateTime.Now.ToFileTime() }
            };
            uid = "'" + uid.Replace(",", "','") + "'";
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_STATUS.Replace("{uid}", uid);

            return _dataDao.execute(sql, p);
        }
        public int updateDescription(string uid, string description)
        {
            var p = new Dictionary<string, object>()
            {
                { "@description", description },
                { "@updated_at", DateTime.Now.ToFileTime() }
            };
            uid = "'" + uid.Replace(",", "','") + "'";
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE_DESCRIPTION.Replace("{uid}", uid);

            return _dataDao.execute(sql, p);
        }
        public string updateAccountAction(string uid, int actionResult, string actionType)
        {
            string description = "";
            if (actionResult == 2)
            {
                // account disabled
                description = "Account disabled";
                actionResult = 0;
            }
            else if (actionResult == 3)
            {
                // account locked
                description = "Account Locked";
                actionResult = 0;
            }
            else if (actionResult == 4)
            {
                // incorrect uid or password
                description = "Incorrect UID or Password";
                actionResult = 0;
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
            else if (actionResult == 7)
            {
                // Confirm Your Identify
                description = "Confirm Your Identify";
                actionResult = 0;
            }
            else if (actionResult == 8)
            {
                // We suspended your account
                description = "We Suspended Your Account";
                actionResult = 0;
            }
            else if (actionResult == 9)
            {
                // Login Approval Needed
                description = "Login Approval Needed";
                actionResult = 0;
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

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE, p);
        }
        public int addProxy(string uid, string proxy)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid },
                { "@proxy", proxy }
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_PROXY, p);
        }
        public int addUserAgent(string uid, string user_agent)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid },
                { "@user_agent", user_agent }
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_USER_AGENT, p);
        }
        public int clearProxy(int groupDeviceId)
        {
            var p = new Dictionary<string, object>()
            {
                { "@group_device_id", groupDeviceId }
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_CLEAR_PROXY, p);
        }
        public int addNote(string uid, string note)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid },
                { "@note", note }
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_NOTE, p);
        }
        public int clearNote(string uid)
        {
            uid = "'" + uid.Replace(",", "','") + "'";
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_CLEAR_NOTE_BY_UID.Replace("{uid}", uid);

            return _dataDao.execute(sql);
        }
        public int clearGroupID(string uid)
        {
            uid = "'" + uid.Replace(",", "','") + "'";
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_CLEAR_GROUP_ID_BY_UID.Replace("{uid}", uid);

            return _dataDao.execute(sql);
        }
        public int addReelSourceVideo(string uid, string reel_source_video)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid },
                { "@reel_source_video", reel_source_video }
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_REEL_SOURCE_VIDEO, p);
        }
        public int clearReelSourceVideo(string uid)
        {
            uid = "'" + uid.Replace(",", "','") + "'";
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_CLEAR_REEL_SOURCE_VIDEO_BY_UID.Replace("{uid}", uid);

            return _dataDao.execute(sql);
        }
        public int addTimelineSource(string uid, string source)
        {
            var p = new Dictionary<string, object>()
            {
                { "@uid", uid },
                { "@timeline_source", source }
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_TIMELINE_SOURCE, p);
        }
        public int clearTimelineSource(string uid)
        {
            uid = "'" + uid.Replace(",", "','") + "'";
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_CLEAR_TIMELINE_SOURCE_BY_UID.Replace("{uid}", uid);

            return _dataDao.execute(sql);
        }
        public int clearProxyByUID(string uid)
        {
            uid = "'" + uid.Replace(",", "','") + "'";
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_CLEAR_PROXY_BY_UID.Replace("{uid}", uid);

            return _dataDao.execute(sql);
        }
        public int clearUserAgentByUID(string uid)
        {
            uid = "'" + uid.Replace(",", "','") + "'";
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_CLEAR_USER_AGENT_BY_UID.Replace("{uid}", uid);

            return _dataDao.execute(sql);
        }
        public int delete(Account account)
        {
            var p = new Dictionary<string, object>()
            {
                { "@id", account.Id }
            };

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_DELETE, p);
        }
        public List<Account> listAccount()
        {
            List<Account> result = new List<Account>();
            DataTable table = _dataDao.query(SQLConstant.AccountSQL.TABLE_ACCOUNT_SELEC_ALL);
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
            DataTable table = _dataDao.query(SQLConstant.AccountSQL.TABLE_ACCOUNT_SELECT_BY_UID, p);
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
                countInsert = _dataDao.executeBatch(SQLConstant.AccountSQL.TABLE_ACCOUNT_INSERT, insertList);
            }
            int countUpdate = 0;
            if( updateList.Count > 0)
            {
                //countUpdate = _dataDao.executeBatch(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE, updateList);
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

            return _dataDao.executeBatch(SQLConstant.AccountSQL.TABLE_ACCOUNT_UPDATE, pList);
        }
        public int add(List<Account> accounts)
        {
            List<Dictionary<string, object>> pList = new List<Dictionary<string, object>>();
            foreach (var ac in accounts)
            {
                var p = to(ac);
                pList.Add(p);
            }

            return _dataDao.executeBatch(SQLConstant.AccountSQL.TABLE_ACCOUNT_INSERT, pList);
        }
        public List<Account> listAccount(int storeId, string keyword="", bool isTempStore= false)
        {
            List<Account> items = new List<Account>();
            var p = new Dictionary<string, object> {
                {"@store_id",storeId }
            };
            string sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_SELECT_ALL_BY_STORE;
            if (isTempStore)
            {
                p = new Dictionary<string, object> {
                    {"@temp_store_id",storeId }
                };
                sql = SQLConstant.AccountSQL.TABLE_ACCOUNT_SELECT_ALL_BY_TEMP_STORE;
            }
            string where = "";
            if(!string.IsNullOrEmpty(keyword))
            {
                if (keyword.Contains(","))
                {
                    where = " AND accounts.uid in (" + keyword + ")";
                }
                else
                {
                    where = " AND (accounts.uid LIKE '%" + keyword + "%' OR accounts.name LIKE '%" + keyword + "%' OR accounts.password LIKE '%" + keyword + "%' OR twofa LIKE '%" + keyword + "%' OR proxy LIKE '%" + keyword + "%' OR accounts.description LIKE '%" + keyword + "%' OR email LIKE '%" + keyword + "%')";
                }
            }
            sql= sql.Replace("{where}", where);

            var dataTable = _dataDao.query(sql, p);
            
            foreach (DataRow row in dataTable.Rows)
            {
                var acc = Account.from(row);
                items.Add(acc);
            }

            return items;
        }
        public List<Account> listAccountNoDevices(int limit, int offset)
        {
            List<Account> items = new List<Account>();
            var p = new Dictionary<string, object> {
                {"@limit",limit },
                {"@offset", offset }
            };
            var dataTable = _dataDao.query(SQLConstant.AccountSQL.TABLE_ACCOUNT_SELECT_ALL_NO_DEVICE, p);
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
            var dataTable = _dataDao.query(SQLConstant.AccountSQL.TABLE_ACCOUNT_SELECT_BY_DEVICE, p);
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

            return _dataDao.execute(SQLConstant.AccountSQL.TABLE_ACCOUNT_RESET_DEVICE_BY_UID, p);
        }
        public Account get(string id)
        {
            var p = new Dictionary<string, object> {
                {"@id", id }
            };
            var dataTable = _dataDao.query(SQLConstant.AccountSQL.TABLE_ACCOUNT_SELECT_ONE, p);
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
            var dataTable = _dataDao.query(SQLConstant.AccountSQL.TABLE_ACCOUNT_SELECT_ONE_BY_UID, p);
            foreach (DataRow row in dataTable.Rows)
            {
                b = true;
                break;
            }

            return b;
        }
    }
}
