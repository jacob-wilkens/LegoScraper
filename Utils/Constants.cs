using OpenQA.Selenium;

namespace LegoScraper.Utils
{
    public static class Constants
    {
        public const string EmptyRecord = "N/A";
        public const string BaseUrl = "https://www.bricklink.com/v2/catalog/catalogitem.page";
        public static By CookieButton => By.XPath("//div[@id='js-btn-section']//button[contains(text(),'Just necessary')]");

        public static Uri GetLegoSetUri(string id)
        {
            var uriBuilder = new UriBuilder(BaseUrl)
            {
                Query = $"S={id}"
            };

            return uriBuilder.Uri;
        }

        public static Uri GetMiniFigUri(string id)
        {
            var uriBuilder = new UriBuilder(BaseUrl)
            {
                Query = $"M={id}"
            };

            return uriBuilder.Uri;
        }
    }
}