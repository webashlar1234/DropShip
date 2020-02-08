using DropshipPlatform.BLL.Services;
using DropshipPlatform.DLL;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DropshipPlatform.Controllers
{
    public class StripeController : Controller
    {
        StripeService _stripeService = new StripeService();
        public ActionResult Index()
        {
            //_stripeService.stipe_CreateCustomer();   
            return View();
        }

        public JsonResult SetupCard() {
            var options = new SetupIntentCreateOptions { };
            var service = new SetupIntentService();
            var intent = service.Create(options);
            return Json(intent.ClientSecret, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddCardToCustomer(SetupIntent intent)
        {
            User user = SessionManager.GetUserSession();
            if (string.IsNullOrEmpty(user.StripeCustomerID))
            {
                _stripeService.stripe_CreateCustomer(user);
            }
            else
            {
                _stripeService.AddCardToExistingCustomer(intent, user.StripeCustomerID);
            }
            return View();
        }
    }
}