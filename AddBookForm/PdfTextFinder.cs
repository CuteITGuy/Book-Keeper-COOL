using System.Linq;
using System.Text.RegularExpressions;
using BitMiracle.Docotic.Pdf;


namespace AddBookForm
{
    public class PdfTextFinder: TextFinderBase
    {
        #region Fields
        private PdfDocument _document;
        #endregion


        #region  Constructors & Destructor
        public PdfTextFinder(string filePath, string password): base(filePath, password) { }
        public PdfTextFinder(string filePath): base(filePath) { }
        #endregion


        #region Override
        public override void Dispose()
        {
            _document?.Dispose();
            _document = null;
        }

        public override Match Find(string pattern)
        {
            LoadFile();
            return _document.Pages
                            .Select(page => page.GetText())
                            .Select(text => Regex.Match(text, pattern))
                            .FirstOrDefault(match => match.Success);
        }
        #endregion


        #region Implementation
        private void LoadFile()
        {
            if (_document != null) return;
            _document = _password != null ? new PdfDocument(_filePath, _password) : new PdfDocument(_filePath);
        }
        #endregion
    }
}