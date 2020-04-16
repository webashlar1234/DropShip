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
                List<aliexpressjoblog> jobLogList = new List<aliexpressjoblog>();
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    jobLogList = datacontext.aliexpressjoblogs.Where(x => x.Result == "null" || x.Result == null || x.Result == string.Empty).ToList();

                    foreach (aliexpressjoblog item in jobLogList)
                    {
                        user user = datacontext.users.Where(x => x.UserID == item.UserID).FirstOrDefault();
                        if(user != null && !string.IsNullOrEmpty(user.AliExpressAccessToken))
                        {
                            ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret, "json");

                            AliexpressSolutionFeedQueryRequest fqReq = new AliexpressSolutionFeedQueryRequest();
                            fqReq.JobId = item.JobId;
                            //fqReq.JobId = 200000020380394453;

                            AliexpressSolutionFeedQueryResponse fqRsp = client.Execute(fqReq, StaticValues.getAccessTokenObjFromStr(user.AliExpressAccessToken));

                            if (fqRsp != null)
                            {
                                item.Result = JsonConvert.SerializeObject(fqRsp.ResultList);
                                datacontext.Entry(item).State = System.Data.Entity.EntityState.Modified;

                                if (fqRsp.ResultList != null)
                                {
                                    if (fqRsp.ResultList.Count > 0)
                                    {
                                        var result = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(fqRsp.ResultList.FirstOrDefault().ItemExecutionResult);

                                        int productOriginalId = int.Parse(item.ProductID);
                                        product prodObj = datacontext.products.Where(x => x.ProductID == productOriginalId).FirstOrDefault();
                                        if (prodObj != null)
                                        {
                                            sellerspickedproduct obj = datacontext.sellerspickedproducts.Where(x => x.UserID == user.UserID && x.ParentProductID == prodObj.ProductID).FirstOrDefault();
                                            if (obj != null)
                                            {
                                                if (result.success == true)
                                                {
                                                    obj.AliExpressProductID = result.productId;
                                                    datacontext.Entry(obj).State = System.Data.Entity.EntityState.Modified;
                                                }
                                                else
                                                {
                                                    List<sellerpickedproductsku> skulist = datacontext.sellerpickedproductskus.Where(x => x.SellerPickedId == obj.SellersPickedID).ToList();
                                                    foreach (var sku in skulist)
                                                    {
                                                        datacontext.sellerpickedproductskus.Remove(sku);
                                                    }
                                                    datacontext.sellerspickedproducts.Remove(obj);
                                                }
                                            }
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
                    List<user> users = datacontext.users.Where(x => x.IsActive == true && x.AliExpressSellerID != null).ToList();

                    foreach (user user in users)
                    {
                        List<AliexpressSolutionBatchProductInventoryUpdateRequest.SynchronizeProductRequestDtoDomain> productList = new List<AliexpressSolutionBatchProductInventoryUpdateRequest.SynchronizeProductRequestDtoDomain>();
                        List<sellerspickedproduct> sellerProducts = datacontext.sellerspickedproducts.Where(x => x.UserID == user.UserID && x.AliExpressProductID != null).ToList();
                        AliExpressAccessToken token = Newtonsoft.Json.JsonConvert.DeserializeObject<AliExpressAccessToken>(user.AliExpressAccessToken);

                        foreach (sellerspickedproduct item in sellerProducts)
                        {
                            AliexpressSolutionBatchProductInventoryUpdateRequest.SynchronizeProductRequestDtoDomain productObj = new AliexpressSolutionBatchProductInventoryUpdateRequest.SynchronizeProductRequestDtoDomain();
                            productObj.ProductId = Int64.Parse(item.AliExpressProductID);
                            productObj.MultipleSkuUpdateList = (from pp in datacontext.sellerpickedproductskus.Where(x => x.SellerPickedId == item.SellersPickedID)
                                                                from p in datacontext.products.Where(x => x.OriginalProductID == pp.SKUCode)
                                                                select new { OriginalProductID = p.OriginalProductID, Inventory = p.Inventory }
                                                                ).ToList().Select(x => new AliexpressSolutionBatchProductInventoryUpdateRequest.SynchronizeSkuRequestDtoDomain
                                                                {
                                                                    SkuCode = x.OriginalProductID,
                                                                    Inventory = x.Inventory ?? 0
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
                
                
                List<string> skus = new List<string>();
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    List<user> users = datacontext.users.Where(x => x.IsActive == true && x.AliExpressSellerID != null).ToList();
                    foreach (user user in users)
                    {
                        List<AliexpressSolutionFeedSubmitRequest.SingleItemRequestDtoDomain> list2 = new List<AliexpressSolutionFeedSubmitRequest.SingleItemRequestDtoDomain>();
                        List<sellerspickedproduct> sellerProducts = datacontext.sellerspickedproducts.Where(x => x.UserID == user.UserID && x.AliExpressProductID != null).ToList();
                        foreach (sellerspickedproduct scproductModel in sellerProducts)
                        {
                            AliexpressSolutionFeedSubmitRequest.SingleItemRequestDtoDomain obj3 = new AliexpressSolutionFeedSubmitRequest.SingleItemRequestDtoDomain();
                            scproductModel productObj = new scproductModel();
                            productObj.SKUModels = (from pp in datacontext.sellerpickedproductskus.Where(x => x.SellerPickedId == scproductModel.SellersPickedID)
                                                    from p in datacontext.products.Where(x => x.ProductID == pp.ProductId)
                                                    select new { skuCode = p.SkuID, Inventory = p.Inventory }
                                                                ).ToList().Select(x => new ProductSKUModel
                                                                {
                                                                    skuCode = x.skuCode,
                                                                    inventory = x.Inventory ?? 0
                                                                }).ToList();
                            if(productObj.SKUModels.Count > 0)
                            {
                                foreach (ProductSKUModel productSKUModel in productObj.SKUModels)
                                {
                                    skus.Add("{\"sku_code\": \"" + productSKUModel.skuCode + "\",\"inventory\": " + productSKUModel.inventory + "}");
                                }
                            }
                            else
                            {
                                product productOrg = datacontext.products.Where(x => x.ProductID == scproductModel.ParentProductID).FirstOrDefault();
                                skus.Add("{\"sku_code\": \"" + productOrg.ProductID + "\",\"inventory\": " + productOrg.Inventory + "}");
                            }
                            
                            obj3.ItemContentId = Guid.NewGuid().ToString();
                            obj3.ItemContent = "{\"aliexpress_product_id\":" + scproductModel.AliExpressProductID + ",\"multiple_sku_update_list\":[" + string.Join(",", skus.ToArray()) + "]}";

                            list2.Add(obj3);
                        }
                        req.ItemList_ = list2;
                        string access_token = StaticValues.getAccessTokenObjFromStr(user.AliExpressAccessToken);
                        if (!string.IsNullOrEmpty(access_token))
                        {
                            AliexpressSolutionFeedSubmitResponse rsp = client.Execute(req, access_token);
                            AliexpressSolutionFeedQueryRequest fqReq = new AliexpressSolutionFeedQueryRequest();
                            fqReq.JobId = rsp.JobId;
                            AliexpressSolutionFeedQueryResponse fqRsp = client.Execute(fqReq, access_token);
                            result = JsonConvert.SerializeObject(fqRsp.ResultList);
                            AliExpressJobLogService _aliExpressJobLogService = new AliExpressJobLogService();
                            //_aliExpressJobLogService.AddAliExpressJobLog(new AliExpressJobLog()
                            //{
                            //    JobId = rsp.JobId,
                            //    ContentId = obj3.ItemContentId,
                            //    SuccessItemCount = fqRsp.SuccessItemCount,
                            //    UserID = user.UserID,
                            //    ProductID = null,
                            //    ProductDetails = Newtonsoft.Json.JsonConvert.SerializeObject(list2),
                            //    Result = result
                            //});

                            List<AliexpressSolutionFeedResponseModel> solutionResponse = JsonConvert.DeserializeObject<List<AliexpressSolutionFeedResponseModel>>(result);
                            if (solutionResponse != null && solutionResponse.Count > 0)
                            {
                                ItemExecutionResultModel itemModel = JsonConvert.DeserializeObject<ItemExecutionResultModel>(solutionResponse[0].ItemExecutionResult);
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

        public void RefreshAliExpressOrders()
        {
            OrderService obj = new OrderService();
            obj.getAllOrders();
        }
    }
}
