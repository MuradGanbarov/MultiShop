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
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index(int page)
        {
            double count = await _context.Products.CountAsync();
            List<Product> products = await _context.Products.Skip(page * 3).Take(3).Include(p => p.Category).Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true)).Include(p => p.ProductSizes).ThenInclude(ps => ps.Size).Include(p => p.ProductColors).ThenInclude(pc => pc.Color).ToListAsync();

            PaginationVM<Product> vm = new()
            {
                CurrentPage = page + 1,
                TotalPage = Math.Ceiling(count / 3),
                Items = products,
            };

            return View(vm);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await GetCategoriesAsync();
            ViewBag.Sizes = await GetSizesAsync();
            ViewBag.Colors = await GetColorsAsync();
            return View();

        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            if (!ModelState.IsValid) return View();
            bool result = await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);
            if (!result)
            {
                ViewBag.Categories = await GetCategoriesAsync();
                ViewBag.Colors = await GetColorsAsync();
                ViewBag.Sizes = await GetSizesAsync();
                ModelState.AddModelError("Category", "Category doesn't exists");
            }

            if (!productVM.MainPhoto.IsValidType(FileType.Image))
            {
                ViewBag.Categories = await GetCategoriesAsync();
                ViewBag.Colors = await GetColorsAsync();
                ViewBag.Sizes = await GetSizesAsync();
                ModelState.AddModelError("MainPhoto", "Incorrect file type");
                return View();
            }
            if (!productVM.MainPhoto.IsValidSize(5, FileSize.Megabite))
            {
                ViewBag.Categories = await GetCategoriesAsync();
                ViewBag.Colors = await GetColorsAsync();
                ViewBag.Sizes = await GetSizesAsync();
                ModelState.AddModelError("MainPhoto", "Photo size can not less or equal 5mb");
                return View();
            }

            ProductImage MainPhoto = new ProductImage
            {
                IsPrimary = true,
                Alternative = productVM.Name,
                URL = await productVM.MainPhoto.CreateAsync(_env.WebRootPath, "img")
            };

            Product product = new Product
            {
                Name = productVM.Name,
                Description = productVM.Description,
                Price = productVM.Price,
                CategoryId = productVM.CategoryId,
                ProductColors = new List<ProductColor>(),
                ProductImages = new List<ProductImage> { MainPhoto }

            };
            foreach (int colorId in productVM.ColorIds)
            {
                bool colresult = await _context.Colors.AnyAsync(c => c.Id == colorId);
                if (!colresult)
                {
                    ViewBag.Categories = await GetCategoriesAsync();
                    ViewBag.Colors = await GetColorsAsync();
                    ViewBag.Sizes = await GetSizesAsync();
                    ModelState.AddModelError("Color", "Color doesn't exists");
                    return View();
                }
                product.ProductColors.Add(new ProductColor { ColorId = colorId });
            }
            foreach (IFormFile sliderPhoto in productVM.SliderPhoto)
            {
                product.ProductImages.Add(new ProductImage { IsPrimary = false, Alternative = productVM.Name, URL = await sliderPhoto.CreateAsync(_env.WebRootPath, "img") });
            }

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int Id)
        {
            if (Id <= 0) return BadRequest();
            Product existed = await _context.Products.Include(p => p.ProductImages).Include(p => p.ProductColors).Include(p => p.ProductSizes).FirstOrDefaultAsync(p => p.Id == Id);
            if (existed is null) return NotFound();

            UpdateProductVM vm = new()
            {
                Name = existed.Name,
                Description = existed.Description,
                Price = existed.Price,
                CategoryId = existed.CategoryId,
                Categories = await GetCategoriesAsync(),
                SizeIds = existed.ProductSizes.Select(existed => existed.SizeId).ToList(),
                ColorIds = existed.ProductColors.Select(existed => existed.ColorId).ToList(),
                Sizes = await GetSizesAsync(),
                Colors = await GetColorsAsync(),
                ProductImages = existed.ProductImages
            };

            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateProductVM productVM)
        {
            Product existed = await _context.Products.Include(pi => pi.ProductImages).Include(p => p.ProductColors).Include(p=>p.ProductSizes).FirstOrDefaultAsync(p => p.Id == id);
            productVM.ProductImages = existed.ProductImages;
            if (!ModelState.IsValid)
            {
                productVM.Categories = await _context.Categories.ToListAsync();
                productVM.Colors = await _context.Colors.ToListAsync();
                productVM.Sizes = await _context.Sizes.ToListAsync();
                return View(productVM);
            }
            
            if (existed is null) return NotFound();

            bool result = await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);
            if (!result)
            {
                productVM.Categories = await _context.Categories.ToListAsync();
                productVM.Colors = await _context.Colors.ToListAsync();
                productVM.Sizes = await _context.Sizes.ToListAsync();
                ModelState.AddModelError("CategoryId", "Bele bir category movcud deyil");
                return View(productVM);
            }
            if (productVM.MainPhoto is not null)
            {
                if (!productVM.MainPhoto.IsValidType(FileType.Image))
                {
                    productVM.Categories = await _context.Categories.ToListAsync();
                    productVM.Colors = await _context.Colors.ToListAsync();
                    productVM.Sizes = await _context.Sizes.ToListAsync();
                    ModelState.AddModelError("MainPhoto", "File novu uygun deyil");
                    return View(productVM);
                }
                if (!productVM.MainPhoto.IsValidSize(5,FileSize.Megabite))
                {
                    productVM.Categories = await _context.Categories.ToListAsync();
                    productVM.Colors = await _context.Colors.ToListAsync();
                    ModelState.AddModelError("MainPhoto", "File olcusu uygun deyil");
                    return View(productVM);
                }

            }


            existed.ProductColors.RemoveAll(pc => !productVM.ColorIds.Exists(cId => cId == pc.ColorId));
            existed.ProductSizes.RemoveAll(ps => !productVM.SizeIds.Exists(sId => sId == ps.SizeId));
            List<int> creatable = productVM.ColorIds.Where(cId => !existed.ProductColors.Exists(pc => pc.ColorId == cId)).ToList();
            List<int> createablesize = productVM.SizeIds.Where(sId=> !existed.ProductSizes.Exists(ps=>ps.SizeId == sId)).ToList();
            foreach (int sizeId in createablesize)
            {
                bool sizeResult = await _context.Colors.AnyAsync(t => t.Id == sizeId);
                if (!sizeResult)
                {
                    productVM.Categories = await _context.Categories.ToListAsync();
                    productVM.Sizes = await _context.Sizes.ToListAsync();
                    ModelState.AddModelError("ColorIds", "Bele bir color movcud deyil");
                    return View(productVM);
                }
                existed.ProductSizes.Add(new ProductSize
                {
                    SizeId = sizeId
                });
            }

            foreach (int colorId in creatable)
            {
                bool colorResult = await _context.Colors.AnyAsync(t => t.Id == colorId);
                if (!colorResult)
                {
                    productVM.Categories = await _context.Categories.ToListAsync();
                    productVM.Colors = await _context.Colors.ToListAsync();
                    ModelState.AddModelError("ColorIds", "Bele bir color movcud deyil");
                    productVM.Sizes = await _context.Sizes.ToListAsync();
                    ModelState.AddModelError("SizesIds", "Bele bir size movcud deyil");
                    return View(productVM);
                }
                existed.ProductColors.Add(new ProductColor
                {
                    ColorId = colorId
                });
            }

            if (productVM.MainPhoto is not null)
            {
                string fileName = await productVM.MainPhoto.CreateAsync(_env.WebRootPath, "img");

                ProductImage mainImage = existed.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true);
                mainImage.URL.Delete(_env.WebRootPath, "img");
                _context.ProductImages.Remove(mainImage);

                existed.ProductImages.Add(new ProductImage
                {
                    Alternative = productVM.Name,
                    IsPrimary = true,
                    URL = fileName
                });
            }
           
            if (productVM.ImageIds is null)
            {
                productVM.ImageIds = new List<int>();
            }
            List<ProductImage> removeable = existed.ProductImages.Where(pi => !productVM.ImageIds.Exists(imgId => imgId == pi.Id) && pi.IsPrimary == null).ToList();
            foreach (ProductImage pImage in removeable)
            {
                pImage.URL.Delete(_env.WebRootPath, "img");
                existed.ProductImages.Remove(pImage);
            }


            TempData["Message"] = "";
            if (productVM.SliderPhoto is not null)
            {
                foreach (IFormFile photo in productVM.SliderPhoto)
                {
                    if (!photo.IsValidType(FileType.Image))
                    {
                        TempData["Message"] += $"<p class=\"text-danger\">{photo.FileName} file tipi uygun deyil</p>";
                        continue;
                    }
                    if (!photo.IsValidSize(5))
                    {
                        TempData["Message"] += $"<p class=\"text-danger\">{photo.FileName} file olcusu uygun deyil</p>";
                        continue;
                    }

                    existed.ProductImages.Add(new ProductImage
                    {
                        Alternative = productVM.Name,
                        IsPrimary = null,
                        URL = await photo.CreateAsync(_env.WebRootPath, "img")
                    });
                }

            }


            existed.Name = productVM.Name;
            existed.Description = productVM.Description;
            existed.Price = productVM.Price;
            existed.CategoryId = productVM.CategoryId;


            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();
            Product product = await _context.Products.Include(p => p.ProductImages).Include(p => p.ProductColors).Include(p => p.ProductSizes).FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();

            foreach (ProductImage image in product.ProductImages)
            {
                image.URL.Delete(_env.WebRootPath, "img");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return BadRequest();
            Product product = await _context.Products.Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true)).Include(p => p.ProductColors).Include(p => p.ProductSizes).FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();
            return View(product);
        }


        private async Task<List<Category>> GetCategoriesAsync()
        {
            List<Category> categories = await _context.Categories.ToListAsync();
            return categories;
        }

        private async Task<List<Color>> GetColorsAsync()
        {
            List<Color> colors = await _context.Colors.ToListAsync();
            return colors;
        }
        private async Task<List<Size>> GetSizesAsync()
        {
            List<Size> sizes = await _context.Sizes.ToListAsync();
            return sizes;
        }



    }
}