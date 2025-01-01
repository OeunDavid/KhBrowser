using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolKHBrowser.ToolLib.Data
{
    public class Pages
    {
        [JsonProperty("Id")]
        public int Id { get; set; }
        [JsonProperty("PageId")]
        public string PageId { get; set; }
        [JsonProperty("UID")]
        public string UID { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("AccessToken")]
        public string AccessToken { get; set; }
        [JsonProperty("Status")]
        public int Status { get; set; }

        public static Pages from(DataRow row)
        {
            int id = Int32.Parse(row["id"].ToString());
            string uid = row["uid"] + "";
            string name = row["name"] + "";
            string pageId = row["page_id"] + "";
            string accessToken = row["access_token"] + "";
            int status = Convert.ToInt32(row["status"]);

            var data = new Pages()
            {
                Id = id,
                PageId = pageId,
                Name = name,
                UID = uid,

                Status = status,
                AccessToken = accessToken
            };

            return data;
        }
    }
}
