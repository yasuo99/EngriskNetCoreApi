using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Domain.Models
{
    public class AccountRole: IdentityUserRole<int>
    {
        public virtual Account Account { get; set; }

        public virtual Role Role { get; set; }
    }
}