using MultiShop.Models;
using System.ComponentModel.DataAnnotations;

namespace MultiShop.Areas.MultiShopAdmin.ViewModels
{
    public class CreateColorVM
    {
        [MaxLength(30,ErrorMessage = "Name of color can contain maximum 30 characters!")]
        public string Name { get; set; }
        public List<ProductColor>? ProductColors { get; set; }
    }
}
