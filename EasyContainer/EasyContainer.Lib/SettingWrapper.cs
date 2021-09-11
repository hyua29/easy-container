namespace EasyContainer.Lib
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public interface ISettingWrapper<T> where T : Setting, new()
    {
        event EventHandler OnReload;

        event AsyncEventHandler OnReloadAsync;

        T Settings { get; }

        void Reload(IConfiguration configuration);

        Task ReloadAsync(IConfiguration configuration);
    }

    public class SettingWrapper<T> : ISettingWrapper<T> where T : Setting, new()
    {
        private readonly ILogger<ISettingWrapper<T>> _logger;

        public SettingWrapper(ILogger<ISettingWrapper<T>> logger, T settings)
        {
            _logger = logger;
            Settings = settings;
        }

        public event EventHandler OnReload;

        public event AsyncEventHandler OnReloadAsync;

        public T Settings { get; private set; }

        public void Reload(IConfiguration configuration)
        {
            var newSettings = new T();
            configuration.Bind(typeof(T).Name, newSettings);

            Settings = newSettings;

            _logger.LogInformation($"\n{JsonConvert.SerializeObject(Settings)}");

            OnReload?.Invoke(this, EventArgs.Empty);
        }

        public async Task ReloadAsync(IConfiguration configuration)
        {
            var newSettings = new T();
            configuration.Bind(typeof(T).Name, newSettings);

            Settings = newSettings;

            _logger.LogInformation($"\n{JsonConvert.SerializeObject(Settings)}");

            if (OnReloadAsync != null)
            {
                await OnReloadAsync(this, EventArgs.Empty).ConfigureAwait(false);
            }
        }
    }
}