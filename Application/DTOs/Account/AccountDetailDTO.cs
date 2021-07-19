using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Application.DTOs.Account.Boxchat;
using Application.DTOs.Account.Follow;
using Application.DTOs.Exam;
using Application.DTOs.Group;
using Application.DTOs.Quiz;
using Application.DTOs.Word;
using Domain.Models;

namespace Application.DTOs.Account
{
    public class AccountDetailDTO
    {
        public int Id { get; set; }
        public string PhotoUrl { get; set; }
        public string PublicId { get; set; }
        public string Email { get; set; }
        public bool IsVerified { get; set; }
        public string Username { get; set; }
        public string Fullname { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string LevelName { get; set; }
        public int Exp { get; set; }
        public int Point { get; set; }
        public bool IsBanned {get;set;}
        public bool  IsDisabled { get; set; }
         public virtual IEnumerable<AccountBadge> AccountBadges { get; set; }
        public virtual IEnumerable<GroupDTO> Groups { get; set; }
        public virtual IEnumerable<string> Roles { get; set; }
        public virtual IEnumerable<QuizHistoryDTO> Histories { get; set; }
        public virtual IEnumerable<AccountMission> Missions { get; set; }
        public virtual IEnumerable<AccountStorage> Storage { get; set; }
        public virtual IEnumerable<AccountAttendance> Attendences { get; set; }
        public virtual IEnumerable<AccountPostDTO> Posts { get; set; }
        public virtual IEnumerable<AccountCommentDTO> Comments { get; set; }
        public virtual IEnumerable<PostRatingDTO> PostRatings { get; set; }
        public virtual IEnumerable<WordLearntDTO> Learned{ get; set; }
        public virtual IEnumerable<ExamHistoryDTO> ExamHistories { get; set; }
        public virtual IEnumerable<FollowingDTO> Following { get; set; }
        public virtual IEnumerable<FollowerDTO> Followers { get; set; }
        public virtual ICollection<BoxchatDTO> BoxChats { get; set; }
        [JsonIgnore]
        public string RefreshToken { get; set; }
    }
}