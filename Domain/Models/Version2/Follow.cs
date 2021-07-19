using System;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models;

namespace Domain.Models.Version2
{
    public class Follow
    {
        public int FollowerId { get; set; }
        [ForeignKey("FollowerId")]
        public virtual Account Follower { get; set; }
        public int FollowingId { get; set; }
        [ForeignKey("FollowingId")]
        public virtual Account Following { get; set; }
        public DateTime Timestamp { get; set; }
        public bool NotificationSwitch { get; set; }
    }
}