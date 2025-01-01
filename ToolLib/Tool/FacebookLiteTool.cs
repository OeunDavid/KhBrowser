using CsQuery;
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
    public interface IFacebookLiteTool
    {
        void setup(string deviceId);
        void setup(string deviceId,string apkID);
        object removeFB(string deviceId,string packageID);
        List<string> apkList( string deviceId);
        void openFb(string deviceId, string packageName);
        int login(Device device, string packageName, string username, string password,string twofaToken=null, bool detectLanguage= false, bool reEnterUID= true);
        void openGroup(string deviceId, string packageName, string groupId);
        bool joinGroup(string deviceId, string packageName, string groupId);
        bool leaveGroup(string deviceId, string packageName);
        bool openUrl(string deviceId, string packageName, string url);
        bool share(string deviceId, string packageName, string text);
        bool postImage(string deviceId, string packageName);
        bool shareToGroup(string deviceId, string packageName, string url, int groupInx);
        bool tapOnControl(string deviceId, IDomObject dom);
        bool tapOnControl(string deviceId, CQ dom);
        string[] listAllDeviceIds();
        void playFb(string deviceId, string packageName);
        void closeAllApp(string deviceId);
        void closeApp(string deviceId, string packageName);
        void loadingFB(string deviceId);
        bool dataMode(string deviceId);
        bool dataMode(string deviceId, string packageName);
        int success(string deviceId, string packageName);
        void scrollUp(string deviceId);
        void scrollUp(string deviceId, string x1, string y1, string x2, string y2,string speed="300");
        void scrollDown(string deviceId);
        bool likePost(string deviceId);
        bool commentPost(string deviceId, string comment);
        void gotoURL(string deviceId, string packageName, string url);
        bool postCaption(string deviceId, string caption);
        void back(string deviceId,bool isClickButton=true);
        bool detectDarkMode(string deviceId);
        bool switchToDarkMode(string deviceId, string packageName);
        bool goToFriend(string deviceId,string packageName);
        bool goToAddFriend(string deviceId,string packageName);
        bool addFriend(string deviceId);
        bool confirmFriend(string deviceId);
        bool shareTimeline(string deviceId, string caption="");
        void simResetIP(string deviceId, int timeWaiting=5000);
        void reboot(string deviceId);
        object goToHome(string deviceId,string packageName);
        bool shareToGroupStep1(string deviceId, string packageName);
        bool shareToGroupStep2(string deviceId, string packageName, int viewIndex);
        bool shareToGroupStep3(string deviceId, string packageName, string caption);
        bool clickPostButton(string deviceId);
        bool goToGroupMenu(string deviceId, string packageName="");
        Point getLeaveGroupPoint(string groupId);
        void tapLeaveGroupPoint(string deviceId, Point groupPoint);
        IImageHelper getImageHelper();
        bool gotoLeaveGroupWithoutGroupID(string deviceId);
        bool isHaveGroup(string deviceId);
        bool tapButtonLoginPrivacy(string deviceId);
        bool tapNotNow(string deviceId);
        void swipeResetIP(string deviceId);
        void clickViewVDO(string deviceId);
        bool goToPost(string deviceId);
        bool makeSureLeaveGroup(string deviceId, string packageName);
        bool detectWatchScreen(string deviceId, string packageName);
        bool detectViewVDO(string deviceId,string packageName);
        bool isScreenLogin(string deviceId);
        bool loginAccount(string deviceId, string packageName, string username, string password, int deviceDataMode = 0, bool detectLanguage= false);
        bool likePage(string deviceId);
    }
    public class FacebookLiteTool : IFacebookLiteTool
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        const string FB_PACKAGE_PREFIX = "com.facebook";
        const string EXCLUDE_PACAKGE = "com.facebook.katana,com.facebook.services,com.facebook.pages.app,com.facebook.system,com.facebook.appmanager,com.facebook.orca";
        private IAdbCommand adbCommand;
        private IAdbTask adbTask;
        private IFacebookTool facebookTool;
        private IImageHelper imageHelper;
        private ITwoFactorRequest twoFactorRequest;
        private IViewParser viewParser;
        private static readonly Dictionary<string, List<string>> PACAKGE_CACHE = new Dictionary<string, List<string>>();
        public FacebookLiteTool( IAdbCommand adbCommand, IAdbTask adbTask , IFacebookTool facebookTool, IImageHelper imageHelper, ITwoFactorRequest twoFactorRequest, IViewParser viewParser)
        {
            this.adbCommand = adbCommand;
            this.adbTask = adbTask;
            this.facebookTool = facebookTool;
            this.imageHelper = imageHelper;
            this.twoFactorRequest = twoFactorRequest;
            this.viewParser = viewParser;
        }
        public IImageHelper getImageHelper()
        {
            return this.imageHelper;
        }
        public void reboot(string deviceId)
        {
            adbTask.reboot(deviceId);
        }
        public bool likePage(string deviceId)
        {
            bool success = false;
            var screenPath = facebookTool.captureScreen(deviceId);
            var likePageBtn = facebookTool.findAndTapV2(deviceId, IconName.FB_LITE_BUTTON_LIKE_PAGE_2);
            if (likePageBtn.IsEmpty)
            {
                var btn = imageHelper.match(IconName.FB_LITE_BUTTON_LIKE_PAGE_1, screenPath);
                if(!btn.IsEmpty)
                {
                    success = true;
                    var midPoint = imageHelper.midPoint(deviceId, btn);
                    adbTask.tap(deviceId, midPoint);
                }
            } 
            if(!success && likePageBtn.IsEmpty)
            {
                likePageBtn = facebookTool.findAndTapV2(deviceId, IconName.FB_LITE_BUTTON_FOLLOW_PAGE_1);
            }
            if(!likePageBtn.IsEmpty)
            {
                success = true;
            }

            return success;
        }
        public bool isScreenLogin(string deviceId)
        {
            bool success = false;
            var screenPath = facebookTool.captureScreen(deviceId);
            var sourceText = imageHelper.getText(screenPath);
            if (sourceText.Contains("Mobile number or email") || sourceText.Contains("Create New Account") || sourceText.Contains("Tab Your Profile"))
            {
                success = true;
            }

            return success;
        }
        public bool detectViewVDO(string deviceId, string packageName)
        {
            bool success = false;
            int counter = 4;
            do
            {
                Thread.Sleep(1000);
                var screenPath = facebookTool.captureScreen(deviceId);
                var sourceText = imageHelper.getText(screenPath);
                if (sourceText.Contains("Follow") || sourceText.Contains("More Videos"))
                {
                    success = true;
                }
                else if (sourceText.Contains("Use Facebook without"))
                {
                    bool s = dataModeButtonNoThanks(deviceId, packageName);
                    if (s)
                    {
                        Thread.Sleep(1000);
                        s = dataModeButtonUseData(deviceId, packageName);
                    }
                }
            } while (!success && counter-- > 0);

            return success;
        }
        public bool detectWatchScreen(string deviceId, string packageName)
        {
            bool success = false;
            int counter = 4;
            do
            {
                Thread.Sleep(1000);
                var screenPath = facebookTool.captureScreen(deviceId);
                var sourceText = imageHelper.getText(screenPath);
                if(sourceText.Contains("Videos"))
                {
                    success = true;
                } else if(sourceText.Contains("Use Facebook without"))
                {
                    bool s = dataModeButtonNoThanks(deviceId, packageName);
                    if (s)
                    {
                        Thread.Sleep(1000);
                        s = dataModeButtonUseData(deviceId, packageName);
                    }
                }
            } while (!success && counter-- > 0);

            return success;
        }
        public void clickViewVDO(string deviceId)
        {
            var viewPath = adbTask.captureView(deviceId);
            if (viewPath != null)
            {
                var view = viewParser.fromFile(viewPath.ToString());
                var viewVDO = viewParser.findBy(view, "resource-id", "com.facebook.litm:id/video_view");
                if (viewVDO != null && viewVDO.Count() > 0)
                {
                    tapOnControl(deviceId, viewVDO);
                }
            }
        }
        public void swipeResetIP(string deviceId)
        {
            adbTask.swipe(deviceId, "300", "0","300","1000");
            Thread.Sleep(500);
            adbTask.swipe(deviceId, "300", "0","300","1000");

            var viewPath = adbTask.captureView(deviceId);
            if (viewPath != null)
            {
                var view = viewParser.fromFile(viewPath.ToString());
                var airplaneMode = viewParser.findBy(view, "content-desc", "Airplane mode");
                if (airplaneMode != null && airplaneMode.Count() > 0)
                {
                    tapOnControl(deviceId, airplaneMode);
                    Thread.Sleep(2000);
                    tapOnControl(deviceId, airplaneMode);
                    Thread.Sleep(500);
                    adbTask.swipe(deviceId, "300", "9000", "300", "0");
                } else
                {
                    airplaneMode = viewParser.findBy(view, "content-desc", "Airplane,mode,Off.,Button");
                    if (airplaneMode != null && airplaneMode.Count() > 0)
                    {
                        tapOnControl(deviceId, airplaneMode);
                        Thread.Sleep(2500);
                        tapOnControl(deviceId, airplaneMode);
                        Thread.Sleep(500);
                        adbTask.swipe(deviceId, "300", "9000", "300", "0");
                    } else
                    {
                        // meitu
                        Point point = new Point();
                        point.X = 879;
                        point.Y = 63;
                        adbTask.tap(deviceId, point);
                        Thread.Sleep(1000);

                        viewPath = adbTask.captureView(deviceId);
                        view = viewParser.fromFile(viewPath.ToString());

                        airplaneMode = viewParser.findBy(view, "text", "Airplane mode");
                        if (airplaneMode != null && airplaneMode.Count() > 0)
                        {
                            tapOnControl(deviceId, airplaneMode);
                            Thread.Sleep(2000);
                            tapOnControl(deviceId, airplaneMode);
                            Thread.Sleep(100);
                            closeAllApp(deviceId);
                        }
                    }
                }
            }
        }
        public List<string> apkList(string deviceId)
        {
            //if( PACAKGE_CACHE.ContainsKey(deviceId))
            //{
                //return PACAKGE_CACHE[deviceId];
            //}
            var items = new List<string>();
            var packages = adbTask.listPackage(deviceId);
            string[] lines = packages.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            var exclude = EXCLUDE_PACAKGE.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            var map = new HashSet<string>(exclude);
      
            var prefix = "package:" + FB_PACKAGE_PREFIX;
            foreach (var l in lines)
            {
                if( l.StartsWith( prefix))
                {
                    var p = l.Substring(8);
                    if(map.Contains(p))
                    {
                        continue;
                    }
                    items.Add(p);
                }
            }
            PACAKGE_CACHE[deviceId] = items;

            return items;
        }
        public object goToHome(string deviceId, string packageName)
        {
            return adbTask.goToHome(deviceId, packageName);
        }
        public void simResetIP(string deviceId, int timeWaiting=5000)
        {
            adbTask.airPlanModeOn(deviceId);
            Thread.Sleep(4000);
            adbTask.airPlanModeOff(deviceId);
            Thread.Sleep(timeWaiting);
        }
        public void closeAllApp(string deviceId)
        {
            facebookTool.closeAllApp(deviceId);
        }
        public void closeApp(string deviceId, string packageName)
        {
            adbTask.closeApp(deviceId, packageName);
        }
        public object removeFB(string deviceId, string packageID)
        {
            return adbTask.removeFB(deviceId, packageID);
        }
        public void openFb(string deviceId, string packageId)
        {
            adbTask.openFbLite(deviceId, packageId);
        }
        public void scrollUp(string deviceId)
        {
            adbTask.scrollUp(deviceId);
        }
        public void scrollUp(string deviceId, string x1, string y1, string x2, string y2, string speed="300")
        {
            adbTask.swipe(deviceId,x1,y1,x2,y2,speed);
        }
        public void scrollDown(string deviceId)
        {
            adbTask.scrollDown(deviceId);
        }
        public void gotoURL(string deviceId, string packageName, string url)
        {
            adbTask.openUrl(deviceId, packageName, url);
        }
        public bool detectDarkMode(string deviceId)
        {
            Thread.Sleep(1000);
            var screenPath = facebookTool.captureScreen(deviceId);
            var textBoxMatch = imageHelper.match(IconName.FB_LITE_DARK_MODE_A1, screenPath);
            if (textBoxMatch.IsEmpty)
            {
                return false;
            }

            return true;
        }
        public bool switchToDarkMode(string deviceId,string packageName)
        {
            int counter = 8;
            bool is_working = false;

            goToMenu(deviceId, packageName);

            do
            {
                scrollUp(deviceId,"120","500","120","100");
                Thread.Sleep(1000);
                var screenPath = facebookTool.captureScreen(deviceId);
                if(counter>=6)
                {
                    string sourceText = imageHelper.getText(screenPath);
                    if(sourceText.Contains("Swipe"))
                    {
                        adbTask.tap(deviceId, "100", "100");
                    }
                }

                var dataMode = imageHelper.match(IconItem.FB_LITE_DARK_MODE_NONE_1, screenPath);
                if (!dataMode.IsEmpty)
                {
                    is_working = true;
                    var midPoint = imageHelper.midPoint(deviceId, dataMode);
                    adbTask.tap(deviceId, midPoint);
                    Thread.Sleep(1000);
                } else
                {
                    var tap = facebookTool.findAndTap(deviceId, IconName.FB_LITE_DARK_MODE_NONE_3);
                    if (!tap.IsEmpty)
                    {
                        is_working = true;
                    }
                }
            } while (!is_working && counter-- > 0);
            if(!is_working)
            {
                is_working= switchToDarkModeStep2(deviceId, packageName);
            } 

            return is_working;
        }
        public bool goToMenu(string deviceId, string packageName)
        {
            adbTask.swipe(deviceId, "500", "300", "0", "300");

            return true;
        }
        public bool switchToDarkModeStep2(string deviceId, string packageName)
        {
            int counter = 2;
            bool is_working = false;

            do
            {
                Thread.Sleep(1000);
                var screenPath = facebookTool.captureScreen(deviceId);
                var dataMode = imageHelper.match(IconItem.FB_LITE_SETTING_2, screenPath);
                if (!dataMode.IsEmpty)
                {
                    is_working = true;
                    var midPoint = imageHelper.midPoint(deviceId, dataMode);
                    adbTask.tap(deviceId, midPoint);
                } 
                else
                {
                    adbTask.scrollDown(deviceId, "120", "500", "120", "100");
                }
            } while (!is_working && counter-- > 0);
            if (is_working)
            {
                is_working= switchToDarkModeStep3(deviceId, packageName);
            }

            return is_working;
        }
        public bool switchToDarkModeStep3(string deviceId, string packageName)
        {
            int counter = 3;
            bool is_working = false;

            do
            {
                scrollUp(deviceId);
                Thread.Sleep(2000);
                var screenPath = facebookTool.captureScreen(deviceId);
                var tap = facebookTool.findAndTap(deviceId, IconName.FB_LITE_DARK_MODE_NONE_3);
                if (!tap.IsEmpty)
                {
                    is_working = true;
                }
                else
                {
                    var dataMode = imageHelper.match(IconName.FB_LITE_DARK_MODE_NONE_3, screenPath);
                    if (dataMode.IsEmpty)
                    {
                        dataMode = imageHelper.match(IconName.FB_LITE_DARK_MODE_NONE_4, screenPath);
                    }
                    if (!dataMode.IsEmpty)
                    {
                        is_working = true;
                        var midPoint = imageHelper.midPoint(deviceId, dataMode);
                        adbTask.tap(deviceId, midPoint);
                    }
                }
            } while (!is_working && counter-- > 0);
            if (is_working)
            {
                is_working= switchToDarkModeStep4(deviceId, packageName);
            }

            return is_working;
        }
        public bool switchToDarkModeStep4(string deviceId, string packageName)
        {
            bool success = false;
            Thread.Sleep(1000);
            var viewPath = adbTask.captureView(deviceId);
            if (viewPath != null)
            {
                var view = viewParser.fromFile(viewPath.ToString());
                var mainView = viewParser.findBy(view, "resource-id", packageName + ":id/main_layout");
                if (mainView.Count() != 0)
                {
                    var wrapper = mainView.Children();
                    var viewGroup = viewParser.findByClass(wrapper, "android.view.ViewGroup");

                    success= tapOnControl(deviceId, viewGroup.Last());
                }
            }

            return success;
        }
        public bool commentPost(string deviceId, string comment)
        {
            bool isComment = false;
            int counter = 1;
            do
            {
                Thread.Sleep(1000);
                facebookTool.captureScreen(deviceId);
                var button = facebookTool.findAndTapV2(deviceId, IconName.FB_LITE_BUTTON_COMMENT_1);
                if (!button.IsEmpty)
                {
                    Thread.Sleep(1000);
                    facebookTool.captureScreen(deviceId);
                    var text = facebookTool.findAndTapV2(deviceId, IconName.FB_LITE_COMMENT_TEXT_BOX_1);
                    if(text.IsEmpty)
                    {
                        text = facebookTool.findAndTapV2(deviceId, IconName.FB_LITE_COMMENT_TEXT_BOX_2);
                    }
                    if (!text.IsEmpty)
                    {
                        Thread.Sleep(1000);
                        adbTask.keyForText(deviceId, comment);
                        Thread.Sleep(500);
                        isComment = send(deviceId);
                        if(isComment)
                        {
                            back(deviceId, false);
                        }
                    }
                }
            } while (!isComment && counter-- > 0);

            return isComment;
        }
        public bool send(string deviceId)
        {
            var viewPath = adbTask.captureView(deviceId);
            if (viewPath != null)
            {
                var view = viewParser.fromFile(viewPath.ToString());
                var addFriendBtn = viewParser.findBy(view, "content-desc", "SEND");
                if (addFriendBtn != null && addFriendBtn.Count() > 0)
                {
                    return tapOnControl(deviceId, addFriendBtn);
                }
            }

            return false;
        }
        public bool likePost(string deviceId)
        {
            Boolean is_working = false;
            int counter = 1;
            do
            {
                Thread.Sleep(1000);
                facebookTool.captureScreen(deviceId);
                var tap = facebookTool.findAndTap(deviceId, IconName.FB_LITE_LIKE_BUTTON_1);
                if (!tap.IsEmpty)
                {
                    is_working = true;
                    back(deviceId);
                }
            } while (!is_working && counter-- > 0);

            return is_working;
        }
        public void back(string deviceId,bool isClickButton=true)
        {
            if (isClickButton)
            {
                int counter = 2;
                Boolean is_working = false;
                do
                {
                    Thread.Sleep(100);
                    var screenPath = facebookTool.captureScreen(deviceId);
                    var dataMode = imageHelper.match(IconName.FB_LITE_BACK_BUTTON_1, screenPath);
                    if (dataMode.IsEmpty)
                    {
                        dataMode = imageHelper.match(IconName.FB_LITE_BACK_BUTTON_2, screenPath);
                    }
                    if (dataMode.IsEmpty)
                    {
                        dataMode = imageHelper.match(IconName.FB_LITE_BACK_BUTTON_3, screenPath);
                    }
                    if (!dataMode.IsEmpty)
                    {
                        is_working = true;
                        var midPoint = imageHelper.midPoint(deviceId, dataMode);
                        adbTask.tap(deviceId, midPoint);
                    }
                } while (!is_working && counter-- > 0);
            } else
            {
                adbTask.back(deviceId);
            }
        }
        public int success(string deviceId, string packageName)
        {
            int result = 0;
            Boolean is_working = false;
            int counter = 8;
            do
            {
                Thread.Sleep(1000);
                var screenPath = facebookTool.captureScreen(deviceId);
                string sourceText = imageHelper.getText(screenPath);
                if(sourceText.Contains("Been Locked") ||
                   sourceText.Contains("been locked"))
                {
                    // Your Account Has Been Locked
                    is_working = true;
                    result = 3;
                } 
                else if(sourceText.Contains("Been Disabled"))
                {
                    // Your Account Has Been Disabled
                    is_working = true;
                    result = 2;
                }
                else if (sourceText.Contains("Incorrect Password") ||
                    sourceText.Contains("We could't find an account") || 
                    sourceText.Contains("CREATE ACCOUNT"))
                {
                    // Incorrect uid or password
                    is_working = true;
                    result = 4;
                }
                else if (sourceText.Contains("Allow Lite") ||
                   sourceText.Contains("Mobile number or email"))
                {
                    // need login
                    is_working = true;
                    result = 5;
                }
                else if (sourceText.Contains("Log Into Another"))
                {
                    // logout
                    is_working = true;
                    result = 6;
                }
                else if(sourceText.Contains("Add to Story") ||
                    sourceText.Contains("Friends") ||
                    sourceText.Contains("Use Facebook without data") || 
                    sourceText.Contains("Story") || 
                    sourceText.Contains("Photo") || 
                    sourceText.Contains("Live video") || 
                    sourceText.Contains("Location") || 
                    sourceText.Contains("Add Friend") || 
                    sourceText.Contains("Live") || 
                    sourceText.Contains("Videos")) 
                {
                    is_working = true;
                    result = 1;
                    /*if(sourceText.Contains("Use Facebook without data"))
                    {
                        bool success = dataModeButtonNoThanks(deviceId, packageName);
                        if (success)
                        {
                            Thread.Sleep(1000);
                            success = dataModeButtonUseData(deviceId, packageName);
                        }
                    }
                    */
                }
            } while (!is_working && counter-- > 0);

            return result;
        }
        public void setup(string deviceId,string apkID)
        {
            adbTask.setup(deviceId, DeviceInfo.FB_LITE_SOURCE, apkID+".apk");
        }
        public void setup(string deviceId)
        {
            DirectoryInfo d = new DirectoryInfo(DeviceInfo.FB_LITE_SOURCE);
            FileInfo[] Files = d.GetFiles("*.apk"); //Getting apk
            foreach (FileInfo file in Files)
            {
                adbTask.setup(deviceId, DeviceInfo.FB_LITE_SOURCE, file.Name);
            }
        }
        private int saveLoginInfo(string deviceId)
        {
            int result = 0;
            Boolean is_working = false;
            int counter = 10;
            do
            {
                Thread.Sleep(1000);
                var screenPath = facebookTool.captureScreen(deviceId);
                var saveLoginInfo = imageHelper.match(IconItem.FB_LITE_SAVE_LOGIN_INFO, screenPath);
                if(!saveLoginInfo.IsEmpty)
                {
                    is_working = true;
                }
            } while (!is_working && counter-- > 0);
            if (is_working)
            {
                result = 1;
                var loginSaveInfoOK = facebookTool.findAndTap(deviceId, IconName.FB_LITE_SAVE_LOGIN_INFO_OK);
                if (!loginSaveInfoOK.IsEmpty)
                {
                    is_working = false;
                    counter = 5;
                    do
                    {
                        Thread.Sleep(1000);
                        var screenPath = facebookTool.captureScreen(deviceId);
                        var sourceText = imageHelper.getText(screenPath).ToLower();
                        if (sourceText.Contains("turn on contact") || sourceText.Contains("mobile number"))
                        {
                            is_working = true;
                        } else
                        {
                            var notFindAny = imageHelper.match(IconName.FB_LITE_LOGIN_FIND_FRIEND, screenPath);
                            if (!notFindAny.IsEmpty)
                            {
                                is_working = true;
                            }
                        }
                        if(is_working)
                        {
                            tapNotNow(deviceId);
                            Thread.Sleep(2000);
                            screenPath = facebookTool.captureScreen(deviceId);
                            sourceText = imageHelper.getText(screenPath).ToLower();
                            if (sourceText.Contains("turn on contact") || sourceText.Contains("mobile number"))
                            {
                                tapNotNow(deviceId);
                            }
                        }
                    } while (!is_working && counter-- > 0);
                }
            }

            return result;
        }		
        private void checkAllowContact(string deviceId)
        {
            var path = adbTask.captureView(deviceId);
            if( path != null)
            {
                var view = viewParser.fromFile(path+"");
                var textVeiw = viewParser.findByClass(view, "android.widget.TextView");
                if( textVeiw!=null && textVeiw.Count() != 0)
                {
                    var contentText = textVeiw.First().Attr("text");
                    if(contentText.Contains("to access your contacts"))
                    {
                        var btns = viewParser.findByClass(view, "android.widget.Button");
                        if( btns!=null && btns.Count() > 0)
                        {
                            var rect = viewParser.viewBound(btns.First());
                            var point = viewParser.midPonit(rect);
                            adbTask.tap(deviceId, point);
                        }
                    }
                }
            }
        }
        public bool dataMode(string deviceId, string packageName)
        {
            bool is_working = false;
            int counter = 2;
            do
            {
                Thread.Sleep(1000);
                var screenPath = facebookTool.captureScreen(deviceId);
                is_working = detectDataMode(screenPath);
            } while (!is_working && counter-- > 0);
            if(!is_working)
            {
                return false;
            }
            bool success = dataModeButtonNoThanks(deviceId,packageName);
            if(success)
            {
                Thread.Sleep(1000);
                success = dataModeButtonUseData(deviceId, packageName);
            }

            return success;
        }
        public bool detectDataMode(string screenPath)
        {
            bool is_working = false;
            string sourceText = imageHelper.getText(screenPath).ToLower();
            if (sourceText.Contains("use facebook without"))
            {
                is_working = true;
            }

            return is_working;
        }
        public bool dataModeButtonNoThanks(string deviceId, string packageName)
        {
            var viewPath = adbTask.captureView(deviceId);
            if (viewPath != null)
            {
                var view = viewParser.fromFile(viewPath.ToString());
                var mainView = viewParser.findBy(view, "resource-id", packageName + ":id/main_layout");
                if (mainView.Count() != 0)
                {
                    var wrapper = mainView.Children();
                    var viewGroup = viewParser.findByClass(wrapper, "android.view.ViewGroup");
                    
                    return tapOnControl(deviceId, viewGroup.Last());
                }
            }

            return false;
        }
        public bool dataModeButtonUseData(string deviceId, string packageName)
        {
            var viewPath = adbTask.captureView(deviceId);
            if (viewPath != null)
            {
                var view = viewParser.fromFile(viewPath.ToString());
                var mainView = viewParser.findBy(view, "resource-id", packageName + ":id/main_layout");
                if (mainView.Count() != 0)
                {
                    var wrapper = mainView.Children();
                    var viewGroup = viewParser.findByClass(wrapper, "android.view.ViewGroup");

                    return tapOnControl(deviceId, viewGroup.ElementAt(8));
                }
            }

            return false;
        }
        public bool dataModeButtonUseData_backup(string deviceId, string packageName)
        {
            var viewPath = adbTask.captureView(deviceId);
            if (viewPath != null)
            {
                var view = viewParser.fromFile(viewPath.ToString());
                var mainView = viewParser.findBy(view, "resource-id", packageName + ":id/main_layout");
                if (mainView.Count() != 0)
                {
                    var wrapper = mainView.Children();
                    var viewGroup = viewParser.findByClass(wrapper, "android.view.ViewGroup");

                    CQ buttonUseData = null;
                    while (viewGroup.Count() > 0)
                    {
                        var bound = viewParser.viewBound(viewGroup);
                        if (bound.Left >= 540 && bound.Left <= 580)
                        {
                            buttonUseData = viewGroup;
                            break;
                        }
                        viewGroup = viewGroup.Next();
                    }
                    if (buttonUseData != null)
                    {
                        return tapOnControl(deviceId, buttonUseData);
                    }

                    return tapOnControl(deviceId, buttonUseData);
                }
            }

            return false;
        }
        public bool dataMode(string deviceId)
        {
            bool success = false;
            Thread.Sleep(2000);
            var screenPath = facebookTool.captureScreen(deviceId);
            string sourceText = imageHelper.getText(screenPath);
            if (!sourceText.Contains("Use Facebook without"))
            {
                return false;
            }
            var viewPath = adbTask.captureView(deviceId);
            if (viewPath != null)
            {
                var view = viewParser.fromFile(viewPath.ToString());
                var noThanks = viewParser.findBy(view, "content-desc", "No, Thanks");
                if (noThanks != null && noThanks.Count() > 0)
                {
                    success = true;
                } else
                {
                    noThanks = viewParser.findBy(view, "content-desc", "No Thanks");
                    if (noThanks != null && noThanks.Count() > 0)
                    {
                        success = true;
                    }
                }
                if(!success)
                {
                    var dataMode = imageHelper.match(IconName.FB_LITE_DATA_MODE_NO_THANK, screenPath);
                    if (!dataMode.IsEmpty)
                    {
                        success = true;
                        var midPoint = imageHelper.midPoint(deviceId, dataMode);
                        adbTask.tap(deviceId, midPoint);
                    }
                    else
                    {
                        dataMode = imageHelper.match(IconName.FB_LITE_DATA_MODE_NO_THANK_1, screenPath);
                        if (!dataMode.IsEmpty)
                        {
                            success = true;
                            var midPoint = imageHelper.midPoint(deviceId, dataMode);
                            adbTask.tap(deviceId, midPoint);
                        }
                    }
                    if(!success)
                    {
                        dataMode = imageHelper.match(IconName.FB_LITE_DATA_MODE_NO_THANK_2, screenPath);
                        if (!dataMode.IsEmpty)
                        {
                            success = true;
                            var midPoint = imageHelper.midPoint(deviceId, dataMode);
                            adbTask.tap(deviceId, midPoint);
                        }
                        else
                        {
                            dataMode = imageHelper.match(IconName.FB_LITE_DATA_MODE_NO_THANK_3, screenPath);
                            if (!dataMode.IsEmpty)
                            {
                                success = true;
                                var midPoint = imageHelper.midPoint(deviceId, dataMode);
                                adbTask.tap(deviceId, midPoint);
                            }
                        }
                    }
                } else
                {
                    tapOnControl(deviceId, noThanks);
                }
                if(success)
                {
                    int counter = 1;
                    do
                    {
                        success = false;
                        Thread.Sleep(2000);
                        screenPath = facebookTool.captureScreen(deviceId);
                        var dataMode = imageHelper.match(IconName.FB_LITE_DATA_MODE_OK_USE_DATA_1, screenPath);
                        if (!dataMode.IsEmpty)
                        {
                            success = true;                            
                        } else
                        {
                            dataMode = imageHelper.match(IconName.FB_LITE_DATA_MODE_OK_USE_DATA_2, screenPath);
                            if (!dataMode.IsEmpty)
                            {
                                success = true;
                            }
                        }
                        if(success)
                        {
                            var midPoint = imageHelper.midPoint(deviceId, dataMode);
                            adbTask.tap(deviceId, midPoint);
                        }
                    } while (!success && counter-- > 0);
                }
            }

            return success;
        }
        public void dataMode_1(string deviceId)
        {
            int counter = 5;
            Boolean is_working = false;
            do
            {
                Thread.Sleep(1000);
                var screenPath = facebookTool.captureScreen(deviceId);
                var dataMode = imageHelper.match(IconName.FB_LITE_DATA_MODE_ICON, screenPath);
                if (!dataMode.IsEmpty)
                {
                    is_working = true;
                }
            } while (!is_working && counter-- > 0);
            if(!is_working)
            {
                return;
            }
            counter = 3;
            do
            {
                Thread.Sleep(1000);
                var screenPath = facebookTool.captureScreen(deviceId);
                var dataMode = imageHelper.match(IconName.FB_LITE_DATA_MODE_NO_THANK, screenPath);
                if (!dataMode.IsEmpty)
                {
                    is_working = true;
                    var midPoint = imageHelper.midPoint(deviceId, dataMode);
                    adbTask.tap(deviceId, midPoint);
                } else
                {
                    dataMode = imageHelper.match(IconName.FB_LITE_DATA_MODE_NO_THANK_1, screenPath);
                    if (!dataMode.IsEmpty)
                    {
                        is_working = true;
                        var midPoint = imageHelper.midPoint(deviceId, dataMode);
                        adbTask.tap(deviceId, midPoint);
                    }
                }
            } while (!is_working && counter-- > 0);
            if (is_working)
            {
                counter = 3;
                is_working = false;
                do
                {
                    Thread.Sleep(1000);
                    var screenPath = facebookTool.captureScreen(deviceId);
                    var dataMode = imageHelper.match(IconName.FB_LITE_DATA_MODE_OK_USE_DATA_1, screenPath);
                    if (!dataMode.IsEmpty)
                    {
                        is_working = true;
                        var midPoint = imageHelper.midPoint(deviceId, dataMode);
                        adbTask.tap(deviceId, midPoint);
                        Thread.Sleep(2000);
                    }
                } while (!is_working && counter-- > 0);
            }
        }
        public void loadingFB(string deviceId)
        {
            Boolean is_working = false;
            int counter = 15;
            Thread.Sleep(1000);
            do
            {
                Thread.Sleep(1000);
                var screenPath = facebookTool.captureScreen(deviceId);
                string sourceText = imageHelper.getText(screenPath);
                if(sourceText.Contains("Facebook") || sourceText.Contains("Log Into Another Account") || sourceText.Contains("Text") || sourceText.Contains("Location") || sourceText.Contains("Add") || sourceText.Contains("Story"))
                {
                    is_working = true;
                } else
                {
                    if(sourceText.Contains("Mobile number")) {
                        is_working = true;
                    } else
                    {
                        if (sourceText.Contains("Allow"))
                        {
                            is_working = true;
                        }
                    }
                }
            } while (!is_working && counter-- > 0);
        }
        public bool loginAccount(string deviceId, string packageName, string username, string password, int deviceDataMode=0, bool detectLanguage= false)
        {
            facebookTool.closeAllApp(deviceId);
            this.openFb(deviceId, packageName);

            // Control FBlite loading screen
            loadingFB(deviceId);

            // deny allow contact
            checkAllowContact(deviceId);
            
            //Thread.Sleep(100);
            //adbTask.tap(deviceId, new Point(200, 100));

            var screenPath = facebookTool.captureScreen(deviceId);
            string sourceText = imageHelper.getText(screenPath);

            if (sourceText.Contains("Tap Your Profile"))
            {
                var another = imageHelper.match(IconName.FB_LITE_LOGIN_ANOTHER_1, screenPath);
                if(!another.IsEmpty)
                {
                    var midPoint = imageHelper.midPoint(deviceId, another);
                    midPoint.Y = midPoint.Y + 110;
                    adbTask.tap(deviceId, midPoint);
                    Thread.Sleep(1000);
                    //checkAllowContact(deviceId);
                }
                /*
                //var tab= facebookTool.findAndTap(deviceId, IconName.FB_LITE_LOGIN_ANOTHER_3);
                var another = imageHelper.match(IconName.FB_LITE_LOGIN_ANOTHER_1, screenPath);
                if (another.IsEmpty)
                {
                    another = imageHelper.match(IconName.FB_LITE_LOGIN_ANOTHER_1, screenPath);
                    if (!another.IsEmpty)
                    {
                        var midPoint = imageHelper.midPoint(deviceId, another);
                        adbTask.tap(deviceId, midPoint); 
                        Thread.Sleep(1000);
                        checkAllowContact(deviceId);
                    }
                } else
                {
                    //Thread.Sleep(1000);
                    //checkAllowContact(deviceId); 
                    //Thread.Sleep(700);

                    var midPoint = imageHelper.midPoint(deviceId, another);
                    adbTask.tap(deviceId, midPoint);
                    Thread.Sleep(1000);
                    checkAllowContact(deviceId);
                }
                */
            } else
            {
                if (detectLanguage)
                {
                    if (!sourceText.Contains("Mobile"))
                    {
                        Thread.Sleep(500);
                        var english = imageHelper.anotherMatch(IconName.FB_LITE_SELECT_LANGUAGE, screenPath);
                        if (!english.IsEmpty)
                        {
                            var midPoint = imageHelper.midPoint(deviceId, english);
                            adbTask.tap(deviceId, midPoint);
                            Thread.Sleep(1000);
                        }
                    }
                }
            }
            var viewPath = adbTask.captureView(deviceId) + "";
            var view = viewParser.fromFile(viewPath);
            var inputParent = viewParser.findByClass(view, "android.widget.MultiAutoCompleteTextView");
            if (inputParent == null || inputParent.Count() == 0)
            {
                return false;
            }
            var inputControl = inputParent.First();//username;
            var inputTap = tapOnControl(deviceId, inputControl);
            Thread.Sleep(1000);
            if (inputTap)
            {
                adbTask.key(deviceId, AndroidKey.KEYCODE_DEL);
                Thread.Sleep(700);
                adbTask.keyForText(deviceId, username.Trim());
                
                Thread.Sleep(300);
                adbTask.key(deviceId, AndroidKey.KEYCODE_TAB);
                Thread.Sleep(500);
                adbTask.key(deviceId, AndroidKey.KEYCODE_DEL);
                Thread.Sleep(600);
                adbTask.keyForText(deviceId, password.Trim());

                var loginControl = inputParent.Last().Next();
                var loginClick = tapOnControl(deviceId, loginControl);
                if (loginClick)
                {
                    return true;
                }
            }

            return false;
        }
        public int verifyTwoFA(string deviceId, string twofaToken, string packageName="",string username ="", string password= "", bool reEnterUID= true)
        {
            int result = 0;
            bool is_working = false, isPrivacy = false ;
            var screenPath = facebookTool.captureScreen(deviceId);            
            Thread.Sleep(2000);
            string sourceText = imageHelper.getText(screenPath).ToLower();

            if (sourceText.Contains("two-factor authentication"))
            {
                is_working = true;
            } else if(sourceText.Contains("log in to facebook"))
            {
                Thread.Sleep(1000);
                tapButtonLoginPrivacy(deviceId);
                is_working = true;
                isPrivacy = true;
            }
            if (is_working)
            {
                if (isPrivacy)
                {
                    Thread.Sleep(1000);
                    is_working = twoFATapButtonOkay(deviceId);
                }
                else
                {
                    is_working = false;
                    screenPath = facebookTool.captureScreen(deviceId);
                    var okayButton = imageHelper.basicMatch(IconName.FB_LITE_2FA_REQUIRED_BUTTON, screenPath);
                    if (!okayButton.IsEmpty)
                    {
                        var midPoint = imageHelper.midPoint(deviceId, okayButton);
                        adbTask.tap(deviceId, midPoint);
                        is_working = true;
                    }
                }
                if (is_working)
                {
                    var token = twoFactorRequest.getPassCode(twofaToken);
                    Thread.Sleep(1000);
                    screenPath = facebookTool.captureScreen(deviceId);
                    sourceText = imageHelper.getText(screenPath).ToLower();
                    if(!sourceText.Contains("password from sms"))
                    {
                        return 0;
                    }

                    var viewPath = adbTask.captureView(deviceId) + "";
                    var view = viewParser.fromFile(viewPath);
                    var inputParent = viewParser.findByClass(view, "android.widget.MultiAutoCompleteTextView");
                    if (inputParent == null || inputParent.Count() == 0)
                    {
                        return -1;
                    }
                    Thread.Sleep(1000);
                    var input = facebookTool.findAndTap(deviceId, IconName.FB_LITE_LOGIN_USERNAME);
                    log.Info(" search for username text of 2fa : " + input);

                    var inputControl = inputParent.First();
                    var inputTap = tapOnControl(deviceId, inputControl);
                    if (inputTap)
                    {
                        log.Info(" begin to input username for 2fa");
                        Thread.Sleep(500);
                        if (reEnterUID)
                        {
                            adbTask.key(deviceId, AndroidKey.KEYCODE_DEL);
                            Thread.Sleep(1000);
                            adbTask.keyForText(deviceId, username);
                            Thread.Sleep(500);
                        }

                        inputControl = inputParent.Last();//SMS;
                        inputTap = tapOnControl(deviceId, inputControl);
                        if (inputTap)
                        {
                            Thread.Sleep(500);// heng
                            adbTask.key(deviceId, AndroidKey.KEYCODE_DEL);
                            Thread.Sleep(1000);// heng
                            adbTask.keyForText(deviceId, token);
                            Thread.Sleep(500);// heng
                            var loginControl = inputParent.Last().Next();
                            tapOnControl(deviceId, loginControl);
                        }
                    }
                }
            }

            return result;
        }
        public bool twoFATapButtonOkay(string deviceId)
        {
            var viewPath = adbTask.captureView(deviceId);
            if (viewPath != null)
            {
                var view = viewParser.fromFile(viewPath.ToString());
                var mainView = viewParser.findBy(view, "resource-id", "android:id/content");
                if (mainView.Count() != 0)
                {
                    var wrapper = mainView.Children();
                    var viewGroup = viewParser.findByClass(wrapper, "android.view.ViewGroup");

                    return tapOnControl(deviceId, viewGroup.ElementAt(6));
                }
            }

            return false;
        }
        public bool tapButtonLoginPrivacy(string deviceId)
        {
            var viewPath = adbTask.captureView(deviceId);
            if (viewPath != null)
            {
                var view = viewParser.fromFile(viewPath.ToString());
                var mainView = viewParser.findBy(view, "resource-id", "android:id/content");
                if (mainView.Count() != 0)
                {
                    var wrapper = mainView.Children();
                    var viewGroup = viewParser.findByClass(wrapper, "android.view.ViewGroup");

                    return tapOnControl(deviceId, viewGroup.Last());
                }
            }

            return false;
        }
        public bool tapNotNow(string deviceId)
        {
            bool success = false;
            var viewPath = adbTask.captureView(deviceId);
            if (viewPath != null)
            {
                var view = viewParser.fromFile(viewPath.ToString());
                var notNowBtn = viewParser.findBy(view, "content-desc", "Not Now");
                if (notNowBtn != null && notNowBtn.Count() > 0)
                {
                    return tapOnControl(deviceId, notNowBtn);
                } else
                {
                    var skipBtn = viewParser.findBy(view, "content-desc", "Skip");
                    if (skipBtn != null && skipBtn.Count() > 0)
                    {
                        return tapOnControl(deviceId, skipBtn);
                    }
                }
            }

            return success;
        }
        public bool logIntoAnotherAccount(string deviceId, string packageName)
        {
            var viewPath = adbTask.captureView(deviceId);
            if (viewPath != null)
            {
                var view = viewParser.fromFile(viewPath.ToString());
                var mainView = viewParser.findBy(view, "resource-sid", packageName + ":id/main_layout");
                if (mainView.Count() != 0)
                {
                    var wrapper = mainView.Children();
                    var viewGroup = viewParser.findByClass(wrapper, "android.view.ViewGroup");

                    return tapOnControl(deviceId, viewGroup.ElementAt(viewGroup.Length-14));
                }
            }

            return false;
        }
        public int login(Device device, string packageName, string username, string password, string twofaToken = null, bool detectLanguage= false, bool reEnterUID = true)
        {
            string deviceId = device.DeviceID;
            int result = 0;// 0= failed, 1= success, 2= disabled, 3= locked, 4= incorrect uid or password
            bool isStop = false;

            bool isLogin = loginAccount(deviceId, packageName, username, password,device.DataMode, detectLanguage);

            if (isLogin)
            {
                Thread.Sleep(4000);
                if (!string.IsNullOrEmpty(twofaToken))
                {
                    Thread.Sleep(1000);
                    int i= verifyTwoFA(deviceId, twofaToken, packageName, username, password,reEnterUID);
                    if(i == -1)
                    {
                        isStop = true;
                    }
                } else
                {
                    var screenPath = facebookTool.captureScreen(deviceId);
                    string sourceText = imageHelper.getText(screenPath).ToLower();
                    if (sourceText.Contains("log in to facebook"))
                    {
                        tapButtonLoginPrivacy(deviceId);
                        Thread.Sleep(2000);
                    }
                }
                if (!isStop)
                {
                    result= saveLoginInfo(deviceId);

                    if (device.DataMode == 1)
                    {
                        Thread.Sleep(1000);
                        dataMode(deviceId,packageName);
                    }
                }
            } 
            if (!isStop && result==0)
            {
                result = success(deviceId,packageName);
            }
            if(result==1)
            {
                bool darkMode= detectDarkMode(deviceId);
                if(!darkMode)
                {
                    switchToDarkMode(deviceId,packageName);
                }
            }
            Thread.Sleep(1000);
            facebookTool.closeAllApp(deviceId);

            return result;
        }
        public string[] listAllDeviceIds()
        {
            return adbTask.deviceIDs();
        }
        public void playFb(string deviceId, string packageName)
        {
            facebookTool.closeAllApp(deviceId);
            openFb(deviceId, packageName);
            Thread.Sleep(2000);
            adbTask.scrollDown(deviceId);
           var screenPath = facebookTool.captureScreen(deviceId);
            
            var homeIcon = imageHelper.basicMatch(IconName.FB_LITE_HOME_BUTTON, screenPath);
            if (!homeIcon.IsEmpty)
            {
                var midPoint = imageHelper.midPoint(deviceId, homeIcon);
                adbTask.tap(deviceId, midPoint);
            }

            adbTask.scrollUp(deviceId);
            Thread.Sleep(1000);
            adbTask.scrollUp(deviceId);
        }
        public bool  tapOnControl(string deviceId ,IDomObject dom)
        {
            try
            {
                var rect = viewParser.viewBound(dom);
                if (rect.IsEmpty)
                {
                    return false;
                }
                var point = viewParser.midPonit(rect);
                adbTask.tap(deviceId, point);
            }
            catch { }

            return true;
        }
        public bool tapOnControl(string deviceId, CQ dom)
        {
            try
            {
                var rect = viewParser.viewBound(dom);
                if (rect.IsEmpty)
                {
                    return false;
                }
                var point = viewParser.midPonit(rect);
                adbTask.tap(deviceId, point);
            }
            catch { }

            return true;
        }
        public bool postCaption(string deviceId, string caption)
        {
            bool success = false;
            Thread.Sleep(2000);
            if (!string.IsNullOrEmpty(caption))
            {
                var screenPath = facebookTool.captureScreen(deviceId);
                var textBoxMatch = imageHelper.match(IconItem.FB_LITE_SHARE_TEXTBOX, screenPath);
                if (textBoxMatch.IsEmpty)
                {
                    textBoxMatch = imageHelper.anotherMatch(IconName.FB_LITE_SHARE_TEXTBOX_OTHER, screenPath);
                }
                if (!textBoxMatch.IsEmpty)
                {
                    var point = imageHelper.midPoint(deviceId, textBoxMatch);
                    adbTask.tap(deviceId, point);

                    adbTask.keyForText(deviceId, caption);
                }
            }
            success= clickPostButton(deviceId);

            return success;
        }
        public bool goToPost(string deviceId)
        {
            bool success = false;
            var screenPath = facebookTool.captureScreen(deviceId);
            var photoBtn = imageHelper.basicMatch(IconName.FB_LITE_BUTTON_POST_PHOTO_2, screenPath);
            if(photoBtn.IsEmpty)
            {
                photoBtn = imageHelper.basicMatch(IconName.FB_LITE_BUTTON_POST_PHOTO_1, screenPath);
            }
            if(!photoBtn.IsEmpty)
            {
                var midPoint = imageHelper.midPoint(deviceId, photoBtn);
                midPoint.X = midPoint.X - 100;
                adbTask.tap(deviceId, midPoint);
            }

            return success;
        }
        public bool clickPostButton(string deviceId)
        {
            bool success = false;
            var viewPath = adbTask.captureView(deviceId);
            if (viewPath != null)
            {
                var view = viewParser.fromFile(viewPath.ToString());
                var mainView = viewParser.findBy(view, "resource-id", "android:id/content");
                if (mainView.Count() != 0)
                {
                    var wrapper = mainView.Children();
                    var viewGroup = viewParser.findByClass(wrapper, "android.view.ViewGroup");

                    success= tapOnControl(deviceId, viewGroup.ElementAt(3));
                }
            }

            return success;
        }
        public bool goToFriend(string deviceId,string packageName)
        {
            var viewPath = adbTask.captureView(deviceId);
            if( viewPath != null)
            {
                var view = viewParser.fromFile(viewPath.ToString());
                var mainView = viewParser.findBy(view, "resource-id", packageName+":id/main_layout");
                if (mainView.Count() != 0)
                {
                    var wrapper = mainView.Children();
                    var viewGroup = viewParser.findByClass(wrapper, "android.view.ViewGroup");

                    CQ findFriend = null;
                    while (viewGroup.Count() > 0)
                    {
                        var bound = viewParser.viewBound(viewGroup);
                        if ((bound.Left >= 180 && bound.Left <= 200) || (bound.Left>=120 && bound.Left <= 170))
                        {
                            findFriend = viewGroup;
                            break;
                        }
                        viewGroup = viewGroup.Next();
                    }
                    if( findFriend != null)
                    {
                        return tapOnControl(deviceId, findFriend);
                    }

                    return tapOnControl(deviceId, findFriend);
                }
            }
            
            return false;
        }
        public bool goToAddFriend(string deviceId, string packageName)
        {
            int counter = 1;
            bool success = goToFriend(deviceId,packageName);
            if(success)
            {
                success = false;
                do
                {
                    Thread.Sleep(1000);
                    var screenPath = facebookTool.captureScreen(deviceId);
                    var icon = imageHelper.match(IconName.FB_LITE_SUGGESTIONS_1, screenPath);
                    if (!icon.IsEmpty)
                    {
                        success = true;
                        var midPoint = imageHelper.midPoint(deviceId, icon);
                        adbTask.tap(deviceId, midPoint);
                    }
                } while (!success && counter-- > 0);
            }

            return success;
        }
        public bool addFriends(string deviceId, string packageName, int addNumber)
        {
            bool success = goToAddFriend(deviceId,packageName);

            if (success)
            {
                success = false;
                bool add_success = false;
                int count_err = 1;
                for (int i = 1; i <= addNumber; i++)
                {
                    add_success= addFriend(deviceId);
                    if(!add_success)
                    {
                        count_err++;
                    } else
                    {
                        success = true;
                    }
                    if(count_err>2)
                    {
                        break;
                    }
                }
            }

            return success;
        }
        public bool addFriend(string deviceId)
        {
            bool success = false;
            var viewPath = adbTask.captureView(deviceId);
            if(viewPath != null)
            {
                var view = viewParser.fromFile(viewPath.ToString());
                var addFriendBtn = viewParser.findBy(view, "content-desc", "Add Friend");
                if( addFriendBtn !=null && addFriendBtn.Count() > 0)
                {
                    return tapOnControl(deviceId, addFriendBtn);
                } else
                {
                    scrollUp(deviceId, "250", "344", "250", "100");
                }
            }

            return success;
        }
        public bool confirmFriend(string deviceId)
        {
            bool success = false;
            var viewPath = adbTask.captureView(deviceId);
            if (viewPath != null)
            {
                var view = viewParser.fromFile(viewPath.ToString());
                var addFriendBtn = viewParser.findBy(view, "content-desc", "Confirm");
                if (addFriendBtn != null && addFriendBtn.Count() > 0)
                {
                    return tapOnControl(deviceId, addFriendBtn);
                }
                else
                {
                    scrollUp(deviceId, "250", "344", "250", "100");
                }
            }

            return success;
        }
        public bool joinGroup(string deviceId, string packageName, string groupId)
        {
            openGroup(deviceId, packageName, groupId);
            Thread.Sleep(3500);
            var viewPath = adbTask.captureView(deviceId) + "";
            var view = viewParser.fromFile(viewPath);
            var joinButton = viewParser.findBy(view, "content-desc", "Join Group");
            if (joinButton != null && joinButton.Count() > 0)
            {
                bool isAnswer = false;
                isAnswer = tapOnControl(deviceId, joinButton);
                if (!isAnswer)
                {
                    return false;
                }
                Thread.Sleep(3500);
                viewPath = adbTask.captureView(deviceId) + "";
                view = viewParser.fromFile(viewPath);
                var answerScreen = viewParser.findBy(view, "content-desc", "Answer Questions");
                if (answerScreen == null || answerScreen.Count() <= 0)
                {
                    answerScreen = viewParser.findBy(view, "content-desc", "Answer questions");
                    if (answerScreen == null || answerScreen.Count() <= 0)
                    {
                        return true;
                    }
                }
                var inputParent = viewParser.findByClass(view, "android.widget.MultiAutoCompleteTextView");
                if (inputParent == null || inputParent.Count() == 0)
                {
                    isAnswer = true;
                    var mainView = viewParser.findBy(view, "resource-id", packageName + ":id/main_layout");
                    if (mainView.Count() != 0)
                    {
                        var wrapper = mainView.Children();
                        var viewGroup = viewParser.findByClass(wrapper, "android.view.ViewGroup");

                        while (viewGroup.Count() > 2)
                        {
                            viewGroup = viewGroup.Next();
                            tapOnControl(deviceId, viewGroup);
                        }
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                    var inputControl = inputParent.First();
                    var inputTap = tapOnControl(deviceId, inputControl);
                    if (inputTap)
                    {
                        isAnswer = true;
                        adbTask.text(deviceId, "OK");
                    }
                }
                if (isAnswer)
                {
                    var submitButton = viewParser.findBy(view, "content-desc", "Submit");
                    if (submitButton != null && submitButton.Count() > 0)
                    {
                        isAnswer = tapOnControl(deviceId, submitButton);
                        if (isAnswer)
                        {
                            Thread.Sleep(2000);
                        }

                        return isAnswer;
                    }
                }
            }

            return false;
        }
        public bool goToLeaveGroup(string deviceId, string packageName)
        {
            var viewPath = adbTask.captureView(deviceId);
            if (viewPath != null)
            {
                var view = viewParser.fromFile(viewPath.ToString());
                var mainView = viewParser.findBy(view, "resource-id", packageName + ":id/main_layout");
                if (mainView.Count() != 0)
                {
                    var wrapper = mainView.Children();
                    var viewGroup = viewParser.findByClass(wrapper, "android.view.ViewGroup");

                    return tapOnControl(deviceId, viewGroup.ElementAt(9));
                }
            }
           
            return false;
        }
        public bool goToConfirmLeaveGroup(string deviceId, string packageName)
        {
            var viewPath = adbTask.captureView(deviceId);
            if (viewPath != null)
            {
                var view = viewParser.fromFile(viewPath.ToString());
                var mainView = viewParser.findBy(view, "resource-id", packageName + ":id/main_layout");
                if (mainView.Count() != 0)
                {
                    var wrapper = mainView.Children();
                    var viewGroup = viewParser.findByClass(wrapper, "android.view.ViewGroup");
                    try
                    {
                        var element = viewGroup.ElementAt(25);

                        return tapOnControl(deviceId, element);
                    }
                    catch { }
                }
            }

            return false;
        }
        public bool makeSureLeaveGroup(string deviceId, string packageName)
        {
            bool success = false;
            var viewPath = adbTask.captureView(deviceId);
            if (viewPath != null)
            {
                var view = viewParser.fromFile(viewPath.ToString());
                var yourGroup = viewParser.findBy(view, "content-desc", "Your groups");
                if (yourGroup != null && yourGroup.Count() > 0)
                {
                    var mainView = viewParser.findBy(view, "resource-id", packageName + ":id/main_layout");
                    var wrapper = mainView.Children();
                    var viewGroup = viewParser.findByClass(wrapper, "android.view.ViewGroup");

                    success= tapOnControl(deviceId, viewGroup.Last().Prev());
                }
            }

            return success;
        }
        public bool confirmLeaveGroup(string deviceId, string packageName)
        {
            bool success = false;
            var viewPath = adbTask.captureView(deviceId);
            if (viewPath != null)
            {
                var view = viewParser.fromFile(viewPath.ToString());
                var addFriendBtn = viewParser.findBy(view, "content-desc", "Leave Group");
                if (addFriendBtn != null && addFriendBtn.Count() > 0)
                {
                    return tapOnControl(deviceId, addFriendBtn);
                }
            }

            return success;
        }
        public bool goToGroupMenu(string deviceId, string packageName="")
        {
            goToMenu(deviceId, packageName);
            Thread.Sleep(1000);

            bool success = false;
            int counter = 5;
            do
            {
                var screenPath = facebookTool.captureScreen(deviceId);
                var buttonMatch = imageHelper.match(IconName.FB_LITE_GROUP_MENU, screenPath);
                if(!buttonMatch.IsEmpty)
                {
                    success = true;
                    var midPoint = imageHelper.midPoint(deviceId, buttonMatch);
                    adbTask.tap(deviceId, midPoint);
                }
            } while (!success && counter-- > 0);

            return true;
        }
        public Point getLeaveGroupPoint(string deviceId)
        {
            var screenPath = facebookTool.captureScreen(deviceId);
            var midBottom = imageHelper.match(IconName.FB_LITE_LEAVE_GROUP_YOUR_GROUP, screenPath);
            if (midBottom.IsEmpty)
            {
                return new Point();
            }
            var bottomBox = new Rectangle(midBottom.X, midBottom.Bottom + 50, midBottom.Width, 100);
            var bottomPoint = imageHelper.midPoint(deviceId, bottomBox);

            return bottomPoint;
        }
        public bool gotoLeaveGroupWithoutGroupID(string deviceId)
        {
            bool success = false;
            var pointMatch = getLeaveGroupPoint(deviceId);
            if(!pointMatch.IsEmpty)
            {
                tapLeaveGroupPoint(deviceId, pointMatch);
                success = true;
            }

            return success;
        }
        public bool isHaveGroup(string deviceId)
        {
            bool success = false;
            var screenPath = facebookTool.captureScreen(deviceId);
            var sourceText = imageHelper.getText(screenPath);
            if(!sourceText.ToLower().Contains("not in any facebook groups"))
            {
                success = true;
            }

            return success;
        }
        public void tapLeaveGroupPoint(string deviceId, Point groupPoint)
        {
            adbTask.tap(deviceId, groupPoint);
        }
        public bool leaveGroup(string deviceId, string packageName)
        {
            Thread.Sleep(2000);
            goToLeaveGroup(deviceId, packageName);
            Thread.Sleep(1000);
            goToConfirmLeaveGroup(deviceId, packageName);
            Thread.Sleep(1000);
            confirmLeaveGroup(deviceId, packageName);

            return true;
        }
        public bool shareTimeline(string deviceId, string caption = "")
        {
            bool success = false;

            var screenPath = facebookTool.captureScreen(deviceId);
            var shareButton = imageHelper.anotherMatch(IconName.FB_LITE_BUTTON_SHARE_3, screenPath);
            if (shareButton.IsEmpty)
            {
                shareButton = imageHelper.anotherMatch(IconName.FB_LITE_BUTTON_SHARE_4, screenPath);
            }
            if (!shareButton.IsEmpty)
            {
                var shareButtonMatch = imageHelper.midPoint(deviceId, shareButton);
                var tab = adbTask.tap(deviceId, shareButtonMatch);

                Thread.Sleep(2000);
                screenPath = facebookTool.captureScreen(deviceId);
                //shareButton = imageHelper.anotherMatch(IconName.FB_LITE_BUTTON_SHARE_TIMELINE_1, screenPath);
                shareButton = imageHelper.basicMatch(IconName.FB_LITE_BUTTON_SHARE_TIMELINE_1, screenPath);
                if(shareButton.IsEmpty)
                {
                    shareButton = imageHelper.anotherMatch(IconName.FB_LITE_BUTTON_SHARE_TIMELINE_2, screenPath);
                    //shareButton = imageHelper.match(IconName.FB_LITE_BUTTON_SHARE_TIMELINE_2, screenPath);
                }
                if (!shareButton.IsEmpty)
                {
                    shareButtonMatch = imageHelper.midPoint(deviceId, shareButton);
                    if (!shareButtonMatch.IsEmpty)
                    {
                        adbTask.tap(deviceId, shareButtonMatch);
                        Thread.Sleep(2000);
                        success = postCaption(deviceId, caption);
                    }
                }
            }

            return success;
        }
        public bool shareToGroupStep1(string deviceId, string packageName)
        {
            bool success = false;
            var screenPath = facebookTool.captureScreen(deviceId);
            var shareButton = imageHelper.anotherMatch(IconName.FB_LITE_BUTTON_SHARE_3, screenPath);
            if(shareButton.IsEmpty)
            {
                shareButton = imageHelper.anotherMatch(IconName.FB_LITE_BUTTON_SHARE_4, screenPath);
            }
            if (!shareButton.IsEmpty)
            {
                var shareButtonMatch = imageHelper.midPoint(deviceId, shareButton);
                var tab= adbTask.tap(deviceId, shareButtonMatch);

                Thread.Sleep(2000);
                screenPath = facebookTool.captureScreen(deviceId);
                shareButton = imageHelper.anotherMatch(IconName.FB_LITE_BUTTON_SHARE_GROUP_1, screenPath);
                if(shareButton.IsEmpty)
                {
                    shareButton = imageHelper.anotherMatch(IconName.FB_LITE_BUTTON_SHARE_GROUP_2, screenPath);
                }
                if(!shareButton.IsEmpty)
                {
                    shareButtonMatch = imageHelper.midPoint(deviceId, shareButton);
                    if (!shareButtonMatch.IsEmpty)
                    {
                        tab = adbTask.tap(deviceId, shareButtonMatch);
                        success = true;
                    }
                }
            }

            return success;
        }
        public bool shareToGroupStep2(string deviceId, string packageName, int viewIndex)
        {
            //viewIndex= 2n;
            var screenPath = facebookTool.captureScreen(deviceId);
            var sourceText = imageHelper.getText(screenPath);
            if(sourceText.Contains("Results Found."))
            {
                return false;
            }
            var viewPath = adbTask.captureView(deviceId);
            if (viewPath != null)
            {
                var view = viewParser.fromFile(viewPath.ToString());
                var mainView = viewParser.findBy(view, "resource-id", packageName + ":id/main_layout");
                if (mainView.Count() != 0)
                {
                    var wrapper = mainView.Children();
                    var viewGroup = viewParser.findByClass(wrapper, "android.view.ViewGroup");
                    try
                    {
                        tapOnControl(deviceId, viewGroup.ElementAt(viewIndex));

                        return true;
                    }
                    catch {
                    }
                }
            }

            return false;
        }
        public bool shareToGroupStep3(string deviceId, string packageName, string caption)
        {
            return postCaption(deviceId, caption);
        }

















        public bool shareToGroup(string deviceId, string packageName, string url, int groupInx)
        {
            openUrl(deviceId, packageName, url);
            Thread.Sleep(1000);
            var screenPath = facebookTool.captureScreen(deviceId);
            var height = imageHelper.screenHeight(screenPath);
            var rect = new Rectangle(50, 50, DeviceInfo.DEVICE_DEFAULT_WIDTH - 10, height - 10);
            var midPoint = imageHelper.midPoint(deviceId, rect);
            adbTask.tap(deviceId, midPoint);
            Thread.Sleep(5000);

            screenPath = facebookTool.captureScreen(deviceId);
            var shareLinkRect = imageHelper.anotherMatch(IconName.FB_LITE_SHARE_TO_GROUP, screenPath);
            var shareLink = imageHelper.midPoint(deviceId, shareLinkRect);
            if (!shareLink.IsEmpty)
            {
                adbTask.tap(deviceId, shareLink);
                Thread.Sleep(5000);
                screenPath = facebookTool.captureScreen(deviceId);
                var cleanImg = imageHelper.cleanForGroup(screenPath);
                // cleanImg.Save(DeviceInfo.SCREEN_PATH + "/clean.png");
                var groupBtn = imageHelper.img(IconName.FB_LITE_SHARE_TO_GROUP_BUTTON);
                var groupList = imageHelper.matchList(groupBtn, cleanImg);

                if (groupInx < groupList.Count)
                {
                    var g = groupList[groupInx];
                    var p = imageHelper.midPoint(deviceId, g);
                    adbTask.tap(deviceId, p);
                    Thread.Sleep(5000);
                    adbTask.key(deviceId, AndroidKey.KEYCODE_BACK);
                    Thread.Sleep(1000);
                    adbTask.key(deviceId, AndroidKey.KEYCODE_BACK);
                }
            }

            return false;
        }
        public bool share(string deviceId, string packageName, string text)
        {
            facebookTool.closeAllApp(deviceId);
            Thread.Sleep(1000);
            openFb(deviceId, packageName);
            Thread.Sleep(3000);
            var screenPath = facebookTool.captureScreen(deviceId);
            var textBoxMatch = imageHelper.match(IconItem.FB_LITE_SHARE_TEXTBOX, screenPath);
            if (textBoxMatch.IsEmpty)
            {
                // facebookTool.closeAllApp(deviceId);
                adbTask.scrollDown(deviceId);
                Thread.Sleep(3000);
                screenPath = facebookTool.captureScreen(deviceId);
                textBoxMatch = imageHelper.match(IconItem.FB_LITE_SHARE_TEXTBOX, screenPath);
            }
            if (textBoxMatch.IsEmpty)
            {
                textBoxMatch = imageHelper.anotherMatch(IconName.FB_LITE_SHARE_TEXTBOX_OTHER, screenPath);
            }
            if (!textBoxMatch.IsEmpty)
            {
                var point = imageHelper.midPoint(deviceId, textBoxMatch);
                adbTask.tap(deviceId, point);
                Thread.Sleep(3000);
                screenPath = facebookTool.captureScreen(deviceId);
                var shareTextBox = facebookTool.findAndTap(deviceId, IconName.FB_LITE_SHARE_TEXTBOX2);
                if (shareTextBox.IsEmpty)
                {
                    // shareTextBox = facebookTool.findAndTap(deviceId, IconName.FB_LITE_SHARE_TEXTBOX_OTHER);
                    var textBoxRect = imageHelper.anotherMatch(IconName.FB_LITE_SHARE_TEXTBOX_OTHER, screenPath);
                    if (!textBoxRect.IsEmpty)
                    {
                        shareTextBox = imageHelper.midPoint(deviceId, textBoxRect);
                        adbTask.tap(deviceId, shareTextBox);
                    }
                }
                if (!shareTextBox.IsEmpty)
                {
                    adbTask.text(deviceId, text);
                    adbTask.text(deviceId, "\n");
                    Thread.Sleep(4000);
                    screenPath = facebookTool.captureScreen(deviceId);
                    var findPost = facebookTool.findAndTap(deviceId, IconName.FB_LITE_POST_BUTTON);
                    if (findPost.IsEmpty)
                    {
                        var postRect = imageHelper.anotherMatch(IconName.FB_LITE_POST_BUTTON, screenPath);
                        if (!postRect.IsEmpty)
                        {
                            findPost = imageHelper.midPoint(deviceId, postRect);
                            adbTask.tap(deviceId, findPost);
                        }
                    }
                    if (!findPost.IsEmpty)
                    {
                        return true;
                    }
                }
            }


            return false;
        }
        public bool shareToGroup_bbbbbb(string deviceId, string packageName, string url, int groupInx)
        {
            openUrl(deviceId, packageName, url);
            Thread.Sleep(1000);
            var screenPath = facebookTool.captureScreen(deviceId);
            var height = imageHelper.screenHeight(screenPath);
            var rect = new Rectangle(50, 50, DeviceInfo.DEVICE_DEFAULT_WIDTH - 10, height - 10);
            var midPoint = imageHelper.midPoint(deviceId, rect);
            adbTask.tap(deviceId, midPoint);
            Thread.Sleep(5000);

            screenPath = facebookTool.captureScreen(deviceId);
            var shareLinkRect = imageHelper.anotherMatch(IconName.FB_LITE_SHARE_TO_GROUP, screenPath);
            var shareLink = imageHelper.midPoint(deviceId, shareLinkRect);
            if (!shareLink.IsEmpty)
            {
                adbTask.tap(deviceId, shareLink);
                Thread.Sleep(5000);
                screenPath = facebookTool.captureScreen(deviceId);
                var cleanImg = imageHelper.cleanForGroup(screenPath);
                // cleanImg.Save(DeviceInfo.SCREEN_PATH + "/clean.png");
                var groupBtn = imageHelper.img(IconName.FB_LITE_SHARE_TO_GROUP_BUTTON);
                var groupList = imageHelper.matchList(groupBtn, cleanImg);

                if (groupInx < groupList.Count)
                {
                    var g = groupList[groupInx];
                    var p = imageHelper.midPoint(deviceId, g);
                    adbTask.tap(deviceId, p);
                    Thread.Sleep(5000);
                    adbTask.key(deviceId, AndroidKey.KEYCODE_BACK);
                    Thread.Sleep(1000);
                    adbTask.key(deviceId, AndroidKey.KEYCODE_BACK);
                }
            }

            return false;
        }
        private bool selectImageAndPost(string deviceId, CQ viewForm)
        {
            var imgView = viewParser.findByClass(viewForm, "android.widget.ImageView");
            imgView = imgView.Last();

            log.Info("image view " + imgView.Text());
            if (imgView != null && imgView.Count() > 0)
            {
                if (tapOnControl(deviceId, imgView))
                {
                    var btn = viewParser.findBy(viewForm, "text", "Next");
                    if (btn != null && btn.Count() > 0)
                    {
                        if (tapOnControl(deviceId, btn))
                        {
                            var screenPath = facebookTool.captureScreen(deviceId);
                            var findPost = facebookTool.findAndTap(deviceId, IconName.FB_LITE_POST_BUTTON);
                            if (findPost.IsEmpty)
                            {
                                var postRect = imageHelper.anotherMatch(IconName.FB_LITE_POST_BUTTON, screenPath);
                                if (!postRect.IsEmpty)
                                {
                                    findPost = imageHelper.midPoint(deviceId, postRect);
                                    adbTask.tap(deviceId, findPost);
                                }
                            }
                            if (!findPost.IsEmpty)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        public bool postImage(string deviceId, string packageName)
        {
            //closeAllApp(deviceId);
            string[] permissionList = { "android.permission.READ_EXTERNAL_STORAGE", "android.permission.WRITE_EXTERNAL_STORAGE", "android.permission.CAMERA" };
            //Thread.Sleep(1000);
            //openFb(deviceId, packageName);
            //Thread.Sleep(3000);
            foreach (var p in permissionList)
            {
                adbTask.grantPermision(deviceId, packageName, p);
            }

            var screenPath = facebookTool.captureScreen(deviceId);
            var textBoxMatch = imageHelper.match(IconItem.FB_LITE_SHARE_TEXTBOX, screenPath);
            if (textBoxMatch.IsEmpty)
            {
                // facebookTool.closeAllApp(deviceId);
                adbTask.scrollDown(deviceId);
                Thread.Sleep(3000);
                screenPath = facebookTool.captureScreen(deviceId);
                textBoxMatch = imageHelper.match(IconItem.FB_LITE_SHARE_TEXTBOX, screenPath);
            }
            if (textBoxMatch.IsEmpty)
            {
                textBoxMatch = imageHelper.anotherMatch(IconName.FB_LITE_SHARE_TEXTBOX_OTHER, screenPath);
            }
            if (!textBoxMatch.IsEmpty)
            {
                var point = imageHelper.midPoint(deviceId, textBoxMatch);
                adbTask.tap(deviceId, point);
                Thread.Sleep(3000);

                Thread.Sleep(1000);
                var viewPath = adbTask.captureView(deviceId) + "";
                var viewForm = viewParser.fromFile(viewPath);
                var tagFriend = viewParser.findBy(viewForm, "content-desc", "Tag Friends");
                if (tagFriend != null && tagFriend.Count() > 0)
                {
                    var photoButton = tagFriend.Prev();
                    if (tapOnControl(deviceId, photoButton))
                    {
                        viewPath = adbTask.captureView(deviceId) + "";
                        viewForm = viewParser.fromFile(viewPath);
                        var galleryItem = viewParser.findBy(viewForm, "text", "Gallery");
                        if (galleryItem != null && galleryItem.Count() > 0)
                        {
                            return selectImageAndPost(deviceId, viewForm);
                        }
                        else
                        {
                            var allowCamera = viewParser.findBy(viewForm, "content-desc", "Allow Access");
                            if (allowCamera != null && allowCamera.Count() > 0)
                            {
                                if (tapOnControl(deviceId, allowCamera))
                                {
                                    //com.android.permissioncontroller:id/permission_message
                                    viewPath = adbTask.captureView(deviceId) + "";
                                    viewForm = viewParser.fromFile(viewPath);
                                    var permissionMsg = viewParser.findBy(viewForm, "resource-id", "com.android.permissioncontroller:id/permission_message");
                                    if (permissionMsg != null && permissionMsg.Count() > 0)
                                    {
                                        if (permissionMsg.Attr("text").Contains("take pictures and record video?"))
                                        {
                                            var btn = viewParser.findByClass(viewForm, "android.widget.Button");
                                            if (tapOnControl(deviceId, btn))
                                            {
                                                viewPath = adbTask.captureView(deviceId) + "";
                                                viewForm = viewParser.fromFile(viewPath);
                                                galleryItem = viewParser.findBy(viewForm, "text", "Gallery");
                                                if (galleryItem != null && galleryItem.Count() > 0)
                                                {
                                                    return selectImageAndPost(deviceId, viewForm);
                                                }
                                                else
                                                {
                                                    var allowPhoto = viewParser.findBy(viewForm, "content-desc", "Allow Access");
                                                    if (allowPhoto != null && allowPhoto.Count() > 0)
                                                    {
                                                        if (tapOnControl(deviceId, allowPhoto))
                                                        {
                                                            viewPath = adbTask.captureView(deviceId) + "";
                                                            viewForm = viewParser.fromFile(viewPath);
                                                            permissionMsg = viewParser.findBy(viewForm, "resource-id", "com.android.permissioncontroller:id/permission_message");
                                                            if (permissionMsg != null && permissionMsg.Count() > 0)
                                                            {
                                                                if (permissionMsg.Attr("text").Contains("to access photos, media, and files"))
                                                                {
                                                                    btn = viewParser.findByClass(viewForm, "android.widget.Button");
                                                                    if (tapOnControl(deviceId, btn))
                                                                    {
                                                                        viewPath = adbTask.captureView(deviceId) + "";
                                                                        viewForm = viewParser.fromFile(viewPath);
                                                                        galleryItem = viewParser.findBy(viewForm, "text", "Gallery");
                                                                        if (galleryItem != null && galleryItem.Count() > 0)
                                                                        {
                                                                            return selectImageAndPost(deviceId, viewForm);
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }


                                        }
                                    }
                                }
                            }
                        }

                    }


                }
            }
            return false;
        }
        public void openGroup(string deviceId, string packageName, string groupId)
        {
            var url = "https://www.facebook.com/groups/" + groupId;
            adbTask.openUrl(deviceId, packageName, url);
        }
        
        public bool joinGroup_backup(string deviceId, string packageName, string groupId)
        {
            openGroup(deviceId, packageName, groupId);
            Thread.Sleep(3000);
            var screenPath = facebookTool.captureScreen(deviceId);

            var joinButton = facebookTool.findAndTap(deviceId, IconName.FB_LITE_JOIN_GROUP_BUTTON);
            if (joinButton.IsEmpty)
            {
                joinButton = facebookTool.findAndTap(deviceId, IconName.FB_LITE_JOIN_GROUP_BUTTON2);
            }
            if (!joinButton.IsEmpty)
            {
                //  var midPoint = imageHelper.midPoint(deviceId, joinButton);
                //  adbTask.tap(deviceId, midPoint);
                Thread.Sleep(2000);
                screenPath = facebookTool.captureScreen(deviceId);
                var cancelRequest = imageHelper.match(IconItem.FB_LITE_CANCEL_GROUP_BUTTON, screenPath);
                if (!cancelRequest.IsEmpty)
                {
                    return true;
                }
                else
                {
                    screenPath = facebookTool.captureScreen(deviceId);
                    var groupQuestion = imageHelper.match(IconName.FB_LITE_JOIN_GROUP_ANSWER, screenPath);
                    if (!groupQuestion.IsEmpty)
                    {
                        var point = imageHelper.midPoint(deviceId, groupQuestion);
                        adbTask.tap(deviceId, point);
                        Thread.Sleep(100);
                        adbTask.text(deviceId, "OK");
                        Thread.Sleep(100);
                        var submitAnswer = facebookTool.findAndTap(deviceId, IconName.FB_LITE_JOIN_GROUP_ANSWER_SUBMIT);
                        if (!submitAnswer.IsEmpty)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        public bool leaveGroup_backup(string deviceId, string packageName, string groupId)
        {
            facebookTool.closeAllApp(deviceId);
            Thread.Sleep(1000);
            openGroup(deviceId, packageName, groupId);
            Thread.Sleep(3000);
            var screenPath = facebookTool.captureScreen(deviceId);//todo
            var midTop = new Rectangle(0, 0, DeviceInfo.DEVICE_DEFAULT_WIDTH, 500);
            var midPoint = imageHelper.midPoint(deviceId, midTop);
            adbTask.tap(deviceId, midPoint);
            Thread.Sleep(1000);
            var rightTop = new Rectangle(DeviceInfo.DEVICE_DEFAULT_WIDTH - 50, 0, 50, 100);
            var rightPoint = imageHelper.midPoint(deviceId, rightTop);
            adbTask.tap(deviceId, rightPoint);
            Thread.Sleep(3000);
            screenPath = facebookTool.captureScreen(deviceId);
            var screen = imageHelper.img(screenPath);
            var midBottom = imageHelper.anotherMatch(IconName.FB_LITE_GROUP_INFO, screenPath);
            if (midBottom.IsEmpty)
            {
                return false;
            }
            var bottomBox = new Rectangle(midBottom.X, midBottom.Bottom - 100, midBottom.Width, 100);
            var bottomPoint = imageHelper.midPoint(deviceId, bottomBox);
            adbTask.tap(deviceId, bottomPoint);
            Thread.Sleep(1000);
            screenPath = facebookTool.captureScreen(deviceId);
            var confirmLeave = facebookTool.findAndTap(deviceId, IconName.FB_LITE_LEAVE_GROUP_CONFIRM_BUTTON);
            if (!confirmLeave.IsEmpty)
            {
                return true;
            }

            return false;
        }
        public bool openUrl(string deviceId, string packageName, string url)
        {
            facebookTool.closeAllApp(deviceId);
            Thread.Sleep(1000);
            adbTask.openUrl(deviceId, packageName, url);
            return true;
        }
    }
}
