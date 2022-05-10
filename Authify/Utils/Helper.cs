using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authify.Utils
{
    public class Helper
    {
        /// <summary>
        /// 获取当前访问使用的平台
        /// </summary>
        /// <param name="UserAgent"></param>
        /// <returns></returns>
        public static string GetPlatform(String UserAgent)
        {
            var ua = UserAgent.ToLower();
            if (ua.Contains("wxwork") && ua.Contains("micromessenger"))
            {
                return "wxwork";
            }
            else if (ua.Contains("micromessenger"))
            {
                return "weixin";
            }
            else
            {
                return "other";
            }
        }
    }
}
