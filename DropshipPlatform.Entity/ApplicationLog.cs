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
    
    public partial class ApplicationLog
    {
        public int ApplicationLogID { get; set; }
        public Nullable<int> UserID { get; set; }
        public string ApplicationName { get; set; }
        public string IP { get; set; }
        public string Agent { get; set; }
        public string ErrorMsg { get; set; }
        public string ErrorDescription { get; set; }
        public Nullable<int> ItemCreatedBy { get; set; }
        public Nullable<System.DateTime> ItemCreatedOn { get; set; }
        public Nullable<int> ProductID { get; set; }
    }
}
