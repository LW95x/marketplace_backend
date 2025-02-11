using Marketplace.BusinessLayer;
using Marketplace.Helpers;
using Marketplace.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Marketplace.Controllers
{
    [Route("auth")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserAuthController> _logger;
        private readonly IConfiguration _configuration;

        public UserAuthController(IUserService userService, ILogger<UserAuthController> logger, IConfiguration configuration)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ??
                throw new ArgumentNullException(nameof(configuration));
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
                var user = await _userService.FetchUserByUsernameAsync(userForLoginDto.UserName);

                if (user == null || user.Id == null || user.UserName == null || user.Email == null)
                {
                    throw new InvalidOperationException("User information is incomplete, preventing the generation of a JWT Authorisation Token.");
                }

                var secretKey = _configuration["Authentication:SecretForKey"];

                if (string.IsNullOrEmpty(secretKey))
                {
                    throw new InvalidOperationException("Secret key is missing from configuration.");
                }

                var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(secretKey));
                var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var tokenClaims = new List<Claim>();
                tokenClaims.Add(new Claim("sub", user.Id.ToString()));
                tokenClaims.Add(new Claim("user_name", user.UserName));
                tokenClaims.Add(new Claim("email", user.Email));

                var jwtSecurityToken = new JwtSecurityToken(
                _configuration["Authentication:Issuer"],
                _configuration["Authentication:Audience"],
                tokenClaims,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(3),
                signingCredentials);

                var returnedToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

                return Ok(returnedToken);
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
