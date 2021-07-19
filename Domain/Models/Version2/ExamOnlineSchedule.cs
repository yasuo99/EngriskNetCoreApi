using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;
using Domain.Models.BaseModel.Generic;

namespace Domain.Models.Version2
{
    public class ExamOnlineSchedule : AuditEntity<Guid>
    {
        [Required(ErrorMessage = "Tiêu đề không được đê trống"), MaxLength(20, ErrorMessage = "Tiêu đề tối đa 20 kí tự")]
        public string Title { get; set; }
        [MaxLength(100, ErrorMessage = "Chi tiết tối đa 100 kí tự")]

        public string Detail { get; set; }
        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống")]
        public TimeSpan Start { get; set; }
        [Required(ErrorMessage = "Thời gian kết thúc không được để trống")]
        public TimeSpan End { get; set; }
        public Guid? ExamId { get; set; }
        [ForeignKey(nameof(ExamId))]
        public virtual Exam Exam { get; set; }
        public int? ListeningQuestionCount { get; set; }
        public int? ReadingQuestionCount { get; set; }
        public int? Exp { get; set; }
        public int? RankOnePoint { get; set; } = 0;
        public int? RankTwoPoint { get; set; } = 0;
        public int? RankThreePoint { get; set; } = 0;
        [Required]
        public DifficultLevel Difficult { get; set; }
        public virtual ICollection<TournamentHistory> TournamentHistories { get; set; }
    }
}