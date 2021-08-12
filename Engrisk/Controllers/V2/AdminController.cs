using System;
using System.Threading.Tasks;
using Application.DTOs.Pagination;
using Application.Services.Core.Abstraction;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V2
{
    public class AdminController : BaseApiController
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }
        [HttpGet("censor")]
        public async Task<IActionResult> GetWaitingCensor([FromQuery] PaginationDTO pagination, [FromQuery] CensorTypes type)
        {
            return Ok(await _adminService.GetWaitingCensorContentAsync(pagination, type));
        }
        [HttpPut("censor/{id}")]
        public async Task<IActionResult> Censored(Guid id, [FromQuery] CensorTypes type, [FromQuery] Status status, [FromQuery] DifficultLevel difficultLevel)
        {
            if (await _adminService.CensoredContentAsync(id, type, status, difficultLevel))
            {
                return Ok();
            }
            return NoContent();
        }
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard(){
            return Ok(await _adminService.GetDashboardAsync());
        }
        [HttpGet("route/overview")]
        public async Task<IActionResult> GetRouteOverview(){
            return Ok();
        }
    }
}