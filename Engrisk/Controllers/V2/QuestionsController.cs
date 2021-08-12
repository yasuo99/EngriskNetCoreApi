using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.DTOs.Pagination;
using Application.DTOs.Question;
using Application.Services.Core;
using Application.Services.Core.Abstraction;
using Domain.Enums;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V2
{
    public class QuestionsController : BaseApiController
    {
        private readonly IQuestionService _questionService;
        public QuestionsController(IQuestionService questionService)
        {
            _questionService = questionService;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            if(!await _questionService.CheckExistAsync(id)){
                return NotFound();
            }
            return Ok(await _questionService.GetQuestionAsync(id));
        }
        [Authorize]
        [AllowAnonymous]
        [HttpGet("{id}/check")]
        public async Task<IActionResult> CheckAnswer(Guid id, [FromQuery] string answer)
        {
            var result = await _questionService.CheckAnswerAsync(id, answer);
            if (User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                if (result)
                {
                    int accountId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    await _questionService.VocabularyPracticeAsync(id, accountId);
                }
            }
            return Ok(new
            {
                Result = result
            });
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllQuestions([FromQuery] QuestionType type, [FromQuery] GrammarQuestionType grammar, [FromQuery] string search)
        {
            return Ok(await _questionService.GetQuestionsAsync(type, grammar, search));
        }
        [HttpGet("manage")]
        public async Task<IActionResult> GetManageQuestions([FromQuery] PaginationDTO pagination, [FromQuery] QuestionType type, [FromQuery] QuestionStatus status, [FromQuery] GrammarQuestionType grammar, [FromQuery] string search = null)
        {
            return Ok(await _questionService.GetQuestionsAsync(pagination, type, status, grammar, search));
        }
        [Authorize]
        [HttpGet("statistical")]
        public async Task<IActionResult> GetStatistical()
        {
            return Ok(await _questionService.GetStatisticalAsync());
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateQuestion([FromForm] QuestionCreateDTO questionCreateDTO)
        {
            if (await _questionService.CreateQuestionAsync(questionCreateDTO) != null)
            {
                return Ok();
            };
            return NoContent();
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(Guid id, [FromForm] QuestionCreateDTO questionUpdate)
        {
            if (await _questionService.UpdateQuestionAsync(id, questionUpdate))
            {
                return Ok();
            };
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(Guid id)
        {
            try
            {
                if (!await _questionService.CheckExistAsync(id))
                {
                    return NotFound();
                }
                if (await _questionService.DeleteQuestionAsync(id))
                {
                    return Ok();
                }
                return NoContent();
            }
            catch (System.Exception ex)
            {
                // TODO
                return BadRequest(ex);
            }
        }
    }
}