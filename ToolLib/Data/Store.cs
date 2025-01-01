using System;
using System.Data;

namespace ToolLib
{
	public class Store 
	{
		public string Id { set; get; }
		
		public string Name { set; get; }
		
		public string Note { set; get; }

        public string CreatedBy { set; get; }
        
        public int State { set; get; }

        public long UpdatedAt { set; get; }

        public static Store from(DataRow dataRow)
        {
            var id = dataRow["id"] + "";
            var name = dataRow["name"] + "";
            var note = dataRow["note"] + "";
            var state = (long)dataRow["state"];
            var createdBy = dataRow["created_by"] + "";
            var updatedAt = (long)dataRow["updated_at"];

            var item = new Store()
            {
                Id = id,
                Name = name,
                Note = note,
                State = Convert.ToInt32(state),
                CreatedBy = createdBy,
                UpdatedAt = updatedAt,
            };

            return item;
        }
    }
}
