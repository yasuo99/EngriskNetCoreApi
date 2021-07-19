using System;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.BaseModel.Generic;

namespace Domain.Models.Version2
{
    public class TournamentHistory: AuditEntity<Guid>
    {
        public int AccountId  { get; set; }
        public int TotalPoint { get; set; }
        public Guid TournamentId { get; set; }
        [ForeignKey(nameof(TournamentId))]
        public virtual ExamOnlineSchedule ExamOnlineSchedule { get; set; }
    }
}