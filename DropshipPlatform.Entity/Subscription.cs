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
    
    public partial class subscription
    {
        public int SubscriptionID { get; set; }
        public Nullable<int> MembershipID { get; set; }
        public Nullable<int> UserID { get; set; }
        public string StripeSubscriptionID { get; set; }
        public Nullable<System.DateTime> NextBillDate { get; set; }
        public Nullable<System.DateTime> MembershipStartDate { get; set; }
        public Nullable<System.DateTime> MembershipExpiredDate { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<int> MembershipCreatedBy { get; set; }
        public Nullable<System.DateTime> MembershipCreatedOn { get; set; }
        public Nullable<int> MembershipModifyBy { get; set; }
        public Nullable<System.DateTime> MembershipModifyOn { get; set; }
        public string Status { get; set; }
    }
}
