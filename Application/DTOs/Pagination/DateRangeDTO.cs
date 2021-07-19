using System;

namespace Application.DTOs.Pagination
{
    public class DateRangeDTO
    {
        public DateTime Start { get; set; } = DateTime.Now.AddDays(-6).Date;
        public DateTime End { get; set; } = DateTime.Now.Date;
    }
}