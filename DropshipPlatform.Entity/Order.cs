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
    
    public partial class order
    {
        public int OrderID { get; set; }
        public string AliExpressOrderID { get; set; }
        public string DeliveryCountry { get; set; }
        public string ShippingWeight { get; set; }
        public string OrderAmount { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string AliExpressLoginID { get; set; }
        public string TrackingNo { get; set; }
        public Nullable<int> ItemCreatedBy { get; set; }
        public Nullable<System.DateTime> ItemCreatedWhen { get; set; }
        public Nullable<int> ItemModifyBy { get; set; }
        public Nullable<System.DateTime> ItemModifyWhen { get; set; }
        public Nullable<bool> SellerPaymentStatus { get; set; }
        public string SellerPaymentDetails { get; set; }
        public byte[] CainiaoLable { get; set; }
        public string OrderApiError { get; set; }
        public string OrderApiResult { get; set; }
        public string orderscol { get; set; }
    }
}
