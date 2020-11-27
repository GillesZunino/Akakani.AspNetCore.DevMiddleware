using System.Collections.Generic;
using Akakani.AspNetCore.DevMiddleware;
using Akakani.AspNetCore.DevMiddleware.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace simple
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions() {
                    HotModuleReplacement = true,
                    ConfigFile = "webpack.4.config.js"
                });
            }

            app.UseDefaultFiles(new DefaultFilesOptions()
            {
                DefaultFileNames = new List<string>() { "Index.html" }
            });

            app.UseRouting();

            app.UseStaticFiles();
        }
    }
}
