namespace EasyContainer.Service.TicketPurchaseJobs
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensions;
    using Lib;
    using Lib.Extensions;
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

        private readonly LoginDriver _loginDriver;

        private readonly IDateTimeSupplier _dateTimeSupplier;

        private readonly Action<string> _cleanupAction;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private Task _taskHandle;

        public TicketPurchaseJob(
            ILogger logger,
            ISettingWrapper<FreightSmartSettings> freightSmartSettings,
            LaneSettings laneSettings,
            LoginDriver loginDriver,
            Action<string> cleanupAction = null,
            IDateTimeSupplier dateTimeSupplier = null,
            CancellationToken cancellationToken = default)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _freightSmartSettings = freightSmartSettings;
            _laneSettings = laneSettings;
            _logger = logger;
            _loginDriver = loginDriver;
            _cleanupAction = cleanupAction;
            _dateTimeSupplier = dateTimeSupplier ?? new DateTimeSupplier();
            JobHash = _laneSettings.GetHash();
        }

        /// <summary>
        /// For testing
        /// </summary>
        internal event EventHandler OnPreparationWaitStarted;

        /// <summary>
        /// For testing
        /// </summary>
        internal event EventHandler OnPreExecutionWaitStarted;

        /// <summary>
        /// For testing
        /// </summary>
        internal event EventHandler OnJobStarted;

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

        private async Task RunAsync()
        {
            var details = JsonConvert.SerializeObject(_laneSettings);

            try
            {
                await MaybeWaitAsync(_laneSettings.GetReleaseTimeDateTime(),
                    _freightSmartSettings.Settings.GetPreparationOffsetTimeSpan(),
                    () => OnPreparationWaitStarted?.Invoke(this, EventArgs.Empty)).ConfigureAwait(false);

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

            if (!_freightSmartSettings.Settings.IsTesting)
            {
                driver.Navigate().GoToUrl("https://freightsmart.oocl.com/prebooking");

                var controlPanel = driver.FindElement(By.XPath("(//div[text()='目的港']/.."));

                var pol = controlPanel.FindElement(By.XPath("//input)[1]"));
                pol.Click();
                pol.SendKeys(_laneSettings.PortOfLoading);
                driver.FindElement(By.XPath($"//span[contains(text(), '{_laneSettings.PortOfLoading}')]")).Click();

                var pod = controlPanel.FindElement(By.XPath("//input)[2]"));
                pod.Click();
                pod.SendKeys(_laneSettings.PortOfDestination);
                driver.FindElement(By.XPath($"//span[contains(text(), '{_laneSettings.PortOfDestination}')]")).Click();

                var etol = controlPanel.FindElement(By.XPath("//input)[3]"));
                etol.Click();
                etol.SendKeys(_laneSettings.GetEarliestTimeOfDepartureDate().ToString("dd-MM-yyyy"));

                var ltol = controlPanel.FindElement(By.XPath("//input)[4]"));
                ltol.Click();
                ltol.SendKeys(_laneSettings.GetLatestTimeOfDepartureDate().ToString("dd-MM-yyyy"));
            }

            var releaseTime = _laneSettings.GetReleaseTimeDateTime();
            await MaybeWaitAsync(releaseTime, _freightSmartSettings.Settings.GetPreExecutionOffsetTimeSpan(),
                    () => OnPreExecutionWaitStarted?.Invoke(this, EventArgs.Empty))
                .ConfigureAwait(false);

            var stopTime = releaseTime + TimeSpan.FromSeconds(_laneSettings.ExecutionDuration);

            OnJobStarted?.Invoke(this, EventArgs.Empty);
            // Repeatedly attempt to buy the ticket until exceeding max attempt duration
            while (_dateTimeSupplier.Now < stopTime)
            {
                if (_freightSmartSettings.Settings.IsTesting)
                {
                    _logger.LogInformation("Testing mode is enabled, no action taken");
                    await Task.Delay(1000, _cancellationTokenSource.Token).ConfigureAwait(false);
                    continue;
                }

                driver.FindElement(By.XPath($"//span[contains(text(), '查 询')]")).Click();
                driver.FindElement(
                    By.XPath(
                        "//div[@id='prebookingGroupTable' and @class='product-result-content']")); // Make sure page is fully loaded
                var lane = driver.FindElements(By.XPath(
                        $"//span[contains(text(), '{_laneSettings.LaneId}')]/../..//span[contains(text(), '直接购买')]"))
                    ?.First();

                if (lane != null)
                {
                    lane.Click();
                    break;
                }
            }
        }

        /// <summary>
        /// Wait until target time. No wait if target time has elapsed
        /// </summary>
        /// <param name="wakeUpTime">Wake up time</param>
        /// <param name="period">Wake up before target time</param>
        /// <param name="action">Action that needs to be taken fore entering the wait</param>
        private async Task MaybeWaitAsync(DateTime wakeUpTime, TimeSpan? period = null, Action action = null)
        {
            var waitTime = wakeUpTime - _dateTimeSupplier.Now - (period ?? TimeSpan.Zero);

            if (waitTime > TimeSpan.Zero)
            {
                action?.Invoke();
                await Task.Delay(waitTime, _cancellationTokenSource.Token).ConfigureAwait(false);
            }
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
            driver.FindElements(By.XPath("//div[@role='switch' and contains(@class,'is-checked')]")).ForEach(
                e => { e.Click(); });
            driver
                .FindElement(By.XPath(
                    "//button[@type='button' and contains(@class,'button--danger')]//*[text() = 'Update Preference']"))
                .Click();
            if (!_freightSmartSettings.Settings.IsTesting)
            {
                _loginDriver.GetOrReloadCookies().ForEach(c => { driver.Manage().Cookies.AddCookie(c); });
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