using MultiShop.Models;

namespace MultiShop.ViewModels
{
    public class ShopVM
    {
        public ICollection<Product> Products { get; set; }
        public List<Category> Categories { get; set; }
        public List<Size> Sizes { get; set; }
        public List<Color> Colors { get; set; }
        public int CurrentPage { get; set; }
        public double TotalPage { get; set; }
        public int? CategoryId { get; set; }
        public int Sort { get; set; }
    }
}
