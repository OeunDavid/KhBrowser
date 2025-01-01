using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ToolLib.Data;
using ToolLib.Test;
using ToolLib.Tool;

namespace ToolLib
{
    class Program
    {
        static void testLoginAndLogout()    
        {
            FacebookTest.testLogout();
            Thread.Sleep(2000);
            FacebookTest.testOpen();
            Thread.Sleep(10000);
            FacebookTest.testClose();
            Thread.Sleep(5000);
            FacebookTest.testLogin();
            Thread.Sleep(2000);
            FacebookTest.testIsHome();
        }
        public static void testOpenAndShare()
        {
            FacebookTest.testOpen();
            Thread.Sleep(1000);
            FacebookTest.testShare();
        }
        static void Main(string[] args)
        {
            AdbTask adb = new AdbTask();
            //var result = adb.captureScreen("22X0219A31008771");
            ImageHelper imgHelper = new ImageHelper();
            // testLoginAndLogout();
            //imgHelper.
            //Scale(@"D:\RD\android\screen\22X0219A31008771.png", @"D:\RD\android\screen\22X0219A31008771_gray.png");
            //imgHelper.match(@"D:\RD\android\screen\play_icon.png", @"D:\RD\android\screen\22X0219A31008771_gray.png");
            //adb.openFacebook("8T89626AA1971900021");
            // adb.logoutFacebook("8T89626AA1971900021");
            //Console.Write("device list :\n" + adb.deviceList());

            //adb.captureScreen("R58M464RBRW");
            //adb.text("8T89626AA1971900021", "hello");
            //adb.facebookWatch("22X0219A31008771");
            //DeviceInfoTest.testDeviceInfo();
            //   FacebookTest.testLogin();
            //ImageHelperTest.testCmp();
            //FacebookTest.testCapture();
            //FacebookTest.testJoinGroup();
            //FacebookTest.testLeaveGroup();
            //AccountDaoTest.test();
            ///ActivityLogDaoTest.test();
            ///
            //FacebookTest.TestBackup();
            // FacebookTest.TestRestore();
            //var accountDao = ToolDiConfig.Get<IAccountDao>();
            //var adbCmd = ToolDiConfig.Get<IAdbTask>();
            //var fbTool = ToolDiConfig.Get<IFacebookTool>();
            //Console.WriteLine("is same ? " + (  fbTool.AdbTask == adb));
            ////FacebookTest.TestIFContain();
            //  ImageHelperTest.TestFeatureMatch();
            //ImageHelperTest.testOcr();
            //  FbLiteTest.TestSelectDevice(0);
            //  FbLiteTest.TestSetup();
            //FbLiteTest.TestListPackage();
            //  FbLiteTest.TestOpenFbLite();
            // FbLiteTest.TestShareToGroup();
            //ImageHelperTest.TestDeepMatchList();
            //var fbLite = ToolDiConfig.Get<IFacebookLiteTool>();
            //  fbTool.captureScreen("22X0219A31008771");
            // FbLiteTest.TestOpenFbLite();
            //FbLiteTest.TestShareToGroup();
            //  FbLiteTest.TestLogin();
            //   TestHttp.TestGet();
            //  FbLiteTest.TestListPackage();
            // ImageHelperTest.TestFeatureMatch();
            // FbLiteTest.TestLogin();
            //  TestViewParser.TestAllChildren();
            //TestViewParser.TestFile();
            // FbLiteTest.TestPostImage();

            //adb.addProxy("ce021602712c210801", "45.95.99.20:7580:ozsomazq-dest:naxmjx87buj9");
            //adb.addProxy("", "5.154.253.4:8262:wuppnapg-dest:hn8g5u9is9qv");
            //adb.captureScreen("0634925149000202");

            string deviceId = "0634925149000202";

            //FbLiteTest.TestGoToFriend();
            Console.ReadLine();
        }
    }
}
