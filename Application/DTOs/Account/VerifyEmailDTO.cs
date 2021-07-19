using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Account
{
    public class VerifyEmailDTO
    {
        [Required]
        public int OTP { get; set; }
        [Required]
        public string Email { get; set; }
    }
}