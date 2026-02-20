namespace ToolLib.Data
{
    public partial class SQLConstant
    {
        public static class GroupSSQL
        {
            public const string TABLE_GROUPSS_SELECT_BY_UID = "SELECT * FROM groupss WHERE uid=@uid";
            public const string TABLE_GROUPSS_INSERT = "INSERT INTO groupss(uid,name,page_id,status,group_id,member,pending,check_pending) VALUES(@uid,@name,@page_id,@status,@group_id,@member,@pending,@check_pending)";
            public const string TABLE_GROUPSS_UPDATE_PENDING = "UPDATE groupss SET pending=@pending, check_pending=1 WHERE group_id=@group_id";
            public const string TABLE_GROUPSS_SELECT_SINGLE_RECORD = "SELECT * FROM groupss WHERE uid=@uid and group_id=@group_id";
            public const string TABLE_GROUPSS_DELETE = "DELETE FROM groupss WHERE uid= @uid and group_id=@group_id";
            public const string TABLE_GROUPSS_DELETE_BY_UID = "DELETE FROM groupss WHERE uid= @uid";
            public const string TABLE_GROUPSS_CREATE = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('groupss') AND type in (N'U')) CREATE TABLE groupss(id INT IDENTITY(1,1) PRIMARY KEY, uid NVARCHAR(MAX) DEFAULT NULL, group_id NVARCHAR(MAX) DEFAULT NULL , name NVARCHAR(MAX) DEFAULT NULL, member INT DEFAULT 0, page_id NVARCHAR(MAX) DEFAULT NULL,is_join INT DEFAULT 0, check_pending INT DEFAULT 0, pending INT DEFAULT 0, status INT default 1); ";
        }
    }
}
