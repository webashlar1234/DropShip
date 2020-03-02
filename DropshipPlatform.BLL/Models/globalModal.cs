using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropshipPlatform.BLL.Models
{
    public class globalModal
    {

    }

    public class simpleModel
    {
        public int key { get; set; }
        public int value { get; set; }
    }

    public class multiLanguage
    {
        public string de { get; set; }
        public string ru { get; set; }
        public string pt { get; set; }
        public string en { get; set; }
        public string it { get; set; }
        public string fr { get; set; }
        public string es { get; set; }
        public string tr { get; set; }
        public string nl { get; set; }
    }

    public class scproductModel
    {
        public int productId { get; set; }
        public List<ProductSKUModel> SKUModels { get; set; }
        public double price { get; set; }
    }

    public class ProductSKUModel
    {
        public string skuCode { get; set; }
        public int inventory { get; set; }
        public double price { get; set; }
        public double discount_price { get; set; }


    }

}
