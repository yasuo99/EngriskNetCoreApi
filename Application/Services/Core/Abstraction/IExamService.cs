using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Answer;
using Application.DTOs.Exam;
using Application.DTOs.Pagination;
using Domain.Enums;
using Domain.Models;

namespace Application.Services.Core.Abstraction
{
    public interface IExamService : ICensorService
    {
        Task<PaginateDTO<ExamDTO>> GetExams(PaginationDTO pagination, Status status = Status.Approved, string search = null);
        Task<List<ExamDTO>> GetExams(string search = null, bool questionSort = false, bool durationSort = false);
        Task<ExamDTO> GetExamAsync(Guid id);
        Task<bool> CheckExist(Guid id);
        Task<bool> CheckConditionAsync(int accountId, Guid examId);
        Task<PaginateDTO<ExamDTO>> GetUserExamAsync(int accountId, PaginationDTO pagination, string search = null);
        Task<Exam> CreateUserExamAsync(int accountId, ExamCreateDTO examCreateDTO);
        Task<Exam> CreateExamAsync(ExamDTO examCreateDTO);
        Task<bool> UpdateExamAsync(Guid id, ExamDTO examCreateDTO);
        Task<bool> DeleteExamAsync(Guid id);
        Task<ExamDTO> DoExamAsync(int accountId, Guid id);
        Task PauseExam(Guid id, int accountId, int currentQuestion);
        Task<int> ResumeExam(Guid id, int accountId);
        Task<ExamResultDTO> DoneExam(int accountId, Guid id, List<AnswerDTO> answers);
        Task<ExamResultDTO> SubmitExamAsync(Guid id, HashSet<AnswerDTO> answers);
        Task<bool> ShareExamAsync(int accountId, Guid examId, List<int> users);
        Task<List<ExamDTO>> GetSharedExamAsync(int accountId);
    }
}