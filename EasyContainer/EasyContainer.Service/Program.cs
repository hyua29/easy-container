namespace EasyContainer.Service
{
    using System.Reflection;
    using Lib.Extensions;
    using log4net;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Settings;
    using TicketPurchaseJobs;

    public static class Program
    {
        public static void Main(string[] args)
        {
            GlobalContext.Properties["ApplicationName"] = Assembly.GetExecutingAssembly().GetName().Name;
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logBuilder =>
                {
                    logBuilder.ClearProviders();
                    logBuilder.AddLog4Net("log4net.config");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.MonitorSetting<FreightSmartSettings>();
                    services.MonitorSetting<TargetLaneSettings>();

                    services.AddHostedService<TicketPurchaseJobManager>();
                });
        }
    }
}