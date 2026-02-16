using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolLib.Data;

namespace ToolKHBrowser.ToolLib.Data
{
    public class Cache
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public int Total { get; set; }
    }
    public interface ICacheDao
    {
        Cache Get(string key);
        int Set(string key, string value);
        int SetTotal(string key, long total);
    }
    public class CacheDao : ICacheDao
    {
        private IDataDao _dataDao;
        public CacheDao(IDataDao dataDao)
        {
            _dataDao = dataDao;
        }
        public Cache From(DataRow row)
        {
            int id = Int32.Parse(row["id"] + "");
            int total = Int32.Parse(row["total"] + "");
            string key = row["key"] + "";
            string value = row["value"] + "";

            return new Cache()
            {
                Id= id,
                Key= key,
                Total= total,
                Value = value
            };
        }
        public Cache Get(string key)
        {
            var p = new Dictionary<string, object> {
                {"@key", key }
            };
            var dataTable = _dataDao.query(SQLConstant.CacheSQL.TABLE_CACHE_SELECT_BY_KEY, p);
            foreach (DataRow row in dataTable.Rows)
            {
                return From(row);
            }

            return new Cache() { Id=0, Key= "", Value= ""};
        }
        public int Set(string key, string value)
        {
            var p = new Dictionary<string, object>() {
                {"@key", key },
                {"@value", value }
            };

            var data = Get(key);
            if(!string.IsNullOrEmpty(data.Key))
            {
                return _dataDao.execute(SQLConstant.CacheSQL.TABLE_CACHE_UPDATE, p);
            }

            return _dataDao.execute(SQLConstant.CacheSQL.TABLE_CACHE_INSERT, p);
        }
        public int SetTotal(string key, long total)
        {
            var p = new Dictionary<string, object>() {
                {"@key", key },
                {"@total", total }
            };

            var data = Get(key);
            if (!string.IsNullOrEmpty(data.Key))
            {
                return _dataDao.execute(SQLConstant.CacheSQL.TABLE_CACHE_UPDATE_TOTAL, p);
            }

            return _dataDao.execute(SQLConstant.CacheSQL.TABLE_CACHE_INSERT_TOTAL, p);
        }
    }
}
