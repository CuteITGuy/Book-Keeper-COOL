using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using HtmlHandling;


namespace AmazonSearcher
{
    public partial class MainWindow
    {
        #region Fields
        private const string FIRST_SEARCH_RESULT_SELECTOR = "#result_0 a.s-access-detail-page";

        private const string SEARCH_URL =
            "http://www.amazon.com/s/ref=nb_sb_noss?url=search-alias%3Dstripbooks&field-keywords=";

        public static readonly RoutedEvent BookInfoSentEvent = EventManager.RegisterRoutedEvent(
            nameof(BookInfoSent), RoutingStrategy.Bubble, typeof(BookInfoEventHandler), typeof(MainWindow));

        private readonly AmazonBookPageParser _amazonParser = new AmazonBookPageParser();

        private readonly HtmlHandler _htmlHandler = new HtmlHandler();
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
        public async Task SearchBook(string search, bool autoGotoFirstResult = true)
        {
            var targetAddress = CreateSearchAddress(search);
            if (autoGotoFirstResult)
            {
                var firstResultAddress = await GetFirstSearchResultAddress(targetAddress);
                if (!string.IsNullOrEmpty(firstResultAddress))
                {
                    targetAddress = firstResultAddress;
                }
            }
            NavigateTo(targetAddress);
        }
        
        #endregion


        #region Event Handlers
        private void CmdCopyAll_OnClick(object sender, RoutedEventArgs e)
        {
            var bookInfo = GetBookInfoContext();
            if (bookInfo == null) return;
            Clipboard.SetText(bookInfo.GeneralInfo);
        }

        private void CmdCopyTitle_OnClick(object sender, RoutedEventArgs e)
        {
            var bookInfo = GetBookInfoContext();
            if (bookInfo == null) return;
            Clipboard.SetText(bookInfo.TitleInfo);
        }

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
            await FetchBookInfoAsync();
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

        private async void WebMain_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading) return;
            await FetchBookInfoAsync();
        }
        #endregion


        #region Implementation
        private static string CreateIsbnSearchString(string isbn) => isbn;

        private static string CreateSearchAddress(string search)
        {
            var searchString = CreateSearchString(search);
            return string.IsNullOrEmpty(searchString) ? "" : SEARCH_URL + searchString;
        }

        private static string CreateSearchString(string search)
            => string.IsNullOrEmpty(search)
                   ? ""
                   : IsIsbn(search)
                         ? CreateIsbnSearchString(search)
                         : CreateTitleSearchString(search);

        private static string CreateTitleSearchString(string title) => Regex.Replace(title, @"\s+", "+");

        private async Task FetchBookInfoAsync()
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                var bookInfo = await _amazonParser.FetchBookInfo(webMain);
                pnlBookInfo.DataContext = bookInfo;
            });
        }

        private AmazonBookInfo GetBookInfoContext() => pnlBookInfo.DataContext as AmazonBookInfo;

        private async Task<string> GetFirstSearchResultAddress(string searchAddress)
        {
            if (string.IsNullOrEmpty(searchAddress)) return null;

            await _htmlHandler.ParseAddress(searchAddress);
            var firstResult = _htmlHandler.SelectAnchor(FIRST_SEARCH_RESULT_SELECTOR);
            return firstResult?.Href;
        }

        private static bool IsIsbn(string text)
            => Regex.IsMatch(text, @"^(\d-?){10}$") || Regex.IsMatch(text, @"^(\d-?){13}$");

        private void NavigateTo(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) return;
            webMain.Address = address;
        }

        protected virtual void OnBookSelected(AmazonBookInfo info)
            => RaiseEvent(new BookInfoEventArgs(BookInfoSentEvent, this, info));

        private async Task<object> RunScript(string script)
            => (await webMain.EvaluateScriptAsync(script, null)).Result;

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
}