using DropshipPlatform.BLL;
using DropshipPlatform.BLL.Models;
using DropshipPlatform.BLL.Services;
using DropshipPlatform.Entity;
using DropshipPlatform.Infrastructure;
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
                ////get expired date
                //TimeSpan timespan = TimeSpan.FromMilliseconds(float.Parse(accessToken.expire_time));
                //DateTime dt = DateTime.Now.Add(timespan);

                new UserService().UpdateUserForAliExpress(SessionManager.GetUserSession().UserID, accessToken);
                SessionManager.SetAccessToken(accessToken);
            }
            return RedirectToAction("Index", "AliExpress");
        }

        [HttpGet]
        public JsonResult checkResultByJobId(long id)
        {
            ProductService _productService = new ProductService();
            string result = _productService.checkResultByJobId(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getJobList()
        {
            ProductService _productService = new ProductService();
            string result = _productService.getJobList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize("Admin", "Operational Manager", "Developer")]
        public ActionResult jobLog()
        {
            return View();
        }

        [HttpPost]
        public ActionResult getJobLogData()
        {
            ProductService _productService = new ProductService();
            int recordsTotal = 0;
            var draw = Request.Form.GetValues("draw") != null ? Request.Form.GetValues("draw").FirstOrDefault() : null;
            var start = Request.Form.GetValues("start") != null ? Request.Form.GetValues("start").FirstOrDefault() : null;
            var length = Request.Form.GetValues("length") != null ? Request.Form.GetValues("length").FirstOrDefault() : null;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            List<aliexpressjoblog> resultList = _productService.getJobLogData();
            List<JobLog> retvalue = resultList.Select(x => new JobLog()
            {
                Id = x.Id,
                JobId = x.JobId.ToString(),
                SuccessItemCount = x.SuccessItemCount.ToString(),
                ContentId = x.ContentId,
                Result = x.Result,
                CreatedOn = x.CreatedOn,
                CreatedBy = x.CreatedBy
            }).ToList();
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

        [HttpPost]
        public JsonResult updateJobLogResult(aliexpressjoblog aliExpressJobLog)
        {
            ProductService _productService = new ProductService();
            return Json(_productService.updateJobLogResult(aliExpressJobLog), JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult getResultByJobId(long id)
        {
            ProductService _productService = new ProductService();
            string result = _productService.getResultByJobId(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}