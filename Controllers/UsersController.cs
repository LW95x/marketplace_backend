using AutoMapper;
using Azure;
using Marketplace.BusinessLayer;
using Marketplace.DataAccess.Entities;
using Marketplace.DataAccess.Services;
using Marketplace.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Controllers
{
    [Route("/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UsersController(IUserService userService, IMapper mapper)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserForResponseDto>>> GetUsers()
        {
            var users = await _userService.FetchUsersAsync();

            return Ok(_mapper.Map<IEnumerable<UserForResponseDto>>(users));
        }

        [HttpGet("{userId}", Name = "GetUserById")]
        public async Task<ActionResult> GetUserById(string userId)
        {
            if (string.IsNullOrEmpty(userId)) 
            {
                return BadRequest("User ID cannot be null or empty.");
            }

            var user = await _userService.FetchUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound("This user ID does not exist.");
            }

            return Ok(_mapper.Map<UserForResponseDto>(user));
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserForCreationDto model)
        {
            var user = new User { UserName = model.UserName, Email = model.Email };
            var result = await _userService.AddUser(user, model.Password);

            if (result.Succeeded)
            {
                var userResponseDto = _mapper.Map<UserForResponseDto>(user);
                return CreatedAtRoute("GetUserById", new { userId = user.Id }, userResponseDto);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID cannot be null or empty.");
            }

            var user = await _userService.FetchUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound("This user ID does not exist.");
            }

           var result = await _userService.RemoveUser(user);

            if (result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpPost("{userId}/change-password")]
        public async Task<ActionResult> ChangeUserPassword(string userId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID cannot be null or empty.");
            }

            var user = await _userService.FetchUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound("This user ID does not exist.");
            }

            var result = await _userService.EditUserPasswordAsync(user, currentPassword, newPassword);

            if (result.Succeeded)
            {
                return Ok();
            }
            else
            { 
                return BadRequest(result.Errors);
            }
        }

        [HttpPatch("{userId}")]
        public async Task<ActionResult> UpdateUser(string userId, [FromBody] JsonPatchDocument<UserForUpdateDto> patchDocument)
        {
            var user = await _userService.FetchUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound("This user ID does not exist.");
            }

            var userToPatch = _mapper.Map<UserForUpdateDto>(user);

            patchDocument.ApplyTo(userToPatch, ModelState);

            TryValidateModel(userToPatch);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(userToPatch, user);

            var updatedUser = await _userService.UpdateUserAsync(user);

            if (updatedUser == null)
            {
                return BadRequest("Failed to update user.");
            }

            return NoContent();
        }
    }
}
