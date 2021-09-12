namespace EasyContainer
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using log4net;
    using log4net.Config;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class Program
    {
        private static ServiceProvider RegisterServices()
        {
            var services = new ServiceCollection();

            var configBuilder = new ConfigurationBuilder();
            var config = configBuilder
                .AddJsonFile("appsettings.json", false)
                .Build();

            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            services.AddSingleton<IConfiguration>(config);

            services.AddSingleton(config.Get<AppSettings>());

            // services.AddSingleton<IBrowserJobManager, BrowserJobManager>();

            return services.BuildServiceProvider();
        }

        private static async Task Main(string[] args)
        {
            await using var serviceProvider = RegisterServices();

            // await serviceProvider.GetRequiredService<IBrowserJobManager>().RunAsync().ConfigureAwait(true);

            Console.ReadKey();
        }
    }
}