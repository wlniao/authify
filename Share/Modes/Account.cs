using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wlniao;
namespace Models
{
    /// <summary>
    /// 账号信息
    /// </summary>
    [Table("authify_account")]
    public class Account
    {
        [Key]
        [Required]
        [StringLength(50)]
        public string sid { get; set; }
        /// <summary>
        /// 手机号 
        /// </summary>
        [StringLength(15)]
        public string mobile { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        [StringLength(150)]
        public string avatar { get; set; }
        /// <summary>
        /// 头像来源地址
        /// </summary>
        [StringLength(150)]
        public string avatarsource { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        [StringLength(30)]
        public string nickname { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        [StringLength(30)]
        public string truename { get; set; }
        /// <summary>
        /// 证件号
        /// </summary>
        [StringLength(25)]
        public string identity { get; set; }
        /// <summary>
        /// 登录密码
        /// </summary>
        [StringLength(100)]
        public string password { get; set; }
        /// <summary>
        /// 用户来源
        /// </summary>
        [StringLength(30)]
        public string source { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        public long jointime { get; set; }
        /// <summary>
        /// 最后登录
        /// </summary>
        public long lastlogin { get; set; }

        /// <summary>
        /// 组合显示的名称
        /// </summary>
        /// <returns></returns>
        public string ShowName()
        {
            var show = string.IsNullOrEmpty(truename) ? nickname : truename;
            if (!string.IsNullOrEmpty(mobile) && strUtil.IsMobile(mobile))
            {
                if (string.IsNullOrEmpty(show))
                {
                    show = mobile.Substring(0, 3) + "****" + mobile.Substring(7);
                }
                else
                {
                    show += "(" + mobile.Substring(0, 3) + "****" + mobile.Substring(7) + ")";
                }
            }
            return show;
        }
        /// <summary>
        /// 显示****手机号
        /// </summary>
        /// <returns></returns>
        public string ShowMobile()
        {
            if (string.IsNullOrEmpty(mobile) || mobile.Length != 11)
            {
                return "";
            }
            else
            {
                return mobile.Substring(0, 3) + "****" + mobile.Substring(7);
            }
        }
        /// <summary>
        /// 显示****证件号
        /// </summary>
        /// <returns></returns>
        public string ShowIdentity()
        {
            if (string.IsNullOrEmpty(identity) || identity.Length != 18)
            {
                return "";
            }
            else
            {
                return identity.Substring(0, 4) + "**********" + identity.Substring(14);
            }
        }

        public static Account Create(String identity, String mobile, String name, MyContext db = null)
        {
            if (db == null)
            {
                db = new MyContext();
            }
            var row = db.Account.Where(a => a.identity == identity).OrderBy(a => a.jointime).FirstOrDefault();
            if (row == null)
            {
                row = new Account { identity = identity, mobile = mobile, truename = name, jointime = DateTools.GetUnix() };
                db.Add(row);
                db.SaveChanges();
            }
            return row;
        }
        public static Account GetOrCreate(String mobile, MyContext db = null)
        {
            if (db == null)
            {
                db = new MyContext();
            }
            var row = db.Account.Where(a => a.mobile == mobile).OrderBy(o => o.jointime).FirstOrDefault();
            if (row == null)
            {
                row = new Account { mobile = mobile, jointime = DateTools.GetUnix() };
                row.sid = Wlniao.OpenApi.Sid.Get(mobile);
                db.Add(row);
                db.SaveChanges();
            }
            return row;
        }
    }
}