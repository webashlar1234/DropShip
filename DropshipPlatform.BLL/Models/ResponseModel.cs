using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropshipPlatform.BLL.Models
{
    public class ResponseModel
    {
        public Object Data { get; set; }
        public bool IsSuccess { get; set; }
        public string Message  { get; set; }
    }
}
