using AutoMapper;
using Marketplace.BusinessLayer;
using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;
using Marketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Controllers
{
    [Route("/users/{userId}/messages")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IMessagesService _messagesService;
        private readonly IUserProductService _userProductService;
        private readonly IMapper _mapper;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(IMessagesService messagesService, IUserProductService userProductService, IMapper mapper, ILogger<MessagesController> logger)
        {
            _messagesService = messagesService ?? throw new ArgumentNullException(nameof(messagesService));
            _userProductService = userProductService ?? throw new ArgumentNullException(nameof(userProductService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        /// <summary>
        /// Get messages shared between two specific users (i.e. a specific conversation). 
        /// </summary>
        [HttpGet("{receiverId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<MessageForResponseDto>>> GetMessagesBetweenUsers(string userId, string receiverId)
        {
            if (!await _userProductService.CheckUserExists(userId) || !await _userProductService.CheckUserExists(receiverId))
            {
                _logger.LogError($"User with ID wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var messages = await _messagesService.FetchMessagesBetweenUsers(userId, receiverId);


            if (messages == null)
            {
                _logger.LogError($"Conversation wasn't found for these users.");
                return NotFound("Conversation wasn't found for these users.");
            }

            return Ok(_mapper.Map<IEnumerable<MessageForResponseDto>>(messages));
        }
        /// <summary>
        /// Get all user messages sorted by conversation.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<MessageForResponseDto>>> GetAllUserMessages(string userId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var messages = await _messagesService.FetchAllUserMessages(userId);

            if (messages == null)
            {
                _logger.LogError($"Messages weren't found for this user.");
                return NotFound("Messages do not exist for this user.");
            }

            return Ok(_mapper.Map<IEnumerable<MessageForResponseDto>>(messages));
        }
        /// <summary>
        /// Delete a message sent by the user.
        /// </summary>
        [HttpDelete("{messageId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteMessage(string userId, string messageId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var result = await _messagesService.RemoveMessage(userId,  messageId);

            if (result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                _logger.LogCritical($"Failed to delete the message.");
                return StatusCode(500, result.Error);
            }
        }
    }
}
