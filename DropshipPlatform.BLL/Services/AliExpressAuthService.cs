using DropshipPlatform.BLL.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
            string authorizeUrl = "https://oauth.aliexpress.com/authorize?response_type=code&client_id=" + StaticValues.aliAppkey + "&redirect_uri=" + StaticValues.aliRedirectURL + "&sp=ae";
            return authorizeUrl;
        }
        public AliExpressAccessToken getAccessToken(string code)
        {

            //AliExpressAccessToken accessToken1 = fetchtoken(code);
            //string authorizeUrl = "https://oauth.aliexpress.com/token?response_type=code&client_id=" + StaticValues.aliAppkey + "&redirect_uri=" + StaticValues.aliRedirectURL + "&sp=ae&code=" + code;


            //ITopClient client1 = new DefaultTopClient("http://gw.api.taobao.com/router/rest", StaticValues.aliAppkey, StaticValues.aliSecret, "json");
            //TimeGetRequest req1 = new TimeGetRequest();
            //TimeGetResponse rsp1 = client1.Execute(req1);

            //ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);
            //TopAuthTokenCreateRequest req = new TopAuthTokenCreateRequest();
            //req.Code = code;
            //TopAuthTokenCreateResponse rsp = client.Execute(req);
            string result = fetchtoken(code);
            AliExpressAccessToken accessToken = JsonConvert.DeserializeObject<AliExpressAccessToken>(result);

            return accessToken;
        }


        public static AliExpressAccessToken gettoken(string code)
        {

            AliExpressAccessToken accessToken;
            string tokeUrl = "https://oauth.aliexpress.com/token?response_type=token&grant_type=authorization_code&client_id=" + StaticValues.aliAppkey + "&redirect_uri=" + StaticValues.aliRedirectURL + "&sp=ae&code=" + code + "&client_secret=" + StaticValues.aliSecret;
            string tokeUrl1 = "https://oauth.aliexpress.com/authorize?response_type=token&client_id=" + StaticValues.aliAppkey + "&sp=ae";
            // Create a request using a URL that can receive a post.   
            WebRequest request = WebRequest.Create(tokeUrl1);
            // Set the Method property of the request to POST.  
            request.Method = "POST";

            // Create POST data and convert it to a byte array.  
            string postData = "This is a test that posts this string to a Web server.";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            // Set the ContentType property of the WebRequest.  
            request.ContentType = "application/x-www-form-urlencoded";
            // Set the ContentLength property of the WebRequest.  
            request.ContentLength = byteArray.Length;

            // Get the request stream.  
            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream.  
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.  
            dataStream.Close();

            // Get the response.  
            WebResponse response = request.GetResponse();
            // Display the status.  
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);

            // Get the stream containing content returned by the server.  
            // The using block ensures the stream is automatically closed.
            using (dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.  
                string responseFromServer = reader.ReadToEnd();
                accessToken = JsonConvert.DeserializeObject<AliExpressAccessToken>(responseFromServer);
                // Display the content.  
                Console.WriteLine(responseFromServer);
            }

            // Close the response.  
            response.Close();
            return accessToken;
        }

        public static string fetchtoken(string code)
        {
            WebUtils webUtils = new WebUtils();
            IDictionary<string, string> pout = new Dictionary<string, string>();
            pout.Add("grant_type", "authorization_code");
            pout.Add("client_id", StaticValues.aliAppkey);
            pout.Add("client_secret", StaticValues.aliSecret);
            pout.Add("sp", "ae");
            pout.Add("code", code);
            pout.Add("redirect_uri", StaticValues.aliRedirectURL);
            string output = webUtils.DoPost("https://oauth.aliexpress.com/token", pout);
            return output;
        } 

    }
}
