namespace EasyContainer.Lib
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public interface ISettingWrapper<T> where T : ISetting, new()
    {
        event EventHandler OnReload;

        event AsyncEventHandler OnReloadAsync;

        T Settings { get; }

        void Reload(IConfiguration configuration);

        Task ReloadAsync(IConfiguration configuration);
    }

    public class SettingWrapper<T> : ISettingWrapper<T> where T : ISetting, new()
    {
        private readonly ILogger<SettingWrapper<T>> _logger;
        private readonly ReaderWriterLockSlim _settingLock;

        private T _setting;

        public SettingWrapper(ILogger<SettingWrapper<T>> logger, T settings)
        {
            _logger = logger;
            _setting = settings;
            _settingLock = new ReaderWriterLockSlim();
        }

        public event EventHandler OnReload;

        public event AsyncEventHandler OnReloadAsync;

        public T Settings
        {
            get
            {
                try
                {
                    _settingLock.EnterReadLock();
                    return _setting;
                }
                finally
                {
                    _settingLock.ExitReadLock();
                }
            }
            private set
            {
                try
                {
                    _settingLock.EnterWriteLock();
                    _setting = value;
                }
                finally
                {
                    _settingLock.ExitWriteLock();
                }
            }
        }

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