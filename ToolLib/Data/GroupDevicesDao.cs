using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLib.Data
{
    public interface IGroupDevicesDao
    {
        DataTable listDataForGrid();
        int add(GroupDevices data);
        int update(GroupDevices data);
    }
    public class GroupDevicesDao : IGroupDevicesDao
    {
        private IDataDao _dataDao;
        public GroupDevicesDao(IDataDao dataDao)
        {
            _dataDao = dataDao;
        }
        public DataTable listDataForGrid()
        {
            DataTable table = _dataDao.query(SQLConstant.TABLE_GROUP_DEVICES_SELECT_ALL);

            return table;
        }
        public int add(GroupDevices data)
        {
            var p = new Dictionary<string, object>() {
                {"@name", data.Name},
                {"@description", data.Description},
                {"@status", data.Status}
            };

            return _dataDao.execute(SQLConstant.TABLE_GROUP_DEVICES_INSERT, p);
        }
        public int update(GroupDevices data)
        {
            var p = new Dictionary<string, object>() {
                {"@id", data.Id},
                {"@name", data.Name},
                {"@description", data.Description},
                {"@status", data.Status}
            };

            return _dataDao.execute(SQLConstant.TABLE_GROUP_DEVICES_UPDATE, p);
        }
    }
}
