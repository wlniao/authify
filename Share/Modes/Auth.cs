using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wlniao;
namespace Models
{
    /// <summary>
    /// 收取信息
    /// </summary>
    [Table("authify_auth")]
    public class Auth
    {
        /// <summary>
        /// 所属域
        /// </summary>
        public int oid { get; set; }
        [Key]
        [Required]
        [StringLength(50)]
        public string key { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        [StringLength(512)]
        public string avatar { get; set; }
        /// <summary>
        /// 用户昵称
        /// </summary>
        [StringLength(50)]
        public string nickname { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public long create { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public long update { get; set; }

        /// <summary>
        /// 获取头像路径
        /// </summary>
        /// <returns></returns>
        public string AvatarPath()
        {
            return "/avatar/" + DateTools.FormatUnix(create, "yyyyMM") + "/" + key + ".jpg";
        }
    }
}