namespace EasyContainer.Lib.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using NUnit.Framework;

    [TestFixture("Development")]
    [TestFixture("Staging")]
    [TestFixture("Production")]
    public class MonitorConfigTests
    {
        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", _env);
            _host = CreateHostBuilder(new string[0]).Build();
            _host.RunAsync();
        }

        [TearDown]
        public async Task TearDownAsync()
        {
            await _host.StopAsync().ConfigureAwait(false);
            _host.Dispose();
        }

        private readonly string _env;

        private IHost _host;

        public MonitorConfigTests(string env)
        {
            _env = env;
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(builder => builder.AddConsole())
                .ConfigureServices((hostContext, services) =>
                {
                    services.MonitorSetting<MySettings1>(TimeSpan.FromMilliseconds(200));
                    services.MonitorSetting<MySettings2>(TimeSpan.FromMilliseconds(200));
                });
        }

        [Test]
        public void MonitorSetting_AllFieldsLoaded_Test()
        {
            var config = _host.Services.GetRequiredService<IConfiguration>();

            var configWrapper1 = _host.Services.GetRequiredService<ISettingWrapper<MySettings1>>();
            Assert.That(configWrapper1.Settings.Message, Is.EqualTo(config.GetValue<string>("MySettings1:Message")));
            Assert.That(configWrapper1.Settings.MySettings1Nested.Message,
                Is.EqualTo(config.GetValue<string>("MySettings1:MySettings1Nested:Message")));

            var configWrapper2 = _host.Services.GetRequiredService<ISettingWrapper<MySettings2>>();
            Assert.That(configWrapper2.Settings.Message, Is.EqualTo(config.GetValue<string>("MySettings2:Message")));
            var mySettings2NestedList = new List<MySettings2Nested>();
            config.GetSection("MySettings2:MySettings2Nested").Bind(mySettings2NestedList);

            Assert.That(configWrapper2.Settings.MySettings2Nested.Count, Is.EqualTo(mySettings2NestedList.Count));
            configWrapper2.Settings.MySettings2Nested.ForEach(p =>
            {
                Assert.True(mySettings2NestedList.Any(s => s.Key == p.Key && s.Value == p.Value));
            });
        }

        [Test]
        public void MonitorSetting_UpdateAppSettingsJson_SettingObjectsAreSynced()
        {
            var config = _host.Services.GetRequiredService<IConfiguration>();
            var message1 = config.GetValue<string>("MySettings1:Message");
            var message2 = config.GetValue<string>("MySettings2:Message");
            var configWrapper1 = _host.Services.GetRequiredService<ISettingWrapper<MySettings1>>();
            var configWrapper2 = _host.Services.GetRequiredService<ISettingWrapper<MySettings2>>();

            Assert.That(configWrapper1.Settings.Message, Is.EqualTo(message1));
            Assert.That(configWrapper2.Settings.Message, Is.EqualTo(message2));

            var newMessage1 = $"new-{message1}";
            var newMessage2 = $"new-{message2}";
            AppSettings.AddOrUpdateAppSetting("MySettings1:Message", newMessage1);
            AppSettings.AddOrUpdateAppSetting("MySettings2:Message", newMessage2);

            var mySetting1Reset = new ManualResetEventSlim();
            var mySettings1Debouncer = _host.Services.GetRequiredService<IConfigEventWatcher<MySettings1>>();
            mySettings1Debouncer.Event += (o, args) =>
            {
                Assert.That(config.GetValue<string>("MySettings1:Message"), Is.EqualTo(newMessage1));
                Assert.That(configWrapper1.Settings.Message, Is.EqualTo(newMessage1));
                mySetting1Reset.Set();
            };
            mySetting1Reset.Wait(TimeSpan.FromSeconds(3));

            var mySetting2Reset = new ManualResetEventSlim();
            var mySettings2Debouncer = _host.Services.GetRequiredService<IConfigEventWatcher<MySettings1>>();
            mySettings2Debouncer.Event += (o, args) =>
            {
                Assert.That(config.GetValue<string>("MySettings2:Message"), Is.EqualTo(newMessage2));
                Assert.That(configWrapper2.Settings.Message, Is.EqualTo(newMessage2));
                mySetting2Reset.Set();
            };
            mySetting1Reset.Wait(TimeSpan.FromSeconds(3));

            AppSettings.AddOrUpdateAppSetting("MySettings1:Message", message1);
            AppSettings.AddOrUpdateAppSetting("MySettings2:Message", message2);
        }
    }
}