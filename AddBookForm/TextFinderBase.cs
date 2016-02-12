using System;
using System.Collections.Generic;
using System.Linq;
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

        protected abstract IEnumerable<string> GetContents(string pattern);
        protected abstract void LoadFile();
        #endregion


        #region Methods
        public virtual Match Find(string pattern)
        {
            LoadFile();
            var contents = GetContents(pattern);
            return contents?.Select(content => Regex.Match(content, pattern))
                            .FirstOrDefault(match => match.Success);
        }

        public virtual async Task<Match> FindAsync(string pattern)
            => await Task.Run(() => Find(pattern));
        #endregion
    }
}