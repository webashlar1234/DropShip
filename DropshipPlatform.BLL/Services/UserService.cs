using DropshipPlatform.BLL.Models;
using DropshipPlatform.Entity;
using System;
using System.Collections.Generic;
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
                    User dbUser = datacontext.Users.Where(m => m.EmailID == model.Email && m.Password == model.Password).FirstOrDefault();
                    if (dbUser != null)
                    {
                        //UserModel userModel = new UserModel();
                        //userModel.UserID = dbUser.UserID;
                        //userModel.Name = dbUser.Name;
                        //userModel.IsPolicyAccepted = dbUser.IsPolicyAccepted.Value;
                        //userModel.AliExpressSellerID = dbUser.AliExpressSellerID;
                        //userModel.StripeCustomerID = dbUser.StripeCustomerID;

                        response.Data = dbUser;
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
                    User dbUser = new User();
                    dbUser.Name = model.Username;
                    dbUser.Password = model.Password;
                    dbUser.Phone = model.Phone;
                    dbUser.EmailID = model.Email;
                    dbUser.IsActive = true;
                    dbUser.IsPolicyAccepted = true;
                    datacontext.Users.Add(dbUser);
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

        public User GetUser(int UserID)
        {
            User dbUser = new User();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    dbUser = datacontext.Users.Where(m => m.UserID == UserID).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                dbUser = null;
                logger.Error(ex.ToString());
            }
            return dbUser;
        }
    }
}
