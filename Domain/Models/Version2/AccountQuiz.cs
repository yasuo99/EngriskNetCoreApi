using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.Version2
{
    public class AccountQuiz
    {
        public int AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public virtual Account Account { get; set; }
        public Guid QuizId { get; set; }
        [ForeignKey(nameof(QuizId))]
        public virtual Quiz Quiz { get; set; }
    }
}