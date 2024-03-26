using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace LegoScraper.Utils
{
    public static class Web
    {
        public static void WaitAndClick(IWebDriver driver, By by)
        {
            var element = driver.FindElement(by);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.IgnoreExceptionTypes(typeof(ElementNotInteractableException));
            wait.Until(_ => element.Displayed && element.Enabled);
            element.Click();
        }

        public static IWebElement WaitForElement(IWebDriver driver, By by)
        {
            var element = driver.FindElement(by);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
            wait.Until(_ => element.Displayed);
            return element;
        }
    }
}