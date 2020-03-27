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
            ResultData orders = new ResultData();
            string result = String.Empty;
            try
            {
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);
                AliexpressSolutionOrderGetRequest req = new AliexpressSolutionOrderGetRequest();

                AliexpressSolutionOrderGetRequest.OrderQueryDomain obj1 = new AliexpressSolutionOrderGetRequest.OrderQueryDomain();

                //var todayDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //obj1.CreateDateEnd = DateTime.Now.AddMinutes(-5).ToString("yyyy-MM-dd HH:mm:ss");
                //obj1.CreateDateStart = todayDate;
                //obj1.ModifiedDateStart = todayDate;

                var todayDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                obj1.CreateDateEnd = todayDate;
                obj1.CreateDateStart = "2020-03-01 00:00:00";
                obj1.ModifiedDateStart = "2020-03-01 00:00:00";

                obj1.OrderStatusList = new List<string> { "SELLER_PART_SEND_GOODS", "PLACE_ORDER_SUCCESS", "IN_CANCEL", "WAIT_SELLER_SEND_GOODS", "WAIT_BUYER_ACCEPT_GOODS", "FUND_PROCESSING", "IN_ISSUE", "IN_FROZEN", "WAIT_SELLER_EXAMINE_MONEY", "RISK_CONTROL", "FINISH" };
                obj1.BuyerLoginId = "edacan0107@aol.com";
                obj1.PageSize = 20L;
                //obj1.ModifiedDateEnd = DateTime.Now.AddMinutes(-5).ToString("yyyy-MM-dd HH:mm:ss");
                obj1.ModifiedDateEnd = todayDate;
                obj1.CurrentPage = 1L;
                obj1.OrderStatus = "SELLER_PART_SEND_GOODS";
                req.Param0_ = obj1;
                Top.Api.Response.AliexpressSolutionOrderGetResponse rsp = client.Execute(req, SessionManager.GetAccessToken().access_token);
                result = JsonConvert.SerializeObject(rsp.Result);
                orders = JsonConvert.DeserializeObject<ResultData>(result);
                InsertOrUpdateOrderData(orders);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return orders;

        }

        public sellerspickedproduct GetProductById(string Id)
        {
            sellerspickedproduct products = new sellerspickedproduct();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    products = datacontext.sellerspickedproducts.Where(x => x.AliExpressProductID == Id).FirstOrDefault();
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

        public void InsertOrUpdateOrderData(ResultData orders)
        {
            try
            {
                if (orders.TargetList != null && orders.TargetList.Count > 0)
                {
                    using (DropshipDataEntities datacontext = new DropshipDataEntities())
                    {
                        foreach (var item in orders.TargetList)
                        {
                            aliexpressorder AliExpressOrderData = new aliexpressorder();
                            aliexpressorderitem AliExpressOrderItemData = new aliexpressorderitem();
                            order OrderData = new order();

                            var AliiExpressOrderId = datacontext.aliexpressorders.Where(x => x.AliExpressOrderID == item.OrderId).Any();

                            if (!AliiExpressOrderId)
                            {
                                AliExpressOrderData.AliExpressOrderID = item.OrderId;
                                AliExpressOrderData.BuyerLoginId = item.BuyerLoginId;
                                AliExpressOrderData.AliExpressSellerID = item.SellerLoginId;
                                AliExpressOrderData.DeliveryCountry = null;
                                AliExpressOrderData.PaymentStatus = null;
                                AliExpressOrderData.TrackingNo = null;
                                AliExpressOrderData.ShippingWeight = null;
                                AliExpressOrderData.OrderAmount = null;
                                AliExpressOrderData.CurrencyCode = null;
                                AliExpressOrderData.OrderStatus = item.OrderStatus;
                                AliExpressOrderData.NoOfOrderItems = null;
                                AliExpressOrderData.ItemCreatedWhen = DateTime.UtcNow;
                                AliExpressOrderData.ItemModifyWhen = DateTime.UtcNow;
                                AliExpressOrderData.AliExpressOrderCreatedTime = item.GmtCreate;
                                AliExpressOrderData.AliExpressOrderUpdatedTime = item.GmtUpdate;
                                datacontext.aliexpressorders.Add(AliExpressOrderData);

                                foreach (var product in item.ProductList)
                                {
                                    AliExpressOrderItemData.AliExpressProductID = product.ProductId.ToString();
                                    sellerspickedproduct productData = GetProductById(AliExpressOrderItemData.AliExpressProductID);

                                    if (productData != null)
                                    {
                                        AliExpressOrderItemData.ProductID = Convert.ToInt64(productData.AliExpressProductID);
                                    }
                                    AliExpressOrderItemData.ProductName = product.ProductName;
                                    AliExpressOrderItemData.Price = product.TotalProductAmount.Amount;
                                    AliExpressOrderItemData.CurrencyCode = product.TotalProductAmount.CurrencyCode;
                                    AliExpressOrderItemData.ItemCreatedWhen = DateTime.UtcNow;
                                    AliExpressOrderItemData.ItemModifyWhen = DateTime.UtcNow;
                                    AliExpressOrderItemData.AliExpressOrderId = item.OrderId;
                                    AliExpressOrderItemData.AliExpressProductOrderId = null;
                                    AliExpressOrderItemData.SCOrderId = null;
                                    datacontext.aliexpressorderitems.Add(AliExpressOrderItemData);
                                }
                                //var OrderId = datacontext.Orders.Where(x => x.OrderID == item.OrderId).Any();
                                OrderData.AliExpressOrderID = item.OrderId.ToString();
                                OrderData.AliExpressProductId = item.ProductList[0].ProductId.ToString();
                                OrderData.OrignalProductId = item.SellerLoginId;
                                OrderData.OrignalProductLink = null;
                                OrderData.ProductTitle = null;
                                OrderData.DeliveryCountry = item.ProductList[0].DeliveryTime;
                                OrderData.ShippingWeight = null;
                                OrderData.OrderAmount = item.ProductList[0].TotalProductAmount.Amount; ;
                                OrderData.OrderStatus = item.OrderStatus;
                                OrderData.PaymentStatus = null;
                                OrderData.SellerID = item.SellerLoginId;
                                OrderData.SellerEmail = null;
                                OrderData.productExist = 1;
                                OrderData.LogisticName = item.ProductList[0].LogisticsServiceName;
                                OrderData.LogisticType = item.ProductList[0].LogisticsType;
                                OrderData.ItemCreatedBy = null;
                                OrderData.ItemCreatedWhen = DateTime.UtcNow;
                                OrderData.ItemModifyBy = null;
                                OrderData.ItemModifyWhen = DateTime.UtcNow;
                                datacontext.orders.Add(OrderData);
                            }
                            else
                            {
                                aliexpressorder AliExpressOrderDataList = new aliexpressorder();
                                aliexpressorderitem AliExpressOrderItemDataList = new aliexpressorderitem();

                                AliExpressOrderDataList = datacontext.aliexpressorders.Where(x => x.AliExpressOrderID == item.OrderId).FirstOrDefault();

                                AliExpressOrderDataList.AliExpressOrderID = item.OrderId;
                                AliExpressOrderDataList.BuyerLoginId = item.BuyerLoginId;
                                AliExpressOrderDataList.AliExpressSellerID = item.SellerLoginId;
                                AliExpressOrderDataList.DeliveryCountry = null;
                                AliExpressOrderDataList.PaymentStatus = null;
                                AliExpressOrderDataList.TrackingNo = null;
                                AliExpressOrderDataList.ShippingWeight = null;
                                AliExpressOrderDataList.OrderAmount = null;
                                AliExpressOrderDataList.CurrencyCode = null;
                                AliExpressOrderDataList.OrderStatus = item.OrderStatus;
                                AliExpressOrderDataList.NoOfOrderItems = null;
                                AliExpressOrderDataList.ItemModifyWhen = DateTime.UtcNow;
                                AliExpressOrderDataList.AliExpressOrderUpdatedTime = item.GmtUpdate;

                                foreach (var product in item.ProductList)
                                {
                                    AliExpressOrderItemDataList.AliExpressProductID = product.ProductId.ToString();
                                    sellerspickedproduct productData = GetProductById(AliExpressOrderItemDataList.AliExpressProductID);

                                    if (productData != null)
                                    {
                                        AliExpressOrderItemDataList.ProductID = Convert.ToInt64(productData.AliExpressProductID);
                                    }
                                    AliExpressOrderItemDataList.ProductName = product.ProductName;
                                    AliExpressOrderItemDataList.ProductName = product.ProductName;
                                    AliExpressOrderItemDataList.Price = product.TotalProductAmount.Amount;
                                    AliExpressOrderItemDataList.CurrencyCode = product.TotalProductAmount.CurrencyCode;
                                    AliExpressOrderItemDataList.ItemModifyWhen = DateTime.UtcNow;
                                    AliExpressOrderItemDataList.AliExpressOrderId = item.OrderId;
                                    AliExpressOrderItemDataList.AliExpressProductOrderId = null;
                                    AliExpressOrderItemDataList.SCOrderId = null;
                                }
                            }
                            datacontext.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        public List<aliexpressorder> getAllOrdersFromDatabase()
        {
            List<aliexpressorder> Orders = new List<aliexpressorder>();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    Orders = datacontext.aliexpressorders.ToList();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return Orders;
        }
    }
}
