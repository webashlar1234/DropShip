using DropshipPlatform.BLL.Models;
using DropshipPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropshipPlatform.BLL.Services
{
    public class UserService
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string Test()
        {
            return "Test";
        }

        public ResponseModel LoginUser(LoginModel model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    user dbUser = datacontext.users.Where(m => m.EmailID == model.Email && m.Password == model.Password).FirstOrDefault();
                    if (dbUser != null)
                    {
                        response.Data = dbUser.UserID;
                        response.Message = "Success";
                        response.IsSuccess = true;
                    }
                    else
                    {
                        response.Message = "User not found";
                        response.IsSuccess = false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return response;
        }

        public ResponseModel RegisterUser(RegisterUserModel model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    user dbUser = new user();
                    dbUser.Name = model.Username;
                    dbUser.Password = model.Password;
                    dbUser.Phone = model.Phone;
                    dbUser.Country = model.Country;
                    dbUser.EmailID = model.Email;
                    dbUser.IsActive = true;
                    dbUser.IsPolicyAccepted = true;
                    datacontext.users.Add(dbUser);
                    datacontext.SaveChanges();

                    user_roles dbUserRoles = new user_roles();
                    dbUserRoles.UserID = dbUser.UserID;
                    dbUserRoles.RoleID = model.RoleID;
                    datacontext.user_roles.Add(dbUserRoles);

                    datacontext.SaveChanges();

                    response.Data = dbUser;
                    response.Message = "Success";
                    response.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                response.Message = "Registration failed , Try again";
                response.IsSuccess = false;
            }
            return response;
        }

        public LoggedUserModel GetUser(int UserID)
        {
            LoggedUserModel dbLoggedUser = new LoggedUserModel();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {

                    dbLoggedUser = (from u in datacontext.users
                                    from ur in datacontext.user_roles.Where(x => x.UserID == u.UserID)
                                    from r in datacontext.roles.Where(x => x.RoleID == ur.RoleID)
                                    from s in datacontext.subscriptions.Where(x => x.UserID == u.UserID).DefaultIfEmpty()
                                    where u.IsActive == true && u.UserID == UserID
                                    select new LoggedUserModel
                                    {
                                        UserID = u.UserID,
                                        Name = u.Name,
                                        EmailID = u.EmailID,
                                        Phone = u.Phone,
                                        Image = u.Image,
                                        Country = u.Country,
                                        AliExpressSellerID = u.AliExpressSellerID,
                                        AliExpressLoginID = u.AliExpressLoginID,
                                        AliExpressAccessToken = u.AliExpressAccessToken,
                                        AliExpressTokenLastModified = u.AliExpressTokenLastModified,
                                        StripeCustomerID = u.StripeCustomerID,
                                        IsActive = r.RoleID == 3 ? (u.IsActive == true && s.IsActive == true ) :  u.IsActive,
                                        IsPolicyAccepted = u.IsPolicyAccepted,
                                        LoggedUserRoleName = r.Name,
                                        LoggedUserRoleID = r.RoleID,
                                        SubscriptionID = s.StripeSubscriptionID
                                    }).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                dbLoggedUser = null;
                logger.Error(ex.ToString());
            }
            return dbLoggedUser;
        }

        public user UpdateUserForAliExpress(int UserID, AliExpressAccessToken AccessToken)
        {
            user dbUser = new user();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    user Obj = datacontext.users.Where(x => x.UserID == UserID).FirstOrDefault();
                    if (Obj != null)
                    {
                        Obj.AliExpressSellerID = AccessToken.user_id;
                        Obj.AliExpressLoginID = AccessToken.user_nick;
                        Obj.AliExpressAccessToken = Newtonsoft.Json.JsonConvert.SerializeObject(AccessToken);
                        Obj.AliExpressTokenLastModified = DateTime.Now;
                        datacontext.Entry(Obj).State = EntityState.Modified;
                        datacontext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                dbUser = null;
                logger.Error(ex.ToString());
            }
            return dbUser;
        }

        //public int GetLoginUserRoleID(int UserID)
        //{
        //    int RoleID = 0;
        //    try
        //    {
        //        using (DropshipDataEntities datacontext = new DropshipDataEntities())
        //        {
        //            RoleID = datacontext.user_roles.Where(m => m.UserID == UserID).Select(x => x.RoleID).FirstOrDefault();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        RoleID = 0;
        //        logger.Error(ex.ToString());
        //    }
        //    return RoleID;
        //}


        public List<RegisterUserModel> getOperationalUsers()
        {
            List<RegisterUserModel> operationalUsers = new List<RegisterUserModel>();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    operationalUsers = (from u in datacontext.users
                                        from ur in datacontext.user_roles.Where(x => x.RoleID == 2 && x.UserID == u.UserID)
                                        select new RegisterUserModel
                                        {
                                            UserID = u.UserID,
                                            Username = u.Name,
                                            Email = u.EmailID,
                                            Phone = u.Phone,
                                            Country = u.Country
                                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return operationalUsers;
        }
        public bool deleteOperationalManager(int UserID)
        {
            bool result = false;
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    var userList = datacontext.users.Where(x => x.UserID == UserID).FirstOrDefault();
                    var userRoleList = datacontext.user_roles.Where(x => x.UserID == UserID).FirstOrDefault();
                    if (userList != null && userRoleList != null)
                    {
                        datacontext.user_roles.Remove(userRoleList);
                        datacontext.users.Remove(userList);
                        datacontext.SaveChanges();
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                logger.Info(ex.ToString());
            }
            return result;
        }

        public List<Seller> getSellerUsers()
        {
            List<Seller> SellerUsers = new List<Seller>();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    SellerUsers = (from u in datacontext.users
                                   from ur in datacontext.user_roles.Where(x => x.RoleID == 3 && x.UserID == u.UserID)
                                   from s in datacontext.subscriptions.Where(x => x.UserID == u.UserID).DefaultIfEmpty()
                                   from mt in datacontext.membershiptypes.Where(x => x.MembershipID == s.MembershipID)
                                   select new Seller {
                                       Name = u.Name,
                                       EmailID = u.EmailID,
                                       Phone = u.Phone,
                                       UserID = u.UserID,
                                       AliExpressSellerID = u.AliExpressSellerID,
                                       AliExpressLoginID = u.AliExpressLoginID,
                                       StripeCustomerID = u.StripeCustomerID,
                                       IsActive = u.IsActive,
                                       //IsActive = (u.IsActive == true && s.IsActive == true) ? true : false,
                                       IsPolicyAccepted = u.IsPolicyAccepted,
                                       ItemCreatedBy = u.ItemCreatedBy,
                                       Membership = mt.Name,
                                       NoOfPickedProducts = datacontext.sellerspickedproducts.Where(x => x.UserID == u.UserID).Count()
                                   }).ToList();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return SellerUsers;
        }

        public bool CheckEmailExist(string emailId)
        {
            bool result = true;
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    var user = datacontext.users.Where(x => x.EmailID == emailId).Count();
                    if (user > 0)
                    {
                        result = false;
                    }
                }
            }
            catch (Exception ex)
            {
                result = true;
                logger.Info(ex.ToString());
            }
            return result;
        }

        public bool UpdateUserStatus(int UserID)
        {
            bool result = true;
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    var user = datacontext.users.Where(x => x.UserID == UserID).FirstOrDefault();
                    if (user != null)
                    {
                        if (user.IsActive == true)
                        {
                            var subscription = datacontext.subscriptions.Where(x => x.UserID == UserID).FirstOrDefault();
                            if(subscription != null)
                            {
                                subscription.IsActive = false;
                                datacontext.Entry(subscription).State = EntityState.Modified;
                            }
                        }
                        user.IsActive = !user.IsActive;
                        datacontext.Entry(user).State = EntityState.Modified;
                        datacontext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                logger.Info(ex.ToString());
            }
            return result;
        }
    }
}
