using System.Windows;


namespace AmazonSearcher
{
    public class BookInfoEventArgs
        : RoutedEventArgs

    {
        #region  Constructors & Destructor
        public BookInfoEventArgs() { }

        public BookInfoEventArgs(AmazonBookInfo info): this(null, null, info) { }

        public BookInfoEventArgs(RoutedEvent routedEvent, AmazonBookInfo info): this(routedEvent, null, info) { }

        public BookInfoEventArgs(RoutedEvent routedEvent, object source, AmazonBookInfo info): base(routedEvent, source)
        {
            Info = info;
        }
        #endregion


        #region  Properties & Indexers
        public AmazonBookInfo Info { get; set; }
        #endregion
    }
}