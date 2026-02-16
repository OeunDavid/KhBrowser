namespace ToolLib.Data
{
    public partial class SQLConstant
    {
        public static class AccountGroupSQL
        {
            public const string TABLE_ACCOUNT_GROUP_CREATE = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('account_groups') AND type in (N'U')) CREATE TABLE account_groups( account_id NVARCHAR(450) , group_id NVARCHAR(450) , state INT default 0, PRIMARY KEY (account_id , group_id )) ";
            public const string TABLE_ACCOUNT_GROUP_DELETE_BY_ACCOUNT = "DELETE FROM account_groups WHERE account_id IN ( @id )";
            public const string TABLE_ACCOUNT_GROUP_INSERT = "INSERT INTO account_groups( account_id , group_id , state ) VALUES( @account_id , @group_id , @state ) ";
            public const string TABLE_ACCOUNT_GROUP_SELECT_BY_ACCOUNT = "SELECT * FROM account_groups WHERE account_id = @id ";
        }
    }
}
