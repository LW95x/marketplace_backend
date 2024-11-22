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
        private readonly ILogger<UserAuthController> _logger;

        public UserAuthController(IUserService userService, ILogger<UserAuthController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Attempts to login the user with provided username and password.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LoginUser([FromBody] UserForLoginDto userForLoginDto)
        {

            if (!ModelState.IsValid)
            {
                _logger.LogError($"Validation has failed for POST Login request.");
                return BadRequest(ModelState);
            }

            var result = await _userService.LoginUser(userForLoginDto.UserName, userForLoginDto.Password);

            if (result == true)
            {
                return Ok();
            }
            else
            {
                _logger.LogError($"Username or password provided to login method was incorrect.");
                return Unauthorized("Username or password provided is incorrect.");
            }
        }
        /// <summary>
        /// Attempts to logout an existing user.
        /// </summary>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LogoutUser()
        {
            var result = await _userService.LogoutUser();

            if (result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                _logger.LogError($"Failed to log out user.");
                return StatusCode(500, result.Error);
            }
        }
    }
}
