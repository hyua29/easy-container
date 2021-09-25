namespace EasyContainer.Service.TicketPurchaseJobs
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using Lib;
    using Lib.Extensions;
    using Lib.Utilities;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Nito.AsyncEx.Synchronous;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using Settings;
    using Settings.SettingExtensions;

    public interface ITicketPurchaseJob : IDisposable

    {
        void Schedule();

        void CancelIfStarted();
    }

    public class TicketPurchaseJob : ITicketPurchaseJob
    {
        private readonly ISettingWrapper<FreightSmartSettings> _freightSmartSettings;

        private readonly LaneSettings _laneSettings;

        private readonly ILogger _logger;

        private readonly Func<IReadOnlyCollection<Cookie>> _getCookies;

        private readonly Action<string> _cleanupAction;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private Task _taskHandle;

        public TicketPurchaseJob(ISettingWrapper<FreightSmartSettings> freightSmartSettings,
            LaneSettings laneSettings,
            ILogger logger,
            Func<IReadOnlyCollection<Cookie>> getCookies = null,
            Action<string> cleanupAction = null,
            CancellationToken cancellationToken = default)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _freightSmartSettings = freightSmartSettings;
            _laneSettings = laneSettings;
            _logger = logger;
            _getCookies = getCookies;
            _cleanupAction = cleanupAction;
            JobHash = _laneSettings.GetHash();
        }

        public void Schedule()
        {
            if (_taskHandle != null)
            {
                throw new InvalidOperationException($"Each {nameof(TicketPurchaseJob)} can only be scheduled once");
            }

            _taskHandle = RunAsync();
        }

        public void CancelIfStarted()
        {
            Dispose();
        }

        public string JobHash { get; }

        private Task WaitUntilAppropriateTimeAsync()
        {
            return MaybeWaitAsync(_laneSettings.GetReleaseTimeDateTime(),
                _freightSmartSettings.Settings.GetPreparationOffsetTimeSpan());
        }

        private async Task RunAsync()
        {
            var details = JsonConvert.SerializeObject(_laneSettings);

            try
            {
                await WaitUntilAppropriateTimeAsync().ConfigureAwait(false);

                await RunImplAsyncPurchaseTicketAsync().ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Browser job is cancelled");
                _logger.LogInformation($"Job detailed: {details}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Job cannot be executed");
                _logger.LogInformation($"Job detailed: {details}");
            }
            finally
            {
                _cleanupAction?.Invoke(JobHash);
            }
        }

        /// <summary>
        /// Open Multiple browsers and attempt to purchase tickets
        /// </summary>
        /// <returns></returns>
        private Task RunImplAsyncPurchaseTicketAsync()
        {
            var currentPosition = new Point(0, 0);
            var positionShift = 10;
            IList<Task> tasks = new List<Task>();
            for (var i = 0; i < _laneSettings.WindowCount; i++)
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    currentPosition = new Point(currentPosition.X + positionShift,
                        currentPosition.Y + positionShift);

                    var position = new Point(currentPosition.X, currentPosition.Y);

                    tasks.Add(Task.Run(() => PurchaseTicketAsync(position), _cancellationTokenSource.Token));
                }
            }

            return Task.WhenAll(tasks);
        }

        private async Task PurchaseTicketAsync(Point position)
        {
            _cancellationTokenSource.Token.ThrowIfCancellationRequested();

            using var driver = CreateDriver(position);

            var releaseTime = SettingHelper.StringToDateTimeNoThrow(_laneSettings.TicketReleaseTime, isUtc: false);
            if (releaseTime.HasValue)
            {
                await MaybeWaitAsync(releaseTime.Value, _freightSmartSettings.Settings.GetPreExecutionTimeSpan())
                    .ConfigureAwait(false);
            }

            var startTime = releaseTime ?? DateTime.Now;
            var stopTime = startTime + TimeSpan.FromSeconds(_laneSettings.ExecutionDuration);

            // Repeatedly attempt to buy the ticket until exceeding max attempt duration
            while (true)
            {
                if (DateTime.Now > stopTime) break;
                if (_freightSmartSettings.Settings.IsTesting)
                {
                    driver.FindElement(By.XPath("//div[@role='switch']")).Click();
                    await Task.Delay(500, _cancellationTokenSource.Token).ConfigureAwait(false);
                    Thread.Sleep(500);
                    continue;
                }

                // TODO: Perform actual job
                await Task.Delay(500, _cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Wait until target time. No wait if target time has elapsed
        /// </summary>
        /// <param name="wakeUpTime">Wake up time</param>
        /// <param name="period">Wake up before target time</param>
        private async Task MaybeWaitAsync(DateTime wakeUpTime, TimeSpan? period = null)
        {
            var waitTime = wakeUpTime - DateTime.Now;

            if (waitTime > (period ?? TimeSpan.Zero))
                await Task.Delay(waitTime, _cancellationTokenSource.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Create a driver and set cookies accordingly
        /// </summary>
        /// <param name="position">Position where the browser will be opened</param>
        /// <returns></returns>
        private IWebDriver CreateDriver(Point position)
        {
            var options = new ChromeOptions();
            options.AddArguments("--incognito");
            options.AddArguments("--disable-plugins-discovery");

            var driver = new ChromeDriver(options);

            driver.Manage().Window.Position = position;
            driver.Manage().Cookies.DeleteAllCookies();

            driver.Navigate().GoToUrl(_freightSmartSettings.Settings.Domain);
            driver.FindElement(By.XPath("//div[@class='ctrl-footer']//a")).Click();
            if (!_freightSmartSettings.Settings.IsTesting)
            {
                _getCookies().ForEach(c => { driver.Manage().Cookies.AddCookie(c); });
                driver.Navigate().Refresh();
            }

            return driver;
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _taskHandle?.WaitWithoutException();
            _taskHandle?.Dispose();
        }
    }
}