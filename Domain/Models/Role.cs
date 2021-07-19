using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Domain.Models
{
    public class Role: IdentityRole<int>
    {
        public virtual ICollection<AccountRole> Accounts { get; set; }
    }
}