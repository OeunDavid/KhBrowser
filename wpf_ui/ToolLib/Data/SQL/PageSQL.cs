namespace ToolLib.Data
{
    public partial class SQLConstant
    {
        public static class PageSQL
        {
            public const string TABLE_PAGES_SELECT_BY_UID = "SELECT * FROM pages WHERE uid=@uid";
            public const string TABLE_PAGES_DELETE_BY_UID = "DELETE FROM pages WHERE uid= @uid";
            public const string TABLE_PAGES_INSERT = "INSERT INTO pages(uid,name,page_id,status,access_token) VALUES(@uid,@name,@page_id,@status,@access_token)";
            public const string TABLE_PAGES_CREATE = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('pages') AND type in (N'U')) CREATE TABLE pages(id INT IDENTITY(1,1) PRIMARY KEY, uid NVARCHAR(MAX) DEFAULT NULL, access_token NVARCHAR(MAX) DEFAULT NULL , name NVARCHAR(MAX) DEFAULT NULL, page_id NVARCHAR(MAX) DEFAULT NULL, status INT default 1); ";
        }
    }
}
