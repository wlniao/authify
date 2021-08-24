using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wlniao.Handler;
using Wlniao.Authify.Request;
using Wlniao.Authify.Response;

namespace Wlniao.Authify
{
    /// <summary>
    /// 企业微信内部开发客户端
    /// </summary>
    public class Client : Wlniao.Handler.IClient
    {
        #region 开发配置信息
        private static string _Host = null;
        private static string _Secret = null;
        /// <summary>
        /// Authify平台地址
        /// </summary>
        private static string CfgHost
        {
            get
            {
                if (_Host == null)
                {
                    _Host = Config.GetSetting("AuthifyHost");
                }
                return _Host;
            }
        }
        /// <summary>
        /// Authify平台开发密钥
        /// </summary>
        private static string CfgSecret
        {
            get
            {
                if (_Secret == null)
                {
                    _Secret = Config.GetSetting("AuthifySecret");
                }
                return _Secret;
            }
        }
        #endregion

        /// <summary>
        /// Authify平台地址
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// Authify平台开发密钥
        /// </summary>
        public string Secret { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Handler handler = null;
        /// <summary>
        /// 
        /// </summary>
        public Client()
        {
            this.Host = CfgHost;
            this.Secret = CfgSecret;
            handler = new Handler();
        }
        /// <summary>
        /// 
        /// </summary>
        public Client(String Secret)
        {
            this.Secret = Secret;
            handler = new Handler();
        }
        /// <summary>
        /// 
        /// </summary>
        public Client(String Host, String Secret)
        {
            this.Host = Host;
            this.Secret = Secret;
            handler = new Handler();
        }


        private Task<TResponse> CallAsync<TRequest, TResponse>(string operation, TRequest request, System.Net.Http.HttpMethod method = null)
            where TResponse : Wlniao.Handler.IResponse, new()
            where TRequest : Wlniao.Handler.IRequest
        {
            if (request == null)
            {
                throw new ArgumentNullException();
            }

            var ctx = new Context() { Host = Host };
            ctx.Secret = Secret;
            ctx.Operation = operation;
            ctx.Method = method == null ? System.Net.Http.HttpMethod.Post : method;
            ctx.Request = request;

            handler.HandleBefore(ctx);
            try
            {
                if (ctx.Response == null)
                {
                    ctx.HttpTask.ContinueWith((t) =>
                    {
                        handler.HandleAfter(ctx);
                    }).Wait();
                }
                if (ctx.Response is ErrorResponse)
                {
                    return Task<TResponse>.Run(() =>
                    {
                        var err = ctx.Response as ErrorResponse;
                        if (string.IsNullOrEmpty(err.node))
                        {
                            err.node = ctx.Node;
                        }
                        var str = Newtonsoft.Json.JsonConvert.SerializeObject(err);
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<TResponse>(str);
                    });
                }
                else if (ctx.Response is TResponse)
                {
                    return Task<TResponse>.Run(() =>
                    {
                        return (TResponse)ctx.Response;
                    });
                }
                else
                {
                    throw new Exception("unkown error");
                }
            }
            catch (Exception ex)
            {
                return Task<TResponse>.Run(() =>
                {
                    var str = Newtonsoft.Json.JsonConvert.SerializeObject(new { node = ctx.Node, code = "-1", success = false, message = ex.Message });
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<TResponse>(str);
                });
            }
        }

        private TResponse GetResponseFromAsyncTask<TResponse>(Task<TResponse> task)
        {
            try
            {
                task.Wait();
            }
            catch (System.AggregateException e)
            {
                log.Error(e.Message);
                throw e.GetBaseException();
            }

            return task.Result;
        }


        #region GetAccount
        /// <summary>
        /// 获取用户信息
        /// </summary>
        public ObjectResponse<Account> GetAccount(String sid)
        {
            var res = GetResponseFromAsyncTask(CallAsync<GetAccountRequest, ObjectResponse<Account>>("/account/getaccount"
                , new GetAccountRequest() { sid = sid }));
            return res;
        }
        #endregion

        #region SendRandPwd
        /// <summary>
        /// 发送随机密码
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="randpwd"></param>
        /// <returns></returns>
        public StringResponse SendRandPwd(String mobile, String randpwd = "")
        {
            return SendRandPwd(mobile, "", randpwd);
        }
        /// <summary>
        /// 发送随机密码
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="sender"></param>
        /// <param name="randpwd"></param>
        /// <returns></returns>
        public StringResponse SendRandPwd(String mobile, String sender, String randpwd = "")
        {
            var res = GetResponseFromAsyncTask(CallAsync<SendRandPwdRequest, StringResponse>("/account/sendrandpwd"
                , new SendRandPwdRequest() { mobile = mobile, sender = sender, randpwd = randpwd }));
            return res;
        }
        #endregion 

        #region CheckRandPwd
        /// <summary>
        /// 验证随机密码
        /// </summary>
        public StringResponse CheckRandPwd(String mobile, String randpwd)
        {
            var res = GetResponseFromAsyncTask(CallAsync<CheckRandPwdRequest, StringResponse>("/account/checkrandpwd"
                , new CheckRandPwdRequest() { mobile = mobile, randpwd = randpwd }));
            return res;
        }
        #endregion
    }
}