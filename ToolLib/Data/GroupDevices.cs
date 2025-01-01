using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLib.Data
{
    public class GroupDevices
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }

        public static GroupDevices from(DataRow row)
        {
            int id = Int32.Parse(row["id"].ToString());
            string name = row["name"] + "";

            string description = row["description"] + "";
            int status = Convert.ToInt32(row["status"]);

            var data = new GroupDevices()
            {
                Id = id,
                Name = name,
                Description = description,
                Status = status
            };

            return data;
        }
    }
}
