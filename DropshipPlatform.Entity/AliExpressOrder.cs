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
    
    public partial class aliexpressorder
    {
        public int ID { get; set; }
        public Nullable<long> AliExpressOrderID { get; set; }
        public string BuyerLoginId { get; set; }
        public string AliExpressLoginID { get; set; }
        public string DeliveryCountry { get; set; }
        public string PaymentStatus { get; set; }
        public string TrackingNo { get; set; }
        public string ShippingWeight { get; set; }
        public string OrderAmount { get; set; }
        public string CurrencyCode { get; set; }
        public string OrderStatus { get; set; }
        public Nullable<int> NoOfOrderItems { get; set; }
        public Nullable<int> ItemCreatedBy { get; set; }
        public Nullable<System.DateTime> ItemCreatedWhen { get; set; }
        public Nullable<int> ItemModifyBy { get; set; }
        public Nullable<System.DateTime> ItemModifyWhen { get; set; }
        public Nullable<System.DateTime> AliExpressOrderCreatedTime { get; set; }
        public Nullable<System.DateTime> AliExpressOrderUpdatedTime { get; set; }
    }
}
