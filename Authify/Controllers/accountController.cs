using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wlniao;

public class accountController : baseController
{

    /// <summary>
    /// 获取账户信息
    /// </summary>
    /// <returns></returns>
    public IActionResult getaccount()
    {
        try
        {
            return Context(Request, db, ctx =>
            {
                var body = GetPostString();
                var sid = PostRequest("sid");
                if (!ctx.Verify(body))
                {
                    return Json(new { Program.node, code = "402", success = false, message = "签名及请求时效性验证失败" });
                }
                else if (string.IsNullOrEmpty(sid))
                {
                    return Json(new { Program.node, code = "501", success = false, message = "未指定要获取的用户Sid" });
                }
                else
                {
                    var row = db.Account.Where(o => o.sid == sid).FirstOrDefault();
                    if (row==null)
                    {
                        return Json(new { Program.node, code = "501", success = false, message = "用户Sid无效，请重新指定" });
                    }
                    else
                    {
                        return Json(new
                        {
                            Program.node,
                            code = "0",
                            success = true,
                            message = "获取成功",
                            data = new
                            {
                                row.sid,
                                row.mobile,
                                row.avatar,
                                row.nickname,
                            }
                        });
                    }
                }
            });
        }
        catch (Exception ex)
        {
            return Json(new { Program.node, code = "-1", success = false, message = ex.Message });
        }
    }
    /// <summary>
    /// 发送或提交随机密码
    /// 参数：
    /// randpwd:由客户端发送的随机密码
    /// </summary>
    /// <returns>data无返回值</returns>
    public IActionResult sendrandpwd()
    {
        try
        {
            return Context(Request, db, ctx =>
            {
                var body = GetPostString();
                var sender = PostRequest("sender");
                var mobile = PostRequest("mobile");
                var randpwd = PostRequest("randpwd");
                if (!ctx.Verify(body))
                {
                    return Json(new { Program.node, code = "402", success = false, message = "签名及请求时效性验证失败" });
                }
                else if (string.IsNullOrEmpty(mobile))
                {
                    return Json(new { Program.node, code = "501", success = false, message = "手机号获取失败" });
                }
                else if (!strUtil.IsMobile(mobile))
                {
                    return Json(new { Program.node, code = "502", success = false, message = "手机号格式无效" });
                }
                else if (string.IsNullOrEmpty(randpwd))
                {
                    randpwd = Wlniao.Cache.Get("account_sendrandpwd_" + mobile);
                    if (!string.IsNullOrEmpty(randpwd))
                    {
                        return Json(new { Program.node, code = "504", success = false, message = "随机密码发送太频繁，请稍后再试" });
                    }
                    else
                    {
                        randpwd = strUtil.CreateRndStr(6);

                        var sendResult = false;
                        if (string.IsNullOrEmpty(sender))
                        {
                            sendResult = Cloopen.SendSMS(Cloopen.CloopenSid, Cloopen.CloopenToken, Cloopen.CloopenAppId, Cloopen.CloopenTemplateId, mobile, randpwd);
                        }
                        else
                        {
                            sendResult = Cloopen.SendSMS(Cloopen.CloopenSid, Cloopen.CloopenToken, Cloopen.CloopenAppId, Cloopen.CloopenTemplateIdBySender, mobile, randpwd, sender);
                        }
                        if (sendResult)
                        {
                            Wlniao.Cache.Set("account_randpwd_" + mobile, randpwd, 600);
                            Wlniao.Cache.Set("account_sendrandpwd_" + mobile, randpwd, 60);
                            return Json(new { Program.node, code = "0", success = true, message = "随机密码发送成功，十分钟内有效" });
                        }
                        else
                        {
                            return Json(new { Program.node, code = "500", success = false, message = "随机密码发送失败，短信网关错误" });
                        }
                    }
                }
                else
                {
                    if (Wlniao.Cache.Set("account_randpwd_" + mobile, randpwd, 180))
                    {
                        return Json(new { Program.node, code = "0", success = true, message = "随机密码提交成功" });
                    }
                    else
                    {
                        return Json(new { Program.node, code = "500", success = false, message = "提交失败，内部异常" });
                    }
                }
            });
        }
        catch (Exception ex)
        {
            return Json(new { Program.node, code = "-1", success = false, message = ex.Message });
        }
    }
    /// <summary>
    /// 校验随机密码(十分钟内)
    /// </summary>
    /// <returns>成功时data返回用户Sid</returns>
    public IActionResult checkrandpwd()
    {
        try
        {
            return Context(Request, db, ctx =>
            {
                var body = GetPostString();
                var mobile = PostRequest("mobile");
                var randpwd = PostRequest("randpwd");
                if (!ctx.Verify(body))
                {
                    return Json(new { Program.node, code = "402", success = false, message = "签名及请求时效性验证失败" });
                }
                else if (string.IsNullOrEmpty(mobile))
                {
                    return Json(new { Program.node, code = "501", success = false, message = "手机号获取失败" });
                }
                else if (!strUtil.IsMobile(mobile))
                {
                    return Json(new { Program.node, code = "502", success = false, message = "手机号格式无效" });
                }
                else
                {
                    var sendrandpwd = Wlniao.Cache.Get("account_randpwd_" + mobile);
                    if (string.IsNullOrEmpty(sendrandpwd))
                    {
                        return Json(new { Program.node, code = "503", success = false, message = "请先获取随机密码" });
                    }
                    else if (string.IsNullOrEmpty(randpwd))
                    {
                        return Json(new { Program.node, code = "504", success = false, message = "随机密码获取失败" });
                    }
                    else if (randpwd != sendrandpwd)
                    {
                        return Json(new { Program.node, code = "505", success = false, message = "随机密码错误" });
                    }
                    else
                    {
                        var account = Models.Account.GetOrCreate(mobile, db);
                        if (string.IsNullOrEmpty(account.source))
                        {
                            account.source = PostRequest("source");
                        }
                        account.lastlogin = DateTools.GetUnix();
                        db.Update(account);
                        db.SaveChangesAsync();
                        return Json(new { Program.node, code = "0", success = true, message = "随机密码验证通过", data = account.sid });
                    }
                }
            });
        }
        catch (Exception ex)
        {
            return Json(new { Program.node, code = "-1", success = false, message = ex.Message });
        }

    }
}