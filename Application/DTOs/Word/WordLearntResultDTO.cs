using System.Collections.Generic;

namespace Application.DTOs.Word
{
    public class WordLearntResultDTO
    {
        public int WeakLearnedWordsCount { get; set; }
        public List<VocabularyLearnedDTO> WeakLearnedWords { get; set; }
        public int MediumLearnedWordsCount { get; set; }
        public List<VocabularyLearnedDTO> MediumLearnedWords { get; set; }
        public int StrongLearnedWordsCount { get; set; }
        public List<VocabularyLearnedDTO> StrongLearnedWords { get; set; }
    }
}