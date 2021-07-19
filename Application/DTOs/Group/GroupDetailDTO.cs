using System;
using System.Collections.Generic;
using Domain.Models;

namespace Application.DTOs.Group
{
    public class GroupDetailDTO
    {
        public Guid Id { get; set; }
        public int AccountId { get; set; }
        public string AccountUsername   {get;set;}
        public string GroupName { get; set; }
        public IEnumerable<WordGroup> Words{ get; set; }
    }
}