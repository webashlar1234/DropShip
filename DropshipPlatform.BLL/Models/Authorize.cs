using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropshipPlatform.BLL
{
    public class Authorize
    {
    }

    public class AliExpressAccessToken
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string w1_valid { get; set; }
        public string refresh_token_valid_time { get; set; }
        public string w2_valid { get; set; }
        public string user_id { get; set; }
        public string expire_time { get; set; }
        public string r2_valid { get; set; }
        public string locale { get; set; }
        public string r1_valid { get; set; }
        public string sp { get; set; }
        public string user_nick { get; set; }

    }
}
