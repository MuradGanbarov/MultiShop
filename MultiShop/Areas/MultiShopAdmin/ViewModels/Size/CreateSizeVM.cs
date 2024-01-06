using MultiShop.Models;
using System.ComponentModel.DataAnnotations;

namespace MultiShop.Areas.MultiShopAdmin.ViewModels
{ 
    public class CreateSizeVM
    {
        [Required(ErrorMessage ="name of size is required")]
        [MaxLength(30,ErrorMessage = "Name of size can contain maximum 30 characters!")]
        public string Name { get; set; }
    }
}
