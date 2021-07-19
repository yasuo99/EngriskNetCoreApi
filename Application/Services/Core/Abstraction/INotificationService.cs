using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Notification;
using Application.DTOs.Pagination;
using Domain.Enums;
using Domain.Models;

namespace Application.Services.Core.Abstraction
{
    public interface INotificationService
    {
        Task<PaginateDTO<Notification>> GetAllNotificationAsync(PaginationDTO pagination);
        Task<PaginateDTO<AccountNotificationDTO>> GetUserNotificationsAsync(int userId, PaginationDTO pagination);
        Task<List<AccountReceiveNotificationDTO>> GetAllUsers(int accountId);
        Task SendNotification(Account sender, Account receiver, string content, NotificationType type, string url);
        Task<Notification> CreateNotificationAsync(int senderId, AdminNotificationCreateDTO notificationCreateDTO);
        Task SeenNotification(int userId, Guid notificationId);
        Task SendSignalRResponse(string method, object arg);
        Task SendSignalRResponseToClient(string method, int accountId, object arg);
        Task SendSignalRResponseToClients(string method, List<int> accounts, object arg);
    }
}