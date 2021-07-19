namespace Application.DTOs.Account.Boxchat
{
    public class BoxchatCreateDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ChatKey { get; set; }
        public int AccountId { get; set; }
    }
}