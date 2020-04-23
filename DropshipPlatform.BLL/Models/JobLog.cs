using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropshipPlatform.BLL.Models
{
    public class JobLog
    {
        public int Id { get; set; }
        public string JobId { get; set; }
        public string SuccessItemCount { get; set; }
        public string ContentId { get; set; }
        public string Result { get; set; }
        public string ErrorType { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
    }
}
