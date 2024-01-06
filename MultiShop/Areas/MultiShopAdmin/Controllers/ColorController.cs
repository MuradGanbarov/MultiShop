using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiShop.Areas.MultiShopAdmin.ViewModels;
using MultiShop.DAL;
using MultiShop.Models;

namespace MultiShop.Areas.MultiShopAdmin.Controllers
{
    [Area("MultiShopAdmin")]
    public class ColorController : Controller
    {
        private readonly AppDbContext _context;

        public ColorController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int page)
        {
            double count = await _context.Colors.CountAsync();
            List<Color>? colors = await _context.Colors.Skip(page*3).Take(3).Include(c=>c.ProductColors).ThenInclude(pc=>pc.Product).ToListAsync();
            PaginationVM<Color>? vm = new()
            {
                CurrentPage = page + 1,
                TotalPage = Math.Ceiling(count / 3),
                Items = colors,
            };
            
            return View(vm);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Create(CreateColorVM vm)
        {
            if (!ModelState.IsValid) return View();

            bool result = _context.Colors.Any(c=>c.Name.ToLower().Trim() == vm.Name.ToLower().Trim());

            if(result)
            {
                ModelState.AddModelError("Name", "This color is existed");
                return View();
            }

            Color color = new()
            {
                Name = vm.Name,
            };

            await _context.Colors.AddAsync(color);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            Color color = await _context.Colors.FirstOrDefaultAsync(c=>c.Id==id);
            if (color is null) return NotFound();
            return View(color);

        }

        [HttpPost]
        public async Task<IActionResult> Update(int id,UpdateColorVM vm)
        {
            if(!ModelState.IsValid) return View();

            Color existed = await _context.Colors.FirstOrDefaultAsync(c => c.Id == id);
            if (existed is null) return NotFound();

            bool result = await _context.Colors.AnyAsync(c => c.Name.ToLower().Trim() == existed.Name.ToLower().Trim() && c.Id != id);
            if (result)
            {
                ModelState.AddModelError("Name", "This color already existed");
                return View();
            }

            existed.Name = vm.Name;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();

            Color color = await _context.Colors.FirstOrDefaultAsync(c => c.Id == id);
            if(color is null) return NotFound();

            _context.Colors.Remove(color);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(int id)
        {
            if(id <= 0) return BadRequest();
            Color color = await _context.Colors.Include(c => c.ProductColors).ThenInclude(pc => pc.Product).ThenInclude(p => p.ProductImages.Where(pi => pi.IsPrimary == true)).FirstOrDefaultAsync(c => c.Id == id);
            if (color is null) return NotFound();
            return View(color);
        }


    }
}
