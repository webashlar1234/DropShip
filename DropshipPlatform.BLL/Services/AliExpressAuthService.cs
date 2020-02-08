using DropshipPlatform.BLL.Models;
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
        public string getAuthorizeUrl()
        {
            string authorizeUrl = "https://oauth.aliexpress.com/authorize?response_type=code&client_id=" + StaticValues.aliAppkey + "&redirect_uri="+ StaticValues.aliRedirectURL + "&sp=ae";
            return authorizeUrl;
        }
        public AliExpressAccessToken getAccessToken(string code) {

            ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);
            TopAuthTokenCreateRequest req = new TopAuthTokenCreateRequest();
            req.Code = code;
            TopAuthTokenCreateResponse rsp = client.Execute(req);
            AliExpressAccessToken accessToken = JsonConvert.DeserializeObject<AliExpressAccessToken>(rsp.TokenResult);
            
            return accessToken;
        }
    }
}
