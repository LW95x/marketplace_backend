using Marketplace.BusinessLayer;
using Marketplace.Helpers;
using Marketplace.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace Marketplace.Controllers
{
    [Route("auth")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserAuthController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }


        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] UserForLoginDto userForLoginDto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.LoginUser(userForLoginDto.UserName, userForLoginDto.Password);

            if (result == true)
            {
                return Ok();
            }
            else
            {
                return Unauthorized("Username or password provided is incorrect.");
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> LogoutUser()
        {
            var result = await _userService.LogoutUser();

            if (result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                return BadRequest(result.Error);
            }
        }
    }
}
