namespace EasyContainer.Lib.Extensions
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public static class ServiceCollectionExtension
    {
        public static void MonitorSetting<T>(this IServiceCollection serviceCollection, TimeSpan? debounceDuration = null)
            where T : ISetting, new()
        {
            serviceCollection.AddSingleton<IConfigEventWatcher<T>>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var logger = provider.GetRequiredService<ILogger<ConfigEventWatcher<T>>>();

                return new ConfigEventWatcher<T>(config, logger, debounceDuration);
            });

            serviceCollection.AddSingleton<ISettingWrapper<T>>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<SettingWrapper<T>>>();
                var config = provider.GetRequiredService<IConfiguration>();

                var configWrapper = new SettingWrapper<T>(logger, new T());

                configWrapper.Reload(config);

                var configDebouncer = provider.GetRequiredService<IConfigEventWatcher<T>>();
                configDebouncer.Event += (o, args) => configWrapper.Reload(config);

                return configWrapper;
            });
        }
    }
}