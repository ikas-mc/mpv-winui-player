using System;
using System.Security.Cryptography;
using System.Text;

namespace mpv_winui.Modules.Common.Utils
{
    public static class HashUtil
    {
        public static string ComputeMd5(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(value)));
        }
    }
}
