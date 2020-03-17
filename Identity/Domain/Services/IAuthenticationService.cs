using Identity.Domain.Response;
using Identity.ResourceModels;
using Identity.Security.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Domain.Services
{
    public interface IAuthenticationService
    {
        Task<BaseResponse<UserResourceModel>> SignUp(UserResourceModel model);
        Task<BaseResponse<AccessToken>> SignIn(SignInResourceModel model);
        /// <summary>
        /// Token expire olduğu zaman çağırılacak metot
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<BaseResponse<AccessToken>> CreateAccessTokenWithRefreshToken(RefreshTokenResourceModel model);
        /// <summary>
        /// Güvenli çıkış refresh token silinir
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<BaseResponse<AccessToken>> RevokeRefreshToken(RefreshTokenResourceModel model);
    }
}
