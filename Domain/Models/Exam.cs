using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;
using Domain.Models.BaseModel.Generic;
using Domain.Models.Version2;

namespace Domain.Models
{
    public class Exam : AuditEntity<Guid>
    {
        public Exam()
        {
            Questions = new HashSet<ExamQuestion>();
            ExamHistories = new HashSet<ExamHistory>();
            Accounts = new HashSet<AccountExam>();
            Shared = new HashSet<AccountShare>();
            VerifiedStatus = Status.Nope;
            PublishStatus = PublishStatus.UnPublished;
            Purpose = ExamPurposes.Test;
        }
        public string Title { get; set; }
        public string Detail { get; set; }
        public DifficultLevel Difficult { get; set; }
        public int TotalListening { get; set; }
        public int TotalReading { get; set; }
        public int TotalScore { get; set; } = 0;
        public int PassScore { get; set; }
        public int Duration { get; set; }
        public ExamPurposes Purpose { get; set; }
        public bool IsPrivate { get; set; } = false;
        public int AccessCount { get; set; }
        public Status VerifiedStatus { get; set; }
        public PublishStatus PublishStatus { get; set; }
        public ExamStartPageType StartPage { get; set; }
        public ExamEndPageType EndPage { get; set; }
        public Guid? ScriptId { get; set; }
        public virtual Script Script { get; set; }
        public virtual ExamOnlineSchedule Schedule { get; set; }
        public virtual ICollection<ExamQuestion> Questions { get; set; }
        public virtual ICollection<ExamHistory> ExamHistories { get; set; }
        public virtual ICollection<AccountExam> Accounts { get; set; }
        public virtual ICollection<AccountShare> Shared { get; set; }
    }
}