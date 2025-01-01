using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolLib.Tool;

namespace ToolLib.Test
{
    public class FbLiteTest
    {
        public static string DEVICE_ID = "22X0219A31008771";
        public static void TestSetup()
        {
            var fbLite = ToolDiConfig.Get<IFacebookLiteTool>();
            fbLite.setup(DEVICE_ID);
        }

        public static void TestListPackage()
        {
            var fbLite = ToolDiConfig.Get<IFacebookLiteTool>();
            var packages = fbLite.apkList(DEVICE_ID);
            Console.WriteLine(" found packages :");
            foreach(var p in packages)
            {
                Console.WriteLine(" -- " + p);
            }
        }
        public static void TestOpenFbLite()
        {
            var fbLite = ToolDiConfig.Get<IFacebookLiteTool>();
            //var packages = fbLite.apkList(DEVICE_ID);
            fbLite.openFb(DEVICE_ID, "com.facebook.lita");
            
        }
        public static void TestLogin()
        {
            var username = "100021690904814";
            var password = "2021@abc@#$";
            var token = "S7IAXBXVUUNAYBTFOU6JIUGTYPCDOU3W";
            var fbLite = ToolDiConfig.Get<IFacebookLiteTool>();
            //fbLite.login(DEVICE_ID, "com.facebook.lita", username, password , token);

        }
        public static void TestJoinGroup()
        {
            var groupId = "389218944461490";
            var fbLite = ToolDiConfig.Get<IFacebookLiteTool>();
            var join = fbLite.joinGroup(DEVICE_ID, "com.facebook.lita", groupId);
            Console.WriteLine("join result : " + join);
        }
        public static void TestLeaveGroup()
        {
            var groupId = "275669916775068";
            var fbLite = ToolDiConfig.Get<IFacebookLiteTool>();
            //var join = fbLite.leaveGroup(DEVICE_ID, "com.facebook.lita", groupId);
            //Console.WriteLine("leave result : "+join);
        }
        public static void TestOpenUlr()
        {
            var url = "https://www.facebook.com/watch/?v=3885518421504945";
            var fbLite = ToolDiConfig.Get<IFacebookLiteTool>();
            fbLite.openUrl(DEVICE_ID, "com.facebook.lith", url);
           
            
        }
        public static void TestShareToGroup()
        {
            var url = "https://www.facebook.com/watch/?v=3885518421504945";
            var fbLite = ToolDiConfig.Get<IFacebookLiteTool>();
            fbLite.shareToGroup(DEVICE_ID, "com.facebook.lita", url, 0);

        }
        public static void TestShare()
        {
            var url = "https://www.facebook.com/watch/?v=3885518421504945";
            var fbLite = ToolDiConfig.Get<IFacebookLiteTool>();
            fbLite.share(DEVICE_ID, "com.facebook.lita", url);

        }
        public static void TestPlayFb()
        {
            var fbLite = ToolDiConfig.Get<IFacebookLiteTool>();
            //var packages = fbLite.apkList(DEVICE_ID);
            fbLite.playFb(DEVICE_ID, "com.facebook.lita");
        }
        public static void TestSelectDevice(int index)
        {
            var fbLite = ToolDiConfig.Get<IFacebookLiteTool>();
            string[] deviceIds = fbLite.listAllDeviceIds();
            if (index < deviceIds.Length)
            {
                DEVICE_ID = deviceIds[index];
            } 
            else
            {
                Console.WriteLine("Can't find device");
            }
        }
        public static void TestPostImage()
        {
            var fbLite = ToolDiConfig.Get<IFacebookLiteTool>();
            fbLite.postImage(DEVICE_ID, "com.facebook.lita");
        }
        public static void TestGoToFriend()
        {
            var fbLite = ToolDiConfig.Get<IFacebookLiteTool>();
            fbLite.goToFriend(DEVICE_ID, "com.facebook.lita");
        }
    

      }
    
}
