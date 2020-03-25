using DropshipPlatform.BLL.Models;
using DropshipPlatform.Entity;
using FastJSON;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace DropshipPlatform.BLL.Services
{
    public class ProductService
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private AliExpressJobLogService _aliExpressJobLogService = new AliExpressJobLogService();
        public List<product> GetAllProducts()
        {
            List<product> products = new List<product>();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    products = datacontext.products.ToList();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return products;
        }

        public List<ProductGroupModel> GetParentProducts(int UserID)
        {
            List<ProductGroupModel> productGroupList = new List<ProductGroupModel>();
            List<ProductViewModel> products = new List<ProductViewModel>();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    products = (from p in datacontext.products
                                join c in datacontext.categories on p.CategoryID equals c.CategoryID
                                from sp in datacontext.sellerspickedproducts.Where(x => x.ParentProductID == p.ProductID && x.UserID == UserID).DefaultIfEmpty()
                                    //from spsku in datacontext.SellerPickedProductSKUs.Where(x => x.ProductId == p.ProductID && x.UserId == UserID).DefaultIfEmpty()
                                where p.ParentProductID == null
                                select new ProductViewModel
                                {
                                    ProductID = p.ProductID,
                                    Title = p.Title,
                                    OriginalProductID = p.OriginalProductID,
                                    CategoryID = p.CategoryID,
                                    CategoryName = c.Name,
                                    Cost = p.Cost,
                                    Inventory = p.Inventory ?? 0,
                                    ShippingWeight = p.ShippingWeight,
                                    SellerPickedCount = datacontext.sellerspickedproducts.Where(x => x.ParentProductID == p.ProductID && x.UserID != UserID).Count(),
                                    IsActive = p.IsActive,
                                    UserID = sp.UserID,
                                    AliExpressCategoryID = c.AliExpressCategoryID ?? 0
                                }).ToList();

                    if (products.Count > 0)
                    {
                        foreach (ProductViewModel productViewModel in products)
                        {
                            double num;
                            if (double.TryParse(productViewModel.Cost, out num))
                            {
                                ProductGroupModel productGroup = new ProductGroupModel();
                                sellerspickedproduct sellersPickedProduct = datacontext.sellerspickedproducts.Where(x => x.ParentProductID == productViewModel.ProductID && x.UserID == UserID).FirstOrDefault();
                                if (sellersPickedProduct != null)
                                {
                                    productViewModel.hasProductSkuSync = true;
                                    if (!string.IsNullOrEmpty(sellersPickedProduct.AliExpressProductID))
                                    {
                                        productViewModel.SellerPrice = sellersPickedProduct.SellerPrice;
                                        productViewModel.isProductPicked = true;
                                    }
                                }
                                List<product> childProducts = datacontext.products.Where(x => x.ParentProductID == productViewModel.ProductID).ToList();
                                productGroup.ParentProduct = productViewModel;
                                productGroup.ChildProductList = new List<ProductViewModel>();
                                //long parentInvetoryTotal = 0;
                                //long parentPriceTotal = 0;

                                bool hasChild = false;
                                if (childProducts.Count > 0)
                                {
                                    hasChild = true;

                                    foreach (product dbChildProduct in childProducts)
                                    {
                                        if (double.TryParse(dbChildProduct.Cost, out num) == false)
                                        {
                                            productGroup.ChildProductList = new List<ProductViewModel>();
                                            break;
                                        }
                                        ProductViewModel childProductModel = GenerateProductViewModel(dbChildProduct);
                                        childProductModel = AddUpdatedValues(childProductModel);
                                        productGroup.ChildProductList.Add(childProductModel);
                                        //parentInvetoryTotal = parentInvetoryTotal + Convert.ToInt32(childProductModel.Inventory);
                                        //parentPriceTotal = parentPriceTotal + Convert.ToInt32(childProductModel.Cost);
                                    }
                                    //productGroup.ParentProduct.Inventory = parentInvetoryTotal.ToString();
                                    //productGroup.ParentProduct.Cost = parentPriceTotal;
                                    // productGroupList.Add(productGroup);
                                }
                                if (hasChild == false)
                                {
                                    productGroupList.Add(productGroup);
                                }
                                else
                                {
                                    if (productGroup.ChildProductList.Count > 0)
                                    {
                                        productGroupList.Add(productGroup);
                                    }
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return productGroupList;
        }

        public bool AddSellersPickedProducts(List<scproductModel> products, int UserID)
        {
            bool result = true;
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    foreach (scproductModel product in products)
                    {
                        sellerspickedproduct obj = datacontext.sellerspickedproducts.Where(x => x.UserID == UserID && x.ParentProductID == product.productId).FirstOrDefault();
                        if (obj == null)
                        {
                            product ParentProduct = datacontext.products.Where(m => m.ProductID == product.productId).FirstOrDefault();
                            if (ParentProduct != null)
                            {
                                obj = new sellerspickedproduct();
                                obj.UserID = UserID;
                                obj.ParentProductID = ParentProduct.ProductID;
                                obj.SellerPrice = Convert.ToDouble(product.price);
                                obj.ItemCreatedBy = UserID;
                                obj.ItemCreatedWhen = DateTime.UtcNow;
                                obj.IsOnline = true;
                                if (product.SKUModels != null)
                                {
                                    foreach (ProductSKUModel productSKUModel in product.SKUModels)
                                    {
                                        obj.sellerpickedproductskus.Add(new sellerpickedproductsku()
                                        {
                                            ProductId = productSKUModel.childproductId,
                                            SKUCode = productSKUModel.skuCode,
                                            UpdatedPrice = Convert.ToDecimal(productSKUModel.price),
                                            UserId = UserID
                                        });
                                    }
                                }
                                datacontext.sellerspickedproducts.Add(obj);
                                datacontext.SaveChanges();


                                category category = datacontext.categories.Where(x => x.CategoryID == ParentProduct.CategoryID).FirstOrDefault();
                                if (category != null)
                                {
                                    int Alicategory = (int)category.AliExpressCategoryID;
                                    //UpdateProductModel updateProductModel = new UpdateProductModel();
                                    user user = SessionManager.GetUserSession();
                                    string productSKU = SyncWithAliExpress(product, Alicategory, user);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return result;
        }

        //public List<ProductGroupModel> GetPickedProducts(int UserID)
        //{
        //    List<ProductGroupModel> productGroupList = new List<ProductGroupModel>();
        //    List<SchemaProprtiesModel> schemaProprtiesModelList = new List<SchemaProprtiesModel>();
        //    try
        //    {
        //        using (DropshipDataEntities datacontext = new DropshipDataEntities())
        //        {
        //            List<SellersPickedProduct> dbSellerPickedProducts = datacontext.SellersPickedProducts.Where(x => x.UserID == UserID && x.AliExpressProductID != null).GroupBy(x => x.ParentProductID).Select(x => x.FirstOrDefault()).ToList();
        //            if (dbSellerPickedProducts.Count > 0)
        //            {
        //                foreach (SellersPickedProduct item in dbSellerPickedProducts)
        //                {

        //                    List<Product> dbParentProducts = datacontext.Products.Where(x => x.ProductID == item.ParentProductID).ToList();

        //                    if (dbParentProducts.Count > 0)
        //                    {
        //                        foreach (Product dbParentProduct in dbParentProducts)
        //                        {
        //                            ProductGroupModel productGroup = new ProductGroupModel();

        //                            List<Product> childProducts = datacontext.Products.Where(x => x.ParentProductID.ToString() == dbParentProduct.OriginalProductID).ToList();

        //                            productGroup.ParentProduct = GenerateProductViewModel(dbParentProduct);
        //                            string productid = dbParentProduct.ProductID.ToString();
        //                            if (productGroup.ParentProduct != null)
        //                            {
        //                                productGroup.ParentProduct.AliExpressProductID = item.AliExpressProductID; //datacontext.SellersPickedProducts.Where(x => x.ParentProductID == dbParentProduct.ProductID).Select(x => x.AliExpressProductID).FirstOrDefault();
        //                            }
        //                            ////--------------Schema Model Properties--------------------------------------
        //                            //SchemaProprtiesModel schemaProprtiesModel = schemaProprtiesModelList.Where(sp => sp.AliExpressID == productGroup.ParentProduct.AliExpressCategoryID).FirstOrDefault();
        //                            //if (schemaProprtiesModel == null)
        //                            //{
        //                            //    schemaProprtiesModel = GenerateSchemaPropertyModel(productGroup.ParentProduct.AliExpressCategoryID);
        //                            //    schemaProprtiesModelList.Add(schemaProprtiesModel);
        //                            //}
        //                            List<SellerPickedProductSKU> sellerPickedProductSKUs = datacontext.SellerPickedProductSKUs.Where(x => x.ProductId == dbParentProduct.ProductID && x.UserId == UserID).ToList();
        //                            productGroup.ChildProductList = new List<ProductViewModel>();
        //                            long parentInvetoryTotal = 0;
        //                            long parentPriceTotal = 0;

        //                            foreach (Product dbChildProduct in childProducts)
        //                            {
        //                                ProductViewModel childProductModel = GenerateProductViewModel(dbChildProduct);
        //                                //childProductModel.schemaProprtiesModel = schemaProprtiesModel;
        //                                childProductModel = AddUpdatedValues(childProductModel);
        //                                productGroup.ChildProductList.Add(childProductModel);
        //                                parentInvetoryTotal = parentInvetoryTotal + Convert.ToInt32(childProductModel.Inventory);
        //                                parentPriceTotal = parentPriceTotal + Convert.ToInt32(childProductModel.Cost);
        //                            }
        //                            productGroup.ParentProduct.Inventory = parentInvetoryTotal.ToString();
        //                            productGroup.ParentProduct.Cost = parentPriceTotal;
        //                            productGroupList.Add(productGroup);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex.ToString());
        //    }
        //    return productGroupList;
        //}

        public List<ProductGroupModel> GetPickedProducts(int UserID)
        {
            List<ProductGroupModel> productGroupList = new List<ProductGroupModel>();
            List<SchemaProprtiesModel> schemaProprtiesModelList = new List<SchemaProprtiesModel>();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    List<sellerspickedproduct> dbSellerPickedProducts = datacontext.sellerspickedproducts.Where(x => x.UserID == UserID && x.AliExpressProductID != null && x.AliExpressProductID != string.Empty).GroupBy(x => x.ParentProductID).Select(x => x.FirstOrDefault()).ToList();
                    if (dbSellerPickedProducts.Count > 0)
                    {
                        foreach (sellerspickedproduct item in dbSellerPickedProducts)
                        {
                            List<product> dbParentProducts = datacontext.products.Where(x => x.ProductID == item.ParentProductID).ToList();
                            if (dbParentProducts.Count > 0)
                            {
                                foreach (product dbParentProduct in dbParentProducts)
                                {
                                    ProductGroupModel productGroup = new ProductGroupModel();

                                    List<product> childProducts = datacontext.products.Where(x => x.ParentProductID == dbParentProduct.ProductID).ToList();

                                    double num;
                                    if (double.TryParse(dbParentProduct.Cost, out num))
                                    {

                                        productGroup.ParentProduct = GenerateProductViewModel(dbParentProduct);
                                        productGroup.ParentProduct.IsOnline = item.IsOnline;
                                        productGroup.ParentProduct.SellerPrice = item.SellerPrice;
                                        if (productGroup.ParentProduct != null)
                                        {
                                            productGroup.ParentProduct.AliExpressProductID = item.AliExpressProductID; //datacontext.SellersPickedProducts.Where(x => x.ParentProductID == dbParentProduct.ProductID).Select(x => x.AliExpressProductID).FirstOrDefault();
                                        }
                                    }
                                    ////--------------Schema Model Properties--------------------------------------
                                    //SchemaProprtiesModel schemaProprtiesModel = schemaProprtiesModelList.Where(sp => sp.AliExpressID == productGroup.ParentProduct.AliExpressCategoryID).FirstOrDefault();
                                    //if (schemaProprtiesModel == null)
                                    //{
                                    //    schemaProprtiesModel = GenerateSchemaPropertyModel(productGroup.ParentProduct.AliExpressCategoryID);
                                    //    schemaProprtiesModelList.Add(schemaProprtiesModel);
                                    //}
                                    List<sellerpickedproductsku> sellerPickedProductSKUs = datacontext.sellerpickedproductskus.Where(x => x.ProductId == dbParentProduct.ProductID && x.UserId == UserID).ToList();
                                    productGroup.ChildProductList = new List<ProductViewModel>();
                                    long parentInvetoryTotal = 0;
                                    long parentPriceTotal = 0;


                                    bool hasChild = false;
                                    if (childProducts.Count > 0)
                                    {
                                        hasChild = true;
                                        foreach (product dbChildProduct in childProducts)
                                        {
                                            if (double.TryParse(dbChildProduct.Cost, out num) == false)
                                            {
                                                productGroup.ChildProductList = new List<ProductViewModel>();
                                                break;
                                            }
                                            ProductViewModel childProductModel = GenerateProductViewModel(dbChildProduct);
                                            //childProductModel.schemaProprtiesModel = schemaProprtiesModel;
                                            childProductModel = AddUpdatedValues(childProductModel);
                                            productGroup.ChildProductList.Add(childProductModel);
                                            //parentInvetoryTotal = parentInvetoryTotal + Convert.ToInt32(childProductModel.Inventory);
                                            //parentPriceTotal = parentPriceTotal + Convert.ToInt32(childProductModel.Cost);
                                        }
                                        //productGroup.ParentProduct.Inventory = parentInvetoryTotal.ToString();
                                        //productGroup.ParentProduct.Cost = parentPriceTotal;
                                        // productGroupList.Add(productGroup);
                                    }
                                    if (hasChild == false)
                                    {
                                        productGroupList.Add(productGroup);
                                    }
                                    else
                                    {
                                        if (productGroup.ChildProductList.Count > 0)
                                        {
                                            productGroupList.Add(productGroup);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return productGroupList;
        }

        public ProductViewModel AddUpdatedValues(ProductViewModel productModel)
        {
            if (productModel != null)
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    sellerpickedproductsku dbPickedProductSKU = datacontext.sellerpickedproductskus.Where(m => m.SKUCode == productModel.SkuID).FirstOrDefault();
                    if (dbPickedProductSKU != null)
                    {
                        //productModel.UpdatedInvetory = dbPickedProduct.UpdatedInventory.Value;
                        productModel.UpdatedPrice = Convert.ToDouble(dbPickedProductSKU.UpdatedPrice.Value);
                    }
                    else
                    {
                        productModel.UpdatedInvetory = 0;
                        productModel.UpdatedPrice = 0;
                    }
                }
            }

            return productModel;
        }



        public ProductViewModel GenerateProductViewModel(product dbChildProduct)
        {
            ProductViewModel productModel = new ProductViewModel();
            productModel.ProductID = dbChildProduct.ProductID;
            productModel.Title = dbChildProduct.Title;
            productModel.CategoryID = dbChildProduct.CategoryID;
            productModel.Cost = dbChildProduct.Cost;
            productModel.SellingPriceCurrency = dbChildProduct.SellingPriceCurrency;
            productModel.OriginalProductID = dbChildProduct.OriginalProductID;
            productModel.Brand = dbChildProduct.Brand;
            productModel.Description = Encoding.UTF8.GetString(dbChildProduct.Description);
            productModel.Inventory = dbChildProduct.Inventory;
            productModel.ManufacturerName = dbChildProduct.ManufacturerName;
            productModel.ExternalCode = dbChildProduct.ExternalCode;
            productModel.ExternalCodeType = dbChildProduct.ExternalCodeType;
            productModel.ParentProductID = dbChildProduct.ParentProductID;
            productModel.NoOfCustomerReviews = dbChildProduct.NoOfCustomerReviews;
            productModel.CustomerReviewRating = dbChildProduct.CustomerReviewRating;
            productModel.NetWeight = dbChildProduct.NetWeight;
            productModel.ShippingWeight = dbChildProduct.ShippingWeight;
            productModel.CountryOfOrigin = dbChildProduct.CountryOfOrigin;
            productModel.Size = dbChildProduct.Size;
            productModel.Color = dbChildProduct.Color;
            productModel.SourceWebsite = dbChildProduct.SourceWebsite;
            productModel.IsActive = dbChildProduct.IsActive;
            productModel.SkuID = dbChildProduct.SkuID;

            if (productModel.CategoryID > 0)
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    category dbCategoryModel = datacontext.categories.Where(m => m.CategoryID == productModel.CategoryID).FirstOrDefault();
                    if (dbCategoryModel != null)
                    {
                        productModel.CategoryName = dbCategoryModel.Name;
                        productModel.AliExpressCategoryID = dbCategoryModel.AliExpressCategoryID.Value;
                    }
                }
            }
            return productModel;
        }

        public bool UpdatePickedProduct(List<scproductModel> products, int userId)
        {
            bool result = false;
            try
            {
                if (products.Count > 0)
                {
                    foreach (scproductModel productModel in products)
                    {
                        using (DropshipDataEntities datacontext = new DropshipDataEntities())
                        {
                            if (productModel.SKUModels == null)
                            {
                                sellerspickedproduct dbPickedProduct = datacontext.sellerspickedproducts.Where(p => p.AliExpressProductID == productModel.aliExpressProductId.ToString() && p.UserID == userId).FirstOrDefault();
                                if (dbPickedProduct != null)
                                {
                                    dbPickedProduct.SellerPrice = productModel.price;
                                    dbPickedProduct.ItemModifyBy = userId;
                                    dbPickedProduct.ItemModifyWhen = DateTime.Now;
                                    datacontext.Entry(dbPickedProduct).State = System.Data.Entity.EntityState.Modified;
                                    datacontext.SaveChanges();
                                    result = true;
                                }
                            }
                            else
                            {
                                foreach (ProductSKUModel productSKUModel in productModel.SKUModels)
                                {
                                    sellerpickedproductsku dbPickedProduct = datacontext.sellerpickedproductskus.Where(p => p.ProductId == productSKUModel.childproductId).FirstOrDefault();
                                    if (dbPickedProduct != null)
                                    {
                                        //long existingInventory = dbPickedProduct.UpdatedInventory.Value > 0 ? dbPickedProduct.UpdatedInventory.Value : 0;

                                        //dbPickedProduct.UpdatedInventory = Convert.ToInt32(item.PickedInventory);
                                        dbPickedProduct.UpdatedPrice = Convert.ToInt32(productSKUModel.price);
                                        dbPickedProduct.UpdatedBy = userId;
                                        dbPickedProduct.UpdatedDate = DateTime.Now;
                                        datacontext.Entry(dbPickedProduct).State = System.Data.Entity.EntityState.Modified;
                                        datacontext.SaveChanges();
                                        //dbProduct.Inventory = Convert.ToString(Convert.ToInt32(dbProduct.Inventory) - (dbPickedProduct.UpdatedInventory - existingInventory));
                                        result = true;
                                    }
                                }
                            }
                            string productSKU = UpdatePriceOnAliExpressJson(products, userId);
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public string SyncWithAliExpress(scproductModel scProduct, int AliCategoryID, user user)
        {
            string result = String.Empty;
            try
            {
                //String Schema = GetSchemaByCategory(AliCategoryID);
                string json = String.Empty;
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret, "json");
                AliexpressSolutionFeedSubmitRequest req = new AliexpressSolutionFeedSubmitRequest();
                req.OperationType = "PRODUCT_CREATE";
                List<AliexpressSolutionFeedSubmitRequest.SingleItemRequestDtoDomain> list2 = new List<AliexpressSolutionFeedSubmitRequest.SingleItemRequestDtoDomain>();
                AliexpressSolutionFeedSubmitRequest.SingleItemRequestDtoDomain obj3 = new AliexpressSolutionFeedSubmitRequest.SingleItemRequestDtoDomain();

                //obj3.ItemContent = "{\"category_id\":" + 348 + ",\"title_multi_language_list\":[{\"locale\":\"en_US\",\"title\":\"" + dbProduct.Title + "\"}]," +
                //    "\"description_multi_language_list\":[{\"locale\":\"en_US\"," +
                //    "\"module_list\":[{\"type\":\"html\",\"html\":{\"content\":\"" + dbProduct.Description + "\"}}]}],\"locale\":\"en_US\"," +
                //    "\"product_units_type\":\"" + dbProduct.Unit + "\"," +
                //    "\"image_url_list\":[\"" + StaticValues.sampleImage + "\"]," +
                //    "\"category_attributes\":{\"Brand Name\":{\"value\":\"" + dbProduct.Brand + "\"},\"Material\":{\"value\":[\"47\",\"49\"]}}," +
                //    "\"sku_info_list\":[" +
                //    "{\"sku_code\":\"" + dbProduct.OriginalProductID + "\",\"inventory\":" + dbProduct.Inventory + ",\"price\":" + dbProduct.Cost + "," +
                //    "\"discount_price\":" + dbProduct.Cost * .9 + ",\"sku_attributes\":{\"Size\":{\"value\":\"" + dbProduct.Size + "\"}," +
                //    "\"Color\":{\"alias\":\"" + dbProduct.Color + "\"," +
                //    "\"sku_image_url\":\"" + StaticValues.sampleImage + "\",\"value\":\"771\"}}}" +
                //    "]," +
                //    "\"inventory_deduction_strategy\":\"place_order_withhold\",\"package_weight\":23.00,\"package_length\":23,\"package_height\":23,\"package_width\":30," +
                //    "\"shipping_preparation_time\":20,\"shipping_template_id\":\"1013213014\",\"service_template_id\":\"0\"}";

                string strReq = GenerateSyncProductRequestJson(scProduct, AliCategoryID);
                if (strReq != null)
                {
                    obj3.ItemContent = strReq;

                    //obj3.ItemContent = "{\"category_attributes\":{\"Brand Name\":{\"value\":\"201470514\"},\"Material\":{\"value\":[\"47\",\"49\"]}},\"category_id\":348,\"description_multi_language_list\":[{\"locale\":\"en_US\",\"module_list\":[{\"html\":{\"content\":\"test\"},\"type\":\"html\"}]}],\"image_url_list\":[\"https://www.begorgeousstylesandbeauty.com/wp-content/uploads/2016/01/2015-100-High-Quality-Mens-Dress-Shirts-Blue-Shirt-Men-Causal-Striped-Shirt-Men-Camisa-Social.jpg\"],\"inventory_deduction_strategy\":\"place_order_withhold\",\"locale\":\"en_US\",\"package_height\":234,\"package_length\":234,\"package_weight\":234.00,\"package_width\":234,\"product_units_type\":\"100000015\",\"service_template_id\":\"0\",\"shipping_preparation_time\":20,\"shipping_template_id\":\"1013213014\",\"sku_info_list\":[{\"discount_price\":1,\"inventory\":11,\"price\":11,\"sku_attributes\":{\"Size\":{\"value\":\"4181\"},\"Color\":{\"alias\":\"32\",\"sku_image_url\":\"https://ae01.alicdn.com/kf/HTB1TZJRVkvoK1RjSZFwq6AiCFXa0.jpg_350x350.jpg\",\"value\":\"771\"}},\"sku_code\":\"234\"}],\"title_multi_language_list\":[{\"locale\":\"en_US\",\"title\":\"English\"}]}";
                    obj3.ItemContentId = Guid.NewGuid().ToString();
                    list2.Add(obj3);
                    req.ItemList_ = list2;

                    //req.ProductInstanceRequest = "{\"category_id\":348,\"title_multi_language_list\":[{\"locale\":\"es_ES\",\"title\":\"atestproducttesttest001\"}],\"description_multi_language_list\":[{\"locale\":\"es_ES\",\"module_list\":[{\"type\":\"html\",\"html\":{\"content\":\"unotesttestdescription002\"}}]}],\"locale\":\"es_ES\",\"product_units_type\":\"100000015\",\"image_url_list\":[\"https://upload.wikimedia.org/wikipedia/commons/b/ba/E-SENS_architecture.jpg\"],\"category_attributes\":{\"BrandName\":{\"value\":\"200010868\"},\"ShirtsType\":{\"value\":\"200001208\"},\"Material\":{\"value\":[\"567\"]},\"SleeveLength(cm)\":{\"value\":\"200001500\"}},\"sku_info_list\":[{\"sku_code\":\"WEO19293829123\",\"inventory\":3,\"price\":9900,\"discount_price\":9800,\"sku_attributes\":{\"Size\":{\"alias\":\"Uni\",\"value\":\"200003528\"}}}],\"inventory_deduction_strategy\":\"payment_success_deduct\",\"package_weight\":1.5,\"package_length\":10,\"package_height\":20,\"package_width\":30,\"shipping_preparation_time\":3,\"shipping_template_id\":\"714844311\",\"service_template_id\":\"0\"}";
                    if (SessionManager.GetAccessToken().access_token != null)
                    {
                        AliexpressSolutionFeedSubmitResponse rsp = client.Execute(req, SessionManager.GetAccessToken().access_token);
                        AliexpressSolutionFeedQueryRequest fqReq = new AliexpressSolutionFeedQueryRequest();
                        fqReq.JobId = rsp.JobId;
                        //fqReq.JobId = 200000021289874453;
                        AliexpressSolutionFeedQueryResponse fqRsp = client.Execute(fqReq, SessionManager.GetAccessToken().access_token);
                        result = JsonConvert.SerializeObject(fqRsp.ResultList);
                        _aliExpressJobLogService.AddAliExpressJobLog(new aliexpressjoblog()
                        {
                            JobId = rsp.JobId,
                            ContentId = obj3.ItemContentId,
                            SuccessItemCount = fqRsp.SuccessItemCount,
                            UserID = user.UserID,
                            ProductID = scProduct.productId.ToString(),
                            ProductDetails = obj3.ItemContent,
                            Result = result
                        });

                        List<AliexpressSolutionFeedResponseModel> solutionResponse = JsonConvert.DeserializeObject<List<AliexpressSolutionFeedResponseModel>>(result);
                        if (solutionResponse != null && solutionResponse.Count > 0)
                        {
                            ItemExecutionResultModel itemModel = JsonConvert.DeserializeObject<ItemExecutionResultModel>(solutionResponse[0].ItemExecutionResult);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                logger.Info(ex.ToString());
            }
            return result;
        }


        public string UpdatePriceOnAliExpress(List<scproductModel> scProducts, int userId)
        {
            string result = String.Empty;
            try
            {
                foreach (scproductModel scproductModel in scProducts)
                {


                    ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret, "json");



                    AliexpressSolutionBatchProductPriceUpdateRequest req = new AliexpressSolutionBatchProductPriceUpdateRequest();
                    List<AliexpressSolutionBatchProductPriceUpdateRequest.SynchronizeProductRequestDtoDomain> list2 = new List<AliexpressSolutionBatchProductPriceUpdateRequest.SynchronizeProductRequestDtoDomain>();
                    AliexpressSolutionBatchProductPriceUpdateRequest.SynchronizeProductRequestDtoDomain obj3 = new AliexpressSolutionBatchProductPriceUpdateRequest.SynchronizeProductRequestDtoDomain();

                    obj3.ProductId = scproductModel.productId;
                    List<AliexpressSolutionBatchProductPriceUpdateRequest.SynchronizeSkuRequestDtoDomain> list5 = new List<AliexpressSolutionBatchProductPriceUpdateRequest.SynchronizeSkuRequestDtoDomain>();

                    foreach (ProductSKUModel productSKUModel in scproductModel.SKUModels)
                    {
                        AliexpressSolutionBatchProductPriceUpdateRequest.SynchronizeSkuRequestDtoDomain obj6 = new AliexpressSolutionBatchProductPriceUpdateRequest.SynchronizeSkuRequestDtoDomain();
                        obj6.Price = productSKUModel.price.ToString();
                        obj6.DiscountPrice = productSKUModel.discount_price.ToString();
                        obj6.SkuCode = productSKUModel.skuCode;
                        list5.Add(obj6);
                    }

                    obj3.MultipleSkuUpdateList = list5;
                    AliexpressSolutionBatchProductPriceUpdateRequest.MultiCountryPriceConfigurationDtoDomain obj7 = new AliexpressSolutionBatchProductPriceUpdateRequest.MultiCountryPriceConfigurationDtoDomain();
                    obj7.PriceType = "absolute";
                    List<AliexpressSolutionBatchProductPriceUpdateRequest.SingleCountryPriceDtoDomain> list9 = new List<AliexpressSolutionBatchProductPriceUpdateRequest.SingleCountryPriceDtoDomain>();
                    AliexpressSolutionBatchProductPriceUpdateRequest.SingleCountryPriceDtoDomain obj10 = new AliexpressSolutionBatchProductPriceUpdateRequest.SingleCountryPriceDtoDomain();

                    obj10.ShipToCountry = "";
                    list9.Add(obj10);

                    obj7.CountryPriceList = list9;
                    obj3.MultiCountryPriceConfiguration = obj7;


                    list2.Add(obj3);
                    req.MutipleProductUpdateList_ = list2;
                    AliexpressSolutionBatchProductPriceUpdateResponse rsp = client.Execute(req, SessionManager.GetAccessToken().access_token);

                    _aliExpressJobLogService.AddAliExpressJobLog(new aliexpressjoblog()
                    {
                        JobId = null,
                        ContentId = obj3.ProductId.ToString(),
                        SuccessItemCount = rsp.UpdateSuccessfulList.Count(),
                        UserID = userId,
                        ProductID = "0",
                        ProductDetails = obj3.ProductId.ToString(),
                        Result = rsp.Body
                    });
                    Console.WriteLine(rsp.Body);
                }

            }
            catch (Exception ex)
            {
                logger.Info(ex.ToString());
            }
            return result;
        }

        public string UpdatePriceOnAliExpressJson(List<scproductModel> scProducts, int userId)
        {
            string result = String.Empty;
            try
            {
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret, "json");
                AliexpressSolutionFeedSubmitRequest req = new AliexpressSolutionFeedSubmitRequest();
                req.OperationType = "PRODUCT_PRICES_UPDATE";
                List<AliexpressSolutionFeedSubmitRequest.SingleItemRequestDtoDomain> list2 = new List<AliexpressSolutionFeedSubmitRequest.SingleItemRequestDtoDomain>();
                AliexpressSolutionFeedSubmitRequest.SingleItemRequestDtoDomain obj3 = new AliexpressSolutionFeedSubmitRequest.SingleItemRequestDtoDomain();
                List<string> skus = new List<string>();//"[{\"sku_code\": \"" + dbProduct[0].OriginalProductID + "\",\"price\": " + dbProduct[0].Cost + "}]";
                foreach (scproductModel scproductModel in scProducts)
                {
                    if (scproductModel.SKUModels == null)
                    {
                        using (DropshipDataEntities datacontext = new DropshipDataEntities())
                        {
                            string parent_SKUCOde = (from p in datacontext.products
                                                     from sp in datacontext.sellerspickedproducts.Where(x => x.ParentProductID == p.ProductID && x.UserID == userId && x.AliExpressProductID == scproductModel.aliExpressProductId.ToString())
                                                     select new
                                                     {
                                                         parent_SKUCOde = p.ProductID.ToString()
                                                     }).Select(x => x.parent_SKUCOde).FirstOrDefault();

                            skus.Add("{\"sku_code\": \"" + parent_SKUCOde + "\",\"price\": " + (100000 + scproductModel.price) + "}");
                        }
                    }
                    else
                    {
                        foreach (ProductSKUModel productSKUModel in scproductModel.SKUModels)
                        {
                            skus.Add("{\"sku_code\": \"" + productSKUModel.skuCode + "\",\"price\": " + (100000 + productSKUModel.price) + "}");
                        }
                    }

                    obj3.ItemContentId = Guid.NewGuid().ToString();
                    obj3.ItemContent = "{\"aliexpress_product_id\":" + scproductModel.aliExpressProductId + ",\"multiple_sku_update_list\":[" + string.Join(",", skus.ToArray()) + "]}";

                    list2.Add(obj3);
                    req.ItemList_ = list2;


                    if (SessionManager.GetAccessToken().access_token != null)
                    {
                        AliexpressSolutionFeedSubmitResponse rsp = client.Execute(req, SessionManager.GetAccessToken().access_token);
                        AliexpressSolutionFeedQueryRequest fqReq = new AliexpressSolutionFeedQueryRequest();
                        fqReq.JobId = rsp.JobId;
                        //fqReq.JobId = 200000021289874453;
                        AliexpressSolutionFeedQueryResponse fqRsp = client.Execute(fqReq, SessionManager.GetAccessToken().access_token);
                        result = JsonConvert.SerializeObject(fqRsp.ResultList);
                        _aliExpressJobLogService.AddAliExpressJobLog(new aliexpressjoblog()
                        {
                            JobId = rsp.JobId,
                            ContentId = obj3.ItemContentId,
                            SuccessItemCount = fqRsp.SuccessItemCount,
                            UserID = userId,
                            ProductID = scproductModel.productId.ToString(),
                            ProductDetails = obj3.ItemContent,
                            Result = result
                        });

                        List<AliexpressSolutionFeedResponseModel> solutionResponse = JsonConvert.DeserializeObject<List<AliexpressSolutionFeedResponseModel>>(result);
                        if (solutionResponse != null && solutionResponse.Count > 0)
                        {
                            ItemExecutionResultModel itemModel = JsonConvert.DeserializeObject<ItemExecutionResultModel>(solutionResponse[0].ItemExecutionResult);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                logger.Info(ex.ToString());
            }
            return result;
        }

        public string GetSchemaByCategory(long AliCategoryID)
        {
            string SchemaString = String.Empty;
            try
            {
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret, "json");
                AliexpressSolutionProductSchemaGetRequest req = new AliexpressSolutionProductSchemaGetRequest();
                req.AliexpressCategoryId = AliCategoryID;
                AliexpressSolutionProductSchemaGetResponse rsp = client.Execute(req, SessionManager.GetAccessToken().access_token);
                Console.WriteLine(rsp.Body);
                AliExpresssProductSchemaModel responseBody = JsonConvert.DeserializeObject<AliExpresssProductSchemaModel>(rsp.Body);
                SchemaString = responseBody.aliexpress_solution_product_schema_get_response.Result.Schema;
            }
            catch (Exception ex)
            {
                logger.Info(ex.ToString());
            }
            return SchemaString;
        }

        public AliExpressPostProductModel SetSyncProductModel(scproductModel scproduct, int AliCategoryID)
        {
            AliExpressPostProductModel model = new AliExpressPostProductModel();
            product dbProduct;
            using (DropshipDataEntities datacontext = new DropshipDataEntities())
            {
                dbProduct = datacontext.products.Where(x => x.OriginalProductID == scproduct.productId.ToString()).FirstOrDefault(); ;
                if (dbProduct != null)
                {

                    model.category_id = AliCategoryID;
                    model.locale = "en_US";
                    model.product_units_type = dbProduct.Unit; //gram
                    model.image_url_list = new List<string>() { StaticValues.sampleImage };
                    model.inventory_deduction_strategy = "place_order_withhold";

                    model.title_multi_language_list = new List<TitleMultiLanguageList>();
                    TitleMultiLanguageList _titleMultiLanguageList = new TitleMultiLanguageList();
                    _titleMultiLanguageList.locale = "en_US";
                    _titleMultiLanguageList.title = dbProduct.Title;
                    model.title_multi_language_list.Add(_titleMultiLanguageList);

                    model.description_multi_language_list = new List<DescriptionMultiLanguageList>();
                    DescriptionMultiLanguageList _description_multi_language_list = new DescriptionMultiLanguageList();
                    _description_multi_language_list.locale = "en_US";
                    _description_multi_language_list.module_list = new List<ModuleList>();
                    ModuleList _moduleList = new ModuleList();
                    _moduleList.type = "html";
                    _moduleList.html = new Html();
                    _moduleList.html.content = Encoding.UTF8.GetString(dbProduct.Description);

                    _description_multi_language_list.module_list.Add(_moduleList);
                    model.description_multi_language_list.Add(_description_multi_language_list);

                    model.package_height = 50;
                    model.package_length = 20;
                    model.package_height = 30;
                    model.package_width = 50;
                    model.shipping_preparation_time = 20;
                    model.shipping_template_id = 1013213014;
                    model.service_template_id = 0;


                    model.sku_info_list = new List<SkuInfoList>();
                    foreach (ProductSKUModel productSKU in scproduct.SKUModels)
                    {
                        SkuInfoList sku_info = new SkuInfoList();
                        sku_info.inventory = Convert.ToInt32(productSKU.inventory);
                        sku_info.sku_code = productSKU.skuCode;
                        sku_info.price = Convert.ToInt32(productSKU.price);
                        sku_info.sku_attributes = new SkuAttributes();
                        sku_info.sku_attributes.Size = new Size()
                        {
                            value = "200003528"
                        };
                        sku_info.sku_attributes.color = new Color()
                        {
                            value = "71"
                        };
                        model.sku_info_list.Add(sku_info);
                    }


                    model.category_attributes = new CategoryAttributes();

                    model.category_attributes.BrandName = new BrandName();
                    model.category_attributes.BrandName.value = "201470514";
                    model.category_attributes.ManufacturerName = new ManufacturerName();
                    model.category_attributes.ManufacturerName.value = dbProduct.ManufacturerName;

                    model.locale = "en_EN";
                }
            }
            return model;
        }

        public string GenerateSyncProductRequestJson(scproductModel scproduct, int AliCategoryID)
        {
            string result = string.Empty;

            using (DropshipDataEntities datacontext = new DropshipDataEntities())
            {
                product dbProduct = datacontext.products.Where(x => x.ProductID == scproduct.productId).FirstOrDefault();
                string CategorySchema = GetSchemaByCategory(AliCategoryID);
                CategorySchemaModel categorySchemaModel = JsonConvert.DeserializeObject<CategorySchemaModel>(CategorySchema);
                List<PropertyModel> colors = new List<PropertyModel>();
                List<PropertyModel> sizes = new List<PropertyModel>();
                List<PropertyModel> brands = new List<PropertyModel>();
                List<PropertyModel> units = new List<PropertyModel>();
                if (categorySchemaModel != null)
                {
                    if (categorySchemaModel.properties.sku_info_list.items.properties.sku_attributes.properties.Color != null)
                    {
                        foreach (OneOf19 item in categorySchemaModel.properties.sku_info_list.items.properties.sku_attributes.properties.Color.properties.value.oneOf)
                        {
                            PropertyModel propertyModel = new PropertyModel();
                            propertyModel.PropertyID = item.@const;
                            propertyModel.PropertyName = item.title;
                            colors.Add(propertyModel);
                        }
                    }

                    if (categorySchemaModel.properties.sku_info_list.items.properties.sku_attributes.properties.Size != null)
                    {
                        foreach (OneOf18 item in categorySchemaModel.properties.sku_info_list.items.properties.sku_attributes.properties.Size.properties.value.oneOf)
                        {
                            PropertyModel propertyModel = new PropertyModel();
                            propertyModel.PropertyID = item.@const;
                            propertyModel.PropertyName = item.title;
                            sizes.Add(propertyModel);
                        }
                    }

                    if (categorySchemaModel.properties.category_attributes.properties.BrandName != null)
                    {
                        foreach (OneOf7 item in categorySchemaModel.properties.category_attributes.properties.BrandName.properties.value.oneOf)
                        {
                            PropertyModel propertyModel = new PropertyModel();
                            propertyModel.PropertyID = item.@const;
                            propertyModel.PropertyName = item.title;
                            brands.Add(propertyModel);
                        }
                    }

                    if (categorySchemaModel.properties.product_units_type != null)
                    {
                        foreach (OneOf6 item in categorySchemaModel.properties.product_units_type.oneOf)
                        {
                            PropertyModel propertyModel = new PropertyModel();
                            propertyModel.PropertyID = item.@const;
                            propertyModel.PropertyName = item.title;
                            units.Add(propertyModel);
                        }
                    }
                }


                if (dbProduct != null)
                {
                    List<string> skuStr = new List<string>();
                    List<int> listSizeDefaultNumbers = new List<int>();
                    List<int> listColorDefaultNumbers = new List<int>();
                    if (scproduct.SKUModels == null)
                    {
                        //string Size = sizes.Where(x => x.PropertyName == dbProduct.Size).Select(x => x.PropertyID).FirstOrDefault();
                        string defaultSize = sizes.Count() > 0 ? sizes[0].PropertyID : null;

                        //string color = colors.Where(x => x.PropertyName.ToLower() == originalSKU.Color.ToLower()).Select(x => x.PropertyID).FirstOrDefault();
                        string defaultColor = colors.Count() > 0 ? colors[0].PropertyID : null;
                        string Size = dbProduct.Size;
                        string dbProductColor = dbProduct.Color != null ? dbProduct.Color.ToLower() : dbProduct.Color;
                        //string color = colors.Where(x => x.PropertyName.ToLower() == dbProductColor).Select(x => x.PropertyID).FirstOrDefault();
                        string color = dbProduct.Color;

                        ali_SKUModel ali_SKUModel = new ali_SKUModel();
                        ali_SKUModel.inventory = dbProduct.Inventory != null ? dbProduct.Inventory.ToString() : "0";
                        ali_SKUModel.price = scproduct.price + 100000;
                        ali_SKUModel.sku_code = dbProduct.ProductID.ToString();
                        //string skuIMage = datacontext.ProductMedias.Where(x => x.ProductID == dbProduct.ProductID && x.IsMainImage == true).Select(x => x.MediaLink).FirstOrDefault();
                        ali_SKUModel.sku_attributes = getSKUattrStr(categorySchemaModel, Size, color, defaultSize, defaultColor);

                        string jsonstr = JsonConvert.SerializeObject(ali_SKUModel, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        skuStr.Add(jsonstr);
                        //only required
                        //skuStr.Add("{\"inventory\":" + dbProduct.Inventory + ",\"price\":" + scproduct.price + ",\"sku_attributes\":{\"Color\":{\"alias\":\"32\",\"sku_image_url\":\"" + StaticValues.sampleImage + "\",\"value\":\"" + color + "\"}},\"sku_code\":\"" + dbProduct.OriginalProductID + "\"}");
                        //all
                        //skuStr.Add("{\"inventory\":" + dbProduct.Inventory + ",\"price\":" + scproduct.price + ",\"sku_attributes\":{\"Size\":{\"value\":\"" + Size + "\"},\"Color\":{\"alias\":\"32\",\"sku_image_url\":\"" + StaticValues.sampleImage + "\",\"value\":\"" + color + "\"}},\"sku_code\":\"" + dbProduct.OriginalProductID + "\"}");
                    }
                    else
                    {
                        int sizecount = 0, colorcount = 0;
                        foreach (ProductSKUModel productSKU in scproduct.SKUModels)
                        {
                            product originalSKU = datacontext.products.Where(x => x.SkuID == productSKU.skuCode).FirstOrDefault();
                            //string Size = sizes.Where(x => x.PropertyName == originalSKU.Size).Select(x => x.PropertyID).FirstOrDefault();
                            string defaultSize = sizes.Count() > 0 ? sizes[sizecount].PropertyID : null;
                            string Size = originalSKU.Size;

                            //string color = colors.Where(x => x.PropertyName.ToLower() == originalSKU.Color.ToLower()).Select(x => x.PropertyID).FirstOrDefault();
                            string defaultColor = colors.Count() > 0 ? colors[colorcount].PropertyID : null;
                            string color = originalSKU.Color;

                            ali_SKUModel ali_SKUModel = new ali_SKUModel();
                            ali_SKUModel.inventory = productSKU.inventory.ToString();
                            ali_SKUModel.price = productSKU.price + 100000;
                            ali_SKUModel.sku_code = originalSKU.SkuID;
                            //string skuIMage = datacontext.ProductMedias.Where(x => x.ProductID == productSKU.childproductId).Select(x => x.MediaLink).FirstOrDefault();
                            ali_SKUModel.sku_attributes = getSKUattrStr(categorySchemaModel, Size, color, defaultSize, defaultColor);

                            string jsonstr = JsonConvert.SerializeObject(ali_SKUModel, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                            skuStr.Add(jsonstr);

                            sizecount++; colorcount++;
                            if (sizes.Count() <= sizecount) { sizecount = 0; }
                            if (colors.Count() <= colorcount) { colorcount = 0; }
                        }
                    }

                    string brandname = brands.Where(x => x.PropertyName == dbProduct.Brand).Select(x => x.PropertyID).FirstOrDefault();
                    string unit = units.Where(x => x.PropertyName == dbProduct.Unit).Select(x => x.PropertyID).FirstOrDefault();
                    MySqlConnection mcon = new MySqlConnection("host=localhost;user=root;password=123456;database=dropshipdata;");
                    MySqlCommand SelectCommand = new MySqlCommand(" SELECT MediaLink FROM dropshipdata.productmedia where ProductID=" + dbProduct.ProductID, mcon);

                    string tempparentImages = string.Empty;
                    MySqlDataReader myReader;
                    mcon.Open();
                    myReader = SelectCommand.ExecuteReader();
                    while (myReader.Read())
                    {
                        tempparentImages = myReader["MediaLink"].ToString();
                    }
                    mcon.Close();
                    List<string> productImages = new List<string>();
                    tempparentImages = tempparentImages.Trim().Replace("[", "");
                    tempparentImages = tempparentImages.Replace("]", "");
                    productImages = tempparentImages.Split(',').ToList();
                    //List<string> productImages = datacontext.productmedias.Where(x => x.ProductID == dbProduct.ProductID && x.IsMainImage == "true").Select(x => x.MediTaext).ToList();

                    if (productImages.Count < 6)
                    {
                        if (scproduct.SKUModels == null)
                        {
                        }
                        else
                        {
                            foreach (ProductSKUModel productSKU in scproduct.SKUModels)
                            {
                                if (productImages.Count < 6)
                                {
                                    product originalSKU = datacontext.products.Where(x => x.SkuID == productSKU.skuCode).FirstOrDefault();
                                    MySqlConnection mcon1 = new MySqlConnection("host=localhost;user=root;password=123456;database=dropshipdata;");
                                    MySqlCommand SelectCommand1 = new MySqlCommand(" SELECT MediaLink FROM dropshipdata.productmedia where ProductID=" + originalSKU.ProductID, mcon1);

                                    string tempchildImages = string.Empty;
                                    MySqlDataReader myReader1;
                                    mcon1.Open();
                                    myReader1 = SelectCommand1.ExecuteReader();
                                    while (myReader1.Read())
                                    {
                                        tempchildImages = myReader1["MediaLink"].ToString();
                                    }
                                    mcon1.Close();
                                    List<string> childproductImages = new List<string>();
                                    tempchildImages = tempchildImages.Trim().Replace("[", "");
                                    tempchildImages = tempchildImages.Replace("]", "");
                                    childproductImages = tempchildImages.Split(',').ToList();
                                    //List<string> childproductImages = datacontext.productmedias.Where(x => x.ProductID == originalSKU.ProductID && x.IsMainImage == "true").Select(x => x.MediTaext).ToList();
                                    productImages.AddRange(childproductImages);
                                }
                            }
                        }
                    }

                    if (productImages.Count > 6)
                    {
                        productImages = productImages.Take(6).ToList();
                    }
                    List<string> uploadImages = new List<string>();
                    foreach (string img in productImages)
                    {
                        uploadImages.Add(img);
                    }
                    //long? serviceTemplateID = dbProduct.ServiceTemplateID;
                    //int? shippingPreparationTime = dbProduct.ShippingPreparationTime;
                    //long? shippingTemplateID = dbProduct.ShippingTemplateID;
                    string packageHeight = dbProduct.PackageHeight != null ? dbProduct.PackageHeight : "1";
                    string packageLength = dbProduct.PackageLength != null ? dbProduct.PackageLength : "1";
                    string packageWidth = dbProduct.PackageWidth != null ? dbProduct.PackageWidth : "1";
                    string productWeight = dbProduct.NetWeight != null ? dbProduct.NetWeight.Split(' ')[0] : "234";

                    //only requred
                    //result = "{\"category_id\":" + AliCategoryID + ",\"brand_name\":" + (brandname ?? "201470514") + ",\"description_multi_language_list\":[{\"locale\":\"" + locale + "\",\"module_list\":[{\"html\":{\"content\":\"" + dbProduct.Description + "\"},\"type\":\"html\"}]}],\"image_url_list\":[" + string.Join(",", uploadImages.ToArray()) + "],\"inventory_deduction_strategy\":\""+ inventoryDeductionStrategy + "\",\"locale\":\"" + locale + "\",\"package_height\":" + packageHeight + ",\"package_length\":" + packageLength + ",\"package_weight\":" + productWeight + ",\"package_width\":" + packageWidth + ",\"product_units_type\":\"" + (unit ?? "100000015") + "\",\"service_template_id\":"+ serviceTemplateID + ",\"shipping_preparation_time\":"+ shippingPreparationTime + ",\"shipping_template_id\":"+ shippingTemplateID + ",\"sku_info_list\":[" + string.Join(",", skuStr) + "],\"title_multi_language_list\":[{\"locale\":\"" + locale + "\",\"title\":\"" + dbProduct.Title + "\"}]}";
                    result = "{\"category_id\":" + AliCategoryID + ",\"brand_name\":" + (brandname ?? "201512802") + ",\"description_multi_language_list\":[{\"locale\":\"en_US\",\"module_list\":[{\"html\":{\"content\":\"" + dbProduct.Description + "\"},\"type\":\"html\"}]}],\"image_url_list\":[" + string.Join(",", uploadImages.ToArray()) + "],\"inventory_deduction_strategy\":\"place_order_withhold\",\"locale\":\"en_US\",\"package_height\":" + packageHeight + ",\"package_length\":" + packageLength + ",\"package_weight\":" + productWeight + ",\"package_width\":" + packageWidth + ",\"product_units_type\":\"" + (unit ?? "100000015") + "\",\"service_template_id\":\"" + StaticValues.serviceTemplateID + "\",\"shipping_preparation_time\":" + StaticValues.shippingPreparationTime + ",\"shipping_template_id\":\"" + StaticValues.shippingTemplateID + "\",\"sku_info_list\":[" + string.Join(",", skuStr) + "],\"title_multi_language_list\":[{\"locale\":\"en_US\",\"title\":\"" + dbProduct.Title + "\"}]}";
                    //all
                    //result = "{\"category_attributes\":{\"Brand Name\":{\"value\":\"201470514\"},\"Material\":{\"value\":[\"47\",\"49\"]}},\"category_id\":" + AliCategoryID + ",\"description_multi_language_list\":[{\"locale\":\"en_US\",\"module_list\":[{\"html\":{\"content\":\"" + dbProduct.Description + "\"},\"type\":\"html\"}]}],\"image_url_list\":[\"" + StaticValues.sampleImage + "\"],\"inventory_deduction_strategy\":\"place_order_withhold\",\"locale\":\"en_US\",\"package_height\":234,\"package_length\":234,\"package_weight\":234.00,\"package_width\":234,\"product_units_type\":\"100000015\",\"service_template_id\":\"0\",\"shipping_preparation_time\":20,\"shipping_template_id\":\"1013213014\",\"sku_info_list\":[" + string.Join(",", skuStr) + "],\"title_multi_language_list\":[{\"locale\":\"en_US\",\"title\":\"" + dbProduct.Title + "\"}]}";
                }
            }
            return result;
        }

        public custom_sku_attributes getSKUattrStr(CategorySchemaModel categorySchemaModel, string size, string color, string defaultSize, string defaultColor)
        {
            string skuStr = string.Empty;

            custom_sku_attributes sku_attributes_obj = new custom_sku_attributes();
            if (categorySchemaModel.properties.sku_info_list.items.properties.sku_attributes.properties.Size != null && size != null)
            {
                sku_attributes_obj.Size = new custom_Size();
                sku_attributes_obj.Size.value = defaultSize;
                sku_attributes_obj.Size.alias = size;

            }
            if (categorySchemaModel.properties.sku_info_list.items.properties.sku_attributes.properties.Color != null && color != null)
            {
                sku_attributes_obj.Color = new custom_color();
                sku_attributes_obj.Color.value = defaultColor;
                sku_attributes_obj.Color.alias = color;
            }
            if (sku_attributes_obj.Size == null && sku_attributes_obj.Color == null)
            {
                sku_attributes_obj = null;
            }
            return sku_attributes_obj;
        }
        public string checkResultByJobId(long jobid)
        {
            ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret, "json");
            AliexpressSolutionFeedQueryRequest fqReq = new AliexpressSolutionFeedQueryRequest();
            fqReq.JobId = jobid;
            AliexpressSolutionFeedQueryResponse fqRsp = client.Execute(fqReq, SessionManager.GetAccessToken().access_token);
            string result = JsonConvert.SerializeObject(fqRsp.ResultList);
            return result;
        }

        //public SchemaProprtiesModel GenerateSchemaPropertyModel(long AliExpCatID)
        //{
        //    SchemaProprtiesModel schemaProprtiesModel = new SchemaProprtiesModel();
        //    try
        //    {
        //        if (AliExpCatID > 0)
        //        {
        //            String schema = GetSchemaByCategory(AliExpCatID);
        //            string CategorySchema = GetSchemaByCategory(AliExpCatID);
        //            if (!string.IsNullOrEmpty(CategorySchema))
        //            {
        //                CategorySchemaModel categorySchemaModel = JsonConvert.DeserializeObject<CategorySchemaModel>(CategorySchema);
        //                if (categorySchemaModel != null)
        //                {
        //                    schemaProprtiesModel.AliExpressID = AliExpCatID;

        //                    List<PropertyModel> units = new List<PropertyModel>();
        //                    foreach (OneOf6 item in categorySchemaModel.properties.product_units_type.oneOf)
        //                    {
        //                        PropertyModel propertyModel = new PropertyModel();
        //                        propertyModel.PropertyID = item.@const;
        //                        propertyModel.PropertyName = item.title;
        //                        units.Add(propertyModel);
        //                    }
        //                    schemaProprtiesModel.ProductUnits = units;

        //                    List<PropertyModel> brands = new List<PropertyModel>();
        //                    foreach (OneOf7 item in categorySchemaModel.properties.category_attributes.properties.BrandName.properties.value.oneOf)
        //                    {
        //                        PropertyModel propertyModel = new PropertyModel();
        //                        propertyModel.PropertyID = item.@const;
        //                        propertyModel.PropertyName = item.title;
        //                        brands.Add(propertyModel);
        //                    }
        //                    schemaProprtiesModel.ProductBrands = brands;

        //                    List<PropertyModel> colors = new List<PropertyModel>();
        //                    foreach (OneOf19 item in categorySchemaModel.properties.sku_info_list.items.properties.sku_attributes.properties.Color.properties.value.oneOf)
        //                    {
        //                        PropertyModel propertyModel = new PropertyModel();
        //                        propertyModel.PropertyID = item.@const;
        //                        propertyModel.PropertyName = item.title;
        //                        colors.Add(propertyModel);
        //                    }
        //                    schemaProprtiesModel.ProductColors = colors;

        //                    List<PropertyModel> sizes = new List<PropertyModel>();
        //                    foreach (OneOf18 item in categorySchemaModel.properties.sku_info_list.items.properties.sku_attributes.properties.Size.properties.value.oneOf)
        //                    {
        //                        PropertyModel propertyModel = new PropertyModel();
        //                        propertyModel.PropertyID = item.@const;
        //                        propertyModel.PropertyName = item.title;
        //                        sizes.Add(propertyModel);
        //                    }
        //                    schemaProprtiesModel.ProductSizes = sizes;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Info(ex.ToString());
        //    }
        //    return schemaProprtiesModel;
        //}


        public string getJobList()
        {
            string result = string.Empty;
            ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret, "json");
            AliexpressSolutionFeedListGetRequest req = new AliexpressSolutionFeedListGetRequest();
            req.CurrentPage = 2L;
            req.FeedType = "PRODUCT_CREATE";
            req.PageSize = 50L;
            req.Status = "FINISH";
            AliexpressSolutionFeedListGetResponse rsp = client.Execute(req, SessionManager.GetAccessToken().access_token);
            result = rsp.Body;
            return result;
        }

        public List<aliexpressjoblog> getJobLogData()
        {
            logger.Info("Hi");
            List<aliexpressjoblog> list = new List<aliexpressjoblog>();

            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    list = datacontext.aliexpressjoblogs.ToList();

                }
            }
            catch (Exception ex)
            {
                logger.Info(ex.ToString());
            }

            return list;
        }

        public bool updateJobLogResult(aliexpressjoblog aliExpressJobLog)
        {
            bool result = true;

            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    aliexpressjoblog obj = datacontext.aliexpressjoblogs.Where(x => x.JobId == aliExpressJobLog.JobId).FirstOrDefault();
                    if (obj != null)
                    {
                        obj.Result = aliExpressJobLog.Result;
                    }
                    datacontext.Entry(obj).State = System.Data.Entity.EntityState.Modified;
                    datacontext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                result = false;
                logger.Info(ex.ToString());
            }

            return result;
        }

        public string getResultByJobId(long jobid)
        {
            string result = null;
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    aliexpressjoblog obj = datacontext.aliexpressjoblogs.Where(x => x.JobId == jobid).FirstOrDefault();
                    if (obj != null)
                    {
                        result = obj.Result;
                    }
                }
            }
            catch (Exception ex)
            {
                result = null;
                logger.Info(ex.ToString());
            }
            return result;
        }


        public bool updateProductStatuts(string id, bool status)
        {
            bool result = false;
            bool isStatusSync = false;
            try
            {
                if (status)
                {
                    isStatusSync = false;
                    ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);
                    AliexpressPostproductRedefiningOfflineaeproductRequest req = new AliexpressPostproductRedefiningOfflineaeproductRequest();
                    req.ProductIds = id;
                    AliexpressPostproductRedefiningOfflineaeproductResponse rsp = client.Execute(req, SessionManager.GetAccessToken().access_token);
                    if (rsp.Result.Success)
                    {
                        isStatusSync = true;
                    }
                }
                else
                {
                    isStatusSync = false;
                    ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);
                    AliexpressPostproductRedefiningOnlineaeproductRequest req = new AliexpressPostproductRedefiningOnlineaeproductRequest();
                    req.ProductIds = id;
                    AliexpressPostproductRedefiningOnlineaeproductResponse rsp = client.Execute(req, SessionManager.GetAccessToken().access_token);
                    if (rsp.Result.Success)
                    {
                        isStatusSync = true;
                    }
                }
                if (isStatusSync)
                {
                    using (DropshipDataEntities datacontext = new DropshipDataEntities())
                    {
                        sellerspickedproduct obj = datacontext.sellerspickedproducts.Where(x => x.AliExpressProductID == id).FirstOrDefault();
                        if (obj != null)
                        {
                            obj.IsOnline = !status;
                            datacontext.Entry(obj).State = System.Data.Entity.EntityState.Modified;
                            datacontext.SaveChanges();
                            result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                logger.Info(ex.ToString());
            }

            return result;
        }
    }
}
