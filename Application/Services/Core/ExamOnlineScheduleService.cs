using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.ExamSchedule;
using Application.DTOs.Hub;
using Application.Helper;
using Application.Hubs;
using Application.Services.Core.Abstraction;
using AutoMapper;
using Domain.Models;
using Domain.Models.Version2;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Persistence;

namespace Application.Services.Core
{
    public class ExamOnlineScheduleService : IExamOnlineScheduleService
    {
        private readonly ApplicationDbContext _repo;
        private readonly IMapper _mapper;
        private readonly IHubContext<ExamHub> _examHub;
        private Timer _start;
        private Timer _questionTimer;
        public ExamOnlineScheduleService(ApplicationDbContext repo, IHubContext<ExamHub> examHub, IMapper mapper)
        {
            _repo = repo;
            _examHub = examHub;
            _mapper = mapper;
        }
        public async Task<ExamOnlineSchedule> CreateExamScheduleAsync(CreateExamScheduleDTO createExamSchedule)
        {
            var examOnline = _mapper.Map<ExamOnlineSchedule>(createExamSchedule);
            examOnline.End = examOnline.Start.Add(new TimeSpan(0, createExamSchedule.Duration, 0));
            HashSet<Question> questions = new HashSet<Question>();
            if (createExamSchedule.ExamId == null)
            {
                if (createExamSchedule.ListeningQuestionCount == 0 || createExamSchedule.ReadingQuestionCount == 0)
                {
                    return null;
                }
                var exam = new Exam
                {
                    Title = createExamSchedule.Title,
                    Detail = createExamSchedule.Detail,
                    TotalListening = createExamSchedule.ListeningQuestionCount.Value,
                    TotalReading = createExamSchedule.ReadingQuestionCount.Value,
                };
                _repo.Exam.Add(exam);
                List<Question> listeningQuestions = await _repo.Questions.Where(q => q.Type == Domain.Enums.QuestionType.Toeic).Include(q => q.Answers).ToListAsync();
                List<Question> readingQuestions = await _repo.Questions.Where(predicate: q => q.Type == Domain.Enums.QuestionType.Toeic).Include(q => q.Answers).ToListAsync();
                foreach (var q in listeningQuestions.GetAmountRandomFromAList(createExamSchedule.ListeningQuestionCount.Value))
                {
                    questions.Add(q);
                }
                foreach (var q in readingQuestions.GetAmountRandomFromAList(createExamSchedule.ReadingQuestionCount.Value))
                {
                    questions.Add(q);
                }
                examOnline.ExamId = exam.Id;

            }
            else
            {
                examOnline.ExamId = createExamSchedule.ExamId;
                var exam = await _repo.Exam.Where(predicate: e => e.Id == examOnline.ExamId).Include(exam => exam.Questions).ThenInclude(question => question.Question).ThenInclude(q => q.Answers).FirstOrDefaultAsync();
                foreach (var question in exam.Questions)
                {
                    questions.Add(question.Question);
                }
            }

            _repo.ExamOnlineSchedules.Add(examOnline);
            SetUpTimer(examOnline.Start, examOnline.ExamId.Value, createExamSchedule.Duration, questions.Count);
            GlobalVariable.Timers.Add(new ExamScheduleDTO
            {
                StartTimer = _start,
                ExamId = createExamSchedule.ExamId.Value,
                Questions = questions,
                QuestionTimer = _questionTimer,
                ElapseTime = createExamSchedule.Duration * 60000 / questions.Count
            });
            if (await _repo.SaveChangesAsync() > 0)
            {
                return examOnline;
            }
            return null;
        }
        private void SetUpTimer(TimeSpan alertTime, Guid examId, int duration, int totalQuestion)
        {
            DateTime current = DateTime.Now;
            TimeSpan timeToGo = alertTime - current.TimeOfDay;
            var elapseQuestion = duration * 60000 / totalQuestion;
            if (timeToGo < TimeSpan.Zero)
            {
                timeToGo = timeToGo.Add(new TimeSpan(24, 0, 0));
            }
            this._start = new System.Threading.Timer(x =>
            {
                this.SomeMethodRunsAt1600(examId, elapseQuestion);
            }, null, timeToGo, Timeout.InfiniteTimeSpan);
        }

        private void SomeMethodRunsAt1600(Guid examId, int elapseQuestion)
        {
            var timer = GlobalVariable.Timers.Where(e => e.ExamId == examId).FirstOrDefault();

            //this runs at 16:00:00

            _questionTimer = new Timer(new TimerCallback(SendQuestion), timer, 0, elapseQuestion);

        }
        private async void SendQuestion(object o)
        {
            var timer = (ExamScheduleDTO)o;
            var remainQuestions = timer.Questions;
            var clients = HubHelper.ExamClientsConnections.Where(ec => ec.Exam.ExamId == timer.ExamId).ToList();
            var nextQuestion = remainQuestions.GetOneRandomFromList();
            if (clients.Count > 0)
            {
                remainQuestions.Remove(nextQuestion);
            }
            if (remainQuestions.Count == 0)
            {
                await this._questionTimer.DisposeAsync();
                foreach (var client in clients)
                {
                    await _examHub.Clients.Client(client.ClientId).SendAsync("ExamEnd");
                }
                System.Console.WriteLine("Thread disposed");
            }
            dynamic returnQuestion = new ExpandoObject();
            returnQuestion.Question = nextQuestion;
            returnQuestion.ElapseTime = timer.ElapseTime;
            string serializedQuestion = Extension.CamelCaseSerialize(returnQuestion);
            foreach (var client in clients)
            {
                await _examHub.Clients.Client(client.ClientId).SendAsync("NextQuestion", serializedQuestion);
            }
            System.Console.WriteLine("Đã gửi câu hỏi");
        }
    }
}