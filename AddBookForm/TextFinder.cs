using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace AddBookForm
{
    public class TextFinder: IFindText, IDisposable
    {
        #region Fields
        private readonly string _filePath;
        private readonly string _password;
        private TextFinderBase _textFinder;
        #endregion


        #region  Constructors & Destructor
        public TextFinder(string filePath, string password)
        {
            _filePath = filePath;
            _password = password;
            InitializeTextFinder();
        }

        public TextFinder(string filePath): this(filePath, null) { }
        #endregion


        #region Methods
        public void Dispose()
        {
            _textFinder?.Dispose();
        }

        public Match Find(string pattern) => _textFinder.Find(pattern);

        public async Task<Match> FindAsync(string pattern) => await _textFinder.FindAsync(pattern);
        #endregion


        #region Implementation
        private void InitializeTextFinder()
        {
            var extension = Path.GetExtension(_filePath)?.ToLower();
            switch (extension)
            {
                case ".pdf":
                    _textFinder = new PdfTextFinder(_filePath, _password);
                    break;
                case ".epub":
                    _textFinder = new EpubTextFinder(_filePath, _password);
                    break;
                default:
                    throw new Exception();
            }
        }
        #endregion
    }
}

//TODO: MobiTextFinder, Azw3TextFinder