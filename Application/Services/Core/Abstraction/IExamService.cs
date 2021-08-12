using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Answer;
using Application.DTOs.Exam;
using Application.DTOs.Pagination;
using Application.DTOs.Question;
using Domain.Enums;
using Domain.Models;

namespace Application.Services.Core.Abstraction
{
    public interface IExamService : ICensorService, IPublishService
    {
        Task<PaginateDTO<ExamDTO>> GetExams(PaginationDTO pagination, ExamPurposes purpose = ExamPurposes.None, DifficultLevel difficult = DifficultLevel.None, string search = null, string sort = null);
        Task<List<ExamDTO>> GetExams(ExamPurposes purposes = ExamPurposes.None, string search = null, bool questionSort = false, bool durationSort = false);
        Task<ExamDTO> GetExamAsync(Guid id);
        Task<bool> CheckExist(Guid id);
        Task<bool> CheckConditionAsync(int accountId, Guid examId);
        Task<PaginateDTO<ExamDTO>> GetUserExamAsync(int accountId, PaginationDTO pagination, string search = null);
        Task<Exam> CreateUserExamAsync(int accountId, ExamCreateDTO examCreateDTO);
        Task<Exam> CreateExamAsync(ExamDTO examCreateDTO);
        Task<bool> UpdateExamAsync(Guid id, ExamDTO examCreateDTO);
        Task<bool> DeleteExamAsync(Guid id);
        Task<bool> CreateExamQuestionAsync(Guid id, QuestionCreateDTO questionCreate);
        Task<ExamDTO> DoExamAsync(int accountId, Guid id);
        Task<ExamResultDTO> DoneExam(int accountId, Guid id, List<AnswerDTO> answers);
        Task<ExamResultDTO> SubmitExamAsync(Guid id, HashSet<AnswerDTO> answers);
        Task<bool> ShareExamAsync(int accountId, Guid examId, List<int> users);
        Task<List<ExamDTO>> GetSharedExamAsync(int accountId);
        Task<ExamAnalyzeDTO> GetExamAnalyzeAsync(Guid examId);
        Task<bool> GenerateQuestionAsync();
        Task<bool> GenerateHistoryAsync(int accountId);
    }
}