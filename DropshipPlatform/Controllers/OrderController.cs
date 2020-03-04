using DropshipPlatform.BLL.Models;
using DropshipPlatform.BLL.Services;
using DropshipPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DropshipPlatform.Controllers
{
    public class OrderController : Controller
    {
        // GET: Order
        public ActionResult Index()
        {
           
            return View();
        }

        public ActionResult getOrders()
        {
            
           
            return View();
        }

        [HttpPost]
        public ActionResult getOrdersData()
        {
            OrderService _orderService = new OrderService();
            int recordsTotal = 0;
            var draw = Request.Form.GetValues("draw") != null ? Request.Form.GetValues("draw").FirstOrDefault() : null;
            var start = Request.Form.GetValues("start") != null ? Request.Form.GetValues("start").FirstOrDefault() : null;
            var length = Request.Form.GetValues("length") != null ? Request.Form.GetValues("length").FirstOrDefault() : null;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            ResultData resultList = _orderService.getAllOrders();
            //List<OrderModel> retvalue = resultList;
            OrderData orderData = new OrderData();
            List<OrderData> retvalue = new List<OrderData>();
            if (resultList.target_list != null)
            {
                foreach (var item in resultList.target_list[0].order_dto)
                {
                    orderData.AliExpressOrderNumber = Convert.ToInt32(item.order_id);
                    orderData.AliExpressProductId = item.product_list.order_product_dto[0].product_id;
                    List<Product> productData = _orderService.GetProductById(orderData.AliExpressProductId.ToString());
                    orderData.OrignalProductId = productData[0].OriginalProductID;
                    orderData.OrignalProductLink = productData[0].SourceWebsite;
                    orderData.ProductTitle = item.product_list.order_product_dto[0].product_name;
                    orderData.OrderAmount = item.product_list.order_product_dto[0].total_product_amount.amount;
                    orderData.DeleveryCountry = null;
                    orderData.ShippingWeight = productData[0].ShippingWeight;
                    orderData.OrderStatus = "New Order";
                    orderData.PaymentStatus = "UnPaid";
                    orderData.SellerID = item.seller_login_id;
                    orderData.SellerEmail = null;
                    retvalue.Add(orderData);
                }
            }
            var data = new List<OrderData>();
            //var data = new List<OrderModel>();
            if (pageSize != -1)
            {
                data = retvalue.Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                data = retvalue.ToList();
            }
            recordsTotal = retvalue.Count();

            var jsonResult = Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}