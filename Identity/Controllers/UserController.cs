using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Identity.Domain.Services;
using Identity.ResourceModels;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Identity.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    //[Authorize("CustomSchema")] burada eğer farklı bir token kullanılacaksa o token yapısına ait schema verilir
    [Authorize]
    public class UserController : ControllerBase , IActionFilter
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        public async Task<IActionResult> GetUser()
        {
            //identity gelen token üzerinden db den kullanıcı bilgilerine ulaşarak kullanıma sunar
            var user = await userService.GetUserByUserName(User.Identity.Name);

            return Ok(user.Adapt<UserResourceModel>());//sadece gerekli alanları dönmek için maplendi
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }
        //action çalışırken password'u modelstate içinden kaldırarak zorunluluğunu etkisiz kılacak
        public void OnActionExecuting(ActionExecutingContext context)
        {
            context.ModelState.Remove("password");
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser(UserResourceModel model)
        {
            var result = await userService.UpdateUser(model, User.Identity.Name);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserPicture(IFormFile picture)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(picture.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory() + "wwwrooot/images", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await picture.CopyToAsync(stream);
            }

            var pathObj = new
            {
                path = "https://" + Request.Host + "/images" + fileName

            };

            var result = await userService.UploadPicture(pathObj.path, User.Identity.Name);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result);
        }
    }
}