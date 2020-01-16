using System;
using System.Collections.Generic;
using System.Text;

namespace Sabio.Models.Requests
{
    public class LoginRequest
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }
    }
}
