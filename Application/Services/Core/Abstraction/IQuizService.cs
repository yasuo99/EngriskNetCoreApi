using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Pagination;
using Application.DTOs.Quiz;
using Domain.Enums;
using Domain.Models;

namespace Application.Services.Core.Abstraction
{
    public interface IQuizService
    {
        Task<bool> CheckExistAsync(Guid id);
        Task<PaginateDTO<QuizDTO>> GetAllQuizAsync(PaginationDTO pagination, Status status = Status.Approved, string search = null);
        Task<QuizDTO> GetQuizAsync(Guid id);
        Task<bool> UpdateQuizAsync(Guid id, QuizDTO quizUpdateDto);
        Task<bool> VerifyQuizAsync(Guid quizId, Status status);
        Task<Quiz> ClientCreateQuizAsync(int accountId, QuizCreateDTO quizCreateDTO);
        Task<Quiz> AdminCreateQuizAsync(QuizCreateDTO quizCreateDTO);
        Task AddQuestionToQuizAsync(Guid id, Guid questionId);
        Task<bool> CheckQuestionAnswerAsync(Guid quizId, Guid questionId, Guid answerId);
        Task<bool> ShareQuizAsync(Guid quizId, int ownerId, string username);
        Task<bool> ShareQuizAsync(Guid quizId, int ownerId, List<int> users);
        Task<bool> CheckAccountQuizAsync(int ownerId, Guid quizId);
        Task<bool> DeleteQuizAsync(Guid quizId);
        Task<PaginateDTO<QuizDTO>> GetUserQuizzesAsync(int accountId, PaginationDTO pagination, string search = null);
        Task<List<QuizDTO>> GetSharedQuizAsync(int accountId);
    }
}