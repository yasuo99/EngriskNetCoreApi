using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;
using Domain.Models.BaseModel.Generic;
using Domain.Models.Version2;

namespace Domain.Models
{
    public class Section : AuditEntity<Guid>
    {
        public Section()
        {
            SectionProgresses = new HashSet<SectionProgress>();
            Scripts = new HashSet<Script>();
        }
        public string PhotoUrl { get; set; }
        public string SectionName { get; set; }
        public string Description { get; set; }
        public bool RequireLogin { get; set; }
        public int Index { get; set; }
        public Guid? RouteId { get; set; }
        [ForeignKey(nameof(RouteId))]
        public virtual Route Route { get; set; }
        public virtual ICollection<AccountSection> Accounts { get; set; }
        public virtual ICollection<WordCategory> WordCategories { get; set; }
        public virtual ICollection<SectionProgress> SectionProgresses { get; set; }
        public virtual ICollection<Script> Scripts { get; set; }
    }
}