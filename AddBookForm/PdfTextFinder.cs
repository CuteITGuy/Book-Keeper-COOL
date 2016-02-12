using System.Collections.Generic;
using System.Linq;
using BitMiracle.Docotic.Pdf;


namespace AddBookForm
{
    public class PdfTextFinder: TextFinderBase
    {
        #region Fields
        private PdfDocument _pdf;
        #endregion


        #region  Constructors & Destructor
        public PdfTextFinder(string filePath, string password): base(filePath, password) { }
        public PdfTextFinder(string filePath): base(filePath) { }
        #endregion


        #region Override
        public override void Dispose()
        {
            _pdf?.Dispose();
            _pdf = null;
        }

        protected override IEnumerable<string> GetContents(string pattern)
        {
            return _pdf.Pages.Select(page => page.GetText());
        }

        protected override void LoadFile()
        {
            if (_pdf != null) return;
            _pdf = _password != null ? new PdfDocument(_filePath, _password) : new PdfDocument(_filePath);
        }
        #endregion
    }
}