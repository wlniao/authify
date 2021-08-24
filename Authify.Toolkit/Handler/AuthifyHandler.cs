using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Wlniao.Handler;
using Wlniao.Authify.Request;
using Wlniao.Authify.Response;
namespace Wlniao.Authify
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthifyHandler : PipelineHandler
    {
        private Dictionary<string, ResponseEncoder> EncoderMap;
        private Dictionary<string, ResponseDecoder> DecoderMap;
        private delegate void ResponseEncoder(Context ctx);
        private delegate void ResponseDecoder(Context ctx);

        /// <summary>
        /// 
        /// </summary>
        public AuthifyHandler(PipelineHandler handler) : base(handler)
        {
            EncoderMap = new Dictionary<string, ResponseEncoder>() {
                { "/account/sendrandpwd", SendRandPwdEncode },
                { "/account/checkrandpwd", CheckRandPwdEncode },
                { "/account/getaccount", GetAccountEncode },
            };
            DecoderMap = new Dictionary<string, ResponseDecoder>() {
                { "/account/sendrandpwd", SendRandPwdDecode },
                { "/account/checkrandpwd", CheckRandPwdDecode },
                { "/account/getaccount", GetAccountDecode },
            };
        }

        #region Handle
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        public override void HandleBefore(IContext ctx)
        {
            var _ctx = (Context)ctx;
            EncoderMap[_ctx.Operation](_ctx);
            inner.HandleBefore(ctx);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        public override void HandleAfter(IContext ctx)
        {
            inner.HandleAfter(ctx);
            var _ctx = (Context)ctx;
            DecoderMap[_ctx.Operation](_ctx);
        }
        #endregion

        #region SendRandPwd
        private void SendRandPwdEncode(Context ctx)
        {
            var req = ctx.Request as SendRandPwdRequest;
            if (string.IsNullOrEmpty(req.mobile))
            {
                ctx.Response = new ErrorResponse() { code = "501", message = "missing mobile" };
            }
            else
            {
                ctx.HttpRequestString = JsonConvert.SerializeObject(req);
                ctx.Sign();
            }
        }
        private void SendRandPwdDecode(Context ctx)
        {
            try
            {
                ctx.Response = JsonConvert.DeserializeObject<StringResponse>(ctx.HttpResponseString);
            }
            catch
            {
                ctx.Response = new ErrorResponse() { code = "403", message = "result not json format" };
            }
        }
        #endregion

        #region CheckRandPwd
        private void CheckRandPwdEncode(Context ctx)
        {
            var req = ctx.Request as CheckRandPwdRequest;
            if (string.IsNullOrEmpty(req.mobile))
            {
                ctx.Response = new ErrorResponse() { code = "501", message = "missing mobile" };
            }
            else if (string.IsNullOrEmpty(req.randpwd))
            {
                ctx.Response = new ErrorResponse() { code = "502", message = "missing randpwd" };
            }
            else
            {
                ctx.HttpRequestString = JsonConvert.SerializeObject(req);
                ctx.Sign();
            }
        }
        private void CheckRandPwdDecode(Context ctx)
        {
            try
            {
                var res = JsonConvert.DeserializeObject<StringResponse>(ctx.HttpResponseString);
                if (res.success && !string.IsNullOrEmpty(res.data))
                {
                    res.message = "Sid获取成功";
                }
                ctx.Response = res;
            }
            catch
            {
                ctx.Response = new ErrorResponse() { code = "403", message = "result not json format" };
            }
        }
        #endregion

        #region GetAccountEncode
        private void GetAccountEncode(Context ctx)
        {
            var req = ctx.Request as GetAccountRequest;
            if (string.IsNullOrEmpty(req.sid))
            {
                ctx.Response = new ErrorResponse() { code = "501", message = "sid mobile" };
            }
            else
            {
                ctx.HttpRequestString = JsonConvert.SerializeObject(req);
                ctx.Sign();
            }
        }
        private void GetAccountDecode(Context ctx)
        {
            try
            {
                var res = JsonConvert.DeserializeObject<ObjectResponse<Account>>(ctx.HttpResponseString);
                if (res.data == null || string.IsNullOrEmpty(res.data.sid))
                {
                    res.code = "-1";
                    res.success = false;
                }
                ctx.Response = res;
            }
            catch
            {
                ctx.Response = new ErrorResponse() { code = "403", message = "result not json format" };
            }
        }
        #endregion

    }
}