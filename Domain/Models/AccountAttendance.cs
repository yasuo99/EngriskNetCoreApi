using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class AccountAttendance
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        public int AttendanceId { get; set; }
        [ForeignKey("AttendanceId")]
        public virtual Attendance Attendance { get; set; }
        public DateTime Date { get; set; }
    }
}