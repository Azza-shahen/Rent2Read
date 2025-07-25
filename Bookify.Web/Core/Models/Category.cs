﻿
namespace Bookify.Web.Core.Models
{
    public class Category
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public DateTime CreatedOn{ get; set; }= DateTime.Now;
        public DateTime? LastUpdatedOn { get; set; }

        public bool IsDeleted { get; set; }  





    }
}
