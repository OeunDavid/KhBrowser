using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolLib.Tool;

namespace ToolLib.Test
{
    public class TestViewParser
    {
        public static void TestAllChildren()
        {
            var content = File.ReadAllText(DeviceInfo.SCREEN_PATH + "/view_login.xml");
            var viewParser = ToolDiConfig.Get<IViewParser>();
            var view = viewParser.createDom(content);
            var childrenForm = viewParser.findAllChildrenByClass(view, "android.widget.FrameLayout", "android.widget.LinearLayout", "android.widget.FrameLayout", "android.widget.FrameLayout", "android.view.ViewGroup");
            Console.WriteLine("children : " + childrenForm.ToList().Count);
            var textBoxes = viewParser.findAllChildrenByClass(childrenForm, "android.widget.MultiAutoCompleteTextView");
            if( textBoxes.Count() > 0)
            {
                var nameRect = viewParser.viewBound(textBoxes.Last());
                Console.WriteLine("username : " + nameRect);
                textBoxes.First().Parent();
            }

        }
        public static void TestFile()
        {
            var path = @"D:\RD\android\apktool\view_camera_access.xml";
            var viewParser = ToolDiConfig.Get<IViewParser>();
            var view = viewParser.fromFile(path);
            var item = viewParser.findBy(view, "text", "Gallery");
            Console.WriteLine("item : " + item +" , count : " + item.Count());
            var viewGallery = viewParser.fromFile(@"D:\RD\android\apktool\view_gallery.xml");
            var item2 = viewParser.findBy(viewGallery, "text", "Gallery");
            Console.WriteLine("item2 : " + item2 + " , count : " + item2.Count());

        }

    }
}
