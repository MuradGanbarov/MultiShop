using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using MultiShop.Areas.MultiShopAdmin.ViewModels;
using MultiShop.DAL;
using MultiShop.Models;
using MultiShop.ViewModels;

namespace MultiShop.Controllers
{
    public class ShopController : Controller
    {
        private readonly AppDbContext _context;

        public ShopController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int page=1,int? catId=null,int sort=1)
        {
            int count;
            int totalpage;
            ICollection<Product> products = new List<Product>();
            if (catId != null)
            {
                if (catId <= 0) return BadRequest();
                count = await _context.Products.Where(p=>p.CategoryId==catId).CountAsync();
                totalpage = (int)Math.Ceiling((double)count /5);
                switch(sort)
                {
                    case 1:
                        products = await _context.Products.Where(p=>p.CategoryId==catId).OrderByDescending(p => p.Id).Skip((page-1)*5).Take(5).Include(p=>p.ProductImages.Where(pi=>pi.IsPrimary==true)).ToListAsync();
                        break;
                    case 2:
                        products = await _context.Products.Where(p => p.CategoryId == catId).OrderBy(p => p.Price).Skip((page - 1) * 5).Take(5).Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true)).ToListAsync();
                    break;
                    case 3:
                        products = await _context.Products.Where(p => p.CategoryId == catId).OrderBy(p => p.Name).Skip((page - 1) * 5).Take(5).Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true)).ToListAsync();
                    break;

                }
              
            }
            else
            {
                count = await _context.Products.CountAsync();
                totalpage = (int)Math.Ceiling((double)count / 5);
                switch (sort)
                {
                    case 1:
                        products = await _context.Products.OrderByDescending(p => p.Id).Skip((page - 1) * 5).Take(5).Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true)).ToListAsync();
                        break;
                    case 2:
                        products = await _context.Products.OrderBy(p => p.Price).Skip((page - 1) * 5).Take(5).Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true)).ToListAsync();
                        break;
                    case 3:
                        products = await _context.Products.OrderBy(p => p.Name).Skip((page - 1) * 5).Take(5).Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true)).ToListAsync();
                        break;

                }
            }

            ShopVM vm = new()
            {
                Colors = await _context.Colors.Include(c => c.ProductColors).ThenInclude(pc => pc.Product).ToListAsync(),
                Sizes = await _context.Sizes.Include(c => c.ProductSizes).ThenInclude(ps => ps.Product).ToListAsync(),
                Products = products,
                TotalPage = totalpage,
                CurrentPage = page,
                CategoryId = catId,
                Sort = sort,
            };
            return View(vm);
        
        }
    }
}
