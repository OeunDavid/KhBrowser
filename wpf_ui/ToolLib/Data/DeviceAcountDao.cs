using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLib.Data
{
    public class DeviceAcountDao
    {
        private DataDao _dataDao;
        public DeviceAcountDao(DataDao dataDao)
        {
            _dataDao = dataDao;
        }
        public int add(List<DeviceAccount> items)
        {
            var pList = new List<Dictionary<string, object>>();
            foreach( var item in items)
            {
                var p = new Dictionary<string, object>() {
                    { "@account_id", item.AccountId },
                    {"@device_id", item.DeviceId }
                };
                pList.Add(p);
            }

            return _dataDao.executeBatch(SQLConstant.TABLE_DEVICE_ACCOUNT_INSERT_BATCH, pList);
        }
        public int delete( DeviceAccount deviceAccount)
        {
            var p = new Dictionary<string, object>()
            {
                {"@account_id", deviceAccount.AccountId },
                {"@device_id", deviceAccount.DeviceId }
            };
            return _dataDao.execute(SQLConstant.TABLE_DEVICE_ACCOUNT_DELETE, p);
        }

    }
}
