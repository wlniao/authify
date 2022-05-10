using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wlniao;
namespace Models
{
    /// <summary>
    /// 平台访问密钥
    /// </summary>
    [Table("authify_accesstoken")]
    public class AccessToken
    {
        [Key]
        [Required]
        [StringLength(50)]
        public string key { get; set; }
        /// <summary>
        /// 密钥
        /// </summary>
        [StringLength(512)]
        public string token { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public long expires_in { get; set; }
        /// <summary>
        /// 返回消息
        /// </summary>
        public string response { get; set; }

        /// <summary>
        /// 获取Token
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="appid"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static string GetToken(String platform, String appid, String secret, MyContext db = null)
        {
            MyContext rw = null;
            if (db == null)
            {
                rw = new MyContext();
                db = rw;
            }
            var add = false;
            var key = platform + "_" + appid + "_" + Encryptor.Md5Encryptor16(secret).ToLower();
            var now = DateTools.GetUnix();
            var row = db.AccessToken.Where(o => o.key == key).FirstOrDefault();
            if (row == null)
            {
                add = true;
                row = new AccessToken { key = key, token = "", expires_in = 0 };
            }
            if (row.expires_in < now)
            {
                if (platform == "wxwork")
                {
                    row.response = Wlniao.XServer.Common.GetResponseString("https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid=" + appid + "&corpsecret=" + secret);
                    var jsonDic = Wlniao.Json.StringToDic(row.response);
                    row.token = jsonDic.GetString("access_token");
                    row.expires_in = now + jsonDic.GetInt32("expires_in");
                }
                if (row.expires_in > now)
                {
                    if (rw == null)
                    {
                        rw = new MyContext();
                    }
                    if (add)
                    {
                        rw.Add(row);
                    }
                    else
                    {
                        rw.Update(row);
                    }
                    rw.SaveChanges();
                }
            }
            return row.token;
        }
    }
}