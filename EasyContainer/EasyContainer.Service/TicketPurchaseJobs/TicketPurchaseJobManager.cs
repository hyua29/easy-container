namespace EasyContainer.Service.TicketPurchaseJobs
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensions;
    using Lib;
    using Lib.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Support.UI;
    using Settings;

    public interface ITicketPurchaseJobManager : IAsyncDisposable
    {
    }

    public class TicketPurchaseJobManager : BackgroundService, ITicketPurchaseJobManager
    {
        private readonly ILogger<TicketPurchaseJobManager> _logger;

        private readonly ILoggerFactory _loggerFactory;

        private readonly ISettingWrapper<SeleniumSettings> _seleniumSettings;

        private readonly ISettingWrapper<FreightSmartSettings> _freightSmartSettings;

        private readonly ISettingWrapper<TargetRouteSettings> _targetRouteSettings;

        private readonly IDictionary<SHA256, TicketPurchaseJob> _browserJobDict;

        private IWebDriver _loginDriver;

        public TicketPurchaseJobManager(ILogger<TicketPurchaseJobManager> logger, ILoggerFactory loggerFactory,
            ISettingWrapper<SeleniumSettings> seleniumSettings,
            ISettingWrapper<FreightSmartSettings> freightSmartSettings,
            ISettingWrapper<TargetRouteSettings> targetRouteSettings)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _seleniumSettings = seleniumSettings;
            _freightSmartSettings = freightSmartSettings;
            _targetRouteSettings = targetRouteSettings;
            _browserJobDict = new Dictionary<SHA256, TicketPurchaseJob>();
        }

        public IReadOnlyCollection<Cookie> Cookies => _loginDriver?.Manage().Cookies.AllCookies;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!stoppingToken.IsCancellationRequested)
            {
                Login();
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var routeSettingDict = GetRouteSettingDict();

                await UpdateJobDictAsync(routeSettingDict, stoppingToken).ConfigureAwait(false);

                await Task.Delay(3000, stoppingToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Scheduler new jobs and remove old jobs
        /// </summary>
        /// <param name="routeSettingDict"></param>
        /// <param name="token"></param>
        internal async Task UpdateJobDictAsync(Dictionary<SHA256, RouteSettings> routeSettingDict,
            CancellationToken token)
        {
            // Add new jobs
            routeSettingDict.Keys.ForEach(k =>
            {
                if (!_browserJobDict.ContainsKey(k))
                {
                    _browserJobDict[k] = new TicketPurchaseJob(_freightSmartSettings, _seleniumSettings,
                        routeSettingDict[k],
                        _loginDriver.Manage().Cookies.AllCookies, _loggerFactory.CreateLogger<TicketPurchaseJob>(),
                        token);

                    _browserJobDict[k].Schedule();
                }
            });

            // Remove deleted jobs
            foreach (var k in _browserJobDict.Keys)
            {
                if (!routeSettingDict.ContainsKey(k))
                {
                    await _browserJobDict[k].DisposeAsync().ConfigureAwait(false);
                    _browserJobDict.Remove(k);
                }
            }
        }

        internal Dictionary<SHA256, RouteSettings> GetRouteSettingDict()
        {
            var routeSettingDict = new Dictionary<SHA256, RouteSettings>();
            _targetRouteSettings.Settings.RouteSettings.ForEach(rs =>
            {
                var key = SHA256.Create(JsonConvert.SerializeObject(rs));
                routeSettingDict[key] = new RouteSettings(rs);
            });

            return routeSettingDict;
        }

        internal void Login()
        {
            var options = new ChromeOptions();
            options.AddArguments("--disable-plugins-discovery");
            options.AddArguments("--incognito");

            _loginDriver = new ChromeDriver(options);

            _loginDriver.Manage().Window.Maximize();
            _loginDriver.Manage().Cookies.DeleteAllCookies();

            _loginDriver.Navigate().GoToUrl(_freightSmartSettings.Settings.Domain);

            if (_seleniumSettings.Settings.AutoLogin)
            {
                // Cookies
                _loginDriver.FindElement(By.XPath("//div[@class='ctrl-footer']//a")).Click();
                _loginDriver.FindElements(By.XPath("//div[@role='switch' and contains(@class,'is-checked')]")).ForEach(
                    e => { e.Click(); });
                _loginDriver
                    .FindElement(By.XPath(
                        "//button[@type='button' and contains(@class,'button--danger')]//*[text() = 'Update Preference']"))
                    .Click();

                // Sign in
                var signin = _loginDriver.FindElement(By.XPath("//*[text()='Sign In/Up']"));
                signin.Click();

                var usernameField = _loginDriver.FindElement(By.XPath("//*[@name='login_dialog_username']"));
                usernameField.Click();
                usernameField.SendKeys(_freightSmartSettings.Settings.Username);

                var passwordField = _loginDriver.FindElement(By.XPath("//*[@id='login-password-input']"));
                passwordField.Click();
                passwordField.SendKeys(_freightSmartSettings.Settings.Password);
                passwordField.SendKeys(Keys.Enter);

                _loginDriver.TryFindElement(By.XPath("//div[@aria-label='警告']//*[contains(text(), '确 定')]"),
                    TimeSpan.FromSeconds(3))?.Click();
            }

            // Wait until logged in 
            new WebDriverWait(_loginDriver, TimeSpan.FromMinutes(30)).Until(d =>
                d.FindElement(By.XPath(
                    $"//a[@class='user-name' and contains(text(), '{_freightSmartSettings.Settings.Username}')]")));
        }

        public async ValueTask DisposeAsync()
        {
            _loginDriver?.Dispose();

            foreach (var v in _browserJobDict.Values)
            {
                await v.DisposeAsync().ConfigureAwait(false);
            }

            _browserJobDict.Clear();
        }
    }
}