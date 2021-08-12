using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models;
using Domain.Models.BaseModel.Generic;
using Newtonsoft.Json;

namespace Domain.Models.Version2
{
    public class Answer: AuditEntity<Guid>
    {
        public Guid QuestionId { get; set; }
        [ForeignKey("QuestionId")]
        [JsonIgnore]
        public virtual Question Question { get; set; }
        public string Content{get;set;}
        public string ImageFileName { get; set; }
        public string AudioFileName { get; set; }
        public bool IsQuestionAnswer { get; set; }
        public virtual ICollection<AccountAnswer> AccountAnswers { get; set; }
    }
}