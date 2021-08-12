using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;
using Domain.Models.BaseModel.Generic;
using Domain.Models.Version2;

namespace Domain.Models
{
    public class Quiz : AuditEntity<Guid>
    {
        public Quiz()
        {
            Questions = new HashSet<QuizQuestion>();
            Accounts = new HashSet<AccountQuiz>();
            Shared = new HashSet<AccountShare>();
            VerifiedStatus = Status.Nope;
            PublishStatus = PublishStatus.UnPublished;
        }
        public string QuizPhoto { get; set; }
        public string QuizName { get; set; }
        public DifficultLevel DifficultLevel { get; set; }
        public int Enrolled { get; set; }
        public bool RequireLogin { get; set; } = false;
        public bool IsPrivate { get; set; } = false;
        public Status VerifiedStatus { get; set; }
        public PublishStatus PublishStatus { get; set; }
        public int AccessCount { get; set; }
        public virtual ICollection<QuizQuestion> Questions { get; set; }
        public virtual ICollection<AccountQuiz> Accounts { get; set; }
        public virtual ICollection<AccountShare> Shared { get; set; }

    }
}