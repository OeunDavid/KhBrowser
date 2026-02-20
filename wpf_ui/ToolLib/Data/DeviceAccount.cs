using System;

namespace ToolLib.Data
{
    public class DeviceAccount
    {
        public string AccountId { get; set; }
        public string DeviceId { get; set; }
        public string UID { get; set; }
        public string DeviceName { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
