﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiShop.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required,StringLength(20)]
        public string Name { get; set; }
        public string Image { get; set; }
        public List<Product> Products { get; set; }
        [NotMapped]
        public IFormFile Photo { get; set; }
    }
}
