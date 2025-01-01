using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ToolLib.Cmd;

namespace ToolLib
{
    public interface IAdbTask
    {
        string deviceList();
        string[] listAllThirdPartyPackages(string deviceId);
        object captureScreen(string deviceId);
        object tap(string deviceId, Point point);
        object tap(string deviceId, string x, string y);
        object text(string deviceId, string text);
        object scrollDown(string deviceId);
        object scrollUp(string deviceId);
        object swipe(string deviceId, string x1, string y1, string x2, string y2, string speed="300");
        object scrollDown(string deviceId, string x1, string y1, string x2, string y2);
        object key(string deviceId, int keyCode);
         object openFacebook(string deviceId);
        object closeFacebook(string deviceId);
        object back(string deviceId);
        object removeFB(string deviceId, string packageID);
        object viewProfile(string deviceId, string userId);
        object logoutFacebook(string deviceId);
        object facebookFeed(string deviceId);
        object facebookWatch(string deviceId);
        object facebookOpenUrl(string deviceId, string url);
        object airPlanModeOn(string deviceId);
        object airPlanModeOff(string deviceId);
        object backup(string deviceId, string fbId);
        object restore(string deviceId, string fbId);
        object closeAllApp(string deviceId, Point p);
        object setup(string deviceId, string src, string apk);
        string listPackage(string deviceId);
        object openFbLite(string deviceId,string package);
        object openUrl(string deviceId, string package, string url);
        void keyForText(string deviceId, string text);
        void closeApp(string deviceId, string packageName);
        object captureView(string deviceId);
        object grantPermision(string deviceId, string packageName, string permisison);
        string[] deviceIDs();
        object reboot(string deviceId);
        object goToHome(string deviceId, string packageName);
        string deviceModel(string deviceId);
        void inputSelectAll(string deviceId);
    }

    public class AdbTask:IAdbTask
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static HashSet<string> CLOSE_BY_SWIPE = new HashSet<string>() { "HWVOG","TECNO-LD7", "CPH1877" };
        private static HashSet<string> CLOSE_BY_FORCE = new HashSet<string>() { };

        private IAdbCommand adb;
        public AdbTask()
        {
            adb = new AdbCommand();
        }
        public AdbTask(IAdbCommand adb)
        {
            this.adb = adb;
        }
        public void inputSelectAll(string deviceId)
        {
            string[] input = { "-s", deviceId, "shell", "settings", "delete", "global", "http_proxy" };
            adb.execute(input);
        }
        public object goToHome(string deviceId, string packageName)
        {
            return openUrl(deviceId, packageName, "https://www.facebook.com/groups/feed/");
        }
        public void addProxy(string deviceId, string proxys)
        {
            string[] proxyArr = proxys.Split(':');
            string ip = "", port = "", username = "", password = "";
            try
            {
                ip = proxyArr[0];
            }
            catch { }
            try
            {
                port = proxyArr[1];
            }
            catch { }
            try
            {
                username = proxyArr[2];
            }
            catch { }
            try
            {
                password = proxyArr[3];
            }
            catch { }
            string[] input = { deviceId, "shell", "settings", "put", "global", "http_proxy", ip+":"+port };
            adb.execute(input);
            //string[] input1 = { deviceId, "shell", "settings", "put", "global", "global_http_proxy_host", ip };
            //adb.execute(input1);
            //string[] input2 = { deviceId, "shell", "settings", "put", "global", "global_http_proxy_port", port };
            //adb.execute(input2);
            string[] input3 = { deviceId, "shell", "settings", "put", "global", "global_http_proxy_username", username };
            adb.execute(input3);
            string[] input4 = { deviceId, "shell", "settings", "put", "global", "global_http_proxy_password", password };
            adb.execute(input4);
        }
        public void removeProxy(string deviceId)
        {
            string[] input = { "-s", deviceId, "shell", "settings", "delete", "global", "http_proxy" };
            adb.execute(input);
            string[] input1 = { "-s", deviceId, "shell", "settings", "delete", "global", "global_http_proxy_host" };
            adb.execute(input1);
            string[] input2 = { "-s", deviceId, "shell", "settings", "delete", "global", "global_http_proxy_port" };
            adb.execute(input2);
            string[] input3 = { "-s", deviceId, "shell", "settings", "delete", "global", "global_http_proxy_username" };
            adb.execute(input3);
            string[] input4 = { "-s", deviceId, "shell", "settings", "delete", "global", "global_http_proxy_password" };
            adb.execute(input4);
            string[] input5 = { "-s", deviceId, "shell", "settings", "delete", "global", "global_http_proxy_exclusion_list" };
            adb.execute(input5);
            string[] input6 = { "-s", deviceId, "shell", "settings", "delete", "global", "global_proxy_pac_url" };
            adb.execute(input6);
        }
        
