using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;
using Domain.Models;
using Domain.Models.BaseModel.Generic;
using Newtonsoft.Json;

namespace Domain.Models.Version2
{
    public class Memory : AuditEntity<Guid>
    {
        public Memory()
        {
            VerifiedStatus = Status.Pending;
        }
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        [JsonIgnore]
        public virtual Account Account { get; set; }
        //Tiêu đề của mem card
        public string Title { get; set; }
        //Hình ảnh của mem card
        public string MemImg { get; set; }
        public Status VerifiedStatus { get; set; }
        public bool IsPrivate { get; set; } = true;
        public Guid WordId { get; set; }
        [ForeignKey(nameof(WordId))]
        [JsonIgnore]
        public virtual Word Word { get; set; }
        [JsonIgnore]
        public virtual ICollection<AccountCardmem> Accounts { get; set; }

    }
}