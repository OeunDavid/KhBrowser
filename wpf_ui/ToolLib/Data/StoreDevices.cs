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
        [JsonProperty("Note")]
        public string Note { get; set; }
        [JsonProperty("State")]
        public int State { get; set; }
        [JsonProperty("TextStatus")]
        public string TextStatus { get; set; }
        [JsonProperty("Temp")]
        public string Temp { get; set; }
        [JsonProperty("IsTemp")]
        public int IsTemp { get; set; }

        public static Store from(DataRow row)
        {
            int id = Int32.Parse(row["id"].ToString());
            string name = row["name"] + "";

            string note = row["note"] + "";
            int state = Convert.ToInt32(row["state"]);
            
            int isTemp = 0;
            try {
                isTemp = Convert.ToInt32(row["is_temp"]);
            } catch { }

            var data = new Store()
            {
                Id = id,
                Name = name,
                Note = note,
                State = state,
                IsTemp = isTemp
            };

            return data;
        }
    }
}
