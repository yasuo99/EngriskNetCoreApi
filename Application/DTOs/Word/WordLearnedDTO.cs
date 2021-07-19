namespace Application.DTOs.Word
{
    public class WordLearnedDTO
    {
        public int AccountId { get; set; }
        public string AccountUsername { get; set; }
        public string AccountFullname { get; set; }
        public string AccountPhotourl { get; set; }
        public int Learned { get; set; }
    }
}