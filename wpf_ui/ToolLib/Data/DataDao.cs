using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
        void ensureDatabaseReady();
        int createTables();
        int dropTables();
    }

    public class DataDao : IDataDao
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static bool _dbInitialized = false;

        public void ensureDatabaseReady()
        {
            if (_dbInitialized) return;

            try
            {
                // 1. Ensure Database exists
                using (var masterCon = new SqlConnection(SQLConstant.MASTER_CONNECTION_STRING))
                {
                    masterCon.Open();
                    string checkDbSql = $@"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{SQLConstant.DATABASE_NAME}') CREATE DATABASE {SQLConstant.DATABASE_NAME};";
                    using (var cmd = new SqlCommand(checkDbSql, masterCon))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // 2. Ensure Tables exist
                createTables();

                _dbInitialized = true;
            }
            catch (Exception e)
            {
                log.Error("Error initializing database", e);
                System.Windows.MessageBox.Show("Database Initialization Error: " + e.Message);
                throw;
            }
        }

        public DataTable query(string query, Dictionary<string, object> args = null)
        {
            if (string.IsNullOrEmpty(query.Trim()))
                return null;

            ensureDatabaseReady();

            using (var con = new SqlConnection(SQLConstant.CONNECTION_STRING))
            {
                con.Open();
                using (var cmd = new SqlCommand(query, con))
                {
                    if (args != null)
                    {
                        foreach (KeyValuePair<string, object> entry in args)
                        {
                            cmd.Parameters.AddWithValue(entry.Key, entry.Value ?? DBNull.Value);
                        }
                    }
                    var da = new SqlDataAdapter(cmd);
                    var dt = new DataTable();
                    da.Fill(dt);
                    da.Dispose();
                    return dt;
                }
            }
        }

        public int executeBatch(string sql, List<Dictionary<string, object>> items)
        {
            ensureDatabaseReady();
            int numberOfRowsAffected = 0;
            using (var con = new SqlConnection(SQLConstant.CONNECTION_STRING))
            {
                con.Open();
                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(sql, con, transaction))
                        {
                            foreach (var d in items)
                            {
                                cmd.Parameters.Clear();
                                foreach (var pair in d)
                                {
                                    cmd.Parameters.AddWithValue(pair.Key, pair.Value ?? DBNull.Value);
                                }
                                numberOfRowsAffected += cmd.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        log.Error("error execute batch : " + sql, e);
                        throw;
                    }
                }
            }
            return numberOfRowsAffected;
        }

        public int execute(string sql, Dictionary<string, object> args = null)
        {
            // Special case for createTables to avoid recursion
            if (!_dbInitialized && !sql.Contains("CREATE TABLE") && !sql.Contains("IF NOT EXISTS"))
            {
                ensureDatabaseReady();
            }

            int numberOfRowsAffected;
            using (var con = new SqlConnection(SQLConstant.CONNECTION_STRING))
            {
                con.Open();
                using (var cmd = new SqlCommand(sql, con))
                {
                    if (args != null)
                    {
                        foreach (var pair in args)
                        {
                            cmd.Parameters.AddWithValue(pair.Key, pair.Value ?? DBNull.Value);
                        }
                    }
                    numberOfRowsAffected = cmd.ExecuteNonQuery();
                }
                return numberOfRowsAffected;
            }
        }

        public int createTables()
        {
            int count = 0;
            List<string> tableCreationSqlList = new List<string> {
                SQLConstant.DeviceSQL.TABLE_DEVICE_CREATE,
                SQLConstant.AccountSQL.TABLE_ACOUNT_CREATE,
                SQLConstant.DeviceAccountSQL.TABLE_DEVICE_ACCOUNT_CREATION,
                SQLConstant.GroupSQL.TABLE_GROUP_CREATE,
                SQLConstant.ActivityLogSQL.TABLE_ACTIVITY_LOG_CREATE,
                SQLConstant.AccountGroupSQL.TABLE_ACCOUNT_GROUP_CREATE,
                SQLConstant.StoreSQL.TABLE_STORE_CREATE,
                SQLConstant.PageSQL.TABLE_PAGES_CREATE,
                SQLConstant.GroupSSQL.TABLE_GROUPSS_CREATE,
                SQLConstant.CacheSQL.TABLE_CACHE_CREATE,
                SQLConstant.GroupDeviceSQL.TABLE_GROUP_DEVICES_CREATE
            };

            // Add columns if they were added later in SQLite (ALTER TABLE scripts)
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_OLD_GROUP_IDS);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_CREATION_DATE);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_TIMELINE_SOURCE);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_REEL_SOURCE_VIDEO);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_PRIMARY_LOCATION);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_MAIL_PASS);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_PAGE);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_PAGE_IDS);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_NOTE);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_TEMP_STORE_ID);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_TEMP_STORE_ORDER);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_TOTAL_SHARE_GROUP);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_TOTAL_SHARE_TIMELINE);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_IS_VERIFY);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_IS_TWOFA);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_STORE_ID);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_IS_SHARE);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_IS_LOGIN);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_IS_ACTIVE);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_IS_JOIN);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_IS_LEAVE);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_USER_AGENT);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_PENDING_JOIN);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_FRIENDS_REQUEST);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_DOB);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_FBLITE_PACKAGE);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_GROUP_IDS);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_COOKIE);
            tableCreationSqlList.Add(SQLConstant.AccountSQL.TABLE_ACCOUNT_ADD_COLUMN_TWOFA);
            tableCreationSqlList.Add(SQLConstant.DeviceAccountSQL.TABLE_DEVICE_ACCOUNT_ADD_COLUMN_ACCOUNT_ID);
            tableCreationSqlList.Add(SQLConstant.CacheSQL.TABLE_CACHE_ADD_COLUMN_TOTAL);
            tableCreationSqlList.Add(SQLConstant.DeviceSQL.TABLE_DEVICE_ADD_COLUMN_SHARE_DELAY);
            tableCreationSqlList.Add(SQLConstant.DeviceSQL.TABLE_DEVICE_ADD_COLUMN_MODEL);
            tableCreationSqlList.Add(SQLConstant.GroupDeviceSQL.TABLE_STORE_ADD_COLUMN_IS_TEMP);
            tableCreationSqlList.Add(SQLConstant.StoreSQL.TABLE_STORE_ADD_COLUMN_IS_TEMP);

            foreach (var sql in tableCreationSqlList)
            {
                count += execute(sql);
            }
            return count;
        }

        public int dropTables()
        {
            int count = 0;
            List<string> tableRemovalSqlList = new List<string> {
                SQLConstant.DeviceSQL.TABLE_DEVICE_DROP,
                SQLConstant.AccountSQL.TABLE_ACCOUNT_DROP,
                SQLConstant.DeviceAccountSQL.TABLE_DEVICE_ACCOUNT_DROP,
                SQLConstant.GroupSQL.TABLE_GROUP_DELETE,
                SQLConstant.ActivityLogSQL.TABLE_ACTIVITY_LOG_DELETE,
                SQLConstant.StoreSQL.TABLE_STORE_DELETE,
                SQLConstant.GroupDeviceSQL.TABLE_GROUP_DEVICES_DROP
            };
            foreach (var sql in tableRemovalSqlList)
            {
                count += execute(sql);
            }
            return count;
        }
    }
}
