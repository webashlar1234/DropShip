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

        public static string stripeTestAPIKey = System.Web.Configuration.WebConfigurationManager.AppSettings["Stripe_test_Apikey"].ToString();
        public static string stripeTestPublishKey = System.Web.Configuration.WebConfigurationManager.AppSettings["Stripe_test_Publishkey"].ToString();
        public static string stripeAPIKey = System.Web.Configuration.WebConfigurationManager.AppSettings["Stripe_Apikey"].ToString();

        public static string stripeTestSecretKey = System.Web.Configuration.WebConfigurationManager.AppSettings["Stripe_testSecretKey"].ToString();

        //public static string sampleImage = "https://www.begorgeousstylesandbeauty.com/wp-content/uploads/2016/01/2015-100-High-Quality-Mens-Dress-Shirts-Blue-Shirt-Men-Causal-Striped-Shirt-Men-Camisa-Social.jpg";
        public static string sampleImage = "http://img1.how01.com/imgs/71/f7/0/4aec0005dcbc40cd378b.jpg";
        public static string serviceTemplateID = "0";
        public static int shippingPreparationTime = 20;
        public static string shippingTemplateID = "1013213014";
        public static string mySqlDb = System.Web.Configuration.WebConfigurationManager.AppSettings["MySqlConnection"].ToString();
        public static string CainiaoFiles_path = System.Web.Configuration.WebConfigurationManager.AppSettings["CainiaoFiles_path"].ToString();

        public static string seller = "Seller";
        public static string admin = "Admin";
        public static string operationalManager = "Operational Manager";

        public static string Unpurchased = "Unpurchased";
        public static string Shipped = "Shipped";
        public static string Waiting_for_Shipment = "Waiting for Shipment";
    }
}
