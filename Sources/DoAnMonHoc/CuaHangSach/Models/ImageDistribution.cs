//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CuaHangSach.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ImageDistribution
    {
        public long ID { get; set; }
        public Nullable<long> authorId { get; set; }
        public Nullable<long> bookId { get; set; }
        public long imageId { get; set; }
    
        public virtual Author Author { get; set; }
        public virtual Book Book { get; set; }
        public virtual Image Image { get; set; }
    }
}
