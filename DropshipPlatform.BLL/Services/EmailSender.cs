using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using DropshipPlatform.Entity;
using DropshipPlatform.BLL.Models;

namespace DropshipPlatform.BLL.Services
{
    public class EmailSender
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public bool SendEmail(byte[] pdfbytes, long orderid)
        {
            bool result = false;
            string toEmail = string.Empty;
            try
            {
                MailMessage message = new MailMessage();
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    user_roles adminRole = datacontext.user_roles.Where(x => x.RoleID == 1).FirstOrDefault();
                    List<user> admin = datacontext.users.Where(x => x.UserID == adminRole.UserID).ToList();
                    if(admin.Count > 0)
                    {
                        foreach (var item in admin)
                        {
                            message.To.Add(item.EmailID);
                        }
                    }
                    else
                    {
                        message.To.Add(StaticValues.AdminEmail);
                    }
                }
               
                string rootUrl = System.Web.HttpContext.Current.Request.Url.Scheme + "://" + System.Web.HttpContext.Current.Request.Url.Authority;
                string redirectUrl = rootUrl + "/Order/getOrders";
                MemoryStream pdfstream = new MemoryStream(pdfbytes);
                string body = "<h2>OrderId:" + orderid + "</h2><h3>There is new order please check it.</h3>";
                body += "<p>Use this link to fullfill orders:<a href='" + redirectUrl + "'>Order Page</a></p>";
                

                message.Subject = "New Order Details";
                message.IsBodyHtml = true;
                message.Body = body;
                message.Attachments.Add(new Attachment(pdfstream, "cainiao_label_" + orderid + ".pdf"));
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Send(message);
                result = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                result = false;
            }
            return result;
        }

        public bool SendFailureEmail(long orderid)
        {
            bool result = false;
            string toEmail = string.Empty;
            try
            {
                MailMessage message = new MailMessage();
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    user_roles adminRole = datacontext.user_roles.Where(x => x.RoleID == 1).FirstOrDefault();
                    List<user> admin = datacontext.users.Where(x => x.UserID == adminRole.UserID).ToList();
                    if (admin.Count > 0)
                    {
                        foreach (var item in admin)
                        {
                            message.To.Add(item.EmailID);
                        }
                    }
                    else
                    {
                        message.To.Add(StaticValues.AdminEmail);
                    }
                }
                
                string body = "<h3>Label creation for order failed.</h3>";
                body += "<p>Please check order" + orderid + "</p>";

                message.Subject = " Creating Label Fail";
                message.IsBodyHtml = true;
                message.Body = body;
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Send(message);
                result = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                result = false;
            }
            return result;
        }
    }
}