        public string deviceList()
        {
            String[] input = { "devices" };
            var result = adb.execute(input);
            if( result == null)
            {
                return "";
            }
            string pattern = "List of devices attached";
            string[] search = result.StandardOutput.Split(new string[] { pattern },StringSplitOptions.RemoveEmptyEntries);
            string content = search[search.Length - 1];
            return content;
        }

        public string[] deviceIDs()
        {
            String[] input = { "devices" };
            var result = adb.execute(input);
            if (result == null)
            {
                return null;
            }

            string title = "List of devices attached\r\n";
            string output = result.StandardOutput.Substring(title.Length);
            output = output.Replace(Environment.NewLine, "");
            
            string[] separatingStrings = { "\tdevice" };

            return  output.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);
         
        }

        public object captureScreen(string deviceId)
        {
            var screenName = deviceId + ".png";
            //String[] input = {  "-s", deviceId, "exec-out", "screencap -p" };
            String[] input = { "-s", deviceId, "exec-out", "screencap -p", " /sdcard/"+ screenName };
            var result = adb.execute(input);
            String[] pull = { "-s" , deviceId , "pull", "/sdcard/"+screenName };
            //byte[] outPut = Encoding.ASCII.GetBytes(result.StandardOutput);
            var pullResult = adb.execute(pull);
            if( pullResult != null)
            {
                return screenName;
            }
            return null;
            //todo convert to gray scale first
            //var gray = new Image<>
            
          
        }
        public object reboot(string deviceId)
        {
            string[] input = { "-s", deviceId, "reboot" };
            return adb.execute(input);
        }
        public object back(string deviceId)
        {
            string[] input = { "-s", deviceId, "shell", "input", "keyevent 4" };
            return adb.execute(input);
        }
        public object tap( string deviceId,Point point)
        {
            string[] input = {"-s", deviceId, "shell", "input", "tap", point.X+"",point.Y+"" };
            return adb.execute(input);
        }
        public object tap(string deviceId, string x, string y)
        {
            string[] input = { "-s", deviceId, "shell", "input", "tap", x, y};
            return adb.execute(input);
        }
        public object text(string deviceId,string text)
        {
            log.Info(" begin to send text cmd -> " + deviceId + " " + text);
            string[] input = {  "-s", deviceId, "shell", "input", "text", "'" + text + "'" };
            return adb.execute(input);
        }
        public object scrollDown(string deviceId)
        {
            string[] input = {  "-s", deviceId, "shell", "input", "swipe", "250", "400", "250", "1000","300" };
            return adb.execute(input);
        }
        public object scrollDown(string deviceId, string x1, string y1, string x2, string y2)
        {
            string[] input = { "-s", deviceId, "shell", "input", "swipe", x1, y1, x2, y2, "300" };
            return adb.execute(input);
        }
        public object key(string deviceId, int keyCode)
        {
            log.Info(" begin to send key cmd -> " + deviceId + " " + keyCode);
            string[] input = { "-s", deviceId, "shell", "input", "keyevent", keyCode + "" };
            return adb.execute(input);
        }
        public object removeFB(string deviceId, string packageID)
        {
            string[] input = { "-s", deviceId, "uninstall", packageID };

            return adb.execute(input);
        }
        public object openFacebook(string deviceId)
        {
            // string[] input = { "-s", deviceId, "shell", "am start -a android.intent.action.VIEW -d facebook://facebook.com/newsfeed" };
            //open thorugh intenet need to have activity class and the most well known is LoginActivity
            string[] input = { "-s", deviceId, "shell", "am start -n com.facebook.katana/com.facebook.katana.LoginActivity" };
            return adb.execute(input);
        }

        public object closeFacebook( string deviceId)
        {
            string[] input = { "-s", deviceId, "shell am force-stop", "com.facebook.katana" };
            return adb.execute(input);
        }

        public object viewProfile( string deviceId, string userId)
        {
            string[] input = { "-s", deviceId, "shell","am start -a android.intent.action.VIEW -d", "facebook://facebook.com/info?user="+userId};
            return adb.execute(input);
        }

        public object logoutFacebook(string deviceId)
        {
            string[] input = { "-s", deviceId, "shell" ,"pm", "clear", "com.facebook.katana" };
            return adb.execute(input); 
        }


        public object facebookFeed(string deviceId)
        {
            string[] input = { "-s", deviceId, "am start -a android.intent.action.VIEW -d", "fb://feed" };
            return adb.execute(input);
        }
        public object facebookWatch(string deviceId)
        {
            //https://www.facebook.com/watch/?v=3885518421504945
            string[] input = { "-s", deviceId, "shell", "am start -a android.intent.action.VIEW -d https://www.facebook.com/watch/?v=3885518421504945", " com.facebook.katana" };
            return adb.execute(input);
        }
        public object facebookOpenUrl(string deviceId , string url)
        {
            string[] input = { "-s", deviceId, "shell", "am start -a android.intent.action.VIEW -d", url, " com.facebook.katana" };
            return adb.execute(input);
        }
        public object airPlanModeOn( string deviceId)
        {
            string[] input = {"-s", deviceId, "shell", "settings", "put", "global", "airplane_mode_on", "1"};
            var result = adb.execute(input);
            string[] input2 = {"-s", deviceId, "shell", "am", "broadcast", "-a", "android.intent.action.AIRPLANE_MODE"};
            var result2 = adb.execute( input2);
            return result2;
        }


        public object airPlanModeOff(string deviceId )
        {
            //adb shell dumpsys wifi | grep mAirplaneModeOn # query for wifi
            string[] input = { "-s", deviceId, "shell", "settings", "put", "global", "airplane_mode_on", "0" };
            var result = adb.execute(input);
            string[] input2 = {"-s", deviceId, "shell", "am", "broadcast", "-a", "android.intent.action.AIRPLANE_MODE" };
            var result2 = adb.execute(input2);
            return result2;
        }

        public object backup(string deviceId,string fbId)
        {
            var dir = DeviceInfo.SCREEN_PATH + "/backup/"+deviceId;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            //adb backup -f fb.backup  com.facebook.katana
            //string[] input = { "-s", deviceId, "backup","-apk -shared -all -f","backup/"+deviceId+"/"+fbId+".ad" };
            string[] input = { "-s", deviceId, "backup", "-f",   fbId + ".backup" ,"com.facebook.katana"};
            var result = adb.execute( dir, input);
            return result;
        }

        public object restore(string deviceId, string fbId)
        {
            var dir = DeviceInfo.SCREEN_PATH + "/backup/" + deviceId;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            //adb restore fb.backup
            // string[] input = { "-s", deviceId, "restore", "backup/" + deviceId + "/" +fbId+ ".ad" };
            string[] input = { "-s", deviceId, "restore",  fbId + ".backup" };
            var result = adb.execute(dir,input);
            return result;
        }

        public string[] listAllThirdPartyPackages(string deviceId)
        {
            //adb shell "pm list packages -3 | cut -c9-"
            string[] input = { "-s", deviceId, "shell", "pm", "list", "packages", "-3"};
            var output = adb.execute(input).StandardOutput;
            output = output.Replace(Environment.NewLine, "");
            string[] separatingStrings = { "package:"};
            return output.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries);
        }

        public object closeAllApp(string deviceId, Point p)
        {
            string model = deviceModel(deviceId).Trim();

            string[] input4 = { "-s", deviceId, "shell", "input", "keyevent", "KEYCODE_APP_SWITCH" };
            adb.execute(input4);
            Thread.Sleep(1000);
            switch (model)
            {
                case "CPH1877": // OPPO R17 Pro
                    string[] input5 = { "-s", deviceId, "shell input swipe", "300", "700", "300", "10", "300" };
                    adb.execute(input5);
                    Thread.Sleep(1000);
                    string[] input11 = { "-s", deviceId, "shell", "input", "tap", "270", "1930" };
                    adb.execute(input11);
                    break;
                case "OP4C72L1": // OPPO A92
                    string[] input10 = { "-s", deviceId, "shell input swipe", "300", "700", "300", "10", "300" };
                    adb.execute(input10);
                    Thread.Sleep(1000);
                    string[] input9 = { "-s", deviceId, "shell", "input", "tap", "330", "2030" };
                    adb.execute(input9);
                    break;
                case "MayaSCM": // Meitu
                    //string[] input8 = { "-s", deviceId, "shell", "input", "tap", "450", "1632" };
                    string[] input8 = { "-s", deviceId, "shell input swipe", "300", "1000", "300", "100", "300" };
                    adb.execute(input8);
                    break;
                case "TECNO-LD7": // POVA
                    string[] input6 = { "-s", deviceId, "shell", "input", "tap", "320", "1379" };
                    adb.execute(input6);
                    break;
                case "heroltelgt": // S7
                case "herolteskt": // S7
                    string[] input7 = { "-s", deviceId, "shell", "input", "tap", "240", "1788" };
                    adb.execute(input7);
                    break;
                default: // don't know device model

                    string[] input12 = { "-s", deviceId, "shell input swipe", "300", "1000", "300", "100", "300" };
                    adb.execute(input12);

                    //string[] input = { "-s", deviceId, "shell", "input", "keyevent", "KEYCODE_APP_SWITCH" };
                    //var r1 = adb.execute(input);
                    //string[] input2 = { "-s", deviceId, "shell", "input", "keyevent", "KEYCODE_DPAD_DOWN" };
                    //var r2 = adb.execute(input2);
                    ////Thread.Sleep(1000);
                    //var midHeight = p.Y;
                    //string[] input3 = { "-s", deviceId, "shell", "input", "keyevent", "KEYCODE_DEL" };

                    ////{ "-s", deviceId,"shell", "input", "swipe", "100", midHeight+"","500", midHeight+"", "500" };
                    //var output = adb.execute(input3);
                    //Thread.Sleep(1000);
                    break;
            }
            Thread.Sleep(1000);


            return "";
            if ( CLOSE_BY_SWIPE.Contains( model))
            {
                
            }
            else if ( CLOSE_BY_FORCE.Contains( model))
            {
                var packages = listAllThirdPartyPackages(deviceId);
                foreach (string package in packages)
                {
                    closeApp(deviceId, package);
                }
            }
            else
            {
                string[] input = { "-s", deviceId, "shell", "input", "keyevent", "KEYCODE_APP_SWITCH" };
                var r1 = adb.execute(input);
                string[] input2 = { "-s", deviceId, "shell", "input", "keyevent", "KEYCODE_DPAD_DOWN" };
                var r2 = adb.execute(input2);
                //Thread.Sleep(1000);
                var midHeight = p.Y;
                string[] input3 = { "-s", deviceId, "shell", "input", "keyevent", "KEYCODE_DEL" };

                //{ "-s", deviceId,"shell", "input", "swipe", "100", midHeight+"","500", midHeight+"", "500" };
                var output = adb.execute(input3);
                Thread.Sleep(1000);

            }

            return "";
   
        }

        public object setup(string deviceId, string src, string apk)
        {
            string[] input = { "-s", deviceId, "install", "\""+apk+"\"" };
            return adb.execute(src,input);
        }

        public string listPackage(string deviceId)
        {
            string[] input = { "-s", deviceId, "shell", "pm", "list", "packages"};
            var output = adb.execute(input);

            return output.StandardOutput;
        }

        public object openFbLite(string deviceId, string package)
        {
            string []input = { "-s", deviceId, "shell", "am", "start", "-n",package+"/.MainActivity"};
            var output = adb.execute(input);
            return output;
        }

        public object openUrl(string deviceId, string package, string url)
        {
            string[] input = { "-s", deviceId, "shell", "am start -a android.intent.action.VIEW -d", url, package };
            return adb.execute(input);
        }
        public object scrollUp(string deviceId)
        {
            string[] input = { "-s", deviceId,"shell", "input", "swipe", "250", "800", "250", "100", "300" };
            return adb.execute(input);
        }
        public object swipe(string deviceId, string x1, string y1, string x2, string y2, string speed="300")
        {
            string[] input = { "-s", deviceId, "shell", "input", "swipe", x1, y1, x2, y2, speed };
            return adb.execute(input);
        }
        public void keyForText(string deviceId, string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                if (AndroidKeyMap.KEY_MAP.ContainsKey(ch))
                {
                    var key = AndroidKeyMap.KEY_MAP[ch];
                    this.key(deviceId, key);
                }
                else
                {
                    log.Warn("not found key code for " + ch);
                    this.text(deviceId, ch + "");
                }
            }
        }

        public void closeApp(string deviceId, string packageName)
        {
            string[] input = { "-s", deviceId, "shell am force-stop", packageName };
             adb.execute(input);
        }

        public object captureView(string deviceId)
        {
            string[] input = { "-s", deviceId, " shell uiautomator dump /sdcard/"+deviceId+".xml" };
            adb.execute(input);
            string[] input2 = { "-s", deviceId, " pull /sdcard/"+deviceId+".xml" };
            adb.execute(input2);
            var path = DeviceInfo.SCREEN_PATH + "/" + deviceId + ".xml";
            if (File.Exists(path))
            {
                return path;
            }
            return null;
            
            
        }

        public object grantPermision(string deviceId, string packageName ,string permisison)
        {
            string[] input = { "-s", deviceId, "shell pm grant", packageName, permisison };
            return adb.execute(input);
        }

        public string deviceModel(string deviceId)
        {
            string[] input = { "-s", deviceId,  "shell", "getprop ro.product.device"};
            var result = adb.execute(input);
            if( result != null)
            {
                return result.StandardOutput;
            }
            return "";
        }
    }
}
