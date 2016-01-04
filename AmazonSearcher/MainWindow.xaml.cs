using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;


namespace AmazonSearcher
{
    public partial class MainWindow
    {
        #region Fields
        private const string SEARCH_URL =
            "http://www.amazon.com/s/ref=nb_sb_noss?url=search-alias%3Dstripbooks&field-keywords=";

        private const string TITLE_SELECTOR = "#productTitle",
                             EDITION_SELECTOR = "#bookEdition",
                             AUTHORS_SELECTOR =
                                 ".author.notFaded > a.a-link-normal, .author.notFaded > span > a.a-link-normal",
                             RATING_SELECTOR = ".a-icon.a-icon-star span",
                             REVIEW_SELECTOR = "#acrCustomerReviewText",
                             PRICE_SELECTOR = ".a-size-medium.a-color-price",
                             ISBN_SELECTOR_1 = "span:contains(ISBN-13) + span",
                             ISBN_SELECTOR_2 = "#detail-bullets li:has(b:contains(ISBN-13:))",
                             PUBLISHER_SELECTOR = "#detail-bullets li:has(b:contains(Publisher:))";

        public static readonly RoutedEvent BookInfoSentEvent = EventManager.RegisterRoutedEvent(
            nameof(BookInfoSent), RoutingStrategy.Bubble, typeof(BookInfoEventHandler), typeof(MainWindow));

        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(2);
        #endregion


        #region  Constructors & Destructor
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion


        #region Events
        public event BookInfoEventHandler BookInfoSent
        {
            add { AddHandler(BookInfoSentEvent, value); }
            remove { RemoveHandler(BookInfoSentEvent, value); }
        }
        #endregion


        #region Methods
        public void SearchBookIsbn(string isbn)
        {
            if (string.IsNullOrEmpty(isbn)) return;

            var searchStr = SEARCH_URL + isbn;
            SearchString(searchStr);
        }

        public void SearchBookTitle(string title)
        {
            if (string.IsNullOrEmpty(title)) return;

            var searchStr = SEARCH_URL + Regex.Replace(title, @"\s+", "+");
            SearchString(searchStr);
        }
        #endregion


        #region Event Handlers
        private void CmdExpand_OnClick(object sender, RoutedEventArgs e)
        {
            if (pnlExpand.Visibility == Visibility.Collapsed)
            {
                cmdExpand.Content = "-";
                pnlExpand.Visibility = Visibility.Hidden;
            }
            else
            {
                cmdExpand.Content = "+";
                pnlExpand.Visibility = Visibility.Collapsed;
            }
        }

        private async void CmdFetch_OnClick(object sender, RoutedEventArgs e)
        {
            await FetchBookInfo();
        }

        private void CmdGo_OnClick(object sender, RoutedEventArgs e)
        {
            webMain.Address = txtAddress.Text;
        }

        private async void CmdRun_OnClick(object sender, RoutedEventArgs e)
        {
            var result = await RunScript(txtScript.Text);
            ShowResult(result);
        }

        private void CmdSend_OnClick(object sender, RoutedEventArgs e)
        {
            SendBookInfo();
        }

        private void CmdSendClose_OnClick(object sender, RoutedEventArgs e)
        {
            SendBookInfo();
            Close();
        }
        #endregion


        #region Implementation
        private static string CreateScript(string selector)
        {
            return $"var ele = jQuery(\"{selector}\")[0]; ele && ele.textContent;";
            /*return $"jQuery(\"{selector}\")[0].textContent;";*/
        }

        private async Task<AmazonBookInfo> FetchBookInfo()
        {
            var bookInfo = await GetBookInfo();
            pnlBookInfo.DataContext = bookInfo;
            return bookInfo;
        }

        private async Task<string> GetAuthorsInfo()
        {
            var script = $"var eles = jQuery(\"{AUTHORS_SELECTOR}\");" +
                         "Array.prototype.join.call(eles.map(function(ele) { return this.textContent; } ), \",\");";
            return (await webMain.EvaluateScriptAsync(script, _timeout)).Result as string;
        }

        private async Task<AmazonBookInfo> GetBookInfo()
        {
            var bookInfo = new AmazonBookInfo
            {
                Url = webMain.Address,
                Title = await GetElementText(TITLE_SELECTOR)
            };
            bookInfo.ParseEditionInfo(await GetElementText(EDITION_SELECTOR));
            bookInfo.ParseRatingInfo(await GetElementText(RATING_SELECTOR));
            bookInfo.ParseReviewInfo(await GetElementText(REVIEW_SELECTOR));
            bookInfo.ParsePriceInfo(await GetElementText(PRICE_SELECTOR));
            bookInfo.ParsePublisherInfo(await GetElementText(PUBLISHER_SELECTOR));
            var isbnInfo = await GetElementText(ISBN_SELECTOR_1);
            if (string.IsNullOrWhiteSpace(isbnInfo)) isbnInfo = await GetElementText(ISBN_SELECTOR_2);
            bookInfo.ParseIsbnInfo(isbnInfo);
            bookInfo.ParseAuthorsInfo(await GetAuthorsInfo());
            return bookInfo;
        }

        private async Task<string> GetElementText(string selector)
        {
            var script = CreateScript(selector);
            return (await webMain.EvaluateScriptAsync(script, _timeout)).Result as string;
        }

        protected virtual void OnBookSelected(AmazonBookInfo info)
        {
            RaiseEvent(new BookInfoEventArgs(BookInfoSentEvent, this, info));
        }

        private async Task<object> RunScript(string script)
        {
            return (await webMain.EvaluateScriptAsync(script, _timeout)).Result;
        }

        private void SearchString(string searchStr)
        {
            if (string.IsNullOrWhiteSpace(searchStr)) return;

            webMain.Address = searchStr;
        }

        private void SendBookInfo()
        {
            var bookInfo = pnlBookInfo.DataContext as AmazonBookInfo;
            OnBookSelected(bookInfo);
        }

        private void ShowResult(object result)
        {
            txtResult.Text = result?.ToString();
        }
        #endregion
    }

    public delegate void BookInfoEventHandler(object sender, BookInfoEventArgs e);

    public class BookInfoEventArgs
        : RoutedEventArgs

    {
        #region  Constructors & Destructor
        public BookInfoEventArgs() { }

        public BookInfoEventArgs(AmazonBookInfo info): this(null, null, info) { }

        public BookInfoEventArgs(RoutedEvent routedEvent, AmazonBookInfo info): this(routedEvent, null, info) { }

        public BookInfoEventArgs(RoutedEvent routedEvent, object source, AmazonBookInfo info): base(routedEvent, source)
        {
            Info = info;
        }
        #endregion


        #region  Properties & Indexers
        public AmazonBookInfo Info { get; set; }
        #endregion
    }
}