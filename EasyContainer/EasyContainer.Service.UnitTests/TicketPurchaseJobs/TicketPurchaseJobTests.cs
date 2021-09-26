namespace EasyContainer.Service.UnitTests.TicketPurchaseJobs
{
    using System;
    using System.Threading;
    using Lib;
    using Lib.Utilities;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Console;
    using Moq;
    using NUnit.Framework;
    using Service.TicketPurchaseJobs;
    using Settings;

    [TestFixture]
    public class TicketPurchaseJobTests
    {
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

            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddConsole()
                    .AddEventLog();
            });

            _freightSmartSettings = new SettingWrapper<FreightSmartSettings>(
                _loggerFactory.CreateLogger<SettingWrapper<FreightSmartSettings>>(), new FreightSmartSettings());
            _freightSmartSettings.Reload(config);
            _freightSmartSettings.Settings.IsTesting = true;

            _targetLaneSettingsSettings = new SettingWrapper<TargetLaneSettings>(
                _loggerFactory.CreateLogger<SettingWrapper<TargetLaneSettings>>(), new TargetLaneSettings());
        }

        [TestCase("")]
        [TestCase(null)]
        public void Schedule_JobStartedImmediately_Test(string releaseTime)
        {
            // Pre-Conditions
            var loginDriver = new LoginDriver(_loggerFactory.CreateLogger<LoginDriver>(), _freightSmartSettings);
            _freightSmartSettings.Settings.IsTesting = false;
            var ls = new LaneSettings
            {
                LaneId = "s21", ExecutionDuration = 10, WindowCount = 1, TicketReleaseTime = releaseTime,
                PortOfLoading = "温哥华", PortOfDestination = "宁波市", EarliestTimeOfDeparture = "2021-09-30",
                LatestTimeOfDeparture = "2021-10-20"
            };
            using var job = new TicketPurchaseJob(
                _loggerFactory.CreateLogger<TicketPurchaseJob>(),
                _freightSmartSettings,
                ls,
                loginDriver);

            job.OnPreparationWaitStarted += (sender, args) => Assert.Fail();
            job.OnPreExecutionWaitStarted += (sender, args) => Assert.Fail();

            var countdown = new CountdownEvent(ls.WindowCount);
            job.OnJobStarted += (sender, args) => countdown.Signal();

            // Action
            job.Schedule();

            // Post-conditions
            Assert.True(countdown.Wait(TimeSpan.FromSeconds(20)));
        }

        [Test]
        public void Schedule_JobStartedOnSchedule_Test()
        {
            // Pre-Conditions
            var releaseTime = DateTime.Now + TimeSpan.FromMinutes(10);
            _freightSmartSettings.Settings.PreparationOffset = 180;
            _freightSmartSettings.Settings.PreExecutionOffset = 10;

            var ls = new LaneSettings()
            {
                LaneId = "s21", ExecutionDuration = 10, WindowCount = 2,
                TicketReleaseTime = releaseTime.ToString(SettingHelper.DateTimeFormat)
            };

            var dtMock = new Mock<IDateTimeSupplier>();
            using var job = new TicketPurchaseJob(
                _loggerFactory.CreateLogger<TicketPurchaseJob>(),
                _freightSmartSettings,
                ls,
                null,
                dateTimeSupplier: dtMock.Object
            );

            dtMock.SetupGet(m => m.Now)
                .Returns(releaseTime - TimeSpan.FromSeconds(_freightSmartSettings.Settings.PreparationOffset + 2));

            var manualResetPreparation = new ManualResetEventSlim(false);
            job.OnPreparationWaitStarted += (sender, args) =>
            {
                manualResetPreparation.Set();
                dtMock.SetupGet(m => m.Now)
                    .Returns(releaseTime - TimeSpan.FromSeconds(_freightSmartSettings.Settings.PreExecutionOffset + 2));
            };

            var countDownPreExecution = new CountdownEvent(ls.WindowCount);
            job.OnPreExecutionWaitStarted += (sender, args) => countDownPreExecution.Signal();

            var countdownResetJobStarted = new CountdownEvent(ls.WindowCount);
            job.OnJobStarted += (sender, args) => countdownResetJobStarted.Signal();

            dtMock.Setup(m => m.Now)
                .Returns(releaseTime - TimeSpan.FromSeconds(_freightSmartSettings.Settings.PreparationOffset + 2));

            // Action
            job.Schedule();

            // Post-conditions
            Assert.True(manualResetPreparation.Wait(TimeSpan.FromSeconds(20)));
            Assert.True(countDownPreExecution.Wait(TimeSpan.FromSeconds(20)));
            Assert.True(countdownResetJobStarted.Wait(TimeSpan.FromSeconds(20)));
        }
    }
}