using System;
using Application.DTOs.Account;
using Application.DTOs.Word;

namespace Application.DTOs.Memory
{
    public class MemoryCensorDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string MemImg { get; set; }
        public string CreatedBy { get; set; }
        public virtual WordDTO Word { get; set; }
        public virtual AccountBlogDTO Account { get; set; }
    }
}