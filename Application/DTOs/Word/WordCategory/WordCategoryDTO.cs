using System;
using System.Collections.Generic;
using Domain.Models.Version2;

namespace Application.DTOs.Word.WordCategory
{
    public class WordCategoryDTO
    {
        public Guid Id { get; set; }
        public string CategoryName { get; set; }
        public string CategoryImage { get; set; }
        public string Description { get; set; }
        public virtual ICollection<CategoryTag> Tags { get; set; }
        public virtual ICollection<WordDTO> Vocabulary { get; set; }
    }
}