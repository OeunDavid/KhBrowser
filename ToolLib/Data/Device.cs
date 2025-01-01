using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLib
{
    public class Device
    {
        public int Key { get; set; }
        public int Id { get; set; }
        public int Status { get; set; }
        public int IsBusy { get; set; }
        public int TotalAccount { get; set; }
        public int TotalAvailable { get; set; }
        public int TotalFBLite { get; set; }
        public string Name { get; set; }
        public string DeviceID { get; set; }
        public string Description { get; set; }
        public long UpdatedAt { get; set; }
        public string LastAccount { get; set; }
        public string LastAction { get; set; }
        public int KeyPressForText { get; set; }
        public int GroupDeviceId { get; set; }
        public int InternetId { get; set; }
        public int DataMode { get; set; }
        public long ActiveDate { get; set; }
        public string TextActiveDate { get; set; }
        public string WorkingDate { get; set; }
        public int AccountLive { get; set; }
        public int Expire { get; set; }

        public static Device from(DataRow row)
        {
            int id = Int32.Parse(row["id"].ToString());
            string device_id = row["device_id"] + "";
            string name = row["name"] + "";
            int is_busy = Int32.Parse(row["is_busy"].ToString());
            string description = row["description"] + "";
            long updatedAt = (long)row["updated_at"];
            int status = Convert.ToInt32(row["status"]);
            int total_account = Convert.ToInt32(row["total_account"]);
            int total_fblite = Convert.ToInt32(row["total_fblite"]);
            int total_available = Convert.ToInt32(row["total_available"]);
            int group_device_id = Convert.ToInt32(row["group_device_id"]);
            int internet_id = Convert.ToInt32(row["internet_id"]);
            int data_mode = Convert.ToInt32(row["data_mode"]);
            int expire = Convert.ToInt32(row["is_expire"]);

            var data = new Device()
            {
                Id = id,
                Name = name,
                DeviceID = device_id,
                IsBusy = is_busy,

                TotalAccount = total_account,
                TotalFBLite = total_fblite,
                TotalAvailable = total_available,

                UpdatedAt = updatedAt,
                Description = description,
                Status = status,
                GroupDeviceId = group_device_id,
                InternetId = internet_id,
                DataMode = data_mode,
                Expire = expire
            };

            return data;
        }
    }
}
