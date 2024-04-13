using System.Text.RegularExpressions;
using HtmlAgilityPack;
using LegoScraper.Interfaces;
using LegoScraper.Utils;
using Microsoft.Extensions.Logging;

namespace LegoScraper.Services
{
    public class LegoClient(HttpClient client, ILogger<LegoClient> logger) : ILegoClient
    {
        private readonly HttpClient _client = client;
        private readonly ILogger<LegoClient> _logger = logger;
        private readonly Regex _pattern = new(@"Avg Price:US \$(\d+\.\d+)");

        public async Task<List<string>> Scrape(string url, string condition)
        {
            var prices = new List<string>();

            try
            {
                var response = await _client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                return ParseContent(content, condition);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error scraping {url}: {ex}", url, ex);
                prices.Add(Constants.EmptyRecord);
                prices.Add(Constants.EmptyRecord);
            }

            return prices;
        }

        private List<string> ParseContent(string content, string condition)
        {
            var prices = new List<string>();
            var html = new HtmlDocument();

            html.LoadHtml(content);
            var nodes = html.DocumentNode.SelectNodes("//tr[contains(.,'New')]");

            if (nodes == null || nodes.Count < 2) return prices;

            var node = nodes.ElementAt(1).NextSibling;

            var elements = new List<HtmlNode>();

            switch (condition)
            {
                case "N":
                    elements.Add(node.ChildNodes.ElementAt(0));
                    break;
                case "U":
                    elements.Add(node.ChildNodes.ElementAt(1));
                    break;
                case "B":
                    elements.Add(node.ChildNodes.ElementAt(0));
                    elements.Add(node.ChildNodes.ElementAt(1));
                    break;
                default:
                    return prices;
            }

            foreach (var element in elements)
            {
                var priceText = Regex.Unescape(element.InnerText).Replace("&nbsp;", " ");
                var match = _pattern.Match(priceText);

                var price = match.Success ? match.Groups[1].Value : Constants.EmptyRecord;

                prices.Add(price);
            }

            return prices;
        }
    }
}