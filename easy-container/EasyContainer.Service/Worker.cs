

namespace EasyContainer.Service
{
    using Lib;
    using Settings;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly SettingWrapper<TargetRouteSetting> _targetRouteWrapper;

        public Worker(ILogger<Worker> logger, SettingWrapper<TargetRouteSetting> targetRouteWrapper)
        {
            _logger = logger;
            _targetRouteWrapper = targetRouteWrapper;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
            }
        }
    }
}