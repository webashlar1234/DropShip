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
    
    public partial class activitylog
    {
        public int ActivityLogID { get; set; }
        public string Type { get; set; }
        public string Event { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
    }
}
