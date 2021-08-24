using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Wlniao;
/// <summary>
/// Cloopen云通讯
/// </summary>
public class Cloopen
{
    internal static String CloopenSid = Config.GetSetting("CloopenSid");
    internal static String CloopenToken = Config.GetSetting("CloopenToken");
    internal static String CloopenAppId = Config.GetSetting("CloopenAppId");
    internal static String CloopenTemplateId = Config.GetSetting("CloopenTemplateId");
    internal static String CloopenTemplateIdBySender = Config.GetSetting("CloopenTemplateIdBySender");
    internal const String ApiUrl = "https://app.cloopen.com:8883/2013-12-26";

    public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }
    public static String Post(string cloopenSid, string cloopenToken, string method, string postData, string contentType = "application/json")
    {
        //生成sig
        byte[] md5_result = System.Security.Cryptography.MD5.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(cloopenSid + cloopenToken + DateTools.Format("yyyyMMddHHmmss")));
        System.Text.StringBuilder sig_builder = new System.Text.StringBuilder();
        foreach (byte b in md5_result)
        {
            sig_builder.Append(b.ToString("x2"));
        }

        var str = "";
        var url = ApiUrl + "/Accounts/" + cloopenSid + "/" + method + "?sig=" + sig_builder.ToString().ToUpper();
        using (var hostSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            try
            {
                var uri = new Uri(url);
                var reqStr = "";
                reqStr += "POST " + uri.PathAndQuery + " HTTP/1.1";
                reqStr += "\r\nHost: " + uri.Host;
                reqStr += "\r\nDate: " + DateTools.ConvertToGMT(DateTools.GetUnix());
                reqStr += "\r\nAccept: " + contentType;
                reqStr += "\r\nAccept-Language: zh-cn";
                reqStr += "\r\nContent-Type: " + contentType + ";charset=utf-8";
                reqStr += "\r\nContent-Length: " + Encoding.UTF8.GetBytes(postData).Length;
                reqStr += "\r\nAuthorization: " + Wlniao.IO.Base64Default.Encoder(cloopenSid + ":" + DateTools.Format("yyyyMMddHHmmss"));
                reqStr += "\r\nConnection: Close";
                reqStr += "\r\n";
                reqStr += "\r\n";
                reqStr += postData;
                reqStr += "\r\n";
                reqStr += "\r\n";
                if (strUtil.IsIP(uri.Host))
                {
                    hostSocket.Connect(System.Net.IPAddress.Parse(uri.Host), uri.Port);
                }
                else
                {
                    var ip = new Wlniao.Net.Dns.DnsTool().GetIPAddress(uri.Host);
                    hostSocket.Connect(ip, uri.Port);
                }
                if (ApiUrl.StartsWith("https://"))
                {
                    #region HTTPS
                    using (SslStream ssl = new SslStream(new NetworkStream(hostSocket, true), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null))
                    {
                        ssl.AuthenticateAsClientAsync(uri.Host).ContinueWith((rlt) =>
                        {
                            if (ssl.IsAuthenticated)
                            {
                                ssl.Write(Encoding.UTF8.GetBytes(reqStr));
                                ssl.Flush();
                                var response = new byte[1024 * 16];
                                var length = ssl.Read(response, 0, response.Length);
                                if (length > 0)
                                {
                                    str = Encoding.UTF8.GetString(response);
                                }
                            }
                        }).Wait();
                    }
                    #endregion
                }
                else
                {
                    #region HTTP
                    var request = Encoding.UTF8.GetBytes(reqStr);
                    if (hostSocket.Send(request, request.Length, SocketFlags.None) > 0)
                    {
                        var response = new byte[1024 * 64];
                        while (true)
                        {
                            var length = hostSocket.Receive(response, response.Length, SocketFlags.None);
                            if (length > 0)
                            {
                                str = Encoding.UTF8.GetString(response);
                                break;
                            }
                        }
                    }
                    #endregion
                }
                try
                {
                    hostSocket.Shutdown(SocketShutdown.Both);
                    hostSocket.Dispose();
                }
                catch { }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                throw ex;
            }
        }
        if (str.StartsWith("HTTP"))
        {
            var lines = str.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            var ts = lines[0].Split(' ');
            if (ts[1] != "200")
            {
                str = "{\"statusCode\":\"-1\",\"statusMsg\":\"服务端请求失败\"}";
            }
            else
            {
                var start = false;
                foreach (var line in lines)
                {
                    if (start)
                    {
                        if (str.IsNullOrEmpty())
                        {
                            str = line;
                        }
                        else
                        {
                            str += "\r\n" + line;
                        }
                    }
                    if (string.IsNullOrEmpty(line))
                    {
                        start = true;
                        str = "";
                    }
                }
            }
        }
        return str;
    }
    public static string AccountsXml(string cloopenSid, string cloopenToken, string method, string postData)
    {
        return Post(cloopenSid, cloopenToken, method, postData, "application/xml");
    }
    public static string AccountsJson(string cloopenSid, string cloopenToken, string method, string postData)
    {
        return Post(cloopenSid, cloopenToken, method, postData, "application/json");
    }
    /// <summary>
    /// 模板短信发送
    /// </summary>
    /// <param name="cloopenSid"></param>
    /// <param name="cloopenToken"></param>
    /// <param name="cloopenAppId"></param>
    /// <param name="cloopenTemplateId"></param>
    /// <param name="toMobile"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    public static Boolean SendSMS(string cloopenSid, string cloopenToken, string cloopenAppId, string cloopenTemplateId, string toMobile, params string[] datas)
    {
        try
        {
            string _datas = "";
            try
            {
                //datas[1] = "test";
                foreach (string data in datas)
                {
                    if (string.IsNullOrEmpty(_datas))
                    {
                        _datas = "'" + data + "'";
                    }
                    else
                    {
                        _datas += ",'" + data + "'";
                    }
                }
            }
            catch { }
            var str = AccountsJson(cloopenSid, cloopenToken, "SMS/TemplateSMS", "{'appId':'" + cloopenAppId + "','to':'" + toMobile + "','templateId':'" + cloopenTemplateId + "','datas':[" + _datas + "]}");
            var statusCode = Json.GetFieldStr(str, "statusCode");
            if (statusCode == "000000")
            {
                return true;
            }
            else
            {
                log.Error("Cloopen Send SMS:" + str);
            }
        }
        catch (Exception ex)
        {
            log.Error("Cloopen Send SMS:" + ex.Message);
        }
        return false;
    }

    public static Dictionary<String, String> FlowPackage(string cloopenSid, string cloopenToken, string mobile)
    {
        var dic = new Dictionary<String, String>();
        var str = AccountsJson(cloopenSid, cloopenToken, "flowPackage/flowPackage", "{'phoneNum':'" + mobile + "'}");
        var statusCode = Json.GetFieldStr(str, "statusCode");
        if (statusCode == "000000")
        {
            var listStr = str.Substring(str.IndexOf("flowPackage") + 13);
            listStr = listStr.Substring(0, listStr.IndexOf(']') + 1);
            var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(listStr);
            if (list != null)
            {
                foreach (var item in list)
                {
                    dic.Add(item["sn"], item["packet"]);
                }
            }
        }
        else
        {
            log.Error("Cloopen Send SMS:" + str);
        }
        return dic;
    }
    public static Boolean FlowRecharge(string cloopenSid, string cloopenToken, string cloopenAppId, string mobile, string sn, string packet, string customId, string callbackUrl, out string rechargeId)
    {
        rechargeId = "";
        var str = AccountsJson(cloopenSid, cloopenToken, "flowPackage/flowRecharge", "{'appId':'" + cloopenAppId + "','phoneNum':'" + mobile + "','sn':'" + sn + "','packet':'" + packet + "','customId':'" + packet + "','callbackUrl':'" + callbackUrl + "'}");
        var statusCode = Json.GetFieldStr(str, "statusCode");
        if (statusCode == "000000")
        {
            rechargeId = Json.GetFieldStr(str, "rechargeId");
            var status = Json.GetFieldStr(str, "status");
            if (status == "1" || status == "3")
            {
                return true;
            }
        }
        else
        {
            log.Error("Cloopen Send SMS:" + str);
        }
        return false;
    }
}