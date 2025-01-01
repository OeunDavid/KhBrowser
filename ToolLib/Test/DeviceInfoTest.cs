using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolLib.Data;

namespace ToolLib.Test
{
    public class DeviceInfoTest
    {
        public static void testDeviceInfo()
        {
            DataDao dataDao = new DataDao();
            DeviceDao deviceDao = new DeviceDao(dataDao);
            var deviceInfo = ToolDiConfig.Get<IDeviceInfo>();
            deviceInfo.start();
            List<Device> deviceItems = deviceDao.listDevice();
            while (true)
            {
           
                var devices = deviceInfo.getList();
                
                //if( devices.Count == 0)
                //{
                //    Console.WriteLine("No devices found...");
                //}
                //foreach(var item in devices)
                //{
                //    Console.WriteLine("device " + item.Key + " -> " + item.Value);
                //}
                foreach(var d in deviceItems)
                {
                    string status = "";
                    if( devices.ContainsKey( d.DeviceID))
                    {
                        var c = devices[d.DeviceID];
                         status = c == "device" ? "online" : c;
                    }
                    else
                    {
                        status = "offline";
                    }
                    Console.WriteLine(" device : " + d.Id + " -> " + d.Description + "  [" + status + "]");
                }
                Thread.Sleep(200);
            }

        }
    }
}
