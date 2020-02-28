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
    public class JobLogRefresh
    {
        private  System.Timers.Timer aTimer;
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
                if (SessionManager.GetAccessToken().access_token != null && SessionManager.GetUserSession() != null)
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

                            if(fqRsp != null)
                            {
                                item.Result = JsonConvert.SerializeObject(fqRsp.ResultList);
                                datacontext.Entry(item).State = System.Data.Entity.EntityState.Modified;

                                if (fqRsp.ResultList.Count > 0)
                                {
                                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(fqRsp.ResultList.FirstOrDefault().ItemExecutionResult);
                                    if (result.success == true)
                                    {
                                        SellersPickedProduct obj = datacontext.SellersPickedProducts.Where(x => x.UserID == userid && x.ParentProductID == item.ProductID).FirstOrDefault();
                                        obj.AliExpressProductID = result.productId;
                                        datacontext.Entry(obj).State = System.Data.Entity.EntityState.Modified;
                                    }
                                }
                            }
                        }
                        datacontext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        public void RefreshPriceAliExpress()
        {
            try
            {
                if (SessionManager.GetAccessToken().access_token != null && SessionManager.GetUserSession() != null)
                {
                    List<Product> productList = new List<Product>();
                    using (DropshipDataEntities datacontext = new DropshipDataEntities())
                    {
                        productList = datacontext.Products.Where(x => x.ItemModifyWhen >= DateTime.Now.AddMinutes(-2)).ToList();
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
