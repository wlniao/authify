using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wlniao;

public class authController : baseController
{
    public IActionResult home()
    {
        var res = "欢迎使用Authify";
        var host = "," + Request.Host.Host + ",";
        var organ = db.Organ.Where(o => o.domain.Contains(host)).OrderBy(o => o.domain.Length).FirstOrDefault();
        if (organ == null)
        {
            organ = new Models.Organ
            {
                domain = host,
                secret = strUtil.CreateRndStrE(16).ToLower(),
                apiurl = "",
                backurl = "",
                config = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    wx_appid = "",
                    wx_secret = "",
                    wx_corp_appid = "",
                    wx_corp_secret = "",
                })
            };
            res += "<br/>初始化Secret：" + organ.secret;
            db.Add(organ);
            db.SaveChanges();
        }
        return Content(res, "text/html", Encoding.UTF8);
    }
    [Route("install")]
    public IActionResult install()
    {
        try
        {
            using (var db = new MyContext())
            {
                db.Database.Migrate();
            }
            return Content("Database migrate success.", "text/html", Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Wlniao.log.Error("Database migrate error: " + ex.Message);
            return Content("Database migrate error!", "text/html", Encoding.UTF8);
        }
    }

    [Route("authify")]
    [Route("authify/{path0}")]
    [Route("authify/{path0}/{path1}")]
    [Route("authify/{path0}/{path1}/{path2}")]
    [Route("authify/{path0}/{path1}/{path2}/{path3}")]
    public IActionResult authify()
    {
        return Context(Request, db, ctx =>
        {
            var platform = Authify.Utils.Helper.GetPlatform(UserAgent);
            if (platform == "weixin"|| platform == "wxwork")
            {
                var appid = platform == "wxwork" ? ctx.Cfg.GetString("wx_corp_appid") : "";
                var code = GetRequest("code");
                if (string.IsNullOrEmpty(code))
                {
                    if (string.IsNullOrEmpty(appid))
                    {
                        platform = "weixin";
                        appid = ctx.Cfg.GetString("wx_appid");
                    }
                    var base_path = Request.Path.Value.Replace("/authify", "");
                    var enter_path = GetRequest("scope", "snsapi_base") == "snsapi_userinfo" ? "/authify/wxsync" : "/authify";
                    return Redirect("https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appid +
                          "&redirect_uri=" + strUtil.UrlEncode((IsHttps ? "https://" : "http://") + Request.Host.Value + enter_path + base_path) +
                          "&response_type=code&scope=snsapi_base&state=STATE#wechat_redirect");
                }
                try
                {
                    var secret = platform == "wxwork" ? ctx.Cfg.GetString("wx_corp_secret") : "";
                    if (string.IsNullOrEmpty(appid) || string.IsNullOrEmpty(secret))
                    {
                        platform = "weixin";
                        appid = ctx.Cfg.GetString("wx_appid");
                        secret = ctx.Cfg.GetString("wx_secret");
                    }
                    if (platform == "wxwork")
                    {
                        var accesstoken = Models.AccessToken.GetToken(platform, appid, secret, db);
                        var jsonDic = Wlniao.Json.StringToDic(Wlniao.XServer.Common.GetResponseString("https://qyapi.weixin.qq.com/cgi-bin/user/getuserinfo?access_token=" + accesstoken + "&code=" + code));
                        var userid = jsonDic.GetString("UserId").Replace('/', '_');
                        var openid = jsonDic.GetString("OpenId");
                        var external_userid = jsonDic.GetString("external_userid");
                        var jsonstr = Wlniao.Json.ToString(new { userid, openid, external_userid });
                        if (string.IsNullOrEmpty(userid))
                        {
                            userid = openid;
                        }
                        return authify(ctx, "wxwork", userid, jsonstr, "", "");
                    }
                    else
                    {
                        var jsonDic = Wlniao.Json.StringToDic(Wlniao.XServer.Common.GetResponseString("https://api.weixin.qq.com/sns/oauth2/access_token?appid=" + appid + "&secret=" + secret + "&code=" + code + "&grant_type=authorization_code"));
                        var openid = jsonDic.GetString("openid");
                        var info_avatar = "";
                        var info_nickname = "";
                        if (jsonDic.GetString("scope") == "snsapi_userinfo")
                        {
                            jsonDic = Wlniao.Json.StringToDic(Wlniao.XServer.Common.GetResponseString("https://api.weixin.qq.com/sns/userinfo?access_token=" + jsonDic.GetString("access_token") + "&openid=" + openid + "&lang=zh_CN"));
                            info_avatar = jsonDic.GetString("headimgurl");
                            info_nickname = jsonDic.GetString("nickname");
                        }
                        return authify(ctx, "weixin", openid, "{}", info_avatar, info_nickname);
                    }
                }
                catch
                {
                    return ErrorMsg("访问异常，请稍后再试");
                }
            }
            else
            {
                return ErrorMsg("暂不支持您的访问环境");
            }
        });
    }

    [NonAction]
    public IActionResult authify(Cache.Context ctx, string platform, string userid, string jsonstr, string info_avatar, string info_nickname)
    {
        if (string.IsNullOrEmpty(userid))
        {
            return ErrorMsg("获取用户授权信息失败");
        }
        var now = DateTools.GetUnix();
        var key = platform + "_" + userid;
        var save = 0;
        var auth = db.Auth.Where(o => o.key == key).FirstOrDefault();
        if (auth == null)
        {
            save = 1000;
            auth = new Models.Auth { oid = ctx.Oid, key = key, create = now };
            auth.avatar = "";
            auth.nickname = "";
        }
        if (auth.jsonstr != jsonstr && jsonstr.IsNotNullAndEmpty())
        {
            save++;
            auth.jsonstr = jsonstr;
        }
        if (auth.avatar != info_avatar && info_avatar.IsNotNullAndEmpty())
        {
            save++;
            auth.avatar = info_avatar;
            Wlniao.XServer.XStorage.SaveUrl(auth.AvatarPath(), info_avatar);
        }
        if (auth.nickname != info_nickname && info_nickname.IsNotNullAndEmpty())
        {
            save++;
            auth.nickname = info_nickname;
        }

        if (save > 0)
        {
            if (save > 999)
            {
                db.Add(auth);
            }
            else
            {
                db.Update(auth);
            }
            save = db.SaveChanges();
        }
        var rlt = ctx.Got<String>(new { method = "get_token", userid, platform });
        if (rlt.success)
        {
            if (!string.IsNullOrEmpty(ctx.Redirect) && ctx.Redirect.EndsWith('/'))
            {
                ctx.Redirect = ctx.Redirect.TrimEnd('/');
            }
            var path = Request.Path.Value.Replace("/authify", "").Replace("/sync", "");
            path = string.IsNullOrEmpty(path) ? "/" : path;
            ViewBag.Token = rlt.data;
            ViewBag.Redirect = ctx.Redirect + path;
            return View("~/Views/authify.cshtml");
        }
        else
        {
            return ErrorMsg(rlt.message);
        }
    }

    /// <summary>
    /// 需要授权微信头像等信息时，跳转此页面判断是否需要二次跳转
    /// </summary>
    /// <returns></returns>
    [Route("authify/sync")]
    [Route("authify/sync/{path0}")]
    [Route("authify/sync/{path0}/{path1}")]
    [Route("authify/sync/{path0}/{path1}/{path2}")]
    public IActionResult sync()
    {
        return Context(Request, db, ctx =>
        {
            try
            {
                var code = GetRequest("code");
                var appid = ctx.Cfg.GetString("wx_appid");
                var secret = ctx.Cfg.GetString("wx_secret");
                var jsonDic = Wlniao.Json.StringToDic(Wlniao.XServer.Common.GetResponseString("https://api.weixin.qq.com/sns/oauth2/access_token?appid=" + appid + "&secret=" + secret + "&code=" + code + "&grant_type=authorization_code"));
                var userid = jsonDic.GetString("openid");
                if (!string.IsNullOrEmpty(userid))
                {
                    var key = "weixin_" + userid;
                    var auth = db.Auth.Where(o => o.key == key).FirstOrDefault();
                    if (auth == null || string.IsNullOrEmpty(auth.nickname))
                    {
                        return Redirect("https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + appid +
                              "&redirect_uri=" + strUtil.UrlEncode((IsHttps ? "https://" : "http://") + Request.Host.Value + Request.Path.Value.Replace("/authify/sync", "/authify")) +
                              "&response_type=code&scope=snsapi_userinfo&state=STATE#wechat_redirect");
                    }
                }
                return authify(ctx, "weixin", userid, "{}", "", "");
            }
            catch
            {
                return ErrorMsg("访问异常，请稍后再试");
            }
        });
    }
}