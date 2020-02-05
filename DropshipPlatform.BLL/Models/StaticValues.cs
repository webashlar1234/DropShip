using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropshipPlatform.BLL.Models
{
    public static class StaticValues
    {
        public static string aliAppkey = System.Web.Configuration.WebConfigurationManager.AppSettings["AliExpress_appKey"].ToString();
        public static string aliSecret = System.Web.Configuration.WebConfigurationManager.AppSettings["AliExpress_appSecreat"].ToString();
        public static string aliRedirectURL = System.Web.Configuration.WebConfigurationManager.AppSettings["AliExpress_RedirectURL"].ToString();
        public static string aliURL = System.Web.Configuration.WebConfigurationManager.AppSettings["AliExpress_URL"].ToString();
    }
}
