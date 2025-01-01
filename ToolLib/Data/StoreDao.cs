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
        int add(Store store);
        int update(Store store);
        int delete(Store store);
        List<Store> list();
    }
    public class StoreDao: IStoreDao
    {
        private IDataDao _dataDao;

        public StoreDao(IDataDao dataDao)
        {
            _dataDao = dataDao;
        }

        private Dictionary<string, object> to(Store store)
        {
            var p = new Dictionary<string, object>()
            {
                {"@id", store.Id },
                {"@name", store.Name },
                {"@note", store.Note },
                {"@created_by", store.CreatedBy },
                {"@state", store.State },
                {"@updated_at", store.UpdatedAt }
            };
            return p;
        }
        public int add(Store store)
        {
            var p = to(store);
            return _dataDao.execute(SQLConstant.TABLE_STORE_INSERT, p);
        }
        public int update(Store store)
        {
            var p = to(store);
            return _dataDao.execute(SQLConstant.TABLE_STORE_UPDATE, p);
        }
        public int delete(Store store)
        {
            var p = to(store);
            return _dataDao.execute(SQLConstant.TABLE_STORE_DELETE, p);

        }
        public List<Store> list()
        {
            List<Store> items = new List<Store>();
            var dataTable = _dataDao.query(SQLConstant.TABLE_STORE_SELECT_ALL);
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var item = Store.from(dataRow);
                items.Add(item);
            }
            return items;
        }
    }
}
