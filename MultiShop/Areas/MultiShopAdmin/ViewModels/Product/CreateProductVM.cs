using MultiShop.Models;
using System.ComponentModel.DataAnnotations;

namespace MultiShop.Areas.MultiShopAdmin.ViewModels
{
    public class CreateProductVM
    {
        public string Name { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Price can not be equal or less than 0")]
        public decimal Price { get; set; }
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
