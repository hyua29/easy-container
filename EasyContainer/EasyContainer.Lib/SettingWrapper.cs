namespace EasyContainer.Lib
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public interface ISettingWrapper<T> where T : Setting, new()
    {
        event EventHandler OnReload;
        
        event AsyncEventHandler OnReloadAsync;

        T Settings { get; set; }

        void ReloadIfDifferent(IConfiguration configuration);
        
        Task ReloadIfDifferentAsync(IConfiguration configuration);
    }

    public class SettingWrapper<T> : ISettingWrapper<T> where T : Setting, new()
    {
        private readonly ILogger<SettingWrapper<T>> _logger;

        public SettingWrapper(ILogger<SettingWrapper<T>> logger, T settings)
        {
            _logger = logger;
            Settings = settings;
        }

        public event EventHandler OnReload;

        public event AsyncEventHandler OnReloadAsync;

        public T Settings { get; set; }

        public void ReloadIfDifferent(IConfiguration configuration)
        {
            var newSettings = new T();
            configuration.Bind(typeof(T).Name, newSettings);
            if (Settings.ToString() == newSettings.ToString())
            {
                return;
            }

            Settings = newSettings;

            _logger.LogInformation(Settings.ToString());
            OnReload?.Invoke(this, EventArgs.Empty);
        }

        public async Task ReloadIfDifferentAsync(IConfiguration configuration)
        {
            var newSettings = new T();
            configuration.Bind(typeof(T).Name, newSettings);
            if (Settings.ToString() == newSettings.ToString())
            {
                return;
            }

            Settings = newSettings;

            _logger.LogInformation(Settings.ToString());

            if (OnReloadAsync != null)
            {
                await OnReloadAsync(this, EventArgs.Empty).ConfigureAwait(false);
            }
        }
    }
}