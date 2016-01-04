using System.IO;


namespace BookDatabase
{
    public partial class BookItem
    {
        #region Override
        public override string ToString()
        {
            return BookEdition == null || EbookType == null ? "" : $"{BookEdition}.{EbookType}";
        }
        #endregion


        public string SuggestFilePath()
        {
            var folderPath = BookEdition?.BookTitle?.BookTopic?.FolderPath;
            var fileName = BookEdition?.ToString();
            var extension = EbookType?.ToString();
            return folderPath == null || fileName == null || extension == null
                       ? "" : Path.Combine(folderPath, $"{fileName}.{extension}");
        }
    }
}