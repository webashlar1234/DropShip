using DropshipPlatform.BLL;
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
                    User user = (User)response.Data;
                    SessionManager.SetUserSession(user);
                    if (!string.IsNullOrEmpty(user.AliExpressAccessToken))
                    {
                        SessionManager.SetAccessToken(Newtonsoft.Json.JsonConvert.DeserializeObject<AliExpressAccessToken>(user.AliExpressAccessToken));
                    }
                    Session["UserName"] = user.Name;
                    Session["UserID"] = user.UserID;
                    return RedirectToAction("Index", "AliExpress");
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

        public void UpdateUserSession()
        {
            User user = new UserService().GetUser(SessionManager.GetUserSession().UserID);
            if (user != null)
            {
                SessionManager.SetUserSession(user);
            }
        }
    }
}