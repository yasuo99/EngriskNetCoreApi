using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class StringFilter
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public DateTime Inserted { get; set; }
    }
}