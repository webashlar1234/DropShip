using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropshipPlatform.BLL.SubscriptionModels
{
    public class PlanViewModel
    {
        public string name { get; set; }
        public long amount { get; set; }
        public string currency { get; set; }
        public string interval { get; set; }
        public int pickLimit { get; set; }
        public string description { get; set; }
    }
}
