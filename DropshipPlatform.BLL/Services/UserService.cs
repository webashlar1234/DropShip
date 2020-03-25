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

        public user GetUser(int UserID)
        {
            user dbUser = new user();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    dbUser = datacontext.users.Where(m => m.UserID == UserID).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                dbUser = null;
                logger.Error(ex.ToString());
            }
            return dbUser;
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
                        Obj.AliExpressAccessToken = Newtonsoft.Json.JsonConvert.SerializeObject(AccessToken);
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

        public int GetLoginUserRoleID(int UserID)
        {
            int RoleID = 0;
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    RoleID = datacontext.user_roles.Where(m => m.UserID == UserID).Select(x => x.RoleID).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                RoleID = 0;
                logger.Error(ex.ToString());
            }
            return RoleID;
        }


        public List<user> getOperationalUsers()
        {
            List<user> operationalUsers = new List<user>();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    List<user_roles> userList = datacontext.user_roles.Where(x => x.RoleID == 2).ToList();
                    foreach (var item in userList)
                    {
                        user Obj = datacontext.users.Where(x => x.UserID == item.UserID).FirstOrDefault();
                        if (Obj != null)
                        {
                            user users = new user();
                            users.UserID = Obj.UserID;
                            users.Name = Obj.Name;
                            users.EmailID = Obj.EmailID;
                            users.Phone = Obj.Phone;
                            users.Country = Obj.Country;

                            operationalUsers.Add(users);
                        }
                    }
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
                    if(userList != null)
                    {
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
    }
}
