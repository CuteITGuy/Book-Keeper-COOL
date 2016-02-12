using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace AddBookForm
{
    public interface IFindText
    {
        #region Abstract
        Match Find(string pattern);
        Task<Match> FindAsync(string pattern);
        #endregion
    }
}