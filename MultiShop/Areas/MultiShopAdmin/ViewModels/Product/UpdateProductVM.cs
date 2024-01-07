using MultiShop.Models;
using System.ComponentModel.DataAnnotations;

namespace MultiShop.Areas.MultiShopAdmin.ViewModels
{
    public class UpdateProductVM
    {
        public string Name { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Price must be bigger than 0")]
        [Required]
        public decimal Price { get; set; }
        [Range(0,double.MaxValue, ErrorMessage ="Discount can not be less than 0")]
        public decimal? Discount { get; set; }

        [Range(1, 300, ErrorMessage = "Chooce a category!")]
        public int? CategoryId { get; set; }
        public string Description { get; set; }
        
        public IFormFile? MainPhoto { get; set; }
        public IFormFile? HoverPhoto { get; set; }
        
        public List<Category>? Categories { get; set; }
        public List<int>? ColorIds { get; set; }
        public List<int>? SizeIds { get; set; }
        public List<Color>? Colors { get; set; }
        public List<Size>? Sizes { get; set; }
        public List<ProductImage>? ProductImages { get; set; }
    }
}
