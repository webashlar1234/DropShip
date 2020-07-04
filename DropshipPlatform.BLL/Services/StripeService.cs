using DropshipPlatform.BLL.Models;
using DropshipPlatform.BLL.SubscriptionModels;
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

        public bool stripe_CreateCustomer(LoggedUserModel user, string PaymentMethodId)
        {
            bool result = true;
            try
            {
                var options = new CustomerCreateOptions
                {
                    Name = user.Name,
                    Email = user.EmailID,
                    Phone = user.Phone,
                    PaymentMethod = string.IsNullOrEmpty(PaymentMethodId) ? null : PaymentMethodId,
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = PaymentMethodId
                    }
                };
                var service = new CustomerService();
                var customer = service.Create(options);

                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    user Obj = datacontext.users.Where(x => x.UserID == user.UserID).FirstOrDefault();
                    if (Obj != null)
                    {
                        Obj.StripeCustomerID = customer.Id;
                        datacontext.Entry(Obj).State = EntityState.Modified;
                        datacontext.SaveChanges();
                    }
                    if (!string.IsNullOrEmpty(customer.Id))
                    {
                        AddCardToPaymentProfile(customer.Id, user.UserID, PaymentMethodId);
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

        public bool stripe_UpdateCustomer(string PaymentMethodId)
        {
            bool result = true;
            try
            {
                var options = new CustomerUpdateOptions
                {
                    //Metadata = new Dictionary<string, string>
                    //  {
                    //    { "order_id", "6735" },
                    //  },
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = PaymentMethodId
                    }
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

        public bool AddCardToExistingCustomer(string PaymentMethodId, string StripeCustomerID)
        {
            bool result = true;
            try
            {
                var options = new PaymentMethodAttachOptions
                {
                    Customer = StripeCustomerID,
                };
                var service = new PaymentMethodService();
                var paymentMethod = service.Attach(PaymentMethodId, options);
                if (!string.IsNullOrEmpty(StripeCustomerID))
                {
                    LoggedUserModel user = SessionManager.GetUserSession();
                    AddCardToPaymentProfile(StripeCustomerID, user.UserID, PaymentMethodId);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                result = false;
            }

            return result;
        }

        public StripeResultModel ChargeSavedCard(string StripeCustomerID, long amount)
        {
            StripeResultModel StripeResultModel = new StripeResultModel();
            try
            {
                amount = amount * 100; //convert to cent https://stripe.com/docs/currencies#zero-decimal
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
                        Currency = "usd",
                        Customer = StripeCustomerID,
                        PaymentMethod = paymentMethodList.FirstOrDefault().Id,
                        Confirm = true,
                        OffSession = true,
                    };
                    var resu = PIservice.Create(options_create);

                    StripeResultModel.PaymentIntentID = resu.Id;
                    StripeResultModel.IsSuccess = true;
                }
                else
                {
                    StripeResultModel.IsSuccess = false;
                    StripeResultModel.Result = "Please add credit card from account";
                }
            }
            catch (StripeException e)
            {
                StripeResultModel.IsSuccess = false;
                StripeResultModel.Result = Newtonsoft.Json.JsonConvert.SerializeObject(e.StripeError);
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

            return StripeResultModel;
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

        public bool SaveSubscriptionToDb(user user, SubscriptionModel subscription)
        {
            bool result = true;
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    List<Entity.subscription> list = datacontext.subscriptions.Where(x => x.UserID == user.UserID).ToList();

                    foreach (var item in list)
                    {
                        item.IsActive = false;
                        datacontext.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    }

                    Entity.subscription obj = new Entity.subscription();
                    obj.UserID = user.UserID;
                    obj.StripeSubscriptionID = subscription.Id;
                    obj.MembershipID = datacontext.membershiptypes.Where(x => x.StripePlanID == subscription.Plan.Id).FirstOrDefault().MembershipID;
                    obj.MembershipStartDate = subscription.CurrentPeriodStart;
                    obj.MembershipExpiredDate = subscription.CurrentPeriodEnd;
                    obj.MembershipCreatedOn = DateTime.Now;
                    obj.MembershipCreatedBy = user.UserID;
                    obj.IsActive = true;
                    obj.Status = subscription.Status;

                    datacontext.subscriptions.Add(obj);
                    datacontext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                result = false;
            }

            return result;
        }

        public bool CreatePlan(LoggedUserModel user, PlanViewModel planModel)
        {
            bool result = true;
            try
            {
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

                if (plan != null)
                {
                    using (DropshipDataEntities datacontext = new DropshipDataEntities())
                    {
                        membershiptype obj = new membershiptype();
                        obj.Name = planModel.name;
                        obj.StripePlanID = plan.Id;
                        obj.Type = planModel.interval;
                        obj.Price = (int)planModel.amount;
                        obj.PickLimit = planModel.pickLimit;
                        obj.Description = planModel.description;
                        obj.ItemCreatedBy = user.UserID;
                        obj.ItemCreatedWhen = DateTime.Now;

                        datacontext.membershiptypes.Add(obj);
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

        public Stripe.Subscription CreateSubscription(string StripeCustomerID, string PlanID)
        {
            Stripe.Subscription subscription = new Stripe.Subscription();
            try
            {
                var subscriptionService = new SubscriptionService();

                subscription = subscriptionService.Create(new SubscriptionCreateOptions
                {
                    Items = new List<SubscriptionItemOptions>
                            {
                                new SubscriptionItemOptions {
                                    Plan = PlanID
                                    },
                            },
                    Customer = StripeCustomerID,
                    Expand = new List<string> { "latest_invoice.payment_intent" }
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return subscription;
        }

        public bool UpgradeSubscription(string SubscriptionID, string PlanID)
        {
            bool result = false;
            if(!string.IsNullOrEmpty(SubscriptionID) && !string.IsNullOrEmpty(PlanID))
            {
                Stripe.Subscription subscriptionRes = new Stripe.Subscription();
                
                try
                {
                    var service = new SubscriptionService();
                    Subscription subscription = service.Get(SubscriptionID);

                    var options = new SubscriptionUpdateOptions
                    {
                        Items = new List<SubscriptionItemOptions>
                            {
                                new SubscriptionItemOptions {
                                    Id = subscription.Items.Data[0].Id,
                                    Plan = PlanID
                                    },
                            }
                    };

                    subscriptionRes = service.Update(SubscriptionID, options);

                    if (subscriptionRes.StripeResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        result = true;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                }
            }
            
            return result;
        }

        public bool UpdateSubscriptionToDb(int UserID, string stripePlanID, string subscriptionID)
        {
            bool result = true;
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    int newPlanID =  datacontext.membershiptypes.Where(x => x.StripePlanID == stripePlanID).Select(x => x.MembershipID).FirstOrDefault();
                    
                    var subscription = datacontext.subscriptions.Where(x => x.UserID == UserID && x.StripeSubscriptionID == subscriptionID && x.IsActive == true).FirstOrDefault();
                    subscription.MembershipID = newPlanID;
                    subscription.MembershipModifyOn = DateTime.Now;
                    subscription.MembershipModifyBy = UserID;

                    datacontext.Entry(subscription).State = EntityState.Modified;
                    datacontext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                result = false;
            }

            return result;
        }

        public bool CancelSubscriptionToDb(int UserID, string subscriptionID)
        {
            bool result = true;
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    var subscription = datacontext.subscriptions.Where(x => x.UserID == UserID && x.StripeSubscriptionID == subscriptionID && x.IsActive == true).FirstOrDefault();
                    subscription.IsActive = false;
                    subscription.MembershipModifyOn = DateTime.Now;
                    subscription.MembershipModifyBy = UserID;

                    var user = datacontext.users.Where(x => x.UserID == UserID && x.IsActive == true).FirstOrDefault();
                    user.IsActive = false;
                    user.ItemModifyWhen = DateTime.Now;
                    user.ItemModifyBy = UserID;

                    datacontext.Entry(subscription).State = EntityState.Modified;
                    datacontext.Entry(user).State = EntityState.Modified;
                    datacontext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                result = false;
            }

            return result;
        }

        public bool CancelSubscription(string SubscriptionID)
        {
            bool result = false;
            try
            {
                var service = new SubscriptionService();
                var options = new SubscriptionCancelOptions
                {
                    InvoiceNow = true,
                    Prorate = true,
                };
                var subscription = service.Cancel(SubscriptionID, options);

                if (subscription.StripeResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return result;
        }

        public bool AddCardToPaymentProfile(string StripeCustomerID, int UserID, string PaymentMethodId)
        {
            bool result = false;
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    StripeList<PaymentMethod> PaymentMethods = new StripeList<PaymentMethod>();
                    PaymentMethods = ListPaymentMethods(StripeCustomerID);
                    if (PaymentMethods.Data.Count() > 0)
                    {
                        foreach (var item in PaymentMethods.Data)
                        {
                            paymentprofile paymentObj = new paymentprofile();
                            var dbpaymentMethodId = datacontext.paymentprofiles.Where(x => x.StripePaymentMethodId == PaymentMethodId).Any();
                            if (!dbpaymentMethodId)
                            {
                                paymentObj.UserID = UserID;
                                paymentObj.CreditCardNumber = item.Card.Last4;
                                paymentObj.CreditCardType = item.Card.Brand;
                                paymentObj.ExpiryDate = item.Card.ExpMonth + "/" + item.Card.ExpYear;
                                paymentObj.NameOnCard = item.BillingDetails.Name;
                                paymentprofile defaultObj = datacontext.paymentprofiles.Where(x => x.UserID == UserID && x.IsDefault == 1).FirstOrDefault();
                                if (defaultObj != null)
                                {
                                    paymentObj.IsDefault = 0;
                                }
                                else
                                {
                                    paymentObj.IsDefault = 1;
                                }
                                paymentObj.StripePaymentMethodId = PaymentMethodId;
                                paymentObj.ItemCreatedBy = UserID;
                                paymentObj.ItemCreatedWhen = DateTime.Now;
                                datacontext.paymentprofiles.Add(paymentObj);
                                result = true;
                            }
                        }
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

        public StripeResultModel RefundStripeUser(int UserID, long amount, string OrderID)
        {
            StripeResultModel StripeResultModel = new StripeResultModel();
            string PaymentMethodId = string.Empty;
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    PaymentMethodId = (from u in datacontext.users
                                       from o in datacontext.orders.Where(x => x.AliExpressLoginID == u.AliExpressLoginID)
                                       where u.UserID == UserID && !string.IsNullOrEmpty(u.StripeCustomerID) && o.AliExpressOrderID == OrderID
                                       select o.PaymentIntentID
                                       ).FirstOrDefault();
                }

                logger.Info("PaymentMethodId" + PaymentMethodId);

                if (!string.IsNullOrEmpty(PaymentMethodId))
                {
                    var refundService = new RefundService();
                    var refundOptions = new RefundCreateOptions
                    {
                        PaymentIntent = PaymentMethodId,
                        Amount = amount
                    };
                    var refund = refundService.Create(refundOptions);
                    StripeResultModel.PaymentIntentID = refund.Id;
                    StripeResultModel.IsSuccess = true;

                    logger.Info("PaymentMethod_ref" + Newtonsoft.Json.JsonConvert.SerializeObject(refund)); ;
                }
                else
                {
                    StripeResultModel.IsSuccess = false;
                    StripeResultModel.Result = "PaymentMethodId is null";
                }
            }
            catch (StripeException ex)
            {
                StripeResultModel.IsSuccess = false;
                StripeResultModel.Result = Newtonsoft.Json.JsonConvert.SerializeObject(ex.StripeError);
                logger.Error(ex.ToString());
            }
            return StripeResultModel;
        }

        public List<SubcriptionPlanModel> GetSubscriptionFromDb()
        {
            List<SubcriptionPlanModel> obj = new List<SubcriptionPlanModel>();
            LoggedUserModel user = SessionManager.GetUserSession();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    obj = (from s in datacontext.subscriptions.Where(x => x.UserID == user.UserID & x.IsActive == true)
                           from p in datacontext.membershiptypes.Where(x => x.MembershipID == s.MembershipID)
                           select new SubcriptionPlanModel
                           {
                               PlanName = p.Name,
                               PlanType = p.Type,
                               BillingNextDate = s.MembershipExpiredDate,
                               BillAmount = p.Price,
                               StripeCustomerID = s.StripeSubscriptionID
                           }).ToList();
                    obj[0].NextBillDate = obj[0].BillingNextDate.Value.AddDays(1).ToShortDateString();

                }
            }
            catch (Exception ex)
            {
                obj = null;
                logger.Error(ex.ToString());
            }

            return obj;
        }
    }
}
