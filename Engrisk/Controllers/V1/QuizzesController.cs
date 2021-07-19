using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Engrisk.Data;
using Application.DTOs;
using Application.DTOs.Quiz;
using Engrisk.Helper;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Application.DTOs.Answer;

namespace Engrisk.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[Controller]")]
    public class QuizzesController : ControllerBase
    {
        private readonly ICRUDRepo _repo;
        private readonly IMapper _mapper;
        public QuizzesController(ICRUDRepo repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllQuizzes([FromQuery] SubjectParams subjectParams)
        {
            var quizzesFromDb = await _repo.GetAll<Quiz>(null, "Section, Questions");
            var quizzes = _mapper.Map<IEnumerable<QuizDetailDTO>>(quizzesFromDb);
            return Ok(quizzes);
        }
        //Tạo quiz nhanh cho bài học từ vựng
        [HttpPost]
        public async Task<IActionResult> CreateQuiz([FromForm] QuizCreateDTO quizCreateDTO)
        {
            try
            {
                var quiz = _mapper.Map<Quiz>(quizCreateDTO);
                _repo.Create(quiz);
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
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuiz(Guid id, [FromForm] QuizCreateDTO quiz)
        {
            var quizFromDb = await _repo.GetOneWithConditionTracking<Quiz>(u => u.Id == id);
            if (quizFromDb == null)
            {
                return NotFound(new
                {
                    Error = "Không tìm thấy quiz"
                });
            }
            _mapper.Map(quiz, quizFromDb);
            if (await _repo.SaveAll())
            {
                return Ok();
            }
            throw new Exception("Error on updating quiz");
        }
        [HttpPost("{quizId}/questions")]
        public async Task<IActionResult> AddQuestionToQuiz(Guid quizId, [FromQuery] Guid questionId)
        {
            try
            {
                var quizFromDb = await _repo.GetOneWithCondition<Quiz>(u => u.Id == quizId);
                if (quizFromDb == null)
                {
                    return NotFound(new
                    {
                        Error = "Không tìm thấy quiz"
                    });
                }
                var questionFromDb = await _repo.GetOneWithCondition<Question>(q => q.Id == questionId);
                if (questionFromDb == null)
                {
                    return NotFound(new
                    {
                        Error = "Không tìm thấy câu hỏi"
                    });
                }
                var questionInQuiz = await _repo.GetOneWithCondition<QuizQuestion>(q => q.QuestionId == questionId && q.QuizId == quizId);
                if (questionInQuiz == null)
                {
                    var quizQuestion = new QuizQuestion()
                    {
                        QuizId = quizId,
                        QuestionId = questionId
                    };
                    _repo.Create(quizQuestion);
                    if (await _repo.SaveAll())
                    {
                        return Ok();
                    }
                    return NoContent();
                }
                _repo.Delete(questionInQuiz);
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
        public async Task<IActionResult> DeleteQuiz(Guid id)
        {
            var quizFromDb = await _repo.GetOneWithCondition<Quiz>(q => q.Id == id);
            if (quizFromDb == null)
            {
                return NotFound(new
                {
                    Error = "Không tìm thấy quiz"
                });
            }
            _repo.Delete<Quiz>(quizFromDb);
            if (await _repo.SaveAll())
            {
                return Ok();
            }
            throw new Exception("Error on deleting quiz");
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> DoQuiz(Guid id)
        {
            var quizQuery = await _repo.GetOneWithManyToMany<Quiz>(q => q.Id == id);
            var quiz = await quizQuery.Include(q => q.Questions).ThenInclude(q => q.Question).ThenInclude(inc => inc.Answers).FirstOrDefaultAsync();
            if (quiz == null)
            {
                return NotFound();
            }
            var returnQuestions = _mapper.Map<QuizDTO>(quiz);
            return Ok(returnQuestions);
        }
        // [HttpPost("{id}/done")]
        // public async Task<IActionResult> DoneExam(Guid id, List<AnswerDTO> answers)
        // {
        //     try
        //     {
        //         var examFromDb = await _repo.GetOneWithConditionTracking<Quiz>(quiz => quiz.Id == id, "Questions");
        //         if (examFromDb == null)
        //         {
        //             return NotFound();
        //         }
        //         foreach (var answer in answers)
        //         {
        //             var question = examFromDb.Questions.FirstOrDefault(question => question.QuestionId == answer.Id);
        //         }

        //         if (User.FindFirst(ClaimTypes.NameIdentifier) != null)
        //         {
        //             var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        //             var accountFromDb = await _repo.GetOneWithConditionTracking<Account>(acc => acc.Id == accountId);
        //             var item = await _repo.GetOneWithCondition<AccountStorage>(store => store.AccountId == accountId && store.IsUsing == true, "Item");
        //             switch (item.Item.ItemName.ToLower())
        //             {
        //                 case "x2 exp":
        //                     accountFromDb.Exp += examFromDb.ExpGain * 2;
        //                     break;
        //                 default:
        //                     accountFromDb.Exp += examFromDb.ExpGain;
        //                     break;
        //             }

        //             var historyFromDb = await _repo.GetOneWithConditionTracking<History>(history => history.QuizId == id && history.AccountId == accountFromDb.Id && history.IsDone == false);
        //             historyFromDb.EndTimestamp = DateTime.Now;
        //             historyFromDb.TimeSpent = (int)Math.Round(DateTime.Now.MinusDate(historyFromDb.StartTimestamp));
        //             historyFromDb.IsDone = true;
        //         }
        //         if (await _repo.SaveAll())
        //         {
        //             return Ok(new
        //             {
        //                 answers = answers
        //             });
        //         }
        //         return NoContent();
        //     }
        //     catch (System.Exception e)
        //     {

        //         throw e;
        //     }

        // }
    }
}
