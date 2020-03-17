using Identity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Security.Token
{
    public interface ITokenHandler
    {
        AccessToken CreateAccessToken(AppUser user);
        /// <summary>
        /// Safe exit
        /// </summary>
        /// <param name="user"></param>
        void RevokeRefreshToken(AppUser user);
    }
}
