

namespace EasyContainer.Service
{
    using Lib;
    using Settings;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Lib.Extensions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly ISettingWrapper<TargetRouteSettings> _targetRouteWrapper;

        public Worker(ILogger<Worker> logger, ISettingWrapper<TargetRouteSettings> targetRouteWrapper)
        {
            _logger = logger;
            _targetRouteWrapper = targetRouteWrapper;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                
                _logger.LogInformation("Number of routes: {number}", _targetRouteWrapper.Settings.RouteSettings.Count);
                
                await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
            }
        }
    }
}