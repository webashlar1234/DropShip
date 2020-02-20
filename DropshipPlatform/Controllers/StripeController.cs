using DropshipPlatform.BLL.Services;
using DropshipPlatform.Entity;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Stripe; 
using System.Net;

namespace DropshipPlatform.Controllers
{
    public class StripeController : Controller
    {
        StripeService _stripeService = new StripeService();

        [HttpPost]
        public ActionResult Index()
        {
            Stream req = Request.InputStream;
            req.Seek(0, System.IO.SeekOrigin.Begin);

            string json = new StreamReader(req).ReadToEnd();
            try
            {
                // as in header, you need https://github.com/jaymedavis/stripe.net
                // it's a great library that should have been offered by Stripe directly
                var stripeEvent = EventUtility.ParseEvent(json);

                if (stripeEvent == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Incoming event empty");


                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    // Then define and call a method to handle the successful payment intent.
                    // handlePaymentIntentSucceeded(paymentIntent);
                }
                else if (stripeEvent.Type == Events.PaymentMethodAttached)
                {
                    var paymentMethod = stripeEvent.Data.Object as PaymentMethod;
                    // Then define and call a method to handle the successful attachment of a PaymentMethod.
                    // handlePaymentMethodAttached(paymentMethod);
                }
                else if (stripeEvent.Type == Events.SetupIntentSucceeded)
                {
                    var paymentMethod = stripeEvent.Data.Object as PaymentMethod;
                    // Then define and call a method to handle the successful attachment of a PaymentMethod.
                    // handlePaymentMethodAttached(paymentMethod);
                }
                else
                {
                    // Unexpected event type
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Unable to parse incoming event");
                }

                //switch (stripeEvent.Type)
                //{
                //    case "charge.refunded":
                //        // do work
                //        break;

                //    case "customer.subscription.updated":
                //    case "customer.subscription.deleted":
                //    case "customer.subscription.created":
                //        // do work
                //        break;
                //}
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Unable to parse incoming event");
            }  

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
        
        [HttpPost]
        public JsonResult SetupCard()
        {
            var options = new SetupIntentCreateOptions { };
            var service = new SetupIntentService();
            var intent = service.Create(options);
            return Json(intent.ClientSecret, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddCardToCustomer(SetupIntent intent)
        {
            bool result = true;
            User user = SessionManager.GetUserSession();
            if (string.IsNullOrEmpty(user.StripeCustomerID))
            {
                result = _stripeService.stripe_CreateCustomer(user, intent.PaymentMethodId);
            }
            else
            {
                result =_stripeService.AddCardToExistingCustomer(intent.PaymentMethodId, user.StripeCustomerID);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        
        public ActionResult ChargeSavedCard()
        {
            User user = SessionManager.GetUserSession();
            _stripeService.ChargeSavedCard(user.StripeCustomerID, 1000);
            return View();
        }

        public JsonResult getStripePaymentMethodsList()
        {
            User user = SessionManager.GetUserSession();
            StripeList<PaymentMethod> list = _stripeService.ListPaymentMethods(user.StripeCustomerID);
            return Json(new
            {
                data = list,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeletePaymentMethod(string paymentMethodID)
        {
            User user = SessionManager.GetUserSession();
            return Json(_stripeService.DeletePaymentMethod(paymentMethodID), JsonRequestBehavior.AllowGet);
        }
    }
}