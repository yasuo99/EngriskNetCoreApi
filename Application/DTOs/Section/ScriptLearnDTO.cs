using System;
using System.Collections.Generic;
using Application.DTOs.Certificate;
using Application.DTOs.Exam;
using Application.DTOs.Question;
using Application.DTOs.Word;
using Domain.Models.Version2;

namespace Application.DTOs.Section
{
    public class ScriptLearnDTO
    {
        public ScriptLearnDTO()
        {
            Words = new List<WordDTO>();
            Questions = new List<QuestionDTO>();
        }
        public Guid Id { get; set; }
        public string Theory { get; set; }
        public ExamDTO MiniExam { get; set; }
        public CertificateDTO Certificate { get; set; }
        public List<WordDTO> Words { get; set; }
        public List<QuestionDTO> VocabularyPractice { get; set; }
        public List<QuestionDTO> Questions { get; set; }
        public string Type { get; set; }
    }
}