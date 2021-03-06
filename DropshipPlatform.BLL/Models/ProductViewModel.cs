﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DropshipPlatform.BLL.Models
{
    public class ProductViewModel
    {
        public int ProductID { get; set; }
        public string Title { get; set; }
        public Nullable<int> CategoryID { get; set; }
        public string CategoryName { get; set; }

        public Nullable<double> SellingPrice { get; set; }
        public string SellingPriceCurrency { get; set; }
        public string OriginalProductID { get; set; }
        public string Brand { get; set; }
        public string Description { get; set; }
        public string Inventory { get; set; }
        public string ManufacturerName { get; set; }
        public string ExternalCode { get; set; }
        public string ExternalCodeType { get; set; }
        public Nullable<int> ParentProductID { get; set; }
        public Nullable<int> NoOfCustomerReviews { get; set; }
        public Nullable<int> CustomerReviewRating { get; set; }
        public string NetWeight { get; set; }
        public string ShippingWeight { get; set; }
        public string CountryOfOrigin { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string SourceWebsite { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public long UpdatedPrice { get; set; }
        public long UpdatedInvetory { get; set; }
    }

    public class ProductGroupModel
    {
        public ProductViewModel ParentProduct { get; set; }
        public List<ProductViewModel> ChildProductList { get; set; }
    }

    public class UpdateProductModel
    {
        public string SKU { get; set; }
        public string ParentSKU { get; set; }
        public int PickedInventory { get; set; }
        public int UpdatedPrice { get; set; }
    }

    public class TitleMultiLanguageList
    {
        public string locale { get; set; }
        public string title { get; set; }
    }

    public class Html
    {
        public string content { get; set; }
    }

    public class ModuleList
    {
        public string type { get; set; }
        public Html html { get; set; }
    }

    public class DescriptionMultiLanguageList
    {
        public string locale { get; set; }
        public List<ModuleList> module_list { get; set; }
    }
    public class CategoryAttributes
    {
        public BrandName BrandName { get; set; }
        public ManufacturerName ManufacturerName { get; set; }
        public Size Size { get; set; }
        public Color Color { get; set; }
    }
    public class BrandName
    {
        public string value { get; set; }
    }

    public class ManufacturerName
    {
        public string value { get; set; }
    }

    public class Size
    {
        public string value { get; set; }
    }

    public class Color
    {
        public string value { get; set; }
    }

    public class SkuAttributes
    {
        public Size Size { get; set; }
    }

    public class SkuInfoList
    {
        public string sku_code { get; set; }
        public int inventory { get; set; }
        public int price { get; set; }
        public int discount_price { get; set; }
        public SkuAttributes sku_attributes { get; set; }
    }

    public class AliExpressPostProductModel
    {
        public int category_id { get; set; }
        public List<TitleMultiLanguageList> title_multi_language_list { get; set; }
        public List<DescriptionMultiLanguageList> description_multi_language_list { get; set; }
        public string locale { get; set; }
        public string product_units_type { get; set; }
        public List<string> image_url_list { get; set; }
        public CategoryAttributes category_attributes { get; set; }
        public List<SkuInfoList> sku_info_list { get; set; }
        public string inventory_deduction_strategy { get; set; }
        public double package_weight { get; set; }
        public int package_length { get; set; }
        public int package_height { get; set; }
        public int package_width { get; set; }
        public int shipping_preparation_time { get; set; }
        public string shipping_template_id { get; set; }
        public string service_template_id { get; set; }
    }

}