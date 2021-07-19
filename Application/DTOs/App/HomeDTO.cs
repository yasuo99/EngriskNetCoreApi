using System.Collections.Generic;
using Application.DTOs.Account;
using Application.DTOs.Quiz;
using Application.DTOs.Word;

namespace Application.DTOs.App
{
    public class HomeDTO
    {
        public List<QuizDTO> Quizzes { get; set; }
        public List<WordDTO> Words { get; set; }
        public List<AccountBlogDTO> Users { get; set; }
    }
}