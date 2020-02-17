using DropshipPlatform.BLL.Models;
using DropshipPlatform.Entity;
using FastJSON;
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
        public List<Product> GetAllProducts()
        {
            List<Product> products = new List<Product>();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    products = datacontext.Products.ToList();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return products;
        }
        public List<Product> GetParentProducts()
        {
            List<Product> products = new List<Product>();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    products = datacontext.Products.Where(x => x.ParentProductID == null).ToList();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return products;
        }

        public bool AddSellersPickedProducts(int[] products, int UserID)
        {
            bool result = true;
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    foreach (int product in products)
                    {
                        SellersPickedProduct obj = datacontext.SellersPickedProducts.Where(x => x.UserID == UserID && x.ParentProductID == product).FirstOrDefault();
                        if (obj == null)
                        {
                            obj = new SellersPickedProduct();
                            obj.UserID = UserID;
                            obj.ParentProductID = product;
                            obj.ItemCreatedBy = UserID;
                            obj.ItemCreatedWhen = DateTime.UtcNow;
                            datacontext.SellersPickedProducts.Add(obj);
                        }
                    }
                    datacontext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return result;
        }


        public List<ProductGroupModel> GetPickedProducts(int UserID)
        {
            List<ProductGroupModel> productGroupList = new List<ProductGroupModel>();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    List<SellersPickedProduct> dbSellerPickedProducts = datacontext.SellersPickedProducts.Where(x => x.UserID == UserID).GroupBy(x => x.ParentProductID).Select(x => x.FirstOrDefault()).ToList();
                    if (dbSellerPickedProducts.Count > 0)
                    {
                        foreach (SellersPickedProduct item in dbSellerPickedProducts)
                        {
                            List<Product> dbParentProducts = datacontext.Products.Where(x => x.ProductID == item.ParentProductID).ToList();
                            if (dbParentProducts.Count > 0)
                            {
                                foreach (Product dbParentProduct in dbParentProducts)
                                {
                                    ProductGroupModel productGroup = new ProductGroupModel();

                                    List<Product> childProducts = datacontext.Products.Where(x => x.ParentProductID.ToString() == dbParentProduct.OriginalProductID).ToList();
                                    productGroup.ParentProduct = GenerateProductViewModel(dbParentProduct);
                                    productGroup.ChildProductList = new List<ProductViewModel>();

                                    long parentInvetoryTotal = 0;
                                    long parentPriceTotal = 0;

                                    foreach (Product dbChildProduct in childProducts)
                                    {
                                        ProductViewModel childProductModel = GenerateProductViewModel(dbChildProduct);
                                        childProductModel = AddUpdatedValues(childProductModel);
                                        productGroup.ChildProductList.Add(childProductModel);
                                        parentInvetoryTotal = parentInvetoryTotal + Convert.ToInt32(childProductModel.Inventory);
                                        parentPriceTotal = parentPriceTotal + Convert.ToInt32(childProductModel.SellingPrice);
                                    }
                                    productGroup.ParentProduct.Inventory = parentInvetoryTotal.ToString();
                                    productGroup.ParentProduct.SellingPrice = parentPriceTotal;
                                    productGroupList.Add(productGroup);
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
                    PickedProduct dbPickedProduct = datacontext.PickedProducts.Where(m => m.SKU == productModel.OriginalProductID).FirstOrDefault();
                    if (dbPickedProduct != null)
                    {
                        productModel.UpdatedInvetory = dbPickedProduct.UpdatedInventory.Value;
                        productModel.UpdatedPrice = dbPickedProduct.UpdatedPrice.Value;
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



        public ProductViewModel GenerateProductViewModel(Product dbChildProduct)
        {
            ProductViewModel productModel = new ProductViewModel();
            productModel.ProductID = dbChildProduct.ProductID;
            productModel.Title = dbChildProduct.Title;
            productModel.CategoryID = dbChildProduct.CategoryID;
            productModel.SellingPrice = dbChildProduct.SellingPrice;
            productModel.SellingPriceCurrency = dbChildProduct.SellingPriceCurrency;
            productModel.OriginalProductID = dbChildProduct.OriginalProductID;
            productModel.Brand = dbChildProduct.Brand;
            productModel.Description = dbChildProduct.Description;
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

            if (productModel.CategoryID > 0)
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    Category dbCategoryModel = datacontext.Categories.Where(m => m.CategoryID == productModel.CategoryID).FirstOrDefault();
                    if (dbCategoryModel != null)
                    {
                        productModel.CategoryName = dbCategoryModel.Name;
                    }
                }
            }
            return productModel;
        }

        public bool UpdatePickedProduct(List<UpdateProductModel> model)
        {
            bool result = false;
            try
            {
                if (model.Count > 0)
                {
                    using (DropshipDataEntities datacontext = new DropshipDataEntities())
                    {
                        foreach (UpdateProductModel item in model)
                        {
                            Product dbProduct = datacontext.Products.Where(m => m.OriginalProductID == item.SKU).FirstOrDefault();
                            if (dbProduct != null)
                            {
                                PickedProduct dbPickedProduct = datacontext.PickedProducts.Where(p => p.SKU == item.SKU).FirstOrDefault();
                                if (dbPickedProduct != null)
                                {
                                    long existingInventory = dbPickedProduct.UpdatedInventory.Value > 0 ? dbPickedProduct.UpdatedInventory.Value : 0;

                                    dbPickedProduct.UpdatedInventory = Convert.ToInt32(item.PickedInventory);
                                    dbPickedProduct.UpdatedPrice = Convert.ToInt32(item.UpdatedPrice);
                                    dbPickedProduct.UpdatedBy = 1;
                                    dbPickedProduct.UpdatedDate = DateTime.Now;
                                    datacontext.SaveChanges();
                                    dbProduct.Inventory = Convert.ToString(Convert.ToInt32(dbProduct.Inventory)- (dbPickedProduct.UpdatedInventory - existingInventory));
                                    datacontext.SaveChanges();
                                    result = true;
                                }
                                else
                                {
                                    PickedProduct dbPickProduct = new PickedProduct();
                                    dbPickedProduct.ProductId = dbProduct.ProductID;
                                    dbPickedProduct.SKU = dbProduct.OriginalProductID;
                                    dbPickedProduct.UpdatedPrice = Convert.ToInt16(item.UpdatedPrice);
                                    dbPickedProduct.UpdatedInventory = Convert.ToInt16(item.PickedInventory);
                                    dbPickedProduct.CreatedBy = 1;
                                    dbPickedProduct.CreatedDate = DateTime.Now;
                                    datacontext.PickedProducts.Add(dbPickedProduct);
                                    datacontext.SaveChanges();
                                    long remainInventory = Convert.ToInt32(dbProduct.Inventory) - dbPickedProduct.UpdatedInventory.Value;
                                    if (remainInventory > 0)
                                    {
                                        dbProduct.Inventory = Convert.ToString(remainInventory);
                                    }
                                    datacontext.SaveChanges();
                                }
                                result = true;
                                //string productSKU = SyncWithAliExpress(dbProduct);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public string SyncWithAliExpress(Product dbProduct)
        {
            string ProductID = String.Empty;
            try
            {
                //AliExpressPostProductModel model = new AliExpressPostProductModel();
                //model.category_id = dbProduct.CategoryID.Value;
                //model.locale = "es_EN";
                //model.product_units_type = "100000015";
                //model.image_url_list = new List<string>() { "https://upload.wikimedia.org/wikipedia/commons/b/ba/E-SENS_architecture.jpg" };
                //model.inventory_deduction_strategy = "payment_success_deduct";

                //model.title_multi_language_list = new List<TitleMultiLanguageList>();
                //TitleMultiLanguageList _titleMultiLanguageList = new TitleMultiLanguageList();
                //_titleMultiLanguageList.locale = "es_EN";
                //_titleMultiLanguageList.title = dbProduct.Title;
                //model.title_multi_language_list.Add(_titleMultiLanguageList);

                //model.sku_info_list = new List<SkuInfoList>();
                //SkuInfoList sku_info = new SkuInfoList();
                //sku_info.inventory = Convert.ToInt32(dbProduct.Inventory);
                //sku_info.sku_code =dbProduct.OriginalProductID;
                //sku_info.price = Convert.ToInt32(dbProduct.SellingPrice);


                //model.category_attributes = new CategoryAttributes();

                //model.category_attributes.BrandName = new BrandName();
                //model.category_attributes.BrandName.value = dbProduct.Brand;
                //model.category_attributes.Color = new Color();
                //model.category_attributes.Color.value = dbProduct.Color;
                //model.category_attributes.Size = new Size();
                //model.category_attributes.Size.value = dbProduct.Size;
                //model.category_attributes.ManufacturerName = new ManufacturerName();
                //model.category_attributes.ManufacturerName.value = dbProduct.ManufacturerName;

                //string json = Newtonsoft.Json.JsonConvert.SerializeObject(model);

                //ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);
                //AliexpressSolutionSchemaProductInstancePostRequest req = new AliexpressSolutionSchemaProductInstancePostRequest();
                //req.ProductInstanceRequest = "{\"category_id\":348,\"title_multi_language_list\":[{\"locale\":\"es_ES\",\"title\":\"atestproducttesttest001\"}],\"description_multi_language_list\":[{\"locale\":\"es_ES\",\"module_list\":[{\"type\":\"html\",\"html\":{\"content\":\"unotesttestdescription002\"}}]}],\"locale\":\"es_ES\",\"product_units_type\":\"100000015\",\"image_url_list\":[\"https://upload.wikimedia.org/wikipedia/commons/b/ba/E-SENS_architecture.jpg\"],\"category_attributes\":{\"BrandName\":{\"value\":\"200010868\"},\"ShirtsType\":{\"value\":\"200001208\"},\"Material\":{\"value\":[\"567\"]},\"SleeveLength(cm)\":{\"value\":\"200001500\"}},\"sku_info_list\":[{\"sku_code\":\"WEO19293829123\",\"inventory\":3,\"price\":9900,\"discount_price\":9800,\"sku_attributes\":{\"Size\":{\"alias\":\"Uni\",\"value\":\"200003528\"}}}],\"inventory_deduction_strategy\":\"payment_success_deduct\",\"package_weight\":1.5,\"package_length\":10,\"package_height\":20,\"package_width\":30,\"shipping_preparation_time\":3,\"shipping_template_id\":\"714844311\",\"service_template_id\":\"0\"}";
                //req.ProductInstanceRequest = json;

                //ITopClient client = new DefaultTopClient(url, appkey, secret);
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);
                AliexpressSolutionSchemaProductInstancePostRequest req = new AliexpressSolutionSchemaProductInstancePostRequest();
                req.ProductInstanceRequest = "{\"category_id\":348,\"title_multi_language_list\":[{\"locale\":\"es_ES\",\"title\":\"atestproducttesttest001\"}],\"description_multi_language_list\":[{\"locale\":\"es_ES\",\"module_list\":[{\"type\":\"html\",\"html\":{\"content\":\"unotesttestdescription002\"}}]}],\"locale\":\"es_ES\",\"product_units_type\":\"100000015\",\"image_url_list\":[\"https://upload.wikimedia.org/wikipedia/commons/b/ba/E-SENS_architecture.jpg\"],\"category_attributes\":{\"BrandName\":{\"value\":\"200010868\"},\"ShirtsType\":{\"value\":\"200001208\"},\"Material\":{\"value\":[\"567\"]},\"SleeveLength(cm)\":{\"value\":\"200001500\"}},\"sku_info_list\":[{\"sku_code\":\"WEO19293829123\",\"inventory\":3,\"price\":9900,\"discount_price\":9800,\"sku_attributes\":{\"Size\":{\"alias\":\"Uni\",\"value\":\"200003528\"}}}],\"inventory_deduction_strategy\":\"payment_success_deduct\",\"package_weight\":1.5,\"package_length\":10,\"package_height\":20,\"package_width\":30,\"shipping_preparation_time\":3,\"shipping_template_id\":\"714844311\",\"service_template_id\":\"0\"}";

                if (SessionManager.GetAccessToken().access_token != null)
                {
                    AliexpressSolutionSchemaProductInstancePostResponse rsp = client.Execute(req, SessionManager.GetAccessToken().access_token);
                    Console.WriteLine(rsp.Body);
                }
                
            }
            catch (Exception ex)
            {
                logger.Info(ex.ToString());
            }
            return ProductID;
        }
    }
}
