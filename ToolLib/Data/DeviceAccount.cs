using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLib.Data
{
    public class DeviceAccount
    {
        public int Key { get; set; }
        public long Id { get; set; }
        public string DeviceID { get; set; }
        public string UID { get; set; }
        public string PackageID { get; set; }
        public int APKID { get; set; }
        public string Description { get; set; }
        public string TextStatus { get; set; }
        public int Status { get; set; }
        public long UpdatedAt { get; set; }
        public string TextUpdatedAt { get; set; }
        public int DarkMode { get; set; }

        public static DeviceAccount from(DataRow row)
        {
            long id = (long)row["id"];
            string device_id = row["device_id"] + "";
            string uid = row["uid"] + "";
            string package_id = row["package_id"] + "";
            int apk_id = Int32.Parse(row["apk_id"].ToString());
            int status = Int32.Parse(row["status"].ToString());
            int darkMode = Int32.Parse(row["dark_mode"].ToString());
            long updated_at = (long)row["updated_at"];
            string description = row["description"].ToString().Trim();

            var data = new DeviceAccount()
            {
                Id = id,
                UID = uid,
                DeviceID = device_id,
                PackageID = package_id,
                APKID = apk_id,
                UpdatedAt = updated_at,
                Description = description,
                DarkMode = darkMode,
                Status = status
            };

            return data;
        }
    }
}
