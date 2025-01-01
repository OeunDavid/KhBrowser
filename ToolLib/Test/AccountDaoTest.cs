using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolLib.Data;

namespace ToolLib.Test
{
    public class AccountDaoTest
    {
        public static void test()
        {
            var account1 = new Account() { 
                //Id ="xxxxxx1111",
                Password ="xxxx",
                Description="test",
                Status = 0,
                UpdatedAt = DateTime.Now.ToFileTimeUtc()
            };
            var account2 = new Account()
            {
                //Id = "xxxxxx222",
                Password = "xxxx",
                Description = "test2",
                Status = 0,
                UpdatedAt = DateTime.Now.ToFileTimeUtc()
            };
            Console.WriteLine("done insert");
            var dataDao = new DataDao();
            dataDao.dropTables();
            dataDao.createTables();
            var accountDao = new AccountDao(dataDao,new DeviceDao(dataDao));
            accountDao.add(account1);
            accountDao.add(account2);
            var accounts = accountDao.listAccount();
            foreach(var acc in accounts)
            {
                Console.WriteLine("account : " + acc.Id + " -" + acc.Password + ", " + acc.Description + " --" + acc.Status);

            }
            Console.WriteLine("done query");

            foreach(var acc in accounts)
            {
                acc.Description = "update";
                accountDao.update(acc);
            }
            Console.WriteLine("done update");
            foreach( var acc in accounts)
            {
                accountDao.delete(acc);
            }
            Console.WriteLine("done delete");




        }
    }
}
