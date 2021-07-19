using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Helper;
using AutoMapper;
using Engrisk.Data;
using Application.DTOs;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Application.DTOs.Notification;
using Application.DTOs.Pagination;

namespace Engrisk.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[Controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly ICRUDRepo _repo;
        private readonly IMapper _mapper;
        public NotificationsController(ICRUDRepo repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var notifications = await _repo.GetAll<Notification>(null, "");
            var returnNotifications = notifications.OrderByDescending(noti => noti.CreatedDate);
            return Ok(notifications);
        }
        [HttpGet("client/publishing")]
        public async Task<IActionResult> GetPublishing([FromQuery] SubjectParams subjectParams)
        {
            var notifications = await _repo.GetAll<Notification>(subjectParams);
            var returnNotifications = notifications.OrderByDescending(noti => noti.CreatedDate);
            Response.AddPaginationHeader(notifications.CurrentPage, notifications.PageSize, notifications.TotalItems, notifications.TotalPages);
            return Ok(returnNotifications);
        }
        [HttpGet("admin/publishing")]
        public async Task<IActionResult> GetAdminPublishing([FromQuery] SubjectParams subjectParams)
        {
            var notifications = await _repo.GetAll<Notification>(subjectParams);
            var returnNotifications = notifications.OrderByDescending(noti => noti.CreatedDate);
            Response.AddPaginationHeader(notifications.CurrentPage, notifications.PageSize, notifications.TotalItems, notifications.TotalPages);
            return Ok(returnNotifications);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            var notify = await _repo.GetOneWithCondition<Notification>(n => n.Id == id);
            if (notify == null)
            {
                return NotFound();
            }
            return Ok(notify);
        }
        [HttpPost]
        public async Task<IActionResult> CreateNotify([FromBody] NotificationCreateDTO notification)
        {
            var notify = _mapper.Map<Notification>(notification);
            _repo.Create(notify);
            if (await _repo.SaveAll())
            {
                return Ok();
            }
            return BadRequest("Error on creating");
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNotify(Guid id, [FromBody] NotificationCreateDTO notification)
        {
            var notificationFromDb = await _repo.GetOneWithConditionTracking<Notification>(notify => notify.Id == id);
            if (notificationFromDb == null)
            {
                return NotFound();
            }
            _mapper.Map(notification, notificationFromDb);
            if (await _repo.SaveAll())
            {
                return Ok();
            }
            else
            {
                return NoContent();
            }
        }
        [HttpPut("publish/{id}")]
        public async Task<IActionResult> Publish(Guid id)
        {
            var notificationFromDb = await _repo.GetOneWithConditionTracking<Notification>(noti => noti.Id == id);
            notificationFromDb.CreatedDate = DateTime.Now;
            if (await _repo.SaveAll())
            {
                return Ok();
            }
            return BadRequest("Error on publishing");
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotify(Guid id)
        {
            var notificationFromDb = await _repo.GetOneWithCondition<Notification>(noti => noti.Id == id);
            if (notificationFromDb == null)
            {
                return NotFound();
            }
            _repo.Delete(notificationFromDb);
            if (await _repo.SaveAll())
            {
                return Ok();
            }
            return BadRequest();
        }
    }
}