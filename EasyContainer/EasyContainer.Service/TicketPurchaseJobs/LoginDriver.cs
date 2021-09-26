namespace EasyContainer.Service.TicketPurchaseJobs
{
    using System;
    using System.Collections.Generic;
    using Extensions;
    using Lib;
    using Lib.Extensions;
    using Microsoft.Extensions.Logging;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Support.UI;
    using Settings;

    public class LoginDriver : BaseDisposable
    {
        private readonly ILogger<LoginDriver> _logger;
        private readonly ISettingWrapper<FreightSmartSettings> _freightSmartSettings;

        private IWebDriver _loginDriver;

        public LoginDriver(
            ILogger<LoginDriver> logger,
            ISettingWrapper<FreightSmartSettings> freightSmartSettings)
        {
            _logger = logger;
            _freightSmartSettings = freightSmartSettings;
        }

        public IReadOnlyCollection<Cookie> GetOrReloadCookies()
        {
            return GetOrReloadDriver().Manage().Cookies.AllCookies;
        }

        internal IWebDriver GetOrReloadDriver()
        {
            if (!string.IsNullOrWhiteSpace(_loginDriver?.Url))
            {
                return _loginDriver;
            }

            Login();

            return _loginDriver;
        }

        private void Login()
        {
            var options = new ChromeOptions();
            options.AddArguments("--disable-plugins-discovery");
            options.AddArguments("--incognito");

            _loginDriver = new ChromeDriver(options);

            _loginDriver.Manage().Window.Maximize();
            _loginDriver.Manage().Cookies.DeleteAllCookies();

            _loginDriver.Navigate().GoToUrl(_freightSmartSettings.Settings.Domain);

            if (_freightSmartSettings.Settings.AutoLogin)
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

                _loginDriver.Manage().Window.Minimize();
            }

            // Wait until logged in 
            new WebDriverWait(_loginDriver, TimeSpan.FromMinutes(30)).Until(d =>
                d.FindElement(By.XPath(
                    $"//a[@class='user-name' and contains(text(), '{_freightSmartSettings.Settings.Username}')]")));

            _logger.LogInformation($"Login Successful: {_freightSmartSettings.Settings.Domain}");
        }

        protected override void DisposeManagedResources()
        {
            _loginDriver?.Dispose();
        }
    }
}