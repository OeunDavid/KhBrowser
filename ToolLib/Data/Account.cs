using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLib.Data
{
    public class Account
    {
        public long Id { get; set; }
        public int GroupDeviceId { get; set; }
        public string UID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Gender { get; set; }
        public long Birthday { get; set; }

        public string TwoFA { get; set; }
        public string Token { get; set; }
        public int Join { get; set; }
        public int Leave { get; set; }
        public int TotalFriend { get; set; }
        public int TotalGroup { get; set; }

        public string Description { get; set; }
        public int Status { get; set; }
        public long UpdatedAt { get; set; }
        public string DeviceId { get; set; }
        public string FbLitePackage { get; set; }
        public string Proxy { get; set; }
        public string PendingJoin { get; set; }
        public int ShareTypeId { get; set; }

        public static Account from(DataRow row)
        {
            long id = Convert.ToInt64(row["id"]);
            string uid = row["uid"].ToString().Trim();
            string name = row["name"] + "";
            string password = row["password"].ToString().Trim();
            string email = row["email"] + "";
            string gender = row["gender"] + "";
            long birthday = (long)row["birthday"];

            string twofa = row["twofa"].ToString().Trim();
            string token = row["token"] + "";
            string proxy = row["proxy"] + "";
            string pendingJoin = row["pending_join"] + "";

            string description = row["description"] + "";
            long updatedAt = (long)row["updated_at"];
            int status = Convert.ToInt32(row["status"]);
            int isLeave = Convert.ToInt32(row["is_leave"]);
            int groupDeviceId = Convert.ToInt32(row["group_device_id"]);
            var deviceId = row["device_id"] + "";
            var totalGroup = Convert.ToInt32(row["total_group"]);
            var totalFriend = Convert.ToInt32(row["total_friend"]);
            var shareTypeId = Convert.ToInt32(row["share_type_id"]);

            var acc = new Account()
            {
                Id = id,
                Name = name,
                Password = password,
                UID = uid,
                Email = email,
                Birthday = birthday,
                Gender = gender,
                ShareTypeId = shareTypeId,

                TwoFA = twofa,
                Token = token,
                Proxy = proxy,
                TotalGroup = totalGroup,
                TotalFriend = totalFriend,
                Leave = isLeave,

                UpdatedAt = updatedAt,
                Description = description,
                Status = status,
                GroupDeviceId = groupDeviceId,
                PendingJoin = pendingJoin,

                DeviceId = deviceId
            };

            return acc;
        }
    }    
}
