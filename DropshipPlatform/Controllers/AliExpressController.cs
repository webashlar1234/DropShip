using DropshipPlatform.BLL;
using DropshipPlatform.BLL.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;
using Top.Api.Util;

namespace DropshipPlatform.Controllers
{
    public class AliExpressController : Controller
    {
        // GET: AliExpress
        string appkey = System.Web.Configuration.WebConfigurationManager.AppSettings["AliExpress_appKey"].ToString();
        string secret = System.Web.Configuration.WebConfigurationManager.AppSettings["AliExpress_appSecreat"].ToString();
        string url = System.Web.Configuration.WebConfigurationManager.AppSettings["AliExpress_URL"].ToString();
        AliExpressAuthService _aliExpressAuthService = new AliExpressAuthService();
        public ActionResult Index()
        {
            ViewBag.authorizeUrl = _aliExpressAuthService.getAuthorizeUrl();
            ViewBag.isAuthorised = false;
            AliExpressAccessToken aliExpressAccessToken = SessionManager.GetAccessToken();
            if (!string.IsNullOrEmpty(aliExpressAccessToken.access_token))
            {
                ViewBag.isAuthorised = true;
                ViewBag.AliExpressUserName = aliExpressAccessToken.user_nick;
            }
            return View();
        }

        [HttpGet]
        public ActionResult Authorize(string code)
        {
            AliExpressAccessToken accessToken = _aliExpressAuthService.getAccessToken(code);
            if (accessToken != null)
            {
                SessionManager.SetAccessToken(accessToken);
            }
            return RedirectToAction("Index", "AliExpress");
        }
    }
}