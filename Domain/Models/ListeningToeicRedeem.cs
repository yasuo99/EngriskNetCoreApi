using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class ListeningToeicRedeem
    {
        public int Id { get; set; }
        public int NumOfSentences { get; set; }
        public int Score { get; set; }
    }
}