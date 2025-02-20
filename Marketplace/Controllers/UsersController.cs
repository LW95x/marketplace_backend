using AutoMapper;
using Marketplace.BusinessLayer;
using Marketplace.DataAccess.Entities;
using Marketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Controllers
{
    [Route("/users")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, IMapper mapper, ILogger<UsersController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
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
        /// Get a specific user. TEST HERE!!!
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

            _mapper.Map(userToPatch, user);

            var updatedUser = await _userService.UpdateUserAsync(user);

            if (updatedUser == null)
            {
                _logger.LogCritical($"Failed to update an existing user.");
                return StatusCode(500, "Failed to update the user due to an internal server error.");
            }

            return NoContent();
        }
    }
}
