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


        public const string TABLE_ACCOUNT_ADD_COLUMN_OLD_GROUP_IDS = "ALTER TABLE accounts ADD COLUMN old_group_ids TEXT DEFAULT NULL;";
        
        public const string TABLE_ACCOUNT_ADD_COLUMN_CREATION_DATE = "ALTER TABLE accounts ADD COLUMN creation_date TEXT DEFAULT NULL;";
        
        public const string TABLE_ACCOUNT_ADD_COLUMN_TIMELINE_SOURCE = "ALTER TABLE accounts ADD COLUMN timeline_source TEXT DEFAULT NULL;";
        public const string TABLE_ACCOUNT_ADD_COLUMN_REEL_SOURCE_VIDEO = "ALTER TABLE accounts ADD COLUMN reel_source_video TEXT DEFAULT NULL;";
        public const string TABLE_ACCOUNT_ADD_COLUMN_PRIMARY_LOCATION = "ALTER TABLE accounts ADD COLUMN primaryLocation TEXT DEFAULT NULL;";
        public const string TABLE_ACCOUNT_ADD_COLUMN_MAIL_PASS = "ALTER TABLE accounts ADD COLUMN mailPass TEXT DEFAULT NULL;";

        public const string TABLE_PAGES_SELECT_BY_UID = "SELECT * FROM pages WHERE uid=@uid";
        public const string TABLE_PAGES_DELETE_BY_UID = "DELETE FROM pages WHERE uid= @uid";
        public const string TABLE_PAGES_INSERT = "INSERT INTO pages(uid,name,page_id,status,access_token) VALUES(@uid,@name,@page_id,@status,@access_token)";
        public const string TABLE_PAGES_CREATE = "CREATE TABLE IF NOT EXISTS pages(id INTEGER PRIMARY KEY AUTOINCREMENT, uid TEXT DEFAULT NULL, access_token TEXT DEFAULT NULL , name TEXT DEFAULT NULL, page_id TEXT DEFAULT NULL, status INTEGER default 1); ";

        public const string TABLE_ACCOUNT_ADD_COLUMN_PAGE = "ALTER TABLE accounts ADD COLUMN total_page INTEGER DEFAULT 0;";
        public const string TABLE_ACCOUNT_ADD_COLUMN_PAGE_IDS = "ALTER TABLE accounts ADD COLUMN page_ids TEXT DEFAULT NULL;";

        public const string TABLE_GROUPSS_SELECT_BY_UID = "SELECT * FROM groupss WHERE uid=@uid";
        public const string TABLE_GROUPSS_INSERT = "INSERT INTO groupss(uid,name,page_id,status,group_id,member,pending,check_pending) VALUES(@uid,@name,@page_id,@status,@group_id,@member,@pending,@check_pending)";
        public const string TABLE_GROUPSS_UPDATE_PENDING = "UPDATE groupss SET pending=@pending, check_pending=1 WHERE group_id=@group_id";
        public const string TABLE_GROUPSS_SELECT_SINGLE_RECORD = "SELECT * FROM groupss WHERE uid=@uid and group_id=@group_id";
        public const string TABLE_GROUPSS_DELETE = "DELETE FROM groupss WHERE uid= @uid and group_id=@group_id";
        public const string TABLE_GROUPSS_DELETE_BY_UID = "DELETE FROM groupss WHERE uid= @uid";
        public const string TABLE_GROUPSS_CREATE = "CREATE TABLE IF NOT EXISTS groupss(id INTEGER PRIMARY KEY AUTOINCREMENT, uid TEXT DEFAULT NULL, group_id TEXT DEFAULT NULL , name TEXT DEFAULT NULL, member INTEGER DEFAULT 0, page_id TEXT DEFAULT NULL,is_join INTEGER DEFAULT 0, check_pending INTEGER DEFAULT 0, pending INTEGER DEFAULT 0, status INTEGER default 1); ";


        public const string TABLE_ACCOUNT_ADD_COLUMN_NOTE = "ALTER TABLE accounts ADD COLUMN note TEXT DEFAULT NULL;";

        public const string TABLE_STORE_ADD_COLUMN_IS_TEMP = "ALTER TABLE group_devices ADD COLUMN is_temp INTEGER DEFAULT 0;";
        public const string TABLE_ACCOUNT_ADD_COLUMN_TEMP_STORE_ID = "ALTER TABLE accounts ADD COLUMN temp_store_id INTEGER DEFAULT 0;";
        public const string TABLE_ACCOUNT_ADD_COLUMN_TEMP_STORE_ORDER = "ALTER TABLE accounts ADD COLUMN temp_order INTEGER DEFAULT 0;";

        public const string TABLE_ACCOUNT_ADD_COLUMN_TOTAL_SHARE_GROUP = "ALTER TABLE accounts ADD COLUMN total_share_group INTEGER DEFAULT 0;";
        public const string TABLE_ACCOUNT_ADD_COLUMN_TOTAL_SHARE_TIMELINE = "ALTER TABLE accounts ADD COLUMN total_share_timeline INTEGER DEFAULT 0;";
        public const string TABLE_ACCOUNT_ADD_COLUMN_IS_VERIFY = "ALTER TABLE accounts ADD COLUMN is_verify INTEGER DEFAULT 0;";
        public const string TABLE_ACCOUNT_ADD_COLUMN_IS_TWOFA = "ALTER TABLE accounts ADD COLUMN is_twofa INTEGER DEFAULT 0;";
        public const string TABLE_CACHE_ADD_COLUMN_TOTAL = "ALTER TABLE caches ADD COLUMN total INTEGER DEFAULT 0;";



        public const string TABLE_ACCOUNT_UPDATE_PRIMARY_LOCATION = " UPDATE accounts SET primaryLocation=@primaryLocation WHERE uid=@uid";
        public const string TABLE_ACCOUNT_UPDATE_TOKEN = " UPDATE accounts SET token=@token WHERE uid=@uid";
        public const string TABLE_ACCOUNT_UPDATE_PAGE = " UPDATE accounts SET total_page=@total_page,page_ids=@page_ids WHERE uid=@uid";

        public const string TABLE_ACCOUNT_UPDATE_SHARE_TO_GROUP = " UPDATE accounts SET total_share_group=total_share_group+@total WHERE uid=@uid";  
        public const string TABLE_ACCOUNT_UPDATE_SHARE_TO_TIMELINE = " UPDATE accounts SET total_share_timeline=total_share_timeline+@total WHERE uid=@uid";  
        public const string TABLE_ACCOUNT_UPDATE_UID = " UPDATE accounts SET uid=@new_uid WHERE uid=@uid";  
        public const string TABLE_ACCOUNT_UPDATE_EMAIL = " UPDATE accounts SET email=@email WHERE uid=@uid";
        public const string TABLE_ACCOUNT_UPDATE_MAIL_PASS = " UPDATE accounts SET mailPass=@mailPass WHERE uid=@uid";
        public const string TABLE_ACCOUNT_UPDATE_INFO = " UPDATE accounts SET email=@email,gender=@gender,name=@name,dob=@dob WHERE uid=@uid";
        public const string TABLE_ACCOUNT_UPDATE_COOKIE = " UPDATE accounts SET cookie=@cookie WHERE uid=@uid";
        public const string TABLE_ACCOUNT_UPDATE_NAME = " UPDATE accounts SET name=@name WHERE uid=@uid";  
        public const string TABLE_ACCOUNT_UPDATE_GENDER = " UPDATE accounts SET gender=@gender WHERE uid=@uid";
        public const string TABLE_ACCOUNT_UPDATE_BIRTHDAY = " UPDATe accounts SET dob=@dob WHERE uid=@uid";
        public const string TABLE_ACCOUNT_UPDATE_FRIENDS = " UPDATe accounts SET total_friend=@total_friend WHERE uid=@uid";
        public const string TABLE_ACCOUNT_UPDATE_FRIENDS_REQUEST = " UPDATe accounts SET friends_request=@friends_request WHERE uid=@uid";
        public const string TABLE_ACCOUNT_UPDATE_TOTAL_GROUP = " UPDATe accounts SET total_group=@total_group WHERE uid=@uid";
        public const string TABLE_ACCOUNT_UPDATE_GROUP_ID = " UPDATE accounts SET group_ids=@group_ids,old_group_ids=@old_group_ids WHERE uid IN ({uid})";

        public const string TABLE_ACCOUNT_INSERT = " INSERT INTO accounts(store_id,uid,password,twofa,cookie,is_leave) VALUES(@store_id,@uid, @password, @twofa, @cookie, 0)";  
        public const string TABLE_ACCOUNT_SELECT_ALL_BY_STORE = " SELECT accounts.*,group_devices.name as temp_name FROM accounts left join group_devices on group_devices.id=accounts.temp_store_id where store_id=@store_id {where} ORDER BY id asc";
        public const string TABLE_ACCOUNT_SELECT_ALL_BY_TEMP_STORE = " SELECT accounts.*,group_devices.name as temp_name FROM accounts left join group_devices on group_devices.id=accounts.store_id where temp_store_id=@temp_store_id {where} ORDER BY temp_order asc";
        public const string TABLE_ACCOUNT_SELECT_ALL = " SELECT * FROM accounts where 1=1 {where} ORDER BY id asc Limit @offset,@limit";

        public const string TABLE_ACCOUNT_SELECT_BY_UID = " SELECT * FROM accounts WHERE uid IN (@uid)";
        public const string TABLE_ACCOUNT_UPDATE_DEVICE = "UPDATE accounts SET device_id=@device_id WHERE id=@id and (device_id is null or device_id='');";
        public const string TABLE_ACCOUNT_RESET_DEVICE_BY_UID = "UPDATE accounts SET device_id=NULL WHERE uid=@uid;";
        public const string TABLE_ACCOUNT_UPDATE_STATUS = "UPDATE accounts SET status=@status,description=@description,updated_at=@updated_at WHERE uid IN ({uid});";
        public const string TABLE_ACCOUNT_UPDATE_DESCRIPTION = "UPDATE accounts SET description=@description,updated_at=@updated_at WHERE uid IN ({uid});";
        public const string TABLE_ACCOUNT_ADD_NOTE = "UPDATE accounts SET note=@note WHERE uid=@uid;";
        public const string TABLE_ACCOUNT_ADD_REEL_SOURCE_VIDEO = "UPDATE accounts SET reel_source_video=@reel_source_video WHERE uid=@uid;";
        public const string TABLE_ACCOUNT_ADD_TIMELINE_SOURCE = "UPDATE accounts SET timeline_source=@timeline_source WHERE uid=@uid;";
        public const string TABLE_ACCOUNT_ADD_PROXY = "UPDATE accounts SET proxy=@proxy WHERE uid=@uid;";
        public const string TABLE_ACCOUNT_ADD_USER_AGENT = "UPDATE accounts SET user_agent=@user_agent WHERE uid=@uid;";
        public const string TABLE_ACCOUNT_CLEAR_PROXY = "UPDATE accounts SET proxy='' WHERE group_device_id=@group_device_id;";
        public const string TABLE_ACCOUNT_CLEAR_NOTE_BY_UID = "UPDATE accounts SET note=NULL WHERE uid IN ({uid});";
        public const string TABLE_ACCOUNT_CLEAR_GROUP_ID_BY_UID = "UPDATE accounts SET group_ids=NULL,total_group=0 WHERE uid IN ({uid});";
        public const string TABLE_ACCOUNT_CLEAR_REEL_SOURCE_VIDEO_BY_UID = "UPDATE accounts SET reel_source_video=NULL WHERE uid IN ({uid});";
        public const string TABLE_ACCOUNT_CLEAR_TIMELINE_SOURCE_BY_UID = "UPDATE accounts SET timeline_source=NULL WHERE uid IN ({uid});";
        public const string TABLE_ACCOUNT_CLEAR_PROXY_BY_UID = "UPDATE accounts SET proxy=NULL WHERE uid IN ({uid});";
        public const string TABLE_ACCOUNT_CLEAR_USER_AGENT_BY_UID = "UPDATE accounts SET user_agent=NULL WHERE uid IN ({uid});";
        public const string TABLE_ACCOUNT_PENDING_JOIN_BY_UID = "UPDATE accounts SET pending_join=@pending_join WHERE uid IN ({uid});";
        public const string TABLE_ACCOUNT_UPDATE_LEAVE_GROUP_BY_UID = "UPDATE accounts SET is_leave=@is_leave WHERE uid IN ({uid});";
        public const string TABLE_ACCOUNT_UPDATE_VERIFY_BY_UID = "UPDATE accounts SET is_verify=@is_verify WHERE uid IN ({uid});";
        public const string TABLE_ACCOUNT_UPDATE_TWOFA_BY_UID = "UPDATE accounts SET twofa=@twofa WHERE uid=@uid;";
        public const string TABLE_ACCOUNT_UPDATE_IS_TWOFA_BY_UID = "UPDATE accounts SET is_twofa=@is_twofa WHERE uid IN ({uid});";
        public const string TABLE_ACCOUNT_UPDATE_LOGIN_BY_UID = "UPDATE accounts SET is_login=@is_login WHERE uid IN ({uid});";
        public const string TABLE_ACCOUNT_UPDATE_SHARE_BY_UID = "UPDATE accounts SET is_share=@is_share WHERE uid IN ({uid});";
        public const string TABLE_ACCOUNT_UPDATE_ACTIVE_BY_UID = "UPDATE accounts SET is_active=@is_active WHERE uid IN ({uid});";
        public const string TABLE_ACCOUNT_UPDATE_PASSWORD_BY_UID = "UPDATE accounts SET password=@password WHERE uid IN ({uid});";
        public const string TABLE_ACCOUNT_UPDATE_TRANSFER_BY_UID = "UPDATE accounts SET store_id=@store_id WHERE uid IN ({uid});";
        public const string TABLE_ACCOUNT_UPDATE_TEMP_STORE_BY_UID = "UPDATE accounts SET temp_store_id=@temp_store_id,temp_order=@temp_order WHERE uid IN ({uid});";
        public const string TABLE_ACCOUNT_UPDATE_SHARE_TYPE_BY_UID = "UPDATE accounts SET share_type_id=@share_type_id WHERE uid=@uid;";
        public const string TABLE_ACCOUNT_SELECT_ALL_NO_DEVICE = " SELECT accounts.id, accounts.uid FROM accounts WHERE accounts.status=1 and accounts.share_type_id IN ({shareTypeIds}) and group_device_id=@group_device_id and (accounts.device_id is null or accounts.device_id='') LIMIT 0, @limit";// (SELECT COUNT(id) FROM device_accounts WHERE device_accounts.device_id=@device_id and (device_accounts.uid is null or device_accounts.uid='') LIMIT 0, @limit)";
        public const string TABLE_ACCOUNT_DELETE_BY_UID = " DELETE FROM accounts WHERE uid IN ({uid}); ";




        public const string TABLE_ACCOUNT_UPDATE = "UPDATE accounts SET password= @password , description = @description , status= @status , updated_at = @updated_at, device_id = @device_id , total_group =@total_group WHERE id = @id ";
        public const string TABLE_ACCOUNT_DELETE = "DELETE FROM accounts WHERE id = @id ";
        public const string TABLE_ACCOUNT_UPDATE_SECURITY = "UPDATE accounts SET mailPass=@mailPass,store_id=@store_id, twofa=@twofa,password=@password WHERE uid=@uid ";
        public const string TABLE_ACCOUNT_UPDATE_STORE = "UPDATE accounts SET store_id=@store_id WHERE uid=@uid ";
        public const string TABLE_ACCOUNT_SELEC_ALL = " SELECT * FROM accounts";
        public const string TABLE_ACCOUNT_DROP = " DROP TABLE IF EXISTS accounts;";
        public const string TABLE_ACCOUNT_SELECT_BY_ID = " SELECT * FROM accounts WHERE id IN ( @id)";
        public const string TABLE_ACCOUNT_SELECT_BY_DEVICE = "SELECT * FROM accounts WHERE device_id = @device_id";
        public const string TABLE_ACCOUNT_SELECT_ONE = " SELECT * FROM accounts WHERE id = @id ";
        public const string TABLE_ACCOUNT_SELECT_ONE_BY_UID = " SELECT * FROM accounts WHERE uid= @uid ";

        public const string TABLE_DEVICE_ADD_COLUMN_SHARE_DELAY = "ALTER TABLE devices ADD COLUMN share_delay INTEGER DEFAULT 0;";
        public const string TABLE_DEVICE_ADD_COLUMN_MODEL = "ALTER TABLE devices ADD COLUMN model text DEFAULT NULL;";

        public const string TABLE_GROUP_DEVICES_CREATE = "CREATE TABLE IF NOT EXISTS group_devices(id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT DEFAULT NULL, description TEXT DEFAULT NULL, status INTEGER DEFAULT 1);";
        public const string TABLE_GROUP_DEVICES_INSERT = "INSERT INTO group_devices(name,description,status,is_temp) VALUES(@name,@description,@status,@is_temp) ";
        public const string TABLE_GROUP_DEVICES_UPDATE = "UPDATE group_devices SET name = @name, description = @description, status = @status WHERE id = @id ";
        public const string TABLE_GROUP_DEVICES_ONE_RECORD = "SELECT * FROM group_devices WHERE id = @id ";
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
        public const string TABLE_DEVICE_SELECT_ALL_GROUP = "SELECT devices.*,COUNT(CASE WHEN device_accounts.status=1 THEN 1 ELSE NULL END) AS account_live FROM devices LEFT JOIN device_accounts ON (devices.device_id=device_accounts.device_id AND device_accounts.status=1 AND device_accounts.uid IS NOT NULL AND device_accounts.uid!='' AND device_accounts.package_id IS NOT NULL AND device_accounts.package_id!='') WHERE devices.group_device_id=@group_device_id GROUP BY devices.device_id ORDER BY devices.name asc";
        public const string TABLE_DEVICE_SELECT_ONE = "SELECT * FROM devices WHERE id=@id";
        public const string TABLE_DEVICE_SELECT_ONE_BY_DEVICE_ID = "SELECT * FROM devices WHERE device_id=@device_id";
        public const string TABLE_DEVICE_UPDATE_NAME = " UPDATE devices SET name=@name WHERE device_id=@device_id;";
        public const string TABLE_DEVICE_UPDATE_MODEL = " UPDATE devices SET model=@model WHERE device_id=@device_id;";
        public const string TABLE_DEVICE_UPDATE_SHARE_DELAY = " UPDATE devices SET share_delay=@share_delay WHERE device_id=@device_id;";
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
        public const string TABLE_DEVICE_ACCOUNT_TOTAL_NO_ACCOUNT = " SELECT COUNT(id) AS total FROM device_accounts WHERE device_id=@device_id AND (uid='' OR uid is null)";

        // wait for update
        public const string TABLE_DEVICE_ACCOUNT_SELECT_ALL = " SELECT device_accounts.*, accounts.pending_join, accounts.is_leave, accounts.share_type_id FROM device_accounts INNER JOIN accounts ON device_accounts.uid=accounts.uid AND device_accounts.device_id=accounts.device_id WHERE device_accounts.status=@status AND accounts.status=1 AND device_accounts.device_id=@device_id {where} ORDER BY {order_by} LIMIT @offset, @limit";


        public const string TABLE_DEVICE_ACCOUNT_DROP = " DROP TABLE IF EXISTS device_accounts; ";

        public const string TABLE_DEVICE_ACCOUNT_INSERT_BATCH = " INSERT INTO device_accounts(device_id, account_id ) VALUES( @device_id , @account_id ) ";


        public const string TABLE_ACOUNT_CREATE = " CREATE TABLE IF NOT EXISTS accounts (id INTEGER, uid TEXT DEFAULT NULL, group_device_id INTEGER DEFAULT 0, name TEXT DEFAULT NULL, password TEXT DEFAULT NULL, email TEXT DEFAULT NULL, description TEXT DEFAULT NULL, status INTEGER DEFAULT 1, updated_at INTEGER DEFAULT 0, device_id TEXT DEFAULT NULL, total_group INTEGER DEFAULT 0, total_friend INTEGER DEFAULT 0, gender TEXT DEFAULT NULL, birthday INTEGER DEFAULT 0, token	TEXT DEFAULT NULL, twofa TEXT DEFAULT NULL, is_join INTEGER DEFAULT 0, is_leave INTEGER DEFAULT 0, fblite_package TEXT DEFAULT NULL, proxy TEXT DEFAULT NULL,pending_join TEXT DEFAULT NULL, share_type_id INTEGER DEFAULT 0,PRIMARY KEY(id AUTOINCREMENT));";



        public const string TABLE_CACHE_CREATE = "CREATE TABLE IF NOT EXISTS caches( id INTEGER, key TEXT DEFAULT NULL, value TEXT DEFAULT NULL,PRIMARY KEY(id AUTOINCREMENT)); ";
        public const string TABLE_CACHE_SELECT_BY_KEY = " SELECT * FROM caches WHERE key=@key Limit 0,1";
        public const string TABLE_CACHE_UPDATE = " UPDATE caches SET value=@value WHERE key=@key";
        public const string TABLE_CACHE_UPDATE_TOTAL = " UPDATE caches SET total=@total WHERE key=@key";
        public const string TABLE_CACHE_INSERT = " INSERT INTO caches (key,value) VALUES (@key,@value)";
        public const string TABLE_CACHE_INSERT_TOTAL = " INSERT INTO caches (key,total) VALUES (@key,@total)";



        public const string TABLE_GROUP_CREATE = "CREATE TABLE IF NOT EXISTS groups( id TEXT PRIMARY KEY , name TEXT DEFAULT NULL,created_by TEXT DEFAULT  NULL, state INTEGER default 1  ,updated_at INTEGER  DEFAULT 0  ); ";
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
