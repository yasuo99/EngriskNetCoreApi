using System.Collections.Generic;
using Application.DTOs.Question;
using Application.DTOs.Word;

namespace Application.DTOs.Vocabulary
{
    public class VocabularyLearnDTO
    {
        public VocabularyLearnDTO()
        {
            Words = new List<WordDTO>();
            Questions = new List<QuestionDTO>();
        }
        public List<WordDTO> Words { get; set; }
        public List<QuestionDTO> Questions { get; set; }
    }
}