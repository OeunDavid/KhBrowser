namespace ToolLib.Data
{
    public partial class SQLConstant
    {
        public static class DeviceAccountSQL
        {
            public const string TABLE_DEVICE_ACCOUNT_CREATION = " IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('device_accounts') AND type in (N'U')) CREATE TABLE device_accounts( id INT IDENTITY(1,1) PRIMARY KEY, device_id INT DEFAULT 0, account_id INT DEFAULT 0, uid NVARCHAR(MAX) DEFAULT NULL, package_id NVARCHAR(MAX) DEFAULT NULL, apk_id INT DEFAULT 0, status INT DEFAULT 0, updated_at BIGINT DEFAULT 0, active_date BIGINT DEFAULT 0, share_date BIGINT DEFAULT 0, join_date BIGINT DEFAULT 0, leave_date BIGINT DEFAULT 0, dark_mode INT DEFAULT 0, description NVARCHAR(MAX) DEFAULT NULL);";
            public const string TABLE_DEVICE_ACCOUNT_ADD_COLUMN_ACCOUNT_ID = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('device_accounts') AND name = 'account_id') ALTER TABLE device_accounts ADD account_id INT DEFAULT 0;";
            public const string TABLE_DEVICE_ACCOUNT_BY_DEVICE = " SELECT device_accounts.*,accounts.status AS account_status FROM device_accounts LEFT JOIN accounts ON accounts.uid=device_accounts.uid WHERE device_accounts.device_id= @device_id";
            public const string TABLE_DEVICE_ACCOUNT_NO_FBLITE = " SELECT * FROM device_accounts WHERE device_id= @device_id and (package_id='' or package_id is null) ORDER BY apk_id desc OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";
            public const string TABLE_DEVICE_ACCOUNT_SELECT_BY_DEVICE_NO_PACKAGE = " SELECT * FROM device_accounts WHERE device_id= @device_id and (package_id is null or package_id=''); ";
            public const string TABLE_DEVICE_ACCOUNT_SELECT_BY_DEVICE_NO_UID = " SELECT * FROM device_accounts WHERE device_id= @device_id and (uid is null or uid='') and package_id is not null; ";
            public const string TABLE_DEVICE_ACCOUNT_SELECT_ONE= " SELECT * FROM device_accounts WHERE id=@id ORDER BY id DESC;";
            public const string TABLE_DEVICE_ACCOUNT_SELECT_ONE_LAST_BY_DEVICE= " SELECT TOP(1) * FROM device_accounts WHERE device_id=@device_id ORDER BY id DESC;";
            public const string TABLE_DEVICE_ACCOUNT_SELECT_BY_DEVICE_PENDING_UID_LOGIN = " SELECT device_accounts.*,accounts.password,accounts.twofa,accounts.token,accounts.proxy FROM device_accounts inner join accounts on accounts.uid=device_accounts.uid WHERE device_accounts.device_id=@device_id and (device_accounts.uid is not null or device_accounts.uid<>'') and (device_accounts.package_id is not null or device_accounts.package_id<>'') and device_accounts.status=0 and accounts.status=1; ";
            public const string TABLE_DEVICE_ACCOUNT_INSERT_APK = " INSERT INTO device_accounts(device_id,apk_id) VALUES(@device_id,@apk_id);";
            public const string TABLE_DEVICE_ACCOUNT_UPDATE_PACKAGE = " UPDATE device_accounts SET package_id=@package_id, description=@description WHERE id=@id;";
            public const string TABLE_DEVICE_ACCOUNT_UPDATE_UID = " UPDATE device_accounts SET uid=@uid WHERE (uid is null or uid='') and id=(select TOP(1) id from device_accounts where device_id=@device_id and (uid is null or uid='') ORDER BY id) ;";
            public const string TABLE_DEVICE_ACCOUNT_UPDATE_STATUS = " UPDATE device_accounts SET status=@status,description=@description,updated_at=@updated_at WHERE id=@id;";
            public const string TABLE_DEVICE_ACCOUNT_UPDATE_DARK_MODE = " UPDATE device_accounts SET dark_mode=@dark_mode WHERE id=@id;";
            public const string TABLE_DEVICE_ACCOUNT_UPDATE_ACTIVE = " UPDATE device_accounts SET status=@status,description=@description {customFieldUpdate},updated_at=@updated_at WHERE id=@id;";
            public const string TABLE_DEVICE_ACCOUNT_UPDATE = " UPDATE device_accounts SET status=@status,description=@description,updated_at=@updated_at,uid=@uid,package_id=@package_id WHERE id=@id;";
            public const string TABLE_DEVICE_ACCOUNT_DELETE = " DELETE FROM device_accounts WHERE id=@id;";
            public const string TABLE_DEVICE_ACCOUNT_TOTAL_NO_ACCOUNT = " SELECT COUNT(id) AS total FROM device_accounts WHERE device_id=@device_id AND (uid='' OR uid is null)";

            public const string TABLE_DEVICE_ACCOUNT_SELECT_ALL = " SELECT device_accounts.*, accounts.pending_join, accounts.is_leave, accounts.share_type_id FROM device_accounts INNER JOIN accounts ON device_accounts.uid=accounts.uid AND device_accounts.device_id=accounts.device_id WHERE device_accounts.status=@status AND accounts.status=1 AND device_accounts.device_id=@device_id {where} ORDER BY {order_by} OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY";

            public const string TABLE_DEVICE_ACCOUNT_DROP = " IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('device_accounts') AND type in (N'U')) DROP TABLE device_accounts; ";

            public const string TABLE_DEVICE_ACCOUNT_INSERT_BATCH = " INSERT INTO device_accounts(device_id, account_id ) VALUES( @device_id , @account_id ) ";
        }
    }
}
