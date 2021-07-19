using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Timers;
using Application.DTOs.Answer;
using Application.DTOs.Exam;
using Application.DTOs.Hub;
using Application.Helper;

using Application.Services.Core.Abstraction;
using Domain.Models;
using Domain.Models.Version2;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Application.Hubs
{
    public class ExamHub : Hub
    {
        private readonly IQuestionService _questionService;
        private readonly UserManager<Account> _userManager;
        private Timer timer;
        public ExamHub(IQuestionService questionService, UserManager<Account> userManager)
        {
            _questionService = questionService;
            _userManager = userManager;
        }
        public override async Task OnConnectedAsync()
        {
            var clientId = Context.ConnectionId;
        }
        public async Task SubmitAnswer(ClientAnswerDTO answer)
        {
            try
            {
                answer.Result = await _questionService.CheckAnswerAsync(answer.QuestionId, answer.Answer);
                var clientId = Context.ConnectionId;
                var clientAnswer = HubHelper.ExamClientsConnections.Where(client => client.Exam.ExamId == answer.ExamId && client.ClientId == Context.ConnectionId).FirstOrDefault();
                clientAnswer.Exam.Answers.Add(answer);
                HubHelper.ExamClientsConnections.OrderByDescending(client => client.Exam.Answers.Where(a => a.Result == true).Count()).OrderBy(client => client.Exam.Answers.Sum(ans => ans.TimeSpent));
                var ranking = Extension.CamelCaseSerialize(HubHelper.ExamClientsConnections);
                var clients = HubHelper.ExamClientsConnections.Where(client => client.Exam.ExamId == answer.ExamId);
                foreach (var client in clients)
                {
                    await Clients.Client(client.ClientId).SendAsync("Ranking", ranking);
                }
            }
            catch (System.Exception ex)
            {
                // TODO
                throw ex;
            }

        }
        public async Task DoExam(ClientExamDTO clientExam)
        {
            var clientId = Context.ConnectionId;
            var accountId = 0;
            // var account = await _userManager.FindByIdAsync(Context.UserIdentifier);
            if (Context.UserIdentifier != null)
            {
                accountId = Int32.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }
            HubHelper.ExamClientsConnections.Add(new ExamClient()
            {
                AccountId = accountId,
                ClientId = clientId,
                Platform = 1,
                Exam = clientExam
            });
        }
        public override async Task OnDisconnectedAsync(Exception e)
        {
            var clientId = Context.ConnectionId;
            var accountId = 0;
            if (Context.UserIdentifier != null)
            {
                accountId = Int32.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }
            HubHelper.ExamClientsConnections.RemoveWhere(c => c.ClientId == Context.ConnectionId && c.AccountId == accountId);
        }
    }
}