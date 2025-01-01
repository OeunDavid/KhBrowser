using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolLib.Data;

namespace ToolLib.Test
{
    public class ActivityLogDaoTest
    {
        public static void test()
        {
            var dataDao = new DataDao();
            var activityLogDao = new ActivityLogDao(dataDao);
            dataDao.dropTables();
            dataDao.createTables();
            var log1 = new ActivityLog() {
                ActionName = "test",
                DeviceId = "xxx1",
                UID = "xx1",
                ActionDate = DateTime.Now.ToFileTimeUtc(),
            };
            var log2 = new ActivityLog()
            {
                ActionName = "test_2",
                DeviceId = "xxx1_2",
                UID = "xx222",
                ActionDate = DateTime.Now.ToFileTimeUtc(),
            };
            activityLogDao.add(log1);
            activityLogDao.add(log2);
            Console.WriteLine("done add");
            var items = activityLogDao.list(100,0);
           
            foreach( var item in items)
            {
                Console.WriteLine(" item : " + item.Id + " , " + item.ActionName + " , " + item.DeviceId + " " + item.UID + " -" + item.ActionDate);
            }
        }
    }
}
