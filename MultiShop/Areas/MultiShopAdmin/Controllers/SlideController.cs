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
    public class SlideController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SlideController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            List<Slide> slides = await _context.Slides.ToListAsync();
            return View(slides);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Create(CreateSlideVM vm)
        {
            if (!ModelState.IsValid) return View();

            if (!vm.Photo.IsValidType(FileType.Image))
            {
                ModelState.AddModelError("Image", "Photo should be image type");
                return View();
            }

            if (!vm.Photo.IsValidSize(5, FileSize.Megabite))
            {
                ModelState.AddModelError("Image", "Photo size can be less than 5 mb");
                return View();
            }

            if (vm.Order <= 0)
            {
                ModelState.AddModelError("Order", "Order can not be less or equal 0");
                return View();
            }

            string fileName = await vm.Photo.CreateAsync(_env.WebRootPath, "img");

            Slide slide = new()
            {
                ImageURL = fileName,
                Title = vm.Title,
                Description = vm.Description,
                Order = vm.Order,
            };

            await _context.Slides.AddAsync(slide);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();

            Slide slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (slide is null) return NotFound();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateSlideVM vm)
        {
            if (!ModelState.IsValid) return View();

            Slide existed = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);

            if (existed is null) return NotFound();
            if (vm.Photo is not null)
            {
                if (!vm.Photo.IsValidType(FileType.Image))
                {
                    ModelState.AddModelError("Image", "Photo should be image type");
                    return View();
                }
                if (!vm.Photo.IsValidSize(5, FileSize.Megabite))
                {
                    ModelState.AddModelError("Image", "Photo can be less or equal 5 mb");
                    return View();
                }

                if (vm.Order <= 0)
                {
                    ModelState.AddModelError("Order", "Order can not be less or equal 0");
                    return View();
                }

                string NewImage = await vm.Photo.CreateAsync(_env.WebRootPath, "img");
                existed.ImageURL.Delete(_env.WebRootPath, "img");
                existed.ImageURL = NewImage;

            }

            existed.Title = vm.Title;
            existed.Description = vm.Description;
            existed.Order = vm.Order;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();
            Slide slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (slide is null) return NotFound();
            slide.ImageURL.Delete(_env.WebRootPath, "img");
            _context.Slides.Remove(slide);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(int id)
        {
            if(id<=0) return BadRequest();
            Slide slide = await _context.Slides.FirstOrDefaultAsync(s=>s.Id == id);
            if (slide is null) return NotFound();
            return View(slide);

        }


    }
}
