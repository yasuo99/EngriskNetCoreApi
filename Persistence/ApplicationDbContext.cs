using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Models.BaseModel.Generic;
using Domain.Models.Version2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Persistence
{
    public class ApplicationDbContext : IdentityDbContext<Account, Role, int, IdentityUserClaim<int>,
    AccountRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountOTP> AccountOTP { get; set; }
        public DbSet<Example> Examples { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Word> Word { get; set; }
        public DbSet<WordCategory> WordCategories { get; set; }
        public DbSet<ReportError> ReportErrors { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Quiz> Quiz { get; set; }
        public DbSet<Exam> Exam { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<DailyMission> DailyMissions { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<History> Histories { get; set; }
        public DbSet<ExamHistory> ExamHistories { get; set; }
        public DbSet<TopupHistory> TopupHistories { get; set; }
        public DbSet<WordGroup> WordGroups { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<AccountSection> AccountSections { get; set; }
        public DbSet<ExamQuestion> ExamQuestions { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<AccountBadge> AccountBadges { get; set; }
        public DbSet<AccountMission> AccountMissions { get; set; }
        public DbSet<AccountRole> AccountRoles { get; set; }
        public DbSet<AccountAttendance> AccountAttendances { get; set; }
        public DbSet<StringFilter> StringFilters { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<Footer> Footers { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<ListeningToeicRedeem> ListeningToeicRedeems { get; set; }
        public DbSet<ReadingToeicRedeem> ReadingToeicRedeems { get; set; }
        public DbSet<WordLearnt> WordLearnts { get; set; }
        public DbSet<AccountNotification> AccountNotifications { get; set; }
        //V2 Model
        public DbSet<Follow> Follows { get; set; }
        public DbSet<WordContributor> WordContributors { get; set; }
        public DbSet<ExampleContributor> ExampleContributors { get; set; }
        public DbSet<Memory> Memories { get; set; }
        public DbSet<PostImage> PostImages { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<ExamOnlineSchedule> ExamOnlineSchedules { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<AccountExam> AccountExams { get; set; }
        public DbSet<AccountCardmem> AccountCardmems { get; set; }
        public DbSet<WordQuestion> WordQuestions { get; set; }
        public DbSet<BoxChat> BoxChats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<BoxChatMember> BoxChatMembers { get; set; }
        public DbSet<AccountQuiz> AccountQuizzes { get; set; }
        public DbSet<Spam> Spams { get; set; }
        public DbSet<TournamentHistory> TournamentHistory { get; set; }
        public DbSet<AccountShare> AccountShares { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<DayStudy> DayStudies { get; set; }
        public DbSet<SectionProgress> SectionProgresses { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<Script> Scripts { get; set; }
        public DbSet<SectionDetailProgress> SectionDetailProgresses { get; set; }
        public DbSet<ScriptQuestion> ScriptQuestions { get; set; }
        public DbSet<ScriptWord> ScriptWords { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<AccountCertificate> AccountCertificates { get; set; }
        public DbSet<CategoryTag> CategoryTags { get; set; }
        public DbSet<WordCategoryTag> WordCategoryTags { get; set; }
        public DbSet<AccountAnswer> AccountAnswers { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<LikedComment>().HasKey(key => new { key.AccountId, key.CommentId });
            builder.Entity<Account>(acc =>
            {
                acc.HasIndex(acc => acc.UserName).IsUnique();
                acc.HasIndex(acc => acc.Email).IsUnique();
                acc.HasIndex(acc => acc.PhoneNumber).IsUnique();

                // acc.HasMany(m => m.AccountBadges).WithOne(o => o.Account).OnDelete(DeleteBehavior.Cascade);
                // acc.HasMany(m => m.Attendences).WithOne(o => o.Account).OnDelete(DeleteBehavior.Cascade);
                acc.HasMany(m => m.Posts).WithOne(o => o.Account).OnDelete(DeleteBehavior.SetNull);
                // acc.HasMany(m => m.LikedPosts).WithOne(o => o.Account).OnDelete(DeleteBehavior.Cascade);


                // acc.HasMany(m => m.Groups).WithOne(o => o.Account).OnDelete(DeleteBehavior.Cascade);

                // acc.HasMany(m => m.Histories).WithOne(o => o.Account).OnDelete(DeleteBehavior.Cascade);
                // acc.HasMany(m => m.Storage).WithOne(o => o.Account).OnDelete(DeleteBehavior.Cascade);
                // acc.HasMany(m => m.Learned).WithOne(o => o.Account).OnDelete(DeleteBehavior.Cascade);
                acc.HasMany(m => m.Cardmems).WithOne(o => o.Account).HasForeignKey(key => key.AccountId).OnDelete(DeleteBehavior.Restrict);

                acc.HasMany(m => m.ReceivedNotification).WithOne(o => o.Account).OnDelete(DeleteBehavior.Cascade);
                acc.HasMany(m => m.CreatedNotification).WithOne(o => o.From).OnDelete(DeleteBehavior.Cascade);
                acc.HasMany(m => m.LikedComments).WithOne(o => o.Account).OnDelete(DeleteBehavior.Cascade);
                acc.HasMany(m => m.BoxChats).WithOne(o => o.Account).OnDelete(DeleteBehavior.Restrict);
                acc.HasMany(m => m.DayStudies).WithOne(o => o.Account).OnDelete(DeleteBehavior.Cascade);
                acc.HasMany(m => m.Routes).WithOne(o => o.Account).OnDelete(DeleteBehavior.Cascade);
                acc.HasMany(m => m.SectionProgresses).WithOne(o => o.Account).OnDelete(DeleteBehavior.Cascade);
                acc.HasMany(m => m.Certificates).WithOne(o => o.Account).OnDelete(DeleteBehavior.Cascade);
                acc.HasMany(m => m.Questions).WithOne(o => o.Account).OnDelete(DeleteBehavior.Cascade);
            });
            //Account Role

            builder.Entity<AccountRole>(role =>
            {
                role.HasKey(key => new { key.UserId, key.RoleId });
                role.HasOne(o => o.Account)
                    .WithMany(m => m.Roles)
                    .HasForeignKey(key => key.UserId)
                    .IsRequired();
                role.HasOne(o => o.Role)
                    .WithMany(m => m.Accounts)
                    .HasForeignKey(key => key.RoleId)
                    .IsRequired();
            });
            //AccountBadge Entity
            builder.Entity<AccountBadge>().HasKey(key => new { key.AccountId, key.BadgeId });
            builder.Entity<AccountBadge>().HasOne(o => o.Account)
                                        .WithMany(m => m.AccountBadges)
                                        .HasForeignKey(key => key.AccountId)
                                        .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<AccountBadge>().HasOne(o => o.Badge)
                                        .WithMany(m => m.Accounts)
                                        .HasForeignKey(key => key.BadgeId)
                                        .OnDelete(DeleteBehavior.Restrict);

            //QuizQuestion Entity
            builder.Entity<QuizQuestion>().HasKey(key => new { key.QuizId, key.QuestionId });
            builder.Entity<QuizQuestion>().HasOne(o => o.Question)
                                        .WithMany(m => m.Quizes)
                                        .HasForeignKey(key => key.QuestionId)
                                        .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<QuizQuestion>().HasOne(o => o.Quiz)
                                        .WithMany(m => m.Questions)
                                        .HasForeignKey(key => key.QuizId)
                                        .OnDelete(DeleteBehavior.Restrict);
            //Quiz entity
            builder.Entity<Quiz>(q =>
            {
                q.Property(prop => prop.VerifiedStatus).HasConversion<string>();
                q.Property(prop => prop.PublishStatus).HasConversion<string>();
                q.HasMany(m => m.Questions).WithOne(o => o.Quiz).OnDelete(DeleteBehavior.Cascade);
                q.Property(q => q.DifficultLevel).HasConversion<string>();
            });

            //Section entity
            builder.Entity<Section>(section =>
            {
                section.Property(prop => prop.PublishStatus).HasConversion<string>();
                section.HasMany(m => m.WordCategories).WithOne(o => o.Section).OnDelete(DeleteBehavior.SetNull);
                section.HasMany(m => m.SectionProgresses).WithOne(o => o.Section).OnDelete(DeleteBehavior.Cascade);
                section.HasMany(m => m.Scripts).WithOne(o => o.Section).OnDelete(DeleteBehavior.Cascade);
                section.HasOne(o => o.Route).WithMany(m => m.Sections).OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<ScriptQuestion>(sq =>
            {
                sq.HasKey(key => new { key.ScriptId, key.QuestionId });
                sq.HasOne(o => o.Question).WithMany(m => m.Scripts).OnDelete(DeleteBehavior.Cascade);
                sq.HasOne(o => o.Script).WithMany(m => m.Questions).OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<ScriptWord>(sw =>
            {
                sw.HasKey(key => new { key.ScriptId, key.WordId });
                sw.HasOne(o => o.Script).WithMany(m => m.Words).OnDelete(DeleteBehavior.Cascade);
                sw.HasOne(o => o.Word).WithMany(m => m.Scripts).OnDelete(DeleteBehavior.Cascade);
            });
            //ExamQuestion Entity
            builder.Entity<ExamQuestion>(exam =>
            {
                exam.HasKey(key => new { key.ExamId, key.QuestionId });
                exam.HasOne(o => o.Exam).WithMany(m => m.Questions).OnDelete(DeleteBehavior.Restrict);
                exam.HasOne(o => o.Question).WithMany(m => m.Exams).OnDelete(DeleteBehavior.Restrict);
            });
            //Exam Entity
            builder.Entity<Exam>(e =>
            {
                e.Property(prop => prop.VerifiedStatus).HasConversion<string>();
                e.Property(prop => prop.Purpose).HasConversion<string>();
                e.Property(prop => prop.PublishStatus).HasConversion<string>();
                e.HasMany(m => m.Questions).WithOne(o => o.Exam).OnDelete(DeleteBehavior.Cascade);
                e.Property(prop => prop.Difficult).HasConversion<string>();
                e.Property(prop => prop.StartPage).HasConversion<string>();
                e.Property(prop => prop.EndPage).HasConversion<string>();
            });
            builder.Entity<ExamHistory>(eh =>
            {   
                eh.HasMany(m => m.AccountAnswers).WithOne(o => o.ExamHistory).OnDelete(DeleteBehavior.SetNull);
            });
            builder.Entity<AccountAnswer>(aa => {
                aa.HasOne(o => o.Question).WithMany(m => m.AccountAnswers).OnDelete(DeleteBehavior.Restrict);
                aa.HasOne(o => o.Answer).WithMany(m => m.AccountAnswers).OnDelete(DeleteBehavior.Cascade);
            });
            //AccountMission Entity
            builder.Entity<AccountMission>().HasKey(key => new { key.AccountId, key.DailyMissionId });
            builder.Entity<AccountMission>().HasOne(o => o.Account)
                                        .WithMany(m => m.Missions)
                                        .HasForeignKey(key => key.AccountId)
                                        .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<AccountMission>().HasOne(o => o.DailyMission)
                                        .WithMany(m => m.Acccounts)
                                        .HasForeignKey(key => key.DailyMissionId)
                                        .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<WordGroup>().HasKey(key => new { key.GroupId, key.WordId });
            builder.Entity<WordGroup>().HasOne(o => o.Group)
                                        .WithMany(m => m.Words)
                                        .HasForeignKey(key => key.GroupId)
                                        .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<WordGroup>().HasOne(o => o.Word)
                                        .WithMany(m => m.Groups)
                                        .HasForeignKey(key => key.WordId)
                                        .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<AccountStorage>().HasKey(key => new { key.AccountId, key.ItemId });
            builder.Entity<AccountStorage>().HasOne(o => o.Account)
                                        .WithMany(m => m.Storage)
                                        .HasForeignKey(key => key.AccountId)
                                        .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<AccountStorage>().HasOne(o => o.Item)
                                        .WithMany(m => m.Accounts)
                                        .HasForeignKey(key => key.ItemId)
                                        .OnDelete(DeleteBehavior.Restrict);
            //Learning in section
            builder.Entity<AccountSection>(acc =>
            {
                acc.HasKey(key => new { key.AccountId, key.SectionId });
                acc.HasOne(o => o.Account).WithMany(m => m.Sections).OnDelete(DeleteBehavior.Restrict);
                acc.HasOne(o => o.Section).WithMany(m => m.Accounts).OnDelete(DeleteBehavior.Restrict);
            });
            //word
            builder.Entity<Word>(word =>
            {
                word.Property(prop => prop.Type).HasConversion<string>();
                word.Property(prop => prop.Class).HasConversion<string>();
                word.Property(prop => prop.PublishStatus).HasConversion<string>();
                word.Property(prop => prop.Status).HasConversion<string>();
                word.HasMany(m => m.Families).WithOne(o => o.Family).OnDelete(DeleteBehavior.Restrict);
                word.HasMany(m => m.Synonyms).WithOne(o => o.Synonym).OnDelete(DeleteBehavior.Restrict);
                word.HasMany(m => m.Examples).WithOne(o => o.Word).OnDelete(DeleteBehavior.Cascade);
                word.HasMany(m => m.Groups).WithOne(o => o.Word).OnDelete(DeleteBehavior.Cascade);
                word.HasMany(m => m.Learned).WithOne(o => o.Word).OnDelete(DeleteBehavior.Cascade);
                word.HasMany(m => m.Questions).WithOne(o => o.Word).OnDelete(DeleteBehavior.Cascade);
                word.HasMany(m => m.Scripts).WithOne(o => o.Word).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Group>().HasMany(m => m.Words).WithOne(o => o.Group).OnDelete(DeleteBehavior.Cascade);
            //Post
            builder.Entity<Post>(post =>
            {
                post.Property(prop => prop.VerifiedStatus).HasConversion<string>();
                post.HasMany(m => m.Comments).WithOne(o => o.Post).OnDelete(DeleteBehavior.Cascade);
            });
            // builder.Entity<Post>().HasOne(o => o.Account).WithMany(m => m.Posts).OnDelete(DeleteBehavior.Restrict);
            //comment
            builder.Entity<Comment>(comment =>
            {
                comment.Property(prop => prop.VerifiedStatus).HasConversion<string>();
                comment.HasMany(m => m.Replies)
                                    .WithOne(o => o.Reply);
                comment.HasMany(m => m.LikedComments).WithOne(o => o.Comment).OnDelete(DeleteBehavior.Restrict);
            });
            // builder.Entity<Comment>().HasMany(m => m.LikedComments).WithOne(o => o.Comment).OnDelete(DeleteBehavior.Cascade);


            // builder.Entity<Post>().HasMany(m => m.LikedPosts).WithOne(o => o.Post).OnDelete(DeleteBehavior.Cascade);

            builder.Entity<LikedPost>().HasKey(key => new { key.AccountId, key.PostId });
            builder.Entity<LikedPost>().HasOne(o => o.Account)
                                    .WithMany(m => m.LikedPosts)
                                    .HasForeignKey(key => key.AccountId)
                                    .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<LikedPost>().HasOne(o => o.Post)
                                    .WithMany(m => m.LikedPosts)
                                    .HasForeignKey(key => key.PostId).OnDelete(DeleteBehavior.Cascade);


            //Ví dụ
            builder.Entity<Example>(example =>
            {
                example.Property(prop => prop.VerifiedStatus).HasConversion<string>();
            });

            builder.Entity<WordLearnt>(w =>
            {
                w.HasOne(o => o.Account).WithMany(m => m.Learned).OnDelete(DeleteBehavior.Cascade);
                w.HasOne(o => o.Word).WithMany(m => m.Learned).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Question>(q =>
            {
                q.Property(prop => prop.Toeic).HasConversion<string>();
                q.Property(prop => prop.Grammar).HasConversion<string>();
                q.Property(prop => prop.Type).HasConversion<string>();
                q.Property(prop => prop.Status).HasConversion<string>();
                q.HasMany(m => m.Quizes).WithOne(o => o.Question).OnDelete(DeleteBehavior.Cascade);
                q.HasMany(m => m.Exams).WithOne(o => o.Question).OnDelete(DeleteBehavior.Cascade);
                q.HasOne(o => o.Account).WithMany(m => m.Questions).OnDelete(DeleteBehavior.Restrict);
                q.HasMany(m => m.Words).WithOne(o => o.Question).OnDelete(DeleteBehavior.Cascade);
            });

            //Exam tài khoản tự tạo
            builder.Entity<AccountExam>(e =>
            {
                e.HasKey(k => new { k.AccountId, k.ExamId });
                e.HasOne(o => o.Account).WithMany(m => m.Exams);
                e.HasOne(o => o.Exam).WithMany(m => m.Accounts);
            });

            //Memory
            builder.Entity<AccountCardmem>(a =>
            {
                a.HasKey(k => new { k.AccountId, k.MemoryId, k.WordId });
                a.HasOne(o => o.Account).WithMany(m => m.Cardmems).HasForeignKey(key => key.AccountId).OnDelete(DeleteBehavior.Restrict);
                a.HasOne(o => o.Memory).WithMany(m => m.Accounts).OnDelete(DeleteBehavior.Restrict);
                a.HasOne(o => o.Word).WithMany(m => m.CardMems).OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<Memory>(mem =>
            {
                mem.Property(prop => prop.VerifiedStatus).HasConversion<string>();
            });
            //Thông báo của tài khoản
            builder.Entity<AccountNotification>(a =>
            {
                a.Property(prop => prop.Status).HasConversion<string>();
                a.HasKey(key => new { key.AccountId, key.NotificationId });
                a.HasOne(o => o.Account).WithMany(m => m.ReceivedNotification).OnDelete(DeleteBehavior.Restrict);
            });

            //Theo dõi
            builder.Entity<Follow>(f =>
            {
                f.HasKey(key => new
                {
                    key.FollowerId,
                    key.FollowingId
                });
                f.HasIndex(index => new { index.FollowerId, index.FollowingId }).IsUnique();
                f.HasOne(o => o.Follower).WithMany(m => m.Following).OnDelete(DeleteBehavior.Restrict);
                f.HasOne(o => o.Following).WithMany(m => m.Followers).OnDelete(DeleteBehavior.Restrict);
            });

            //Đóng góp
            builder.Entity<ExampleContributor>(ec =>
            {
                ec.HasKey(key => new { key.AccountId, key.ExampleId });
            });
            builder.Entity<WordContributor>(wc =>
            {
                wc.HasKey(key => new { key.AccountId, key.WordId });
                wc.HasOne(o => o.Approver).WithMany(m => m.WordContributorsApproved).HasForeignKey(fk => fk.ApproverId);
            });
            builder.Entity<BoxChatMember>(bc =>
            {
                bc.Property(prop => prop.Status).HasConversion<string>();
                bc.HasKey(key => new { key.AccountId, key.BoxChatId });
                bc.HasOne(o => o.Account).WithMany(m => m.BoxChatMembers).OnDelete(DeleteBehavior.Restrict);
                bc.HasOne(o => o.BoxChat).WithMany(m => m.Members).OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<ExamOnlineSchedule>(eo =>
            {
                eo.HasOne(o => o.Exam).WithOne(o => o.Schedule).OnDelete(DeleteBehavior.Cascade);
                eo.Property(prop => prop.Difficult).HasConversion<string>();
            });
            builder.Entity<WordQuestion>(wq =>
            {
                wq.HasKey(key => new { key.WordId, key.QuestionId });
                wq.HasOne(o => o.Question).WithMany(m => m.Words).OnDelete(DeleteBehavior.Cascade);
                wq.HasOne(o => o.Word).WithMany(m => m.Questions).OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<BoxChat>(bc =>
            {
                bc.HasMany(o => o.Members).WithOne(m => m.BoxChat).OnDelete(DeleteBehavior.Cascade);
                bc.HasMany(m => m.Messages).WithOne(o => o.BoxChat).OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<Notification>(notify =>
            {
                notify.Property(prop => prop.Type).HasConversion<string>();
                notify.HasMany(m => m.Invites).WithOne(o => o.Notification).OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<AccountQuiz>(aq =>
            {
                aq.HasKey(key => new { key.AccountId, key.QuizId });
                aq.HasOne(o => o.Account).WithMany(m => m.Quizzes).OnDelete(DeleteBehavior.Restrict);
                aq.HasOne(o => o.Quiz).WithMany(m => m.Accounts).OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<Spam>(spam =>
            {
                spam.Property(prop => prop.Status).HasConversion<string>();
            });
            builder.Entity<AccountShare>(share =>
            {
                share.HasOne(o => o.Owner).WithMany(m => m.Sharing).OnDelete(DeleteBehavior.Restrict);
                share.HasOne(o => o.Quiz).WithMany(m => m.Shared).OnDelete(DeleteBehavior.Restrict);
                share.HasOne(o => o.Exam).WithMany(m => m.Shared).OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<Category>(category =>
            {
                category.HasKey(key => new { key.WordId, key.WordCategoryId });
                category.HasOne(o => o.Word).WithMany(m => m.Categories).OnDelete(DeleteBehavior.Cascade);
                category.HasOne(o => o.WordCategory).WithMany(m => m.Words).OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<DayStudy>(ds =>
            {
                ds.HasIndex(index => new { index.AccountId, index.Date }).IsUnique();
            });
            builder.Entity<Route>(route =>
            {
                route.Property(prop => prop.PublishStatus).HasConversion<string>();
                route.Property(prop => prop.VerifiedStatus).HasConversion<string>();
                route.HasMany(m => m.Sections).WithOne(o => o.Route).OnDelete(DeleteBehavior.SetNull);
            });
            builder.Entity<SectionDetailProgress>(sd =>
            {
                sd.HasKey(key => new { key.SectionProgressId, key.ScriptId });
                sd.HasOne(o => o.SectionProgress).WithMany(m => m.Details).OnDelete(DeleteBehavior.Cascade);
                sd.HasOne(o => o.Script).WithMany(m => m.Progresses).OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<SectionProgress>(sp =>
            {
                sp.HasIndex(index => new { index.AccountId, index.SectionId }).IsUnique();
                sp.HasMany(m => m.Details).WithOne(o => o.SectionProgress).HasForeignKey(key => key.SectionProgressId).OnDelete(DeleteBehavior.Cascade);
                sp.HasOne(o => o.Section).WithMany(m => m.SectionProgresses).OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<Script>(script =>
            {
                script.Property(prop => prop.Type).HasConversion<string>();
                script.HasOne(o => o.MiniExam).WithOne(o => o.Script).OnDelete(DeleteBehavior.SetNull);
                script.HasMany(m => m.Words).WithOne(o => o.Script).OnDelete(DeleteBehavior.Cascade);
                script.HasMany(m => m.Questions).WithOne(o => o.Script).OnDelete(DeleteBehavior.Cascade);
                script.HasMany(m => m.Progresses).WithOne(o => o.Script).OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<AccountCertificate>(ac =>
            {
                ac.HasOne(o => o.Account).WithMany(m => m.Certificates);
                ac.HasOne(o => o.Certificate).WithMany(m => m.Accounts);
            });
            builder.Entity<WordCategoryTag>(wct =>
            {
                wct.HasKey(key => new { key.WordCategoryId, key.CategoryTagId });
                wct.HasOne(o => o.CategoryTag).WithMany(m => m.Categories).OnDelete(DeleteBehavior.Cascade);
                wct.HasOne(o => o.WordCategory).WithMany(m => m.Tags).OnDelete(DeleteBehavior.Cascade);
            });
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var entries = ChangeTracker.Entries().Where(e => e.Entity is AuditEntity<Guid> && (e.State == EntityState.Added || e.State == EntityState.Modified));
            var httpContextAccessor = this.GetService<IHttpContextAccessor>();
            foreach (var entityEntry in entries)
            {
                if (entityEntry.State == EntityState.Added)
                {
                    ((AuditEntity<Guid>)entityEntry.Entity).CreatedDate = DateTime.Now;
                    ((AuditEntity<Guid>)entityEntry.Entity).CreatedBy = httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "Engrisk";
                }
                else
                {
                    Entry((AuditEntity<Guid>)entityEntry.Entity).Property(p => p.CreatedDate).IsModified = false;
                    Entry((AuditEntity<Guid>)entityEntry.Entity).Property(p => p.CreatedBy).IsModified = false;
                    ((AuditEntity<Guid>)entityEntry.Entity).UpdatedDate = DateTime.Now;
                    ((AuditEntity<Guid>)entityEntry.Entity).UpdatedBy = httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "Engrisk";
                }

            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}