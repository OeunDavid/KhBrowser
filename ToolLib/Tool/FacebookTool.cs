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

namespace ToolLib.Tool
{
    public interface IFacebookTool
    {
        string captureScreen(string deviceId);
        void close(string deviceId);
        void openFacebook(string deviceId);
        void logout(string deviceId);
        void joinGroup(string deviceId, string groupId);
        void openGroup(string deviceId, string groupId);
        void leaveGroup(string deviceId, string groupId);
        void openUrl(string deviceId, string url);
        void share(string deviceId, string url);
        bool isInHomeScreen(string screenPath);
        Point findAndTap(string deviceId, string icon);
        Point findAndTapV2(string deviceId, string icon);
        Point findAndTapV2(string deviceId, Image<Bgr, byte> screen, Image<Bgr, byte> icon);
        Point findAndTap(string deviceId, Image<Bgr, byte> screen, Image<Bgr, byte> icon);
        bool isAlreadyLogin(string deviceId, string fbId);
        bool login(string deviceId, string id, string password);
         string backup(string deviceId, string fbId);
        bool restore(string deviceId, string fbId);
        IAdbTask AdbTask { get; }
        void closeAllApp(string deviceId);
        void captureScreenCallBack(string deviceId,IFbCaptureScreen fbCaptureScreen);
        void text(string deviceId,string text);
    }
    public interface IFbCaptureScreen
    {
        void captured(string deviceId,string path);
    }
    public class FacebookTool:IFacebookTool
    {
   
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IAdbTask adbTask;
        private IImageHelper imgHelper;
        private Dictionary<string,IFbCaptureScreen> screenObservers = new Dictionary<string,IFbCaptureScreen>();
        private IDeviceInfo deviceInfo;
        public IAdbTask AdbTask
        {
            get { return adbTask; }
        }
      
        public FacebookTool(IAdbTask adbTask, IImageHelper imageHelper,IDeviceInfo deviceInfo)
        {
            this.adbTask = adbTask;
            this.imgHelper = imageHelper;
            this.deviceInfo = deviceInfo;
        }
        public string captureScreen(string deviceId)
        {
           var screenName =  adbTask.captureScreen(deviceId);
            if( screenName != null)
            {
                var screenPath = DeviceInfo.SCREEN_PATH + "/" + screenName;
                if (File.Exists(screenPath))
                {
                    try
                    {
                        Image<Bgr, byte> image = new Image<Bgr, byte>(screenPath);
                        int screenWith = image.Width;
                        DeviceInfo.putScreen(deviceId, screenWith);
                        image = imgHelper.resize(image, width: DeviceInfo.DEVICE_DEFAULT_WIDTH);
                        //  Image<Gray, byte> gray = image.Convert<Gray, byte>();
                        // gray = imgHelper.resize(gray, width: DeviceInfo.DEVICE_DEFAULT_WIDTH);
                        //gray.Save(screenPath);
                        image.Save(screenPath);
                        if (screenObservers.ContainsKey(deviceId) && screenObservers[deviceId] != null)
                        {
                            screenObservers[deviceId].captured(deviceId, screenPath);
                        }
                    }
                    catch { }

                    return screenPath;
                }
            }
          
            return null;
        }
        public void close(string deviceId)
        {
            adbTask.closeFacebook(deviceId);
        }
        //todo check for home icon before exit;
        public void openFacebook( string deviceId)
        {
            adbTask.openFacebook(deviceId);
        }
        public void logout(string deviceId)
        {
            adbTask.logoutFacebook(deviceId);
        }


