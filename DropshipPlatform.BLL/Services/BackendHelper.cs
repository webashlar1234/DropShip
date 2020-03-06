using DropshipPlatform.BLL.Models;
using DropshipPlatform.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace DropshipPlatform.BLL.Services
{
    public class BackendHelper
    {
        private System.Timers.Timer aTimer;
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //public void Init()
        //{
        //    if(aTimer == null)
        //    {
        //        aTimer = new System.Timers.Timer(10000);
        //        aTimer.Elapsed += RefreshAliExpressJobLog;
        //        aTimer.AutoReset = true;
        //        aTimer.Enabled = true;
        //    }
        //}

        public void RefreshAliExpressJobLog()
        {
            try
            {
                List<AliExpressJobLog> jobLogList = new List<AliExpressJobLog>();
                int userid = SessionManager.GetUserSession().UserID;
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    jobLogList = datacontext.AliExpressJobLogs.Where(x => x.UserID == userid && x.Result == "null" || x.Result == null || x.Result == string.Empty).ToList();

                    foreach (AliExpressJobLog item in jobLogList)
                    {
                        ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret, "json");

                        AliexpressSolutionFeedQueryRequest fqReq = new AliexpressSolutionFeedQueryRequest();
                        fqReq.JobId = item.JobId;
                        //fqReq.JobId = 200000020380394453;

                        AliexpressSolutionFeedQueryResponse fqRsp = client.Execute(fqReq, SessionManager.GetAccessToken().access_token);

                        if (fqRsp != null)
                        {
                            item.Result = JsonConvert.SerializeObject(fqRsp.ResultList);
                            datacontext.Entry(item).State = System.Data.Entity.EntityState.Modified;

                            if (fqRsp.ResultList.Count > 0)
                            {
                                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(fqRsp.ResultList.FirstOrDefault().ItemExecutionResult);

                                string productOriginalId = item.ProductID.ToString();
                                Product prodObj = datacontext.Products.Where(x => x.OriginalProductID == productOriginalId).FirstOrDefault();
                                if (prodObj != null)
                                {
                                    SellersPickedProduct obj = datacontext.SellersPickedProducts.Where(x => x.UserID == userid && x.ParentProductID == prodObj.ProductID).FirstOrDefault();
                                    if (obj != null)
                                    {
                                        if (result.success == true)
                                        {
                                            obj.AliExpressProductID = result.productId;
                                            datacontext.Entry(obj).State = System.Data.Entity.EntityState.Modified;
                                        }
                                        else
                                        {
                                            List<SellerPickedProductSKU> skulist = datacontext.SellerPickedProductSKUs.Where(x => x.SellerPickedId == obj.SellersPickedID).ToList();
                                            foreach (var sku in skulist)
                                            {
                                                datacontext.SellerPickedProductSKUs.Remove(sku);
                                            }
                                            datacontext.SellersPickedProducts.Remove(obj);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    datacontext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        public void RefreshAliExpressInventory1()
        {
            try
            {
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret, "json");
                AliexpressSolutionBatchProductInventoryUpdateRequest req = new AliexpressSolutionBatchProductInventoryUpdateRequest();

                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    List<User> users = datacontext.Users.Where(x => x.IsActive == true && x.AliExpressSellerID != null).ToList();

                    foreach (User user in users)
                    {
                        List<AliexpressSolutionBatchProductInventoryUpdateRequest.SynchronizeProductRequestDtoDomain> productList = new List<AliexpressSolutionBatchProductInventoryUpdateRequest.SynchronizeProductRequestDtoDomain>();
                        List<SellersPickedProduct> sellerProducts = datacontext.SellersPickedProducts.Where(x => x.UserID == user.UserID && x.AliExpressProductID != null).ToList();
                        AliExpressAccessToken token = Newtonsoft.Json.JsonConvert.DeserializeObject<AliExpressAccessToken>(user.AliExpressAccessToken);

                        foreach (SellersPickedProduct item in sellerProducts)
                        {
                            AliexpressSolutionBatchProductInventoryUpdateRequest.SynchronizeProductRequestDtoDomain productObj = new AliexpressSolutionBatchProductInventoryUpdateRequest.SynchronizeProductRequestDtoDomain();
                            productObj.ProductId = Int64.Parse(item.AliExpressProductID);
                            productObj.MultipleSkuUpdateList = (from pp in datacontext.SellerPickedProductSKUs.Where(x => x.SellerPickedId == item.SellersPickedID)
                                                                from p in datacontext.Products.Where(x => x.OriginalProductID == pp.SKUCode)
                                                                select new { OriginalProductID = p.OriginalProductID, Inventory = p.Inventory }
                                                                ).ToList().Select(x => new AliexpressSolutionBatchProductInventoryUpdateRequest.SynchronizeSkuRequestDtoDomain
                                                                {
                                                                    SkuCode = x.OriginalProductID,
                                                                    Inventory = x.Inventory != null ? Int64.Parse(x.Inventory) : 0
                                                                }).ToList();

                            productList.Add(productObj);
                        }

                        for (int i = 0; i < productList.Count; i = i + 1)
                        {
                            req.MutipleProductUpdateList_ = productList.Skip(i * 20).Take(20).ToList();
                            AliexpressSolutionBatchProductInventoryUpdateResponse rsp = client.Execute(req, token.access_token);
                            Console.WriteLine(rsp.Body);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        public string RefreshAliExpressInventory()
        {
            string result = String.Empty;
            try
            {
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret, "json");
                AliexpressSolutionFeedSubmitRequest req = new AliexpressSolutionFeedSubmitRequest();
                req.OperationType = "PRODUCT_STOCKS_UPDATE";
                List<AliexpressSolutionFeedSubmitRequest.SingleItemRequestDtoDomain> list2 = new List<AliexpressSolutionFeedSubmitRequest.SingleItemRequestDtoDomain>();
                AliexpressSolutionFeedSubmitRequest.SingleItemRequestDtoDomain obj3 = new AliexpressSolutionFeedSubmitRequest.SingleItemRequestDtoDomain();
                List<string> skus = new List<string>();
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    List<User> users = datacontext.Users.Where(x => x.IsActive == true && x.AliExpressSellerID != null).ToList();
                    foreach (User user in users)
                    {
                        List<SellersPickedProduct> sellerProducts = datacontext.SellersPickedProducts.Where(x => x.UserID == user.UserID && x.AliExpressProductID != null).ToList();
                        foreach (SellersPickedProduct scproductModel in sellerProducts)
                        {
                            scproductModel productObj = new scproductModel();
                            productObj.SKUModels = (from pp in datacontext.SellerPickedProductSKUs.Where(x => x.SellerPickedId == scproductModel.SellersPickedID)
                                                    from p in datacontext.Products.Where(x => x.OriginalProductID == pp.SKUCode)
                                                    select new { OriginalProductID = p.OriginalProductID, Inventory = p.Inventory }
                                                                ).ToList().Select(x => new ProductSKUModel
                                                                {
                                                                    skuCode = x.OriginalProductID,
                                                                    inventory = x.Inventory != null ? Int32.Parse(x.Inventory) : 0
                                                                }).ToList();
                            foreach (ProductSKUModel productSKUModel in productObj.SKUModels)
                            {
                                skus.Add("{\"sku_code\": \"" + productSKUModel.skuCode + "\",\"inventory\": " + productSKUModel.inventory + "}");
                            }
                            obj3.ItemContentId = Guid.NewGuid().ToString();
                            obj3.ItemContent = "{\"aliexpress_product_id\":" + scproductModel.AliExpressProductID + ",\"multiple_sku_update_list\":[" + string.Join(",", skus.ToArray()) + "]}";

                            list2.Add(obj3);
                            req.ItemList_ = list2;


                            if (SessionManager.GetAccessToken().access_token != null)
                            {
                                AliexpressSolutionFeedSubmitResponse rsp = client.Execute(req, SessionManager.GetAccessToken().access_token);
                                AliexpressSolutionFeedQueryRequest fqReq = new AliexpressSolutionFeedQueryRequest();
                                fqReq.JobId = rsp.JobId;
                                AliexpressSolutionFeedQueryResponse fqRsp = client.Execute(fqReq, SessionManager.GetAccessToken().access_token);
                                result = JsonConvert.SerializeObject(fqRsp.ResultList);
                                AliExpressJobLogService _aliExpressJobLogService = new AliExpressJobLogService();
                                _aliExpressJobLogService.AddAliExpressJobLog(new AliExpressJobLog()
                                {
                                    JobId = rsp.JobId,
                                    ContentId = obj3.ItemContentId,
                                    SuccessItemCount = fqRsp.SuccessItemCount,
                                    UserID = user.UserID,
                                    ProductID = scproductModel.ParentProductID,
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
                }
            }
            catch (Exception ex)
            {
                logger.Info(ex.ToString());
            }
            return result;
        }
    }
}
