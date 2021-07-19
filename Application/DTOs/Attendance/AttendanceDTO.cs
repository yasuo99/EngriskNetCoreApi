using System;
using Domain.Models;

namespace Application.DTOs.Attendance
{
    public class AttendanceDTO
    {
        public DateTime Date { get; set; }
        public string Type {get;set;}
        public int Value { get; set; }
        public bool IsTookAttendance { get; set; }
    }
}