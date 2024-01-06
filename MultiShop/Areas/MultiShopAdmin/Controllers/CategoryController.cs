using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiShop.Areas.MultiShopAdmin.Models;
using MultiShop.Areas.MultiShopAdmin.Models.Utilities.Enums;
using MultiShop.Areas.MultiShopAdmin.ViewModels;
using MultiShop.DAL;
using MultiShop.Models;

namespace MultiShop.Areas.MultiShopAdmin.Controllers
{
    [Area("MultiShopAdmin")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CategoryController(AppDbContext context,IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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

        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid) return View();

            if (category.Photo is null)
            {
                ModelState.AddModelError("Photo", "Please choose image");
                return View(category);
            }
            if (!category.Photo.IsValidType(FileType.Image))
            {
                ModelState.AddModelError("Photo", "Photo should be image type");
                return View(category);
            }
            if (!category.Photo.IsValidSize(5, FileSize.Megabite))
            {
                ModelState.AddModelError("Photo", "Photo size can be less or equal 5mb");
                return View(category);
            }

            category.Image = await category.Photo.CreateAsync(_env.WebRootPath, "category");


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
        
        public async Task<IActionResult> Update(int id,Category category)
        {
            if(id <=0) return BadRequest();
            Category existed = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if(existed is null) return NotFound();
            if (existed.Name is null) return NotFound();
            if (ModelState.IsValid) return View(existed);
            if(category.Photo is null)
            {
                string fileName = existed.Image;
                _context.Entry(existed).CurrentValues.SetValues(category);
                existed.Image= fileName;
            }
            else
            {
                if (!category.Photo.IsValidType(FileType.Image))
                {
                    ModelState.AddModelError("Photo", "Photo should be image type");
                    return View(existed);
                }
                if (!category.Photo.IsValidSize(5, FileSize.Megabite))
                {
                    ModelState.AddModelError("Photo", "Please choose correct image");
                    return View(existed);
                }

                FileValidator.Delete(_env.WebRootPath, "category", existed.Image);
                _context.Entry(existed).CurrentValues.SetValues(category);
                existed.Image = await category.Photo.CreateAsync(_env.WebRootPath, "category");
            }

            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
        public async Task<IActionResult> Delete(int id)
        {
            if(id <= 0) return BadRequest();

            Category existed = await _context.Categories.FirstOrDefaultAsync(c=>c.Id==id);

            if (existed is null) return NotFound();
            FileValidator.Delete(_env.WebRootPath, "category", existed.Image);
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
