using DropshipPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropshipPlatform.BLL.Services
{
    public class AliExpressJobLogService
    {

        public long? AddAliExpressJobLog(AliExpressJobLog aliExpressJobLog)
        {
            using (DropshipDataEntities datacontext = new DropshipDataEntities())
            {
                aliExpressJobLog.CreatedOn = DateTime.Now;
                datacontext.AliExpressJobLogs.Add(aliExpressJobLog);
                datacontext.SaveChanges();
                return aliExpressJobLog.JobId;
            }
        }
    }
}
