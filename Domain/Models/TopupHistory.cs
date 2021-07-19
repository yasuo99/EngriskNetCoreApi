using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class TopupHistory
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        public string OrderId { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime TopupDate { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; }
        public string Status{ get; set; }
    }
}