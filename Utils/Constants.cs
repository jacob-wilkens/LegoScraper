namespace LegoScraper.Utils
{
    public static class Constants
    {
        public const string EmptyRecord = "N/A";
        public const string BaseUrl = "https://www.bricklink.com/catalogPG.asp";
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