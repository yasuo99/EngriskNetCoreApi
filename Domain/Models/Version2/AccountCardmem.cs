using System;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models;

namespace Domain.Models.Version2
{
    public class AccountCardmem
    {
        public int AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public virtual Account Account { get; set; }
        public Guid WordId { get; set; }
        [ForeignKey(nameof(WordId))]
        public virtual Word Word { get; set; }
        public Guid MemoryId  { get; set; }
        [ForeignKey(nameof(MemoryId))]
        public virtual Memory Memory { get; set; }
    }
}