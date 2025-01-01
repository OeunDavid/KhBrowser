using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsQuery;
namespace ToolLib.Tool
{
    public interface IViewParser
    {
        CQ fromFile(string path);
         CQ createDom(string xml);
        CQ findAllChildrenByClass(CQ dom, params string[] clNames);
        Rectangle viewBound(CQ view);
         Rectangle viewBound(IDomObject dom);
        Point midPonit(Rectangle rect);
        CQ findBy(CQ dom, string key, string value);
        CQ childrenBy(CQ dom, string key, string value);
        CQ findByClass(CQ dom, string className);


    }
    public class ViewParser:IViewParser
    {
        public CQ fromFile(string path)
        {
            var content = File.ReadAllText(path);
            return createDom(content);
        }
        public CQ createDom(string xml)
        {
            return CQ.Create(xml);
        }
        public Point midPonit(Rectangle rect)
        {
            var x = (rect.Left + rect.Right) / 2;
            var y = (rect.Top + rect.Bottom) / 2;
            return new Point(x, y);
        }
        public Rectangle viewBound(IDomObject dom)
        {
           var att =  dom.GetAttribute("bounds");
            return fromAtt(att);
        }
        private Rectangle fromAtt(string att)
        {
            if (!string.IsNullOrEmpty(att))
            {
                att = att.TrimEnd(']');
                att = att.TrimStart('[');

                string[] binds = att.Split(new string[] { "][" }, StringSplitOptions.RemoveEmptyEntries);
                var first = binds[0].Split(',');
                var second = binds[1].Split(',');
                var startX = Convert.ToInt32(first[0]);
                var startY = Convert.ToInt32(first[1]);
                var endX = Convert.ToInt32(second[0]);
                var endY = Convert.ToInt32(second[1]);
                return new Rectangle(startX, startY, (endX - startX), (endY - startY));

            }

            return new Rectangle();
        }
        public Rectangle viewBound(CQ view)
        {
            try
            {
                if (view != null)
                {
                    var att = view.Attr("bounds");

                    return fromAtt(att);
                }
            }
            catch(NullReferenceException ex) { }

            return new Rectangle();
        }
        public CQ findAllChildrenByClass(CQ dom, params string [] clNames)
        {
            CQ res = dom;
            foreach(var cs in clNames)
            {
                var selector = "[class=" + cs +"]";
                var tmp = res.Children(selector);
                if( tmp.ToList().Count == 0)
                {
                    return null;
                }
                res = tmp.First();
            }

            return res;
        }
        public CQ findBy(CQ dom ,string key ,string value)
        {
            var selector = "["+key+"=" + value + "]";
            return dom[selector];
        }
        public CQ childrenBy(CQ dom , string key , string value)
        {
            var selector = "[" + key + "=" + value + "]";
            return dom.Children(selector);
        }

        public CQ findByClass(CQ dom, string className)
        {
            var selector = "[class=" + className + "]";
            return dom[selector];
        }
    }
}
