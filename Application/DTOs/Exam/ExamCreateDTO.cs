using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Application.DTOs.Question;
using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Exam
{
    public class ExamCreateDTO
    {
        public ExamCreateDTO()
        {
            QuestionImages = new List<IFormFile>();
        }
        [Required]
        public string Title { get; set; }
        public string Detail { get; set; }
        [Required, Range(10,180, ErrorMessage = "Thời gian làm bài phải tối thiểu 10 phút và tối đa 180 phút")]
        public int Duration { get; set; }
        public int Exp { get; set; }
        public int Pass { get; set; }
        public int UnlockPoint { get; set; }
        public int TotalScore { get; set; }
        public DifficultLevel Difficult { get; set; }
        public ICollection<QuestionCreateDTO> ExamQuestions { get; set; }
        public List<IFormFile> QuestionImages { get; set; }
        public string StringifyQuestions { get; set; }
        
        
    }
}