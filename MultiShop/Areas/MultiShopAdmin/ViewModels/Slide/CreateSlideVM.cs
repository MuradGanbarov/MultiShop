using System.ComponentModel.DataAnnotations;

namespace MultiShop.Areas.MultiShopAdmin.ViewModels
{
    public class CreateSlideVM
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(25, ErrorMessage = "Title of slide can contain 25 characters")]
        public string Title { get; set; }
        public string? Description { get; set; }
        [Required]
        public int Order { get; set; }
        [Required]
        public IFormFile Photo { get; set; }
    }
}
