using DropshipPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropshipPlatform.BLL.Services
{
    public class MemberShipService
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public membershiptype GetMemberShipDetail(int PlanId)
        {
            membershiptype obj = new membershiptype();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    obj = datacontext.membershiptypes.Where(m => m.MembershipID == PlanId).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                obj = null;
                logger.Error(ex.ToString());
            }
            return obj;
        }

        public List<membershiptype> GetMemberShipData()
        {
            List<membershiptype> obj = new List<membershiptype>();
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    obj = datacontext.membershiptypes.ToList();
                }
            }
            catch (Exception ex)
            {
                obj = null;
                logger.Error(ex.ToString());
            }
            return obj;
        }
    }
}
