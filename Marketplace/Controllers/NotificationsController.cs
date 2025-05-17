using AutoMapper;
using Marketplace.BusinessLayer;
using Marketplace.DataAccess.Entities;
using Marketplace.Models;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Controllers
{
    [Route("/users/{userId}/notifications")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationsService _notificationsService;
        private readonly IUserProductService _userProductService;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(INotificationsService notificationsService, IUserProductService userProductService, IMapper mapper, ILogger<NotificationsController> logger)
        {
            _notificationsService = notificationsService ?? throw new ArgumentNullException(nameof(notificationsService));
            _userProductService = userProductService ?? throw new ArgumentNullException(nameof(userProductService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        /// <summary>
        /// Get a user's notifications list in it's entirety. 
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<NotificationForResponseDto>>> GetNotifications(string userId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var userNotifications = await _notificationsService.FetchNotifications(userId);

            return Ok(_mapper.Map<IEnumerable<NotificationForResponseDto>>(userNotifications));
        }
        /// <summary>
        /// Get a singular user notification.
        /// </summary>
        [HttpGet("{notificationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NotificationForResponseDto>> GetSingleNotification(string userId, Guid notificationId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var singleUserNotification = await _notificationsService.FetchSingleNotification(userId, notificationId);

            if (singleUserNotification == null)
            {
                _logger.LogError($"Notification wasn't found for this user.");
                return NotFound("This Notification ID does not exist for this user.");
            }

            return Ok(_mapper.Map<NotificationForResponseDto>(singleUserNotification));
        }
        /// <summary>
        /// This method will attempt to add a notification for the user (201).
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NotificationForResponseDto>> AddNotification(string userId, NotificationForCreationDto notificationDto)
        {

            if (!ModelState.IsValid)
            {
                _logger.LogError($"Validation has failed for POST Product request.");
                return BadRequest(ModelState);
            }

            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var notification = new Notification(userId, notificationDto.Message, notificationDto.Url);

            var createdNotification = await _notificationsService.AddNotification(notification);

            if (createdNotification == null)
            {
                _logger.LogCritical($"Failed to create the notification.");
                return StatusCode(500, "Failed to create notification due to an internal server error.");
            }

            var notificationResponseDto = _mapper.Map<NotificationForResponseDto>(createdNotification);

            return CreatedAtAction("GetSingleNotification", new { userId = createdNotification.UserId, notificationId = createdNotification.Id }, notificationResponseDto);
        }
        /// <summary>
        /// Delete an existing notification.
        /// </summary>
        public async Task<ActionResult> DeleteNotification(string userId, Guid notificationId)
        {
            if (!await _userProductService.CheckUserExists(userId))
            {
                _logger.LogError($"User with ID {userId} wasn't found.");
                return NotFound("This User ID does not exist.");
            }

            var notification = await _notificationsService.FetchSingleNotification(userId, notificationId);

            if (notification == null)
            {
                _logger.LogError($"Notification with ID {notificationId} wasn't found.");
                return NotFound("This Notification ID does not exist.");
            }

            var result = await _notificationsService.RemoveNotification(notification);

            if (result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                _logger.LogCritical($"Failed to delete the notification.");
                return StatusCode(500, result.Error);
            }
        }
    }
}
