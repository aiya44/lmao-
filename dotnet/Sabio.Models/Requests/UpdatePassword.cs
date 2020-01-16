using System;
using System.Collections.Generic;
using System.Text;

namespace Sabio.Models.Requests
{
    public class UpdatePassword
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
