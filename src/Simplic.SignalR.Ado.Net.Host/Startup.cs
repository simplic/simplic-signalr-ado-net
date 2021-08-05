using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Simplic.SignalR.Ado.Net.Server;

namespace Simplic.SignalR.Ado.Net.Host
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            var routeBuilder = new RouteBuilder(app);

            routeBuilder.MapGet("status", context =>
            {
                var assembly = typeof(Startup).GetTypeInfo().Assembly;
                Stream resource = assembly.GetManifestResourceStream("Simplic.SignalR.Ado.Net.Host.Templates.Status.html");

                using(var stream = new MemoryStream())
                {
                    resource.CopyTo(stream);
                    resource.Position = 0;

                    var text = Encoding.UTF8.GetString(stream.ToArray());

                    text = text.Replace("{client_count}", DatabaseHub.ConnectedIds.Count.ToString());
                    text = text.Replace("{query_count}", DatabaseHub.QueryCounter.ToString());

                    context.Response.ContentType = "text/html";
                    return context.Response.WriteAsync(text);
                }
            });

            app.UseRouter(routeBuilder.Build());

            app.UseSignalR(x =>
            {
                x.MapHub<DatabaseHub>("/database");
            });
        }
    }
}
