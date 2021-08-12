using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Pagination;
using Application.DTOs.Question;
using Application.DTOs.Quiz;
using Domain.Enums;
using Domain.Models;

namespace Application.Services.Core.Abstraction
{
    public interface IQuizService: IPublishService
    {
        Task<bool> CheckExistAsync(Guid id);
        Task<PaginateDTO<QuizDTO>> GetAllQuizAsync(PaginationDTO pagination,PublishStatus publishStatus = PublishStatus.None, Status status = Status.Nope, DifficultLevel difficult = DifficultLevel.None, string search = null, string sort = null);
        Task<QuizDTO> GetQuizAsync(Guid id);
        Task<List<QuestionDTO>> AnonymousDoQuizAsync(Guid id);
        Task<List<QuestionDTO>> DoQuizAsync(Guid id, int accountId);
        Task<bool> DoneQuizAsync(Guid id, int accountId);
        Task<bool> UpdateQuizAsync(Guid id, QuizDTO quizUpdateDto);
        Task<bool> VerifyQuizAsync(Guid quizId, Status status);
        Task<Quiz> AdminCreateQuizAsync(QuizCreateDTO quizCreateDTO);
        Task<bool> CreateQuizQuestionAsync(Guid id, QuestionCreateDTO questionCreate);
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