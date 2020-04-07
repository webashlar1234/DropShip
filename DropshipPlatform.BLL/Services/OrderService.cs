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
                                StripeService _stripeservice = new StripeService();
                                string amount = item.PayAmount != null ? item.PayAmount.Amount : null;
                                string stripeCustomerId = datacontext.users.Where(x => x.AliExpressLoginID == item.SellerLoginId).Select(x => x.StripeCustomerID).FirstOrDefault();
                                bool resultStripePayment = _stripeservice.ChargeSavedCard(stripeCustomerId, Convert.ToInt64(amount));
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
                                OrderData.SellerPaymentStatus = resultStripePayment;
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
            List<OrderData> OrdersList = new List<OrderData>();
            List<OrderData> Orders = new List<OrderData>();
            List<OrderViewModel> ChildOrders = new List<OrderViewModel>();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    Orders = (from o in datacontext.orders
                              join u in datacontext.users on o.AliExpressLoginID equals u.AliExpressLoginID
                              where u.IsActive == true
                              select new OrderData
                              {
                                  AliExpressOrderID = o.AliExpressOrderID,
                                  AliExpressOrderNumber = o.AliExpressOrderID,
                                  OrderAmount = o.OrderAmount,
                                  DeleveryCountry = o.DeliveryCountry,
                                  ShippingWeight = o.ShippingWeight,
                                  OrderStatus = o.OrderStatus,
                                  PaymentStatus = o.PaymentStatus,
                                  SellerID = u.AliExpressSellerID,
                                  SellerEmail = u.EmailID
                              }).ToList();

                    List<OrderViewModel> childList = (from o in datacontext.orders
                                                      join oi in datacontext.aliexpressorderitems on o.AliExpressOrderID equals oi.AliExpressOrderId.ToString()
                                                      from sp in datacontext.sellerspickedproducts.Where(x => x.AliExpressProductID == oi.AliExpressProductID).DefaultIfEmpty()
                                                      join p in datacontext.products on sp.ParentProductID equals p.ProductID
                                                      where o.AliExpressOrderID != null
                                                      select new OrderViewModel
                                                      {
                                                          AliExpressOrderId = oi.AliExpressOrderId.ToString(),
                                                          AliExpressProductId = oi.AliExpressProductID,
                                                          OrignalProductId =  p.OriginalProductID,
                                                          OrignalProductLink = p.SourceWebsite,
                                                          ProductName = oi.ProductName,
                                                          Price = oi.Price,
                                                          Colour = p.Color,
                                                          Size = p.Size
                                                      }).ToList();

                    if (Orders.Count > 0)
                    {
                        foreach (OrderData OrderdataModel in Orders)
                        {
                            OrderData OrderGroup = new OrderData();

                            OrderdataModel.ChildOrderItemList = childList.Where(x => x.AliExpressOrderId == OrderdataModel.AliExpressOrderID).ToList();

                            OrdersList.Add(OrderdataModel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return OrdersList;
        }


        public bool getLogisticsServicByOrderId(long OrderID)
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
                req.OrderId = OrderID; //required OrderId
                AliexpressLogisticsRedefiningGetonlinelogisticsservicelistbyorderidResponse rsp = client.Execute(req, SessionManager.GetAccessToken().access_token);
                resultdata = JsonConvert.SerializeObject(rsp.ResultList);
                orderLogisticData = JsonConvert.DeserializeObject<List<LogisticServiceData>>(resultdata);
                createWarehouseOrder(orderLogisticData, OrderID);
            }
            catch (Exception ex)
            {
                result = false;
                logger.Info(ex.ToString());
            }

            return result;
        }

        public bool createWarehouseOrder(List<LogisticServiceData> orderLogisticData, long OrderID)
        {
            bool result = true;
            string resultdata = String.Empty;
            try
            {
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);

                AliexpressSolutionOrderInfoGetRequest req1 = new AliexpressSolutionOrderInfoGetRequest();
                AliexpressSolutionOrderInfoGetRequest.OrderDetailQueryDomain obj = new AliexpressSolutionOrderInfoGetRequest.OrderDetailQueryDomain();
                obj.ExtInfoBitFlag = 11111L;
                obj.OrderId = OrderID;
                req1.Param1_ = obj;
                AliexpressSolutionOrderInfoGetResponse rsp1 = client.Execute(req1, SessionManager.GetAccessToken().access_token);
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
                AliexpressLogisticsCreatewarehouseorderResponse rsp = client.Execute(req, SessionManager.GetAccessToken().access_token);
                string res = JsonConvert.SerializeObject(rsp.Result);
                getInternationalLogisticNoByOrderId(OrderID);
            }
            catch (Exception ex)
            {
                result = false;
                logger.Info(ex.ToString());
            }

            return result;
        }

        public bool getInternationalLogisticNoByOrderId(long OrderID)
        {
            bool result = true;
            string resultdata = String.Empty;
            try
            {
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);
                AliexpressLogisticsQuerylogisticsorderdetailRequest req = new AliexpressLogisticsQuerylogisticsorderdetailRequest();
                req.TradeOrderId = OrderID; //required OrderId
                AliexpressLogisticsQuerylogisticsorderdetailResponse rsp = client.Execute(req, SessionManager.GetAccessToken().access_token);
                resultdata = JsonConvert.SerializeObject(rsp.Result);

                if (rsp.Result.Success)
                {
                    List<AliexpressLogisticsQuerylogisticsorderdetailResponse.AeopLogisticsOrderDetailDtoDomain> OrderDetailDto = rsp.Result.ResultList;
                    getPrintInfo(OrderDetailDto.FirstOrDefault().InternationalLogisticsNum);
                }
            }
            catch (Exception ex)
            {
                result = false;
                logger.Info(ex.ToString());
            }

            return result;
        }

        public bool getPrintInfo(string international_logistics_id)
        {
            bool result = true;
            string resultdata = String.Empty;
            try
            {
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);
                AliexpressLogisticsRedefiningGetprintinfoRequest req = new AliexpressLogisticsRedefiningGetprintinfoRequest();
                req.InternationalLogisticsId = international_logistics_id; //required InternationalLogisticsId from getInternationalLogisticNoByOrderId result
                AliexpressLogisticsRedefiningGetprintinfoResponse rsp = client.Execute(req, SessionManager.GetAccessToken().access_token);
                resultdata = JsonConvert.SerializeObject(rsp.Result);

                LogisticPrintinfo LogisticPrintinfo = JsonConvert.DeserializeObject<LogisticPrintinfo>(rsp.Result);

                byte[] bytes = Convert.FromBase64String(LogisticPrintinfo.body);

                var path = StaticValues.CainiaoFiles_path + international_logistics_id + ".pdf";

                System.IO.FileStream stream =
                new FileStream(path, FileMode.CreateNew);
                System.IO.BinaryWriter writer =
                    new BinaryWriter(stream);
                writer.Write(bytes, 0, bytes.Length);
                writer.Close();
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
