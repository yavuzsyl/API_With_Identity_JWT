using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Security.Token
{
    public class SignHandler
    {
        public static SecurityKey GetSecurityKey(string securiryKey)
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securiryKey));
        }
    }
}
