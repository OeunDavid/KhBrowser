namespace ToolLib.Data
{
    public partial class SQLConstant
    {
        public static class ActivityLogSQL
        {
            public const string TABLE_ACTIVITY_LOG_CREATE = " IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('activity_logs') AND type in (N'U')) CREATE TABLE activity_logs( id INT IDENTITY(1,1) PRIMARY KEY, device_id NVARCHAR(MAX) DEFAULT NULL, uid NVARCHAR(MAX) DEFAULT NULL, action_name NVARCHAR(MAX) DEFAULT NULL, action_date BIGINT DEFAULT 0, description NVARCHAR(MAX) DEFAULT NULL, share_timeline INT DEFAULT 0, share_groups INT DEFAULT 0, url NVARCHAR(MAX) DEFAULT NULL );";
            public const string TABLE_ACTIVITY_LOG_INSERT = " INSERT INTO activity_logs(device_id, uid, action_name, action_date, description, url, share_timeline, share_groups ) VALUES (@device_id , @uid , @action_name, @action_date, @description, @url, @share_timeline, @share_groups ) ";
            public const string TABLE_ACTIVITY_LOG_DELETE = " DELETE FROM activity_logs WHERE id= @id ;";
            public const string TABLE_ACTIVITY_LOG_TRUNCATE = "DELETE FROM activity_logs; ";
            public const string TABLE_ACTIVITY_LOG_SELECT_ALL = "SELECT * FROM activity_logs ";
            public const string TABLE_ACTIVITY_LOG_SELECT_BY_UID = "SELECT * FROM activity_logs WHERE uid=@uid ORDER BY action_date desc";
            public const string TABLE_ACTIVITY_LOG_SELECT_ONE = "SELECT * FROM activity_logs WHERE id=@id";
            public const string TABLE_ACTIVITY_LOG_TOTAL_SHARE_GROUP = " SELECT COUNT(id) AS total FROM activity_logs WHERE uid=@uid AND action_name='ShareToGroup' AND action_date>=@start_date AND action_date<=@end_date";
            public const string TABLE_ACTIVITY_LOG_TOTAL_SHARE_TIMELINE = " SELECT COUNT(id) AS total FROM activity_logs WHERE uid=@uid AND action_name='ShareToTimeline' AND action_date>=@start_date AND action_date<=@end_date";
        }
    }
}
