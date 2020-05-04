using DropshipPlatform.BLL;
using DropshipPlatform.BLL.Models;
using DropshipPlatform.BLL.Services;
using DropshipPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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
                    int userid = (int)response.Data;
                    LoggedUserModel loggedUserModel = new UserService().GetUser(userid);
                    
                    SessionManager.SetUserSession(loggedUserModel);
                    if (!string.IsNullOrEmpty(loggedUserModel.AliExpressAccessToken))
                    {
                        SessionManager.SetAccessToken(Newtonsoft.Json.JsonConvert.DeserializeObject<AliExpressAccessToken>(loggedUserModel.AliExpressAccessToken));
                    }
                    if(loggedUserModel.LoggedUserRoleName == StaticValues.seller)
                    {
                        return RedirectToAction("Index", "AliExpress");
                    }
                    else
                    {
                        return RedirectToAction("getOrders", "Order");
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

        public void UpdateUserSession()
        {
            LoggedUserModel user = new UserService().GetUser(SessionManager.GetUserSession().UserID);
            if (user != null)
            {
                SessionManager.SetUserSession(user);
            }
        }
    }
}