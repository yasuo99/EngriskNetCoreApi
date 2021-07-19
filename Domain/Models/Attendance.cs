using System.Collections.Generic;

namespace Domain.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int Value {get;set;}
        public virtual ICollection<AccountAttendance> Accounts { get; set; }
    }
}