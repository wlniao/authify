using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wlniao;
namespace Models
{
    /// <summary>
    /// 组织机构信息
    /// </summary>
    [Table("authify_organ")]
    public class Organ
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int oid { get; set; }
        /// <summary>
        /// 签名密钥
        /// </summary>
        [StringLength(50)]
        public string secret { get; set; }
        /// <summary>
        /// 来访域名
        /// </summary>
        [StringLength(50)]
        public string domain { get; set; }
        /// <summary>
        /// 通讯接口地址
        /// </summary>
        [StringLength(100)]
        public string apiurl { get; set; }
        /// <summary>
        /// 回调跳转地址
        /// </summary>
        [StringLength(100)]
        public string backurl { get; set; }

        /// <summary>
        /// 配置信息Json文本
        /// </summary>
        public string config { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="apiurl"></param>
        /// <param name="backurl"></param>
        /// <param name="kvs"></param>
        public static void Set(string domain, string apiurl, string backurl, params KeyValuePair<string, string>[] kvs)
        {
            var ht = new Hashtable();
            foreach (var kv in kvs)
            {
                ht.Add(kv.Key, kv.Value);
            }
            var db = new MyContext();
            var row = db.Organ.Where(o => domain.Contains(o.domain)).OrderByDescending(o => o.domain.Length).FirstOrDefault();
            if (row == null)
            {
                row = new Organ { domain = domain, secret = strUtil.CreateRndStrE(32) };
            }
            row.apiurl = apiurl;
            row.backurl = backurl;
            row.config = Newtonsoft.Json.JsonConvert.SerializeObject(ht);
            if (row.oid > 0)
            {
                db.Update(row);
            }
            else
            {
                db.Add(row);
            }
            db.SaveChanges();
        }
    }
}