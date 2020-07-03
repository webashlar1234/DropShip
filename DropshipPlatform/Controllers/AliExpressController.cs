using DropshipPlatform.BLL;
using DropshipPlatform.BLL.Models;
using DropshipPlatform.BLL.Services;
using DropshipPlatform.Entity;
using DropshipPlatform.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
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

        [AjaxFilter]
        //[CustomAuthorize("Seller")]
        public ActionResult Index()
        {
            ViewBag.authorizeUrl = _aliExpressAuthService.getAuthorizeUrl();
            ViewBag.isAuthorised = false;
            AliExpressAccessToken aliExpressAccessToken = SessionManager.GetAccessToken();
            UserService user = new UserService();
            LoggedUserModel userData = SessionManager.GetUserSession();
            if (userData.AliExpressTokenLastModified == null)
            {
                userData = user.GetUser(userData.UserID);
            }
            if (!string.IsNullOrEmpty(aliExpressAccessToken.access_token))
            {
                ViewBag.isAuthorised = true;
                ViewBag.AliExpressUserName = aliExpressAccessToken.user_nick;
                if (userData.AliExpressTokenLastModified != null)
                {
                    ViewBag.ExpiresDay = 30 - ((DateTime.Now.Date - userData.AliExpressTokenLastModified.Value.Date).TotalDays);
                }
                else
                {
                    ViewBag.ExpiresDay = 0;
                }
            }
            return View();
        }

        [HttpGet]
        [AjaxFilter]
        public ActionResult Authorize(string code)
        {
            AliExpressAccessToken accessToken = _aliExpressAuthService.getAccessToken(code);
            if (accessToken != null)
            {
                ////get expired date
                //TimeSpan timespan = TimeSpan.FromMilliseconds(float.Parse(accessToken.expire_time));
                //DateTime dt = DateTime.Now.Add(timespan);

                new UserService().UpdateUserForAliExpress(SessionManager.GetUserSession().UserID, accessToken);
                SessionManager.SetAccessToken(accessToken);

                LoggedUserModel loggedUserModel = new UserService().GetUser(SessionManager.GetUserSession().UserID);
                SessionManager.SetUserSession(loggedUserModel);
            }
            return RedirectToAction("Index", "AliExpress");
        }

        [HttpGet]
        [AjaxFilter]
        public JsonResult checkResultByJobId(long jobid,int id)
        {
            new BackendHelper().RefreshAliExpressJobLog();
            string result = new ProductService().getResultByJobId(jobid,id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [AjaxFilter]
        public JsonResult getJobList()
        {
            ProductService _productService = new ProductService();
            string result = _productService.getJobList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize("Admin", "Operational Manager", "Developer", "Seller")]
        public ActionResult jobLog()
        {
            return View();
        }

        [HttpPost]
        [AjaxFilter]
        public ActionResult getJobLogData()
        {
            ProductService _productService = new ProductService();
            int recordsTotal = 0;
            var draw = Request.Form.GetValues("draw") != null ? Request.Form.GetValues("draw").FirstOrDefault() : null;
            var start = Request.Form.GetValues("start") != null ? Request.Form.GetValues("start").FirstOrDefault() : null;
            var length = Request.Form.GetValues("length") != null ? Request.Form.GetValues("length").FirstOrDefault() : null;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            //Find Order Column
            var sortColumn = Request.Form.GetValues("order[0][column]") != null ? Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][data]").FirstOrDefault() : null;
            var sortColumnDir = Request.Form.GetValues("order[0][dir]") != null ? Request.Form.GetValues("order[0][dir]").FirstOrDefault() : null;
            string sortOrder = "";

            int userid = 0;
            LoggedUserModel user = SessionManager.GetUserSession();
            if(user.LoggedUserRoleName == StaticValues.seller) { userid = user.UserID; }

            List<aliexpressjoblog> resultList = _productService.getJobLogData(userid);
            List<JobLog> retvalue = resultList.Select(x => new JobLog()
            {
                Id = x.Id,
                JobId = x.JobId.ToString(),
                SuccessItemCount = x.SuccessItemCount.ToString(),
                ContentId = x.ContentId,
                Result = x.Result,
                ErrorType = x.ErrorType,
                CreatedOn = x.CreatedOn,
                CreatedBy = x.CreatedBy
            }).ToList();
            if ((!string.IsNullOrEmpty(sortColumn)) && (!string.IsNullOrEmpty(sortColumnDir)))
            {
                sortOrder = sortColumn + " " + sortColumnDir + "," + Request.Form.GetValues("order[0][column]").FirstOrDefault();
            }
            if (!string.IsNullOrEmpty(sortOrder))
            {
                string orderBy = sortOrder.Split(',')[0];
                retvalue = retvalue.OrderBy(orderBy).ToList();
            }
            var data = new List<JobLog>();
            if (pageSize != -1)
            {
                data = retvalue.Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                data = retvalue.ToList();
            }
            recordsTotal = retvalue.Count();

            var jsonResult = Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        
        [HttpGet]
        [AjaxFilter]
        public JsonResult getResultByJobId(long jobId,int id)
        {
            ProductService _productService = new ProductService();
            string result = _productService.getResultByJobId(jobId,id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}