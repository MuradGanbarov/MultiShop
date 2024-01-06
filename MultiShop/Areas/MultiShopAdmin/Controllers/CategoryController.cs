using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiShop.Areas.MultiShopAdmin.ViewModels;
using MultiShop.DAL;
using MultiShop.Models;

namespace MultiShop.Areas.MultiShopAdmin.Controllers
{
    [Area("MultiShopAdmin")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int page)
        {
            double count = await _context.Categories.CountAsync();
            List<Category>? categories = await _context.Categories.Skip(page*3).Take(3).Include(c=>c.Products).ToListAsync();

            PaginationVM<Category>? paginationVM = new()
            {
                CurrentPage = page + 1,
                TotalPage = Math.Ceiling(count / 3),
                Items=categories,
            };
 
            return View(paginationVM);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Create(CreateCategoryVM vm)
        {
            if (!ModelState.IsValid) return View();

            bool result = _context.Categories.Any(c => c.Name == vm.Name);

            if (result)
            {
                ModelState.AddModelError("Name", "This Category already exists");
                return View();
            }

            Category category = new()
            {
                Name = vm.Name,
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (category is null) return NotFound();
            
            return View(category);
        
        }
        [HttpPost]
        
        public async Task<IActionResult> Update(int id,UpdateCategoryVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            Category existed = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if(existed is null) return NotFound();

            bool ExistCheck = await _context.Categories.AnyAsync(c => c.Name.ToLower().Trim() == vm.Name.ToLower().Trim());

            if (ExistCheck)
            {
                ModelState.AddModelError("Name", "This category already exists");
                return View(vm);
            }

            existed.Name = vm.Name;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
        public async Task<IActionResult> Delete(int id)
        {
            if(id <= 0) return BadRequest();

            Category existed = await _context.Categories.FirstOrDefaultAsync(c=>c.Id==id);

            if (existed is null) return NotFound();

            _context.Categories.Remove(existed);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Detail(int id)
        {
            if(id<=0) return BadRequest();

            Category category = await _context.Categories.Include(c => c.Products).ThenInclude(p=>p.ProductImages.Where(pi=>pi.IsPrimary==true)).FirstOrDefaultAsync(c=>c.Id == id);

            if(category is null) return NotFound();
            
            return View(category);
    
        }


    }
}
