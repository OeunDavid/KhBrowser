namespace ToolLib.Data
{
    public partial class SQLConstant
    {
        public static class StoreSQL
        {
            public const string TABLE_STORE_ADD_COLUMN_IS_TEMP = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('stores') AND name = 'is_temp') ALTER TABLE stores ADD is_temp INT DEFAULT 0;";
            public const string TABLE_STORE_CREATE = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('stores') AND type in (N'U')) CREATE TABLE stores(id INT IDENTITY(1,1) PRIMARY KEY, name NVARCHAR(MAX) DEFAULT NULL, note NVARCHAR(MAX) DEFAULT NULL, created_by NVARCHAR(MAX) DEFAULT NULL, state INT DEFAULT 1, updated_at BIGINT DEFAULT 0, is_temp INT DEFAULT 0);";
            public const string TABLE_STORE_INSERT = "INSERT INTO stores( name, note, created_by, state, updated_at, is_temp ) VALUES( @name , @note, @created_by, @state, @updated_at, @is_temp ) ";
            public const string TABLE_STORE_UPDATE = "UPDATE stores SET name = @name , note =@note, state= @state, updated_at = @updated_at, is_temp = @is_temp WHERE id = @id ;";
            public const string TABLE_STORE_DELETE = "DELETE FROM stores WHERE id = @id ;";
            public const string TABLE_STORE_SELECT_ALL = "SELECT * FROM stores ";
            public const string TABLE_STORE_SELECT_ONE = "SELECT * FROM stores WHERE id=@id";
            public const string TABLE_STORE_TOTAL_ALL = " SELECT COUNT(id) AS total_store FROM stores";
        }
    }
}
