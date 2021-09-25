namespace EasyContainer.Service.UnitTests.TicketPurchaseJobs
{
    using System.Threading;
    using Lib;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using NUnit.Framework;
    using Service.TicketPurchaseJobs;
    using Settings;
    using Settings.SettingExtensions;

    [TestFixture]
    public class TicketPurchaseJobManagerTests
    {
        private TicketPurchaseJobManager _ticketPurchaseJobManager;

        private ISettingWrapper<FreightSmartSettings> _freightSmartSettings;

        private ISettingWrapper<TargetLaneSettings> _targetLaneSettingsSettings;

        private ILoggerFactory _loggerFactory;

        [SetUp]
        public void SetUp()
        {
            var configBuilder = new ConfigurationBuilder();
            var config = configBuilder
                .AddJsonFile("appsettings.json", false)
                .Build();

            _loggerFactory = new LoggerFactory();

            _freightSmartSettings = new SettingWrapper<FreightSmartSettings>(
                _loggerFactory.CreateLogger<SettingWrapper<FreightSmartSettings>>(), new FreightSmartSettings());
            _freightSmartSettings.Reload(config);

            _targetLaneSettingsSettings = new SettingWrapper<TargetLaneSettings>(
                _loggerFactory.CreateLogger<SettingWrapper<TargetLaneSettings>>(), new TargetLaneSettings());

            var loginDriver = new LoginDriver(_loggerFactory.CreateLogger<LoginDriver>(), _freightSmartSettings);
            _ticketPurchaseJobManager = new TicketPurchaseJobManager(
                _loggerFactory.CreateLogger<TicketPurchaseJobManager>(), _freightSmartSettings,
                _targetLaneSettingsSettings, loginDriver);
        }

        [Test]
        public void  SchedulerJob_Test()
        {
            // Pre-conditions
            using var cancellationSource = new CancellationTokenSource();
            var autoReset = new AutoResetEvent(false);
            _ticketPurchaseJobManager.OnJobDictUpdated += (sender, args) => autoReset.Set();

            // Action - 1
            var lane1 = new LaneSettings()
                {LaneId = "S1", ExecutionDuration = 1, WindowCount = 1, TicketReleaseTime = "12/17/2100 - 10:30:00"};
            
            _targetLaneSettingsSettings.Settings.LaneSettings.Add(lane1);
            _ticketPurchaseJobManager.SchedulerJob(cancellationSource.Token);

            // Post-conditions - 1
            Assert.That(_ticketPurchaseJobManager.ScheduledJobs.Count, Is.EqualTo(1));
            _targetLaneSettingsSettings.Settings.LaneSettings.ForEach(s =>
                Assert.True(_ticketPurchaseJobManager.ScheduledJobs.ContainsKey(s.GetHash())));

            // Action - 2
            var lane2 = new LaneSettings()
                {LaneId = "s2", ExecutionDuration = 2, WindowCount = 2, TicketReleaseTime = "12/17/2100 - 10:30:00"};
            _targetLaneSettingsSettings.Settings.LaneSettings.Add(lane2);
            _ticketPurchaseJobManager.SchedulerJob(cancellationSource.Token);

            // Post-conditions - 2
            Assert.That(_ticketPurchaseJobManager.ScheduledJobs.Count, Is.EqualTo(2));
            _targetLaneSettingsSettings.Settings.LaneSettings.ForEach(s =>
                Assert.True(_ticketPurchaseJobManager.ScheduledJobs.ContainsKey(s.GetHash())));
        }

        [TearDown]
        public void TearDown()
        {
            _ticketPurchaseJobManager?.Dispose();
            _loggerFactory?.Dispose();
        }
    }
}