using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Model
{
    public class AppUser : IdentityUser
    {
        public string City { get; set; }
        public DateTime? BirthDay { get; set; }
        public string PictureUrl { get; set; }
    }
}
