using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class AccountFollower
    {
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        public int FollowerId { get; set; }
        [ForeignKey("FollowerId")]
        public virtual Account Follower { get; set; }

    }
}