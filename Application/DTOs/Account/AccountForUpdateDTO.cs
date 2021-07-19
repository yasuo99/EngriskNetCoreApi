using System;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Account
{
    public class AccountForUpdateDTO
    {
        public string Fullname { get; set; }
        public string Address { get; set; }
        public string Phone {get;set;}
        public DateTime DateOfBirth { get; set; }
        public IFormFile File { get; set; }
        public string PhotoUrl { get; set; }
        public string PublicId { get; set; }
    }
}