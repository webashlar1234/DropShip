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
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Data.Entity;

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
                List<user> users = new List<user>();
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    users = datacontext.users.Where(x => x.IsActive == true && x.AliExpressSellerID != null).ToList();
                }

                foreach (user user in users)
                {
                    string access_token = StaticValues.getAccessTokenObjFromStr(user.AliExpressAccessToken);
                    if (!string.IsNullOrEmpty(access_token))
                    {
                        AliexpressSolutionOrderGetRequest req = new AliexpressSolutionOrderGetRequest();

                        AliexpressSolutionOrderGetRequest.OrderQueryDomain obj1 = new AliexpressSolutionOrderGetRequest.OrderQueryDomain();

                        //var todayDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        //obj1.CreateDateEnd = DateTime.Now.AddMinutes(-5).ToString("yyyy-MM-dd HH:mm:ss");
                        //obj1.CreateDateStart = todayDate;
                        //obj1.ModifiedDateStart = todayDate;

                        var todayDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        obj1.CreateDateEnd = todayDate;
                        obj1.CreateDateStart = "2020-03-01 00:00:00";
                        //obj1.ModifiedDateStart = "2020-03-01 00:00:00";//don't add this, latest orders aren't fetching using this

                        obj1.OrderStatusList = new List<string> { "SELLER_PART_SEND_GOODS", "PLACE_ORDER_SUCCESS", "IN_CANCEL", "WAIT_SELLER_SEND_GOODS", "WAIT_BUYER_ACCEPT_GOODS", "FUND_PROCESSING", "IN_ISSUE", "IN_FROZEN", "WAIT_SELLER_EXAMINE_MONEY", "RISK_CONTROL", "FINISH" };
                        //obj1.BuyerLoginId = "edacan0107@aol.com";
                        obj1.PageSize = 20L;
                        //obj1.ModifiedDateEnd = DateTime.Now.AddMinutes(-5).ToString("yyyy-MM-dd HH:mm:ss");
                        obj1.ModifiedDateEnd = todayDate;
                        obj1.CurrentPage = 1L;
                        //obj1.OrderStatus = "SELLER_PART_SEND_GOODS";
                        req.Param0_ = obj1;
                        Top.Api.Response.AliexpressSolutionOrderGetResponse rsp = client.Execute(req, access_token);
                        result = JsonConvert.SerializeObject(rsp.Result);
                        orders = JsonConvert.DeserializeObject<ResultData>(result);
                        if (orders != null)
                        {
                            if (orders.TargetList.Count > 0)
                            {
                                InsertOrUpdateOrderData(orders, access_token);
                            }
                        }
                    }
                    else
                    {
                        logger.Error("Access Token is null for user " + user.UserID);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return orders;

        }

        public product GetProductByAliId(string Id, string sku)
        {
            product Product = new product();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    long? ProductId = (from sp in datacontext.sellerspickedproducts
                                       from sps in datacontext.sellerpickedproductskus.Where(x => x.SellerPickedId == sp.SellersPickedID && x.SKUCode == sku).DefaultIfEmpty()
                                       where sp.AliExpressProductID == Id
                                       select sps.ProductId > 0 ? sps.ProductId : sp.ParentProductID //if product is sku get sku productid else parent productid
                                 ).FirstOrDefault();

                    Product = GetProductCostById(ProductId);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return Product;
        }

        public product GetProductCostById(long? ProductId)
        {
            product Product = new product();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    Product = datacontext.products.Where(x => x.ProductID == ProductId).FirstOrDefault();
                    if (Product != null)
                    {
                        if (Product.Cost == null)
                        {
                            product ParentProduct = datacontext.products.Where(x => x.ProductID == Product.ParentProductID).FirstOrDefault();
                            Product.Cost = ParentProduct.Cost;
                        }
                        currencyrate currency = datacontext.currencyrates.Where(x => x.CurrencyCode == Product.SellingPriceCurrency).FirstOrDefault();
                        if (currency != null && !string.IsNullOrEmpty(Product.Cost))
                        {
                            Product.Cost = StaticValues.GetUSDprice(Product.Cost, currency.Rate);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return Product;
        }

        public bool FullFillAliExpressOrder(OrderData orderData, bool isFullShip)
        {
            bool result = true;

            try
            {
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);

                order DbOrder = new order();
                orderapiresult orderresult = new orderapiresult();
                string LogisticserviceName = string.Empty;
                string accessToken = string.Empty;
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    DbOrder = datacontext.orders.Where(x => x.AliExpressOrderID == orderData.AliExpressOrderNumber).FirstOrDefault();

                    var res_data = (from o in datacontext.orders
                                    join or in datacontext.orderapiresults on o.OrderID equals or.SC_OrderID
                                    join u in datacontext.users on o.AliExpressLoginID equals u.AliExpressLoginID
                                    where o.AliExpressOrderID == orderData.AliExpressOrderNumber
                                    select new
                                    {
                                        orderResult = or,
                                        access_token = u.AliExpressAccessToken
                                    }).FirstOrDefault();

                    if (res_data != null)
                    {
                        orderresult = res_data.orderResult;
                        accessToken = StaticValues.getAccessTokenObjFromStr(res_data.access_token);
                    }
                }

                if (DbOrder != null)
                {
                    if (orderresult != null)
                    {
                        LogisticserviceName = orderresult.LogisticsServiceId;
                    }
                    if (string.IsNullOrEmpty(LogisticserviceName))
                    {
                        //get Logistic service
                        AliexpressLogisticsRedefiningGetonlinelogisticsservicelistbyorderidRequest req1 = new AliexpressLogisticsRedefiningGetonlinelogisticsservicelistbyorderidRequest();
                        req1.GoodsWidth = 1L;
                        req1.GoodsHeight = 1L;
                        req1.GoodsWeight = "1.5";
                        req1.GoodsLength = 1L;
                        req1.OrderId = Convert.ToInt64(orderData.AliExpressOrderNumber);
                        AliexpressLogisticsRedefiningGetonlinelogisticsservicelistbyorderidResponse rsp1 = client.Execute(req1, accessToken);
                        LogisticserviceName = rsp1.ResultList[0].LogisticsServiceId;
                    }

                    //get logistic service name (Service key)
                    AliexpressLogisticsRedefiningListlogisticsserviceRequest req3 = new AliexpressLogisticsRedefiningListlogisticsserviceRequest();
                    AliexpressLogisticsRedefiningListlogisticsserviceResponse rsp3 = client.Execute(req3, accessToken);
                    string ServiceName = rsp3.ResultList.Where(x => LogisticserviceName.Contains(x.LogisticsCompany)).Select(x => x.ServiceName).FirstOrDefault();
                    string res = JsonConvert.SerializeObject(rsp3.ResultList);

                    ////get logistic number (tracking number)
                    //AliexpressLogisticsQuerylogisticsorderdetailRequest req4 = new AliexpressLogisticsQuerylogisticsorderdetailRequest();
                    //req4.TradeOrderId = Convert.ToInt64(orderData.AliExpressOrderNumber); //required OrderId
                    //AliexpressLogisticsQuerylogisticsorderdetailResponse rsp4 = client.Execute(req4, SessionManager.GetAccessToken().access_token);
                    //string resultdata = JsonConvert.SerializeObject(rsp4.Result);

                    if (!string.IsNullOrEmpty(ServiceName))
                    {
                        AliexpressSolutionOrderFulfillRequest req = new AliexpressSolutionOrderFulfillRequest();
                        req.ServiceName = ServiceName;
                        //req.TrackingWebsite = orderData.OrignalProductLink; //only if servicename is other
                        req.OutRef = orderData.AliExpressOrderNumber;
                        req.SendType = isFullShip ? "all" : "part"; //'all' if fullfill full order
                        req.Description = "memo";
                        req.LogisticsNo = orderData.TrackingNumber;
                        AliexpressSolutionOrderFulfillResponse rsp = client.Execute(req, accessToken);
                        orderresult.OrderFulfillResponse = JsonConvert.SerializeObject(rsp);

                        using (DropshipDataEntities datacontext = new DropshipDataEntities())
                        {
                            AddOrderResult(orderresult);
                            if (rsp.Result.ResultSuccess)
                            {
                                DbOrder.OrderStatus = "Shipped";
                                datacontext.Entry(DbOrder).State = System.Data.Entity.EntityState.Modified;
                            }
                            datacontext.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                logger.Info(orderData.TrackingNumber + ": " + ex.ToString());
            }

            return result;
        }

        public void InsertOrUpdateOrderData(ResultData orders, string access_token)
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

                            ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);

                            if (!AliiExpressOrderId)
                            {
                                AliexpressSolutionOrderInfoGetRequest req1 = new AliexpressSolutionOrderInfoGetRequest();
                                AliexpressSolutionOrderInfoGetRequest.OrderDetailQueryDomain obj = new AliexpressSolutionOrderInfoGetRequest.OrderDetailQueryDomain();
                                obj.ExtInfoBitFlag = 11111L;
                                obj.OrderId = item.OrderId;
                                req1.Param1_ = obj;
                                AliexpressSolutionOrderInfoGetResponse rsp1 = client.Execute(req1, access_token);
                                var resultdata1 = JsonConvert.SerializeObject(rsp1.Result);
                                AliexpressSolutionOrderInfoGetResponse.BaseResultDomain OrderDetails = rsp1.Result;

                                orderapiresult orderResult = new orderapiresult();
                                StripeResultModel resultStripePayment = null;
                                double sellerChargeAmount = 0;

                                AliExpressOrderData.AliExpressOrderID = item.OrderId;
                                AliExpressOrderData.BuyerLoginId = item.BuyerLoginId;
                                AliExpressOrderData.AliExpressLoginID = item.SellerLoginId;
                                AliExpressOrderData.DeliveryCountry = OrderDetails.Data.ReceiptAddress.Country;
                                AliExpressOrderData.PaymentStatus = item.FundStatus;
                                AliExpressOrderData.TrackingNo = null;
                                AliExpressOrderData.ShippingWeight = null;
                                AliExpressOrderData.OrderAmount = item.PayAmount != null ? item.PayAmount.Amount : null;
                                AliExpressOrderData.CurrencyCode = item.PayAmount != null ? item.PayAmount.CurrencyCode : null;
                                AliExpressOrderData.OrderStatus = item.OrderStatus;
                                AliExpressOrderData.NoOfOrderItems = orders.TotalCount;
                                AliExpressOrderData.ItemCreatedWhen = DateTime.UtcNow;
                                AliExpressOrderData.ItemModifyWhen = DateTime.UtcNow;
                                AliExpressOrderData.AliExpressOrderCreatedTime = item.GmtCreate;
                                AliExpressOrderData.AliExpressOrderUpdatedTime = item.GmtUpdate;
                                datacontext.aliexpressorders.Add(AliExpressOrderData);

                                foreach (var product in item.ProductList)
                                {
                                    AliExpressOrderItemData.AliExpressProductID = product.ProductId.ToString();
                                    product DbProduct = GetProductByAliId(AliExpressOrderItemData.AliExpressProductID, product.SkuCode);

                                    if (DbProduct != null)
                                    {
                                        sellerChargeAmount = sellerChargeAmount + double.Parse(DbProduct.Cost);
                                        AliExpressOrderItemData.ProductID = Convert.ToInt64(DbProduct.ProductID);
                                    }
                                    insertOrderItemData(OrderDetails,item, product, access_token);
                                }


                                if (item.OrderStatus == "WAIT_SELLER_SEND_GOODS" || item.OrderStatus == "SELLER_PART_SEND_GOODS")
                                {
                                    try
                                    {
                                        //charge seller
                                        StripeService _stripeservice = new StripeService();
                                        string stripeCustomerId = datacontext.users.Where(x => x.AliExpressLoginID == item.SellerLoginId).Select(x => x.StripeCustomerID).FirstOrDefault();
                                        if (!string.IsNullOrEmpty(stripeCustomerId) && sellerChargeAmount > 0)
                                        {
                                            resultStripePayment = _stripeservice.ChargeSavedCard(stripeCustomerId, (long)sellerChargeAmount);
                                        }

                                        //get logistic tracking number and cainiao label
                                        orderResult = getLogisticsServicByOrderId(item.OrderId, access_token);
                                        //AddOrderResult(orderResult);
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Info(ex);
                                    }
                                }

                                //var OrderId = datacontext.Orders.Where(x => x.OrderID == item.OrderId).Any();
                                OrderData.AliExpressOrderID = item.OrderId.ToString();
                                OrderData.DeliveryCountry = OrderDetails.Data != null && OrderDetails.Data.ReceiptAddress != null ? OrderDetails.Data.ReceiptAddress.Country : string.Empty; ;
                                OrderData.ShippingWeight = null;
                                OrderData.OrderAmount = item.PayAmount != null ? item.PayAmount.Amount : null;
                                OrderData.OrderStatus = getLocalOrderStatus(item.OrderStatus);
                                OrderData.PaymentStatus = item.FundStatus;
                                OrderData.AliExpressLoginID = item.SellerLoginId;
                                OrderData.ItemCreatedBy = null;
                                OrderData.ItemCreatedWhen = DateTime.UtcNow;
                                OrderData.ItemModifyBy = null;
                                OrderData.ItemModifyWhen = DateTime.UtcNow;
                                if (resultStripePayment != null)
                                {
                                    OrderData.SellerPaymentStatus = resultStripePayment.IsSuccess == true ? 1 : 0;
                                    OrderData.SellerPaymentDetails = resultStripePayment.Result;
                                }
                                datacontext.orders.Add(OrderData);
                                datacontext.SaveChanges();
                                orderResult.SC_OrderID = OrderData.OrderID;
                                AddOrderResult(orderResult);
                            }
                            else
                            {
                                List<string> aliExpressSkuCode = new List<string>();
                                List<string> orderItemSkuCode = new List<string>();

                                AliExpressOrderData = datacontext.aliexpressorders.Where(x => x.AliExpressOrderID == item.OrderId).FirstOrDefault();

                                AliExpressOrderData.AliExpressOrderID = item.OrderId;
                                AliExpressOrderData.BuyerLoginId = item.BuyerLoginId;
                                AliExpressOrderData.AliExpressLoginID = item.SellerLoginId;
                                AliExpressOrderData.DeliveryCountry = null;
                                AliExpressOrderData.PaymentStatus = item.FundStatus;
                                AliExpressOrderData.TrackingNo = null;
                                AliExpressOrderData.ShippingWeight = null;
                                AliExpressOrderData.OrderAmount = item.PayAmount != null ? item.PayAmount.Amount : null;
                                AliExpressOrderData.CurrencyCode = item.PayAmount != null ? item.PayAmount.CurrencyCode : null;
                                AliExpressOrderData.OrderStatus = item.OrderStatus;
                                AliExpressOrderData.NoOfOrderItems = orders.TotalCount;
                                AliExpressOrderData.ItemModifyWhen = DateTime.UtcNow;
                                AliExpressOrderData.AliExpressOrderUpdatedTime = item.GmtUpdate;
                                datacontext.Entry(AliExpressOrderData).State = System.Data.Entity.EntityState.Modified;

                                foreach (var product in item.ProductList)
                                {
                                    aliExpressSkuCode.Add(product.SkuCode);
                                    AliExpressOrderItemData = datacontext.aliexpressorderitems.Where(x => x.AliExpressProductID == product.ProductId.ToString() && x.AliExpressOrderId == item.OrderId && x.SkuCode == product.SkuCode).FirstOrDefault();
                                    if (AliExpressOrderItemData != null)
                                    {
                                        AliExpressOrderItemData.ProductName = product.ProductName;
                                        AliExpressOrderItemData.Price = product.TotalProductAmount.Amount;
                                        AliExpressOrderItemData.CurrencyCode = product.TotalProductAmount.CurrencyCode;
                                        AliExpressOrderItemData.ItemModifyWhen = DateTime.UtcNow;
                                        AliExpressOrderItemData.AliExpressOrderId = item.OrderId;
                                        AliExpressOrderItemData.AliExpressProductOrderId = null;
                                        AliExpressOrderItemData.SCOrderId = null;
                                        AliExpressOrderItemData.SkuCode = product.SkuCode;
                                        orderItemSkuCode.Add(AliExpressOrderItemData.SkuCode);
                                        datacontext.Entry(AliExpressOrderItemData).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    else
                                    {
                                        orderItemSkuCode.Add(product.SkuCode);
                                        insertOrderItemData(null,item,product, access_token);
                                    }
                                }
                                List<string> differSkuCode = aliExpressSkuCode.Except(orderItemSkuCode).ToList();
                                if (differSkuCode.Count() > 0)
                                {
                                    foreach (var skuCode in differSkuCode)
                                    {
                                        AliExpressOrderItemData = datacontext.aliexpressorderitems.Where(x => x.AliExpressOrderId == item.OrderId && x.SkuCode == skuCode).FirstOrDefault();
                                         if (AliExpressOrderItemData != null)
                                         {
                                             datacontext.aliexpressorderitems.Remove(AliExpressOrderItemData);
                                         }
                                    }
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

        public List<OrderData> getAllOrdersFromDatabase(int? UserID = null)
        {
            //List<OrderData> OrdersList = new List<OrderData>();
            List<OrderData> Orders = new List<OrderData>();
            List<OrderViewModel> ChildOrders = new List<OrderViewModel>();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    Orders = (from o in datacontext.orders
                              join u in datacontext.users on o.AliExpressLoginID equals u.AliExpressLoginID
                              from or in datacontext.orderapiresults.Where(x => x.SC_OrderID == o.OrderID).DefaultIfEmpty()
                              from ao in datacontext.aliexpressorders.Where(x => x.AliExpressOrderID.ToString() == o.AliExpressOrderID).DefaultIfEmpty()
                              where u.IsActive == true && UserID > 0 ? u.UserID == UserID : true
                              select new OrderData
                              {
                                  AliExpressOrderID = o.AliExpressOrderID,
                                  AliExpressOrderNumber = o.AliExpressOrderID,
                                  OrderAmount = o.OrderAmount,
                                  DeleveryCountry = o.DeliveryCountry,
                                  ShippingWeight = o.ShippingWeight,
                                  OrderStatus = o.OrderStatus,
                                  PaymentStatus = o.PaymentStatus,
                                  SellerPaymentStatus = o.SellerPaymentStatus,
                                  SellerID = u.AliExpressSellerID,
                                  SellerEmail = u.EmailID,
                                  TrackingNumber = or.LogisticsNumber,
                                  IsReadyToShipAny = o.OrderStatus == StaticValues.Waiting_for_Shipment ? true : false,
                                  IsReadyToRefund = (o.SellerPaymentStatus == 1 && o.OrderStatus == StaticValues.Waiting_for_Shipment) ? true : false,
                                  AliExpressOrderCreatedTime = ao.AliExpressOrderCreatedTime
                              }).ToList();

                    List<OrderViewModel> childList = (from o in datacontext.orders
                                                      join oi in datacontext.aliexpressorderitems on o.AliExpressOrderID equals oi.AliExpressOrderId.ToString()
                                                      from sp in datacontext.sellerspickedproducts.Where(x => x.AliExpressProductID == oi.AliExpressProductID).DefaultIfEmpty()
                                                      from p in datacontext.products.Where(x => x.ProductID == sp.ParentProductID).DefaultIfEmpty()
                                                      //join p in datacontext.products on sp.ParentProductID equals p.ProductID
                                                      where o.AliExpressOrderID != null
                                                      select new OrderViewModel
                                                      {
                                                          AliExpressOrderId = oi.AliExpressOrderId.ToString(),
                                                          AliExpressProductId = oi.AliExpressProductID,
                                                          OrignalProductId = p.OriginalProductID,
                                                          OrignalProductLink = p.SourceWebsite,
                                                          ProductName = oi.ProductName,
                                                          Price = oi.Price,
                                                          Colour = oi.Color,
                                                          Size = oi.Size,
                                                          IsReadyToBuy = !string.IsNullOrEmpty(sp.AliExpressProductID) && o.SellerPaymentStatus == 1 && o.OrderStatus == StaticValues.Unpurchased ? true : false
                                                      }).ToList();

                    if (Orders.Count > 0)
                    {
                        foreach (OrderData OrderdataModel in Orders)
                        {
                            //OrderData OrderGroup = new OrderData();

                            //if (childList.Count > 0)
                            //{
                            //    foreach (var childdata in childList)
                            //    {
                            //        sellerspickedproduct data = datacontext.sellerspickedproducts.Where(x => x.AliExpressProductID == childdata.AliExpressProductId).FirstOrDefault();
                            //        if (data != null)
                            //        {
                            //            var parantproductId = data.ParentProductID;
                            //            product productdata = datacontext.products.Where(x => x.ProductID == parantproductId).FirstOrDefault();
                            //            childdata.OrignalProductLink = productdata.SourceWebsite;
                            //            childdata.OrignalProductId = productdata.OriginalProductID;
                            //        }
                            //    }
                            //}
                            OrderdataModel.ChildOrderItemList = childList.Where(x => x.AliExpressOrderId == OrderdataModel.AliExpressOrderID).ToList();
                            OrderdataModel.IsReadyToBuyAny = OrderdataModel.ChildOrderItemList.Where(x => x.IsReadyToBuy == true).Any();
                            //OrdersList.Add(OrderdataModel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return Orders;
        }

        public orderapiresult getLogisticsServicByOrderId(long OrderID, string access_token)
        {
            orderapiresult OrderResult = new orderapiresult();
            OrderResult.IsSuccess = true;
            OrderResult.AliExpressOrderID = OrderID.ToString();

            List<LogisticServiceData> orderLogisticData = new List<LogisticServiceData>();
            string resultdata = String.Empty;
            try
            {
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);
                //Get Available Online Logistics Service List by order id
                AliexpressLogisticsRedefiningGetonlinelogisticsservicelistbyorderidRequest req = new AliexpressLogisticsRedefiningGetonlinelogisticsservicelistbyorderidRequest();
                req.GoodsWidth = 1L; //optional
                req.GoodsHeight = 1L; //optional
                req.GoodsWeight = "1.5"; //optional
                req.GoodsLength = 1L; //optional
                req.OrderId = OrderID; //required OrderId
                AliexpressLogisticsRedefiningGetonlinelogisticsservicelistbyorderidResponse rsp = client.Execute(req, access_token);
                OrderResult.getonlinelogisticsserviceResponse = JsonConvert.SerializeObject(rsp);

                if (!rsp.IsError && rsp.ResultList.Count > 0)
                {
                    resultdata = JsonConvert.SerializeObject(rsp.ResultList);
                    orderLogisticData = JsonConvert.DeserializeObject<List<LogisticServiceData>>(resultdata);
                    OrderResult.LogisticsServiceId = orderLogisticData[0].LogisticsServiceId;
                    if (!string.IsNullOrEmpty(OrderResult.LogisticsServiceId))
                    {
                        OrderResult = createWarehouseOrder(orderLogisticData, OrderID, access_token, OrderResult);
                    }
                }
                else
                {
                    OrderResult.IsSuccess = false;
                    OrderResult.Error = "getLogisticsServicByOrderId : " + rsp.ErrCode + "," + rsp.ErrMsg;
                }
            }
            catch (Exception ex)
            {
                logger.Info("OrderID " + OrderID + ": " + ex.ToString());
            }

            return OrderResult;
        }

        public orderapiresult createWarehouseOrder(List<LogisticServiceData> orderLogisticData, long OrderID, string access_token, orderapiresult OrderResult)
        {
            string resultdata = String.Empty;
            try
            {
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);

                AliexpressSolutionOrderInfoGetRequest req1 = new AliexpressSolutionOrderInfoGetRequest();
                AliexpressSolutionOrderInfoGetRequest.OrderDetailQueryDomain obj = new AliexpressSolutionOrderInfoGetRequest.OrderDetailQueryDomain();
                obj.ExtInfoBitFlag = 11111L;
                obj.OrderId = OrderID;
                req1.Param1_ = obj;
                AliexpressSolutionOrderInfoGetResponse rsp1 = client.Execute(req1, access_token);
                var resultdata1 = JsonConvert.SerializeObject(rsp1.Result);
                AliexpressSolutionOrderInfoGetResponse.BaseResultDomain OrderDetails = rsp1.Result;

                //Create Warehouse Order
                AliexpressLogisticsCreatewarehouseorderRequest req = new AliexpressLogisticsCreatewarehouseorderRequest();
                AliexpressLogisticsCreatewarehouseorderRequest.AddressdtosDomain AddressDTO = new AliexpressLogisticsCreatewarehouseorderRequest.AddressdtosDomain();

                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    defaultaddress senderAdd = datacontext.defaultaddresses.Where(x => x.Type == "sender").FirstOrDefault();
                    defaultaddress refundAdd = datacontext.defaultaddresses.Where(x => x.Type == "refund").FirstOrDefault();

                    if (senderAdd != null)
                    {
                        AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareAddressDtoDomain SenderObj = new AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareAddressDtoDomain();
                        SenderObj.Phone = senderAdd.Phone;
                        SenderObj.Fax = senderAdd.Fax;
                        SenderObj.Street = senderAdd.Street;
                        SenderObj.PostCode = senderAdd.PostCode;
                        SenderObj.Country = refundAdd.Country;
                        SenderObj.City = senderAdd.City;
                        SenderObj.County = senderAdd.County;
                        SenderObj.AddressId = -1;
                        SenderObj.Name = senderAdd.Name;
                        SenderObj.Province = senderAdd.Province;
                        SenderObj.StreetAddress = senderAdd.StreetAddress;
                        SenderObj.Mobile = senderAdd.Mobile;
                        AddressDTO.Sender = SenderObj;
                    }

                    if (refundAdd != null)
                    {
                        AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareAddressDtoDomain RefundObj = new AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareAddressDtoDomain();
                        RefundObj.Phone = refundAdd.Phone;
                        RefundObj.Fax = refundAdd.Fax;
                        RefundObj.Street = refundAdd.Street;
                        RefundObj.Country = refundAdd.Country;
                        RefundObj.City = refundAdd.City;
                        RefundObj.County = refundAdd.County;
                        RefundObj.AddressId = -1;
                        RefundObj.Name = refundAdd.Name;
                        RefundObj.Province = refundAdd.Province;
                        RefundObj.StreetAddress = refundAdd.StreetAddress;
                        RefundObj.Mobile = refundAdd.Mobile;
                        RefundObj.PostCode = refundAdd.PostCode;
                        AddressDTO.Refund = RefundObj;
                    }

                    //if (senderAdd != null)
                    //{
                    //    AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareAddressDtoDomain SenderObj = new AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareAddressDtoDomain();
                    //    SenderObj.Phone = "098-234234";
                    //    SenderObj.Fax = "234234234";
                    //    SenderObj.Street = "street";
                    //    SenderObj.PostCode = "01010";
                    //    SenderObj.Country = "TR";
                    //    SenderObj.City = "Seyhan";
                    //    SenderObj.County = "county";
                    //    SenderObj.AddressId = -1;
                    //    SenderObj.Name = "Linda";
                    //    SenderObj.Province = "Adana";
                    //    SenderObj.StreetAddress = "street address";
                    //    SenderObj.Mobile = "18766234324";
                    //    AddressDTO.Sender = SenderObj;
                    //}

                    //if (refundAdd != null)
                    //{
                    //    AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareAddressDtoDomain RefundObj = new AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareAddressDtoDomain();
                    //    RefundObj.Fax = "234234234";
                    //    RefundObj.Street = "street";
                    //    RefundObj.Country = "RU";
                    //    RefundObj.City = "Moscow";
                    //    RefundObj.County = "county";
                    //    RefundObj.AddressId = -1;
                    //    RefundObj.Name = "Linda";
                    //    RefundObj.Province = "Moscow";
                    //    RefundObj.StreetAddress = "street address";
                    //    RefundObj.Mobile = "18766234324";
                    //    RefundObj.PostCode = "056202";
                    //    AddressDTO.Refund = RefundObj;
                    //}
                }

                AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareAddressDtoDomain ReceiverObj = new AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareAddressDtoDomain();
                ReceiverObj.Phone = OrderDetails.Data.ReceiptAddress.PhoneNumber;
                ReceiverObj.Fax = OrderDetails.Data.ReceiptAddress.FaxNumber;
                //obj4.MemberType = "类型"; //optional
                //obj4.TrademanageId = "cn234234234"; //optional
                ReceiverObj.Street = OrderDetails.Data.ReceiptAddress.Address;
                ReceiverObj.PostCode = OrderDetails.Data.ReceiptAddress.Zip;
                ReceiverObj.Country = OrderDetails.Data.ReceiptAddress.Country;
                ReceiverObj.City = OrderDetails.Data.ReceiptAddress.City;
                ReceiverObj.County = OrderDetails.Data.ReceiptAddress.DetailAddress;
                //obj4.Email = "alibaba@alibaba.com"; //optional
                ReceiverObj.AddressId = -1;
                ReceiverObj.Name = OrderDetails.Data.ReceiptAddress.ContactPerson;
                ReceiverObj.Province = OrderDetails.Data.ReceiptAddress.Province;
                ReceiverObj.StreetAddress = "street address";
                ReceiverObj.Mobile = OrderDetails.Data.ReceiptAddress.MobileNo;
                AddressDTO.Receiver = ReceiverObj;

                req.AddressDTOs_ = AddressDTO;
                List<AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareProductForTopDtoDomain> ProductList = new List<AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareProductForTopDtoDomain>();

                foreach (var item in OrderDetails.Data.ChildOrderList)
                {
                    AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareProductForTopDtoDomain ProductObj = new AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareProductForTopDtoDomain();
                    //obj8.AneroidMarkup = false;
                    //obj8.Breakable = false;
                    ProductObj.CategoryCnDesc = "衬衫";
                    ProductObj.CategoryEnDesc = "Shirt";
                    //obj8.ContainsBattery = false;
                    //obj8.HsCode = "77234";
                    //obj8.OnlyBattery = false;
                    ProductObj.ProductDeclareAmount = item.InitOrderAmt.Amount;
                    ProductObj.ProductId = item.ProductId;
                    ProductObj.ProductNum = item.ProductCount;
                    //obj8.ProductWeight = "1.5";
                    //obj8.ScItemCode = "scItem code";
                    //obj8.ScItemId = 1000L;
                    //obj8.ScItemName = "scItem name";
                    //obj8.SkuCode = "sku code";
                    //obj8.SkuValue = "sku value";
                    ProductList.Add(ProductObj);
                }

                req.DeclareProductDTOs_ = ProductList;

                //req.DomesticLogisticsCompany = "tiantiankuaidi";
                req.DomesticLogisticsCompanyId = 505L;
                req.DomesticTrackingNo = "none";
                req.PackageNum = 1L;
                req.TradeOrderFrom = "ESCROW";
                req.TradeOrderId = OrderID;
                req.UndeliverableDecision = 0L;
                req.WarehouseCarrierService = orderLogisticData[0].LogisticsServiceId;
                req.InvoiceNumber = "";
                //req.TopUserKey = "xxxxxxx";
                AliexpressLogisticsCreatewarehouseorderResponse rsp = client.Execute(req, access_token);
                OrderResult.createwarehouseorderResponse = JsonConvert.SerializeObject(rsp);

                if (rsp.ResultSuccess)
                {
                    OrderResult.WarehouseOrderId = rsp.Result.WarehouseOrderId;
                }
                else
                {
                    OrderResult.IsSuccess = false;
                    OrderResult.Error = "createWarehouseOrder : " + rsp.ErrCode + "," + rsp.ErrMsg;
                }

            }
            catch (Exception ex)
            {
                logger.Info("OrderID " + OrderID + ": " + ex.ToString());
            }

            return OrderResult;
        }

        public bool checkWarehouceOrderStatus()
        {
            bool result = true;
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    var orderapiresult = (from oar in datacontext.orderapiresults
                                          join o in datacontext.orders on new { Key1 = oar.SC_OrderID, Key2 = oar.AliExpressOrderID } equals new { Key1 = o.OrderID, Key2 = o.AliExpressOrderID }
                                          join u in datacontext.users on o.AliExpressLoginID equals u.AliExpressLoginID
                                          where string.IsNullOrEmpty(oar.LogisticsNumber)
                                          select new
                                          {
                                              orderapiresults = oar,
                                              access_token = u.AliExpressAccessToken
                                          }).ToList();

                    foreach (var item in orderapiresult)
                    {
                        if (item.orderapiresults != null && !string.IsNullOrEmpty(item.access_token))
                        {
                            orderapiresult OrderResult = item.orderapiresults;
                            OrderResult = getInternationalLogisticNoByOrderId(StaticValues.getAccessTokenObjFromStr(item.access_token), OrderResult);
                            AddOrderResult(OrderResult);
                            if (OrderResult.CainiaoLabel != null && OrderResult.CainiaoLabel.Length > 0)
                            {
                                new EmailSender().SendEmail(OrderResult.CainiaoLabel, OrderResult.AliExpressOrderID);
                            }
                            else
                            {
                                new EmailSender().SendFailureEmail(OrderResult.AliExpressOrderID);
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

        public orderapiresult getInternationalLogisticNoByOrderId(string access_token, orderapiresult OrderResult)
        {
            string resultdata = String.Empty;
            long AliExpressOrderID = OrderResult.AliExpressOrderID != null ? Convert.ToInt64(OrderResult.AliExpressOrderID) : 0;
            try
            {
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);
                AliexpressLogisticsQuerylogisticsorderdetailRequest req = new AliexpressLogisticsQuerylogisticsorderdetailRequest();
                req.TradeOrderId = AliExpressOrderID; //required OrderId
                AliexpressLogisticsQuerylogisticsorderdetailResponse rsp = client.Execute(req, access_token);
                resultdata = JsonConvert.SerializeObject(rsp.Result);

                OrderResult.getLogisticNumberResponse = resultdata;
                if (rsp.Result.Success)
                {
                    List<AliexpressLogisticsQuerylogisticsorderdetailResponse.AeopLogisticsOrderDetailDtoDomain> OrderDetailDto = rsp.Result.ResultList;
                    OrderResult.LogisticsNumber = OrderDetailDto.FirstOrDefault().InternationalLogisticsNum;

                    if (!string.IsNullOrEmpty(OrderResult.LogisticsNumber))
                    {
                        OrderResult = getPrintInfo(access_token, OrderResult);
                    }
                }
                else
                {
                    OrderResult.IsSuccess = false;
                    OrderResult.Error = "getInternationalLogisticNoByOrderId : " + rsp.ErrCode + "," + rsp.ErrMsg;
                }
            }
            catch (Exception ex)
            {
                logger.Info("OrderID " + AliExpressOrderID + ": " + ex.ToString());
            }

            return OrderResult;
        }

        public orderapiresult getPrintInfo(string access_token, orderapiresult OrderResult)
        {
            string resultdata = String.Empty;
            try
            {
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);
                AliexpressLogisticsRedefiningGetprintinfoRequest req = new AliexpressLogisticsRedefiningGetprintinfoRequest();
                req.InternationalLogisticsId = OrderResult.LogisticsNumber; //required InternationalLogisticsId from getInternationalLogisticNoByOrderId result
                AliexpressLogisticsRedefiningGetprintinfoResponse rsp = client.Execute(req, access_token);
                resultdata = JsonConvert.SerializeObject(rsp.Result);
                OrderResult.getprintinfoResponse = resultdata;

                if (!rsp.IsError)
                {
                    LogisticPrintinfo LogisticPrintinfo = JsonConvert.DeserializeObject<LogisticPrintinfo>(rsp.Result);

                    byte[] bytes = Convert.FromBase64String(LogisticPrintinfo.body);
                    OrderResult.CainiaoLabel = bytes;

                    var path = StaticValues.CainiaoFiles_path + OrderResult.LogisticsNumber + ".pdf";

                    System.IO.FileStream stream =
                    new FileStream(path, FileMode.CreateNew);
                    System.IO.BinaryWriter writer =
                        new BinaryWriter(stream);
                    writer.Write(bytes, 0, bytes.Length);
                    writer.Close();
                }
                else
                {
                    OrderResult.IsSuccess = false;
                    OrderResult.Error = "getPrintInfo : " + rsp.ErrCode + "," + rsp.ErrMsg;
                }
            }
            catch (Exception ex)
            {
                logger.Info("LogisticsNo " + OrderResult.LogisticsNumber + ": " + ex.ToString());
            }

            return OrderResult;
        }

        public bool AddOrderResult(orderapiresult model)
        {
            bool result = true;
            try
            {
                orderapiresult DbOrderresult = new orderapiresult();

                if (model != null)
                {
                    using (DropshipDataEntities datacontext = new DropshipDataEntities())
                    {
                        DbOrderresult = datacontext.orderapiresults.Where(x => x.AliExpressOrderID == model.AliExpressOrderID).FirstOrDefault();
                        if (DbOrderresult != null)
                        {
                            datacontext.Entry(DbOrderresult).State = EntityState.Detached;
                            datacontext.Entry(model).State = EntityState.Modified;
                        }
                        else
                        {
                            datacontext.orderapiresults.Add(model);
                        }

                        datacontext.SaveChanges();
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

        public bool BuyOrderFromSourceWebsite(string OrderID)
        {
            bool result = true;
            try
            {
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);

                order DbOrder = new order();
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    DbOrder = datacontext.orders.Where(x => x.AliExpressOrderID == OrderID).FirstOrDefault();
                    DbOrder.OrderStatus = StaticValues.Waiting_for_Shipment;
                    datacontext.Entry(DbOrder).State = EntityState.Modified;
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
        public bool PayForOrderBySeller(string OrderID, int UserID)
        {
            StripeResultModel resultStripePayment = new StripeResultModel();
            try
            {
                //charge seller
                StripeService _stripeservice = new StripeService();

                order DbOrder = new order();
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    List<aliexpressorderitem> aliexpressorderitem = (from o in datacontext.orders
                                                                     from oi in datacontext.aliexpressorderitems.Where(x => x.AliExpressOrderId.ToString() == o.AliExpressOrderID)
                                                                     where o.AliExpressOrderID == OrderID
                                                                     select oi).ToList();

                    var UserData = (from o in datacontext.orders
                                    from u in datacontext.users.Where(x => x.AliExpressLoginID == o.AliExpressLoginID && x.UserID == UserID)
                                    where o.AliExpressOrderID == OrderID
                                    select new
                                    {
                                        u.StripeCustomerID,
                                        u.AliExpressLoginID
                                    }).FirstOrDefault();

                    double totalCharge = 0;
                    foreach (aliexpressorderitem item in aliexpressorderitem)
                    {
                        product Product = GetProductCostById(item.ProductID);
                        totalCharge = totalCharge + double.Parse(Product.Cost);
                    }


                    if (UserData != null)
                    {
                        if (!string.IsNullOrEmpty(UserData.StripeCustomerID) && totalCharge > 0 && !string.IsNullOrEmpty(UserData.AliExpressLoginID))
                        {
                            resultStripePayment = _stripeservice.ChargeSavedCard(UserData.StripeCustomerID, (long)totalCharge);
                            if (resultStripePayment != null)
                            {
                                order order = datacontext.orders.Where(x => x.AliExpressOrderID == OrderID && x.AliExpressLoginID == UserData.AliExpressLoginID).FirstOrDefault();
                                order.SellerPaymentStatus = resultStripePayment.IsSuccess == true ? 1 : 0;
                                order.SellerPaymentDetails = resultStripePayment.Result;
                                datacontext.Entry(order).State = EntityState.Modified;
                                datacontext.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resultStripePayment.IsSuccess = false;
                logger.Info(ex.ToString());
            }

            return resultStripePayment.IsSuccess;
        }


        public bool RefundSellerForOrder(string OrderID)
        {
            StripeResultModel resultStripeRefund = new StripeResultModel();
            try
            {
                StripeService _stripeservice = new StripeService();

                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    List<aliexpressorderitem> aliexpressorderitem = (from o in datacontext.orders
                                                                     from oi in datacontext.aliexpressorderitems.Where(x => x.AliExpressOrderId.ToString() == o.AliExpressOrderID)
                                                                     where o.AliExpressOrderID == OrderID
                                                                     select oi).ToList();

                    var UserData = (from o in datacontext.orders
                                    from u in datacontext.users.Where(x => x.AliExpressLoginID == o.AliExpressLoginID)
                                    where o.AliExpressOrderID == OrderID
                                    select new
                                    {
                                        u.UserID,
                                        o.AliExpressLoginID
                                    }).FirstOrDefault();

                    double totalCharge = 0;
                    foreach (aliexpressorderitem item in aliexpressorderitem)
                    {
                        product Product = GetProductCostById(item.ProductID);
                        totalCharge = totalCharge + double.Parse(Product.Cost);
                    }

                    if (UserData != null)
                    {
                        if (totalCharge > 0 && UserData.UserID > 0)
                        {
                            resultStripeRefund = _stripeservice.RefundStripeUser(UserData.UserID, (long)totalCharge);
                            if (resultStripeRefund != null)
                            {
                                order order = datacontext.orders.Where(x => x.AliExpressOrderID == OrderID && x.AliExpressLoginID == UserData.AliExpressLoginID).FirstOrDefault();
                                order.SellerPaymentStatus = resultStripeRefund.IsSuccess == true ? 2 : 0;
                                order.SellerPaymentDetails = resultStripeRefund.Result;
                                datacontext.Entry(order).State = EntityState.Modified;
                                datacontext.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resultStripeRefund.IsSuccess = false;
                logger.Info(ex.ToString());
            }

            return resultStripeRefund.IsSuccess;
        }

        public string getLocalOrderStatus(string AliOrderStatus)
        {
            string OrderStatus = string.Empty;

            switch (AliOrderStatus)
            {
                case "WAIT_SELLER_SEND_GOODS":
                    OrderStatus = StaticValues.Unpurchased;
                    break;
                case "SELLER_PART_SEND_GOODS":
                    OrderStatus = StaticValues.Unpurchased;
                    break;
                case "FINISH":
                    OrderStatus = StaticValues.Shipped;
                    break;
                default:
                    OrderStatus = StaticValues.Shipped;
                    break;
            }

            return OrderStatus;
        }

        public void insertOrderItemData( AliexpressSolutionOrderInfoGetResponse.BaseResultDomain itemOrderDetails,TargetList item,ProductList product,string access_token)
        {
            aliexpressorderitem AliExpressOrderItemData = new aliexpressorderitem();
            ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);
            AliexpressSolutionOrderInfoGetResponse.BaseResultDomain OrderDetails = new AliexpressSolutionOrderInfoGetResponse.BaseResultDomain();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    if (itemOrderDetails == null)
                    {
                        AliexpressSolutionOrderInfoGetRequest req1 = new AliexpressSolutionOrderInfoGetRequest();
                        AliexpressSolutionOrderInfoGetRequest.OrderDetailQueryDomain obj = new AliexpressSolutionOrderInfoGetRequest.OrderDetailQueryDomain();
                        obj.ExtInfoBitFlag = 11111L;
                        obj.OrderId = item.OrderId;
                        req1.Param1_ = obj;
                        AliexpressSolutionOrderInfoGetResponse rsp1 = client.Execute(req1, access_token);
                        var resultdata1 = JsonConvert.SerializeObject(rsp1.Result);
                        OrderDetails = rsp1.Result;
                    }
                    else
                    {
                        OrderDetails = itemOrderDetails;
                    }
                    AliExpressOrderItemData.AliExpressProductID = product.ProductId.ToString();
                    product DbProduct = GetProductByAliId(AliExpressOrderItemData.AliExpressProductID, product.SkuCode);

                    if (DbProduct != null)
                    {
                        AliExpressOrderItemData.ProductID = Convert.ToInt64(DbProduct.ProductID);
                    }
                    AliExpressOrderItemData.ProductName = product.ProductName;
                    AliExpressOrderItemData.Price = product.TotalProductAmount.Amount;
                    AliExpressOrderItemData.CurrencyCode = product.TotalProductAmount.CurrencyCode;
                    AliExpressOrderItemData.ItemCreatedWhen = DateTime.UtcNow;
                    AliExpressOrderItemData.ItemModifyWhen = DateTime.UtcNow;
                    AliExpressOrderItemData.AliExpressOrderId = item.OrderId;
                    AliExpressOrderItemData.AliExpressProductOrderId = null;
                    AliExpressOrderItemData.SCOrderId = null;
                    AliExpressOrderItemData.SkuCode = product.SkuCode;


                    var myJonString = OrderDetails.Data.ChildOrderExtInfoList[0].Sku;
                    var jo = JObject.Parse(myJonString);
                    if (jo["sku"].HasValues)
                    {
                        foreach (var item1 in jo["sku"])
                        {
                            if (item1["pName"].ToString().Trim() == "Color")
                            {
                                var colour = item1["pValue"].ToString();
                                AliExpressOrderItemData.Color = colour;
                            }
                            else if (item1["pName"].ToString().Trim() == "Size")
                            {
                                var size = item1["pValue"].ToString();
                                AliExpressOrderItemData.Size = size;
                            }
                        }
                    }
                    datacontext.aliexpressorderitems.Add(AliExpressOrderItemData);
                    datacontext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }
    }
}
