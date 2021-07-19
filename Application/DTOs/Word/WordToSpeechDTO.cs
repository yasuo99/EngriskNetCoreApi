namespace Application.DTOs.Word
{
    public class WordToSpeechDTO
    {
        public string Engine { get; set; }
        public Data Data { get; set; }

    }
    public class Data
    {
        public string Text { get; set; }
        public string Voice { get; set; }
    }
}