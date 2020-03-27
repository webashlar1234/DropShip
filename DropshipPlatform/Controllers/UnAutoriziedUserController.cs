using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DropshipPlatform.Controllers
{
    public class UnAutoriziedUserController : Controller
    {
        // GET: UnAutoriziedUser
        public ActionResult Index()
        {
            return View();
        }
    }
}