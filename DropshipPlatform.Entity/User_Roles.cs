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
    
    public partial class User_Roles
    {
        public int UserRoleID { get; set; }
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public Nullable<int> ItemCreatedBy { get; set; }
        public Nullable<System.DateTime> ItemCreatedWhen { get; set; }
        public Nullable<int> ItemModifyBy { get; set; }
        public Nullable<System.DateTime> ItemModifyWhen { get; set; }
    }
}
