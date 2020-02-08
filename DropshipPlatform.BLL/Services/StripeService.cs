using DropshipPlatform.BLL.Models;
using DropshipPlatform.DLL;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropshipPlatform.BLL.Services
{
    public class StripeService
    {
        public StripeService() {
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

        public bool stripe_CreateCustomer(User user)
        {
            bool result = true;
            try
            {
                var options = new CustomerCreateOptions
                {
                    Name= user.Name,
                    Email = user.EmailID,
                    Phone = user.Phone
                };
                var service = new CustomerService();
                service.Create(options);
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
    }
}
