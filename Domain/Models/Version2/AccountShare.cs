using System;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.BaseModel.Generic;

namespace Domain.Models.Version2
{
    public class AccountShare : AuditEntity<Guid>
    {
        public int OwnerId { get; set; }
        [ForeignKey(nameof(OwnerId))]
        public virtual Account Owner { get; set; }
        public Guid? QuizId { get; set; }
        [ForeignKey(nameof(QuizId))]
        public virtual Quiz Quiz { get; set; }
        public Guid? ExamId { get; set; }
        [ForeignKey(nameof(ExamId))]
        public virtual Exam Exam { get; set; }
        public int ShareToId { get; set; }
        [ForeignKey(nameof(ShareToId))]
        public virtual Account ShareTo { get; set; }
    }
}