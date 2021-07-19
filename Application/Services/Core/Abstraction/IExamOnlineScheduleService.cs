using System.Threading.Tasks;
using Application.DTOs.ExamSchedule;
using Domain.Models.Version2;

namespace Application.Services.Core.Abstraction
{
    public interface IExamOnlineScheduleService
    {
         Task<ExamOnlineSchedule> CreateExamScheduleAsync(CreateExamScheduleDTO createExamSchedule);
    }
}