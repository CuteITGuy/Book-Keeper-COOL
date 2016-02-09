using System.Collections.Generic;
using System.Text.RegularExpressions;
using BookDatabase;
using CB.Model.Common;
using CB.Primitives.Extensions;


namespace AmazonSearcher
{
    public class AmazonBookInfo: ObservableObject
    {
        #region Fields
        private IEnumerable<string> _authors;
        private int? _edition = 1;
        private string _generalInfo;
        private string _isbn;
        private decimal? _price;
        private string _publisher;
        private decimal? _rating;
        private int? _review;
        private string _title;

        private string _titleInfo;
        private string _url;
        private int? _year;
        #endregion


        #region  Properties & Indexers
        public IEnumerable<string> Authors
        {
            get { return _authors; }
            set { SetProperty(ref _authors, value); }
        }

        public int? Edition
        {
            get { return _edition; }
            set { if (SetProperty(ref _edition, value)) SetInfos(); }
        }

        public string GeneralInfo
        {
            get { return _generalInfo; }
            private set { SetProperty(ref _generalInfo, value); }
        }

        public string Isbn
        {
            get { return _isbn; }
            set { SetProperty(ref _isbn, value); }
        }

        public decimal? Price
        {
            get { return _price; }
            set { SetProperty(ref _price, value); }
        }

        public string Publisher
        {
            get { return _publisher; }
            set { SetProperty(ref _publisher, value); }
        }

        public decimal? Rating
        {
            get { return _rating; }
            set { if (SetProperty(ref _rating, value)) SetInfos(); }
        }

        public int? Review
        {
            get { return _review; }
            set { if (SetProperty(ref _review, value)) SetInfos(); }
        }

        public string Title
        {
            get { return _title; }
            set { if (SetProperty(ref _title, value)) SetInfos(); }
        }

        public string TitleInfo
        {
            get { return _titleInfo; }
            set { SetProperty(ref _titleInfo, value); }
        }

        public string Url
        {
            get { return _url; }
            set { SetProperty(ref _url, value); }
        }

        public int? Year
        {
            get { return _year; }
            set { if (SetProperty(ref _year, value)) SetInfos(); }
        }
        #endregion


        #region Implementation
        private static string CreateEditionText(int? edition)
            => !edition.HasValue || edition.Value < 2 ? "" : $@" {NumberHandler.ToOrdinal(edition.Value)} edition";

        private static string CreateRatingReviewText(decimal? rating, int? review)
            => !rating.HasValue || !review.HasValue ? "" : $" ({rating}/{review})";

        private static string CreateTitleText(string title)
            => string.IsNullOrEmpty(title)
                   ? ""
                   : title.RegexReplace(@"\s*[\:]\s*", " - ").RegexReplace(@"[<>?""'\\|]", "");

        private static string CreateYearText(int? year)
            => !year.HasValue ? string.Empty : " " + year;

        private void SetInfos()
        {
            TitleInfo = $"{CreateTitleText(Title)}{CreateEditionText(Edition)}{CreateYearText(Year)}";
            GeneralInfo =
                $"{Title}{CreateEditionText(Edition)}{CreateYearText(Year)}{CreateRatingReviewText(Rating, Review)}";
        }
        #endregion
    }
}


//TODO: Ordinal number string to number: Implement