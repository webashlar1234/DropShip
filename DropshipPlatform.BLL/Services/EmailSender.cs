using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using DropshipPlatform.Entity;

namespace DropshipPlatform.BLL.Services
{
    public class EmailSender
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public bool SendEmail(string pdf, string orderid)
        {
            bool result = false;
            string toEmail = string.Empty;
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    user_roles adminRole = datacontext.user_roles.Where(x => x.RoleID == 1).FirstOrDefault();
                    user admin = datacontext.users.Where(x => x.UserID == adminRole.UserID).FirstOrDefault();
                    toEmail = admin.EmailID;
                }
                //pdf = File.ReadAllText("C:\\Users\\Lenovo\\Downloads\\cainiaoByteArr.txt");
                string fromEmail = "scpplatform@hotmail.com";
                string password = "Temp1001";
                //toEmail = "pritiagarwal.wa@gmail.com";
                //orderid = "3003330237457587";

                string rootUrl = System.Web.HttpContext.Current.Request.Url.Scheme + "://" + System.Web.HttpContext.Current.Request.Url.Authority;
                string redirectUrl = rootUrl + "/Order/getOrders";
                byte[] pdfbytes = Convert.FromBase64String(pdf);
                MemoryStream pdfstream = new MemoryStream(pdfbytes);
                string body = "<h2>OrderId:" + orderid + "</h2><h3>There is new order please check it.</h3>";
                body += "<p>Use this link to fullfill orders:<a href='" + redirectUrl + "'>Order Page</a></p>";

                MailMessage message = new MailMessage();
                message.To.Add(toEmail);
                message.From = new System.Net.Mail.MailAddress(fromEmail);
                message.Subject = "New Order Details";
                message.IsBodyHtml = true;
                message.Body = body;
                message.Attachments.Add(new Attachment(pdfstream, "cainiao_label_" + orderid + ".pdf"));
                System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient("smtp.live.com");
                smtpClient.Port = 587;//587
                smtpClient.UseDefaultCredentials = false;
                smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                smtpClient.Credentials = new System.Net.NetworkCredential(fromEmail, password);
                smtpClient.EnableSsl = true;
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

        public bool SendFailureEmail(string orderid)
        {
            bool result = false;
            string toEmail = string.Empty;
            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    user_roles adminRole = datacontext.user_roles.Where(x => x.RoleID == 1).FirstOrDefault();
                    user admin = datacontext.users.Where(x => x.UserID == adminRole.UserID).FirstOrDefault();
                    toEmail = admin.EmailID;
                }
                string fromEmail = "scpplatform@hotmail.com";
                string password = "Temp1001";
                //toEmail = "pritiagarwal.wa@gmail.com";
                //orderid = "3003330237457587";
                string body = "<h3>Label creation for order failed.</h3>";
                body += "<p>Please check order" + orderid + "</p>";

                MailMessage message = new MailMessage();
                message.To.Add(toEmail);
                message.From = new System.Net.Mail.MailAddress(fromEmail);
                message.Subject = " Creating Label Fail";
                message.IsBodyHtml = true;
                message.Body = body;
                System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient("smtp.live.com");
                smtpClient.Port = 587;//587
                smtpClient.UseDefaultCredentials = false;
                smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                smtpClient.Credentials = new System.Net.NetworkCredential(fromEmail, password);
                smtpClient.EnableSsl = true;
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
