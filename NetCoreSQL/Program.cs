using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace NetCoreSQL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                              .ConfigureLogging(builder =>
                              {

                                  builder.AddApplicationInsights("db325280-2e32-4dee-ba5b-4c0563ac8fd7");
                                  builder.AddFilter<ApplicationInsightsLoggerProvider>
                                 ("", LogLevel.Information);


                              }) ;
                });
    }
}
