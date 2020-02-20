using DropshipPlatform.BLL.Models;
using DropshipPlatform.Entity;
using Stripe;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropshipPlatform.BLL.Services
{
    public class StripeService
    {
        public StripeService()
        {
            StripeConfiguration.ApiKey = StaticValues.stripeTestAPIKey;
        }

        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public bool stripeTestFun()
        {
            bool result = true;
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = 1000,
                    Currency = "usd",
                    PaymentMethodTypes = new List<string> { "card" },
                    ReceiptEmail = "jenny.rosen@example.com",
                };
                var service = new PaymentIntentService();
                PaymentIntent respone = service.Create(options);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                result = false;
            }

            return result;
        }

        public bool stripe_CreateCustomer(User user, string PaymentMethodId)
        {
            bool result = true;
            try
            {
                var options = new CustomerCreateOptions
                {
                    Name = user.Name,
                    Email = user.EmailID,
                    Phone = user.Phone,
                    PaymentMethod = string.IsNullOrEmpty(PaymentMethodId) ? null : PaymentMethodId
                };
                var service = new CustomerService();
                var customer  = service.Create(options);

                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    User Obj = datacontext.Users.Where(x => x.UserID == user.UserID).FirstOrDefault();
                    if(Obj != null)
                    {
                        Obj.StripeCustomerID = customer.Id;
                        datacontext.Entry(Obj).State = EntityState.Modified;
                        datacontext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                result = false;
            }

            return result;
        }

        public bool stripe_UpdateCustomer()
        {
            bool result = true;
            try
            {
                var options = new CustomerUpdateOptions
                {
                    Metadata = new Dictionary<string, string>
                      {
                        { "order_id", "6735" },
                      },
                };
                var service = new CustomerService();
                service.Update("cus_GgsfpfsLgmjI9q", options);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                result = false;
            }

            return result;
        }

        public bool AddCardToExistingCustomer(SetupIntent intent, string StripeCustomerID)
        {
            bool result = true;
            try
            {
                var options = new PaymentMethodAttachOptions
                {
                    Customer = StripeCustomerID,
                };
                var service = new PaymentMethodService();
                var paymentMethod = service.Attach(intent.PaymentMethodId, options);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                result = false;
            }

            return result;
        }

        public bool ChargeSavedCard(string StripeCustomerID, long amount)
        {
            bool result = true;
            try
            {
                var options = new PaymentMethodListOptions
                {
                    Customer = StripeCustomerID,
                    Type = "card",
                };
                var service = new PaymentMethodService();
                var paymentMethodList = service.List(options);

                if (paymentMethodList.Count() > 0)
                {
                    var PIservice = new PaymentIntentService();
                    var options_create = new PaymentIntentCreateOptions
                    {
                        Amount = amount,
                        Currency = "hkd",
                        Customer = StripeCustomerID,
                        PaymentMethod = paymentMethodList.FirstOrDefault().Id,
                        Confirm = true,
                        OffSession = true,
                    };
                    PIservice.Create(options_create);
                }
            }
            catch (StripeException e)
            {
                switch (e.StripeError.ErrorType)
                {
                    case "card_error":
                        // Error code will be authentication_required if authentication is needed
                        logger.Error("Error code: " + e.StripeError.Code);
                        var paymentIntentId = e.StripeError.PaymentIntent.Id;
                        var service = new PaymentIntentService();
                        var paymentIntent = service.Get(paymentIntentId);

                        logger.Error(paymentIntent.Id);
                        break;
                    default:
                        logger.Error(e.Message);
                        break;
                }
            }

            return result;
        }

        public StripeList<PaymentMethod> ListPaymentMethods(string StripeCustomerID)
        {
            StripeList<PaymentMethod> PaymentMethods = new StripeList<PaymentMethod>();
            try
            {
                var options = new PaymentMethodListOptions
                {
                    Customer = StripeCustomerID,
                    Type = "card",
                };
                var service = new PaymentMethodService();
                PaymentMethods = service.List(options);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            return PaymentMethods;
        }

        public bool DeletePaymentMethod(string paymentMethodID)
        {
            bool result = true;
            try
            {
                var service = new PaymentMethodService();
                service.Detach(paymentMethodID);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            return result;
        }
    }
}
