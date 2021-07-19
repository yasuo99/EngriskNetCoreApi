using System;
using Application.DTOs.Account;
using Application.DTOs.Word;
using Domain.Models.Version2;
using Newtonsoft.Json;

namespace Application.DTOs.Example
{
    public class ExampleDTO
    {
        public Guid Id { get; set; }
        public string Eng { get; set; }
        public string Vie { get; set; }
        public string Contributor { get; set; }
        public virtual AccountBlogDTO Account { get; set; }
        public DateTime CreatedDate { get; set; }
        [JsonIgnore]
        public virtual WordDTO Word { get; set; }
    }
}