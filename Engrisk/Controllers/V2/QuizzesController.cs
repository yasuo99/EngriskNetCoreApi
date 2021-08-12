using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.DTOs.Pagination;
using Application.DTOs.Question;
using Application.DTOs.Quiz;
using Application.Services.Core.Abstraction;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V2
{
    public class QuizzesController : BaseApiController
    {
        private readonly IQuizService _service;
        private IAccountService _accountService;
        public QuizzesController(IQuizService service, IAccountService accountService)
        {
            _service = service;
            _accountService = accountService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllQuizzes([FromQuery] PaginationDTO pagination, [FromQuery] Status status, [FromQuery] DifficultLevel difficult = DifficultLevel.None, [FromQuery] PublishStatus publishStatus = PublishStatus.None, string search = null, [FromQuery] string sort = null)
        {
            return Ok(await _service.GetAllQuizAsync(pagination, publishStatus, status, difficult, search, sort));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuiz(Guid id)
        {
            if (!await _service.CheckExistAsync(id))
            {
                return NotFound();
            }
            return Ok(await _service.GetQuizAsync(id));
        }
        [Authorize]
        [AllowAnonymous]
        [HttpGet("{id}/do")]
        public async Task<IActionResult> DoQuiz(Guid id)
        {
            if (!await _service.CheckExistAsync(id))
            {
                return NotFound();
            }
            if (User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                int accountId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Ok(await _service.DoQuizAsync(id, accountId));
            }
            return Ok(await _service.AnonymousDoQuizAsync(id));
        }
        [Authorize]
        [AllowAnonymous]
        [HttpGet("{id}/done")]
        public async Task<IActionResult> DoneQuiz(Guid id)
        {
            if (!await _service.CheckExistAsync(id))
            {
                return NotFound();
            }
            if (User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                int accountId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if(await _service.DoneQuizAsync(id, accountId)){
                    return Ok();
                }
                return NoContent();
            }
            return Ok();
        }
        [HttpGet("{id}/questions/{questionId}")]
        public async Task<IActionResult> AddQuestionToQuiz(Guid id, Guid questionId)
        {
            await _service.AddQuestionToQuizAsync(id, questionId);
            return Ok();
        }
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserQuizzes(int id, [FromQuery] PaginationDTO pagination, [FromQuery] string search)
        {
            return Ok(await _service.GetUserQuizzesAsync(id, pagination, search));
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateQuiz([FromBody] QuizCreateDTO quizCreateDTO)
        {
            return Ok(await _service.AdminCreateQuizAsync(quizCreateDTO));
        }
        [Authorize]
        [HttpPost("{id}/questions")]
        public async Task<IActionResult> CreateQuizQuestion(Guid id, [FromForm] QuestionCreateDTO questionCreate)
        {
            if (!await _service.CheckExistAsync(id))
            {
                return NotFound();
            }
            if (await _service.CreateQuizQuestionAsync(id, questionCreate))
            {
                return Ok();
            }
            return NoContent();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuiz(Guid id, [FromBody] QuizDTO quizUpdateDto)
        {
            if (!await _service.CheckExistAsync(id))
            {
                return NotFound();
            }
            if (await _service.UpdateQuizAsync(id, quizUpdateDto))
            {
                return Ok();
            };
            return NoContent();
        }
        [Authorize]
        [HttpPut("{id}/publish/change")]
        public async Task<IActionResult> PublishChange(Guid id, [FromQuery] PublishStatus status)
        {
            try
            {
                if (!await _service.CheckExistAsync(id))
                {
                    return NotFound();
                }
                await _service.PublishAsync(id, status);
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
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (!await _service.CheckExistAsync(id))
                {
                    return NotFound();
                }
                if (await _service.DeleteQuizAsync(id))
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