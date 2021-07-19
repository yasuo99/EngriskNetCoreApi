using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class AccountStorage
    {
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        public int ItemId { get; set; }
        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; }
        public DateTime PurchasedDate { get; set; }
        public bool IsUsing { get; set; }
        public DateTime UseDate { get; set; }
        public DateTime OverDate { get; set; }
    }
}