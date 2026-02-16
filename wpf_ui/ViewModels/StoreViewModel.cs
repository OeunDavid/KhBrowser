using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using ToolLib.Data;

namespace WpfUI.ViewModels
{
    public interface IStoreViewModel
    {
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        ObservableCollection<Store> listDataForGrid(int status=-1);
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [STAThread]
        IStoreDao getGroupDevicesDao();
    }
    public class StoreViewModel: IStoreViewModel
    {
        private IStoreDao _storeDao;
        public StoreViewModel(IStoreDao groupDeviceDao)
        {
            this._storeDao = groupDeviceDao;
        }
        public IStoreDao getGroupDevicesDao()
        {
            return this._storeDao;
        }
        public ObservableCollection<Store> listDataForGrid(int status=-1)
        {
            var items = new ObservableCollection<Store>();
            
            DataTable table = _storeDao.listDataForGrid(); 
            if (table != null)
            {
                int key = 1;
                string text_status = "";
                foreach (DataRow row in table.Rows)
                {
                    int id = Int32.Parse(row["id"].ToString());
                    string name = row["name"].ToString();
                    
                    string note = "";
                    try { note = row["note"].ToString(); } catch { }
                    
                    int state = 0;
                    try { state = Int32.Parse(row["state"].ToString()); } catch { }

                    int isTemp = 0;
                    try { isTemp = Int32.Parse(row["is_temp"].ToString()); } catch { }

                    if (status!=-1)
                    {
                        if(state!=status)
                        {
                            continue;
                        }
                    }

                    if (state == 1)
                    {
                        text_status = "Active";
                    }
                    else if (state == 0)
                    {
                        text_status = "Inactive";
                    }
                    string temp = "No";
                    if (isTemp == 1)
                    {
                        temp = "Yes";
                    }

                    var d = new Store()
                    {
                        Key = key,
                        Id = id,
                        Name = name,
                        State = state,
                        IsTemp = isTemp,
                        Temp = temp,
                        TextStatus = text_status,
                        Note = note
                    };
                    key++;
                    items.Add(d);
                }
            }

            return items;
        }
    }
}
