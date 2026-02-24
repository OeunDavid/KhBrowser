using System;

namespace ToolLib.Data
{
    public partial class SQLConstant
    {
        public const string DATABASE_NAME = "KhBrowser";

        ///// [Work Local]
        //public const string CONNECTION_STRING = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=KhBrowser;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;Application Name=\"ToolKHBrowser\";";
        //public const string MASTER_CONNECTION_STRING = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;Application Name=\"ToolKHBrowser\";";


        ///// [Server Test]
        public const string CONNECTION_STRING = "Data Source=192.168.69.83\\MSSQLSERVER23;Initial Catalog=KhBrowser;user id=sothea;password=Thea8989;Connection Timeout=30;Pooling=true;Max Pool Size=200;Encrypt=False;TrustServerCertificate=true;MultipleActiveResultSets=True;";
        public const string MASTER_CONNECTION_STRING = "Data Source=192.168.69.83\\MSSQLSERVER23;Initial Catalog=master;user id=sothea;password=Thea8989;Connection Timeout=30;Pooling=true;Max Pool Size=200;Encrypt=False;TrustServerCertificate=true;MultipleActiveResultSets=True;";

        ///// [David Home Test]
        //public const string CONNECTION_STRING ="Data Source=localhost;Persist Security Info=True;User ID=david;Password=David@123;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Application Name=\"SQL Server Management Studio\";";
        //public const string MASTER_CONNECTION_STRING = "Data Source=localhost;Persist Security Info=True;User ID=david;Password=David@123;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Application Name=\"SQL Server Management Studio\";";
    }
}

