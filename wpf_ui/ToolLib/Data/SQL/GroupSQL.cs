namespace ToolLib.Data
{
    public partial class SQLConstant
    {
        public static class GroupSQL
        {
            public const string TABLE_GROUP_CREATE = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('groups') AND type in (N'U')) CREATE TABLE groups( id NVARCHAR(450) PRIMARY KEY , name NVARCHAR(MAX) DEFAULT NULL,created_by NVARCHAR(MAX) DEFAULT  NULL, state INT default 1  ,updated_at BIGINT  DEFAULT 0  ); ";
            public const string TABLE_GROUP_INSERT = "INSERT INTO groups( id , name ,created_by , state ,updated_at  ) VALUES( @id , @name, @created_by , @state, @updated_at ) ;";
            public const string TABLE_GROUP_UPDATE = "UPDATE groups SET name = @name , state= @state, updated_at = @updated_at WHERE id = @id ;";
            public const string TABLE_GROUP_DELETE = "DELETE FROM groups WHERE id = @id ;";
            public const string TABLE_GROUP_TRUNCATE = "DELETE FROM groups; ";
            public const string TABLE_GROUP_SELECT_ALL = "SELECT * FROM groups ";
            public const string TABLE_GROUP_SELECT_ONE = "SELECT * FROM groups WHERE id=@id";
        }
        public const string TABLE_GROUP_SELECT_BY_ACCOUNT_ID = " SELECT g.* FROM groups g INNER JOIN account_groups c ON g.id = c.group_id WHERE c.account_id = @id";
    }
}
