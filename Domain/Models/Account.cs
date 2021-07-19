using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Domain.Models.Version2;
using Microsoft.AspNetCore.Identity;

namespace Domain.Models
{
    public class Account : IdentityUser<int>
    {
        public Account()
        {
            Followers = new HashSet<Follow>();
            Following = new HashSet<Follow>();
            CreatedNotification = new HashSet<Notification>();
            ReceivedNotification = new HashSet<AccountNotification>();
            WordContributors = new HashSet<WordContributor>();
            ExampleContributors = new HashSet<ExampleContributor>();
            BoxChats = new HashSet<BoxChat>();
            Quizzes = new HashSet<AccountQuiz>();
            Exams = new HashSet<AccountExam>();
            Groups = new HashSet<Group>();
            DayStudies = new HashSet<DayStudy>();
            SectionProgresses = new HashSet<SectionProgress>();
            JoinedDate = DateTime.Now.Date;
            Certificates = new HashSet<AccountCertificate>();
        }
        public string Fullname { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public int Exp { get; set; } = 0;
        public int Point { get; set; } = 0;
        public DateTime Locked { get; set; }
        public bool IsDisabled { get; set; }
        public string PhotoUrl { get; set; }
        public DateTime JoinedDate { get; set; }
        public virtual ICollection<AccountBadge> AccountBadges { get; set; }
        public virtual ICollection<AccountSection> Sections { get; set; }
        public virtual ICollection<Group> Groups { get; set; }
        public virtual List<AccountRole> Roles { get; set; }
        public virtual ICollection<History> Histories { get; set; }
        public virtual ICollection<ExamHistory> ExamHistories { get; set; }
        public virtual ICollection<TopupHistory> TopupHistories { get; set; }
        public virtual ICollection<AccountMission> Missions { get; set; }
        public virtual ICollection<AccountStorage> Storage { get; set; }
        public virtual ICollection<AccountAttendance> Attendences { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<LikedPost> LikedPosts { get; set; }
        public virtual ICollection<LikedComment> LikedComments { get; set; }
        public virtual ICollection<WordLearnt> Learned { get; set; }
        public virtual List<AccountOTP> AccountOTP { get; set; }
        public virtual ICollection<AccountExam> Exams { get; set; }
        public virtual ICollection<Log> Logs { get; set; }
        public virtual ICollection<AccountCardmem> Cardmems { get; set; }
        public virtual ICollection<Memory> Memories { get; set; }
        public virtual ICollection<AccountNotification> ReceivedNotification { get; set; }
        public virtual ICollection<Notification> CreatedNotification { get; set; }
        public virtual ICollection<Follow> Followers { get; set; }
        public virtual ICollection<Follow> Following { get; set; }
        public virtual ICollection<ExampleContributor> ExampleContributors { get; set; }
        public virtual ICollection<WordContributor> WordContributors { get; set; }
        public virtual ICollection<WordContributor> WordContributorsApproved { get; set; }
        public virtual ICollection<BoxChat> BoxChats { get; set; }
        public virtual ICollection<BoxChatMember> BoxChatMembers { get; set; }
        public virtual ICollection<AccountQuiz> Quizzes { get; set; }
        public virtual ICollection<TournamentHistory> TournamentHistories { get; set; }
        public virtual ICollection<AccountShare> Shared { get; set; }
        public virtual ICollection<AccountShare> Sharing { get; set; }
        public virtual ICollection<DayStudy> DayStudies { get; set; }
        public virtual ICollection<SectionProgress> SectionProgresses { get; set; }
        public virtual ICollection<Route> Routes { get; set; }
        public virtual ICollection<AccountCertificate> Certificates { get; set; }
        public virtual ICollection<Question> Questions { get; set; }

        ///<summary>
        ///<para>Danh sách refresh token</para>
        ///</summary>
        [JsonIgnore]
        public virtual List<RefreshToken> RefreshTokens { get; set; }
    }
}