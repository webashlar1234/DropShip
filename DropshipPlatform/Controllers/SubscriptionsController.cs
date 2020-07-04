using DropshipPlatform.BLL.Models;
using DropshipPlatform.BLL.Services;
using DropshipPlatform.BLL.SubscriptionModels;
using DropshipPlatform.Entity;
using DropshipPlatform.Infrastructure;
using Stripe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DropshipPlatform.Controllers
{
    [CustomAuthorize("Admin", "Operational Manager", "Seller", "Developer")]
    public class SubscriptionsController : Controller
    {
        StripeService _stripeService = new StripeService();

        private readonly StripeClient client;
        public SubscriptionsController()
        {
            this.client = new StripeClient(StaticValues.stripeTestSecretKey);
        }

        public ActionResult Index(int PlandID)
        {
            membershiptype model = new MemberShipService().GetMemberShipDetail(PlandID);
            return View(model);
        }

        //[HttpGet]
        //public PublicKeyResponse GetPublishableKey()
        //{
        //    PublicKeyResponse keyResponse = new PublicKeyResponse();
        //    keyResponse.PublicKey = StaticValues.stripeTestPublishKey;
        //    return keyResponse;
        //}

        [HttpPost]
        public async Task<JsonResult> CreateCustomerAsync(CustomerCreateRequest request, string planID)
        {
            Stripe.Subscription result = new Stripe.Subscription();
            bool hasPaymentMethod = true;
            try
            { 
                LoggedUserModel user = SessionManager.GetUserSession();
                if (string.IsNullOrEmpty(user.StripeCustomerID))
                {
                    hasPaymentMethod = _stripeService.stripe_CreateCustomer(user, request.PaymentMethod);
                    new LoginController().UpdateUserSession();
                    user = SessionManager.GetUserSession();
                }
                else
                {
                    hasPaymentMethod = _stripeService.AddCardToExistingCustomer(request.PaymentMethod, user.StripeCustomerID);
                }
                if (hasPaymentMethod)
                {
                    result = _stripeService.CreateSubscription(user.StripeCustomerID, planID);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public async Task<IActionResult> ProcessWebhookEvent()
        //{
        //    var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        //    try
        //    {
        //        var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], this.options.Value.StripeWebhookSecret);
        //        logger.LogInformation($"Webhook event type: {stripeEvent.Type}");
        //        logger.LogInformation(json);
        //        return Ok();
        //    }
        //    catch (Exception e)
        //    {
        //        logger.LogError(e, "Exception while processing webhook event.");
        //        return BadRequest();
        //    }
        //}


        public ActionResult Plan()
        {
            return View();
        }

        [HttpPost]
        public JsonResult createPlan(PlanViewModel planModel)
        {
            bool result = true;
            result = _stripeService.CreatePlan(SessionManager.GetUserSession(), planModel);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveSubscriptionToDb(SubscriptionModel subscription)
        {
            bool result = true;
            LoggedUserModel user = SessionManager.GetUserSession();
            if (user != null && subscription != null)
            {
                result = _stripeService.SaveSubscriptionToDb(user, subscription);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpgradeSubscription(string PlanID)
        {
            bool result = true;
            LoggedUserModel user = SessionManager.GetUserSession();
            result = _stripeService.UpgradeSubscription(user.SubscriptionID, PlanID);
            if (result)
            {
                result = _stripeService.UpdateSubscriptionToDb(user.UserID, PlanID, user.SubscriptionID); 
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CancelSubscription(string PlanID)
        {
            bool result = true;
            LoggedUserModel user = SessionManager.GetUserSession();
            // subscription is cancled but need to check for refund is done or not
            result = _stripeService.CancelSubscription(user.SubscriptionID);
            if (result)
            {
                result = _stripeService.CancelSubscriptionToDb(user.UserID, user.SubscriptionID);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}