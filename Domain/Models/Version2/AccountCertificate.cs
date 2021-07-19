using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.Version2
{
    public class AccountCertificate
    {
        public int AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public virtual Account Account { get; set; }
        public Guid CertificateId { get; set; }
        public virtual Certificate Certificate { get; set; }
        public string Signature { get; set; }
        public DateTime AchievedDate { get; set; }
        public DateTime ExpireDate { get; set; }
    }
}