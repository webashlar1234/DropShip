using DropshipPlatform.BLL.Models;
using DropshipPlatform.BLL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DropshipPlatform.Controllers
{
    public class LoginController : Controller
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(LoginModel model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                UserService userService = new UserService();
                ViewBag.Message = String.Empty;
                response = userService.LoginUser(model);
                if (response.IsSuccess)
                {
                    UserModel user = (UserModel)response.Data;
                    Session["UserName"] = user.Name;
                    Session["UserID"] = user.UserID;
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            ViewBag.Message = response.Message;
            return View();
        }

        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registration(RegisterUserModel model)
        {
            ResponseModel response = new ResponseModel();
            ViewBag.Message = String.Empty;

            try
            {
                UserService userService = new UserService();
                response = userService.RegisterUser(model);
                if (response.IsSuccess)
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            ViewBag.Message = response.Message;
            return View();
        }
    }
}