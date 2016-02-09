namespace BookDatabase
{
    public partial class BookEdition
    {
        #region Methods
        private string GetEditionString()
        {
            return Edition == null ? "" : $" {NumberHandler.ToOrdinal(Edition.Value)} edition";
        }
        #endregion


        #region Override
        public override string ToString()
        {
            return $"{BookTitle}{GetEditionString()}" + (Year == null ? "" : $" {Year.Value}");
        }
        #endregion
    }
}