using System;

namespace Application.DTOs.Word
{
    public class WordLearntDTO
    {
        public Guid WordId { get; set; }
        public string Eng { get; set; }
        public string WordCategory { get; set; }
        public int PracticeCount { get; set; }
        public DateTime LastPractice { get; set; }
    }
}