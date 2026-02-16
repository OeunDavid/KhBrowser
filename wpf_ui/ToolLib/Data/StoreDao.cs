using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLib.Data
{
    public interface IStoreDao
    {
        DataTable listDataForGrid();
        int add(Store data);
        int update(Store data);
        Store Get(int id);
    }
    public class StoreDao : IStoreDao
    {
        private IDataDao _dataDao;
        public StoreDao(IDataDao dataDao)
        {
            _dataDao = dataDao;
        }
        public DataTable listDataForGrid()
        {
            DataTable table = _dataDao.query(SQLConstant.StoreSQL.TABLE_STORE_SELECT_ALL);

            return table;
        }
        public int add(Store data)
        {
            var p = new Dictionary<string, object>() {
                {"@name", data.Name},
                {"@note", data.Note},
                {"@state", data.State},
                {"@created_by", "system"},
                {"@updated_at", DateTimeOffset.Now.ToUnixTimeSeconds()},
                {"@is_temp", data.IsTemp}
            };

            return _dataDao.execute(SQLConstant.StoreSQL.TABLE_STORE_INSERT, p);
        }
        public int update(Store data)
        {
            var p = new Dictionary<string, object>() {
                {"@id", data.Id},
                {"@name", data.Name},
                {"@note", data.Note},
                {"@state", data.State},
                {"@updated_at", DateTimeOffset.Now.ToUnixTimeSeconds()},
                {"@is_temp", data.IsTemp}
            };

            return _dataDao.execute(SQLConstant.StoreSQL.TABLE_STORE_UPDATE, p);
        }
        public Store From(DataRow row)
        {
            int id = Int32.Parse(row["id"].ToString());
            string name = row["name"].ToString();
            
            string note = "";
            try { note = row["note"].ToString(); } catch { }
            
            int state = 0;
            try { state = Int32.Parse(row["state"].ToString()); } catch { }

            int isTemp = 0;
            try { isTemp = Int32.Parse(row["is_temp"].ToString()); } catch { }

            string text_status = "Inactive";
            if (state == 1)
            {
                text_status = "Active";
            }
            string temp = "No";
            if (isTemp == 1)
            {
                temp = "Yes";
            }

            var d = new Store()
            {
                Key = 1,
                Id = id,
                Name = name,
                State = state,
                IsTemp = isTemp,
                Temp = temp,
                TextStatus = text_status,
                Note = note
            };

            return d;
        }
        public Store Get(int id)
        {
            var p = new Dictionary<string, object> {
                {"@id", id }
            };
            var dataTable = _dataDao.query(SQLConstant.StoreSQL.TABLE_STORE_SELECT_ONE, p);
            foreach (DataRow row in dataTable.Rows)
            {
                return From(row);
            }

            return new Store() { Id = 0, Name = "", Temp = "", IsTemp = 0 };
        }
    }
}
