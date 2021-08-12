using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;
using Domain.Models.BaseModel.Generic;
using Domain.Models.Version2;
using Newtonsoft.Json;

namespace Domain.Models
{
    public class Question : AuditEntity<Guid>
    {
        public Question()
        {
            Answers = new HashSet<Answer>();
            Status = QuestionStatus.Free;
        }
        public string AudioFileName { get; set; }
        public string ImageFileName { get; set; }
        public GrammarQuestionType? Grammar { get; set; }
        public ToeicPart? Toeic { get; set; }
        public string PreQuestion { get; set; }
        public string Content { get; set; }
        public int Duration { get; set; }
        public int? AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public virtual Account Account { get; set; }
        public virtual ICollection<Answer> Answers { get; set; }
        public int? Score { get; set; }
        public QuestionStatus Status { get; set; }
        public bool IsMultitpleRightAnswer { get; set; } = false;
        public string Explaination { get; set; }
        public QuestionType Type { get; set; }
        [JsonIgnore]
        public virtual ICollection<QuizQuestion> Quizes { get; set; }
        [JsonIgnore]
        public virtual ICollection<ExamQuestion> Exams { get; set; }
        [JsonIgnore]
        public virtual ICollection<WordQuestion> Words { get; set; }
        [JsonIgnore]
        public virtual ICollection<ScriptQuestion> Scripts { get; set; }
        [JsonIgnore]
        public virtual ICollection<AccountAnswer> AccountAnswers { get; set; }
    }
}