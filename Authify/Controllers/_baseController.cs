using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Wlniao;

/// <summary>
/// 
/// </summary>
public partial class baseController : Wlniao.XCoreController
{
    public MyContext db = new MyContext();
    /// <summary>
    /// 验证用户是否登录
    /// </summary>
    /// <param name="db"></param>
    /// <param name="token"></param>
    /// <param name="action"></param>
    /// <param name="message"></param>
    [NonAction]
    public IActionResult Context(Microsoft.AspNetCore.Http.HttpRequest req, MyContext db, Func<Cache.Context, IActionResult> func)
    {
        var organ = db.Organ.Where(o => o.domain == req.Host.Host || o.domain.Contains("," + req.Host.Host + ",")).OrderBy(o => o.domain.Length).FirstOrDefault();
        if (organ == null)
        {
            return Json(new { node = XCore.WebNode, success = false, message = "请使用您的专有域名访问本服务" });
        }
        else
        {
            var ctx = new Cache.Context
            {
                Oid = organ.oid,
                Secret = organ.secret,
                ApiUrl = organ.apiurl,
                Redirect = organ.backurl,
                Timestamp = req.Query.ContainsKey("timestamp") ? Wlniao.Convert.ToLong(req.Query["timestamp"]) : 0,
                Sign = req.Query.ContainsKey("sign") ? req.Query["sign"].ToString().ToLower() : "",
                Cfg = new Dictionary<string, string>()
            };
            if (!string.IsNullOrEmpty(organ.config))
            {
                ctx.Cfg = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<String, String>>(organ.config);
            }
            return func?.Invoke(ctx);
        }
    }
}