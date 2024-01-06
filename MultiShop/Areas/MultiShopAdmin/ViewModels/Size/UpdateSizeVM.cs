using System.ComponentModel.DataAnnotations;

namespace MultiShop.Areas.MultiShopAdmin.ViewModels
{
    public class UpdateSizeVM
    {
        [MaxLength(ErrorMessage = "Name of color can contain maximum 30 characters!")]
        public string Name { get; set; }

    }
}
