namespace EasyContainer.Service.Extensions
{
    using System.Linq;
    using OpenQA.Selenium;

    public static class SearchContextExtension
    {
        public static IWebElement TryFindElement(this ISearchContext searchContext, By by)
        {
            return searchContext.FindElements(by).FirstOrDefault();
        }
    }
}