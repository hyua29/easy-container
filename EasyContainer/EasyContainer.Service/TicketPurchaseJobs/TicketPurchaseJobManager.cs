namespace EasyContainer.Service.TicketPurchaseJobs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Threading;
    using System.Threading.Tasks;
    using Lib;
    using Lib.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Settings;
    using Settings.SettingExtensions;

    public interface ITicketPurchaseJobManager : IDisposable
    {
        IImmutableDictionary<string, TicketPurchaseJob> ScheduledJobs { get; }
    }

    public class TicketPurchaseJobManager : BackgroundService, ITicketPurchaseJobManager
    {
        private readonly ILogger<TicketPurchaseJobManager> _logger;

        private readonly ISettingWrapper<FreightSmartSettings> _freightSmartSettings;

        private readonly ISettingWrapper<TargetLaneSettings> _targetLaneSettings;

        private readonly ConcurrentDictionary<string, TicketPurchaseJob> _browserJobDict;

        private readonly LoginDriver _loginDriver;

        public TicketPurchaseJobManager(ILogger<TicketPurchaseJobManager> logger,
            ISettingWrapper<FreightSmartSettings> freightSmartSettings,
            ISettingWrapper<TargetLaneSettings> targetLaneSettings, LoginDriver loginDriver)
        {
            _logger = logger;
            _freightSmartSettings = freightSmartSettings;
            _targetLaneSettings = targetLaneSettings;
            _browserJobDict = new ConcurrentDictionary<string, TicketPurchaseJob>();
            _loginDriver = loginDriver;
        }

        public IImmutableDictionary<string, TicketPurchaseJob> ScheduledJobs => _browserJobDict.ToImmutableDictionary();

        public event EventHandler OnJobDictUpdated;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{nameof(TicketPurchaseJobManager)} has been started");
            while (!stoppingToken.IsCancellationRequested)
            {
                SchedulerJob(stoppingToken);
                OnJobDictUpdated?.Invoke(this, EventArgs.Empty);
                await Task.Delay(3000, stoppingToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Scheduler new jobs and remove old jobs
        /// </summary>
        /// <param name="token"></param>
        internal void SchedulerJob(CancellationToken token)
        {
            var routeSettingDict = new Dictionary<string, LaneSettings>();
            _targetLaneSettings.Settings.LaneSettings.ForEach(ls =>
            {
                var key = ls.GetHash();
                var lsCopy = new LaneSettings(ls);
                if (ValidateLaneSettings(lsCopy))
                {
                    routeSettingDict[key] = lsCopy;
                }
            });

            // Remove deleted jobs
            foreach (var k in routeSettingDict.Keys)
            {
                _browserJobDict.TryRemove(k, out var jobWithWait);
                jobWithWait?.Dispose();
            }

            // Add new jobs
            routeSettingDict.Keys.ForEach(k =>
            {
                if (!_browserJobDict.ContainsKey(k))
                {
                    var jobHash = routeSettingDict[k].GetHash();
                    using var logger = _logger.BeginScope(new Dictionary<string, object>
                        {["BrowserJobHash"] = jobHash});

                    var job = new TicketPurchaseJob(
                        _logger,
                        _freightSmartSettings,
                        routeSettingDict[k],
                        _loginDriver,
                        hash =>
                        {
                            var successful = _browserJobDict.TryGetValue(hash, out var value);
                            value?.Dispose();
                            if (successful)
                            {
                                _browserJobDict.TryRemove(hash, out _);
                            }
                        },
                        cancellationToken: token);
                    var added = _browserJobDict.TryAdd(k, job);

                    if (added) job.Schedule();
                }
            });
        }

        private bool ValidateLaneSettings(LaneSettings lsCopy)
        {
            return true;
        }

        public override void Dispose()
        {
            base.Dispose();

            _loginDriver?.Dispose();

            foreach (var v in _browserJobDict.Values)
            {
                v.Dispose();
            }

            _browserJobDict.Clear();
        }
    }
}