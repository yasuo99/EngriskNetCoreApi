using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Account.Boxchat;
using Application.DTOs.Hub;
using Application.DTOs.Section;
using Application.Helper;
using Application.Services.Core.Abstraction;
using AutoMapper;
using Domain.Models.Version2;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Persistence;

namespace Application.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ISectionService _sectionService;
        private int i = 0;
        public NotificationHub(ApplicationDbContext context, IMapper mapper, ISectionService sectionService)
        {
            _context = context;
            _mapper = mapper;
            _sectionService = sectionService;
        }
        public override async Task OnConnectedAsync()
        {
            var clientId = Context.ConnectionId;
            if (Context.UserIdentifier != null)
            {
                var accountId = Int32.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var account = await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(acc => acc.Id == accountId);
                HubHelper.NotificationClientsConnections.Add(new DTOs.Notification.NotificationClient
                {
                    ClientId = clientId,
                    AccountId = accountId,
                    Username = account.UserName
                });
                var onlineList = HubHelper.NotificationClientsConnections.Select(sel => sel.AccountId);
                await Clients.All.SendAsync("NewOnline", account.UserName);
                await Clients.Clients(clientId).SendAsync("CurrentOnline", HubHelper.NotificationClientsConnections.Where(client => client.AccountId != accountId).Select(sel => sel.Username).CamelcaseSerialize());
            }
        }
        public async Task PracticeSync(List<Guid> words)
        {
            var clientId = Context.ConnectionId;
            var accountId = Int32.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userClients = HubHelper.NotificationClientsConnections.Where(uc => uc.AccountId == accountId);
            foreach (var client in userClients)
            {
                await Clients.Clients(client.ClientId).SendAsync("PracticeSync", words.CamelcaseSerialize());
            }
        }
        public async Task DoExam()
        {

        }
        public async Task SendMessage(MessageDTO messageDTO)
        {
            var sender = await _context.Accounts.FirstOrDefaultAsync(acc => acc.Id == messageDTO.FromId);

            var message = new Message
            {
                Content = messageDTO.Content,
                IsEdited = false,
                Account = sender,
            };

            var boxchat = await _context.BoxChats.Where(box => box.Id == messageDTO.BoxchatId).Include(inc => inc.Members).FirstOrDefaultAsync();
            boxchat.Messages.Add(message);
            if (await _context.SaveChangesAsync() > 0)
            {
                var responseMessage = _mapper.Map<MessageResponseDTO>(message);
                var boxchatHost = HubHelper.NotificationClientsConnections.FirstOrDefault(rc => rc.AccountId == boxchat.AccountId);
                //Gửi tin nhắn cho member của box chat
                foreach (var mem in boxchat.Members.Where(mem => mem.Status == Domain.Enums.Status.Approved))
                {
                    var receiver = HubHelper.NotificationClientsConnections.FirstOrDefault(rc => rc.AccountId == mem.AccountId);
                    if (receiver != null)
                    {
                        await Clients.Client(receiver.ClientId).SendAsync("NewMessage", responseMessage.CamelcaseSerialize());

                    }
                }
                //Gửi tin nhắn cho chủ host của box
                if (boxchatHost != null)
                {
                    await Clients.Client(boxchatHost.ClientId).SendAsync("NewMessage", responseMessage.CamelcaseSerialize());
                }

            };


        }
        public async Task Support(string message)
        {
            var clientId = Context.ConnectionId;
            System.Console.WriteLine(message);
            await Clients.Client(clientId).SendAsync("SupportResponse", $"Có cc nè bạn {new Random().Next(0, 10000)}");
        }
        public async Task SectionDone(SectionDoneDTO section)
        {
            var progress = await _sectionService.SectionFinishUpAsync(section.SectionId, section.AccountId, "finish");
            await Clients.Client(Context.ConnectionId).SendAsync("SectionProgress", progress.CamelcaseSerialize());
        }
        public async Task ScriptDone(ScriptDoneDTO script)
        {
            var nextScript = await _sectionService.ScriptDoneAsync(script.ScriptId, script.AccountId);
            if (nextScript != null)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("cc","clm");
                System.Console.WriteLine(nextScript.CamelcaseSerialize());
                await Clients.Client(Context.ConnectionId).SendAsync("NextScript", nextScript.CamelcaseSerialize());
            }
            else
            {
                var progress = await _sectionService.SectionFinishUpAsync(script.SectionId, script.AccountId, "finish");
                await Clients.Client(Context.ConnectionId).SendAsync("SectionProgress", progress.CamelcaseSerialize());
            }
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var clientId = Context.ConnectionId;
            if (Context.UserIdentifier != null)
            {
                int userId = Int32.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var account = await _context.Accounts.FirstOrDefaultAsync(acc => acc.Id == userId);
                HubHelper.NotificationClientsConnections.RemoveWhere(client => client.AccountId == userId && client.ClientId == clientId);
                await Clients.All.SendAsync("NewOffline", account.UserName);
            }
            System.Console.WriteLine(clientId);
        }
    }
}