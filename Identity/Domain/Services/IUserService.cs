using Identity.Domain.Response;
using Identity.Model;
using Identity.ResourceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Identity.Domain.Services
{
    public interface IUserService
    {
        Task<BaseResponse<UserResourceModel>> UpdateUser(UserResourceModel model, string userName);
        Task<AppUser> GetUserByUserName(string userName);
        Task<BaseResponse<AppUser>> UploadPicture(string path, string userName);
        Task<BaseResponse<Tuple<AppUser, IList<Claim>>>> GetUserByRefreshToken(string refreshToken);
        Task<bool> RevokeRefreshToken(string refreshToken);
    }
}
