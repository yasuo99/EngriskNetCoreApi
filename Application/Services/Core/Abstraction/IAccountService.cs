using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Account;
using Application.DTOs.Account.Boxchat;
using Application.DTOs.Account.Follow;
using Application.DTOs.Account.Sharing;
using Application.DTOs.Certificate;
using Application.DTOs.Exam;
using Application.DTOs.Group;
using Application.DTOs.Pagination;
using Application.DTOs.Question;
using Application.DTOs.Quiz;
using Application.DTOs.Ticket.VerifyTicket;
using Application.DTOs.Word;
using Domain.Enums;
using Domain.Models;
using Domain.Models.Version2;

namespace Application.Services.Core.Abstraction
{
    public interface IAccountService
    {
        //Account Entity
        /// <summary>
        /// Get account detail asynchronous
        /// </summary>
        /// <param name="id">Identify of account</param>
        /// <returns>Account object</returns>
        Task<Account> GetAccountAsync(int id);
        Task<AccountDetailDTO> GetAccountWithRolesAsync(int id);
        Task<List<string>> GetAccountRolesAsync(int id);
        Task<int> CheckAccounts(List<int> users);

        //Follow Entity
        Task<List<FollowingDTO>> GetAccountFollowing(int id);
        Task<List<FollowingDTO>> GetAccountFollower(int id);
        Task<bool> FollowingUserAsync(Account follower, Account following);
        Task SwitchNotificationAsync(Account follower, Account following);


        //BoxChat Entity
        Task<bool> CheckBoxchatInviteAsync(Guid notificationId, int id);
        Task<bool> CheckBoxchatOwnerAsync(Guid boxchatId, int id);
        Task<BoxChat> CreateBoxChatAsync(BoxchatCreateDTO boxchatCreateDTO);
        Task<List<BoxchatDTO>> GetUserBoxchatAsync(int accountId);
        Task InviteUserToBoxchat(int userId, Guid boxchatId);
        Task InviteUsersToBoxchatAsync(Guid boxchatId, List<int> users);
        Task RemoveUserFromBoxchat(int userId, Guid boxchatId);
        Task AcceptBoxChatInvite(int accountId, Guid notificationId, string action);
        Task<BoxchatDTO> GetBoxchatMessageAsync(int accountId, Guid boxChatId);
        //Section Entity
        Task<Section> CreateSectionAsync(int accountId, Section section);

        //Group entity
        Task<List<GroupDTO>> GetAllUserGroupAsync(int accountId);
        Task<Group> GetGroupAsync(Guid groupId);
        Task WordGroupActionAsync(Group group, Word word);

        //Word entity
        Task<Word> GetWordAsync(Guid id);
        Task<WordLearntResultDTO> GetWordLearntPathAsync(int accountId);
        Task<List<QuestionDTO>> VocabularyReviewAsync(int accountId, string option);
        Task<bool> CheckReviewQuestionAnswer(int accountId, Guid questionId, Guid answerId);

        //Quiz entity
        Task<PaginateDTO<QuizDTO>> GetSharedQuizAsync(int accountId, PaginationDTO pagination);
        Task<dynamic> GetQuizSharingInformationAsync(int accountId, Guid quizId);
        Task<bool> SharingQuizAsync(int accountId, Guid quizId, List<int> users);
        Task<bool> CheckQuizAsync(int accountId, Guid quizId);
        Task<bool> DeleteQuizAsync(int accountId, Guid quizId);
        
        //Exam entity
        Task<PaginateDTO<ExamDTO>> GetSharedExamAsync(int accountId, PaginationDTO pagination);
        Task<dynamic> GetExamSharingInformationAsync(int accountId, Guid examId);
        Task<bool> SharingExamAsync(int accountId, Guid examId, List<int> users);
        Task<bool> CheckExamAsync(int accountId, Guid examId);
        Task<bool> DeleteExamAsync(int accountId, Guid examId);

        //Resourse
        Task<ResourcesSharedDTO> GetSharedResourcesWithUserAsync(int accountId);
        //Learning
        Task<DailyLearningDTO> GetDailyLearningAsync(int accountId, DateRangeDTO dateRange);
        //Certificates
        Task<PaginateDTO<AccountCertificate>> GetUserCertificatesAsync(PaginationDTO pagination, int accountId, string search = null);
        //Question
        Task<PaginateDTO<QuestionDTO>> GetUserQuestionAsync(PaginationDTO pagination,QuestionType type, int accountId, string search = null);
    }
}