        public void joinGroup(string deviceId,string groupId)
        {
            //310418125726184
            openGroup( deviceId,groupId);
            Thread.Sleep(5000);
             captureScreen(deviceId);
            var tap = findAndTap(deviceId, IconName.FB_JOIN_GROUP_BUTTON);
            if(!tap.IsEmpty)
            {
                Thread.Sleep(1000);

                log.Info(" join group success");
            }
            else
            {
                log.Info(" could not find join group button");
            }



        }
        public void openGroup(string deviceId,string groupId)
        {
            var url = "https://www.facebook.com/groups/" + groupId;
            adbTask.facebookOpenUrl(deviceId, url);
        }
        public void leaveGroup(string deviceId, string groupId)
        {
            openGroup(deviceId, groupId);
            Thread.Sleep(5000);
            var screenPath = captureScreen(deviceId);
            var tap = findAndTap(deviceId, IconName.FB_GROUP_SETTING_BUTTON);
            if(!tap.IsEmpty)
            {
                log.Info("click on group setting successfully");
                Thread.Sleep(5000);
                screenPath = captureScreen(deviceId);
                var tapLeave = findAndTap(deviceId, IconName.FB_GROUP_LEAVE_BUTON);
                if( !tapLeave.IsEmpty)
                {
                    Thread.Sleep(5000);
                    screenPath = captureScreen(deviceId);
                    var finalTap = findAndTap(deviceId, IconName.FB_GROUP_LEAVE_FINAL_BUTON);
                    if(!finalTap.IsEmpty)
                    {
                        log.Info("final leave the group");
                    }
                    else
                    {
                        log.Info(" could not leave the group");
                    }
                }
                else
                {
                    log.Info(" could not find button leave group");
                }
            }
            else
            {
                log.Info(" could not find group setting");
            }
        }
        public void openUrl(string deviceId, string url)
        {
            adbTask.facebookOpenUrl(deviceId, url);
        }

        public void share(string deviceId, string url)
        {
            var screenPath = captureScreen(deviceId);
            if( screenPath !=null && File.Exists(screenPath))
            {
                var tap = findAndTap(deviceId, IconName.FB_HOME_POST_INPUT);
                
                if (!tap.IsEmpty)
                {
                    log.Info("found text input for post" + tap.ToString());
                    screenPath = captureScreen(deviceId);
                    if( screenPath == null)
                    {
                        log.Info("could not capture screen");
                        return;
                    }
                    var textTap = findAndTap(deviceId, IconName.FB_POST_INPUT_TEXT);
                    if(!textTap.IsEmpty)
                    {
                        adbTask.text(deviceId, url);
                        screenPath = captureScreen(deviceId);
                        adbTask.key(deviceId, AndroidKey.KEYCODE_ENTER);
                        Thread.Sleep(100);
                        if( screenPath == null)
                        {
                            log.Info("could not capture screen");
                            return;
                        }
                        var postTap = findAndTap(deviceId, IconName.FB_POST_BUTTON);
                        if(!postTap.IsEmpty)
                        {
                            log.Info("done posting url");
                        }
                        else
                        {
                            log.Info(" could not post url");
                        }
                    }
                }
                else
                {
                    log.Info(" not found home screen input");
                }
            }
            
        }
        public bool isInHomeScreen(string screenPath)
        {
            var img = new Image<Bgr, byte>(screenPath);
            var icon = new Image<Bgr, byte>(IconName.FB_HOME_ICON);
            return isInHomeScreen(img, icon);
        }
        public bool isInHomeScreen(Image<Bgr, byte>screen, Image<Bgr, byte> homeIcon)
        {
            var search = imgHelper.match(homeIcon, screen);
            return !search.IsEmpty;
        }

        public Point findAndTap(string deviceId, string icon)
        {
            var screenPath = DeviceInfo.SCREEN_PATH + "/" + deviceId + ".png";
            if( File.Exists(screenPath))
            {
                var screen = imgHelper.img(screenPath);
                return findAndTap(deviceId, screen, imgHelper.img(icon));
            }
            return new Point();
        }

        public Point findAndTap(string deviceId, Image<Bgr, byte>screen, Image<Bgr, byte> icon)
        {
            var search = imgHelper.match(icon, screen);
            if( !search.IsEmpty)
            {
                var point = imgHelper.midPoint(deviceId, search);
             
                adbTask.tap(deviceId, point);
                return point;
            }
            return new Point();
        }
        public void tap(string deviceId,Image<Bgr, byte> screen,Rectangle rectangle)
        {
            var point = imgHelper.midPoint(deviceId, rectangle);
            adbTask.tap(deviceId, point);
        }


