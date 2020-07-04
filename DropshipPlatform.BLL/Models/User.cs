using DropshipPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropshipPlatform.BLL.Models
{
    public class RegisterUserModel
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public int RoleID { get; set; }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoggedUserModel : user
    {
        public string LoggedUserRoleName { get; set; }
        public int LoggedUserRoleID { get; set; }
        public string SubscriptionID { get; set; }
    }

    public class Seller : user
    {
        public string Membership { get; set; }
        public int NoOfPickedProducts { get; set; }
    }
}
