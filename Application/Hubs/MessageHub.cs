using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Helper;
using Application.Services.Core.Abstraction;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Hubs
{
    public class MessageHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ISectionService _sectionService;
        public MessageHub(ApplicationDbContext context, IMapper mapper, ISectionService sectionService)
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
    }
}