namespace LegoScraper.Utils
{
    public static class Constants
    {
        public const string MiniFigCsvFile = "updated_minifigs.csv";
        public const string BaseUrl = "https://www.bricklink.com/v2/catalog/catalogitem.page";
        public const string LegoSetCsvFile = "updated_lego_sets.csv";

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