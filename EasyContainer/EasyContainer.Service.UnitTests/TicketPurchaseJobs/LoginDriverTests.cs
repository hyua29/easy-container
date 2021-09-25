namespace EasyContainer.Service.UnitTests.TicketPurchaseJobs
{
    using Lib;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;
    using Service.TicketPurchaseJobs;
    using Settings;

    [TestFixture]
    public class LoginDriverTests
    {
        private LoginDriver _driver;

        private ISettingWrapper<FreightSmartSettings> _freightSmartSettingsWrapper;

        [SetUp]
        public void SetUp()
        {
            var configBuilder = new ConfigurationBuilder();
            var config = configBuilder
                .AddJsonFile("appsettings.json", false)
                .Build();

            _freightSmartSettingsWrapper =
                new SettingWrapper<FreightSmartSettings>(
                    new Mock<ILogger<SettingWrapper<FreightSmartSettings>>>().Object, new FreightSmartSettings());
            _freightSmartSettingsWrapper.Reload(config);

            _driver = new LoginDriver(new Mock<ILogger<LoginDriver>>().Object, _freightSmartSettingsWrapper);
        }

        [Test]
        public void GetOrReloadDriver_Test()
        {
            var webDriver = _driver.GetOrReloadDriver();
            Assert.That(webDriver.Url, Is.EqualTo(_freightSmartSettingsWrapper.Settings.Domain));
        }

        [TearDown]
        public void TearDown()
        {
            _driver.Dispose();
        }
    }
}