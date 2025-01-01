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
            DataTable table = _dataDao.query(SQLConstant.TABLE_GROUP_DEVICES_SELECT_ALL);

            return table;
        }
        public int add(Store data)
        {
            var p = new Dictionary<string, object>() {
                {"@name", data.Name},
                {"@description", data.Description},
                {"@status", data.Status},
                {"@is_temp", data.IsTemp}
            };

            return _dataDao.execute(SQLConstant.TABLE_GROUP_DEVICES_INSERT, p);
        }
        public int update(Store data)
        {
            var p = new Dictionary<string, object>() {
                {"@id", data.Id},
                {"@name", data.Name},
                {"@description", data.Description},
                {"@status", data.Status},
                {"@is_temp", data.IsTemp}
            };

            return _dataDao.execute(SQLConstant.TABLE_GROUP_DEVICES_UPDATE, p);
        }
        public Store From(DataRow row)
        {
            int id = Int32.Parse(row["id"].ToString());
            string name = row["name"].ToString();
            string description = row["description"].ToString();
            int cStatus = Int32.Parse(row["status"].ToString());
            int isTemp = Int32.Parse(row["is_temp"].ToString());
            string text_status = "Inactive";
            if (cStatus == 1)
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
                Status = cStatus,
                IsTemp = isTemp,
                Temp = temp,
                TextStatus = text_status,
                Description = description
            };

            return d;
        }
        public Store Get(int id)
        {
            var p = new Dictionary<string, object> {
                {"@id", id }
            };
            var dataTable = _dataDao.query(SQLConstant.TABLE_GROUP_DEVICES_ONE_RECORD, p);
            foreach (DataRow row in dataTable.Rows)
            {
                return From(row);
            }

            return new Store() { Id = 0, Name = "", Temp = "", IsTemp = 0 };
        }
    }
}
