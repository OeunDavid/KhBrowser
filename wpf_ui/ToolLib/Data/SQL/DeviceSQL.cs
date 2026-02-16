namespace ToolLib.Data
{
    public partial class SQLConstant
    {
        public static class DeviceSQL
        {
            public const string TABLE_DEVICE_ADD_COLUMN_SHARE_DELAY = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('devices') AND name = 'share_delay') ALTER TABLE devices ADD share_delay INT DEFAULT 0;";
            public const string TABLE_DEVICE_ADD_COLUMN_MODEL = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('devices') AND name = 'model') ALTER TABLE devices ADD model NVARCHAR(MAX) DEFAULT NULL;";

            public const string TABLE_DEVICE_CREATE = " IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('devices') AND type in (N'U')) CREATE TABLE devices( id INT IDENTITY(1,1) PRIMARY KEY, is_expire INT DEFAULT 0, data_mode INT DEFAULT 0, group_device_id INT DEFAULT 0, internet_id INT DEFAULT 0, device_id NVARCHAR(MAX) DEFAULT NULL,name NVARCHAR(MAX) DEFAULT NULL, total_account INT DEFAULT 0, total_available INT DEFAULT 0, total_fblite INT DEFAULT 0, is_busy INT DEFAULT 0, status INT DEFAULT 0, description NVARCHAR(MAX) DEFAULT NULL, updated_at BIGINT DEFAULT 0, key_press_for_text INT DEFAULT 0, active_date BIGINT DEFAULT 0); ";
            public const string TABLE_DEVICE_DROP = " IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('devices') AND type in (N'U')) DROP TABLE devices; ";
            public const string TABLE_DEVICE_INSERT = " INSERT INTO devices(device_id,name,data_mode,total_account,total_available,description,key_press_for_text,group_device_id,internet_id) VALUES(@device_id,@name,@data_mode,@total_account,@total_available,@description,@key_press_for_text,@group_device_id,@internet_id) ;";
            public const string TABLE_DEVICE_TOTAL_DEVICE = " SELECT COUNT(id) as total_device FROM devices WHERE status=1 AND internet_id=@internet_id and group_device_id=@group_device_id and is_busy=@is_busy";
            public const string TABLE_DEVICE_ALL_TOTAL_DEVICE = " SELECT COUNT(id) as total_device FROM devices WHERE status=1 AND group_device_id=@group_device_id and is_busy=@is_busy";
            public const string TABLE_DEVICE_UPDATE_CONNECTION = " UPDATE devices SET status =@status , description= @description, key_press_for_text= @key_press_for_text WHERE id = @id  ;";
            public const string TABLE_DEVICE_UPDATE_ACTIVE = " UPDATE devices SET active_date=@active_date, description= @description WHERE id=@id  ;";
            public const string TABLE_DEVICE_UPDATE_EXPIRE = " UPDATE devices SET is_expire=@is_expire WHERE device_id=@device_id;";
            
            public const string TABLE_DEVICE_SELECT_ALL_ACTION = " SELECT * FROM devices WHERE is_expire=0 AND is_busy=0 AND status=@status AND group_device_id=@group_device_id AND internet_id IN ({internets}) ORDER BY {order_by} OFFSET 0 ROWS FETCH NEXT @limit ROWS ONLY";
            
            public const string TABLE_DEVICE_UPDATE = " UPDATE devices SET status=@status , description= @description, key_press_for_text= @key_press_for_text WHERE id = @id  ;";
            public const string TABLE_DEVICE_UPDATE_FBLITE = " UPDATE devices SET total_fblite=@total_fblite WHERE device_id=@device_id;";
            public const string TABLE_DEVICE_UPDATE_BUSY = " UPDATE devices SET is_busy=@is_busy WHERE id=@id;";
            public const string TABLE_DEVICE_UPDATE_AVAILABLE = " UPDATE devices SET total_available=@total_available WHERE id=@id;";
            public const string TABLE_DEVICE_UPDATE_DATA_MODE = " UPDATE devices SET data_mode=@data_mode WHERE device_id=@device_id;";
            public const string TABLE_DEVICE_DELETE = " DELETE FROM devices WHERE id= @id ;";
            public const string TABLE_DEVICE_TRUNCATE = "TRUNCATE TABLE devices; ";
            public const string TABLE_DEVICE_SELECT_ALL = "SELECT * FROM devices ORDER BY name asc";
            public const string TABLE_DEVICE_SELECT_ALL_GROUP = "SELECT devices.*, (SELECT COUNT(id) FROM device_accounts WHERE device_accounts.device_id=devices.device_id AND device_accounts.status=1 AND device_accounts.uid IS NOT NULL AND device_accounts.uid!='' AND device_accounts.package_id IS NOT NULL AND device_accounts.package_id!='') AS account_live FROM devices WHERE devices.group_device_id=@group_device_id ORDER BY devices.name asc";
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

            public const string TABLE_DEVICE_UPDATE_GROUP = " UPDATE devices SET group_device_id=@group_device_id WHERE device_id=@device_id;";

            public const string TABLE_DEVICE_TOTAL_ALL = " SELECT COUNT(id) AS total_device FROM devices";
        }
    }
}
