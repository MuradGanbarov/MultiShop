using MultiShop.Models;

namespace MultiShop.ViewModels
{
    public class HomeVM
    {
        public List<Slide> Slides { get; set; }
        public List<Product> Products { get; set; }
        public List<Product> NewProducts { get; set; }
        public List<Category> Categories { get; set; }
        public int CurrentPage { get; set; }
        public double TotalPage { get; set; }
        
    }
}
