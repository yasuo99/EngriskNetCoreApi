using System.Collections.Generic;

namespace Application.DTOs.Account
{
    public class AccountBlogDTO
    {
        public int Id { get; set; }
        public string PhotoUrl { get; set; }
        public string Email { get; set; }
        public bool IsVerified { get; set; }
        public string Username { get; set; }
        public string Fullname { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsBanned {get;set;}
        public bool  IsDisabled { get; set; }
        public int WordLearned{ get; set; }
        public int ExamDone { get; set; }
        public int QuizDone { get; set; }
    }
}