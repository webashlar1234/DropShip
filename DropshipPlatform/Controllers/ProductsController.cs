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
    public class ProductsController : Controller
    {
        ProductService _productService = new ProductService();
        // GET: Products
        public ActionResult Index()
        {
            ViewBag.AliCategory = new CategoryService().getCategories();
            return View();
        }

        public JsonResult getProductManagementDT(int? category)
        {

            List<Product> list = _productService.GetParentProducts();
            if (category > 0)
            {
                list = list.Where(x => x.CategoryID == category).ToList();
            }
            return Json(new
            {
                data = list.ToArray(),
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult pickSellerProducts(int[] products)
        {

            int UserID = 1;
            bool result = _productService.AddSellersPickedProducts(products, UserID);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PickupProduct()
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
    }
}