        public bool isAlreadyLogin(string deviceId, string fbId)
        {
            var dir = DeviceInfo.SCREEN_PATH + "/backup";
            if( !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var path = DeviceInfo.SCREEN_PATH + "/backup/" + deviceId + "/" + fbId + ".backup";
            if(File.Exists(path))
            {
                var fileInfo = new FileInfo(path);
                if( fileInfo.Length > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool login(string deviceId, string id, string password)
        {
            
            adbTask.openFacebook(deviceId);
            Thread.Sleep(5000);
            log.Info("open facebook now wait for a while");
            var phoneInputImg = IconHelper.get(IconName.FB_LOGIN_PHONE_INPUT);
            var loginHeaderImg = IconHelper.get(IconName.FB_LOGIN_HEADER);
            string screenPah = captureScreen(deviceId);
            int counter = 100;
            var emailSearch = new Rectangle();
            while (counter-- >0)
            {
                Thread.Sleep(2000);
                screenPah = captureScreen(deviceId);
                var screen = imgHelper.img(screenPah);
                var loginHeaderSearch = imgHelper.match(loginHeaderImg, screen);
                if(!loginHeaderSearch.IsEmpty)
                {
                    break;
                }

            }

            Thread.Sleep(1000);
            screenPah = captureScreen(deviceId);
            log.Info("capture screen done.");
            if (File.Exists(screenPah))
            {

                var screen = imgHelper.img(screenPah);
               
                log.Info("save as gray " + screenPah + " done");
                 emailSearch = imgHelper.match( phoneInputImg, screen);
                var homeIcon = IconHelper.get(IconName.FB_HOME_ICON);
                if (!emailSearch.IsEmpty)
                {
                    log.Info("found email input");
                    Point point = imgHelper.midPoint(deviceId, emailSearch);
                    adbTask.tap(deviceId, point);
                    Thread.Sleep(300);
                    //adbTask.text(deviceId, id);
                    text(deviceId, id);
                    Thread.Sleep(200);
                    adbTask.key(deviceId, AndroidKey.KEYCODE_TAB);
                    text(deviceId, password);
                    Thread.Sleep(100);
                    adbTask.key(deviceId, AndroidKey.KEYCODE_ENTER);
                    Thread.Sleep(2000);
                    log.Info(" check if it's in home screen");
                    counter = 20;
                    var iconSaveLoginInfo = IconHelper.get(IconName.FB_LOGIN_SAVE_INFO);
                    while (counter -- > 0)
                    {
                        Thread.Sleep(2000);
                        screenPah = captureScreen(deviceId);
                        screen = imgHelper.img(screenPah);
                        var notSaveInfoSearch = imgHelper.match(iconSaveLoginInfo, screen);
                        if(notSaveInfoSearch.IsEmpty)
                        {
                            log.Info("not found save info button");
                            if(isInHomeScreen(screen, homeIcon))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            log.Info("found and tab on not save info");
                            screenPah= captureScreen(deviceId);
                            findAndTap(deviceId, IconName.FB_NOT_SAVE_LOGIN_INFO);
                            Thread.Sleep(3000);
                            break;
                        }
                    }
                    screenPah = captureScreen(deviceId);
                    screen = imgHelper.img(screenPah);
                    return isInHomeScreen(screen, homeIcon);
                }
                else
                {
                    log.Info("not found email");
                    return false;
                }

            }
            else
            {
                log.Info(" not found screen during capture : " + deviceId);
            }
            return false;
        }

        public string backup(string deviceId, string fbId)
        {
            
            Thread thread = new Thread(() => {
                Thread.Sleep(5000);
                var counter = 10;
                while(counter -- > 0)
                {
                    //log.Info("capture screen for backup ");
                    //var screenPath = captureScreen(deviceId);
                    //if (screenPath != null)
                    //{
                    //    var point = findAndTap(deviceId, IconName.DEVICE_CONFIRM_BACKUP_BUTTON);
                    //    if(!point.IsEmpty)
                    //    {
                    //        log.Info("success backup "+deviceId+":"+fbId);
                    //        break;
                    //    }
                    //}
                    adbTask.key(deviceId, AndroidKey.KEYCODE_TAB);
                    Thread.Sleep(100);
                    adbTask.key(deviceId, AndroidKey.KEYCODE_TAB);
                    Thread.Sleep(100);
                    adbTask.key(deviceId, AndroidKey.KEYCODE_ENTER);
                    Thread.Sleep(1000);
                    break;
                }
               
            }
         );
            thread.Start();

            var backup = adbTask.backup(deviceId, fbId);
            log.Info("begin backup for " + deviceId + " - " + fbId);
            thread.Join();
            log.Info("done backup for "+deviceId+"-" +fbId);
            return backup+"";
        }
        public bool restore(string deviceId , string fbId) 
        {
            adbTask.closeFacebook(deviceId);
           
            Thread thread = new Thread(() =>
            {
                var counter = 10;
                Thread.Sleep(2000);
                while (counter-- > 0)
                {
               
                    //var screenPath = captureScreen(deviceId);
                    //if (screenPath != null)
                    //{
                    //    var point = findAndTap(deviceId, IconName.DEVICE_CONFIRM_RESTORE_BUTTON);
                    //    if (!point.IsEmpty)
                    //    {
                    //        log.Info("success restore " + deviceId + ":" + fbId);
                    //        break;
                    //    }
                    //}
                    adbTask.key(deviceId, AndroidKey.KEYCODE_TAB);
                    Thread.Sleep(100);
                    adbTask.key(deviceId, AndroidKey.KEYCODE_TAB);
                    Thread.Sleep(100);
                    adbTask.key(deviceId, AndroidKey.KEYCODE_ENTER);
                    Thread.Sleep(200);
                    break;


                }
            }
            );
            thread.Start();
            log.Info("begin restore for " + deviceId + " -> " + fbId + "");
            var restore = adbTask.restore(deviceId, fbId);
            log.Info("done restore for " + deviceId + " -> " + fbId);
            thread.Join();
            return true;
        }

        public void closeAllApp(string deviceId)
        {
            var screenPath = captureScreen(deviceId);
            var img = imgHelper.img(screenPath);
            var height = img.Height;
            var rect = new Rectangle(0, 0, DeviceInfo.DEVICE_DEFAULT_WIDTH, height);
            var point = imgHelper.midPoint(deviceId, rect);
            adbTask.closeAllApp(deviceId, point);
            Thread.Sleep(500);
        }

        public void captureScreenCallBack(string deviceId,IFbCaptureScreen fbCaptureScreen)
        {
            screenObservers[deviceId] = fbCaptureScreen;
        }

        public void text(string deviceId,string text)
        {
            if(deviceInfo.isKeyForText(deviceId))
            {
                adbTask.keyForText(deviceId, text);
            }
            else
            {
                adbTask.text(deviceId, text);
            }
        }

        public Point findAndTapV2(string deviceId, string icon)
        {
            var screenPath = DeviceInfo.SCREEN_PATH + "/" + deviceId + ".png";
            if (File.Exists(screenPath))
            {
                var screen = imgHelper.img(screenPath);
                return findAndTapV2(deviceId, screen, imgHelper.img(icon));
            }
            return new Point();
        }

        public Point findAndTapV2(string deviceId, Image<Bgr, byte> screen, Image<Bgr, byte> icon)
        {
            var search = imgHelper.anotherMatch(icon, screen);
            if (!search.IsEmpty)
            {
                var point = imgHelper.midPoint(deviceId, search);

                adbTask.tap(deviceId, point);
                return point;
            }
            return new Point();
        }
    }
}
