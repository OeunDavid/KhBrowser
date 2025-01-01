using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolKHBrowser.ToolLib.Data
{
    public class Groups
    {
        [JsonProperty("Id")]
        public int Id { get; set; }
        [JsonProperty("UID")]
        public string UID { get; set; }
        [JsonProperty("PageId")]
        public string PageId { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("GroupId")]
        public string GroupId { get; set; }
        [JsonProperty("Status")]
        public int Status { get; set; }
        [JsonProperty("Pending")]
        public int Pending { get; set; }
        [JsonProperty("Check_Pending")]
        public int Check_Pending { get; set; }
        [JsonProperty("Member")]
        public int Member { get; set; }

        public static Groups from(DataRow row)
        {
            int id = Int32.Parse(row["id"].ToString());
            string uid = row["uid"] + "";
            string name = row["name"] + "";

            string pageId = row["page_id"] + "";
            string groupId = row["group_id"] + "";
            int status = Convert.ToInt32(row["status"]);
            int pending = Convert.ToInt32(row["pending"]);
            int check_pending = Convert.ToInt32(row["check_pending"]);
            int member = Convert.ToInt32(row["member"]);

            var data = new Groups()
            {
                Id = id,
                UID = uid,
                Name = name,

                PageId = pageId,
                Status = status,
                GroupId = groupId,

                Pending = pending,
                Check_Pending = check_pending,
                Member = member,
            };

            return data;
        }
    }
}
