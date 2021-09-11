namespace EasyContainer.Lib.UnitTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SettingWrapperTests
    {
        [Test]
        public async Task OnReload_UpdatingSettings_SettingsReloadedIfNotChanged()
        {
            // Pre-conditions
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Production");

            var configBuilder = new ConfigurationBuilder();
            var config = configBuilder
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            long reloadCount = 0;
            long reloadAsyncCount = 0;
            var mySettings1 = new MySettings1();
            config.Bind("MySettings1", mySettings1);
            var configWrapper1 =  new SettingWrapper<MySettings1>(Mock.Of<ILogger<SettingWrapper<MySettings1>>>(), mySettings1);
            configWrapper1.OnReload += (o, args) =>
            {
                reloadCount++;
            };
            configWrapper1.OnReloadAsync += (o, args) =>
            {
                Interlocked.Increment(ref reloadAsyncCount);
                return Task.CompletedTask;
            };

            // Action
            configWrapper1.ReloadIfDifferent(config);
            await configWrapper1.ReloadIfDifferentAsync(config).ConfigureAwait(false);

            // Post-conditions
            Assert.That(reloadCount, Is.EqualTo(0));
            Assert.That(Interlocked.Read(ref reloadAsyncCount), Is.EqualTo(0));
            
            // Action
            var originalMessage = config.GetValue<string>("MySettings1:Message");
            AppSettings.AddOrUpdateAppSetting("MySettings1:Message", $"{originalMessage}-Updated-1");
            await Task.Delay(500).ConfigureAwait(false);
            configWrapper1.ReloadIfDifferent(config);

            // Post-conditions
            Assert.That(reloadCount, Is.EqualTo(1));
            Assert.That(configWrapper1.Settings.Message, Is.EqualTo($"{originalMessage}-Updated-1"));
            
            // Action
            AppSettings.AddOrUpdateAppSetting("MySettings1:Message", $"{originalMessage}-Updated-2");
            await Task.Delay(500).ConfigureAwait(false);
            await configWrapper1.ReloadIfDifferentAsync(config).ConfigureAwait(false);

            // Post-conditions
            Assert.That(reloadAsyncCount, Is.EqualTo(Interlocked.Read(ref reloadAsyncCount)));
            Assert.That(configWrapper1.Settings.Message, Is.EqualTo($"{originalMessage}-Updated-2"));

            AppSettings.AddOrUpdateAppSetting("MySettings1:Message", $"{originalMessage}");
        }
    }
}