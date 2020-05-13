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
    public class RegistrationController : Controller
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: Registration
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(RegisterUserModel model)
        {
            ResponseModel response = new ResponseModel();
            ViewBag.Message = String.Empty;

            try
            {
                UserService userService = new UserService();
                response = userService.RegisterUser(model);
                if (response.IsSuccess)
                {
                    if (model.RoleID == 2)
                    {
                        return RedirectToAction("UserManagement", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Login");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            ViewBag.Message = response.Message;
            return View();
        }

        [HttpPost]
        [CustomAuthorize("Admin")]
        public ActionResult getOperationalUsers()
        {
            UserService userService = new UserService();
            int recordsTotal = 0;
            var draw = Request.Form.GetValues("draw") != null ? Request.Form.GetValues("draw").FirstOrDefault() : null;
            var start = Request.Form.GetValues("start") != null ? Request.Form.GetValues("start").FirstOrDefault() : null;
            var length = Request.Form.GetValues("length") != null ? Request.Form.GetValues("length").FirstOrDefault() : null;
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            List<RegisterUserModel> retvalue = userService.getOperationalUsers();

            var data = new List<RegisterUserModel>();
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
        [CustomAuthorize("Admin")]
        public JsonResult deleteOperationalManager(int UserID)
        {
            UserService userService = new UserService();
            return Json(userService.deleteOperationalManager(UserID), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CheckEmailExist(string emailId)
        {
            UserService userService = new UserService();
            return Json(userService.CheckEmailExist(emailId), JsonRequestBehavior.AllowGet);
        }

    }
}