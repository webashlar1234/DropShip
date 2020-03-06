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
                                            List<SellerPickedProductSKU> skulist =datacontext.SellerPickedProductSKUs.Where(x => x.SellerPickedId == obj.SellersPickedID).ToList();
                                            foreach(var sku in skulist)
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

        public void RefreshAliExpressInventory()
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

                        req.MutipleProductUpdateList_ = productList;
                        AliexpressSolutionBatchProductInventoryUpdateResponse rsp = client.Execute(req, token.access_token);
                        Console.WriteLine(rsp.Body);
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }
    }
}
