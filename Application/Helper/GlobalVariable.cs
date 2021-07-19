using System.Collections.Generic;
using System.Threading;
using Application.DTOs.ExamSchedule;

namespace Application.Helper
{
    public static class GlobalVariable
    {
        public static List<ExamScheduleDTO> Timers = new List<ExamScheduleDTO>();
    }
}