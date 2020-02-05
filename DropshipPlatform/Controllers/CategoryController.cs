using DropshipPlatform.BLL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using DropshipPlatform.BLL.Models;
using DropshipPlatform.DLL;

namespace DropshipPlatform.Controllers
{
    public class CategoryController : Controller
    {
        CategoryService _categoryService = new CategoryService();
        // GET: Category
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult getCategoryDatatable(string ddlMappingValue) {
            var draw = Request.Form.GetValues("draw") != null ? Request.Form.GetValues("draw").FirstOrDefault() : null;
            var start = Request.Form.GetValues("start") != null ? Request.Form.GetValues("start").FirstOrDefault() : null;
            var length = Request.Form.GetValues("length") != null ? Request.Form.GetValues("length").FirstOrDefault() : null;
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("order[0][column]") != null ? Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][data]").FirstOrDefault() : null;
            var sortColumnDir = Request.Form.GetValues("order[0][dir]") != null ? Request.Form.GetValues("order[0][dir]").FirstOrDefault() : null;

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            string sortOrder = "";

            List<Category> Categorylist = _categoryService.getCategories();

            List<CategoryData> result = Categorylist.Where(x => x.IsLeafCategory == true).Select(x => new CategoryData() {
                CategoryID = x.CategoryID,
                Name = x.Name,
                CategoryLevel = x.CategoryLevel,
                ParentCategoryID = x.ParentCategoryID,
                IsLeafCategory = x.IsLeafCategory,
                AliExpressCategoryName = x.AliExpressCategoryName,
                AliExpressCategoryId = x.AliExpressCategoryId,
                categoryFullPath = x.Name + getCategoryFullPath(Categorylist, x.ParentCategoryID, x.CategoryLevel)
            }).ToList();
            
            if(ddlMappingValue == "mapped")
            {
                result = result.Where(x => x.AliExpressCategoryId > 0).ToList();
            }
            else if (ddlMappingValue == "unmapped")
            {
                result = result.Where(x => x.AliExpressCategoryId == null).ToList();
            }
            var retvalue = result;

            if (!string.IsNullOrEmpty(search))
            {
                retvalue = retvalue.Where(x => x.Name != null && x.Name.ToLower().Contains(search.ToLower()) ||
                x.AliExpressCategoryName != null && x.AliExpressCategoryName.ToString().ToLower().Contains(search.ToLower())
                ).ToList();
            }
            if ((!string.IsNullOrEmpty(sortColumn)) && (!string.IsNullOrEmpty(sortColumnDir)))
            {
                sortOrder = sortColumn + " " + sortColumnDir + "," + Request.Form.GetValues("order[0][column]").FirstOrDefault();
            }
            if (!string.IsNullOrEmpty(sortOrder))
            {
                string orderBy = sortOrder.Split(',')[0];
                retvalue = retvalue.OrderBy(orderBy).ToList();
            }

            recordsTotal = retvalue.Count();
            var data = new List<CategoryData>();
            if (pageSize != -1)
            {
                data = retvalue.Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                data = retvalue.ToList();
            }

            List<AliExpressCategory> AliCategory = _categoryService.getlocalAliExpressCategories();
            List<AliExpressCategory> list = AliCategory.Where(x =>  x.AliExpressCategoryIsLeaf == true).ToList();
            foreach (AliExpressCategory item in list) {
                item.AliCategoryFullPath = item.AliExpressCategoryName +  getAliCategoryFullPath(AliCategory, item.AliExpressParentCategoryID, item.AliExpressCategoryLevel);
            }

            var jsonResult = Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data, aliCategory = list }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

            //List<AliExpressCategory> list = _categoryService.getlocalAliExpressCategories();

            //return Json(new
            //{
            //    data = _categoryService.getCategories().ToArray(), aliCategory = list
            //}, JsonRequestBehavior.AllowGet);
        }

        public string getCategoryFullPath(List<Category> list, string categoryID, int? level) {

            string fullCategoryPath = "";
            do
            {
                Category obj = list.Where(x => x.CategoryID == Int32.Parse(categoryID)).FirstOrDefault();
                level = obj.CategoryLevel;
                categoryID = obj.ParentCategoryID;
                fullCategoryPath += " --> "+ obj.Name;
            }
            while (level > 1);
            
            return fullCategoryPath;
        }
        public string getAliCategoryFullPath(List<AliExpressCategory> list, long? categoryID, long level)
        {

            string fullCategoryPath = "";
            do
            {
                AliExpressCategory obj = list.Where(x => x.AliExpressCategoryID == categoryID).FirstOrDefault();
                level = obj.AliExpressCategoryLevel;
                categoryID = obj.AliExpressParentCategoryID;
                fullCategoryPath += " --> " + obj.AliExpressCategoryName;
            }
            while (level > 1);

            return fullCategoryPath;
        }

        [HttpPost]
        public JsonResult MapCategory(Category category) {
            return Json(_categoryService.MapCategory(category), JsonRequestBehavior.AllowGet);
        }
    }
}