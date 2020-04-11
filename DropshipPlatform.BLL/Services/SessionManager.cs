using DropshipPlatform.BLL.Models;
using DropshipPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DropshipPlatform.BLL.Services
{
    public class SessionManager
    {
        public static void SetAccessToken(AliExpressAccessToken accessToken)
        {
            if (accessToken != null)
            {
                HttpContext.Current.Session["AliExpressAccessToken"] = HttpUtility.UrlEncode(Newtonsoft.Json.JsonConvert.SerializeObject(accessToken));
            }
        }

        public static AliExpressAccessToken GetAccessToken()
        {
            AliExpressAccessToken token = new AliExpressAccessToken();
            try
            {
                if (HttpContext.Current.Session != null && HttpContext.Current.Session["AliExpressAccessToken"] != null)
                {
                    string data = HttpContext.Current.Session["AliExpressAccessToken"].ToString();
                    token = Newtonsoft.Json.JsonConvert.DeserializeObject<AliExpressAccessToken>(HttpUtility.UrlDecode(data));
                }
            }
            catch (Exception ex)
            {

            }

            return token;
        }

        public static void SetUserSession(LoggedUserModel user)
        {
            if (user != null)
            {
                HttpContext.Current.Session["userSession"] = HttpUtility.UrlEncode(Newtonsoft.Json.JsonConvert.SerializeObject(user));
            }
        }

        public static LoggedUserModel GetUserSession()
        {
            LoggedUserModel token = new LoggedUserModel();
            try
            {
                if (HttpContext.Current.Session != null && HttpContext.Current.Session["userSession"] != null)
                {
                    string data = HttpContext.Current.Session["userSession"].ToString();
                    token = Newtonsoft.Json.JsonConvert.DeserializeObject<LoggedUserModel>(HttpUtility.UrlDecode(data));
                }
                else
                {
                    token = null;
                }
            }
            catch (Exception ex)
            {

            }
            return token;
        }
        public static void RemoveUserSession()
        {
            HttpContext.Current.Session["userSession"] = null;
            HttpContext.Current.Session["AliExpressAccessToken"] = null;
        }
    }
}
