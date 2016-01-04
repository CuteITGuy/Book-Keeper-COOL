using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;


namespace CB.Html
{
    public class HtmlQuery
    {
        #region Fields
        private IEnumerable<HtmlNode> _allElementNodes;
        private HtmlDocument _document;
        #endregion


        #region Methods
        public static HtmlDocument GetDocumentFromHtmlString(string htmlStr)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlStr);
            return doc;
        }

        public static async Task<HtmlDocument> GetDocumentFromHtmlStringAsync(string htmlStr)
        {
            var doc = new HtmlDocument();
            await Task.Run(() => doc.LoadHtml(htmlStr));
            return doc;
        }

        public static HtmlDocument GetDocumentFromUrlString(string url)
        {
            using (var webClient = new WebClient())
            {
                var htmlStr = webClient.DownloadString(url);
                return GetDocumentFromHtmlString(htmlStr);
            }
        }

        public static async Task<HtmlDocument> GetDocumentFromUrlStringAsync(string url)
        {
            using (var webClient = new WebClient())
            {
                var htmlStr = await webClient.DownloadStringTaskAsync(url);
                return await GetDocumentFromHtmlStringAsync(htmlStr);
            }
        }

        public static IEnumerable<HtmlNode> QueryElements(HtmlDocument doc, Func<HtmlNode, bool> elementSelector)
        {
            var selectedNodes = new List<HtmlNode>();
            QueryElements(elementSelector, doc.DocumentNode, selectedNodes);
            return selectedNodes;
        }

        public static async Task<IEnumerable<HtmlNode>> QueryElementsAsync(HtmlDocument doc,
            Func<HtmlNode, bool> elementSelector) => await Task.Run(() => QueryElements(doc, elementSelector));

        public HtmlNode GetElementById(string id)
        {
            ThrowIfNotLoaded();
            return _document.GetElementbyId(id);
        }

        public HtmlNode GetElementById2(string id)
        {
            ThrowIfNotLoaded();
            return _allElementNodes.FirstOrDefault(n => n.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<HtmlNode> GetElementsByAttribute(string attribute, string value)
        {
            ThrowIfNotLoaded();
            return _allElementNodes.Where(n =>
            {
                var attrValue = n.GetAttributeValue(attribute, null);
                return attrValue != null && attrValue.Equals(value, StringComparison.InvariantCultureIgnoreCase);
            });
        }

        public IEnumerable<HtmlNode> GetElementsByAttribute(string attribute)
        {
            ThrowIfNotLoaded();
            return _allElementNodes.Where(
                n => n.Attributes.Select(a => a.Name).Contains(attribute, StringComparer.InvariantCultureIgnoreCase));
        }

        public IEnumerable<HtmlNode> GetElementsByAttributePattern(string attribute, string pattern)
        {
            ThrowIfNotLoaded();
            return _allElementNodes.Where(n =>
            {
                var attrValue = n.GetAttributeValue(attribute, null);
                return attrValue != null && Regex.IsMatch(attrValue, pattern);
            });
        }

        public IEnumerable<HtmlNode> GetElementsByClass(string classValue)
            => GetElementsByClassPattern($@"\b{classValue}\b");

        public IEnumerable<HtmlNode> GetElementsByClasses(IEnumerable<string> classValues)
            => GetElementsByClassPattern(CreatePatternForClasses(classValues));

        public IEnumerable<HtmlNode> GetElementsByClassPattern(string pattern)
            => GetElementsByAttributePattern("class", pattern);

        public IEnumerable<HtmlNode> GetElementsByData(string data, string value)
            => GetElementsByAttribute(GetDataAttribute(data), value);

        public IEnumerable<HtmlNode> GetElementsByData(string data) => GetElementsByAttribute(GetDataAttribute(data));

        public IEnumerable<HtmlNode> GetElementsByDataPattern(string data, string pattern)
            => GetElementsByAttributePattern(GetDataAttribute(data), pattern);

        public IEnumerable<HtmlNode> GetElementsByTag(string tag)
        {
            ThrowIfNotLoaded();
            return _allElementNodes.Where(n => n.Name.Equals(tag, StringComparison.InvariantCultureIgnoreCase));
        }

        public void LoadHtmlString(string htmlStr)
        {
            _document = GetDocumentFromHtmlString(htmlStr);
            RetrieveAllNodes();
        }

        public async Task LoadHtmlStringAsync(string htmlStr)
        {
            _document = await GetDocumentFromHtmlStringAsync(htmlStr);
            await RetrieveAllNodesAsync();
        }

        public void LoadUrlString(string urlStr)
        {
            _document = GetDocumentFromUrlString(urlStr);
            RetrieveAllNodes();
        }

        public async Task LoadUrlStringAsync(string urlStr)
        {
            _document = await GetDocumentFromUrlStringAsync(urlStr);
            await RetrieveAllNodesAsync();
        }
        #endregion


        #region Implementation
        private static string CreatePatternForClasses(IEnumerable<string> classValues)
        {
            //  ^(?=.*class1)(?=.*class2)(?=.*class3)
            return "^" + string.Join("", classValues.Select(s => $"(?=.*{s})"));
        }

        private static string GetDataAttribute(string data)
        {
            return $"data-{data}";
        }

        private IEnumerable<HtmlNode> QueryElements(Func<HtmlNode, bool> elementSelector)
        {
            return QueryElements(_document, elementSelector);
        }

        private static void QueryElements(Func<HtmlNode, bool> elementSelector, HtmlNode node,
            ICollection<HtmlNode> selectedNodes)
        {
            foreach (var childNode in node.ChildNodes)
            {
                if (elementSelector(childNode))
                {
                    selectedNodes.Add(childNode);
                }
                QueryElements(elementSelector, childNode, selectedNodes);
            }
        }

        private async Task<IEnumerable<HtmlNode>> QueryElementsAsync(Func<HtmlNode, bool> elementSelector)
        {
            return await QueryElementsAsync(_document, elementSelector);
        }

        private void RetrieveAllNodes()
        {
            _allElementNodes = QueryElements(e => true);
        }

        private async Task RetrieveAllNodesAsync()
        {
            _allElementNodes = await QueryElementsAsync(e => true);
        }

        private void ThrowIfNotLoaded()
        {
            if (_document == null || _allElementNodes == null)
                throw new InvalidOperationException("Document has not loaded.");
        }
        #endregion
    }
}