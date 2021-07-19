using System;
using System.Collections.Generic;
using Domain.Models;

namespace Application.DTOs.Account
{
    public class AdminAccountDTO
    {
         public string Fullname { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public int Exp { get; set; }
        public int Point { get; set; }
        public DateTime Locked { get; set; }
        public bool IsDisabled { get; set; }
        public string PhotoUrl { get; set; }
        public string PublicId { get; set; }
        public virtual IEnumerable<AccountRole> Roles { get; set; }
        public virtual IEnumerable<Domain.Models.Comment> Comments { get; set; }


    }
}