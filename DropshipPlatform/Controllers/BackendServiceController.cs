using DropshipPlatform.BLL.Services;
using DropshipPlatform.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace DropshipPlatform.Controllers
{
    public class BackendServiceController : Controller
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static object Lock = new object();
        [HttpPost]
        [AjaxFilter]
        public JsonResult Index()
        {
            if (!Monitor.TryEnter(Lock, new TimeSpan(0)))
            {
                logger.Info("BackendServic called returned");
                return new JsonResult { Data = "InUse" };
            }
            try
            {
                logger.Info("BackendServic called: " + SessionManager.GetUserSession().UserID);
                new BackendHelper().RefreshAliExpressJobLog();
                new BackendHelper().RefreshAliExpressInventory();
                new BackendHelper().RefreshAliExpressOrders();
                new OrderService().checkWarehouceOrderStatus();
            }
            finally
            {
                Monitor.Exit(Lock);
            }

            return new JsonResult { Data = "Success" };
        }
    }
}