using Identity.Domain.Response;
using Identity.Domain.Services;
using Identity.Model;
using Identity.ResourceModels;
using Mapster;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Identity.Services
{
    public class UserSerivce : BaseService, IUserService
    {
        public UserSerivce(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppUser> roleManager) : base(userManager, signInManager, roleManager)
        {
        }

        public async Task<BaseResponse<Tuple<AppUser, IList<Claim>>>> GetUserByRefreshToken(string refreshToken)
        {
            Claim claimRefrehsToken = new Claim("refreshToken", refreshToken);

            var users = await userManager.GetUsersForClaimAsync(claimRefrehsToken);
            if (!users.Any())
                return new BaseResponse<Tuple<AppUser, IList<Claim>>>("There is no user with this refreshtoken");

            var user = users.First();
            IList<Claim> claims = await userManager.GetClaimsAsync(user);

            string refreshTokenEndDate = claims.First(c => c.Type == "refreshTokenEndDate").Value;//userın claimlerinin içinden refreshtokenenddate claimine bakılarak refToken süresi kontrol edilir
            if (DateTime.Parse(refreshTokenEndDate) < DateTime.Now)
                return new BaseResponse<Tuple<AppUser, IList<Claim>>>("Refreshtoken has expired");

            return new BaseResponse<Tuple<AppUser, IList<Claim>>>(new Tuple<AppUser, IList<Claim>>(user, claims));


        }

        public async Task<AppUser> GetUserByUserName(string userName)
        {
            return await userManager.FindByNameAsync(userName);
        }

        public async Task<bool> RevokeRefreshToken(string refreshToken)
        {
            var user = await GetUserByRefreshToken(refreshToken);
            if (!user.Success || user.Result.Item1 == null)
                return false;
            //kullanıcıya ait claimler silinecek refreshtoken - refreshtoken expire time vs ...
            IdentityResult result = await userManager.RemoveClaimsAsync(user.Result.Item1, user.Result.Item2);
            if (!result.Succeeded)
                return false;
            return true;

        }

        public async Task<BaseResponse<UserResourceModel>> UpdateUser(UserResourceModel model, string userName)
        {
            AppUser appUser = await userManager.FindByNameAsync(userName);
            if (appUser != null)
                return new BaseResponse<UserResourceModel>("There is no user such this username");
            if (userManager.Users.Any(x => x.PhoneNumber == model.PhoneNumber))
                return new BaseResponse<UserResourceModel>("This phone number already in use");

            appUser.BirthDay = model.BirthDate;
            appUser.City = model.City;
            appUser.Email = model.Email;//default her email unique identity
            appUser.UserName = model.UserName;//default her username unique identity
            appUser.PhoneNumber = model.PhoneNumber;

            IdentityResult result = await userManager.UpdateAsync(appUser);
            if (!result.Succeeded)
                return new BaseResponse<UserResourceModel>(result.Errors.First().Description);
            //mapster adapt metodu ile maplendi
            return new BaseResponse<UserResourceModel>(appUser.Adapt<UserResourceModel>());

        }

        public async Task<BaseResponse<AppUser>> UploadPicture(string path, string userName)
        {
            var appUser = await userManager.FindByNameAsync(userName);
            if (appUser != null)
                return new BaseResponse<AppUser>("There is no user such this username");

            appUser.PictureUrl = path;
            IdentityResult result = await userManager.UpdateAsync(appUser);
            
            if (!result.Succeeded)
                return new BaseResponse<AppUser>(result.Errors.First().Description);

            return new BaseResponse<AppUser>(appUser);
        }
    }
}
