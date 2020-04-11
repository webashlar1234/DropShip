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
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string[] allowedroles;
        public CustomAuthorizeAttribute(params string[] roles)
        {
            this.allowedroles = roles;
        }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool authorize = false;
            int userId = SessionManager.GetUserSession().dbUser.UserID;

            if (userId > 0)

                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    var userRole = (from u in datacontext.users
                                    join ur in datacontext.user_roles on u.UserID equals ur.UserID
                                    join rl in datacontext.roles on ur.RoleID equals rl.RoleID
                                    where u.UserID == userId
                                    select new
                                    {
                                        rl.RoleID,
                                        rl.Name
                                    }).FirstOrDefault();

                    if (userRole != null)
                    {
                        foreach (var role in allowedroles)
                        {
                            if (role == userRole.Name) return true;
                        }
                    }
                }
            return authorize;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
               new RouteValueDictionary
               {
                    { "controller", "UnAutoriziedUser" },
                    { "action", "Index" }
               });
        }
    }
}