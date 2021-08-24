using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;


public class Program
{
    public const string node = "authify";
    public static void Main(string[] args)
    {
#if !DEBUG
        try
        {
            using (var db = new MyContext())
            {
                db.Database.Migrate();
            }
        }
        catch (Exception ex)
        {
            Wlniao.log.Error("Database migrate error: " + ex.Message);
        }
#endif
        var host = new WebHostBuilder()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseStartup<Startup>()
        .UseKestrel(o =>
        {
            o.Listen(System.Net.IPAddress.IPv6Any, Wlniao.XCore.ListenPort);
        })
        .Build();
        host.Run();
    }
}