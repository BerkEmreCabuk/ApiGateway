using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;

namespace ProductService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "ProductService")
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Information)
            .WriteTo.Console()
            .WriteTo.Elasticsearch(
                    new ElasticsearchSinkOptions(
                        new Uri("http://localhost:9200/"))
                    {
                        CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true),
                        AutoRegisterTemplate = true,
                        TemplateName = "serilog-events-template",
                        IndexFormat = "log-{0:yyyy.MM.dd}"
                    })
            .CreateLogger();
            try
            {
                Log.Information("Starting host");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel();
                    webBuilder.ConfigureLogging((hostingContext, config) => { config.ClearProviders(); });
                    webBuilder.UseUrls("http://localhost:5102");
                    webBuilder.UseSerilog();
                });
    }
}
