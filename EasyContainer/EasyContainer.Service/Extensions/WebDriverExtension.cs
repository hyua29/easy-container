namespace EasyContainer.Service.Extensions
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    public static class WebDriverExtension
    {
        public static IWebElement TryFindElement(this IWebDriver driver, By by, TimeSpan? timeout = null)
        {
            try
            {
                timeout ??= TimeSpan.FromMilliseconds(100);
                return new WebDriverWait(driver, timeout.Value).Until(d =>
                    d.FindElement(by));
            }
#pragma warning disable CS0168 // Intentionally catch exception if the element cannot be found
            catch (WebDriverTimeoutException e)
#pragma warning restore CS0168 // Intentionally catch exception if the element cannot be found
            {
                return null;
            }
        }

        public static async Task<bool> WaitUntilElementDisappearsAsync(
            this IWebDriver driver,
            By by,
            TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.MaxValue;
            var stopTime = DateTime.UtcNow + timeout.Value;
            do
            {
                await Task.Delay(50).ConfigureAwait(false);
            } while (driver.FindElements(by).Any() && stopTime - DateTime.UtcNow > TimeSpan.Zero);

            return false;
        }
    }
}