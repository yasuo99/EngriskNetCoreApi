using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.DTOs.Pagination;
using Application.DTOs.Section;
using Application.Services.Core.Abstraction;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V2
{
    public class SectionsController : BaseApiController
    {
        private readonly ISectionService _sectionService;
        public SectionsController(ISectionService sectionService)
        {
            _sectionService = sectionService;
        }
        [Authorize]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetSections([FromQuery] PaginationDTO pagination)
        {
            if (User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                var accountId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                return Ok(await _sectionService.GetUserSectionsAndProgressAsync(pagination, accountId));
            }
            return Ok(await _sectionService.GetSectionsAsync(pagination));
        }
        [Authorize]
        [HttpGet("manage")]
        public async Task<IActionResult> GetManageSections([FromQuery] PaginationDTO pagination, [FromQuery] string search)
        {
            return Ok(await _sectionService.GetManageSectionsAsync(pagination, search));
        }
        [Authorize]
        [HttpGet("free")]
        public async Task<IActionResult> GetFreeSections()
        {
            return Ok(await _sectionService.GetFreeSectionsAsync());
        }
        [HttpGet("{id}/preview")]
        public async Task<IActionResult> PreviewSection(Guid id)
        {
            return Ok(await _sectionService.SectionPreviewAsync(id));
        }
        [HttpGet("{id}/learn/{quizId}/questions/{questionId}/check/{answerId}")]
        public async Task<IActionResult> CheckQuizAnswer(Guid id, Guid questionId, Guid quizId, Guid answerId)
        {
            return Ok(await _sectionService.CheckQuestionAnswerAsync(quizId, questionId, answerId));
        }
        [HttpGet("{id}/scripts/edit")]
        public async Task<IActionResult> GetSectionScript(Guid id)
        {
            if (!await _sectionService.CheckExistAsync(id))
            {
                return NotFound();
            }
            return Ok(await _sectionService.GetSectionScriptAsync(id));
        }
        [Authorize]
        [AllowAnonymous]
        [HttpGet("{id}/vocabulary")]
        public async Task<IActionResult> LearnVocabulary(Guid id)
        {
            if (User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                int accountId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var scripts = await _sectionService.SectionLearn(id, accountId);
                if (scripts != null)
                {
                    return Ok(scripts);
                }
                return BadRequest(new
                {
                    Error = "Bạn phải hoàn thành section trước đó"
                });
            }
            else
            {
                if (!await _sectionService.CheckAnonymousLearnAsync(id))
                {
                    return BadRequest(new
                    {
                        Error = "Bạn phải đăng nhập để học section này"
                    });
                }
                var scripts = await _sectionService.AnonymousSectionLearnAsync();
                return Ok(scripts);
            }

        }
        [Authorize]
        [HttpGet("{id}/finish-up")]
        public async Task<IActionResult> GetSectionProgress(Guid id)
        {
            int accountId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var progress = await _sectionService.SectionFinishUpAsync(id, accountId, "review");
            return Ok(progress);
        }
        [HttpPut("{id}/learn/{quizId}/done")]
        public async Task<IActionResult> FinishLearnByQuiz(Guid id, Guid quizId)
        {
            if (!await _sectionService.CheckExistAsync(id))
            {
                return NotFound();
            }

            return Ok();
        }
        [Authorize]
        [HttpPut("{id}/scripts")]
        public async Task<IActionResult> CreateSectionScript(Guid id, [FromBody] List<ScriptCreateDTO> scripts)
        {
            if (!await _sectionService.CheckExistAsync(id))
            {
                return NotFound();
            }
            if (await _sectionService.CreateSectionScriptAsync(id, scripts))
            {
                return Ok();
            }
            return NoContent();
        }
        [Authorize]
        [HttpPut("{id}/publish/change")]
        public async Task<IActionResult> PublishChange(Guid id, [FromQuery] PublishStatus status){
            try
            {
                if(!await _sectionService.CheckExistAsync(id)){
                    return NotFound();
                }
                await _sectionService.PublishAsync(id,status);
                return Ok();
            }
            catch (System.Exception ex)
            {
                 // TODO
                 return BadRequest(ex);
            }
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSection(Guid id)
        {
            if (!await _sectionService.CheckExistAsync(id))
            {
                return NotFound();
            }
            if (await _sectionService.DeleteSectionAsync(id))
            {
                return Ok();
            }
            return NoContent();
        }
    }
}