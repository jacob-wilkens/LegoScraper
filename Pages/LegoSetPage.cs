using System.Text.RegularExpressions;
using LegoScraper.Utils;
using OpenQA.Selenium;

namespace LegoScraper.Pages
{
    public static class LegoSetPage
    {
        private static By CookieButton => By.XPath("//div[@id='js-btn-section']//button[contains(text(),'Just necessary')]");
        private static By PriceGuideTab => By.XPath("//td[contains(text(),'Price Guide')]");
        private static By GroupByCurrencyCheckBox => By.XPath("//input[@id='_idchkPGGroupByCurrency']");
        private static By ExcludeIncompleteCheckBox => By.XPath("//input[@id='_idchkPGExcludeIncomplete']");
        private static By PriceGuideTable => By.XPath("((//td[contains(text(),'US Dollar')])/parent::*)/following-sibling::tr");
        private static Regex AveragePricePattern => new(@"Avg Price: US \$(\d+\.\d+)");

        public static string Scrape(IWebDriver driver, string condition, int index)
        {
            if (index == 0) Web.WaitAndClick(driver, CookieButton);

            Web.WaitAndClick(driver, PriceGuideTab);
            Web.WaitAndClick(driver, GroupByCurrencyCheckBox);
            Web.WaitAndClick(driver, ExcludeIncompleteCheckBox);

            var row = Web.WaitForElement(driver, PriceGuideTable);
            var cell = condition == "N" ? 1 : 2;
            var cellTable = row.FindElement(By.XPath($"td[{cell}]"));
            var txt = cellTable.Text;
            var match = AveragePricePattern.Match(txt);

            return match.Success ? match.Groups[1].Value : "N/A";
        }
    }
}