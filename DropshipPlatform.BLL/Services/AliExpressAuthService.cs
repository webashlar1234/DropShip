using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;
using Top.Api.Util;

namespace DropshipPlatform.BLL.Services
{
    public class AliExpressAuthService 
    {
        string appkey = System.Web.Configuration.WebConfigurationManager.AppSettings["AliExpress_appKey"].ToString();
        string secret = System.Web.Configuration.WebConfigurationManager.AppSettings["AliExpress_appSecreat"].ToString();
        string redirectURL = System.Web.Configuration.WebConfigurationManager.AppSettings["AliExpress_RedirectURL"].ToString();
        string url = System.Web.Configuration.WebConfigurationManager.AppSettings["AliExpress_URL"].ToString();

        public string getAuthorizeUrl()
        {
            string authorizeUrl = "https://oauth.aliexpress.com/authorize?response_type=code&client_id=" + appkey + "&redirect_uri="+ redirectURL + "&sp=ae";
            return authorizeUrl;
        }
        public AliExpressAccessToken getAccessToken(string code) {

            ITopClient client = new DefaultTopClient(url, appkey, secret);
            TopAuthTokenCreateRequest req = new TopAuthTokenCreateRequest();
            req.Code = code;
            TopAuthTokenCreateResponse rsp = client.Execute(req);
            AliExpressAccessToken accessToken = JsonConvert.DeserializeObject<AliExpressAccessToken>(rsp.TokenResult);
            
            return accessToken;
        }
    }
}
