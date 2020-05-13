using DropshipPlatform.BLL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using DropshipPlatform.BLL.Models;
using DropshipPlatform.Entity;
using DropshipPlatform.Infrastructure;

namespace DropshipPlatform.Controllers
{
    public class UserController : Controller
    {
        UserService _userService = new UserService();
        // GET: User

        [CustomAuthorize("Admin", "Operational Manager", "Developer")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [AjaxFilter]
        public ActionResult getUserDatatable()
        {
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

            List<user> Userlist = _userService.getSellerUsers();
          
            List<user> result = Userlist.Select(x => new user()
            {
                Name = x.Name,
                EmailID = x.EmailID,
                Phone = x.Phone,
                AliExpressSellerID = x.AliExpressSellerID,
                AliExpressLoginID = x.AliExpressLoginID,
                StripeCustomerID = x.StripeCustomerID,
                IsActive = x.IsActive,
                IsPolicyAccepted = x.IsPolicyAccepted,
                ItemCreatedWhen = DateTime.UtcNow
            }).ToList();


            var retvalue = result;

            if (!string.IsNullOrEmpty(search))
            {
                retvalue = retvalue.Where(x => x.Name != null && x.Name.ToLower().Contains(search.ToLower()) ||
                x.EmailID != null && x.EmailID.ToString().ToLower().Contains(search.ToLower()) ||
                x.AliExpressSellerID != null && x.AliExpressSellerID.ToString().ToLower().Contains(search.ToLower())
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

            var data = new List<user>();

            if (pageSize != -1)
            {
                data = retvalue.Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                data = retvalue.ToList();
            }

            var jsonResult = Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}