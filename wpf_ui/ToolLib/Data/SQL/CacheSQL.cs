namespace ToolLib.Data
{
    public partial class SQLConstant
    {
        public static class CacheSQL
        {
            public const string TABLE_CACHE_ADD_COLUMN_TOTAL = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('caches') AND name = 'total') ALTER TABLE caches ADD total INT DEFAULT 0;";
            public const string TABLE_CACHE_CREATE = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('caches') AND type in (N'U')) CREATE TABLE caches( id INT IDENTITY(1,1) PRIMARY KEY, [key] NVARCHAR(450) DEFAULT NULL, value NVARCHAR(MAX) DEFAULT NULL); ";
            public const string TABLE_CACHE_SELECT_BY_KEY = " SELECT TOP(1) * FROM caches WHERE [key]=@key";
            public const string TABLE_CACHE_UPDATE = " UPDATE caches SET value=@value WHERE [key]=@key";
            public const string TABLE_CACHE_UPDATE_TOTAL = " UPDATE caches SET total=@total WHERE [key]=@key";
            public const string TABLE_CACHE_INSERT = " INSERT INTO caches ([key],value) VALUES (@key,@value)";
            public const string TABLE_CACHE_INSERT_TOTAL = " INSERT INTO caches ([key],total) VALUES (@key,@total)";
        }
    }
}
