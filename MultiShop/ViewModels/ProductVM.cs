using MultiShop.Models;

namespace MultiShop.ViewModels
{
    public class ProductVM
    {
        public Product? Product { get; set; }
        public List<Product>? SimilarProducts { get; set; }
    }
}
