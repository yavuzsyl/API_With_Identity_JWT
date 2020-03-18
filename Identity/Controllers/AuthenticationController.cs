using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identity.Domain.Response;
using Identity.Domain.Services;
using Identity.ResourceModels;
using Identity.Security.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService authService;

        public AuthenticationController(IAuthenticationService authService)
        {
            this.authService = authService;
        }

        //gelen request headerındaki token payloadından user id ile identity kullanıcının giriş yapıp yapmadığı kontrol edecek
        [HttpGet]
        [Authorize]
        public IActionResult IsAuthenticated() => Ok(User.Identity.IsAuthenticated);

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInResourceModel model)
        {
            var result = await authService.SignIn(model);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok(result);

        }
        [HttpPost]
        public async Task<IActionResult> SignUp(UserResourceModel model)
        {
            var result = await authService.SignUp(model);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> TokenByRefreshToken(RefreshTokenResourceModel model)
        {
            var result = await authService.CreateAccessTokenWithRefreshToken(model);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> RevokeRefreshToken(RefreshTokenResourceModel model)
        {
            var result = await authService.RevokeRefreshToken(model);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok(result);
        }
    }
}