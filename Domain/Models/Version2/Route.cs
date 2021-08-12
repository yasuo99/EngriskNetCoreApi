using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;
using Domain.Models.BaseModel.Generic;

namespace Domain.Models.Version2
{
    public class Route : AuditEntity<Guid>
    {
        public Route()
        {
            Sections = new HashSet<Section>();
            VerifiedStatus = Status.Nope;
            PublishStatus = PublishStatus.UnPublished;
        }
        public string Title { get; set; }
        public string Description { get; set; }
        public string RouteImage { get; set; }
        public int? AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public virtual Account Account { get; set; }
        public bool IsPrivate { get; set; } = true;
        public PublishStatus PublishStatus { get; set; }
        public Status VerifiedStatus { get; set; }
        public bool IsSequentially { get; set; }
        public virtual ICollection<Section> Sections { get; set; }
    }
}