using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolLib.Data;

namespace ToolKHBrowser.ToolLib.Data
{
    public interface IGroupsDao
    {
        int add(Groups data);
        int updatePending(string groupId, int pending);
        DataTable GetRecordsByUID(string uid);
        int delete(string uid, string group_id);
        bool isExist(string uid, string group_id);
        int deleteByUID(string uid);
    }
    public class GroupsDao : IGroupsDao
    {
        private IDataDao _dataDao;
        public GroupsDao(IDataDao dataDao)
        {
            _dataDao = dataDao;
        }
        public int deleteByUID(string uid)
        {
            var p = new Dictionary<string, object>() {
                {"@uid", uid}
            };

            return _dataDao.execute(SQLConstant.TABLE_GROUPSS_DELETE_BY_UID, p);
        }
        public int delete(string uid, string group_id)
        {
            var p = new Dictionary<string, object>() {
                {"@uid", uid},
                {"@group_id", group_id},
            };

            return _dataDao.execute(SQLConstant.TABLE_GROUPSS_DELETE, p);
        }
        public bool isExist(string uid, string group_id)
        {
            var p = new Dictionary<string, object> {
                {"@uid", uid },
                {"@group_id", group_id },
            };
            var dataTable = _dataDao.query(SQLConstant.TABLE_GROUPSS_SELECT_SINGLE_RECORD, p);
            bool isExist = false;
            foreach (DataRow row in dataTable.Rows)
            {
                isExist = true;
                break;
            }

            return isExist;
        }
        public int updatePending(string groupId, int pending)
        {
            var p = new Dictionary<string, object>() {
                {"@group_id", groupId},
                {"@pending", pending},
            };

            return _dataDao.execute(SQLConstant.TABLE_GROUPSS_UPDATE_PENDING, p);
        }
        public int add(Groups data)
        {
            var p = new Dictionary<string, object>() {
                {"@name", data.Name},
                {"@uid", data.UID},
                {"@page_id", data.PageId},
                {"@group_id", data.GroupId},
                {"@status", data.Status},
                {"@pending", data.Pending},
                {"@check_pending", data.Check_Pending},
                {"@member", data.Member},
            };

            return _dataDao.execute(SQLConstant.TABLE_GROUPSS_INSERT, p);
        }
        public DataTable GetRecordsByUID(string uid)
        {
            var p = new Dictionary<string, object>() {
                {"@uid", uid }
            };

            string sql = SQLConstant.TABLE_GROUPSS_SELECT_BY_UID;
            DataTable table = _dataDao.query(sql, p);

            return table;
        }
        public Groups From(DataRow row)
        {
            int id = Int32.Parse(row["id"].ToString());
            string name = row["name"].ToString();
            string uid = row["uid"].ToString();
            string page_id = row["page_id"].ToString();
            string group_id = row["group_id"].ToString();
            int cStatus = Int32.Parse(row["status"].ToString());
            int pending = Int32.Parse(row["pending"].ToString());
            int check_pending = Int32.Parse(row["check_pending"].ToString());

            var d = new Groups()
            {
                Id = id,
                Name = name,
                Status = cStatus,
                UID = uid,
                GroupId = group_id,
                PageId = page_id,

                Pending = pending,
                Check_Pending = check_pending
            };

            return d;
        }
    }
}
