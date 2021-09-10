namespace EasyContainer.Lib
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;

    public class ConfigEventDebouncer : EventDebouncer
    {
        private readonly ILogger<ConfigEventDebouncer> _logger;

        public ConfigEventDebouncer(IConfiguration configuration, ILogger<ConfigEventDebouncer> logger,
            TimeSpan? timeSpan = null) : base(timeSpan)
        {
            _logger = logger;

            ChangeToken.OnChange<object>(configuration.GetReloadToken,
#pragma warning disable 4014
                _ => InvokeEventAsync(), null);
#pragma warning restore 4014
        }

        public override Task InvokeEventAsync()
        {
            try
            {
                return base.InvokeEventAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogError(e.StackTrace);
                return Task.CompletedTask;
            }
        }
    }
}