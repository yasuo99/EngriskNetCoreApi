
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Helper;
using AutoMapper;
using Engrisk.Data;
using Application.DTOs;
using Application.DTOs.Exam;
using Application.DTOs.Question;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Application.DTOs.Answer;
using Domain.Enums;

namespace Engrisk.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[Controller]")]
    public class ExamsController : ControllerBase
    {
        private readonly ICRUDRepo _repo;
        private readonly IMapper _mapper;
        public ExamsController(ICRUDRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllExam([FromQuery] SubjectParams subjectParams)
        {
            var examsFromDb = await _repo.GetAll<Exam>(subjectParams, expression: null, includeProperties: "ExamHistories", exam => exam.OrderByDescending(o => o.Questions.Count()));
            if (User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                foreach (var exam in examsFromDb)
                {
                    exam.ExamHistories = exam.ExamHistories.Where(history => history.AccountId == userId && history.IsDoing == false).ToList();
                }
            }
            else
            {
                examsFromDb = examsFromDb.SetNullProperty("ExamHistories");
            }
            var returnExams = _mapper.Map<IEnumerable<ExamDTO>>(examsFromDb);
            Response.AddPaginationHeader(examsFromDb.CurrentPage, examsFromDb.PageSize, examsFromDb.TotalItems, examsFromDb.TotalPages);
            return Ok(returnExams);
        }
        [HttpGet("manage")]
        public async Task<IActionResult> GetManage()
        {
            var examsFromDb = await _repo.GetAll<Exam>(null, "");
            var returnExams = _mapper.Map<IEnumerable<ExamDTO>>(examsFromDb);
            return Ok(returnExams);
        }
        [HttpGet("accounts/ranking")]
        public async Task<IActionResult> GetRankingExam([FromQuery] string category)
        {
            var exams = await _repo.GetAll<ExamHistory>(e => e.IsDoing == false, "Exam, Account");
            var returnRanking = exams.OrderBy(e => e.TotalTime).OrderByDescending(e => e.Score).GroupBy(e => e.ExamId).Select(e => new ExamRankingDTO() { ExamId = e.FirstOrDefault().ExamId, ExamTitle = e.FirstOrDefault().Exam.Title, Score = e.Max(e => e.Score), AccountId = e.FirstOrDefault().AccountId, AccountUsername = e.FirstOrDefault().Account.UserName, TotalTime = e.FirstOrDefault().TotalTime, TotalScore = e.FirstOrDefault().Exam.TotalScore });
            return Ok(returnRanking);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            var examQuery = await _repo.GetOneWithManyToMany<Exam>(exam => exam.Id == id);
            var exam = await examQuery.Include(e => e.Questions).ThenInclude(q => q.Question).FirstOrDefaultAsync();
            if (exam == null)
            {
                return NotFound();
            }
            var returnExam = _mapper.Map<ExamDTO>(exam);
            return Ok(returnExam);
        }
        [HttpGet("histories/{accountId}")]
        public async Task<IActionResult> GetHistories(int accountId)
        {
            var histories = await _repo.GetAll<ExamHistory>(e => e.AccountId == accountId && e.IsDoing == false, "Exam");
            var returnHistories = _mapper.Map<IEnumerable<ExamHistoryDTO>>(histories.OrderByDescending(e => e.Score));
            return Ok(returnHistories);
        }
        [HttpGet("{id}/do")]
        public async Task<IActionResult> DoExam(Guid id)
        {
            var examQuery = await _repo.GetOneWithManyToMany<Exam>(exam => exam.Id == id);
            var exam = await examQuery.Include(e => e.Questions).ThenInclude(q => q.Question).FirstOrDefaultAsync();
            if (exam == null)
            {
                return NotFound();
            }
            var returnExam = _mapper.Map<ExamDTO>(exam);
            returnExam.Questions = returnExam.Questions.OrderBy(q => q.Toeic);
            returnExam.Questions = returnExam.Questions.Shuffle();
            if (User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var histories = await _repo.GetOneWithCondition<ExamHistory>(e => e.ExamId == id && e.AccountId == userId && e.IsDoing);
                if (histories == null)
                {
                    var history = new ExamHistory()
                    {
                        AccountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value),
                        ExamId = id,
                        Timestamp_start = DateTime.Now,
                        IsDoing = true
                    };
                    _repo.Create(history);
                    await _repo.SaveAll();
                }
                return Ok(returnExam);
            }
            return Ok(returnExam);
        }
        [HttpPost]
        public async Task<IActionResult> CreateExam([FromBody] ExamCreateDTO examCreateDTO)
        {
            try
            {
                var exam = _mapper.Map<Exam>(examCreateDTO);
                if (await _repo.Exists<Exam>(exam))
                {
                    return Conflict();
                }
                _repo.Create(exam);
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
        public async Task<IActionResult> UpdateExam(Guid id, [FromBody] ExamCreateDTO examCreateDTO)
        {

            try
            {
                var examFromDb = await _repo.GetOneWithConditionTracking<Exam>(e => e.Id == id);
                if (examFromDb == null)
                {
                    return NotFound(new
                    {
                        Error = "Không tìm thấy exam"
                    });
                }
                _mapper.Map(examCreateDTO, examFromDb);
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
        [HttpPost("{id}/done")]
        public async Task<IActionResult> DoneExam(Guid id, [FromBody] IEnumerable<AnswerDTO> answers)
        {
            var examQuery = await _repo.GetOneWithManyToMany<Exam>(exam => exam.Id == id);
            var exam = await examQuery.Include(e => e.Questions).ThenInclude(q => q.Question).FirstOrDefaultAsync();
            if (exam == null)
            {
                return NotFound();
            }
            var returnExam = _mapper.Map<ExamAnswerDTO>(exam);
            var listeningRightCount = 0;
            var readingRightCount = 0;
            foreach (var answer in answers)
            {
                var questionAnswer = returnExam.Questions.Where(q => q.Id == answer.Id).FirstOrDefault();
                if (answer.Answer.Equals(questionAnswer.Answer))
                {
                    questionAnswer.IsRightAnswer = true;
                    if (questionAnswer.Type == Enum.GetName(typeof(QuestionType), QuestionType.Listening))
                    {
                        listeningRightCount++;
                    }
                    else
                    {
                        readingRightCount++;
                    }
                }
                else
                {
                    questionAnswer.IsRightAnswer = false;
                }
                questionAnswer.UserAnswer = answer.Answer;
            }
            var listeningScores = await _repo.GetOneWithCondition<ListeningToeicRedeem>(n => n.NumOfSentences == listeningRightCount);
            var readingScores = await _repo.GetOneWithCondition<ReadingToeicRedeem>(n => n.NumOfSentences == readingRightCount);
            var totalScore = listeningScores.Score + readingScores.Score;
            var totalRightAnswer = listeningRightCount + readingRightCount;
            if (User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var historyFromDb = await _repo.GetOneWithConditionTracking<ExamHistory>(h => h.ExamId == id && h.AccountId == userId && h.IsDoing);
                historyFromDb.IsDoing = false;
                historyFromDb.Score = totalScore;
                historyFromDb.Timestamp_end = DateTime.Now;
                historyFromDb.TotalTime = (int)Math.Round(DateTime.Now.MinusDate(historyFromDb.Timestamp_start), MidpointRounding.AwayFromZero);
                await _repo.SaveAll();
            }
            return Ok(new
            {
                score = totalScore,
                listening = listeningRightCount,
                reading = readingRightCount,
                answer = returnExam
            });
        }
        [HttpGet("{id}/answers")]
        public async Task<IActionResult> GetAnswers(Guid id)
        {
            // var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            // var user = await _repo.GetOneWithCondition<Account>(acc => acc.Id == id);
            var examQuery = await _repo.GetOneWithManyToMany<Exam>(exam => exam.Id == id);
            var exam = await examQuery.Include(e => e.Questions).ThenInclude(q => q.Question).FirstOrDefaultAsync();
            if (exam == null)
            {
                return NotFound();
            }
            var returnExam = _mapper.Map<ExamAnswerDTO>(exam);
            return Ok(returnExam);
        }
        [HttpPut("{id}/questions/{questionId}")]
        public async Task<IActionResult> AddQuestionToExam(Guid id, Guid questionId)
        {
            try
            {
                var examFromDb = await _repo.GetOneWithConditionTracking<Exam>(exam => exam.Id == id, "Questions");
                if (examFromDb == null)
                {
                    return NotFound(new
                    {
                        Error = "Không tìm thấy exam"
                    });
                }
                var questionOfExam = examFromDb.Questions.FirstOrDefault(q => q.QuestionId == questionId);
                var questionFromDb = await _repo.GetOneWithCondition<Question>(q => q.Id == questionId);
                if (questionOfExam == null)
                {
                    var questionExam = new ExamQuestion()
                    {
                        QuestionId = questionId,
                        ExamId = id,
                    };
                    _repo.Create(questionExam);
                }
                else
                {
                    _repo.Delete(questionOfExam);
                }
                var listening = await _repo.GetOneWithCondition<ListeningToeicRedeem>(n => n.NumOfSentences == examFromDb.TotalListening);
                var reading = await _repo.GetOneWithCondition<ReadingToeicRedeem>(n => n.NumOfSentences == examFromDb.TotalReading);
                examFromDb.TotalScore = listening.Score + reading.Score;
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
        [HttpDelete("{id}/questions")]
        public async Task<IActionResult> RemoveQuestionFromExam(Guid id, [FromQuery] Guid questionId)
        {
            try
            {
                var examFromDb = await _repo.GetOneWithCondition<Exam>(exam => exam.Id == id, "Questions");
                if (examFromDb == null)
                {
                    return NotFound(new
                    {
                        Error = "Không tìm thấy exam"
                    });
                }
                var questionOfExam = examFromDb.Questions.FirstOrDefault(q => q.QuestionId == questionId);
                if (questionOfExam == null)
                {
                    return NotFound(new
                    {
                        Error = "Không tìm thấy câu hỏi"
                    });
                }
                _repo.Delete(questionOfExam);
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
        public async Task<IActionResult> DeleteExam(Guid id)
        {
            try
            {
                var examFromDb = await _repo.GetOneWithCondition<Exam>(exam => exam.Id == id);
                if (examFromDb == null)
                {
                    return NotFound(new
                    {
                        Error = "Không tìm thấy exam"
                    });
                }
                _repo.Delete(examFromDb);
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
    }
}