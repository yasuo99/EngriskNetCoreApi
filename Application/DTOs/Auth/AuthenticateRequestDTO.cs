namespace Application.DTOs.Auth
{
    public class AuthenticateRequestDTO
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}