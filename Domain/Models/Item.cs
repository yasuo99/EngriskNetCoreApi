using System.Collections.Generic;

namespace Domain.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public string ImgUrl { get; set; }
        public string Uses { get; set; }
        public int Price { get; set; }
        public int Usage { get; set; }
        public virtual ICollection<AccountStorage> Accounts { get; set; }
    }
}