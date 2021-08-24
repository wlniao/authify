using System;
using System.Collections.Generic;
namespace Wlniao.Authify.Request
{
    /// <summary>
    /// 验证随机密码 的请求参数
    /// </summary>
    public class CheckRandPwdRequest : Wlniao.Handler.IRequest
    {
        /// <summary>
        /// 要验证的手机号码
        /// </summary>
        public string mobile { get; set; }
        /// <summary>
        /// 要验证的随机密码
        /// </summary>
        public string randpwd { get; set; }
    }
}