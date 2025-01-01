using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolLib.Data;

namespace ToolLib
{

    public interface IDeviceInfo
    {
        void start();
        void stop();
        Dictionary<string, string> getList();
        void facebookId(string deviceId, string fbId);
        void removeFacebook(string deviceId);
        bool isKeyForText(string deviceId);
    }
    public class DeviceInfo:IDeviceInfo

    {
        public const int DEVICE_DEFAULT_WIDTH = 720; // resize device screen;
        public static readonly string SCREEN_PATH = System.Environment.CurrentDirectory + "/screen";
        public static readonly string ICON_PATH = System.Environment.CurrentDirectory + "/icon";
        public static readonly string TMP_PATH = System.Environment.CurrentDirectory + "/tmp";
        public static readonly string FB_LITE_SOURCE = System.Environment.CurrentDirectory + "/fblites";
        private AdbTask adbTask = new AdbTask();
        private Dictionary<string, string> deviceList = new Dictionary<string, string>();
        private static readonly Dictionary<string, int> DEVICE_SCREEN = new Dictionary<string, int>();
        private static readonly Dictionary<string, string> DEVICE_FACEBOOK = new Dictionary<string, string>();
        private static readonly Dictionary<string, Device> DEVICE_MAP = new Dictionary<string, Device>();
        
        private IDeviceDao deviceDao;
        public DeviceInfo(IDeviceDao deviceDao)
        {
            this.deviceDao = deviceDao;
        }
        public static void putScreen(string deviceId, int width)
        {
            DEVICE_SCREEN[deviceId] = width;
        }
        public static int deviceScreen(string deviceId )
        {
            if( DEVICE_SCREEN.ContainsKey( deviceId))
            {
                return DEVICE_SCREEN[deviceId];
            }
            return 0;
        }
        public void start()
        {
            if(!Directory.Exists(SCREEN_PATH))
            {
                Directory.CreateDirectory(SCREEN_PATH);
            }
            Thread thread = new Thread(run);
            thread.Start();
            
        }
        public void run()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                getList();
                Thread.Sleep(10000);
            }
        }
        
        public void stop()
        {
            Thread.CurrentThread.Interrupt();
        }
        private void mapDevice()
        {
            DEVICE_MAP.Clear();
               var items = deviceDao.listDevice();
            foreach(var item in items)
            {
                DEVICE_MAP[item.DeviceID] = item;
            }
        }
        public Dictionary<string,string> getList()
        {
            string des =  adbTask.deviceList();
            string[] deviceIds = des.Split('\n');
            deviceList.Clear();

            mapDevice();
            foreach( var id in deviceIds)
            {
                if (!string.IsNullOrEmpty(id.Trim()))
                {
                    var items = id.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    var deviceId = items[0].Trim();
                    var status = items[1].Trim();
                    deviceList[deviceId] = status;
                }
            }

            return deviceList;
        }
        public void facebookId(string deviceId, string fbId)
        {
            DEVICE_FACEBOOK[deviceId] = fbId;
        }
        public void removeFacebook(string deviceId)
        {
            DEVICE_FACEBOOK.Remove(deviceId);
        }

        public bool isKeyForText(string deviceId)
        {
            if( DEVICE_MAP.ContainsKey(deviceId))
            {
                return DEVICE_MAP[deviceId].KeyPressForText == 1;
            }
            return false
                ;
        }
    }
}
