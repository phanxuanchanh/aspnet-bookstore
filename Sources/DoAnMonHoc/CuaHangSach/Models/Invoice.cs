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
    
    public partial class Invoice
    {
        public long ID { get; set; }
        public string name { get; set; }
        public string customerName { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
        public string location { get; set; }
        public System.DateTime orderTime { get; set; }
        public double totalMoney { get; set; }
    
        public virtual InvoiceDetail InvoiceDetail { get; set; }
    }
}