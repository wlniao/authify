using System;
using System.Collections.Generic;
namespace Wlniao.Authify.Response
{
    /// <summary>
    /// 用户账号信息
    /// </summary>
    public class Account
    {
        public string sid { get; set; }
        /// <summary>
        /// 手机号 
        /// </summary>
        public string mobile { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string avatar { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string nickname { get; set; }
    }
}