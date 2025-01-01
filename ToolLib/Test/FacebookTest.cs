using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolLib.Cmd;
using ToolLib.Img;
using ToolLib.Tool;

namespace ToolLib.Test
{
    public class FacebookTest
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static readonly string DEVICE_ID = "06157df6a3ef6e09";
        public static void testLogout()
        {
            AdbTask adbTask = new AdbTask();
            //R58M464RBRW
            //22X0219A31008771
            //4d00bbc74dcf31dd
            adbTask.logoutFacebook(DEVICE_ID);//work
        }
        public static void testCapture()
        {
            var facebookTool = ToolDiConfig.Get<IFacebookTool>();
            //R58M464RBRW
            facebookTool.captureScreen( DEVICE_ID);
            //22X0219A31008771
        }
        private static void sendPassword()
        {

        }
        public static void testOpen()
        {
            var facebookTool = ToolDiConfig.Get<IFacebookTool>();
            facebookTool.openFacebook("4d00bbc74dcf31dd");
            Thread.Sleep(1000);
        }
        public static void testClose()
        {
            var facebookTool = ToolDiConfig.Get<IFacebookTool>();
            facebookTool.close("4d00bbc74dcf31dd");
            Thread.Sleep(1000);
        }
        public static void testLogin()
        {
            string accountId = "kh.napoli.94\n";
            string password = "iq123456789IQ@@";
            string deviceID = "4d00bbc74dcf31dd";
            var facebookTool = ToolDiConfig.Get<IFacebookTool>();
            var loginResult = facebookTool.login(deviceID, accountId, password);
            log.Info("login success : " + loginResult);


        }
        public static void testShare()
        {
            
            string deviceID = "R58M464RBRW";
            var url = "https://web.facebook.com/charmingstor/videos/3885518421504945/";
            var facebookTool = ToolDiConfig.Get<IFacebookTool>();
            facebookTool.share(deviceID, url);
           
        }

        public static void testIsHome()
        {
            var facebookTool = ToolDiConfig.Get<IFacebookTool>();
            string screenPath = facebookTool.captureScreen("4d00bbc74dcf31dd");
            Thread.Sleep(100);
            log.Info("is home " + facebookTool.isInHomeScreen(screenPath));
        }
        public static void testJoinGroup()
        {
            /*
             * 310418125726184
   287016505728711
   838684949914846
   156937971068279
   858793497801163
   370106077214598
   370106077214598
   681907285241770
   328419393908306
   622903044436001
   613871312079255
   213038135777066
   235252637911297
   313691799120573
   774532819254839
   2664036793868415
   889413111175912
   501720319877657
   679201342111929
   278448273420016
   769757879726636
   291941841161011
   389218944461490
   918343284843368
   330756704194737
   732322970962483
   */
            var groupId = "769757879726636";
            var facebookTool = ToolDiConfig.Get<IFacebookTool>();
            facebookTool.joinGroup("R58M464RBRW", groupId);

        }
        public static void testLeaveGroup()
        {
            var groupId = "560267923995056";
            var facebookTool = ToolDiConfig.Get<IFacebookTool>();
            facebookTool.leaveGroup( "22X0219A31008771", groupId);

        }
        public static void TestBackup()
        {
            var facebookTool = ToolDiConfig.Get<IFacebookTool>();
            var accountID = "kh.napoli.94";
            facebookTool.backup(DEVICE_ID, accountID);
        }


        public static void TestRestore()
        {
            var facebookTool = ToolDiConfig.Get<IFacebookTool>();
            var accountID = "kh.napoli.94";
            facebookTool.restore(DEVICE_ID, accountID);
        }
        public static void TestIFContain()
        {
            var facebookTool = ToolDiConfig.Get<IFacebookTool>();
            var imgHelper = ToolDiConfig.Get<IImageHelper>();
            var icon = IconHelper.get(IconName.FB_LOGIN_PHONE_INPUT);
            var screen = imgHelper.img(DeviceInfo.SCREEN_PATH + "/" + DEVICE_ID + ".png");
            var search = imgHelper.match(icon, screen);
            log.Info("Search found " + (!search.IsEmpty) + " " + search);
            if(!search.IsEmpty)
            {
                int thickness = 2;
                CvInvoke.Rectangle(screen, search, new Bgr(Color.Red).MCvScalar, thickness);
                screen.Save(DeviceInfo.SCREEN_PATH + "/" + DEVICE_ID + "_new.png");
            }
           

        }

    }
}
