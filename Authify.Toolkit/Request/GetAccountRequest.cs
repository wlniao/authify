using System;
using System.Collections.Generic;
namespace Wlniao.Authify.Request
{
    /// <summary>
    /// 获取用户信息 的请求参数
    /// </summary>
    public class GetAccountRequest : Wlniao.Handler.IRequest
    {
        /// <summary>
        /// 要获取的用户Sid
        /// </summary>
        public string sid { get; set; }
    }
}