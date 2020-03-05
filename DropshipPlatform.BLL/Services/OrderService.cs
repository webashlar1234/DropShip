using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropshipPlatform.BLL.Models;
using DropshipPlatform.Entity;
using Newtonsoft.Json;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace DropshipPlatform.BLL.Services
{
    public class OrderService
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ResultData getAllOrders()
        {
            ResultData orders =new ResultData();
            string result = String.Empty;
            try
            {
                ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);
                AliexpressSolutionOrderGetRequest req = new AliexpressSolutionOrderGetRequest();

                AliexpressSolutionOrderGetRequest.OrderQueryDomain obj1 = new AliexpressSolutionOrderGetRequest.OrderQueryDomain();
                obj1.CreateDateEnd = "2020-03-05 12:12:12";
                obj1.CreateDateStart = "2020-03-01 12:12:12";
                obj1.ModifiedDateStart = "2020-03-01 12:12:12";
                obj1.OrderStatusList = new List<string> { "SELLER_PART_SEND_GOODS", "PLACE_ORDER_SUCCESS", "IN_CANCEL", "WAIT_SELLER_SEND_GOODS", "WAIT_BUYER_ACCEPT_GOODS", "FUND_PROCESSING" , "IN_ISSUE", "IN_FROZEN", "WAIT_SELLER_EXAMINE_MONEY", "RISK_CONTROL", "FINISH" };
                obj1.BuyerLoginId = "edacan0107@aol.com";
                obj1.PageSize = 20L;
                obj1.ModifiedDateEnd = "2020-03-05 12:12:12"; 
                obj1.CurrentPage = 1L;
                obj1.OrderStatus = "SELLER_PART_SEND_GOODS";
                req.Param0_ = obj1;
                Top.Api.Response.AliexpressSolutionOrderGetResponse rsp = client.Execute(req, SessionManager.GetAccessToken().access_token);
                result = JsonConvert.SerializeObject(rsp.Result);
                orders = JsonConvert.DeserializeObject<ResultData>(result);

            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return orders;

        }

        public List<Product> GetProductById(string Id)
        {
            List<Product> products = new List<Product>();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    products = datacontext.Products.Where(x=>x.OriginalProductID == Id).ToList();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return products;
        }
    }
}
