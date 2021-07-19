using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class AccountMission
    {
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        public int DailyMissionId { get; set; }
        [ForeignKey("DailyMissionId")]
        public virtual DailyMission DailyMission { get; set; }
        public DateTime ActivateDate { get; set; }
        public bool IsDone { get; set; }    
    }
}