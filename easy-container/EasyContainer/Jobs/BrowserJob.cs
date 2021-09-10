namespace EasyContainer.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;

    public interface IBrowserJob : IDisposable

    {
        Task RunAsync();
    }

    public class BrowserJob : BaseDisposable, IBrowserJob
    {
        private readonly AppSettings _appSettings;

        private readonly CancellationToken _cancellationToken;

        private readonly IReadOnlyCollection<Cookie> _cookieCollection;

        private readonly ILogger _logger;

        private readonly IWebDriver _webDriver;

        public BrowserJob(AppSettings appSettings, IReadOnlyCollection<Cookie> cookieCollection, Point position,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            _appSettings = appSettings;
            var options = new ChromeOptions();
            options.AddArguments("--incognito");
            options.AddArguments("--disable-plugins-discovery");

            _webDriver = new ChromeDriver(options);

            _webDriver.Manage().Window.Position = position;
            _webDriver.Manage().Cookies.DeleteAllCookies();

            _cookieCollection = cookieCollection;
            _cancellationToken = cancellationToken;
            _logger = logger;
        }

        public Task RunAsync()
        {
            return Task.Run(RunImpl, _cancellationToken);
        }

        private void RunImpl()
        {
            try
            {
                _webDriver.Navigate().GoToUrl(_appSettings.FreightSmart.Domain);
                // _cookieCollection.ForEach(c => { _webDriver.Manage().Cookies.AddCookie(c); });
                // _webDriver.Navigate().Refresh();

                _webDriver.FindElement(By.XPath("//div[@class='ctrl-footer']//a")).Click();
                while (true)
                {
                    _webDriver.FindElement(By.XPath("//div[@role='switch']")).Click();
                    Thread.Sleep(500);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void DisposeManagedResources()
        {
            _webDriver?.Dispose();
        }
    }
}