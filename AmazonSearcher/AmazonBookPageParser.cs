using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CefSharp;


namespace AmazonSearcher
{
    public class AmazonBookPageParser
    {
        #region Fields
        private const string AUTHORS_SELECTOR =
            ".author.notFaded > a.a-link-normal, .author.notFaded > span > a.a-link-normal";

        private const string DECIMAL_CAPTURE = @"(\d+(\.\d+)?)";
        private const string EDITION_CAPTURE = @"(?i)(\d+)(?:\w{2})? edition";
        private const string EDITION_SELECTOR = "#bookEdition";
        private const string INTEGER_CAPTURE = @"(\d+)";
        private const string ISBN_SELECTOR_1 = "span:contains(ISBN-13) + span";
        private const string ISBN_SELECTOR_2 = "#detail-bullets li:contains(ISBN-13:)";
        private const string PRICE_SELECTOR = ".a-size-medium.a-color-price";
        private const string PUBLICATION_DATE_SELECTOR = "#detail-bullets li:contains(Publication Date:)";
        private const string PUBLISHER_CAPTURE = @"^Publisher: ([^;\(\)]+);?";
        private const string PUBLISHER_SELECTOR = "#detail-bullets li:contains(Publisher:)";
        private const string RATING_CAPTURE = @"(\d(?:\.\d)?) out of 5 stars";
        private const string RATING_SELECTOR = ".a-icon.a-icon-star span";
        private const string REVIEW_SELECTOR = "#acrCustomerReviewText";
        private const string TITLE_SELECTOR = "#productTitle";
        private const string YEAR_CAPTURE = @"(\d{4})\)$";
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(3);
        private IWebBrowser _webMain;
        #endregion


        #region Methods
        public async Task<AmazonBookInfo> FetchBookInfo(IWebBrowser webMain)
        {
            _webMain = webMain;
            var bookInfo = new AmazonBookInfo
            {
                Url = webMain.Address,
                Title = await GetBookTitle(),
                Edition = await GetBookEdition(),
                Rating = await GetBookRating(),
                Review = await GetBookReview(),
                Authors = await GetBookAuthors(),
                Price = await GetBookPrice(),
                Isbn = await GetBookIsbn(),
                Publisher = await GetBookPublisher(),
                Year = await GetBookYear()
            };
            return bookInfo;
        }
        #endregion


        #region Implementation
        private static decimal? CaptureDecimal(string info, string pattern = DECIMAL_CAPTURE)
        {
            var capturedString = CaptureString(info, pattern);
            return capturedString == null ? null : decimal.Parse(capturedString) as decimal?;
        }

        private static int? CaptureInt(string info, string pattern = INTEGER_CAPTURE)
        {
            var capturedString = CaptureString(info, pattern);
            return capturedString == null ? null : int.Parse(capturedString) as int?;
        }

        private static string CaptureString(string info, string pattern)
        {
            if (string.IsNullOrWhiteSpace(info)) return null;
            var match = Regex.Match(info, pattern);
            return match.Groups.Count < 2 ? null : match.Groups[1].Value;
        }

        private static string CreateGetTextContentScript(string selector)
            => $"var ele = jQuery(\"{selector}\")[0]; ele && ele.textContent;";

        private async Task<string[]> GetBookAuthors()
        {
            var script = $"var eles = jQuery(\"{AUTHORS_SELECTOR}\");" +
                         "Array.prototype.join.call(eles.map(function(ele) { return this.textContent; } ), \",\");";
            var authorsInfo = (await _webMain.EvaluateScriptAsync(script, _timeout)).Result as string;
            return string.IsNullOrWhiteSpace(authorsInfo) ? null : Regex.Split(authorsInfo, @"[,]");
        }

        private async Task<int?> GetBookEdition()
        {
            return CaptureInt(await GetElementText(EDITION_SELECTOR)) ??
                   CaptureInt(await GetElementText(PUBLISHER_SELECTOR), EDITION_CAPTURE);
        }

        private async Task<string> GetBookIsbn()
        {
            var isbnInfo = await GetElementText(ISBN_SELECTOR_1);
            if (string.IsNullOrWhiteSpace(isbnInfo)) isbnInfo = await GetElementText(ISBN_SELECTOR_2);
            return string.IsNullOrWhiteSpace(isbnInfo) ? null : Regex.Replace(isbnInfo, @"\D", "");
        }

        private async Task<decimal?> GetBookPrice()
        {
            return CaptureDecimal(await GetElementText(PRICE_SELECTOR));
        }

        private async Task<string> GetBookPublisher()
        {
            return CaptureString(await GetElementText(PUBLISHER_SELECTOR), PUBLISHER_CAPTURE);
        }

        private async Task<decimal?> GetBookRating()
        {
            return CaptureDecimal(await GetElementText(RATING_SELECTOR), RATING_CAPTURE);
        }

        private async Task<int?> GetBookReview()
        {
            return CaptureInt(await GetElementText(REVIEW_SELECTOR));
        }

        private async Task<string> GetBookTitle()
        {
            return await GetElementText(TITLE_SELECTOR);
        }

        private async Task<int?> GetBookYear()
        {
            return CaptureInt(await GetElementText(PUBLISHER_SELECTOR), YEAR_CAPTURE) ??
                   CaptureInt(await GetElementText(PUBLICATION_DATE_SELECTOR), YEAR_CAPTURE);
        }

        private async Task<string> GetElementText(string selector)
        {
            var script = CreateGetTextContentScript(selector);
            return (await _webMain.EvaluateScriptAsync(script, _timeout)).Result as string;
        }
        #endregion
    }
}