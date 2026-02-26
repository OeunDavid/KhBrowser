namespace ToolLib.Data
{
    public partial class SQLConstant
    {
        public static class AccountSQL
        {
            public const string TABLE_ACCOUNT_ADD_COLUMN_OLD_GROUP_IDS = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'old_group_ids') ALTER TABLE accounts ADD old_group_ids NVARCHAR(MAX) NULL;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_CREATION_DATE = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'creation_date') ALTER TABLE accounts ADD creation_date NVARCHAR(MAX) NULL;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_TIMELINE_SOURCE = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'timeline_source') ALTER TABLE accounts ADD timeline_source NVARCHAR(MAX) NULL;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_REEL_SOURCE_VIDEO = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'reel_source_video') ALTER TABLE accounts ADD reel_source_video NVARCHAR(MAX) NULL;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_PRIMARY_LOCATION = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'primaryLocation') ALTER TABLE accounts ADD primaryLocation NVARCHAR(MAX) NULL;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_MAIL_PASS = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'mailPass') ALTER TABLE accounts ADD mailPass NVARCHAR(MAX) NULL;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_PAGE = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'total_page') ALTER TABLE accounts ADD total_page INT DEFAULT 0;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_PAGE_IDS = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'page_ids') ALTER TABLE accounts ADD page_ids NVARCHAR(MAX) NULL;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_NOTE = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'note') ALTER TABLE accounts ADD note NVARCHAR(MAX) NULL;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_TEMP_STORE_ID = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'temp_store_id') ALTER TABLE accounts ADD temp_store_id INT DEFAULT 0;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_TEMP_STORE_ORDER = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'temp_order') ALTER TABLE accounts ADD temp_order INT DEFAULT 0;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_TOTAL_SHARE_GROUP = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'total_share_group') ALTER TABLE accounts ADD total_share_group INT DEFAULT 0;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_TOTAL_SHARE_TIMELINE = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'total_share_timeline') ALTER TABLE accounts ADD total_share_timeline INT DEFAULT 0;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_IS_VERIFY = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'is_verify') ALTER TABLE accounts ADD is_verify INT DEFAULT 0;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_IS_TWOFA = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'is_twofa') ALTER TABLE accounts ADD is_twofa INT DEFAULT 0;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_STORE_ID = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'store_id') ALTER TABLE accounts ADD store_id INT DEFAULT 0;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_IS_SHARE = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'is_share') ALTER TABLE accounts ADD is_share INT DEFAULT 0;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_IS_LOGIN = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'is_login') ALTER TABLE accounts ADD is_login INT DEFAULT 0;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_IS_ACTIVE = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'is_active') ALTER TABLE accounts ADD is_active INT DEFAULT 0;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_IS_JOIN = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'is_join') ALTER TABLE accounts ADD is_join INT DEFAULT 0;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_IS_LEAVE = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'is_leave') ALTER TABLE accounts ADD is_leave INT DEFAULT 0;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_USER_AGENT = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'user_agent') ALTER TABLE accounts ADD user_agent NVARCHAR(MAX) NULL;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_PENDING_JOIN = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'pending_join') ALTER TABLE accounts ADD pending_join NVARCHAR(MAX) NULL;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_FRIENDS_REQUEST = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'friends_request') ALTER TABLE accounts ADD friends_request INT DEFAULT 0;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_DOB = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'dob') ALTER TABLE accounts ADD dob NVARCHAR(MAX) NULL;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_FBLITE_PACKAGE = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'fblite_package') ALTER TABLE accounts ADD fblite_package NVARCHAR(MAX) NULL;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_GROUP_IDS = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'group_ids') ALTER TABLE accounts ADD group_ids NVARCHAR(MAX) NULL;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_COOKIE = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'cookie') ALTER TABLE accounts ADD cookie NVARCHAR(MAX) NULL;";
            public const string TABLE_ACCOUNT_ADD_COLUMN_TWOFA = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('accounts') AND name = 'twofa') ALTER TABLE accounts ADD twofa NVARCHAR(MAX) NULL;";

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
            public const string TABLE_ACCOUNT_SELECT_ALL_BY_STORE = " SELECT accounts.*, temp_group.name as temp_name, store_group.name as store_name FROM accounts left join group_devices temp_group on temp_group.id=accounts.temp_store_id left join group_devices store_group on store_group.id=accounts.store_id where accounts.store_id=@store_id {where} ORDER BY accounts.id asc";
            public const string TABLE_ACCOUNT_SELECT_ALL_BY_TEMP_STORE = " SELECT accounts.*, store_group.name as temp_name, main_store.name as store_name FROM accounts left join group_devices store_group on store_group.id=accounts.store_id left join group_devices main_store on main_store.id=accounts.store_id where temp_store_id=@temp_store_id {where} ORDER BY temp_order asc";
            public const string TABLE_ACCOUNT_SELECT_ALL_FILTER = " SELECT accounts.*, temp_group.name as temp_name, store_group.name as store_name FROM accounts left join group_devices temp_group on temp_group.id=accounts.temp_store_id left join group_devices store_group on store_group.id=accounts.store_id where 1=1 {where} ORDER BY accounts.id asc";
            public const string TABLE_ACCOUNT_SELECT_ALL = " SELECT * FROM accounts where 1=1 {where} ORDER BY id asc OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";

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
            public const string TABLE_ACCOUNT_SELECT_ALL_NO_DEVICE = " SELECT TOP(@limit) id, uid FROM accounts WHERE status=1 and share_type_id IN ({shareTypeIds}) and group_device_id=@group_device_id and (device_id is null or device_id='')";
            public const string TABLE_ACCOUNT_DELETE_BY_UID = " DELETE FROM accounts WHERE uid IN ({uid}); ";

            public const string TABLE_ACCOUNT_UPDATE = "UPDATE accounts SET password= @password , description = @description , status= @status , updated_at = @updated_at, device_id = @device_id , total_group =@total_group WHERE id = @id ";
            public const string TABLE_ACCOUNT_DELETE = "DELETE FROM accounts WHERE id = @id ";
            public const string TABLE_ACCOUNT_UPDATE_SECURITY = "UPDATE accounts SET mailPass=@mailPass,store_id=@store_id, twofa=@twofa,password=@password WHERE uid=@uid ";
            public const string TABLE_ACCOUNT_UPDATE_STORE = "UPDATE accounts SET store_id=@store_id WHERE uid=@uid ";
            public const string TABLE_ACCOUNT_SELEC_ALL = " SELECT * FROM accounts";
            public const string TABLE_ACCOUNT_DROP = " IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('accounts') AND type in (N'U')) DROP TABLE accounts;";
            public const string TABLE_ACCOUNT_SELECT_BY_ID = " SELECT * FROM accounts WHERE id IN ( @id)";
            public const string TABLE_ACCOUNT_SELECT_BY_DEVICE = "SELECT * FROM accounts WHERE device_id = @device_id";
            public const string TABLE_ACCOUNT_SELECT_ONE = " SELECT * FROM accounts WHERE id = @id ";
            public const string TABLE_ACCOUNT_SELECT_ONE_BY_UID = " SELECT * FROM accounts WHERE uid= @uid ";

            public const string TABLE_ACOUNT_CREATE = " IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('accounts') AND type in (N'U')) CREATE TABLE accounts (id INT IDENTITY(1,1) PRIMARY KEY, uid NVARCHAR(MAX) DEFAULT NULL, store_id INT DEFAULT 0, group_device_id INT DEFAULT 0, name NVARCHAR(MAX) DEFAULT NULL, password NVARCHAR(MAX) DEFAULT NULL, email NVARCHAR(MAX) DEFAULT NULL, description NVARCHAR(MAX) DEFAULT NULL, status INT DEFAULT 1, updated_at BIGINT DEFAULT 0, device_id NVARCHAR(MAX) DEFAULT NULL, total_group INT DEFAULT 0, total_friend INT DEFAULT 0, gender NVARCHAR(MAX) DEFAULT NULL, birthday BIGINT DEFAULT 0, token NVARCHAR(MAX) DEFAULT NULL, twofa NVARCHAR(MAX) DEFAULT NULL, is_join INT DEFAULT 0, is_leave INT DEFAULT 0, is_share INT DEFAULT 0, is_login INT DEFAULT 0, is_active INT DEFAULT 0, is_verify INT DEFAULT 0, is_twofa INT DEFAULT 0, cookie NVARCHAR(MAX) DEFAULT NULL, proxy NVARCHAR(MAX) DEFAULT NULL, user_agent NVARCHAR(MAX) DEFAULT NULL, pending_join NVARCHAR(MAX) DEFAULT NULL, share_type_id INT DEFAULT 0, total_page INT DEFAULT 0, page_ids NVARCHAR(MAX) DEFAULT NULL, friends_request INT DEFAULT 0, note NVARCHAR(MAX) DEFAULT NULL, mailPass NVARCHAR(MAX) DEFAULT NULL, primaryLocation NVARCHAR(MAX) DEFAULT NULL, reel_source_video NVARCHAR(MAX) DEFAULT NULL, timeline_source NVARCHAR(MAX) DEFAULT NULL, creation_date NVARCHAR(MAX) DEFAULT NULL, old_group_ids NVARCHAR(MAX) DEFAULT NULL, group_ids NVARCHAR(MAX) DEFAULT NULL, temp_store_id INT DEFAULT 0, temp_order BIGINT DEFAULT 0, dob NVARCHAR(MAX) DEFAULT NULL, total_share_group INT DEFAULT 0, total_share_timeline INT DEFAULT 0, fblite_package NVARCHAR(MAX) DEFAULT NULL);";

            public const string TABLE_ACCOUNT_UPDATE_GROUP_DEVICE = " UPDATE accounts SET group_device_id=@group_device_id WHERE device_id=@device_id ";
            public const string TABLE_ACCOUNT_UPDATE_GROUP_DEVICE_BY_UID = " UPDATE accounts SET group_device_id=@group_device_id WHERE uid=@uid AND (device_id IS NULL OR device_id=''); ";

            public const string TABLE_ACCOUNT_TOTAL_LIVE = " SELECT COUNT(id) as total_account_live FROM accounts WHERE status=1;";
            public const string TABLE_ACCOUNT_TOTAL_DIE = " SELECT COUNT(id) AS total_account_die FROM accounts WHERE status!=1;";
            public const string TABLE_ACCOUNT_UPDATE_PASSWORD = "UPDATE accounts SET Password = @password WHERE UID = @uid";

        }
    }
}
