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
            return Ok();
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
        public async Task<IActionResult> GetAllQuestions([FromQuery] QuestionType type, [FromQuery] GrammarQuestionType grammar){
            return Ok(await _questionService.GetQuestionsAsync(type,grammar));
        }
        [HttpGet("manage")]
        public async Task<IActionResult> GetManageQuestions([FromQuery] PaginationDTO pagination, [FromQuery] QuestionType type, [FromQuery] GrammarQuestionType grammar, [FromQuery] string search = null){
            return Ok(await _questionService.GetQuestionsAsync(pagination,type,grammar,search));
        }
        [HttpGet("statistical")]
        public async Task<IActionResult> GetStatistical(){
            return Ok(await _questionService.GetStatisticalAsync());
        }
        [HttpPost]
        public async Task<IActionResult> CreateQuestion([FromForm] QuestionCreateDTO questionCreateDTO)
        {
            if(await _questionService.CreateQuestionAsync(questionCreateDTO)){
                return Ok();
            };
            return NoContent();
        }
    }
}