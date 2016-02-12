using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace AddBookForm
{
    public class TextFinder: TextFinderBase
    {
        #region Fields
        private TextFinderBase _textFinder;
        #endregion


        #region  Constructors & Destructor
        public TextFinder(string filePath, string password): base(filePath, password)
        {
            InitializeTextFinder();
        }

        public TextFinder(string filePath): base(filePath)
        {
            InitializeTextFinder();
        }
        #endregion


        #region Override
        public override void Dispose()
        {
            _textFinder?.Dispose();
        }

        public override Match Find(string pattern) => _textFinder.Find(pattern);

        public override async Task<Match> FindAsync(string pattern) => await _textFinder.FindAsync(pattern);
        #endregion


        #region Implementation
        private void InitializeTextFinder()
        {
            var extension = Path.GetExtension(_filePath)?.ToLower();
            switch (extension)
            {
                case ".pdf":
                    _textFinder = new PdfTextFinder(_filePath);
                    break;
                default:
                    throw new Exception();
            }
        }
        #endregion
    }
}