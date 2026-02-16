namespace ToolLib.Data
{
    public partial class SQLConstant
    {
        public static class GroupDeviceSQL
        {
            public const string TABLE_STORE_ADD_COLUMN_IS_TEMP = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('group_devices') AND name = 'is_temp') ALTER TABLE group_devices ADD is_temp INT DEFAULT 0;";
            public const string TABLE_GROUP_DEVICES_CREATE = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('group_devices') AND type in (N'U')) CREATE TABLE group_devices(id INT IDENTITY(1,1) PRIMARY KEY, name NVARCHAR(MAX) DEFAULT NULL, description NVARCHAR(MAX) DEFAULT NULL, status INT DEFAULT 1);";
            public const string TABLE_GROUP_DEVICES_INSERT = "INSERT INTO group_devices( name, description, status )  VALUES( @name , @description, 1 ) ";
            public const string TABLE_GROUP_DEVICES_UPDATE = "UPDATE group_devices SET name = @name , description= @description, status= @status WHERE id = @id ;";
            public const string TABLE_GROUP_DEVICES_DELETE = "DELETE FROM group_devices WHERE id = @id ;";
            public const string TABLE_GROUP_DEVICES_SELECT_ALL = "SELECT * FROM group_devices ";
            public const string TABLE_GROUP_DEVICES_SELECT_ONE = "SELECT * FROM group_devices WHERE id=@id";
            public const string TABLE_GROUP_DEVICES_DROP = "IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('group_devices') AND type in (N'U')) DROP TABLE group_devices;";
        }
    }
}
