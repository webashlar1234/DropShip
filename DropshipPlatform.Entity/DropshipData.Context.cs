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
    
        public virtual DbSet<AliExpressCategory> AliExpressCategories { get; set; }
        public virtual DbSet<AliExpressOrderItem> AliExpressOrderItems { get; set; }
        public virtual DbSet<AliExpressOrder> AliExpressOrders { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<PaymentProfile> PaymentProfiles { get; set; }
        public virtual DbSet<ProductMedia> ProductMedias { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Subscription> Subscriptions { get; set; }
        public virtual DbSet<User_Roles> User_Roles { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<MembershipType> MembershipTypes { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<SellersPickedProduct> SellersPickedProducts { get; set; }
        public virtual DbSet<ApplicationLog> ApplicationLogs { get; set; }
        public virtual DbSet<SellerPickedProductSKU> SellerPickedProductSKUs { get; set; }
        public virtual DbSet<AliExpressJobLog> AliExpressJobLogs { get; set; }
    }
}
