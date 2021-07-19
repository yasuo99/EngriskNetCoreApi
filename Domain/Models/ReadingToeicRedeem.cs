using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class ReadingToeicRedeem
    {
        public int Id { get; set; }
        public int NumOfSentences { get; set; }
        public int Score { get; set; }
    }
}