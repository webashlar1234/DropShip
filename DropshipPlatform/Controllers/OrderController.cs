using DropshipPlatform.BLL.Models;
using DropshipPlatform.BLL.Services;
using DropshipPlatform.Entity;
using DropshipPlatform.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DropshipPlatform.Controllers
{
    public class OrderController : Controller
    {
        [CustomAuthorize("Admin", "Operational Manager","Seller", "Developer")]
        public ActionResult getOrders()
        {
            OrderService _orderService = new OrderService();
            //_orderService.getLogisticsServicByOrderId(1000102673302508);
            //_orderService.getPrintInfo("AEFP0000134813RU2");

            return View();
        }

        [HttpPost]
        [AjaxFilter]
        public ActionResult getOrdersData(string orderStatus, int? sellerPaymentStatus)
        {
            OrderService _orderService = new OrderService();
            int recordsTotal = 0;
            var draw = Request.Form.GetValues("draw") != null ? Request.Form.GetValues("draw").FirstOrDefault() : null;
            var start = Request.Form.GetValues("start") != null ? Request.Form.GetValues("start").FirstOrDefault() : null;
            var length = Request.Form.GetValues("length") != null ? Request.Form.GetValues("length").FirstOrDefault() : null;
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            LoggedUserModel user = SessionManager.GetUserSession();
            int UserID = 0;
            if (user.LoggedUserRoleName != null)
            {
                if (user.LoggedUserRoleName == StaticValues.seller)
                {
                    UserID = user.UserID;
                }
            }

            List<OrderData> retvalue = _orderService.getAllOrdersFromDatabase(UserID);

            if (orderStatus != "All")
            {
                retvalue = retvalue.Where(x => x.OrderStatus == orderStatus).ToList();
            }
            if (sellerPaymentStatus == 1)
            {
                retvalue = retvalue.Where(x => x.SellerPaymentStatus == true).ToList();
            }
            else if (sellerPaymentStatus == 2)
            {
                retvalue = retvalue.Where(x => x.SellerPaymentStatus == false).ToList();
            }
            if (!string.IsNullOrEmpty(search))
            {
                retvalue = retvalue.Where(x => x.AliExpressOrderID != null && x.AliExpressOrderID.ToLower().Contains(search.ToLower()) ||
                x.SellerID != null && x.SellerID.ToString().ToLower().Contains(search.ToLower()) ||
                x.SellerEmail != null && x.SellerEmail.ToString().ToLower().Contains(search.ToLower())
                ).ToList();
            }
            var data = new List<OrderData>();
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

        [HttpPost]
        [CustomAuthorize("Admin", "Operational Manager", "Developer")]
        public JsonResult FullFillAliExpressOrder(OrderData orderData, bool isFullShip)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(orderData.TrackingNumber) && !string.IsNullOrEmpty(orderData.AliExpressOrderNumber))
            {
                OrderService _orderService = new OrderService();
                result = _orderService.FullFillAliExpressOrder(orderData, isFullShip);

            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomAuthorize("Admin", "Operational Manager", "Developer")]
        public JsonResult BuyOrderFromSourceWebsite(string OrderID)
        {
            OrderService _orderService = new OrderService();
            return Json(_orderService.BuyOrderFromSourceWebsite(OrderID), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomAuthorize("Seller")]
        public JsonResult PayForOrderBySeller(string OrderID)
        {
            OrderService _orderService = new OrderService();
            LoggedUserModel user = SessionManager.GetUserSession();
            return Json(_orderService.PayForOrderBySeller(OrderID, user.UserID), JsonRequestBehavior.AllowGet);
        }
    }
}