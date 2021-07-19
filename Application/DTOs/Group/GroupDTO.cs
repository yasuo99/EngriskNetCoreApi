using System;
using System.Collections.Generic;
using Application.DTOs.Word;
using Domain.Models;

namespace Application.DTOs.Group
{
    public class GroupDTO
    {
        public Guid Id { get; set; }
        public int AccountId { get; set; }
        public string AccountUsername   {get;set;}
        public string GroupName { get; set; }
        public IEnumerable<WordDTO> Words { get; set; }
    }
}