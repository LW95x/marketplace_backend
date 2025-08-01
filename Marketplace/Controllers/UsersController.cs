﻿using AutoMapper;
using Marketplace.BusinessLayer;
using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;
using Marketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Marketplace.Controllers
{
    [Route("/users")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, UserManager<User> userManager, IEmailService emailService, IMapper mapper, ILogger<UsersController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        /// <summary>
        /// Get all users.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserForResponseDto>>> GetUsers()
        {
            var users = await _userService.FetchUsersAsync();

            return Ok(_mapper.Map<IEnumerable<UserForResponseDto>>(users));
        }
        /// <summary>
        /// Get a specific user.
        /// </summary>
        [HttpGet("{userId}", Name = "GetUserById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetUserById(string userId)
        {
            var user = await _userService.FetchUserByIdAsync(userId);

            if (user == null)
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This user ID does not exist.");
            }

            return Ok(_mapper.Map<UserForResponseDto>(user));
        }
        /// <summary>
        /// Get a specific user by user name.
        /// </summary>
        [HttpGet("username")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetUserByUserName([FromQuery] string userName)
        {
            var user = await _userService.FetchUserByUsernameAsync(userName);

            if (user == null)
            {
                _logger.LogError($"User with username {userName} wasn't found.");
                return NotFound("This username does not exist.");
            }

            return Ok();
        }
        /// <summary>
        /// Create a new user.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateUser([FromBody] UserForCreationDto userDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Validation has failed for POST User request.");
                return BadRequest(ModelState);
            }

            var user = _mapper.Map<User>(userDto);
            var result = await _userService.AddUser(user, userDto.Password);

            if (result.Succeeded)
            {
                var userResponseDto = _mapper.Map<UserForResponseDto>(user);
                return CreatedAtRoute("GetUserById", new { userId = user.Id }, userResponseDto);
            }
            else
            {
                _logger.LogCritical($"Failed to create a new user.");
                return StatusCode(500, result.Errors);
            }
        }
        /// <summary>
        /// Delete an existing user.
        /// </summary>
        [HttpDelete("{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userService.FetchUserByIdAsync(userId);

            if (user == null)
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This user ID does not exist.");
            }

           var result = await _userService.RemoveUser(user);

            if (result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                _logger.LogCritical($"Failed to delete existing user.");
                return StatusCode(500, result.Errors);
            }
        }
        /// <summary>
        /// Change the password of an existing user.
        /// </summary>
        [HttpPost("{userId}/change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ChangeUserPassword(string userId, string currentPassword, string newPassword)
        {
            var user = await _userService.FetchUserByIdAsync(userId);

            if (user == null)
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This user ID does not exist.");
            }

            var result = await _userService.EditUserPasswordAsync(user, currentPassword, newPassword);

            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                _logger.LogCritical($"Failed to update existing user's password.");
                return StatusCode(500, result.Errors);
            }
        }
        /// <summary>
        /// Update a user.
        /// </summary>
        [HttpPatch("{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateUser(string userId, [FromBody] JsonPatchDocument<UserForUpdateDto> patchDocument)
        {
            var user = await _userService.FetchUserByIdAsync(userId);

            if (user == null)
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This user ID does not exist.");
            }

            var userToPatch = _mapper.Map<UserForUpdateDto>(user);

            patchDocument.ApplyTo(userToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                _logger.LogError($"Validation has failed for PATCH User request.");
                return BadRequest(ModelState);
            }

            if (!string.IsNullOrWhiteSpace(userToPatch.Email) && userToPatch.Email != user.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, userToPatch.Email);
                if (!setEmailResult.Succeeded)
                {
                    _logger.LogError($"Failed to set new email via UserManager.");
                    return StatusCode(500, "Failed to update the user's email due to an internal server error with UserManager.");
                }
            }

            _mapper.Map(userToPatch, user);

            var updatedUser = await _userService.UpdateUserAsync(user);

            if (updatedUser == null)
            {
                _logger.LogCritical($"Failed to update an existing user.");
                return StatusCode(500, "Failed to update the user due to an internal server error.");
            }

            return NoContent();
        }

        /// <summary>
        /// Make a password reset request, sending a verification request to the user's email.
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PasswordReset(PasswordResetDto passwordResetDto)
        {
            var user = await _userManager.FindByEmailAsync(passwordResetDto.Email);

            if (user == null)
            {
                _logger.LogError($"User with email {passwordResetDto.Email} wasn't found.");
                return NotFound("This user email does not exist.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);

            var resetUrl = $"{passwordResetDto.ClientAppUrl}/reset-password?email={passwordResetDto.Email}&token={encodedToken}";

            var html = $"<p>Click <a href=\"{resetUrl}\">here</a> to reset your password.</p>";

            await _emailService.SendEmailAsync(passwordResetDto.Email, "Reset your password", html);

            return Ok();
        }
    }
}
