using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class Footer
    {
        public int Id { get; set; }
        public string TaxCode { get; set; }
        public string PhoneContact { get; set; }
        public string Description { get; set; }
        public string FacebookLink1 { get; set; }
        public string FacebookLink2 { get; set; }
        public bool IsPublished { get; set; }
        public DateTime Inserted { get; set; }
    }
}