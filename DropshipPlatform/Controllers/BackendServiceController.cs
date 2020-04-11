﻿using DropshipPlatform.BLL.Services;
using DropshipPlatform.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DropshipPlatform.Controllers
{
    public class BackendServiceController : Controller
    {
        [HttpPost]
        [AjaxFilter]
        public JsonResult Index()
        {
            if (SessionManager.GetAccessToken().access_token != null && SessionManager.GetUserSession() != null)
            {
                new BackendHelper().RefreshAliExpressJobLog();
                //new BackendHelper().RefreshAliExpressInventory();
                new BackendHelper().RefreshAliExpressOrders();
            }
            return new JsonResult { Data = "Success" };
        }
    }
}