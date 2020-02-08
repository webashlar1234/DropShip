using DropshipPlatform.BLL.Services;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DropshipPlatform.Controllers
{
    public class MyAccountController : Controller
    {
        StripeService _stripeService = new StripeService();
        public ActionResult Index()
        {
            return View();
        }
    }
}