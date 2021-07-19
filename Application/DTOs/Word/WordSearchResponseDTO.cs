using System.Collections.Generic;

namespace Application.DTOs.Word
{
    public class WordSearchResponseDTO
    {
        public WordSearchResponseDTO()
        {
            Meanings = new List<Meaning>();
            Phonetics = new List<Phonetic>();
        }
        public string Word { get; set; }
        public List<Meaning> Meanings { get; set; }
        public List<Phonetic> Phonetics { get; set; }

    }

    public class Meaning
    {
        public Meaning()
        {
            Definitions = new List<Define>();
        }
        public string PartOfSpeech { get; set; }
        public List<Define> Definitions { get; set; }
    }

    public class Define
    {
        public Define()
        {
            Synonyms = new List<string>();
        }
        public string Definition { get; set; }
        public string Example { get; set; }
        public List<string> Synonyms { get; set; }
    }

    public class Phonetic
    {
        public string Text { get; set; }
        public string Audio { get; set; }
    }
}