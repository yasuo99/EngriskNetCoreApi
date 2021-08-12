using System;
using System.Linq;
using Application.DTOs.Exam;
using Application.DTOs.Question;
using Application.DTOs.Word;
using AutoMapper;
using Application.DTOs.Account;
using Application.DTOs.Attendance;
using Application.DTOs.Comment;
using Application.DTOs.Example;
using Application.DTOs.Group;
using Application.DTOs.Notification;
using Application.DTOs.Post;
using Application.DTOs.Quiz;
using Application.DTOs.Section;
using Domain.Models;
using Application.DTOs.Memory;
using Domain.Models.Version2;
using Application.DTOs.ExamSchedule;
using Application.DTOs.Ticket.VerifyTicket;
using Application.DTOs.Answer;
using Application.DTOs.Word.WordCategory;
using Application.DTOs.Account.Follow;
using Domain.Enums;
using Application.DTOs.Account.Sharing;
using Application.DTOs.Account.Boxchat;
using Application.DTOs.Hub;
using Application.DTOs.Account.Route;
using Application.DTOs.Admin;
using Application.DTOs.Certificate;

namespace Application.Helper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Account, AccountDetailDTO>()
            .ForMember(account => account.Age, source => source.MapFrom(src => src.DateOfBirth.CalculateAge()))
            .ForMember(account => account.Roles, options => options.MapFrom(src => src.Roles.Select(r => r.Role.Name)))
            .ForMember(acc => acc.RefreshToken, opts => opts.MapFrom(src => src.RefreshTokens.Count() > 0 ? src.RefreshTokens.Last().Token : ""))
            .ForMember(acc => acc.IsBanned, opts => opts.MapFrom(src => src.Locked > DateTime.Now))
            .ForMember(acc => acc.IsVerified, opts => opts.MapFrom(sourceMember => sourceMember.EmailConfirmed));
            CreateMap<AccountForRegisterDTO, Account>();
            CreateMap<Account, AccountBlogDTO>()
            .ForMember(account => account.Age, source => source.MapFrom(src => src.DateOfBirth.CalculateAge()))
            .ForMember(acc => acc.IsBanned, opts => opts.MapFrom(src => src.Locked > DateTime.Now))
            .ForMember(acc => acc.WordLearned, opts => opts.MapFrom(src => src.Learned.Count()))
            .ForMember(acc => acc.ExamDone, opts => opts.MapFrom(src => src.ExamHistories.Count()))
            .ForMember(acc => acc.QuizDone, opts => opts.MapFrom(src => src.Histories.Select(s => s.Quiz).Distinct().ToHashSet().Count()));
            CreateMap<AccountDetailDTO, Account>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<AccountForUpdateDTO, Account>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<AccountAttendance, AttendanceDTO>()
            .ForMember(gift => gift.Type, options => options.MapFrom(src => src.Attendance.Type))
            .ForMember(gift => gift.Value, options => options.MapFrom(src => src.Attendance.Value));
            CreateMap<Question, QuestionDTO>()
            .ForMember(mem => mem.Audio, opts => opts.MapFrom(src => src.AudioFileName))
            .ForMember(mem => mem.PhotoUrl, opts => opts.MapFrom(src => src.ImageFileName));
            CreateMap<Question, QuestionDetailDTO>();
            CreateMap<QuestionDTO, Question>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<QuestionCreateDTO, Question>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Exam, ExamDTO>()
            .ForMember(exam => exam.IsNew, opts => opts.MapFrom(src => DateTime.Now.Subtract(src.CreatedDate).Days < 3 ? true : false))
            .ForMember(exam => exam.Questions, opts => opts.MapFrom(src => src.Questions.Select(sl => sl.Question)));
            CreateMap<ExamDTO, Exam>();
            CreateMap<ExamCreateDTO, Exam>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Quiz, QuizDTO>();
            CreateMap<QuizDTO, Quiz>();
            CreateMap<QuizQuestion, QuestionDTO>()
            .ForMember(question => question.Id, options => options.MapFrom(src => src.Question.Id))
            .ForMember(question => question.Audio, opts => opts.MapFrom(src => src.Question.AudioFileName))
            .ForMember(question => question.Content, options => options.MapFrom(src => src.Question.Content))
            .ForMember(question => question.Type, options => options.MapFrom(src => src.Question.Type))
            .ForMember(question => question.PreQuestion, opts => opts.MapFrom(src => src.Question.PreQuestion))
            .ForMember(question => question.PhotoUrl, opts => opts.MapFrom(src => src.Question.ImageFileName))
            .ForMember(question => question.Answers, opts => opts.MapFrom(src => src.Question.Answers));
            CreateMap<QuestionDTO, QuizQuestion>()
            .ForMember(mem => mem.QuestionId, opts => opts.MapFrom(src => src.Id));
            CreateMap<Post, PostDTO>()
            .ForMember(post => post.TotalComment, opts => opts.MapFrom(src => src.Comments.Count()));
            CreateMap<Account, AdminAccountDTO>()
            .ForMember(account => account.Roles, options => options.MapFrom(src => src.Roles.Select(r => r.Role.Name)));
            CreateMap<Comment, AccountCommentDTO>()
            .ForMember(cmt => cmt.AccountUsername, opts => opts.MapFrom(src => src.Account.UserName));
            CreateMap<Group, GroupDTO>()
            .ForMember(group => group.AccountUsername, opts => opts.MapFrom(src => src.Account.UserName));
            CreateMap<QuizDTO, Quiz>();
            CreateMap<QuizCreateDTO, Quiz>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Quiz, QuizDetailDTO>().ForMember(q => q.TotalQuestion, opts => opts.MapFrom(src => src.Questions.Count()));
            CreateMap<WordDTO, Word>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<WordGroup, WordDTO>()
            .ForMember(w => w.Id, opts => opts.MapFrom(src => src.WordId))
            .ForMember(w => w.Eng, opts => opts.MapFrom(src => src.Word.Eng))
            .ForMember(w => w.Vie, opts => opts.MapFrom(src => src.Word.Vie))
            .ForMember(w => w.WordImg, opts => opts.MapFrom(src => src.Word.WordImg))
            .ForMember(w => w.Spelling, opts => opts.MapFrom(src => src.Word.Spelling))
            .ForMember(w => w.WordVoice, opts => opts.MapFrom(src => src.Word.WordVoice));
            CreateMap<Word, WordDTO>();
            CreateMap<Word, WordDetailDTO>();
            CreateMap<Example, ExampleDTO>();
            CreateMap<ExampleDTO, Example>();
            // CreateMap<WordExample, ExampleDTO>()
            // .ForMember(example => example.Eng, opts => opts.MapFrom(src => src.Example.Eng))
            // .ForMember(example => example.Vie, opts => opts.MapFrom(src => src.Example.Vie))
            // .ForMember(example => example.Id, opts => opts.MapFrom(src => src.ExampleId))
            // .ForMember(example => example.Inserted, opts => opts.MapFrom(src => src.Example.CreatedDate));
            CreateMap<Section, SectionDTO>();
            CreateMap<SectionDTO, Section>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<SectionCreateDTO, Section>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<SectionUpdateDTO, Section>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ExamQuestion, QuestionDTO>()
            .ForMember(question => question.Id, options => options.MapFrom(src => src.Question.Id))
            .ForMember(question => question.Audio, opts => opts.MapFrom(src => src.Question.AudioFileName))
            .ForMember(question => question.Content, options => options.MapFrom(src => src.Question.Content))
            .ForMember(question => question.Type, options => options.MapFrom(src => src.Question.Type))
            .ForMember(question => question.PhotoUrl, opts => opts.MapFrom(src => src.Question.ImageFileName))
            .ForMember(question => question.Toeic, opts => opts.MapFrom(src => src.Question.Toeic))
            .ForMember(question => question.Answers, opts => opts.MapFrom(src => src.Question.Answers));
            CreateMap<QuestionDTO, ExamQuestion>()
            .ForMember(mem => mem.QuestionId, opts => opts.MapFrom(src => src.Id));
            CreateMap<ExamHistory, ExamHistoryDTO>()
            .ForMember(h => h.ExamTitle, opts => opts.MapFrom(src => src.Exam.Title))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Exam, ExamAnswerDTO>();
            CreateMap<ExamQuestion, QuestionAnswerDTO>()
            .ForMember(question => question.Id, options => options.MapFrom(src => src.Question.Id))
            .ForMember(question => question.Content, options => options.MapFrom(src => src.Question.Content))
            .ForMember(question => question.Answers, opts => opts.MapFrom(src => src.Question.Answers))
            .ForMember(question => question.Answer, opts => opts.MapFrom(src => src.Question.Answers.FirstOrDefault(x => x.IsQuestionAnswer).Content))
            .ForMember(question => question.Explaination, opts => opts.MapFrom(src => src.Question.Explaination))
            .ForMember(question => question.Type, options => options.MapFrom(src => src.Question.Type))
            .ForMember(question => question.AudioFileName, opts => opts.MapFrom(sourceMember => sourceMember.Question.AudioFileName))
            .ForMember(question => question.ImageFileName, opts => opts.MapFrom(sourceMember => sourceMember.Question.ImageFileName))
            .ForMember(question => question.ToeicPart, opts => opts.MapFrom(src => src.Question.Toeic))
            .ReverseMap();
            CreateMap<WordCreateDTO, Word>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<WordUpdateDTO, Word>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<NotificationCreateDTO, Notification>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Post, PostDetailDTO>()
            .ForMember(post => post.AccountVerified, opts => opts.MapFrom(src => src.Account.EmailConfirmed))
            .ForMember(post => post.AccountPhotoUrl, opts => opts.MapFrom(src => src.Account.PhotoUrl))
            .ForMember(post => post.AccountUsername, opts => opts.MapFrom(src => src.Account.UserName));
            CreateMap<Comment, CommentDTO>().ForMember(c => c.AccountUsername, opts => opts.MapFrom(src => src.Account.UserName))
            .ForMember(c => c.Comment, opts => opts.MapFrom(src => src.Content))
            .ForMember(c => c.AccountPhotoUrl, opts => opts.MapFrom(src => src.Account.PhotoUrl))
            .ForMember(c => c.IsVerified, opts => opts.MapFrom(src => src.Account.EmailConfirmed));
            CreateMap<WordLearnt, WordLearntDTO>()
            .ForMember(w => w.Eng, opts => opts.MapFrom(src => src.Word.Eng));
            CreateMap<History, QuizHistoryDTO>().ForMember(h => h.QuizName, opts => opts.MapFrom(src => src.Quiz.QuizName));
            CreateMap<PostUpdateDTO, Post>().ForAllMembers(all => all.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ExamHistory, ExamProgressDTO>();
            CreateMap<Question, QuestionAnswerDTO>()
            .ForMember(mem => mem.Type, opts => opts.MapFrom(src => src.Type));

            //Version 2
            CreateMap<MemoryCreateDTO, Memory>();
            CreateMap<CreateExamScheduleDTO, ExamOnlineSchedule>();
            CreateMap<CreateAnswerDTO, Answer>().ForAllMembers(mem => mem.Condition((src, dest, srcMem) => srcMem != null));
            CreateMap<AdminNotificationCreateDTO, Notification>().ForAllMembers(mem => mem.Condition((src, dest, prop) => prop != null));
            CreateMap<Notification, ResponseNotificationDTO>()
            .ForMember(notification => notification.CreatedBy, opts => opts.MapFrom(src => src.From.UserName));
            CreateMap<WordCategory, WordCategoryDTO>()
            .ForAllMembers(mem => mem.Condition((src, dest, mem) => mem != null));
            CreateMap<Answer, AnswerDTO>()
            .ForMember(mem => mem.Answer, opts => opts.MapFrom(src => src.Content))
            .ForAllMembers(mem => mem.Condition((src, dest, prop) => prop != null));
            CreateMap<Account, AccountReceiveNotificationDTO>();
            CreateMap<PostCreateDTO, Post>().ForAllMembers(mem => mem.Condition((src, dest, prop) => prop != null));
            CreateMap<Follow, FollowingDTO>()
            .ForMember(mem => mem.AccountId, opts => opts.MapFrom(src => src.Following.Id))
            .ForMember(mem => mem.UserName, opts => opts.MapFrom(src => src.Following.UserName))
            .ForMember(mem => mem.PhotoUrl, opts => opts.MapFrom(src => src.Following.PhotoUrl))
            .ForAllMembers(mem => mem.Condition((src, dest, prop) => prop != null));
            CreateMap<Follow, FollowerDTO>()
           .ForMember(mem => mem.AccountId, opts => opts.MapFrom(src => src.Follower.Id))
           .ForMember(mem => mem.UserName, opts => opts.MapFrom(src => src.Follower.UserName))
           .ForMember(mem => mem.PhotoUrl, opts => opts.MapFrom(src => src.Follower.PhotoUrl))
           .ForAllMembers(mem => mem.Condition((src, dest, prop) => prop != null));
            CreateMap<AccountNotification, AccountNotificationDTO>()
            .ForMember(mem => mem.Status, opts => opts.MapFrom(src => Enum.GetName(typeof(NotificationStatus), src.Status)))
            .ForMember(mem => mem.Content, opts => opts.MapFrom(src => src.Notification.Content))
            .ForMember(mem => mem.Url, opts => opts.MapFrom(src => src.Notification.Url))
            .ForMember(mem => mem.CreatedDate, opts => opts.MapFrom(src => src.Notification.CreatedDate))
            .ForMember(mem => mem.CreatedBy, opts => opts.MapFrom(src => src.Notification.CreatedBy))
            .ForMember(mem => mem.Id, opts => opts.MapFrom(src => src.NotificationId))
            .ForMember(mem => mem.Type, opts => opts.MapFrom(src => src.Notification.Type));
            CreateMap<Word, VocabularyLearnedDTO>()
            .ForAllMembers(mem => mem.Condition((src, dest, mem) => mem != null));
            CreateMap<AccountShare, AccountSharingDTO>()
            .ForMember(mem => mem.QuizTitle, opts => opts.MapFrom(sourceMember => sourceMember.Quiz.QuizName))
            .ForMember(mem => mem.ExamTitle, opts => opts.MapFrom(src => src.Exam.Title));
            CreateMap<AccountShare, QuizSharingDTO>();
            CreateMap<BoxChat, BoxchatDTO>()
            .ForMember(mem => mem.PhotoUrl, opts => opts.MapFrom(src => src.Account.PhotoUrl));
            CreateMap<BoxchatUpdateDTO, BoxChat>()
            .ForAllMembers(mems => mems.Condition((src, dest, mem) => mem != null));
            CreateMap<Message, ReceiveMessageDTO>();
            CreateMap<BoxChatMember, AccountBlogDTO>()
            .ForMember(mem => mem.Id, opts => opts.MapFrom(src => src.AccountId))
            .ForMember(mem => mem.Username, opts => opts.MapFrom(src => src.Account.UserName))
            .ForMember(mem => mem.PhotoUrl, opts => opts.MapFrom(src => src.Account.PhotoUrl))
            .ForAllMembers(mems => mems.Condition((src, dest, mem) => mem != null));
            CreateMap<Message, MessageResponseDTO>()
            .ForMember(mem => mem.Text, opts => opts.MapFrom(src => src.Content))
            .ForPath(mem => mem.Sender.Name, opts => opts.MapFrom(src => src.Account.UserName))
            .ForPath(mem => mem.Sender.Uid, opts => opts.MapFrom(src => src.Account.UserName))
            .ForPath(mem => mem.Sender.Avatar, opts => opts.MapFrom(src => src.Account.PhotoUrl))
            .ForAllMembers(mems => mems.Condition((src, dest, mem) => mem != null));
            CreateMap<BoxchatCreateDTO, BoxChat>()
            .ForAllMembers(mem => mem.Condition((src, dest, mem) => mem != null));
            CreateMap<WordCategoryCreateDTO, WordCategory>();
            CreateMap<Section, SectionLearnDTO>();
            CreateMap<SectionProgress, SectionDTO>()
            .ForMember(mem => mem.IsDone, opts => opts.MapFrom(sourceMember => sourceMember.IsDone))
            .ForMember(mem => mem.IsCurrentLocked, opts => opts.MapFrom(src => src.IsLock))
            .ForMember(mem => mem.Id, opts => opts.MapFrom(src => src.SectionId))
            .ForMember(mem => mem.PhotoUrl, opts => opts.MapFrom(src => src.Section.PhotoUrl))
            .ForMember(mem => mem.SectionName, opts => opts.MapFrom(src => src.Section.SectionName))
            .ForMember(mem => mem.Description, opts => opts.MapFrom(src => src.Section.Description))
            .ForMember(mem => mem.RequireLogin, opts => opts.MapFrom(src => src.Section.RequireLogin));
            CreateMap<SectionProgress, SectionLearnDTO>()
            .ForMember(mem => mem.IsDone, opts => opts.MapFrom(sourceMember => sourceMember.IsDone))
            .ForMember(mem => mem.IsCurrentLocked, opts => opts.MapFrom(src => src.IsLock))
            .ForMember(mem => mem.Id, opts => opts.MapFrom(src => src.SectionId))
            .ForMember(mem => mem.PhotoUrl, opts => opts.MapFrom(src => src.Section.PhotoUrl))
            .ForMember(mem => mem.SectionName, opts => opts.MapFrom(src => src.Section.SectionName))
            .ForMember(mem => mem.Description, opts => opts.MapFrom(src => src.Section.Description))
            .ForMember(mem => mem.RequireLogin, opts => opts.MapFrom(src => src.Section.RequireLogin));
            CreateMap<BoxChatMember, BoxchatMemberDTO>()
            .ForMember(mem => mem.Id, opts => opts.MapFrom(src => src.AccountId))
            .ForMember(mem => mem.Username, opts => opts.MapFrom(src => src.Account.UserName))
            .ForMember(mem => mem.Status, opts => opts.MapFrom(src => src.Status));
            CreateMap<Route, RouteDTO>()
            .ForAllMembers(mem => mem.Condition((src, dest, mem) => mem != null));
            CreateMap<Route, RouteLearnDTO>()
          .ForAllMembers(mem => mem.Condition((src, dest, mem) => mem != null));
            CreateMap<RouteCreateDTO, Route>()
            .ForAllMembers(mem => mem.Condition((src, dest, mem) => mem != null));
            CreateMap<RouteUpdateDTO, Route>()
            .ForAllMembers(mem => mem.Condition((src, dest, mem) => mem != null));
            CreateMap<ExampleContributor, ExampleDTO>()
            .ForMember(mem => mem.Id, opts => opts.MapFrom(sourceMember => sourceMember.ExampleId))
            .ForMember(mem => mem.Eng, opts => opts.MapFrom(src => src.Example.Eng))
            .ForMember(mem => mem.Vie, opts => opts.MapFrom(src => src.Example.Vie))
            .ForMember(mem => mem.CreatedDate, opts => opts.MapFrom(src => src.Example.CreatedDate))
            .ForMember(mem => mem.Contributor, opts => opts.MapFrom(src => src.Account.UserName));
            CreateMap<Memory, MemoryCensorDTO>()
            .ForAllMembers(mems => mems.Condition((src, dest, mem) => mem != null));
            CreateMap<Post, PostCensorDTO>()
            .ForAllMembers(mems => mems.Condition((src, dest, mem) => mem != null));
            CreateMap<Comment, CommentCensorDTO>();
            CreateMap<Route, RouteCensorDTO>();
            CreateMap<Category, WordCategoryDTO>()
            .ForMember(mem => mem.CategoryName, opts => opts.MapFrom(src => src.WordCategory.CategoryName))
            .ForMember(mem => mem.Id, opts => opts.MapFrom(src => src.WordCategoryId));
            CreateMap<Script, ScriptLearnDTO>();
            CreateMap<ScriptQuestion, QuestionDTO>()
           .ForMember(question => question.Id, options => options.MapFrom(src => src.Question.Id))
           .ForMember(question => question.Audio, opts => opts.MapFrom(src => src.Question.AudioFileName))
           .ForMember(question => question.Content, options => options.MapFrom(src => src.Question.Content))
           .ForMember(question => question.Type, options => options.MapFrom(src => src.Question.Type))
           .ForMember(question => question.PhotoUrl, opts => opts.MapFrom(src => src.Question.ImageFileName))
           .ForMember(question => question.Toeic, opts => opts.MapFrom(src => src.Question.Toeic))
           .ForMember(question => question.Answers, opts => opts.MapFrom(src => src.Question.Answers))
           .ForMember(question => question.PreQuestion, opts => opts.MapFrom(src => src.Question.PreQuestion));
            CreateMap<ScriptWord, WordDTO>()
            .ForMember(w => w.Id, opts => opts.MapFrom(src => src.WordId))
            .ForMember(w => w.Eng, opts => opts.MapFrom(src => src.Word.Eng))
            .ForMember(w => w.Vie, opts => opts.MapFrom(src => src.Word.Vie))
            .ForMember(w => w.WordImg, opts => opts.MapFrom(src => src.Word.WordImg))
            .ForMember(w => w.Spelling, opts => opts.MapFrom(src => src.Word.Spelling))
            .ForMember(w => w.WordVoice, opts => opts.MapFrom(src => src.Word.WordVoice))
            .ForMember(w => w.Examples, opts => opts.MapFrom(src => src.Word.Examples));
            CreateMap<Section, SectionScriptDTO>();
            CreateMap<SectionDetailProgress, ScriptLearnHistoryDTO>()
            .ForMember(mem => mem.Type, opts => opts.MapFrom(src => src.Script.Type))
            .ForMember(mem => mem.ExamId, opts => opts.MapFrom(src => src.Script.MiniExam.Id));
            CreateMap<WordCategoryDTO, Category>()
            .ForMember(mem => mem.WordCategoryId, opts => opts.MapFrom(src => src.Id));
            CreateMap<QuestionDTO, WordQuestion>()
            .ForMember(mem => mem.QuestionId, opts => opts.MapFrom(src => src.Id));
            CreateMap<QuestionDTO, ScriptQuestion>()
            .ForMember(mem => mem.QuestionId, opts => opts.MapFrom(src => src.Id));
            CreateMap<WordDTO, ScriptWord>()
            .ForMember(mem => mem.WordId, opts => opts.MapFrom(src => src.Id));
            CreateMap<ScriptCreateDTO, Script>();
            CreateMap<ExamScriptDTO, Exam>();
            CreateMap<Certificate, CertificateDTO>();
            CreateMap<CertificateCreateDTO, Certificate>()
            .ForAllMembers(mems => mems.Condition((src, dest, mem) => mem != null));
            CreateMap<CategoryTag, WordCategoryTag>()
            .ForMember(mem => mem.CategoryTagId, opts => opts.MapFrom(src => src.Id));
            CreateMap<WordCategoryTag, CategoryTag>()
            .ForMember(mem => mem.Id, opts => opts.MapFrom(src => src.CategoryTagId))
            .ForMember(mem => mem.Tag, opts => opts.MapFrom(src => src.CategoryTag.Tag));
            CreateMap<Answer, AnswerAnalyzeDTO>();
            CreateMap<ExamQuestion, QuestionAnalyzeDTO>()
            .ForMember(mem => mem.Id, opts => opts.MapFrom(src => src.Question.Id))
            .ForMember(mem => mem.PreQuestion, opts => opts.MapFrom(src => src.Question.PreQuestion))
            .ForMember(mem => mem.Content, opts => opts.MapFrom(src => src.Question.Content))
            .ForMember(mem => mem.Audio, opts => opts.MapFrom(src => src.Question.AudioFileName))
            .ForMember(mem => mem.PhotoUrl, opts => opts.MapFrom(src => src.Question.ImageFileName))
            .ForMember(mem => mem.Answers, opts => opts.MapFrom(src => src.Question.Answers));
            CreateMap<Exam, ExamAnalyzeDTO>();
            CreateMap<WordQuestion, QuestionDTO>();
            CreateMap<Script,ScriptAnalyzeDTO>();
            CreateMap<Section,SectionAnalyzeDTO>();
            CreateMap<Route,RouteAnalyzeDTO>();
        }
    }
}