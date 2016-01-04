namespace BookDatabase
{
    public partial class BookEdition
    {
        #region Methods
        private static string ToOrdinal(int num)
        {
            if (num <= 0) return num.ToString();

            if (num % 100 == 11 || num % 100 == 12 || num % 100 == 13)
            {
                return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }
        }

        private string GetEditionString()
        {
            return Edition == null ? "" : $" {ToOrdinal(Edition.Value)} edition";
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