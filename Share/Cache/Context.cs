using System;
using System.Collections.Generic;
using System.Text;
using Wlniao;
namespace Cache
{
    public class Context
    {
        /// <summary>
        /// 机构Id
        /// </summary>
        public int Oid { get; set; }
        /// <summary>
        /// 当前时间
        /// </summary>
        public long Now = Wlniao.DateTools.GetUnix();
        /// <summary>
        /// 签名密钥
        /// </summary>
        public string Secret { get; set; }
        /// <summary>
        /// 通讯接口地址
        /// </summary>
        public string ApiUrl { get; set; }
        /// <summary>
        /// 回调跳转地址
        /// </summary>
        public string Redirect { get; set; }
        /// <summary>
        /// 请求时间戳
        /// </summary>
        public long Timestamp { get; set; }
        /// <summary>
        /// 请求签名字符串
        /// </summary>
        public string Sign { get; set; }
        /// <summary>
        /// 配置信息
        /// </summary>
        public Dictionary<string, string> Cfg { get; set; }

        /// <summary>
        /// 请求有效性验证
        /// </summary>
        /// <returns></returns>
        public Boolean Verify(String body)
        {
            if (Timestamp - 180 > Now || Timestamp + 600 < Now)
            {
                return false;
            }
            else
            {
                return Sign == Wlniao.Encryptor.GetSHA1(body + Timestamp + Secret).ToLower();
            }
        }
        /// <summary>
        /// 请求有效性验证
        /// </summary>
        /// <returns></returns>
        public Boolean Verify()
        {
            return Verify("");
        }
        /// <summary>
        /// 请求接口通讯
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public ApiResult<T> Got<T>(Object obj)
        {
            return Got<T>(Newtonsoft.Json.JsonConvert.SerializeObject(obj));
        }
        /// <summary>
        /// 请求接口通讯
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public ApiResult<T> Got<T>(String data)
        {
            try
            {
                var sign = Encryptor.Md5Encryptor32(data + Now + Secret);
                var jsonStr = Wlniao.XServer.Common.PostResponseString(ApiUrl + "?time=" + Now + "&sign=" + sign, data);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResult<T>>(jsonStr);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return new ApiResult<T>() { message = "系统错误，请稍后再试<br/>authify got error" };
            }
        }
    }
}
