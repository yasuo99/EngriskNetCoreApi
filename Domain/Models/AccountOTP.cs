using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class AccountOTP
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        public int OTP { get; set; }
        public string Token { get; set; }
        public DateTime ValidUntil { get; set; }
        public bool IsAlive => ValidUntil > DateTime.Now;
    }
}