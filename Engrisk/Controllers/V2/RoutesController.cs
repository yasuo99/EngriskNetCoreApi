using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.DTOs.Account.Route;
using Application.DTOs.Certificate;
using Application.DTOs.Pagination;
using Application.Services;
using Application.Services.Core.Abstraction;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V2
{
    public class RoutesController : BaseApiController
    {
        private readonly IRouteService _routeService;
        private readonly ISectionService _sectionService;
        private readonly IFileService _fileService;
        public RoutesController(IRouteService routeService, IFileService fileService, ISectionService sectionService)
        {
            _routeService = routeService;
            _fileService = fileService;
            _sectionService = sectionService;
        }
        [HttpGet]
        public async Task<IActionResult> AdminGetAll([FromQuery] PaginationDTO pagination, [FromQuery] PublishStatus status, [FromQuery] string search = null)
        {
            return Ok(await _routeService.AdminGetEngriskAllRouteAsync(pagination, status, search));
        }
        [HttpGet("overview")]
        public async Task<IActionResult> GetRouteOverview([FromQuery] DateRangeDTO dateRange){
            return Ok(await _routeService.GetRouteOverviewAsync(dateRange));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRouteDetail(Guid id)
        {
            return Ok(await _routeService.GetRouteDetailAsync(id));
        }
        [HttpGet("{id}/analyze")]
        public async Task<IActionResult> GetRouteAnalyze(Guid id){
            if(!await _routeService.CheckRouteExistAsync(id)){
                return NotFound();
            }
            return Ok(await _routeService.GetRouteAnalyzeAsync(id));
        }
        [HttpGet("users/{id}/manage")]
        public async Task<IActionResult> GetUserRoute(int id, [FromQuery] PaginationDTO pagination, [FromQuery] bool isPrivate = true, [FromQuery] Status status = Status.Nope)
        {
            return Ok(await _routeService.GetAllUserRoute(pagination, id, isPrivate, status));
        }

        [HttpGet("{id}/users/{accountId}")]
        public async Task<IActionResult> GetUserRouteProgress(Guid id, int accountId)
        {
            return Ok(await _routeService.GetRouteProgressAsync(id, accountId));
        }
        [HttpGet("users/{id}/nearest-finish")]
        public async Task<IActionResult> GetNearest(int id)
        {
            return Ok(await _routeService.GetNearestFinishRouteAsync(id));
        }
        [HttpGet("anonymous")]
        public async Task<IActionResult> GetAnonymous()
        {
            return Ok(await _routeService.GetAnonymousRouteAsync());
        }
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetEngriskRoute(int id)
        {
            System.Console.WriteLine(HttpContext.Request.Headers["devicetype"]);
            var result = await _routeService.GetAllEngriskRouteAndProgressAsync(id);
            return Ok(result);
        }
        [Authorize]
        [HttpGet("{id}/certificate/check")]
        public async Task<IActionResult> CertificateRequestCheck(Guid id)
        {
            if (!await _routeService.CheckRouteExistAsync(id))
            {
                return NotFound();
            }
            int accountId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Ok(await _routeService.RequestCertificateAsync(accountId, id));
        }
        [Authorize]
        [HttpGet("{id}/certificate/claim")]
        public async Task<IActionResult> CertificateClaimRequest(Guid id, [FromQuery] SignatureCertificateDTO signatureCertificateDTO)
        {
            if (!await _routeService.CheckRouteExistAsync(id))
            {
                return NotFound();
            }
            int accountId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Ok(await _routeService.ClaimCertificateAsync(accountId, id, signatureCertificateDTO));
        }
        [HttpPost]
        public async Task<IActionResult> CreateRoute([FromForm] RouteCreateDTO routeCreate)
        {
            var route = await _routeService.CreateRouteAsync(routeCreate);
            if (route != null)
            {
                return Ok(new
                {
                    Status = 200,
                    Route = route
                });
            }
            return NoContent();

        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoute(Guid id, [FromForm] RouteUpdateDTO routeUpdate)
        {
            if (!await _routeService.CheckRouteExistAsync(id))
            {
                return NotFound();
            }
            var result = await _routeService.UpdateRouteAsync(id, routeUpdate);
            if (result != null)
            {
                return Ok();
            }
            return NoContent();
        }
        [HttpPut("{id}/publish/change")]
        public async Task<IActionResult> ChangePublishStatus(Guid id, [FromQuery] PublishStatus status)
        {
            try
            {
                if (!await _routeService.CheckRouteExistAsync(id))
                {
                    return NotFound();
                }
                await _routeService.PublishAsync(id, status);
                return Ok();
            }
            catch (System.Exception ex)
            {
                // TODO
                return BadRequest(ex.Message);
            }

        }
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateRouteStatus(Guid id)
        {
            if (!await _routeService.CheckRouteExistAsync(id))
            {
                return NotFound();
            }
            var result = await _routeService.ChangeRouteStatusAsync(id);
            if (result)
            {
                return Ok();
            }
            return NoContent();
        }
        [HttpPut("{id}/users/{accountId}/status")]
        public async Task<IActionResult> ChangePrivateStatus(Guid id, int accountId)
        {
            if (!await _routeService.CheckRouteExistAsync(id))
            {
                return NotFound();
            }
            if (!await _routeService.CheckRouteOwnerAsync(id, accountId))
            {
                return Unauthorized();
            }
            var result = await _routeService.ChangePrivateStatusAsync(id);
            if (result)
            {
                return Ok();
            }
            return NoContent();
        }
        [HttpPut("{id}/sections")]
        public async Task<IActionResult> EditRouteSections(Guid id, [FromBody] List<Guid> sections)
        {
            if (await _routeService.ReArrangeSectionsRouteAsync(id, sections))
            {
                return Ok();
            }
            return NoContent();
        }
        [Authorize]
        [AllowAnonymous]
        [HttpGet("{routeId}/sections/{sectionId}/scripts/{scriptId}")]
        public async Task<IActionResult> LearningScript(Guid routeId, Guid sectionId, Guid scriptId)
        {
            if (!await _routeService.CheckRouteExistAsync(routeId))
            {
                return NotFound(new
                {
                    Status = 404,
                    Error = "L??? tr??nh h???c kh??ng t???n t???i"
                });
            }
            if (!await _routeService.CheckRouteStatusAsync(routeId))
            {
                return BadRequest(new
                {
                    Status = 400,
                    Error = "L??? tr??nh h???c hi???n kh??ng s???n s??ng"
                });
            }
            if (!await _routeService.CheckSectionExistAsync(routeId, sectionId))
            {
                return NotFound(new
                {
                    Status = 400,
                    Error = "B??i h???c kh??ng t???n t???i ho???c thu???c l??? tr??nh"
                });
            }
            if (!await _routeService.CheckSectionStatusAsync(routeId, sectionId))
            {
                return BadRequest(new
                {
                    Status = 400,
                    Error = "B??i h???c hi???n kh??ng s???n s??ng"
                });
            }
            if (!await _sectionService.CheckSectionScriptExistAsync(sectionId, scriptId))
            {
                return NotFound(new
                {
                    Status = 404,
                    Error = "K???ch b???n kh??ng t???n t???i ho???c thu???c b??i h???c"
                });
            }
            //Ng?????i d??ng ???? ????ng nh???p
            if (User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                int accountId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                //Ki???m tra xem l??? tr??nh h???c n??y c?? y??u c???u t??nh tu???n t??? kh??ng
                //N???u kh??ng th?? tr??? v??? ngay k???ch b???n ???????c ch???n
                if (!await _routeService.CheckRouteSequentiallyAsync(routeId))
                {
                    return Ok(await _sectionService.ScriptLearnAsync(scriptId, accountId));
                }
                //N???u c?? 
                else
                {
                    //Check is that previous section has been done yet ?
                    if (!await _sectionService.CheckPreviousSectionDoneAsync(sectionId, accountId))
                    {
                        //If not return bad request status
                        return BadRequest(new
                        {
                            Status = 400,
                            Error = "B???n ph???i ho??n th??nh b??i h???c tr?????c ????"
                        });
                    }
                    //If yes, return selected script data
                    return Ok(await _sectionService.ScriptLearnAsync(scriptId, accountId));
                }
            }
            //Ki???m tra xem c?? ph???i b??i h???c demo c???a l??? tr??nh kh??ng 
            if (!await _routeService.CheckAnonymousSectionAsync(routeId, sectionId))
            {
                return BadRequest(new
                {
                    Status = 400,
                    Error = "B???n ph???i ????ng nh???p ????? h???c b??i n??y"
                });
            }
            return Ok(await _sectionService.AnonymousScriptLearnAsync(scriptId));
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoute(Guid id)
        {
            if (!await _routeService.CheckRouteExistAsync(id))
            {
                return NotFound();
            }
            await _routeService.DeleteRouteAsync(id);
            return Ok();
        }
    }
}