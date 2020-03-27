using DropshipPlatform.BLL.Models;
using DropshipPlatform.BLL.Services;
using DropshipPlatform.Entity;
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
        // GET: Products
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult PickupProducts()
        {
            ViewBag.AliCategory = new CategoryService().getCategories();
            return View();
        }

        [HttpPost]
        public JsonResult getProductManagementDT(int? category,int? filterOptions)
        {
            user user = SessionManager.GetUserSession();
            DTRequestModel DTRequestModel = new DTRequestModel();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            DTRequestModel.PageSize = length != null ? Convert.ToInt32(length) : 0;
            DTRequestModel.Skip = start != null ? Convert.ToInt32(start) : 0;
            DTRequestModel.SortBy = sortColumn + " " + sortColumnDir;
            int recordsTotal = 0;
            List<ProductGroupModel> list = _productService.GetParentProducts(user.UserID, DTRequestModel, out recordsTotal);



            //SORT
            //if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            //{
            //    list = list.OrderBy(sortColumn + " " + sortColumnDir).ToList();
            //}
            //if (category > 0)
            //{
            //    list = list.Where(x => x.ParentProduct.CategoryID == category).ToList();
            //}
            //if (filterOptions == 1)
            //{
            //    list = list.Where(x => x.ParentProduct.UserID == user.UserID).ToList();
            //    list = list.Where(x => x.ParentProduct.isProductPicked == true).ToList();
            //}
            //else if (filterOptions == 2)
            //{
            //    list = list.Where(x => x.ParentProduct.UserID != user.UserID).ToList();
            //    list = list.Where(x => x.ParentProduct.hasProductSkuSync == false && x.ParentProduct.isProductPicked == false).ToList();
            //}
            //else if (filterOptions == 3)
            //{
            //    list = list.Where(x => x.ParentProduct.UserID == user.UserID).ToList();
            //    list = list.Where(x => x.ParentProduct.hasProductSkuSync == true && x.ParentProduct.isProductPicked == false).ToList();
            //}

            //recordsTotal = list.Count();
            var data = list.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult pickSellerProducts(List<scproductModel> products)
        {
            user user = SessionManager.GetUserSession();
            
                bool result = _productService.AddSellersPickedProducts(products, user.UserID);
            
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult MyProduct()
        {
            //if (SessionManager.GetAccessToken().access_token == null)
            //{
            //    return RedirectToAction("Index", "AliExpress");
            //}
            ViewBag.AliCategory = new CategoryService().getCategories();
            return View();
        }

        public JsonResult getPickedAliProducts(int? category)
        {
            List<ProductGroupModel> list = _productService.GetPickedProducts(SessionManager.GetUserSession().UserID);
            if (category > 0)
            {
                list = list.Where(x => x.ParentProduct.CategoryID == category).ToList();
            }
            return Json(new { data = list.ToArray(), }, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public JsonResult UpdatePickedProduct(List<UpdateProductModel> UpdatedModels)
        //{          
        //    bool result = false;
        //    result = _productService.UpdatePickedProduct(UpdatedModels, SessionManager.GetUserSession());
        //    //string pId = _productService.SyncWithAliExpress();
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}
        [HttpPost]
        //public JsonResult Up
        public JsonResult UpdatePickedProduct(List<scproductModel> products)
        {
            user user = SessionManager.GetUserSession();

            bool result = _productService.UpdatePickedProduct(products, user.UserID);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult checkResultByJobId(long id)
        {
            ProductService _productService = new ProductService();
            string result = _productService.checkResultByJobId(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult updateProductStatuts(string id,bool status)
        {
            ProductService _productService = new ProductService();
            return Json(_productService.updateProductStatuts(id, status), JsonRequestBehavior.AllowGet);
        }
    }
}