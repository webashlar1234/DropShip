using DropshipPlatform.BLL.Models;
using DropshipPlatform.BLL.SubscriptionModels;
using Stripe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DropshipPlatform.Controllers
{
    public class SubscriptionsController : Controller
    {
        private readonly StripeClient client;
        public SubscriptionsController()
        {
            this.client = new StripeClient(StaticValues.stripeTestSecretKey);
        }

        public ActionResult Index()
        {
            return View();
        }

        //[HttpGet]
        //public PublicKeyResponse GetPublishableKey()
        //{
        //    PublicKeyResponse keyResponse = new PublicKeyResponse();
        //    keyResponse.PublicKey = StaticValues.stripeTestPublishKey;
        //    return keyResponse;
        //}

        [HttpPost]
        public async Task<JsonResult> CreateCustomerAsync(CustomerCreateRequest request)
        {
            Subscription result = new Subscription();
            try
            {
                var customerService = new CustomerService(this.client);

                var customer = await customerService.CreateAsync(new CustomerCreateOptions
                {
                    Email = request.Email,
                    PaymentMethod = request.PaymentMethod,
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = request.PaymentMethod,
                    }
                });

                var subscriptionService = new SubscriptionService(this.client);

                var subscription = await subscriptionService.CreateAsync(new SubscriptionCreateOptions
                {
                    Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions {
                        Plan = "plan_GiQrXsdH6vbs2u"   // created on stripe
                        },
                },
                    Customer = customer.Id,
                    Expand = new List<string> { "latest_invoice.payment_intent" }
                });
                result = subscription;
                //return subscription;

            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
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
            string result = String.Empty;
            StripeConfiguration.ApiKey = StaticValues.stripeTestAPIKey;
            var options = new PlanCreateOptions
            {
                Product = new PlanProductCreateOptions
                {
                    Name = planModel.name
                },
                Amount = planModel.amount,
                Currency = planModel.currency,
                Interval = planModel.interval
            };
            var service = new PlanService();
            Plan plan = service.Create(options);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}