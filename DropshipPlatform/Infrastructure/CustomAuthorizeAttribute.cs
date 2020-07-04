using DropshipPlatform.BLL.Models;
using DropshipPlatform.BLL.Services;
using DropshipPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DropshipPlatform.Infrastructure
{
    public class CustomAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string[] allowedroles;
        private readonly string[] AccessTokenPages= { "Products", "Order" };
        public CustomAuthorizeAttribute(params string[] roles)
        {
            this.allowedroles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            LoggedUserModel user = SessionManager.GetUserSession();
            int userId = user != null ? user.UserID : 0;
            bool UserIsValid = false;
            if (userId > 0)
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    if (!string.IsNullOrEmpty(user.LoggedUserRoleName))
                    {
                        foreach (var role in allowedroles)
                        {
                            if (role == user.LoggedUserRoleName)
                            {
                                UserIsValid = true;
                                break;
                            }
                        }
                        if (UserIsValid)
                        {
                            if(user.LoggedUserRoleID == 3)
                            {
                                string actionName = filterContext.ActionDescriptor.ActionName;
                                string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;

                                if (AccessTokenPages.Contains(controllerName))
                                {
                                    if (string.IsNullOrEmpty(user.AliExpressAccessToken))
                                    {
                                        new RedirectHelper().RedirectToLogin(filterContext, "AliExpress", "Index");
                                    }
                                    else if (user.IsActive != true)
                                    {
                                        new RedirectHelper().RedirectToLogin(filterContext, "MyAccount", "Index");
                                    }
                                }
                                
                            }
                        }
                        else
                        {
                            new RedirectHelper().RedirectToLogin(filterContext);
                        }
                    }
                }
            }
            else
            {
                new RedirectHelper().RedirectToLogin(filterContext);
            }
        }
    }

    public class AjaxFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
           
            if (SessionManager.GetUserSession() != null)
            {
            }
            else
            {
                new RedirectHelper().RedirectToLogin(filterContext);
            }
        }
    }
    public class RedirectHelper
    {
        public void RedirectToLogin(ActionExecutingContext filterContext, string controller = "Login", string action= "Index")
        {
            if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.HttpContext.Response.StatusCode = 401;
                filterContext.HttpContext.Response.End();
                filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary
                {
                     { "controller", controller },
                     { "action", action }
                });
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary
                {
                     { "controller", controller },
                     { "action", action }
                });
            }
        }
    }
}