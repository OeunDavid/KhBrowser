using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLib.Data
{
    public class SQLConstant
    {
        public const string DB_NAME = "core_db.db";

        public const string TABLE_GROUP_DEVICES_CREATE = "CREATE TABLE IF NOT EXISTS group_devices(id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT DEFAULT NULL, description TEXT DEFAULT NULL, status INTEGER DEFAULT 1);";
        public const string TABLE_GROUP_DEVICES_INSERT = "INSERT INTO group_devices(name,description,status) VALUES(@name,@description,@status) ";
        public const string TABLE_GROUP_DEVICES_UPDATE = "UPDATE group_devices SET name = @name, description = @description, status = @status WHERE id = @id ";
        public const string TABLE_GROUP_DEVICES_DELETE = "DELTE FROM group_devices WHERE id = @id ";
        public const string TABLE_GROUP_DEVICES_SELECT_ALL = "SELECT * FROM group_devices ORDER BY name asc";
        public const string TABLE_GROUP_DEVICES_DROP = "DROP TABLE IF EXISTS group_devices";

        public const string TABLE_DEVICE_CREATE = " CREATE TABLE IF NOT EXISTS devices( id INTEGER PRIMARY KEY AUTOINCREMENT, is_expire INTEGER DEFAULT 0, data_mode INTEGER DEFAULT 0, group_device_id INTEGER DEFAULT 0, internet_id INTEGER DEFAULT 0, device_id TEXT DEFAULT NULL,name TEXT DEFAULT NULL, total_account INTEGER DEFAULT 0, total_available INTEGER DEFAULT 0, total_fblite INTEGER DEFAULT 0, is_busy INTEGER DEFAULT 0, status INTEGER  DEFAULT  0 , description TEXT DEFAULT  NULL, updated_at INTEGER  DEFAULT 0 ,key_press_for_text INTEGER  DEFAULT 0,active_date INTEGER DEFAULT 0); ";
        public const string TABLE_DEVICE_DROP = " DROP TABLE IF EXISTS devices; ";
        public const string TABLE_DEVICE_INSERT = " INSERT INTO devices(device_id,name,data_mode,total_account,total_available,description,key_press_for_text,group_device_id,internet_id) VALUES(@device_id,@name,@data_mode,@total_account,@total_available,@description,@key_press_for_text,@group_device_id,@internet_id) ;";
        public const string TABLE_DEVICE_TOTAL_DEVICE = " SELECT COUNT(id) as total_device FROM devices WHERE status=1 AND internet_id=@internet_id and group_device_id=@group_device_id and is_busy=@is_busy";
        public const string TABLE_DEVICE_ALL_TOTAL_DEVICE = " SELECT COUNT(id) as total_device FROM devices WHERE status=1 AND group_device_id=@group_device_id and is_busy=@is_busy";
        public const string TABLE_DEVICE_UPDATE_CONNECTION = " UPDATE devices SET status =@status , description= @description, key_press_for_text= @key_press_for_text WHERE id = @id  ;";
        public const string TABLE_DEVICE_UPDATE_ACTIVE = " UPDATE devices SET active_date=@active_date, description= @description WHERE id=@id  ;";
        public const string TABLE_DEVICE_UPDATE_EXPIRE = " UPDATE devices SET is_expire=@is_expire WHERE device_id=@device_id;";
        
        //wait for update
        //public const string TABLE_DEVICE_SELECT_ALL_ACTION = " SELECT * FROM devices WHERE is_busy=0 AND total_available<total_account AND status=@status AND group_device_id=@group_device_id AND internet_id IN (0,1,2,3) ORDER BY @order LIMIT 0, @limit";
        public const string TABLE_DEVICE_SELECT_ALL_ACTION = " SELECT * FROM devices WHERE is_expire=0 AND is_busy=0 AND status=@status AND group_device_id=@group_device_id AND internet_id IN ({internets}) ORDER BY {order_by} LIMIT 0, @limit";
        
        public const string TABLE_DEVICE_UPDATE = " UPDATE devices SET status=@status , description= @description, key_press_for_text= @key_press_for_text WHERE id = @id  ;";
        public const string TABLE_DEVICE_UPDATE_FBLITE = " UPDATE devices SET total_fblite=@total_fblite WHERE device_id=@device_id;";
        public const string TABLE_DEVICE_UPDATE_BUSY = " UPDATE devices SET is_busy=@is_busy WHERE id=@id;";
        public const string TABLE_DEVICE_UPDATE_AVAILABLE = " UPDATE devices SET total_available=@total_available WHERE id=@id;";
        public const string TABLE_DEVICE_UPDATE_DATA_MODE = " UPDATE devices SET data_mode=@data_mode WHERE device_id=@device_id;";
        public const string TABLE_DEVICE_DELETE = " DELETE FROM devices WHERE id= @id ;";
        public const string TABLE_DEVICE_TRUNCATE = "TRUNCATE TABLE devices; ";
        public const string TABLE_DEVICE_SELECT_ALL = "SELECT * FROM devices ORDER BY name asc";
        public const string TABLE_DEVICE_SELECT_ALL_GROUP = "SELECT devices.*,COUNT(CASE WHEN device_accounts.status=1 THEN 1 ELSE NULL END) AS account_live FROM devices LEFT JOIN device_accounts ON devices.device_id=device_accounts.device_id AND device_accounts.status=1 WHERE devices.group_device_id=@group_device_id GROUP BY devices.device_id ORDER BY devices.name asc";
        public const string TABLE_DEVICE_SELECT_ONE = "SELECT * FROM devices WHERE id=@id";
        public const string TABLE_DEVICE_SELECT_ONE_BY_DEVICE_ID = "SELECT * FROM devices WHERE device_id=@device_id";
        public const string TABLE_DEVICE_UPDATE_NAME = " UPDATE devices SET name=@name WHERE device_id=@device_id;";
        public const string TABLE_DEVICE_UPDATE_STATUS_TO_DISCONNECT = " UPDATE devices SET status=0;";
        public const string TABLE_DEVICE_UPDATE_STATUS = " UPDATE devices SET status=@status WHERE device_id=@device_id;";
        public const string TABLE_DEVICE_UPDATE_INTERNET = " UPDATE devices SET internet_id=@internet_id WHERE device_id=@device_id;";
        public const string TABLE_DEVICE_UPDATE_ACCOUNT = " UPDATE devices SET total_account=@total_account,total_available=@total_available WHERE device_id=@device_id;";
        public const string TABLE_DEVICE_UPDATE_REMOVE_FBLITE = " UPDATE devices SET status=0,total_fblite=@total_fblite,total_available=@total_available WHERE device_id=@device_id;";

        //update group device
        public const string TABLE_DEVICE_UPDATE_GROUP = " UPDATE devices SET group_device_id=@group_device_id WHERE device_id=@device_id;";
        public const string TABLE_ACCOUNT_UPDATE_GROUP_DEVICE = " UPDATE accounts SET group_device_id=@group_device_id WHERE device_id=@device_id ";
        public const string TABLE_ACCOUNT_UPDATE_GROUP_DEVICE_BY_UID = " UPDATE accounts SET group_device_id=@group_device_id WHERE uid=@uid AND (device_id IS NULL OR device_id=''); ";


        public const string TABLE_DEVICE_ACCOUNT_CREATION = " CREATE TABLE IF NOT EXISTS device_accounts( id INTEGER PRIMARY KEY AUTOINCREMENT, device_id INTEGER DEFAULT 0, uid TEXT DEFAULT NULL, package_id TEXT DEFAULT NULL, apk_id INTEGER DEFAULT 0, status INTEGER DEFAULT 0, updated_at INTEGER DEFAULT 0, active_date INTEGER DEFAULT 0, share_date INTEGER DEFAULT 0, join_date INTEGER DEFAULT 0, leave_date INTEGER DEFAULT 0, dark_mode INTEGER DEFAULT 0, description TEXT DEFAULT NULL);";
        public const string TABLE_DEVICE_ACCOUNT_BY_DEVICE = " SELECT device_accounts.*,accounts.status AS account_status FROM device_accounts LEFT JOIN accounts ON accounts.uid=device_accounts.uid WHERE device_accounts.device_id= @device_id";
        public const string TABLE_DEVICE_ACCOUNT_NO_FBLITE = " SELECT * FROM device_accounts WHERE device_id= @device_id and (package_id='' or package_id is null) ORDER BY apk_id desc LIMIT @offset, @limit";
        public const string TABLE_DEVICE_ACCOUNT_SELECT_BY_DEVICE_NO_PACKAGE = " SELECT * FROM device_accounts WHERE device_id= @device_id and (package_id is null or package_id=''); ";
        public const string TABLE_DEVICE_ACCOUNT_SELECT_BY_DEVICE_NO_UID = " SELECT * FROM device_accounts WHERE device_id= @device_id and (uid is null or uid='') and package_id is not null; ";
        public const string TABLE_DEVICE_ACCOUNT_SELECT_ONE= " SELECT * FROM device_accounts WHERE id=@id ORDER BY id DESC;";
        public const string TABLE_DEVICE_ACCOUNT_SELECT_ONE_LAST_BY_DEVICE= " SELECT * FROM device_accounts WHERE device_id=@device_id ORDER BY id DESC LIMIT 0,1;";
        public const string TABLE_DEVICE_ACCOUNT_SELECT_BY_DEVICE_PENDING_UID_LOGIN = " SELECT device_accounts.*,accounts.password,accounts.twofa,accounts.token,accounts.proxy FROM device_accounts inner join accounts on accounts.uid=device_accounts.uid WHERE device_accounts.device_id=@device_id and (device_accounts.uid is not null or device_accounts.uid<>'') and (device_accounts.package_id is not null or device_accounts.package_id<>'') and device_accounts.status=0 and accounts.status=1; ";
        public const string TABLE_DEVICE_ACCOUNT_INSERT_APK = " INSERT INTO device_accounts(device_id,apk_id) VALUES(@device_id,@apk_id);";
        public const string TABLE_DEVICE_ACCOUNT_UPDATE_PACKAGE = " UPDATE device_accounts SET package_id=@package_id, description=@description WHERE id=@id;";
        public const string TABLE_DEVICE_ACCOUNT_UPDATE_UID = " UPDATE device_accounts SET uid=@uid WHERE (uid is null or uid='') and id=(select id from device_accounts where device_id=@device_id and (uid is null or uid='') limit 1) ;";
        public const string TABLE_DEVICE_ACCOUNT_UPDATE_STATUS = " UPDATE device_accounts SET status=@status,description=@description,updated_at=@updated_at WHERE id=@id;";
        public const string TABLE_DEVICE_ACCOUNT_UPDATE_DARK_MODE = " UPDATE device_accounts SET dark_mode=@dark_mode WHERE id=@id;";
        public const string TABLE_DEVICE_ACCOUNT_UPDATE_ACTIVE = " UPDATE device_accounts SET status=@status,description=@description {customFieldUpdate},updated_at=@updated_at WHERE id=@id;";
        public const string TABLE_DEVICE_ACCOUNT_UPDATE = " UPDATE device_accounts SET status=@status,description=@description,updated_at=@updated_at,uid=@uid,package_id=@package_id WHERE id=@id;";
        public const string TABLE_DEVICE_ACCOUNT_DELETE = " DELETE FROM device_accounts WHERE id=@id;";

        // wait for update
        public const string TABLE_DEVICE_ACCOUNT_SELECT_ALL = " SELECT device_accounts.*, accounts.pending_join, accounts.is_leave, accounts.share_type_id FROM device_accounts INNER JOIN accounts ON device_accounts.uid=accounts.uid WHERE device_accounts.status=@status AND device_accounts.device_id=@device_id {where} ORDER BY {order_by} LIMIT @offset, @limit";


        public const string TABLE_DEVICE_ACCOUNT_DROP = " DROP TABLE IF EXISTS device_accounts; ";

        public const string TABLE_DEVICE_ACCOUNT_INSERT_BATCH = " INSERT INTO device_accounts(device_id, account_id ) VALUES( @device_id , @account_id ) ";


        public const string TABLE_ACOUNT_CREATE = " CREATE TABLE IF NOT EXISTS accounts (id INTEGER, uid TEXT DEFAULT NULL, group_device_id INTEGER DEFAULT 0, name TEXT DEFAULT NULL, password TEXT DEFAULT NULL, email TEXT DEFAULT NULL, description TEXT DEFAULT NULL, status INTEGER DEFAULT 1, updated_at INTEGER DEFAULT 0, device_id TEXT DEFAULT NULL, total_group INTEGER DEFAULT 0, total_friend INTEGER DEFAULT 0, gender TEXT DEFAULT NULL, birthday INTEGER DEFAULT 0, token	TEXT DEFAULT NULL, twofa TEXT DEFAULT NULL, is_join INTEGER DEFAULT 0, is_leave INTEGER DEFAULT 0, fblite_package TEXT DEFAULT NULL, proxy TEXT DEFAULT NULL,pending_join TEXT DEFAULT NULL, share_type_id INTEGER DEFAULT 0,PRIMARY KEY(id AUTOINCREMENT));";
        public const string TABLE_ACCOUNT_INSERT = " INSERT INTO accounts(group_device_id,uid,password,twofa,token,description,share_type_id) VALUES(@group_device_id,@uid, @password, @twofa, @token,@description,@share_type_id)";
        public const string TABLE_ACCOUNT_SELECT_ALL_BY_STORE = " SELECT * FROM accounts where group_device_id=@group_device_id ORDER BY id asc";
        public const string TABLE_ACCOUNT_SELECT_BY_UID = " SELECT * FROM accounts WHERE uid IN (@uid)";
        public const string TABLE_ACCOUNT_UPDATE_DEVICE = "UPDATE accounts SET device_id=@device_id WHERE id=@id and (device_id is null or device_id='');";
        public const string TABLE_ACCOUNT_RESET_DEVICE_BY_UID = "UPDATE accounts SET device_id=NULL WHERE uid=@uid;";
        public const string TABLE_ACCOUNT_UPDATE_STATUS = "UPDATE accounts SET status=@status,description=@description,updated_at=@updated_at WHERE uid=@uid;";
        public const string TABLE_ACCOUNT_ADD_PROXY = "UPDATE accounts SET proxy=@proxy WHERE uid=@uid;";
        public const string TABLE_ACCOUNT_CLEAR_PROXY = "UPDATE accounts SET proxy='' WHERE group_device_id=@group_device_id;";
        public const string TABLE_ACCOUNT_CLEAR_PROXY_BY_UID = "UPDATE accounts SET proxy=NULL WHERE uid=@uid;";
        public const string TABLE_ACCOUNT_PENDING_JOIN_BY_UID = "UPDATE accounts SET pending_join=@pending_join WHERE uid=@uid;";
        public const string TABLE_ACCOUNT_UPDATE_LEAVE_GROUP_BY_UID = "UPDATE accounts SET is_leave=@is_leave WHERE uid=@uid;";
        public const string TABLE_ACCOUNT_UPDATE_SHARE_TYPE_BY_UID = "UPDATE accounts SET share_type_id=@share_type_id WHERE uid=@uid;";
        public const string TABLE_ACCOUNT_SELECT_ALL_NO_DEVICE = " SELECT accounts.id, accounts.uid FROM accounts WHERE accounts.status=1 and accounts.share_type_id IN ({shareTypeIds}) and group_device_id=@group_device_id and (accounts.device_id is null or accounts.device_id='') LIMIT 0, @limit";// (SELECT COUNT(id) FROM device_accounts WHERE device_accounts.device_id=@device_id and (device_accounts.uid is null or device_accounts.uid=''))";
        public const string TABLE_ACCOUNT_DELETE_BY_UID = " DELETE FROM accounts WHERE uid=@uid AND (device_id IS NULL OR device_id=''); ";



        public const string TABLE_ACCOUNT_SELECT_ALL_LIMIT = " SELECT * FROM accounts ORDER BY id asc LIMIT  @offset, @limit";
        public const string TABLE_ACCOUNT_UPDATE = "UPDATE accounts SET password= @password , description = @description , status= @status , updated_at = @updated_at, device_id = @device_id , total_group =@total_group WHERE id = @id ";
        public const string TABLE_ACCOUNT_DELETE = "DELETE FROM accounts WHERE id = @id ";
        public const string TABLE_ACCOUNT_UPDATE_SECURITY = "UPDATE accounts SET twofa=@twofa,password=@password WHERE uid=@uid ";
        public const string TABLE_ACCOUNT_SELEC_ALL = " SELECT * FROM accounts";
        public const string TABLE_ACCOUNT_DROP = " DROP TABLE IF EXISTS accounts;";
        public const string TABLE_ACCOUNT_SELECT_BY_ID = " SELECT * FROM accounts WHERE id IN ( @id)";
        //public const string TABLE_ACCOUNT_SELECT_ALL_NO_DEVICE = "SELECT * FROM accounts  as a LEFT JOIN devices d on a.device_id = d.id " + "   WHERE a.device_id is null OR d.status != 'connected' LIMIT @offset , @limit";
        public const string TABLE_ACCOUNT_SELECT_BY_DEVICE = "SELECT * FROM accounts WHERE device_id = @device_id";
        public const string TABLE_ACCOUNT_SELECT_ONE = " SELECT * FROM accounts WHERE id = @id ";
        public const string TABLE_ACCOUNT_SELECT_ONE_BY_UID = " SELECT * FROM accounts WHERE uid= @uid ";





        public const string TABLE_GROUP_CREATE = "CREATE TABLE IF NOT EXISTS groups( id TEXT PRIMARY KEY , name TEXT DEFAULT  NULL,created_by TEXT DEFAULT  NULL, state INTEGER default 1  ,updated_at INTEGER  DEFAULT 0  ); ";
        public const string TABLE_GROUP_INSERT = "INSERT INTO groups( id , name ,created_by , state ,updated_at  ) VALUES( @id , @name, @created_by , @state, @updated_at ) ;";
        public const string TABLE_GROUP_UPDATE = "UPDATE groups SET name = @name , created_by= @created_by , state = @state , updated_at= @updated_at WHERE id = @id";
        public const string TABLE_GROUP_DELETE = "DELETE FROM groups WHERE id = @id ";
        public const string TABLE_GROUP_DROP = " DROP TABLE IF EXISTS groups ";
        public const string TABLE_GROUP_SELECT_ALL = " SELECT * FROM groups ORDER by updated_at DESC";
        public const string TABLE_GROUP_SELECT_BY_ID = " SELECT * FROM groups WHERE id IN ( @id )";
        public const string TABLE_GROUP_SELECT_BY_ACCOUNT_ID = " SELECT g.* FROM groups g INNER JOIN account_groups c ON g.id = c.group_id WHERE c.account_id = @id";


        // activity log
        public const string TABLE_ACTIVITY_LOG_CREATE = " CREATE TABLE IF NOT EXISTS activity_logs( id  INTEGER PRIMARY KEY AUTOINCREMENT,device_id TEXT DEFAULT  NULL, uid  TEXT DEFAULT  NULL, action_name TEXT  DEFAULT NULL, action_date INTEGER DEFAULT 0, description TEXT  DEFAULT NULL, share_timeline INTEGER DEFAULT 0, share_groups INTEGER DEFAULT 0, url TEXT  DEFAULT NULL );";
        public const string TABLE_ACTIVITY_LOG_INSERT = " INSERT INTO activity_logs(device_id, uid, action_name, action_date, description, url, share_timeline, share_groups ) VALUES (@device_id , @uid , @action_name, @action_date, @description, @url, @share_timeline, @share_groups ) ";
        public const string TABLE_ACTIVITY_LOG_SELECT_ALL = " SELECT * FROM activity_logs WHERE action_date>=@from_date AND action_date<=@to_date ORDER BY action_date DESC ";
        public const string TABLE_ACTIVITY_LOG_UPDATE = " UPDATE activity_logs set description=@description, url=@url, share_timeline=@share_timeline, share_groups=@share_groups WHERE id=(SELECT id FROM activity_logs WHERE device_id=@device_id ORDER BY id desc LIMIT 0,1); ";


        public const string TABLE_ACTIVITY_LOG_DROP = " DROP TABLE IF  EXISTS activity_logs ";

        public const string TABLE_ACCOUNT_GROUP_CREATE = "CREATE TABLE IF NOT EXISTS account_groups( account_id TEXT , group_id text , state INTEGER default 0, PRIMARY KEY (account_id , group_id )) ";
        public const string TABLE_ACCOUNT_GROUP_DELETE_BY_ACCOUNT = "DELETE FROM account_groups WHERE account_id IN ( @id )";
        public const string TABLE_ACCOUNT_GROUP_INSERT = "INSERT INTO account_groups(account_id , group_id ) VALUES( @account_id , @group_id )";
        public const string TABLE_ACCOUNT_GROUP_SELECT_BY_DEVICE_ID = "SELECT * FROM account_groups g INNER JOIN accounts a WHERE a.id = g.account_id AND a.device_id =@device_id ";

        public const string TABLE_STORE_CREATE = "CREATE TABLE IF NOT EXISTS stores(id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT DEFAULT NULL, note TEXT DEFAULT NULL, created_by TEXT DEFAULT NULL, state INTEGER DEFAULT 1, updated_at INTEGER  DEFAULT 0 );";
        public const string TABLE_STORE_INSERT = "INSERT INTO stores( name, note, created_by, state, updated_at )  VALUES( @name , @note, @created_by, @state, @updated_at ) ";
        public const string TABLE_STORE_UPDATE = "UPDATE stores SET name = @name, note = @note, updated_at = @updated_at WHERE id = @id ";
        public const string TABLE_STORE_DELETE = "DELTE FROM stores WHERE id = @id ";
        public const string TABLE_STORE_SELECT_ALL = "SELECT * FROM stores ORDER BY name asc";
        public const string TABLE_STORE_DROP = "DROP TABLE IF EXISTS stores";

        public const string TABLE_ACCOUNT_TOTAL_LIVE = " SELECT COUNT(id) as total_account_live FROM accounts WHERE status=1;";
        public const string TABLE_ACCOUNT_TOTAL_DIE = " SELECT COUNT(id) AS total_account_die FROM accounts WHERE status!=1;";
        public const string TABLE_DEVICE_TOTAL_ALL = " SELECT COUNT(id) AS total_device FROM devices";
        public const string TABLE_ACTIVITY_LOG_TOTAL_SHARE_TIMELINE = " SELECT SUM(share_timeline) as total_share_timeline FROM activity_logs";
        public const string TABLE_ACTIVITY_LOG_TOTAL_SHARE_GROUP = " SELECT SUM(share_groups) as total_share_group FROM activity_logs";
    }
}
