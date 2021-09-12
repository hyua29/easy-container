namespace EasyContainer.Service.TicketPurchaseJobs
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using Lib;
    using Lib.Extensions;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using Settings;
    using Settings.SettingExtensions;

    public interface ITicketPurchaseJob : IAsyncDisposable

    {
        void Schedule();
    }

    public class TicketPurchaseJob : ITicketPurchaseJob
    {
        private readonly CancellationToken _cancellationToken;

        private readonly ISettingWrapper<FreightSmartSettings> _freightSmartSettings;

        private readonly ISettingWrapper<SeleniumSettings> _seleniumSettings;

        private readonly RouteSettings _routeSettings;

        private readonly IReadOnlyCollection<Cookie> _cookieCollection;

        private readonly ILogger<TicketPurchaseJob> _logger;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private Task _taskHandle = null;

        public TicketPurchaseJob(ISettingWrapper<FreightSmartSettings> freightSmartSettings,
            ISettingWrapper<SeleniumSettings> seleniumSettings, RouteSettings routeSettings,
            IReadOnlyCollection<Cookie> cookieCollection,
            ILogger<TicketPurchaseJob> logger,
            CancellationToken cancellationToken = default)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _freightSmartSettings = freightSmartSettings;
            _seleniumSettings = seleniumSettings;
            _routeSettings = routeSettings;
            _cookieCollection = cookieCollection;
            _cancellationToken = cancellationToken;
            _logger = logger;
        }

        public void Schedule()
        {
            _taskHandle = RunAsync();
        }
        private async Task RunAsync()
        {
            var details = JsonConvert.SerializeObject(_routeSettings);
            var hash = SHA256.Create(details);

            try
            {
                var releaseTime = SettingHelper.StringToDateTimeNoThrow(_routeSettings.TicketReleaseTime, isUtc: false);
                if (releaseTime.HasValue)
                {
                    await MaybeWaitAsync(releaseTime.Value, _seleniumSettings.Settings.GetPreparationDurationTimeSpan())
                        .ConfigureAwait(false);
                }

                var currentPosition = new Point(0, 0);
                var positionShift = 10;
                IList<Task> tasks = new List<Task>();
                for (var i = 0; i < _routeSettings.WindowCount; i++)
                {
                    if (!_cancellationToken.IsCancellationRequested)
                    {
                        currentPosition = new Point(currentPosition.X + positionShift,
                            currentPosition.Y + positionShift);

                        var position = new Point(currentPosition.X, currentPosition.Y);

                        tasks.Add(Task.Run(() => RunImplAsync(position), _cancellationToken));
                    }
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (OperationCanceledException e)
            {
                using (_logger.BeginScope(new Dictionary<string, object> {["BrowserJobHash"] = hash}))
                {
                    _logger.LogInformation("Browser job is cancelled");
                    _logger.LogInformation($"Job detailed: {details}");
                }
            }
            catch (Exception e)
            {
                using (_logger.BeginScope(new Dictionary<string, object> {["BrowserJobHash"] = hash}))
                {
                    _logger.LogError(e, "Job cannot be executed");
                    _logger.LogInformation($"Job detailed: {details}");
                }
            }
        }

        /// <summary>
        /// Wait until target time. No wait if target time has elapsed
        /// </summary>
        /// <param name="targetTime">Wake up time</param>
        /// <param name="period">Wake up before target time</param>
        private async Task MaybeWaitAsync(DateTime targetTime, TimeSpan? period = null)
        {
            var waitTime = targetTime - DateTime.Now;

            if (waitTime > (period ?? TimeSpan.Zero))
                await Task.Delay(waitTime, _cancellationToken).ConfigureAwait(false);
        }

        private async Task RunImplAsync(Point position)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            using var driver = CreateDriver(position);

            driver.Navigate().GoToUrl(_freightSmartSettings.Settings.Domain);
            driver.FindElement(By.XPath("//div[@class='ctrl-footer']//a")).Click();
            if (!_seleniumSettings.Settings.IsTesting)
            {
                _cookieCollection.ForEach(c => { driver.Manage().Cookies.AddCookie(c); });
                driver.Navigate().Refresh();
            }

            var releaseTime = SettingHelper.StringToDateTimeNoThrow(_routeSettings.TicketReleaseTime, isUtc: false);
            if (releaseTime.HasValue)
            {
                await MaybeWaitAsync(releaseTime.Value, _seleniumSettings.Settings.GetPreExecutionTimeSpan())
                    .ConfigureAwait(false);
            }

            var startTime = releaseTime ?? DateTime.Now;
            var stopTime = startTime + TimeSpan.FromSeconds(_routeSettings.ExecutionDuration);

            // Repeatedly attempt to buy the ticket until exceeding max attempt duration
            while (true)
            {
                if (DateTime.Now > stopTime) break;
                if (_seleniumSettings.Settings.IsTesting)
                {
                    driver.FindElement(By.XPath("//div[@role='switch']")).Click();
                    await Task.Delay(500, _cancellationToken).ConfigureAwait(false);
                    Thread.Sleep(500);
                    continue;
                }

                // TODO: Perform actual job
                await Task.Delay(500, _cancellationToken).ConfigureAwait(false);
            }
        }

        private IWebDriver CreateDriver(Point position)
        {
            var options = new ChromeOptions();
            options.AddArguments("--incognito");
            options.AddArguments("--disable-plugins-discovery");

            var driver = new ChromeDriver(options);

            driver.Manage().Window.Position = position;
            driver.Manage().Cookies.DeleteAllCookies();

            return driver;
        }

        public async ValueTask DisposeAsync()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            await _taskHandle.ConfigureAwait(false);
        }
    }
}