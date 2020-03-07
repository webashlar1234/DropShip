using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropshipPlatform.BLL.Models;
using DropshipPlatform.Entity;
using Newtonsoft.Json;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace DropshipPlatform.BLL.Services
{
    public class OrderService
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ResultData getAllOrders()
        {
            ResultData orders =new ResultData();
            string result = String.Empty;
            try
            {
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);
                AliexpressSolutionOrderGetRequest req = new AliexpressSolutionOrderGetRequest();

                AliexpressSolutionOrderGetRequest.OrderQueryDomain obj1 = new AliexpressSolutionOrderGetRequest.OrderQueryDomain();
                var todayDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                obj1.CreateDateEnd = todayDate;
                obj1.CreateDateStart = "2020-03-01 00:00:00";
                obj1.ModifiedDateStart = "2020-03-01 00:00:00";
                obj1.OrderStatusList = new List<string> { "SELLER_PART_SEND_GOODS", "PLACE_ORDER_SUCCESS", "IN_CANCEL", "WAIT_SELLER_SEND_GOODS", "WAIT_BUYER_ACCEPT_GOODS", "FUND_PROCESSING" , "IN_ISSUE", "IN_FROZEN", "WAIT_SELLER_EXAMINE_MONEY", "RISK_CONTROL", "FINISH" };
                obj1.BuyerLoginId = "edacan0107@aol.com";
                obj1.PageSize = 20L;
                obj1.ModifiedDateEnd = todayDate; 
                obj1.CurrentPage = 1L;
                obj1.OrderStatus = "SELLER_PART_SEND_GOODS";
                req.Param0_ = obj1;
                Top.Api.Response.AliexpressSolutionOrderGetResponse rsp = client.Execute(req, SessionManager.GetAccessToken().access_token);
                result = JsonConvert.SerializeObject(rsp.Result);
                orders = JsonConvert.DeserializeObject<ResultData>(result);

            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return orders;

        }

        public List<Product> GetProductById(string Id)
        {
            List<Product> products = new List<Product>();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    products = datacontext.Products.Where(x=>x.OriginalProductID == Id).ToList();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return products;
        }

        public bool trackingOrder(OrderData orderData)
        {
            bool result = true;

            try
            {
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);

                AliexpressLogisticsRedefiningGetonlinelogisticsservicelistbyorderidRequest req1 = new AliexpressLogisticsRedefiningGetonlinelogisticsservicelistbyorderidRequest();
                req1.GoodsWidth = 1L;
                req1.GoodsHeight = 1L;
                req1.GoodsWeight = "1.5";
                req1.GoodsLength = 1L;
                req1.OrderId = Convert.ToInt64(orderData.AliExpressOrderNumber);
                AliexpressLogisticsRedefiningGetonlinelogisticsservicelistbyorderidResponse rsp1 = client.Execute(req1, SessionManager.GetAccessToken().access_token);
                var LogisticsNo = string.Empty;
                if (rsp1.ResultList.Count > 0)
                {
                    LogisticsNo = rsp1.ResultList[0].LogisticsServiceId;
                }

                AliexpressSolutionOrderInfoGetRequest req2 = new AliexpressSolutionOrderInfoGetRequest();
                AliexpressSolutionOrderInfoGetRequest.OrderDetailQueryDomain obj1 = new AliexpressSolutionOrderInfoGetRequest.OrderDetailQueryDomain();
                //obj1.ExtInfoBitFlag = 11111L;
                obj1.OrderId = Convert.ToInt64(orderData.AliExpressOrderNumber);
                req2.Param1_ = obj1;
                AliexpressSolutionOrderInfoGetResponse rsp2 = client.Execute(req2, SessionManager.GetAccessToken().access_token);

                AliexpressLogisticsRedefiningListlogisticsserviceRequest req3 = new AliexpressLogisticsRedefiningListlogisticsserviceRequest();
                AliexpressLogisticsRedefiningListlogisticsserviceResponse rsp3 = client.Execute(req3, SessionManager.GetAccessToken().access_token);

                AliexpressSolutionOrderFulfillRequest req = new AliexpressSolutionOrderFulfillRequest();
                req.ServiceName = orderData.LogisticType;
                req.TrackingWebsite = orderData.OrignalProductLink;
                req.OutRef = orderData.AliExpressOrderNumber;
                req.SendType = "part";
                req.Description = "memo";
                //req.LogisticsNo = "AEFP123456RU2";
                req.LogisticsNo = LogisticsNo;
                AliexpressSolutionOrderFulfillResponse rsp = client.Execute(req, SessionManager.GetAccessToken().access_token);
                var data = rsp;
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
