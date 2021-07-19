using System;
using System.Collections.Generic;

namespace Application.DTOs.Account
{
    public class DailyLearningDTO
    {
        public DailyLearningDTO()
        {
            DayStudied = new Dictionary<DateTime, LearningResultDTO>();
        }
        public Dictionary<DateTime, LearningResultDTO> DayStudied { get; set; }

    }
    public class LearningResultDTO
    {
        public bool Status { get; set; }
        public int LearnedVocabulary { get; set; }
        public int SectionDone { get; set; }
        public int VocabularyScript { get; set; }
        public int ListeningScript { get; set; }
        public int ReadingScript { get; set; }
        public int WritingScript { get; set; }
        public int GrammarScript { get; set; }
        public int ConversationScript { get; set; }
    }
}