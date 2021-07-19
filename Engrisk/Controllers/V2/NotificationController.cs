using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.DTOs.Notification;
using Application.DTOs.Pagination;
using Application.Services.Core;
using Application.Services.Core.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V2
{
    public class NotificationController : BaseApiController
    {
        private INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        [Authorize]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers(){
            int accountId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            return Ok(await _notificationService.GetAllUsers(accountId));
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] AdminNotificationCreateDTO notificationCreateDTO)
        {
            var senderId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var notification = await _notificationService.CreateNotificationAsync(senderId, notificationCreateDTO);
            if (notification != null)
            {
                return Ok(notification);
            }
            return NoContent();
        }
        [HttpGet("clients")]
        public async Task<IActionResult> GetClient()
        {
            return Ok();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> EditNotification(Guid id, [FromBody] AdminNotificationCreateDTO notificationCreateDTO)
        {
            return Ok();
        }
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetNotification(int userId, [FromQuery] PaginationDTO pagination)
        {
            // if (userId != Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            // {
            //     return Unauthorized();
            // }
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, pagination);
            return Ok(notifications);
        }
        [HttpPut("{userId}/seen/{notificationId}")]
        public async Task<IActionResult> SeenNotification(int userId, Guid notificationId){
            await _notificationService.SeenNotification(userId,notificationId);
            return Ok();
        }
        // [HttpPost("send")]
        // public async Task<IActionResult> SendNotification(){
        //     await _notificationService.SendNotification(1,2,"test",Domain.Enums.NotificationType.info,"dddd");
        //     return Ok();
        // }
    }
}