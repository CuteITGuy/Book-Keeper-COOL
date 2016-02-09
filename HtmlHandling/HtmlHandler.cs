using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;


namespace HtmlHandling
{
    public class HtmlHandler
    {
        #region Fields
        private IDocument _document;
        #endregion


        #region Methods
        public async Task ParseAddress(string address)
        {
            //_document = await BrowsingContext.New().OpenAsync(address);
            using (var webClient = new WebClient())
            {
                var html = await webClient.DownloadStringTaskAsync(address);
                var htmlParser = new HtmlParser();
                _document = await htmlParser.ParseAsync(html);
            }
        }

        public IElement Select(string selector) => _document.QuerySelector(selector);

        public IEnumerable<IElement> SelectAll(string selector, Func<IElement, bool> predicate = null)
            => _document.QuerySelectorAll(selector).Where(e => predicate == null || predicate(e));

        public IHtmlAnchorElement SelectAnchor(string selector) => Select(selector) as IHtmlAnchorElement;

        public IEnumerable<IHtmlAnchorElement> SelectAnchors(string selector, Func<IHtmlAnchorElement, bool> predicate = null)
            => _document.QuerySelectorAll(selector).OfType<IHtmlAnchorElement>().Where(
                e => predicate == null || predicate(e));
        #endregion
    }
}