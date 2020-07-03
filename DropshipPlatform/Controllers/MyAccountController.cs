using DropshipPlatform.BLL.Services;
using DropshipPlatform.Entity;
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
        MemberShipService _MemberShipService = new MemberShipService();

        [CustomAuthorize("Admin", "Operational Manager", "Seller", "Developer")]
        public ActionResult Index()
        {
            List<membershiptype> membershiptypesList = _MemberShipService.GetMemberShipData();
            ViewBag.PlanList = membershiptypesList;
            ViewBag.CurrentPlan = _stripeService.GetSubscriptionFromDb();
            return View();
        }
    }
}