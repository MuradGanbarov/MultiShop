using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiShop.DAL;
using MultiShop.Models;
using MultiShop.ViewModels;

namespace MultiShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<Slide> slides = await _context.Slides.OrderBy(s=>s.Order).Take(3).ToListAsync();
            List<Product> products = await _context.Products.Include(p => p.ProductImages).OrderByDescending(s => s.Id).ToListAsync();

            HomeVM vm = new()
            {
                Slides = slides,
                Products = products,
                NewProduct = products.Take(8).ToList()
            };
            return View(vm);
        }
    }
}
