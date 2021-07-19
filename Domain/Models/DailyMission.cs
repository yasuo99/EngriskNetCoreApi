using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class DailyMission
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Instruction { get; set; }
        public int MaxProcess { get; set; }
        public int ExpGain { get; set; }
        public virtual ICollection<AccountMission> Acccounts { get; set; }
    }
}