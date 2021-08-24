using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Wlniao.Authify
{
    /// <summary>
    /// 请求线程
    /// </summary>
    public class Context : Wlniao.Handler.IContext
    {
        /// <summary>
        /// ApiResult消息节点
        /// </summary>
        public string Node = "sdk-authify";
        /// <summary>
        /// 请求的地址
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// Authify平台开发密钥
        /// </summary>
        public string Secret { get; set; }
        /// <summary>
        /// HTTP请求方式
        /// </summary>
        public System.Net.Http.HttpMethod Method { get; set; }
        /// <summary>
        /// 要调用的API操作
        /// </summary>
        public string Operation { get; set; }
        /// <summary>
        /// 要调用的路径
        /// </summary>
        public string RequestPath { get; set; }
        /// <summary>
        /// 要发送的请求内容
        /// </summary>
        public Wlniao.Handler.IRequest Request { get; set; }
        /// <summary>
        /// API的输出内容
        /// </summary>
        public Wlniao.Handler.IResponse Response { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Task<System.Net.Http.HttpResponseMessage> HttpTask;
        /// <summary>
        /// 请求使用的证书
        /// </summary>
        public System.Security.Cryptography.X509Certificates.X509Certificate Certificate;
        /// <summary>
        /// 输出的状态
        /// </summary>
        public System.Net.HttpStatusCode StatusCode = System.Net.HttpStatusCode.Created;
        /// <summary>
        /// 请求的Headers参数
        /// </summary>
        public Dictionary<String, String> HttpRequestHeaders;
        /// <summary>
        /// 输出的Headers参数
        /// </summary>
        public Dictionary<String, String> HttpResponseHeaders;
        /// <summary>
        /// 请求的内容
        /// </summary>
        public string HttpRequestString { get; set; }
        /// <summary>
        /// 输出的内容
        /// </summary>
        public byte[] HttpResponseBody { get; set; }
        /// <summary>
        /// 输出的内容
        /// </summary>
        public string HttpResponseString { get; set; }
        /// <summary>
        /// 请求重试次数
        /// </summary>
        public int Retry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Context()
        {
            Retry = 0;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Sign()
        {
            var time = DateTools.GetUnix().ToString();
            var sign = Wlniao.Encryptor.GetSHA1(HttpRequestString + time + this.Secret);
            this.RequestPath = this.Operation + "?timestamp=" + time + "&sign=" + sign;
        }
    }
}