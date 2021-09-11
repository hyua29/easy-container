namespace EasyContainer.Extensions
{
    using System;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    public static class WebDriverExtension
    {
        public static IWebElement TryFindElement(this IWebDriver driver, By by, TimeSpan timeout)
        {
            try
            {
                return new WebDriverWait(driver, timeout).Until(d =>
                    d.FindElement(by));
            }
#pragma warning disable CS0168 // Intentionally catch exception if the element cannot be found
            catch (WebDriverTimeoutException e)
#pragma warning restore CS0168 // Intentionally catch exception if the element cannot be found
            {
                return null;
            }
        }
    }
}