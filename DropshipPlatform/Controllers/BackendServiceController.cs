﻿using DropshipPlatform.BLL.Services;
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
        public JsonResult Index()
        {
              if(SessionManager.GetAccessToken().access_token != null && SessionManager.GetUserSession() != null)
            {
                new BackendHelper().RefreshAliExpressJobLog();
                new BackendHelper().RefreshAliExpressInventory();
            }
            return new JsonResult { Data = "Success" };
        }
    }
}