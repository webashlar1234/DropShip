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
        public static int UserID
        {
            get
            {
                try
                {
                    return (HttpContext.Current.Session["UserID"] == null) ? 0 : (int.Parse(HttpContext.Current.Session["UserID"].ToString()));
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                HttpContext.Current.Session["UserID"] = value;
            }
        }

        public static void SetUserSession(user user)
        {
            if (user != null)
            {
                HttpContext.Current.Session["userSession"] = HttpUtility.UrlEncode(Newtonsoft.Json.JsonConvert.SerializeObject(user));
            }
        }

        public static user GetUserSession()
        {
            user token = new user();
            try
            {
                if (HttpContext.Current.Session != null && HttpContext.Current.Session["userSession"] != null)
                {
                    string data = HttpContext.Current.Session["userSession"].ToString();
                    token = Newtonsoft.Json.JsonConvert.DeserializeObject<user>(HttpUtility.UrlDecode(data));
                }
            }
            catch (Exception ex)
            {

            }
            return token;
        }
    }
}
