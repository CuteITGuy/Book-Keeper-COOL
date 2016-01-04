using System.IO;
using System.Text;


namespace BookDatabase
{
    public partial class BookTopic
    {
        #region  Properties & Indexers
        public static string RootFolderPath { get; set; } = @"C:\";
        #endregion


        #region Methods
        public string SuggestFolderPath()
        {
            if (string.IsNullOrWhiteSpace(Topic)) return "";

            var pathBuilder = new StringBuilder(Topic);
            var superTopic = SuperTopic;
            while (superTopic != null)
            {
                pathBuilder.Insert(0, SuperTopic + "\\");
                superTopic = superTopic.SuperTopic;
            }
            return Path.Combine(RootFolderPath, pathBuilder.ToString());
        }
        #endregion


        #region Override
        public override string ToString()
        {
            return Topic;
        }
        #endregion
    }
}