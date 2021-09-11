namespace EasyContainer.Lib.Extensions
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public static class ServiceCollectionExtension
    {
        public static void AddSettingDebouncer(this IServiceCollection serviceCollection, TimeSpan? pollInterval = null)
        {
            serviceCollection.AddSingleton<ConfigEventDebouncer>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var logger = provider.GetRequiredService<ILogger<ConfigEventDebouncer>>();
                var debouncer = new ConfigEventDebouncer(config, logger, pollInterval);

                return debouncer;
            });
        }

        public static void MonitorSetting<T>(this IServiceCollection serviceCollection)
            where T : Setting, new()
        {
            serviceCollection.AddSingleton<SettingWrapper<T>>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<SettingWrapper<T>>>();
                var config = provider.GetRequiredService<IConfiguration>();

                var configWrapper = new SettingWrapper<T>(logger, new T());

                configWrapper.ReloadIfDifferent(config);

                var configDebouncer = provider.GetRequiredService<ConfigEventDebouncer>();
                configDebouncer.Event += (o, args) => configWrapper.ReloadIfDifferentAsync(config);

                return configWrapper;
            });
        }
    }
}