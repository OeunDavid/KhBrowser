using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolLib.Data;

namespace ToolKHBrowser.ToolLib.Data
{
    public interface IPagesDao
    {
        int add(Pages data);
        DataTable GetRecordsByUID(string uid);
        int deleteByUID(string uid);
    }
    public class PagesDao : IPagesDao
    {
        private IDataDao _dataDao;
        public PagesDao(IDataDao dataDao)
        {
            _dataDao = dataDao;
        }
        public int deleteByUID(string uid)
        {
            var p = new Dictionary<string, object>() {
                {"@uid", uid}
            };

            return _dataDao.execute(SQLConstant.TABLE_PAGES_DELETE_BY_UID, p);
        }
        public int add(Pages data)
        {
            var p = new Dictionary<string, object>() {
                {"@name", data.Name},
                {"@uid", data.UID},
                {"@page_id", data.PageId},
                {"@access_token", data.AccessToken},
                {"@status", data.Status},
            };

            return _dataDao.execute(SQLConstant.TABLE_PAGES_INSERT, p);
        }
        public DataTable GetRecordsByUID(string uid)
        {
            var p = new Dictionary<string, object>() {
                {"@uid", uid }
            };

            string sql = SQLConstant.TABLE_PAGES_SELECT_BY_UID;
            DataTable table = _dataDao.query(sql, p);

            return table;
        }
        public Pages From(DataRow row)
        {
            int id = Int32.Parse(row["id"].ToString());
            string name = row["name"].ToString();
            string uid = row["uid"].ToString();
            string page_id = row["page_id"].ToString();
            string access_token = row["access_token"].ToString();
            int cStatus = Int32.Parse(row["status"].ToString());

            var d = new Pages()
            {
                Id = id,
                Name = name,
                Status = cStatus,
                UID = uid,
                AccessToken = access_token,
                PageId = page_id
            };

            return d;
        }
    }
}
