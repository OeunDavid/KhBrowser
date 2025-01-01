using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLib.Data
{
    public class ItemList<T>
    {
        private List<T> _items;
        private int index;
        public ItemList(List<T> items)
        {
            _items = items;
        }
        public T Next()
        {
            var item = _items[index];
            index++;
            if( index > _items.Count -1)
            {
                index=0;
            }
            return item;
        }
    }
}
