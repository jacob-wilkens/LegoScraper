using System.Text.RegularExpressions;
using LegoScraper.Utils;
using OpenQA.Selenium;

namespace LegoScraper.Pages
{
    public static class MiniFigPage
    {
        private static By PriceGuideTab => By.XPath("//td[contains(text(),'Price Guide')]");
        private static By GroupByCurrencyCheckBox => By.XPath("//input[@id='_idchkPGGroupByCurrency']");
        private static By PriceGuideTable => By.XPath("((//td[contains(text(),'US Dollar')])/parent::*)/following-sibling::tr");
        private static Regex AveragePricePattern => new(@"Avg Price: US \$(\d+\.\d+)");

        public static List<string> Scrape(IWebDriver driver, string condition)
        {
            Web.WaitAndClick(driver, PriceGuideTab);
            Web.WaitAndClick(driver, GroupByCurrencyCheckBox);

            var row = Web.WaitForElement(driver, PriceGuideTable);

            if (condition == "B")
            {
                var newCell = row.FindElement(By.XPath("td[1]"));
                var usedCell = row.FindElement(By.XPath("td[2]"));

                var newPriceMatch = AveragePricePattern.Match(newCell.Text);
                var usedPriceMatch = AveragePricePattern.Match(usedCell.Text);

                return
                [
                    newPriceMatch.Success ? newPriceMatch.Groups[1].Value : "N/A",
                    usedPriceMatch.Success ? usedPriceMatch.Groups[1].Value : "N/A"
                ];
            }

            var cell = condition == "N" ? 1 : 2;
            var cellTable = row.FindElement(By.XPath($"td[{cell}]"));
            var txt = cellTable.Text;
            var match = AveragePricePattern.Match(txt);

            return [match.Success ? match.Groups[1].Value : "N/A"];
        }
    }
}