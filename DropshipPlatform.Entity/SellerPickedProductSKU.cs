//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DropshipPlatform.Entity
{
    using System;
    using System.Collections.Generic;
    
    public partial class SellerPickedProductSKU
    {
        public int Id { get; set; }
        public Nullable<int> ProductId { get; set; }
        public string SKUCode { get; set; }
        public Nullable<decimal> UpdatedPrice { get; set; }
        public int SellerPickedId { get; set; }
        public Nullable<int> UserId { get; set; }
    
        public virtual SellersPickedProduct SellersPickedProduct { get; set; }
    }
}
