//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BookDatabase
{
    using System;
    using System.Collections.Generic;
    
    public partial class BookTopic
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BookTopic()
        {
            this.BookTitles = new HashSet<BookTitle>();
            this.SubTopics = new HashSet<BookTopic>();
        }
    
        public short Id { get; set; }
        public string Topic { get; set; }
        public Nullable<short> SuperTopicId { get; set; }
        public string FolderPath { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BookTitle> BookTitles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BookTopic> SubTopics { get; set; }
        public virtual BookTopic SuperTopic { get; set; }
    }
}
