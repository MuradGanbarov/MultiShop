using MultiShop.Models;
using System.ComponentModel.DataAnnotations;

namespace MultiShop.Areas.MultiShopAdmin.ViewModels
{
    public class CreateProductVM
    {
        public string Name { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Price can not be equal or less than 0")]
        public decimal Price { get; set; }
        [Range(0,double.MaxValue,ErrorMessage ="Discount can not be less than 0")]
        public decimal? Discount { get; set; }
        [Required]
        [MinLength(10, ErrorMessage = "Description can contain minimum 10 characters")]
        public string Description { get; set; }
        [Required]
        public IFormFile MainPhoto { get; set; }
        public List<IFormFile> SliderPhoto { get; set; }
        
        [Range(1, 300, ErrorMessage = "Chooce a category!")]
        public int? CategoryId { get; set; }
        public List<Category>? Categories { get; set; }
        public List<Color>? Colors { get; set; }
        public List<int>? ColorIds { get; set; }
        public List<Size>? Sizes { get; set; }
        public List<int>? SizeIds { get; set; }
    }
}
