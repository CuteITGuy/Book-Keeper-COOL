using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;


namespace AddBookForm
{
    /*public class EpubTextFinder: TextFinderBase
    {
        #region Fields
        private Epub _epub;
        private EpubBook _epubBook;
        #endregion


        #region  Constructors & Destructor
        public EpubTextFinder(string filePath, string password): base(filePath, password) { }
        public EpubTextFinder(string filePath): base(filePath) { }
        #endregion


        #region Override
        public override void Dispose() { }

        protected override IEnumerable<string> GetContents(string pattern)
        {
            return _epubBook?.Content.Html.Values.Select(content => content.Content)
                   ?? _epub?.Content.Cast<ContentData>().Select(content => content.GetContentAsPlainText());
        }

        protected override void LoadFile()
        {
            if (_epubBook != null || _epub != null) return;
            try
            {
                _epubBook = EpubReader.OpenBook(_filePath);
            }
            catch
            {
                try
                {
                    _epub = new Epub(_filePath);
                }
                catch
                {
                    // ignored
                }
            }
        }
        #endregion
    }*/

    public class EpubTextFinder: TextFinderBase
    {
        #region Fields
        private const string TEMP_FOLDER = "temp";
        private readonly string[] _contentFilePatterns = { "*.html", "*.htm", "*.xhtml" };
        #endregion


        #region  Constructors & Destructor
        public EpubTextFinder(string filePath, string password): base(filePath, password) { }
        public EpubTextFinder(string filePath): base(filePath) { }
        #endregion


        #region Override
        public override void Dispose()
        {
            try { ClearTempFolder(); }
            catch
            {
                // ignored
            }
        }

        protected override IEnumerable<string> GetContents(string pattern)
        {
            return Directory.Exists(TEMP_FOLDER)
                       ? _contentFilePatterns
                             .SelectMany(p => Directory.EnumerateFiles(TEMP_FOLDER, p, SearchOption.AllDirectories))
                             .Select(File.ReadAllText)
                       : null;
        }

        protected override void LoadFile()
        {
            try
            {
                PrepareTempFolder();
                ZipFile.ExtractToDirectory(_filePath, TEMP_FOLDER);
            }
            catch
            {
                // ignored
            }
        }
        #endregion


        #region Implementation
        private static void ClearTempFolder()
        {
            foreach (var file in Directory.EnumerateFiles(TEMP_FOLDER))
            {
                File.Delete(file);
            }
            foreach (var directory in Directory.EnumerateDirectories(TEMP_FOLDER))
            {
                Directory.Delete(directory, true);
            }
        }

        private static void PrepareTempFolder()
        {
            if (!Directory.Exists(TEMP_FOLDER)) Directory.CreateDirectory(TEMP_FOLDER);
            ClearTempFolder();
        }
        #endregion
    }
}