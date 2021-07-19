using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.DTOs.ExamSchedule
{
    public class CreateExamScheduleDTO
    {
        [Required(ErrorMessage = "Tiêu đề không được đê trống"), MaxLength(20, ErrorMessage = "Tiêu đề tối đa 20 kí tự")]
        public string Title { get; set; }
        [MaxLength(100, ErrorMessage = "Chi tiết tối đa 100 kí tự")]

        public string Detail { get; set; }
        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống")]
        public TimeSpan Start { get; set; }
        [Required(ErrorMessage = "Thời gian kết thúc không được để trống")]
        public int Duration{get;set;}
        public Guid? ExamId { get; set; }
        public int? ListeningQuestionCount { get; set; }
        public int? ReadingQuestionCount { get; set; }
        public int? Exp { get; set; }
        [Required]
        public int DifficultLevel { get; set; }
    }
}