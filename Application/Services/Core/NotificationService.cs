using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Notification;
using Application.DTOs.Pagination;
using Application.Helper;
using Application.Hubs;
using Application.Utilities;
using AutoMapper;
using Domain.Models;
using Domain.Models.Version2;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Persistence;
using Application.Services.Core.Abstraction;
using Domain.Enums;
using System;

namespace Application.Services.Core
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly IMapper _mapper;
        public NotificationService(ApplicationDbContext context, IMapper mapper, IHubContext<NotificationHub> hub)
        {
            _context = context;
            _mapper = mapper;
            _hub = hub;
        }
        public async Task<Notification> CreateNotificationAsync(int senderId, AdminNotificationCreateDTO notificationCreateDTO)
        {
            List<NotificationClient> receivers = new List<NotificationClient>();
            var notification = _mapper.Map<Notification>(notificationCreateDTO);
            var sender = await _context.Accounts.FirstOrDefaultAsync(acc => acc.Id == senderId);
            notification.From = sender;
            _context.Notifications.Add(notification);
            foreach (var receiverId in notificationCreateDTO.Receivers)
            {
                var receiver = await _context.Accounts.FirstOrDefaultAsync(acc => acc.Id == receiverId);
                receiver.ReceivedNotification.Add(new AccountNotification
                {
                    Notification = notification,
                    Status = NotificationStatus.Unseen
                });

            }
            if (await _context.SaveChangesAsync() > 0)
            {
                var returnNotification = _mapper.Map<ResponseNotificationDTO>(notification);
                await SendSignalRResponseToClients("NewNotification", notificationCreateDTO.Receivers, returnNotification);
                return notification;
            }
            return null;
        }

        public async Task<PaginateDTO<Notification>> GetAllNotificationAsync(PaginationDTO pagination)
        {
            var notifications = _context.Notifications.Where(notification => notification.FromId != null);
            var prePaging = await PagingList<Notification>.OnCreateAsync(notifications, pagination.CurrentPage, pagination.PageSize);
            return prePaging.CreatePaginate();
        }

        public async Task<List<AccountReceiveNotificationDTO>> GetAllUsers(int accountId)
        {
            var accounts = await _context.Accounts.Where(acc => acc.Id != accountId && acc.Roles.Any(r => r.Role.Name == "teacher" || r.Role.Name == "learner" || r.Role.Name == "student")).Select(sl => new AccountReceiveNotificationDTO { AccountId = sl.Id, Username = sl.UserName }).ToListAsync();
            return accounts;
        }

        public async Task<PaginateDTO<AccountNotificationDTO>> GetUserNotificationsAsync(int userId, PaginationDTO pagination)
        {
            var notifications = from notify in _context.AccountNotifications.Where(ac => ac.AccountId == userId).Include(inc => inc.Notification).AsNoTracking() select notify;
            var paginateNotifications = await PagingList<AccountNotification>.OnCreateAsync(notifications, pagination.CurrentPage, pagination.PageSize);
            var result = paginateNotifications.CreatePaginate();
            var returnNotifications = _mapper.Map<List<AccountNotificationDTO>>(result.Items);
            return new PaginateDTO<AccountNotificationDTO>
            {
                CurrentPage = pagination.CurrentPage,
                PageSize = pagination.PageSize,
                Items = returnNotifications,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };
        }

        public async Task SeenNotification(int userId, Guid notificationId)
        {
            var notification = await _context.AccountNotifications.FirstOrDefaultAsync(an => an.AccountId == userId && an.NotificationId == notificationId);
            notification.Status = NotificationStatus.Seen;
            await _context.SaveChangesAsync();
        }

        public async Task SendNotification(Account sender, Account receiver, string content, NotificationType type, string url)
        {
            var notification = new Notification
            {
                Content = content,
                Url = url,
                Type = Domain.Enums.NotificationType.info,
                From = sender
            };
            sender.CreatedNotification.Add(notification);
            receiver.ReceivedNotification.Add(new AccountNotification
            {
                Notification = notification,
                Status = NotificationStatus.Unseen
            });
            await _context.SaveChangesAsync();
            var client = HubHelper.NotificationClientsConnections.FirstOrDefault(client => client.AccountId == receiver.Id);
            if (client != null)
            {
                var returnNotification = _mapper.Map<ResponseNotificationDTO>(notification);
                var serializeResponse = Extension.CamelCaseSerialize(returnNotification);
                await _hub.Clients.Client(client.ClientId).SendAsync("NewNotification", serializeResponse);
            }
        }

        public async Task SendSignalRResponse(string method, object arg)
        {
            var responseObject = Extension.CamelCaseSerialize(arg);
            foreach (var client in HubHelper.NotificationClientsConnections)
            {
                await _hub.Clients.Client(client.ClientId).SendAsync(method, responseObject);
            }
        }

        public async Task SendSignalRResponseToClient(string method, int accountId, object arg)
        {
            var responseObject = Extension.CamelCaseSerialize(arg);
            var receiver = HubHelper.NotificationClientsConnections.Where(nc => nc.AccountId == accountId).ToList();
            if (receiver.Count > 0)
            {
                foreach (var client in receiver)
                {
                    await _hub.Clients.Client(client.ClientId).SendAsync(method, responseObject);
                }

            }
        }

        public async Task SendSignalRResponseToClients(string method, List<int> accounts, object arg)
        {
            var responseObject = Extension.CamelCaseSerialize(arg);
            foreach (var id in accounts)
            {
                var receiver = HubHelper.NotificationClientsConnections.Where(nc => nc.AccountId == id).ToList();
                if (receiver.Count > 0)
                {
                    foreach (var client in receiver)
                    {
                        await _hub.Clients.Client(client.ClientId).SendAsync(method, responseObject);
                    }

                }
            }
        }
    }
}