using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLib.Data
{
    public class Store
    {
        [JsonProperty("Id")]
        public int Id { get; set; }
        [JsonProperty("Key")]
        public int Key { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("Description")]
        public string Description { get; set; }
        [JsonProperty("TextStatus")]
        public string TextStatus { get; set; }
        [JsonProperty("Status")]
        public int Status { get; set; }
        [JsonProperty("Temp")]
        public string Temp { get; set; }
        [JsonProperty("IsTemp")]
        public int IsTemp { get; set; }

        public static Store from(DataRow row)
        {
            int id = Int32.Parse(row["id"].ToString());
            string name = row["name"] + "";

            string description = row["description"] + "";
            int status = Convert.ToInt32(row["status"]);
            int isTemp = Convert.ToInt32(row["is_temp"]);

            var data = new Store()
            {
                Id = id,
                Name = name,
                Description = description,
                Status = status,
                IsTemp = isTemp
            };

            return data;
        }
    }
}
