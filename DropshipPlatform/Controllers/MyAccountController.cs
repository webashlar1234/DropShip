using DropshipPlatform.BLL.Services;
using DropshipPlatform.Infrastructure;
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

        [CustomAuthorize("Admin", "Operational Manager", "Seller", "Developer")]
        public ActionResult Index()
        {
            return View();
        }
    }
}