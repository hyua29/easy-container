namespace EasyContainer.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Threading.Tasks;
    using Extensions;
    using Lib;
    using Lib.Extensions;
    using Microsoft.Extensions.Logging;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Support.UI;
    using AppSettings = EasyContainer.AppSettings;

    public interface IBrowserJobManager : IDisposable
    {
        Task RunAsync();
    }

    public class BrowserJobManager : BaseDisposable, IBrowserJobManager
    {
        private readonly AppSettings _appSettings;

        private readonly IList<IDisposable> _disposables = new List<IDisposable>();

        private readonly ILogger<BrowserJobManager> _logger;

        private readonly int _positionShift = 10;

        private Point _currentPosition;

        private IWebDriver _loginDriver;

        public BrowserJobManager(AppSettings appSettings, ILogger<BrowserJobManager> logger)
        {
            _appSettings = appSettings;
            _logger = logger;

            _currentPosition = new Point(0, 0);
        }

        public Task RunAsync()
        {
            _logger.LogDebug("this is a debug message");
            _logger.LogInformation("this is an info message");
            // Login();

            // IList<IBrowserJob> jobs = new List<IBrowserJob>();
            // for (var i = 0; i < _appSettings.Selenium.WindowCount; i++)
            // {
            //     _currentPosition = new Point(_currentPosition.X + _positionShift, _currentPosition.Y + _positionShift);
            //     var job = new BrowserJob(_appSettings, _loginDriver.Manage().Cookies.AllCookies, _currentPosition, _logger);
            //
            //     jobs.Add(job);
            //     _disposables.Add(job);
            // }
            //
            // return Task.WhenAll(jobs.Select(j => j.RunAsync()).ToList());
            return Task.CompletedTask;
        }

        private void Login()
        {
            var options = new ChromeOptions();
            options.AddArguments("--disable-plugins-discovery");
            options.AddArguments("--incognito");

            _loginDriver = new ChromeDriver(options);

            _loginDriver.Manage().Window.Maximize();
            _loginDriver.Manage().Cookies.DeleteAllCookies();

            _loginDriver.Navigate().GoToUrl(_appSettings.FreightSmart.Domain);

            if (_appSettings.Selenium.AutoLogin)
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
                usernameField.SendKeys(_appSettings.FreightSmart.Username);

                var passwordField = _loginDriver.FindElement(By.XPath("//*[@id='login-password-input']"));
                passwordField.Click();
                passwordField.SendKeys(_appSettings.FreightSmart.Password);
                passwordField.SendKeys(Keys.Enter);

                _loginDriver.TryFindElement(By.XPath("//div[@aria-label='警告']//*[contains(text(), '确 定')]"),
                    TimeSpan.FromSeconds(3))?.Click();
            }

            // Wait until logged in 
            new WebDriverWait(_loginDriver, TimeSpan.FromMinutes(30)).Until(d =>
                d.FindElement(By.XPath(
                    $"//a[@class='user-name' and contains(text(), '{_appSettings.FreightSmart.Username}')]")));
        }

        protected override void DisposeManagedResources()
        {
            _loginDriver?.Dispose();
            _disposables.ForEach(d => { d?.Dispose(); });
        }
    }
}