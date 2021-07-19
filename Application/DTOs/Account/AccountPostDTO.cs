using System;

namespace Application.DTOs.Account
{
    public class AccountPostDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int UpVote { get; set; }
        public int DownVote { get; set; }
        public DateTime Date { get; set; }
    }
}