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
        public async Task<IActionResult> Index(int page)
        {
            HomeVM vm = new()
            {
                Slides = await _context.Slides.OrderBy(s => s.Order).Take(3).ToListAsync(),
                Products = await _context.Products.Include(p => p.ProductImages).Include(p => p.Category).ToListAsync(),
                Categories = await _context.Categories.Where(c=>c.Products.Count>0).Include(p => p.Products).ToListAsync(),
                
            };


            return View(vm);
        }
    }
}
