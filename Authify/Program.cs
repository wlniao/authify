using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


public class Program
{
    public static void Main(string[] args)
    {
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