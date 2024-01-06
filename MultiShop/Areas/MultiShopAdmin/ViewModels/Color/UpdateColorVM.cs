using System.ComponentModel.DataAnnotations;

namespace MultiShop.Areas.MultiShopAdmin.ViewModels
{
    public class UpdateColorVM
    {
        [MaxLength(ErrorMessage = "Name of category can contain maximum 30 characters!")]
        public string Name { get; set; }
    }
}
