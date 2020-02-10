﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropshipPlatform.BLL.Models
{
    public class UserModel
    {
        public int UserID { get; set; }
        public string Name { get; set; }
        public string EmailID { get; set; }
        public string Phone { get; set; }
        public int CountryID { get; set; }
        public string Password { get; set; }
        public string AliExpressSellerID { get; set; }
        public bool IsPolicyAccepted { get; set; }
        public string StripeCustomerID { get; set; }
    }

    public class RegisterUserModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int Country { get; set; }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}