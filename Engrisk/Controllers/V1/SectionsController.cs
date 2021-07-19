using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Helper;
using AutoMapper;
using Engrisk.Data;
using Application.DTOs;
using Application.DTOs.Quiz;
using Application.DTOs.Section;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR;
using Application.Hubs;
using Application.Services;
using Application.Utilities;

namespace Engrisk.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[Controller]")]
    public class SectionsController : ControllerBase
    {
        private readonly ICRUDRepo _repo;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly IFileService _fileService;
        public SectionsController(ICRUDRepo repo, IMapper mapper, IHubContext<NotificationHub> hub, IFileService fileService)
        {
            _mapper = mapper;
            _repo = repo;
            _hub = hub;
            _fileService = fileService;
        }
        [HttpGet]
        public async Task<IActionResult> GetSections([FromQuery] SubjectParams subjectParams)
        {
            var sections = await _repo.GetAll<Section>(subjectParams, includeProperties: "Quizzes");
            var returnSections = _mapper.Map<IEnumerable<SectionDTO>>(sections);
            if (User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                foreach (var section in returnSections)
                {
                    var done = await _repo.GetAll<AccountSection>(a => a.AccountId == userId && a.SectionId == section.Id);
                }
            }
            return Ok(returnSections);
        }
        [HttpGet("manage")]
        public async Task<IActionResult> ManageSections([FromQuery] SubjectParams subjectParams)
        {
            var sections = await _repo.GetAll<Section>(subjectParams, includeProperties: "Quizzes");
            var returnSections = _mapper.Map<IEnumerable<SectionDTO>>(sections);
            return Ok(returnSections);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            var sectionFromDb = await _repo.GetOneWithConditionTracking<Section>(section => section.Id == id, "Quizzes");
            if (sectionFromDb == null)
            {
                return NotFound();
            }
            return Ok(sectionFromDb);
        }
        [HttpPost]
        public async Task<IActionResult> CreateSection([FromForm] SectionCreateDTO sectionDTO)
        {
            var properties = new Dictionary<dynamic, dynamic>();
            properties.Add("SectionName", sectionDTO.SectionName);
            if (_repo.Exists<Section>(properties))
            {
                return Conflict(new
                {
                    Error = "Section đã tồn tại"
                });
            }
            var section = _mapper.Map<Section>(sectionDTO);
            if (sectionDTO.File != null)
            {
                section.PhotoUrl = _fileService.UploadFile(sectionDTO.File, SD.ImagePath);
            }
            _repo.Create(section);
            if (await _repo.SaveAll())
            {
                var returnSection = _mapper.Map<SectionDTO>(section);
                var responseSection = Extension.CamelCaseSerialize(returnSection);
                return Ok();
            }
            return StatusCode(500);
        }
        // [HttpGet("{sectionId}/quizzes")]
        // public async Task<IActionResult> GetAllQuizzes(Guid sectionId)
        // {
        //     var sectionFromDb = await _repo.GetOneWithManyToMany<Section>(section => section.Id == sectionId);
        //     var section = await sectionFromDb.Include(q => q.Quizzes).ThenInclude(q => q.Questions).FirstOrDefaultAsync();
        //     if (section == null)
        //     {
        //         return NotFound();
        //     }
        //     var returnQuizzes = _mapper.Map<IEnumerable<QuizDetailDTO>>(section.Quizzes);
        //     return Ok(returnQuizzes);
        // }
        // [HttpGet("{sectionId}/do")]
        // public async Task<IActionResult> DoQuiz(Guid sectionId, [FromQuery] int currentQuiz = 0)
        // {
        //     if (await _repo.GetOneWithKey<Section>(sectionId) == null)
        //     {
        //         return NotFound(new
        //         {
        //             Error = "Không tìm thấy bài học"
        //         });
        //     }
        //     var sectionFromDb = await _repo.GetOneWithManyToMany<Section>(sec => sec.Id == sectionId);
        //     var section = await sectionFromDb.Include(s => s.Quizzes).Include(s => s.Accounts).FirstOrDefaultAsync();
        //     if (section.Quizzes.Count == 0)
        //     {
        //         return NoContent();
        //     }
        //     //Kiem tra co phai la user da dang nhap
        //     if (User.FindFirst(ClaimTypes.NameIdentifier) != null)
        //     {

        //         int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        //         var quizDone = section.Accounts.Where(s => s.AccountId == userId);
        //         var newQuiz = section.Quizzes.Skip(quizDone == null ? 0 : quizDone.Count()).Take(1).FirstOrDefault();
        //         if (newQuiz == null)
        //         {
        //             var randomQuiz = section.Quizzes.GetOneRandomFromList();
        //             var oldQuizHistory = new History()
        //             {
        //                 AccountId = userId,
        //                 QuizId = randomQuiz.Id,
        //                 StartTimestamp = DateTime.Now,
        //                 IsDone = false
        //             };
        //             _repo.Create(oldQuizHistory);
        //             await _repo.SaveAll();
        //             var quizQuery = await _repo.GetOneWithManyToMany<Quiz>(quiz => quiz.Id == randomQuiz.Id);
        //             var quizQuestions = await quizQuery.Include(e => e.Questions).ThenInclude(q => q.Question).FirstOrDefaultAsync();
        //             var questions = _mapper.Map<QuizDTO>(quizQuestions);

        //             // var examFromDb = await _repo.GetOneWithManyToMany<Section>(quiz => quiz.Id == quizId);
        //             // var exam = await examFromDb.Include(e => e.Questions).ThenInclude(q => q.Question).FirstOrDefaultAsync();
        //             // var returnQuestions = _mapper.Map<QuizDTO>(exam);
        //             return Ok(questions);
        //         }
        //         var userQuizQuery = await _repo.GetOneWithManyToMany<Quiz>(quiz => quiz.Id == newQuiz.Id);
        //         var userQuiz = await userQuizQuery.Include(e => e.Questions).ThenInclude(q => q.Question).ThenInclude(inc => inc.Answers).FirstOrDefaultAsync();
        //         var returnQuiz = _mapper.Map<QuizDTO>(userQuiz);
        //         if (returnQuiz.Questions.Count() > 0)
        //         {
        //             var accountSection = await _repo.GetOneWithConditionTracking<AccountSection>(s => s.AccountId == userId && s.SectionId == sectionId);
        //             if (accountSection == null)
        //             {
        //                 var accountSec = new AccountSection()
        //                 {
        //                     AccountId = userId,
        //                     SectionId = sectionId,
        //                 };
        //                 _repo.Create(accountSec);
        //             }
        //             var history = new History()
        //             {
        //                 AccountId = userId,
        //                 QuizId = userQuiz.Id,
        //                 StartTimestamp = DateTime.Now,
        //                 IsDone = false
        //             };               
        //             _repo.Create(history);
        //             await _repo.SaveAll();
        //         }
        //         return Ok(returnQuiz);
        //     }
        //     var anonymousQuiz = section.Quizzes.Skip(currentQuiz).Take(1).FirstOrDefault();
        //     if (anonymousQuiz == null)
        //     {
        //         return NotFound();
        //     }
        //     var examFromDb = await _repo.GetOneWithManyToMany<Quiz>(quiz => quiz.Id == anonymousQuiz.Id);
        //     var exam = await examFromDb.Include(e => e.Questions).ThenInclude(q => q.Question).FirstOrDefaultAsync();
        //     var returnQuestions = _mapper.Map<QuizDTO>(exam);

        //     // var examFromDb = await _repo.GetOneWithManyToMany<Section>(quiz => quiz.Id == quizId);
        //     // var exam = await examFromDb.Include(e => e.Questions).ThenInclude(q => q.Question).FirstOrDefaultAsync();
        //     // var returnQuestions = _mapper.Map<QuizDTO>(exam);
        //     return Ok(returnQuestions);
        // }
        // [HttpPost("{sectionId}/quizzes/{quizId}/done")]
        // public async Task<IActionResult> DoneQuiz(Guid sectionId, Guid quizId)
        // {
        //     var sectionFromDb = await _repo.GetOneWithCondition<Section>(s => s.Id == sectionId, includeProperties: "Quizzes");
        //     if (!sectionFromDb.Quizzes.Any(q => q.Id == quizId))
        //     {
        //         return BadRequest(new
        //         {
        //             Error = "Quiz này không thuộc section này"
        //         });
        //     }
        //     var examFromDb = await _repo.GetOneWithConditionTracking<Quiz>(quiz => quiz.Id == quizId, "Questions");
        //     if (examFromDb == null)
        //     {
        //         return NotFound();
        //     }
        //     if (User.FindFirst(ClaimTypes.NameIdentifier) != null)
        //     {
        //         var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        //         var historyFromDb = await _repo.GetOneWithConditionTracking<History>(history => history.QuizId == quizId && history.AccountId == accountId && history.IsDone == false);
        //         historyFromDb.EndTimestamp = DateTime.Now;
        //         historyFromDb.TimeSpent = (int)DateTime.Now.Subtract(historyFromDb.StartTimestamp).TotalSeconds;
        //         historyFromDb.IsDone = true;
        //         var accountSection = await _repo.GetOneWithConditionTracking<AccountSection>(s => s.AccountId == accountId && s.SectionId == sectionId);
        //         if (accountSection == null)
        //         {
        //             var newAccountSection = new AccountSection
        //             {
        //                 AccountId = accountId,
        //                 SectionId = sectionId,
        //             };
        //             _repo.Create(accountSection);
        //         }
        //         if (await _repo.SaveAll())
        //         {
        //             return Ok(historyFromDb);
        //         }
        //         else
        //         {
        //             return NoContent();
        //         }
        //     }

        //     return NoContent();
        // }
        [HttpPut("{id}")]
        public async Task<IActionResult> EditSection(Guid id, [FromForm] SectionUpdateDTO section)
        {
            var sectionFromDb = await _repo.GetOneWithConditionTracking<Section>(section => section.Id == id);
            if (sectionFromDb == null)
            {
                return NotFound();
            }
            _mapper.Map(section, sectionFromDb);
            if (section.File != null)
            {
                if (!string.IsNullOrEmpty(sectionFromDb.PhotoUrl))
                {
                    _fileService.DeleteFile(sectionFromDb.PhotoUrl);
                }
                sectionFromDb.PhotoUrl = _fileService.UploadFile(section.File, SD.ImagePath);
            }
            if (await _repo.SaveAll())
            {
                return Ok();
            }
            return StatusCode(500);
        }
        [HttpPut("{id}/require-login")]
        public async Task<IActionResult> SetRequireLogin(Guid id)
        {
            try
            {
                var sectionFromDb = await _repo.GetOneWithConditionTracking<Section>(section => section.Id == id);
                if (sectionFromDb == null)
                {
                    return NotFound();
                }
                sectionFromDb.RequireLogin = sectionFromDb.RequireLogin ? false : true;
                if (await _repo.SaveAll())
                {
                    return Ok();
                }
                return NoContent();
            }
            catch (System.Exception e)
            {

                throw e;
            }

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSection(Guid id)
        {
            try
            {
                var sectionFromDb = await _repo.GetOneWithCondition<Section>(section => section.Id == id);
                if (sectionFromDb == null)
                {
                    return NotFound();
                }
                _repo.Delete(sectionFromDb);
                if (await _repo.SaveAll())
                {
                    return NoContent();
                }
                return StatusCode(500);
            }
            catch (System.Exception e)
            {

                throw e;
            }

        }
    }
}