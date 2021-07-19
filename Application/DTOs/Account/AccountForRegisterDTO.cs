using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Account
{
    public class AccountForRegisterDTO
    {
        [Required, MinLength(8), MaxLength(11)]
        [RegularExpression(@"^[0-9a-zA-Z''-'\s]{1,40}$", ErrorMessage = "special characters are not allowed.")]
        public string Username { get; set; }
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
        public string Fullname { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Address { get; set; }
        [Phone, MaxLength(10), MinLength(10)]
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        [Required]
        public IEnumerable<string> Roles { get; set; }
    }
}