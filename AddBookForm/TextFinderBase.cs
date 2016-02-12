using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace AddBookForm
{
    public abstract class TextFinderBase: IFindText, IDisposable
    {
        #region Fields
        protected readonly string _filePath;
        protected readonly string _password;
        #endregion


        #region  Constructors & Destructor
        protected TextFinderBase(string filePath, string password)
        {
            _filePath = filePath;
            _password = password;
        }

        protected TextFinderBase(string filePath)
        {
            _filePath = filePath;
        }
        #endregion


        #region Abstract
        public abstract void Dispose();
        public abstract Match Find(string pattern);
        #endregion


        #region Methods
        public virtual async Task<Match> FindAsync(string pattern)
            => await Task.Run(() => Find(pattern));
        #endregion
    }
}