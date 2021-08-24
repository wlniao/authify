using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options => { options.AllowSynchronousIO = true; });
        services.AddMvc(options => options.EnableEndpointRouting = false).AddRazorRuntimeCompilation();
    }
    public void Configure(IApplicationBuilder app)
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                try { ctx.Context.Response.Headers.Add("Access-Control-Allow-Origin", new Microsoft.Extensions.Primitives.StringValues("*")); } catch { }
            }
        });
        app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        app.UseMvc();
        app.UseMvc(routes =>
        {
            routes.MapRoute("Path", "{controller}/{action}", new { controller = "", action = "" });
            routes.MapRoute("Default", "", new { controller = "auth", action = "home" });
        });
    }
}