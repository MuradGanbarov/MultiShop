using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiShop.DAL;
using MultiShop.Models;
using MultiShop.ViewModels;

namespace MultiShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Detail(int id)
        {
            Product product = await _context.Products.
                Include(p => p.ProductImages).
                Include(p => p.Category).
                Include(p => p.ProductColors).
                ThenInclude(pc => pc.Color).
                Include(pc => pc.ProductSizes).
                ThenInclude(pc => pc.Size).FirstOrDefaultAsync(p=>p.Id==id);

            if (product is null) return NotFound();

            List<Product> SimiliarProducts = await _context.Products.Include(product => product.ProductImages.Where(pi => pi.IsPrimary != false)).Where(p => p.CategoryId == product.CategoryId && product.Id != p.Id).Take(4).ToListAsync();

            ProductVM vm = new()
            {
                Product = product,
                SimilarProducts = SimiliarProducts,
            };

            return View(vm);
        }
    }
}
