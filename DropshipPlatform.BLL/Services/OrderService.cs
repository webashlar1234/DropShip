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
                                AliExpressOrderData.AliExpressLoginID = item.SellerLoginId;
                                AliExpressOrderData.DeliveryCountry = null;
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
                                    sellerspickedproduct productData = GetProductById(AliExpressOrderItemData.AliExpressProductID);

                                    if (productData != null)
                                    {
                                        AliExpressOrderItemData.ProductID = Convert.ToInt64(productData.ParentProductID);
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
                                OrderData.DeliveryCountry = null;
                                OrderData.ShippingWeight = null;
                                OrderData.OrderAmount = item.PayAmount != null ? item.PayAmount.Amount : null;
                                OrderData.OrderStatus = item.OrderStatus;
                                OrderData.PaymentStatus = item.FundStatus;
                                OrderData.AliExpressLoginID = item.SellerLoginId;
                                OrderData.ItemCreatedBy = null;
                                OrderData.ItemCreatedWhen = DateTime.UtcNow;
                                OrderData.ItemModifyBy = null;
                                OrderData.ItemModifyWhen = DateTime.UtcNow;
                                datacontext.orders.Add(OrderData);
                            }
                            else
                            {
                                AliExpressOrderData = datacontext.aliexpressorders.Where(x => x.AliExpressOrderID == item.OrderId).FirstOrDefault();

                                AliExpressOrderData.AliExpressOrderID = item.OrderId;
                                AliExpressOrderData.BuyerLoginId = item.BuyerLoginId;
                                AliExpressOrderData.AliExpressLoginID = item.SellerLoginId;
                                AliExpressOrderData.DeliveryCountry = null;
                                AliExpressOrderData.PaymentStatus = null;
                                AliExpressOrderData.TrackingNo = null;
                                AliExpressOrderData.ShippingWeight = null;
                                AliExpressOrderData.OrderAmount = null;
                                AliExpressOrderData.CurrencyCode = null;
                                AliExpressOrderData.OrderStatus = item.OrderStatus;
                                AliExpressOrderData.NoOfOrderItems = null;
                                AliExpressOrderData.ItemModifyWhen = DateTime.UtcNow;
                                AliExpressOrderData.AliExpressOrderUpdatedTime = item.GmtUpdate;
                                datacontext.Entry(AliExpressOrderData).State = System.Data.Entity.EntityState.Modified;

                                foreach (var product in item.ProductList)
                                {
                                    AliExpressOrderItemData = datacontext.aliexpressorderitems.Where(x => x.AliExpressProductID == product.ProductId.ToString()).FirstOrDefault();
                                    if (AliExpressOrderItemData != null)
                                    {
                                        AliExpressOrderItemData.ProductName = product.ProductName;
                                        AliExpressOrderItemData.ProductName = product.ProductName;
                                        AliExpressOrderItemData.Price = product.TotalProductAmount.Amount;
                                        AliExpressOrderItemData.CurrencyCode = product.TotalProductAmount.CurrencyCode;
                                        AliExpressOrderItemData.ItemModifyWhen = DateTime.UtcNow;
                                        AliExpressOrderItemData.AliExpressOrderId = item.OrderId;
                                        AliExpressOrderItemData.AliExpressProductOrderId = null;
                                        AliExpressOrderItemData.SCOrderId = null;
                                        datacontext.Entry(AliExpressOrderItemData).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    else
                                    {
                                        datacontext.aliexpressorderitems.Remove(AliExpressOrderItemData);
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

        public List<OrderData> getAllOrdersFromDatabase()
        {
            List<OrderData> Orders = new List<OrderData>();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    Orders = (from o in datacontext.orders
                              join u in datacontext.users on o.AliExpressLoginID equals u.AliExpressLoginID
                              where u.IsActive == true
                              select new OrderData
                              {
                                  AliExpressOrderNumber = o.AliExpressOrderID,
                                  OrderAmount = o.OrderAmount,
                                  DeleveryCountry = o.DeliveryCountry,
                                  ShippingWeight = o.ShippingWeight,
                                  OrderStatus = o.OrderStatus,
                                  PaymentStatus = o.PaymentStatus,
                                  SellerID = u.AliExpressSellerID,
                                  SellerEmail = u.EmailID
                              }).ToList();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return Orders;
        }


        public bool getLogisticsServicByOrderId()
        {
            bool result = true;
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
                req.OrderId = 1000102673302508; //required OrderId
                AliexpressLogisticsRedefiningGetonlinelogisticsservicelistbyorderidResponse rsp = client.Execute(req, SessionManager.GetAccessToken().access_token);
                resultdata = JsonConvert.SerializeObject(rsp.ResultList);
                orderLogisticData = JsonConvert.DeserializeObject<List<LogisticServiceData>>(resultdata);
                createWarehouseOrder(orderLogisticData);
            }
            catch (Exception ex)
            {
                result = false;
                logger.Info(ex.ToString());
            }

            return result;
        }

        public bool createWarehouseOrder(List<LogisticServiceData> orderLogisticData)
        {
            bool result = true;
            string resultdata = String.Empty;
            try
            {
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);

                AliexpressSolutionOrderInfoGetRequest req1 = new AliexpressSolutionOrderInfoGetRequest();
                AliexpressSolutionOrderInfoGetRequest.OrderDetailQueryDomain obj = new AliexpressSolutionOrderInfoGetRequest.OrderDetailQueryDomain();
                obj.ExtInfoBitFlag = 11111L;
                obj.OrderId = 1000102673302508;
                req1.Param1_ = obj;
                AliexpressSolutionOrderInfoGetResponse rsp1 = client.Execute(req1, SessionManager.GetAccessToken().access_token);

                //Create Warehouse Order
                AliexpressLogisticsCreatewarehouseorderRequest req = new AliexpressLogisticsCreatewarehouseorderRequest();
                AliexpressLogisticsCreatewarehouseorderRequest.AddressdtosDomain obj1 = new AliexpressLogisticsCreatewarehouseorderRequest.AddressdtosDomain();
                AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareAddressDtoDomain obj2 = new AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareAddressDtoDomain();
                obj2.Phone = "098-234234";
                obj2.Fax = "234234234";
                obj2.MemberType = "类型";
                obj2.TrademanageId = "cn234234234";
                obj2.Street = "street";
                obj2.Country = "RU";
                obj2.City = "Moscow";
                obj2.County = "county";
                obj2.Email = "alibaba@alibaba.com";
                obj2.AddressId = -1;
                obj2.Name = "Linda";
                obj2.Province = "Moscow";
                obj2.StreetAddress = "street address";
                obj2.Mobile = "18766234324";
                obj2.PostCode = "056202";
                obj1.Sender = obj2;
                AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareAddressDtoDomain obj3 = new AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareAddressDtoDomain();
                obj3.Phone = "098-234234";
                obj3.Fax = "234234234";
                obj3.MemberType = "类型";
                obj3.TrademanageId = "cn234234234";
                obj3.Street = "street";
                obj3.Country = "RU";
                obj3.City = "Moscow";
                obj3.County = "county";
                obj3.Email = "alibaba@alibaba.com";
                obj3.AddressId = -1;
                obj3.Name = "Linda";
                obj3.Province = "Moscow";
                obj3.StreetAddress = "street address";
                obj3.Mobile = "18766234324";
                obj3.PostCode = "056202";
                obj1.Pickup = obj3;
                AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareAddressDtoDomain obj4 = new AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareAddressDtoDomain();
                obj4.Phone = "098-234234";
                obj4.Fax = "234234234";
                obj4.MemberType = "类型";
                obj4.TrademanageId = "cn234234234";
                obj4.Street = "street";
                obj4.PostCode = "056202";
                obj4.Country = "RU";
                obj4.City = "Moscow";
                obj4.County = "county";
                obj4.Email = "alibaba@alibaba.com";
                obj4.AddressId = -1;
                obj4.Name = "Linda";
                obj4.Province = "Moscow";
                obj4.StreetAddress = "street address";
                obj4.Mobile = "18766234324";
                obj1.Receiver = obj4;
                AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareAddressDtoDomain obj5 = new AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareAddressDtoDomain();
                obj5.Phone = "098-234234";
                obj5.Fax = "234234234";
                obj5.MemberType = "类型";
                obj5.TrademanageId = "cn234234234";
                obj5.Street = "street";
                obj5.Country = "RU";
                obj5.City = "Moscow";
                obj5.County = "county";
                obj5.Email = "alibaba@alibaba.com";
                obj5.AddressId = -1;
                obj5.Name = "Linda";
                obj5.Province = "Moscow";
                obj5.StreetAddress = "street address";
                obj5.Mobile = "18766234324";
                obj5.PostCode = "056202";
                obj1.Refund = obj5;
                req.AddressDTOs_ = obj1;
                List<AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareProductForTopDtoDomain> list7 = new List<AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareProductForTopDtoDomain>();
                AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareProductForTopDtoDomain obj8 = new AliexpressLogisticsCreatewarehouseorderRequest.AeopWlDeclareProductForTopDtoDomain();
                list7.Add(obj8);
                obj8.AneroidMarkup = false;
                obj8.Breakable = false;//required
                obj8.CategoryCnDesc = "连衣裙";//required
                obj8.CategoryEnDesc = "dress";//required
                obj8.ContainsBattery = false;//required
                obj8.HsCode = "77234";
                obj8.OnlyBattery = false;
                obj8.ProductDeclareAmount = "1.3";//required
                obj8.ProductId = 10000124511021L;//required
                obj8.ProductNum = 2L;//required
                obj8.ProductWeight = "1.5";//required
                obj8.ScItemCode = "scItem code";
                //obj8.ScItemId = 1000L;
                obj8.ScItemName = "scItem name";
                obj8.SkuCode = "sku code";//required
                obj8.SkuValue = "sku value";//required
                req.DeclareProductDTOs_ = list7;
                req.DomesticLogisticsCompany = orderLogisticData[0].LogisticsServiceName;
                req.DomesticLogisticsCompanyId = 505L;//required
                req.DomesticTrackingNo = "none";//required
                req.PackageNum = 1L;
                req.TradeOrderFrom = "ESCROW";
                req.TradeOrderId = 1000102673302508L;
                req.UndeliverableDecision = 0L;
                req.WarehouseCarrierService = orderLogisticData[0].LogisticsServiceId;
                // req.WarehouseCarrierService = "CPAM_WLB_FPXSZ;CPAM_WLB_CPHSH;CPAM_WLB_ZTOBJ;HRB_WLB_ZTOGZ;HRB_WLB_ZTOSH";
                req.InvoiceNumber = "38577123";
                //req.TopUserKey = "xxxxxxx";
                AliexpressLogisticsCreatewarehouseorderResponse rsp = client.Execute(req, SessionManager.GetAccessToken().access_token);
                Console.WriteLine(rsp.Body);
                getInternationalLogisticNoByOrderId();
            }
            catch (Exception ex)
            {
                result = false;
                logger.Info(ex.ToString());
            }

            return result;
        }

        public bool getInternationalLogisticNoByOrderId()
        {
            bool result = true;
            string resultdata = String.Empty;
            try
            {
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);
                AliexpressLogisticsQuerylogisticsorderdetailRequest req = new AliexpressLogisticsQuerylogisticsorderdetailRequest();
                req.TradeOrderId = 1000102673302508; //required OrderId
                AliexpressLogisticsQuerylogisticsorderdetailResponse rsp = client.Execute(req, SessionManager.GetAccessToken().access_token);
                resultdata = JsonConvert.SerializeObject(rsp.Result);
            }
            catch (Exception ex)
            {
                result = false;
                logger.Info(ex.ToString());
            }

            return result;
        }

        public bool getPrintInfo()
        {
            bool result = true;
            string resultdata = String.Empty;
            try
            {
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);
                AliexpressLogisticsRedefiningGetprintinfoRequest req = new AliexpressLogisticsRedefiningGetprintinfoRequest();
                req.InternationalLogisticsId = "1000102673302508"; //required InternationalLogisticsId from getInternationalLogisticNoByOrderId result
                AliexpressLogisticsRedefiningGetprintinfoResponse rsp = client.Execute(req, SessionManager.GetAccessToken().access_token);
                resultdata = JsonConvert.SerializeObject(rsp.Result);
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
