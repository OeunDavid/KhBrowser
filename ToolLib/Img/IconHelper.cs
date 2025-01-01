using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolLib.Img
{
    public class IconHelper
    {

        private static Dictionary<string, Image<Bgr, byte>> CACHE = new Dictionary<string, Image<Bgr, byte>>();
        public static Image<Bgr, byte> get(string icon)
        {
            if (!CACHE.ContainsKey(icon))
            {
                var img = new Image<Bgr, byte>(icon);
                CACHE[icon] = img;
            }
            return CACHE[icon];
        }
    }
    public class IconItem
    {
        public string Path { get; set; }
        public string Text { get; set; }
        public IconItem()
        {

        }
        public IconItem(string path, string text)
        {
            this.Path = path;
            this.Text = text;
        }
        public static readonly IconItem FB_LITE_ALLOW_CONTACT = new IconItem(IconName.FB_LITE_ALLOW_CONTACT_ACCESS, "access your contacts");
        public static readonly IconItem FB_LITE_LOGIN_ANOTHER = new IconItem(IconName.FB_LITE_LOGIN_ANOTHER_1, "Log Into Another Account");
        public static readonly IconItem FB_LITE_SELECT_LANGUAGE = new IconItem(IconName.FB_LITE_SELECT_LANGUAGE, "English");
        public static readonly IconItem FB_LITE_HOME_ICON = new IconItem(IconName.FB_LITE_HOME_ICON, "cebook");
        public static readonly IconItem FB_LITE_SAVE_LOGIN_INFO = new IconItem(IconName.FB_LITE_SAVE_LOGIN_INFO, "Save Login Info");
        public static readonly IconItem FB_LITE_UPLOAD_CONTACT = new IconItem(IconName.FB_LITE_UPLOAD_CONTACT, "Facebook Is Better With Friends");
        public static readonly IconItem FB_LITE_ADD_FRIEND_REQ = new IconItem(IconName.FB_LITE_ADD_FRIEND_REQ, "Add Your Friends");
        public static readonly IconItem FB_LITE_CONFIRM_ACCOUNT = new IconItem(IconName.FB_LITE_CONFIRM_ACCOUNT, "Is this your account");
        public static readonly IconItem FB_LITE_CANCEL_GROUP_BUTTON = new IconItem(IconName.FB_LITE_CANCEL_GROUP_BUTTON, "Cancel Request");
        public static readonly IconItem FB_LITE_JOIN_GROUP_ANSWER = new IconItem(IconName.FB_LITE_JOIN_GROUP_ANSWER, "Write your answer");
        public static readonly IconItem FB_LITE_SHARE_TEXTBOX = new IconItem(IconName.FB_LITE_SHARE_TEXTBOX, "What's on your mind");
        public static readonly IconItem FB_LITE_SHARE_TEXTBOX_OTHER = new IconItem(IconName.FB_LITE_SHARE_TEXTBOX_OTHER, "something");

        // dark mode
        public static readonly IconItem FB_LITE_DARK_MODE_NONE_1 = new IconItem(IconName.FB_LITE_DARK_MODE_NONE_1, "Dark Mode");
        public static readonly IconItem FB_LITE_DARK_MODE_NONE_3 = new IconItem(IconName.FB_LITE_DARK_MODE_NONE_3, "Dark Mode");
        public static readonly IconItem FB_LITE_SETTING_2 = new IconItem(IconName.FB_LITE_SETTING_2,"Settings");

        //FB_LITE_SHARE_TEXTBOX_OTHER
        public static readonly IconItem FB_LITE_JOIN_GROUP_BUTTON = new IconItem(IconName.FB_LITE_JOIN_GROUP_BUTTON, "Join");
        public static readonly IconItem FB_LITE_JOIN_GROUP_BUTTON2 = new IconItem(IconName.FB_LITE_JOIN_GROUP_BUTTON2, "Join");
        //FB_LITE_SHARE_BUTTON
    }

    public class DeepIcon
    {
        private List<string> _iconList = new List<string>();
        public List<string>IconList { get
            {
                return _iconList;
            } }
        public DeepIcon(string folder)
        {
            DirectoryInfo d = new DirectoryInfo( folder);
            FileInfo[] files = d.GetFiles("*.png"); //Getting icon
            foreach(var f in files)
            {
                _iconList.Add(f.FullName);
            }
        }
        

    }
}
