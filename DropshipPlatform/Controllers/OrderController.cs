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
        // GET: Order
        [CustomAuthorize("Admin", "Seller", "Developer")]
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
            _orderService.getLogisticsServicByOrderId();
            int recordsTotal = 0;
            var draw = Request.Form.GetValues("draw") != null ? Request.Form.GetValues("draw").FirstOrDefault() : null;
            var start = Request.Form.GetValues("start") != null ? Request.Form.GetValues("start").FirstOrDefault() : null;
            var length = Request.Form.GetValues("length") != null ? Request.Form.GetValues("length").FirstOrDefault() : null;
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            List<OrderData> retvalue = _orderService.getAllOrdersFromDatabase();

            if (!string.IsNullOrEmpty(search))
            {
                retvalue = retvalue.Where(x => x.AliExpressOrderNumber != null && x.AliExpressOrderNumber.ToLower().Contains(search.ToLower()) ||
                x.AliExpressProductId != null && x.AliExpressProductId.ToString().ToLower().Contains(search.ToLower()) ||
                x.OrignalProductId != null && x.OrignalProductId.ToString().ToLower().Contains(search.ToLower()) ||
                x.OrignalProductLink != null && x.OrignalProductLink.ToString().ToLower().Contains(search.ToLower())
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
        public JsonResult trackingOrder(OrderData orderData)
        {
            OrderService _orderService = new OrderService();
            return Json(_orderService.trackingOrder(orderData), JsonRequestBehavior.AllowGet);
        }
    }
}