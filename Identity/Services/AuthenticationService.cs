using Identity.Domain.Response;
using Identity.Domain.Services;
using Identity.Model;
using Identity.ResourceModels;
using Identity.Security.Token;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Identity.Services
{
    public class AuthenticationService : BaseService, IAuthenticationService
    {
        private readonly ITokenHandler tokenHandler;
        private readonly AppTokenOptions tokenOptions;
        private readonly IUserService userService;
        public AuthenticationService(ITokenHandler tokenHandler, IUserService userService, IOptions<AppTokenOptions> options, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppUser> roleManager) : base(userManager, signInManager, roleManager)
        {
            this.tokenHandler = tokenHandler;
            this.tokenOptions = options.Value;
            this.userService = userService;
        }

        public async Task<BaseResponse<AccessToken>> CreateAccessTokenWithRefreshToken(RefreshTokenResourceModel model)
        {
            var user = await userService.GetUserByRefreshToken(model.RefreshToken);
            if (!user.Success)
                return new BaseResponse<AccessToken>(user.Message);

            var token = tokenHandler.CreateAccessToken(user.Result.Item1);

            //yeni token ile refreshtoken claimleri güncellenecek
            Claim refreshTokenClaim = new Claim("refreshToken", token.RefreshToken);
            Claim refreshTokenEndDate = new Claim("refreshTokenEndDate", DateTime.Now.AddMinutes(tokenOptions.RefreshTokenExpiration).ToString());

            var refReplace = await userManager.ReplaceClaimAsync(user.Result.Item1, user.Result.Item2.FirstOrDefault(c => c.Type == "refreshToken"), refreshTokenClaim);
            if (!refReplace.Succeeded)
                return new BaseResponse<AccessToken>("refreshtoken güncellenemedi");
            var refDateReplace = await userManager.ReplaceClaimAsync(user.Result.Item1, user.Result.Item2.FirstOrDefault(c => c.Type == "refreshTokenEndDate"), refreshTokenEndDate);
            if (!refDateReplace.Succeeded)
                return new BaseResponse<AccessToken>("refreshtokenDate güncellenemedi");

            return new BaseResponse<AccessToken>(token);

        }

        public async Task<BaseResponse<AccessToken>> RevokeRefreshToken(RefreshTokenResourceModel model)
        {
            var result = await userService.RevokeRefreshToken(model.RefreshToken);
            if (!result)
                return new BaseResponse<AccessToken>("Fail couldnt revoke refreshtoken");
            //refreshtoken silinirse eğer bos accesstoken dönecek bunu daha sonra düzenleyeceğim niggas
            return new BaseResponse<AccessToken>(new AccessToken());

        }

        public async Task<BaseResponse<AccessToken>> SignIn(SignInResourceModel model)
        {
            var user = await userManager.FindByNameAsync(model.Email);
            if (user == null)
                return new BaseResponse<AccessToken>("No user with this email");

            var isPasswordValidForUser = await userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValidForUser)
                return new BaseResponse<AccessToken>("Wrong password");

            //token oluşturulur
            var token = tokenHandler.CreateAccessToken(user);

            //refresh token claim olarak claims table'a insert edilecek
            Claim refreshTokenClaim = new Claim("refreshToken", token.RefreshToken);
            Claim refreshTokenEndDate = new Claim("refreshTokenEndDate", DateTime.Now.AddMinutes(tokenOptions.RefreshTokenExpiration).ToString());

            //kullanıcıya ait refreshtoken claimleri varsa kontrol edilecek
            List<Claim> refreshClaimList = userManager.GetClaimsAsync(user).Result.Where(c => c.Type.Contains("refreshToken")).ToList();

            //eğer kullanıya ait refreshtoken claimleri varsa bu claimler güncellenir
            if (refreshClaimList.Any())
            {
                var refReplace = await userManager.ReplaceClaimAsync(user, refreshClaimList.FirstOrDefault(c => c.Type == "refreshToken"), refreshTokenClaim);
                if (!refReplace.Succeeded)
                    return new BaseResponse<AccessToken>("refreshtoken güncellenemedi");
                var refDateReplace = await userManager.ReplaceClaimAsync(user, refreshClaimList.FirstOrDefault(c => c.Type == "refreshTokenEndDate"), refreshTokenEndDate);
                if (!refDateReplace.Succeeded)
                    return new BaseResponse<AccessToken>("refreshtokenDate güncellenemedi");

            }
            else
            {
                List<Claim> claims = new List<Claim>();
                claims.Add(refreshTokenClaim);
                claims.Add(refreshTokenEndDate);

                var result = await userManager.AddClaimsAsync(user, claims);
                if (!result.Succeeded)
                    return new BaseResponse<AccessToken>(result.Errors.First().Description);
            }

            return new BaseResponse<AccessToken>(token);
        }

        public async Task<BaseResponse<UserResourceModel>> SignUp(UserResourceModel model)
        {
            var newUser = new AppUser() { UserName = model.UserName, Email = model.Email };

            IdentityResult result = await userManager.CreateAsync(newUser);

            if (!result.Succeeded)
                return new BaseResponse<UserResourceModel>(result.Errors.First().Description);

            return new BaseResponse<UserResourceModel>(newUser.Adapt<UserResourceModel>());

        }
    }
}
