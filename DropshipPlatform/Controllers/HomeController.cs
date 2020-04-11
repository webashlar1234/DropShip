﻿using DropshipPlatform.BLL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DropshipPlatform.Controllers
{
    public class HomeController : Controller
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        ProductService _productService = new ProductService();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            Response.Write("hi");
            Response.End();
            return View();
        }
        [HttpGet]
        public ActionResult LogOut()
        {
            SessionManager.GetUserSession().dbUser.Name = null;
            SessionManager.GetUserSession().dbUser.UserID = 0;
            return RedirectToAction("Index", "Home");
        }

        public ActionResult UserManagement()
        {
            return View();
        }

        public ActionResult Membership()
        {
            return View();
        }
    }
}