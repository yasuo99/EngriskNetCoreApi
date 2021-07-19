using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Account
{
    public class AccountChangePwDTO
    {
        [Required]
        public string CurrentPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}