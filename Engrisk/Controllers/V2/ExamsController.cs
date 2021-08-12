using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Exam;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.Services.Core.Abstraction;
using Application.DTOs.Pagination;
using System;
using System.Security.Claims;
using System.Collections.Generic;
using Application.DTOs.Answer;
using Domain.Enums;
using Application.DTOs.Question;

namespace Engrisk.Controllers.V2
{
    [ApiController]
    [Route("api/v2/[Controller]")]
    public class ExamsController : BaseApiController
    {
        private IExamService _examService;
        private IAccountService _accountService;
        public ExamsController(IExamService examService, IAccountService accountService)
        {
            _examService = examService;
            _accountService = accountService;
        }
        [HttpGet]
        public async Task<IActionResult> GetExam([FromQuery] PaginationDTO pagination, [FromQuery] ExamPurposes purpose = ExamPurposes.None, [FromQuery] DifficultLevel difficult = DifficultLevel.None, [FromQuery] string search = null, [FromQuery] string sort = null)
        {
            return Ok(await _examService.GetExams(pagination, purpose, difficult, search, sort));
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetExam([FromQuery] ExamPurposes purpose = ExamPurposes.None, [FromQuery] string search = null)
        {
            return Ok(await _examService.GetExams(purpose, search));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetExam(Guid id)
        {
            if (!await _examService.CheckExist(id))
            {
                return NotFound();
            }
            return Ok(await _examService.GetExamAsync(id));
        }
        [HttpGet("{id}/analyze")]
        public async Task<IActionResult> GetExamAnalyze(Guid id)
        {
            if (!await _examService.CheckExist(id))
            {
                return NotFound();
            }
            return Ok(await _examService.GetExamAnalyzeAsync(id));
        }
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserExam(int id, [FromQuery] PaginationDTO pagination, [FromQuery] string search)
        {
            return Ok(await _examService.GetUserExamAsync(id, pagination, search));
        }
        [Authorize]
        [HttpGet("{examId}/do")]
        public async Task<IActionResult> DoExam(Guid examId)
        {
            if (!await _examService.CheckExist(examId))
            {
                return NotFound();
            }
            var accountId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (await _examService.CheckConditionAsync(accountId, examId))
            {
                return Ok(await _examService.DoExamAsync(accountId, examId));
            }
            return BadRequest(new
            {
                Error = "Tài khoản không đủ điểm để mở khóa"
            });
        }
        [Authorize]
        [HttpPost("{examId}/done")]
        public async Task<IActionResult> DoneExam(Guid examId, [FromBody] List<AnswerDTO> answers)
        {
            var accountId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            return Ok(await _examService.DoneExam(accountId, examId, answers));
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExam(Guid id, [FromBody] ExamDTO examUpdateDto)
        {
            if (!await _examService.CheckExist(id))
            {
                return NotFound();
            }
            if (await _examService.UpdateExamAsync(id, examUpdateDto))
            {
                return Ok();
            }
            return NoContent();
        }
        [Authorize]
        [HttpPost("{id}/questions")]
        public async Task<IActionResult> CreateExamQuestion(Guid id, [FromForm] QuestionCreateDTO questionCreate){
            if(!await _examService.CheckExist(id)){
                return NotFound();
            }
            if(await _examService.CreateExamQuestionAsync(id,questionCreate)){
                return Ok();
            }
            return NoContent();
        }
        [Authorize]
        [HttpPut("{id}/publish/change")]
        public async Task<IActionResult> PublishChange(Guid id, [FromQuery] PublishStatus status)
        {
            try
            {
                if (!await _examService.CheckExist(id))
                {
                    return NotFound();
                }
                await _examService.PublishAsync(id, status);
                return Ok();
            }
            catch (System.Exception ex)
            {
                // TODO
                return BadRequest(ex);
            }
        }
        [Authorize]
        [HttpPost("user/{id}")]
        public async Task<IActionResult> CreateUserExam(int id, [FromBody] ExamCreateDTO examCreateDTO)
        {
            // var roles = ((ClaimsIdentity)User.Identity).Claims
            //     .Where(c => c.Type == ClaimTypes.Role)
            //     .Select(c => c.Value);
            // if(roles.Any(r => r == SD.STUDENT) || roles.Any(r => r == SD.TEACHER)){

            // }
            var account = await _accountService.GetAccountAsync(id);
            if (account == null)
            {
                return Unauthorized();
            }
            var exam = await _examService.CreateUserExamAsync(id, examCreateDTO);
            return Ok(exam);
        }
        [HttpPost]
        public async Task<IActionResult> CreateAdminExam([FromBody] ExamDTO examCreateDTO)
        {
            try
            {
                var exam = await _examService.CreateExamAsync(examCreateDTO);
                if (exam == null)
                {
                    return NoContent();
                }
                return Ok();
            }
            catch (System.Exception ex)
            {
                // TODO
                return BadRequest();
            }

        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExam(Guid id)
        {
            if (!await _examService.CheckExist(id))
            {
                return NotFound();
            }
            if (await _examService.DeleteExamAsync(id))
            {
                return Ok();
            }
            return NoContent();
        }
        private string GenerateSharedUrl(Exam exam)
        {
            StringBuilder sharedUrl = new StringBuilder();
            sharedUrl.Append("/exam?id=" + exam.Id);
            sharedUrl.Append("&shared=true");
            return sharedUrl.ToString();
        }
        [HttpGet("data")]
        public async Task<IActionResult> GenerateData(){
            return Ok(await _examService.GenerateQuestionAsync());
        }

    }
}