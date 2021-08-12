using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Engrisk.Data;
using Application.DTOs;
using Application.DTOs.Question;
using Engrisk.Helper;
using Domain.Models;
using Engrisk.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Application.DTOs.Answer;
using Application.Helper;
using Domain.Enums;
using Domain.Models.Version2;

namespace Engrisk.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[Controller]")]
    public class QuestionsController : ControllerBase
    {
        private readonly ICRUDRepo _repo;
        private readonly IMapper _mapper;
        private readonly CloudinaryHelper _helper;
        private readonly IConfiguration _config;
        public QuestionsController(ICRUDRepo repo, IMapper mapper, IOptions<Application.Helper.CloudinarySettings> cloudSetting, IConfiguration config)
        {
            var account = new CloudinaryDotNet.Account()
            {
                ApiSecret = cloudSetting.Value.ApiSecret,
                ApiKey = cloudSetting.Value.ApiKey,
                Cloud = cloudSetting.Value.CloudName
            };
            _helper = new CloudinaryHelper(account);
            _mapper = mapper;
            _repo = repo;
            _config = config;
        }
        // [HttpGet]
        // public async Task<IActionResult> GetAll([FromQuery] Application.Helper.SubjectParams subjectParams, [FromQuery] string type = null, [FromQuery] string category = "exam")
        // {
        //     var questions = await _repo.GetAll<Question>(null, "");
        //     if (type != null)
        //     {
        //         switch (type)
        //         {
        //             case "reading":
        //                 switch (category)
        //                 {
        //                     case "exam":
        //                         var readingExamQuestions = questions.Where(q => q.IsFillOutQuestion && q.IsQuizQuestion == false).ToList();
        //                         return Ok(readingExamQuestions);
        //                     case "quiz":
        //                         var readingQuizQuestions = questions.Where(q => q.IsListeningQuestion == false && q.IsQuizQuestion).ToList();
        //                         return Ok(readingQuizQuestions);
        //                     default:
        //                         break;
        //                 }
        //                 break;
        //             case "listening":
        //                 switch (category)
        //                 {
        //                     case "exam":
        //                         var listeningExamQuestions = questions.Where(q => q.IsListeningQuestion && q.IsQuizQuestion == false).ToList();
        //                         return Ok(listeningExamQuestions);
        //                     case "quiz":
        //                         var listeningQuizQuestions = questions.Where(q => q.IsListeningQuestion && q.IsQuizQuestion).ToList();
        //                         return Ok(listeningQuizQuestions);
        //                     default:
        //                         break;
        //                 }
        //                 break;
        //             default:
        //                 break;
        //         }
        //     }
        //     return Ok(questions);
        // }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuestion(Guid id)
        {
            var questionFromDb = await _repo.GetOneWithConditionTracking<Question>(question => question.Id == id, "");
            if (questionFromDb == null)
            {
                return NotFound();
            }
            var returnQuestion = _mapper.Map<QuestionDetailDTO>(questionFromDb);
            return Ok(returnQuestion);
        }
        [HttpPost]
        public async Task<IActionResult> CreateQuestion([FromForm] QuestionCreateDTO questionDTO)
        {
            var properties = new Dictionary<dynamic, dynamic>();
            properties.Add("Content", questionDTO.Content);
            if (_repo.Exists<Question>(properties))
            {
                return Conflict();
            }
            var question = _mapper.Map<Question>(questionDTO);
            _repo.Create(question);
            if (await _repo.SaveAll())
            {
                return Ok();
            }
            return BadRequest("Error on create question");
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(Guid id, [FromForm] QuestionCreateDTO questionCreateDTO)
        {
            try
            {
                var questionFromDb = await _repo.GetOneWithConditionTracking<Question>(q => q.Id == id);
                if (questionFromDb == null)
                {
                    return NotFound(new
                    {
                        Error = "Không tìm thấy câu hỏi"
                    });
                }
                _mapper.Map(questionCreateDTO, questionFromDb);
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
        [HttpPost("{questionId}/check")]
        public async Task<IActionResult> CheckAnswer(Guid questionId, [FromBody] AnswerDTO answer)
        {
            if (questionId != answer.Id)
            {
                return NotFound();
            }
            var questionFromDb = await _repo.GetOneWithCondition<Question>(q => q.Id == questionId, includeProperties: "Answers");
            if (questionFromDb == null)
            {
                return NotFound();
            }
            if (User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var wordFromDb = await _repo.GetOneWithCondition<Word>(w => w.Eng.Equals(questionFromDb.Content) || w.Vie.Equals(questionFromDb.Content));
                if (wordFromDb != null)
                {
                    var learntFromDb = await _repo.GetOneWithConditionTracking<WordLearnt>(w => w.WordId == wordFromDb.Id && w.AccountId == userId);
                    if (learntFromDb == null)
                    {
                        var wordLearnt = new WordLearnt()
                        {
                            AccountId = userId,
                            WordId = wordFromDb.Id,
                        };
                        _repo.Create(wordLearnt);
                    }

                }
                var answerDb = await _repo.GetAll<Answer>(ans => ans.QuestionId == questionId);
                if(answerDb.Any(ans => ans.Content.Equals(answer.Answer.Trim()) && ans.IsQuestionAnswer)){
                    answer.IsQuestionAnswer = true;
                }else{
                    answer.IsQuestionAnswer = false;    
                }
                await _repo.SaveAll();
            }
            return Ok(answer);
        }
        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportExcel([FromQuery] string sheet, [FromForm] IFormFile file)
        {

            if (file.Length > 0)
            {
                var extension = Path.GetExtension(file.FileName);
                if (extension.Equals(".csv") || extension.Equals(".xlsx"))
                {
                    var result = await file.ReadExcel();
                    var dataTable = result.Tables[sheet];
                    if (dataTable != null)
                    {
                        System.Console.WriteLine(dataTable.Rows.Count);
                        foreach (DataRow row in dataTable.Rows)
                        {
                            try
                            {
                                var question = new Question();
                                question.Toeic = row["ToeicPart"] == DBNull.Value ? ToeicPart.No : (ToeicPart)row["ToeicPart"];
                                question.Content = row["Content"] == DBNull.Value ? null : (string)row["Content"];
                                question.Explaination = row["Explaination"] == DBNull.Value ? null : (string)row["Explaination"];
                                question.CreatedDate = DateTime.Now;
                                var questionFromDb = await _repo.GetOneWithCondition<Question>(q => q.Content.Equals(question.Content));
                                if (questionFromDb == null)
                                {
                                    _repo.Create(question);
                                }
                            }
                            catch (System.Exception e)
                            {
                                System.Console.WriteLine(row["Content"]);
                                throw e;
                            }

                        }
                        if (await _repo.SaveAll())
                        {
                            return Ok();
                        }
                        else
                        {
                            return NoContent();
                        }
                    }
                }

            }
            return NoContent();

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(Guid id)
        {
            var questionFromDb = await _repo.GetOneWithCondition<Question>(ques => ques.Id == id);
            if (questionFromDb == null)
            {
                return NotFound();
            }
            _repo.Delete(questionFromDb);
            if (await _repo.SaveAll())
            {
                return Ok();
            }
            return StatusCode(500);
        }

    }
}