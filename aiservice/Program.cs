using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace aiservice.api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) => {
                //config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddJsonFile("appsettings.cdnsettings.json", optional: true, reloadOnChange: true);
                config.AddJsonFile("appsettings.extrasettings.json", optional: true, reloadOnChange: true);
                config.AddJsonFile("appsettings.logsettings.json", optional: true, reloadOnChange: true);
                config.AddJsonFile("appsettings.watsonsettings.json", optional: true, reloadOnChange: true);
                config.AddJsonFile("sqlfunctions.json", optional: true, reloadOnChange: true);
            })
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseIISIntegration();
                webBuilder.UseUrls("http://*:62859");
                webBuilder.ConfigureKestrel(serverOptions =>
                {
                    serverOptions.Limits.MaxRequestBodySize = long.MaxValue;
                });
                webBuilder.UseStartup<Startup>();
            });
    }
}
