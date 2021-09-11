namespace WorkerService
{
    using System.Reflection;
    using EasyContainer.Lib;
    using EasyContainer.Lib.Extensions;
    using log4net;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

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
                    services.AddHostedService<Worker>();
                    services.MonitorSetting<MySetting>();
                });
        }
    }
}