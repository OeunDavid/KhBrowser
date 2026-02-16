using OtpNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolKHBrowser.Helper
{
    public static class TwoFaHelper
    {
        public static string GenerateCode(string secret)
        {
            if (string.IsNullOrEmpty(secret))
                return "";

            secret = secret.Replace(" ", "").Trim();

            var secretBytes = Base32Encoding.ToBytes(secret);
            var totp = new Totp(secretBytes);

            return totp.ComputeTotp(); // 6-digit
        }
    }
}
