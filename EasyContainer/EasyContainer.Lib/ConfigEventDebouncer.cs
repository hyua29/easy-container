namespace EasyContainer.Lib
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;

    public interface IConfigEventWatcher<T> : IDisposable where T : ISetting
    {
        event EventHandler Event;
    }

    public class ConfigEventWatcher<T> : BaseDisposable, IConfigEventWatcher<T> where T : ISetting
    {
        private readonly ILogger<ConfigEventWatcher<T>> _logger;

        private readonly IEventDebouncer _debouncer;

        private readonly IDisposable _changeTokenHandle;

        public ConfigEventWatcher(IConfiguration configuration, ILogger<ConfigEventWatcher<T>> logger,
            TimeSpan? debounceDuration = null)
        {
            _logger = logger;
            _debouncer = new EventDebouncer(debounceDuration);

            _changeTokenHandle = ChangeToken.OnChange(configuration.GetSection(typeof(T).Name).GetReloadToken,
#pragma warning disable 4014
                InvokeEvent);
#pragma warning restore 4014
        }

        public event EventHandler Event;

        private void InvokeEvent()
        {
            _debouncer.InvokeEventAsync(InvokeEventImplAsync);
        }

        private async Task InvokeEventImplAsync(object o, EventArgs args)
        {
            try
            {
                _logger.LogInformation($"Reloading {typeof(T).Name}");
                Event?.Invoke(o, args);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogError(e.StackTrace);
                await Task.CompletedTask.ConfigureAwait(false);
            }
        }

        protected override void DisposeManagedResources()
        {
            _changeTokenHandle?.Dispose();
            _debouncer?.Dispose();
        }
    }
}