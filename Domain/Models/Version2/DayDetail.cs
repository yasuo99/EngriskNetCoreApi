using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.Version2
{
    public class DayDetail
    {
        public Guid Id { get; set; }
        public Guid DayId { get; set; }
        [ForeignKey(nameof(DayId))]
        public virtual DayStudy DayStudy { get; set; }
        public Guid SectionId { get; set; }
        [ForeignKey(nameof(SectionId))]
        public virtual Section Section { get; set; }
    }
}