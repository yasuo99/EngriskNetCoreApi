namespace Application.DTOs.Account
{
    public class AccountLoggedInDTO
    {
        public AccountDetailDTO Account { get; set; }
        public string Token { get; set; }
    }
}