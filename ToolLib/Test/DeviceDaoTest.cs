using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolLib.Data;

namespace ToolLib
{

    public class DeviceDaoTest
    {
        public static void Test()
        {

            DataDao dataDao = new DataDao();
            DeviceDao deviceDao = new DeviceDao(dataDao);
            dataDao.dropTables();
            dataDao.createTables();
            var device1 = new Device()
            {
                DeviceID = "22X0219A31008771",
                Description = "Huawei",
                Status = 1,
                UpdatedAt = DateTime.Now.ToFileTimeUtc()
            };
            deviceDao.add(device1);
            var device2 = new Device()
            {
                DeviceID = "8T89626AA1971900021",
                Description = "Nokia",
                Status = 1,
                UpdatedAt = DateTime.Now.ToFileTimeUtc()
            };
            deviceDao.add(device2);
            List<Device> items = deviceDao.listDevice();
            foreach (var d in items)
            {
                Console.WriteLine(" id : " + d.Id + "," + d.Status + "," + d.UpdatedAt);
            }
        }
    }
}
