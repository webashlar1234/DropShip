﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DropshipDataEntities : DbContext
    {
        public DropshipDataEntities()
            : base("name=DropshipDataEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<activitylog> activitylogs { get; set; }
        public virtual DbSet<aliexpresscategory> aliexpresscategories { get; set; }
        public virtual DbSet<aliexpressorderitem> aliexpressorderitems { get; set; }
        public virtual DbSet<aliexpressorder> aliexpressorders { get; set; }
        public virtual DbSet<applicationlog> applicationlogs { get; set; }
        public virtual DbSet<category> categories { get; set; }
        public virtual DbSet<country> countries { get; set; }
        public virtual DbSet<frequencyupdate> frequencyupdates { get; set; }
        public virtual DbSet<layout> layouts { get; set; }
        public virtual DbSet<membershiptype> membershiptypes { get; set; }
        public virtual DbSet<order> orders { get; set; }
        public virtual DbSet<orders1> orders1 { get; set; }
        public virtual DbSet<paymentprofile> paymentprofiles { get; set; }
        public virtual DbSet<product> products { get; set; }
        public virtual DbSet<role> roles { get; set; }
        public virtual DbSet<user_roles> user_roles { get; set; }
        public virtual DbSet<website> websites { get; set; }
        public virtual DbSet<sellerspickedproduct> sellerspickedproducts { get; set; }
        public virtual DbSet<productmedia> productmedias { get; set; }
        public virtual DbSet<sellerpickedproductsku> sellerpickedproductskus { get; set; }
        public virtual DbSet<subscription> subscriptions { get; set; }
        public virtual DbSet<aliexpressjoblog> aliexpressjoblogs { get; set; }
        public virtual DbSet<user> users { get; set; }
    }
}
