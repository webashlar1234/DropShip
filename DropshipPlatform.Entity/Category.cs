//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DropshipPlatform.DLL
{
    using System;
    using System.Collections.Generic;
    
    public partial class Category
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }
        public Nullable<int> CategoryLevel { get; set; }
        public string ParentCategoryID { get; set; }
        public Nullable<bool> IsLeafCategory { get; set; }
        public string AliExpressCategoryName { get; set; }
        public Nullable<long> AliExpressCategoryId { get; set; }
        public Nullable<int> ItemCreatedBy { get; set; }
        public Nullable<System.DateTime> ItemCreatedWhen { get; set; }
        public Nullable<int> ItemModifyBy { get; set; }
        public Nullable<System.DateTime> ItemModifyWhen { get; set; }
    }
}
