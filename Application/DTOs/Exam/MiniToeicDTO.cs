using System.Collections.Generic;
using Application.DTOs.Question;

namespace Application.DTOs.Exam
{
    public class MiniToeicDTO
    {
        public List<QuestionDTO> PartOne { get; set; }
        public List<QuestionDTO> PartTwo { get; set; }
        public List<QuestionDTO> PartThree { get; set; }
        public List<QuestionDTO> PartFour { get; set; }
        public List<QuestionDTO> PartFive { get; set; }
        public List<QuestionDTO> PartSix { get; set; }
        public List<QuestionDTO> PartSeven { get; set; }
    }
}