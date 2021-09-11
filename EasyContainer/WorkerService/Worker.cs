namespace WorkerService
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class Worker : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Worker> _logger;

        private readonly IOptions<MySetting> _mySettings;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IOptions<MySetting> mySettings)
        {
            _logger = logger;
            _configuration = configuration;
            _mySettings = mySettings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                _logger.LogInformation($"{_configuration["MySettings:Message"]}/{_mySettings.Value.Message}");
                await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
            }
        }
    }
}