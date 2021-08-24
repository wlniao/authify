using System;
using System.Collections.Generic;
namespace Wlniao.Authify.Request
{
    /// <summary>
    /// 获取access_token 的请求参数
    /// </summary>
    public class SendRandPwdRequest : Wlniao.Handler.IRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public string sender { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string mobile { get; set; }
        /// <summary>
        /// 仅本地发送时向服务器提交随机密码，不为空时Authify将不发送随机密码
        /// </summary>
        public string randpwd { get; set; }
    }
}