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
        public async Task<IActionResult> GetExam([FromQuery] PaginationDTO pagination, [FromQuery] Status status, string search = null)
        {
            return Ok(await _examService.GetExams(pagination, status, search));
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetExam(){
            return Ok(await _examService.GetExams());
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetExam(Guid id){
            if(!await _examService.CheckExist(id)){
                return NotFound();
            }
            return Ok(await _examService.GetExamAsync(id));
        }
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserExam(int id, [FromQuery] PaginationDTO pagination, [FromQuery] string search) 
        {
            return Ok(await _examService.GetUserExamAsync(id,pagination,search));
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
        public async Task<IActionResult> UpdateExam(Guid id, [FromBody] ExamDTO examUpdateDto){
            if(!await _examService.CheckExist(id)){
                return NotFound();
            }
            if(await _examService.UpdateExamAsync(id,examUpdateDto)){
                return Ok();
            }
            return NoContent();
        }
        [Authorize]
        [HttpPut("{examId}/pause")]
        public async Task<IActionResult> PauseExam(Guid examId, [FromQuery] int pauseQuestion)
        {
            if (!await _examService.CheckExist(examId))
            {
                return NotFound();
            }
            var accountId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _examService.PauseExam(examId, accountId, pauseQuestion);
            return Ok();
        }
        [Authorize]
        [HttpPut("{examId}/resume")]
        public async Task<IActionResult> ResumeExam(Guid examId)
        {
            if (!await _examService.CheckExist(examId))
            {
                return NotFound();
            }
            var accountId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var pauseQuestion = await _examService.ResumeExam(examId, accountId);
            return Ok(pauseQuestion);
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
            var exam = await _examService.CreateExamAsync(examCreateDTO);
            return Ok();
        }

        private string GenerateSharedUrl(Exam exam)
        {
            StringBuilder sharedUrl = new StringBuilder();
            sharedUrl.Append("/exam?id=" + exam.Id);
            sharedUrl.Append("&shared=true");
            return sharedUrl.ToString();
        }

    }
}