using DropshipPlatform.BLL.Models;
using DropshipPlatform.BLL.Services;
using DropshipPlatform.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

        public JsonResult getProductManagementDT(int? category, int? filterOptions)
        {
            User user = SessionManager.GetUserSession();
            List<ProductViewModel> list = _productService.GetParentProducts(user.UserID);
            if (category > 0)
            {
                list = list.Where(x => x.CategoryID == category).ToList();
            }
            if (filterOptions == 1)
            {
                list = list.Where(x => x.UserID == user.UserID).ToList();
            }
            else if(filterOptions == 2)
            {
                list = list.Where(x => x.UserID != user.UserID).ToList();
            }          
            return Json(new
            {
                data = list.ToArray(),
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult pickSellerProducts(List<simpleModel> products)
        {
            User user = SessionManager.GetUserSession();
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

        public JsonResult getPickedAliProducts(int UserID = 1)
        {
            List<ProductGroupModel> list = _productService.GetPickedProducts(UserID);
            return Json(new { data = list.ToArray(), }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdatePickedProduct(List<UpdateProductModel> UpdatedModels)
        {          
            bool result = false;
            result = _productService.UpdatePickedProduct(UpdatedModels);
            //string pId = _productService.SyncWithAliExpress();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult checkResultByJobId(long id)
        {
            ProductService _productService = new ProductService();
            string result = _productService.checkResultByJobId(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}