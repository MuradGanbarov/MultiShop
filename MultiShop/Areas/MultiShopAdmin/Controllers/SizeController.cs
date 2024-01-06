using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiShop.Areas.MultiShopAdmin.ViewModels;
using MultiShop.DAL;
using MultiShop.Models;

namespace MultiShop.Areas.MultiShopAdmin.Controllers
{
    [Area("MultiShopAdmin")]
    public class SizeController : Controller
    {
        private readonly AppDbContext _context;

        public SizeController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int page)
        {
            double count = await _context.Sizes.CountAsync();
            List<Size>? sizes = await _context.Sizes.Skip(page * 3).Take(3).Include(c => c.ProductSizes).ThenInclude(pc => pc.Product).ToListAsync();

            PaginationVM<Size>? vm = new()
            {
                CurrentPage = page + 1,
                TotalPage = Math.Ceiling(count / 3),
                Items = sizes
            };
            
            return View(vm);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Create(CreateSizeVM vm)
        {
            if (!ModelState.IsValid) return View();
            bool result = await _context.Sizes.AnyAsync(s => s.Name.ToLower().Trim() == vm.Name.ToLower().Trim());
            if (result)
            {
                ModelState.AddModelError("Size", "This size already exist");
                return View();
            }

            Size size = new()
            {
                Name = vm.Name,
            };
        
            await _context.Sizes.AddAsync(size);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();

            Size existed = await _context.Sizes.FirstOrDefaultAsync(s=>s.Id == id);
            if (existed is null) return NotFound();
            return View(existed);

        }
        [HttpPost]

        public async Task<IActionResult> Update(int id,UpdateSizeVM vm)
        {
            if(!ModelState.IsValid) return View();

            Size existed = await _context.Sizes.FirstOrDefaultAsync(s=>s.Id==id);
            if (existed is null) return NotFound();
            bool result = await _context.Sizes.AnyAsync(s=>s.Name.ToLower().Trim()==vm.Name.ToLower().Trim() && s.Id != id);
            if(result)
            {
                ModelState.AddModelError("Size", "This size already existed");
                return View();
            }

            existed.Name = vm.Name;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            if(id<=0) return BadRequest();
            Size size = await _context.Sizes.FirstOrDefaultAsync(s => s.Id == id);
            if (size is null) return NotFound();
            _context.Remove(size);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            if(id<=0) return BadRequest();
            Size size = await _context.Sizes.FirstOrDefaultAsync(s=>s.Id==id);
            if (size is null) return NotFound();
            return View(size);
        }

    }
}
