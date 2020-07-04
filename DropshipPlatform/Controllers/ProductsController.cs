using DropshipPlatform.BLL.Models;
using DropshipPlatform.BLL.Services;
using DropshipPlatform.Entity;
using DropshipPlatform.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;

namespace DropshipPlatform.Controllers
{
    public class ProductsController : Controller
    {
        ProductService _productService = new ProductService();
       
        [CustomAuthorize("Admin", "Operational Manager", "Seller", "Developer")]
        public ActionResult PickupProducts()
        {
            ViewBag.AliCategory = new CategoryService().getCategoriesOnlyAvailableProd();
            return View();
        }

        [HttpPost]
        [AjaxFilter]
        public JsonResult getProductManagementDT(int? category, int? filterOptions)
        {
            LoggedUserModel user = SessionManager.GetUserSession();
            DTRequestModel DTRequestModel = new DTRequestModel();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][data]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();

            DTRequestModel.PageSize = length != null ? Convert.ToInt32(length) : 0;
            DTRequestModel.Skip = start != null ? Convert.ToInt32(start) : 0;
            DTRequestModel.SortBy = (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir)) ? sortColumn + " " + sortColumnDir : "";
            DTRequestModel.Search = search;
            int recordsTotal = 0;
            List<ProductGroupModel> list = _productService.GetParentProducts(user.UserID,user.LoggedUserRoleName, DTRequestModel, category, filterOptions, out recordsTotal);
            
            var data = list.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data }, JsonRequestBehavior.AllowGet);
        }

        [AjaxFilter]
        public JsonResult pickSellerProducts(List<scproductModel> products)
        {
            LoggedUserModel user = SessionManager.GetUserSession();

            bool result = _productService.AddSellersPickedProducts(products, user.UserID);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize("Seller", "Developer")]
        public ActionResult MyProduct()
        {
            //if (SessionManager.GetAccessToken().access_token == null)
            //{
            //    return RedirectToAction("Index", "AliExpress");
            //}
            ViewBag.AliCategory = new CategoryService().getCategoriesOnlyAvailableProd();
            return View();
        }

        [AjaxFilter]
        public JsonResult getPickedAliProducts(int? category)
        {
            List<ProductGroupModel> list = _productService.GetPickedProducts(SessionManager.GetUserSession().UserID);
            if (category > 0)
            {
                list = list.Where(x => x.ParentProduct.CategoryID == category).ToList();
            }
            return Json(new { data = list.ToArray(), }, JsonRequestBehavior.AllowGet);
        }

        
        [HttpPost]
        [AjaxFilter]
        public JsonResult UpdatePickedProduct(List<scproductModel> products)
        {
            LoggedUserModel user = SessionManager.GetUserSession();

            bool result = _productService.UpdatePickedProduct(products, user.UserID);

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [AjaxFilter]
        public JsonResult updateProductStatuts(string id, bool status)
        {
            ProductService _productService = new ProductService();
            return Json(_productService.updateProductStatuts(id, status), JsonRequestBehavior.AllowGet);
        }
    }
}