using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace AmazonSearcher
{
    public class AmazonBookInfo
    {
        #region  Properties & Indexers
        public IEnumerable<string> Authors { get; set; }

        public int? Edition { get; set; } = 1;

        public string Isbn { get; set; }

        public decimal? Price { get; set; }

        public string Publisher { get; set; }

        public decimal? Rating { get; set; }

        public int? Review { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public int? Year { get; set; }
        #endregion


        #region Implementation
        private static decimal? CaptureDecimal(string info, string pattern = @"(\d+(\.\d+)?)")
        {
            var capturedString = CaptureString(info, pattern);
            return capturedString == null ? null : decimal.Parse(capturedString) as decimal?;
        }

        private static int? CaptureInt(string info, string pattern = @"(\d+)")
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

        internal void ParseAuthorsInfo(string info)
        {
            Authors = string.IsNullOrWhiteSpace(info) ? null : Regex.Split(info, @"[,]");
        }

        internal void ParseEditionInfo(string info)
        {
            Edition = CaptureInt(info);
        }

        internal void ParseIsbnInfo(string info)
        {
            Isbn = string.IsNullOrWhiteSpace(info) ? null : Regex.Replace(info, @"\D", "");
        }

        internal void ParsePriceInfo(string info)
        {
            Price = CaptureDecimal(info);
        }

        internal void ParsePublisherInfo(string info)
        {
            Publisher = CaptureString(info, @"^Publisher: (.+);?(?: \()?");
            Year = CaptureInt(info, @"(\d{4})\)$");
        }

        internal void ParseRatingInfo(string info)
        {
            Rating = CaptureDecimal(info, @"(\d(?:\.\d)?) out of 5 stars");
        }

        internal void ParseReviewInfo(string info)
        {
            Review = CaptureInt(info);
        }
        #endregion
    }
}


//TODO: Ordinal number string to number: Implement