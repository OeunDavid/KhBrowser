using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLib.Data
{
    public interface IDataDao
    {
        DataTable query(string query, Dictionary<string, object> args = null);
        int executeBatch(string sql, List<Dictionary<string, object>> items);
        int execute(string sql, Dictionary<string, object> args = null);
        int createTables();
        int dropTables();
    }
    public class DataDao:IDataDao
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public DataTable query(string query, Dictionary<string, object> args = null)
        {
            if (string.IsNullOrEmpty(query.Trim()))
                return null;

            using (var con = new SQLiteConnection("Data Source="+SQLConstant.DB_NAME))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(query, con))
                {
                    if (args != null)
                    {

                        foreach (KeyValuePair<string, object> entry in args)
                        {
                            //Console.WriteLine("Key: "+entry.Key+", Value: "+entry.Value);
                            cmd.Parameters.AddWithValue(entry.Key, entry.Value);
                        }
                    }
                    //Console.WriteLine(cmd.CommandText);
                    var da = new SQLiteDataAdapter(cmd);
                    var dt = new DataTable();
                    da.Fill(dt);

                    da.Dispose();
                    return dt;
                }
            }
        }

        public int executeBatch(string sql, List<Dictionary<string, object>> items)
        {
            int numberOfRowsAffected = 0;
            using (var con = new SQLiteConnection("Data Source=" + SQLConstant.DB_NAME))
            {
                con.Open();
                IDbTransaction transaction = con.BeginTransaction();
                try
                {
                    //open a new command
                    using (var cmd = new SQLiteCommand(sql, con))
                    {
                        foreach( var d in items)
                        {
                            foreach (var pair in d)
                            {
                                cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                            }
                            numberOfRowsAffected += cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                } catch(Exception e)
                {
                    transaction.Rollback();
                    log.Error("error execute batch : " + sql, e);
                    throw e;
                }
            }
            return numberOfRowsAffected;

        }
        public int execute(string sql, Dictionary<string,object>args= null)
        {
            int numberOfRowsAffected;
            using (var con = new SQLiteConnection("Data Source=" + SQLConstant.DB_NAME))
            {
                con.Open();

                //open a new command
                using (var cmd = new SQLiteCommand(sql, con))
                {
                    //set the arguments given in the query
                    if( args != null)
                    {
                        foreach (var pair in args)
                        {
                            cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                        }
                    }
                    //execute the query and get the number of row affected
                    numberOfRowsAffected = cmd.ExecuteNonQuery();
                }

                return numberOfRowsAffected;
            }
        }
        public int createTables()
        {
            int count = 0;
            List<string> tableCreationSqlList = new List<string> { 
                SQLConstant.TABLE_DEVICE_CREATE,
                SQLConstant.TABLE_ACOUNT_CREATE,
                SQLConstant.TABLE_DEVICE_ACCOUNT_CREATION,
                SQLConstant.TABLE_GROUP_CREATE,
                SQLConstant.TABLE_ACTIVITY_LOG_CREATE,
                SQLConstant.TABLE_ACCOUNT_GROUP_CREATE,
                SQLConstant.TABLE_STORE_CREATE
            };
            foreach(var sql in tableCreationSqlList)
            {
                count += execute(sql);
            }

            return count;
        }
        public int dropTables()
        {
            int count = 0;
            List<string> tableRemovalSqlList = new List<string> {
                SQLConstant.TABLE_DEVICE_DROP,
                SQLConstant.TABLE_ACCOUNT_DROP,
               SQLConstant.TABLE_DEVICE_ACCOUNT_DROP,
                SQLConstant.TABLE_GROUP_DROP,
                SQLConstant.TABLE_ACTIVITY_LOG_DROP,
                SQLConstant.TABLE_STORE_DROP
            };
            foreach (var sql in tableRemovalSqlList)
            {
                count += execute(sql);
            }

            return count;
        }
    }
}
