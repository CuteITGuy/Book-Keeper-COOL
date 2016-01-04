using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;


namespace CB.Html
{
    public static class HtmlQueryHelper
    {
        #region Methods
        public static HtmlNode FindDescendantElementById(this HtmlNode parent, string id)
            => FindFirstDescendantElement(parent, CreateIdSelector(id));

        public static IEnumerable<HtmlNode> FindDescendantElements(this HtmlNode parent,
            Func<HtmlNode, bool> elementSelector)
        {
            var selectedElements = new List<HtmlNode>();
            FindDescendantElements(parent, elementSelector, selectedElements);
            return selectedElements;
        }

        public static HtmlNode FindElementById(this IEnumerable<HtmlNode> elements, string id)
        {
            return elements.FirstOrDefault(CreateIdSelector(id));
        }

        public static IEnumerable<HtmlNode> FindElementsByAttribute(this IEnumerable<HtmlNode> elements,
            string attribute, string value) => elements.Where(CreateAttributeSelector(attribute, value));

        public static IEnumerable<HtmlNode> FindElementsByAttribute(this IEnumerable<HtmlNode> elements,
            string attribute) => elements.Where(CreateAttributeSelector(attribute));

        public static IEnumerable<HtmlNode> FindElementsByAttributePattern(this IEnumerable<HtmlNode> elements,
            string attribute, string pattern) => elements.Where(CreateAttributePatternSelector(attribute, pattern));

        public static IEnumerable<HtmlNode> FindElementsByClass(this IEnumerable<HtmlNode> elements, string classValue)
            => FindElementsByClassPattern(elements, $@"\b{classValue}\b");

        public static IEnumerable<HtmlNode> FindElementsByClasses(this IEnumerable<HtmlNode> elements,
            IEnumerable<string> classValues)
            => FindElementsByClassPattern(elements, CreatePatternForClasses(classValues));

        public static IEnumerable<HtmlNode> FindElementsByClassPattern(this IEnumerable<HtmlNode> elements,
            string pattern)
            => FindElementsByAttributePattern(elements, "class", pattern);

        public static IEnumerable<HtmlNode> FindElementsByData(this IEnumerable<HtmlNode> elements, string data,
            string value)
            => FindElementsByAttribute(elements, CreateDataAttribute(data), value);

        public static IEnumerable<HtmlNode> FindElementsByData(this IEnumerable<HtmlNode> elements, string data)
            => FindElementsByAttribute(elements, CreateDataAttribute(data));

        public static IEnumerable<HtmlNode> FindElementsByDataPattern(this IEnumerable<HtmlNode> elements, string data,
            string pattern)
            => FindElementsByAttributePattern(elements, CreateDataAttribute(data), pattern);

        public static IEnumerable<HtmlNode> FindElementsByTag(this IEnumerable<HtmlNode> elements, string tag)
            => elements.Where(CreateTagSelector(tag));

        public static HtmlNode FindFirstDescendantElement(this HtmlNode parent, Func<HtmlNode, bool> elementSelector)
        {
            foreach (var node in parent.ChildNodes)
            {
                if (elementSelector(node)) return node;
                var result = FindFirstDescendantElement(node, elementSelector);
                if (result != null) return result;
            }
            return null;
        }
        #endregion


        #region Implementation
        private static Func<HtmlNode, bool> CreateAttributePatternSelector(string attribute, string pattern)
        {
            return n =>
            {
                var attrValue = n.GetAttributeValue(attribute, null);
                return attrValue != null && Regex.IsMatch(attrValue, pattern);
            };
        }

        private static Func<HtmlNode, bool> CreateAttributeSelector(string attribute)
        {
            return n => n.Attributes.Select(a => a.Name).Contains(attribute, StringComparer.InvariantCultureIgnoreCase);
        }

        private static Func<HtmlNode, bool> CreateAttributeSelector(string attribute, string value)
        {
            return n =>
            {
                var attrValue = n.GetAttributeValue(attribute, null);
                return attrValue != null && attrValue.Equals(value, StringComparison.InvariantCultureIgnoreCase);
            };
        }

        private static string CreateDataAttribute(string data)
        {
            return $"data-{data}";
        }

        private static Func<HtmlNode, bool> CreateIdSelector(string id)
        {
            return e => e.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase);
        }

        private static string CreatePatternForClasses(IEnumerable<string> classValues)
        {
            //  ^(?=.*class1)(?=.*class2)(?=.*class3)
            return "^" + string.Join("", classValues.Select(s => $"(?=.*{s})"));
        }

        private static Func<HtmlNode, bool> CreateTagSelector(string tag)
        {
            return n => n.Name.Equals(tag, StringComparison.InvariantCultureIgnoreCase);
        }

        private static void FindDescendantElements(HtmlNode parentNode, Func<HtmlNode, bool> elementSelector,
            ICollection<HtmlNode> selectedNodes)
        {
            foreach (var node in parentNode.ChildNodes)
            {
                if (elementSelector(node)) selectedNodes.Add(node);
                FindDescendantElements(node, elementSelector, selectedNodes);
            }
        }
        #endregion
    }
}