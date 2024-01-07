using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiShop.Areas.MultiShopAdmin.Models;
using MultiShop.Areas.MultiShopAdmin.Models.Utilities.Enums;
using MultiShop.Areas.MultiShopAdmin.ViewModels;
using MultiShop.DAL;
using MultiShop.Models;
using MultiShop.ViewModels;

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
            CreateProductVM vm = new()
            {
                Categories = await GetCategoriesAsync(),
                Sizes = await GetSizesAsync(),
                Colors = await GetColorsAsync(),
            };
            return View(vm);

        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            if (!ModelState.IsValid) return View();
            bool result = _context.Categories.Any(c => c.Id == productVM.CategoryId);
            if (!result)
            {

                productVM.Categories = await GetCategoriesAsync();
                productVM.Sizes = await GetSizesAsync();
                productVM.Colors = await GetColorsAsync();
                
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
                Discount = productVM.Discount,
                CategoryId = productVM.CategoryId,
                ProductColors = new List<ProductColor>(),
                ProductImages = new List<ProductImage> { MainPhoto}

            };
            foreach (int colorId in productVM.ColorIds)
            {
                bool colresult = await _context.Colors.AnyAsync(c => c.Id == colorId);
                if (!colresult)
                {
                    ViewBag.Categories = GetCategoriesAsync();
                    ViewBag.Colors = GetColorsAsync();
                    ViewBag.Sizes =  GetSizesAsync();
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
                Discount = existed.Discount,
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
        public async Task<IActionResult> Update(int id,UpdateProductVM vm)
        {
            if(id<=0) return BadRequest();

            Product existed = await _context.Products.Include(p => p.ProductImages).Include(p => p.Category).Include(p => p.ProductColors).Include(p => p.ProductSizes).FirstOrDefaultAsync(p => p.Id == id);

            if (!ModelState.IsValid)
            {
                vm.Categories = await GetCategoriesAsync();
                vm.Colors= await GetColorsAsync();
                vm.Sizes= await GetSizesAsync();
                vm.ProductImages = existed.ProductImages;
                return View(vm);
            }

            if(vm.Name.ToLower().Trim() != existed.Name.ToLower().Trim())
            {
                if(await _context.Products.AnyAsync(p=>p.Name == vm.Name))
                {
                    ModelState.AddModelError("Name", "This product with this name already exists");
                    vm.Categories = await GetCategoriesAsync();
                    vm.Colors= await GetColorsAsync();
                    vm.Sizes= await GetSizesAsync();
                    vm.ProductImages= existed.ProductImages;
                    return View(vm);
                }
            }

            if(vm.CategoryId != existed.CategoryId)
            {
                if(!await _context.Categories.AnyAsync(c=>c.Id == existed.CategoryId))
                {
                    ModelState.AddModelError("CatId", "This category doesn't exist, please try again");
                    vm.Categories = await GetCategoriesAsync();
                    vm.Colors= await GetColorsAsync();
                    vm.Sizes= await GetSizesAsync();
                    vm.ProductImages = existed.ProductImages;
                    return View(vm);
                }

                existed.CategoryId = vm.CategoryId;
            }

            if(vm.MainPhoto is not null)
            {
                if (!vm.MainPhoto.IsValidType(FileType.Image))
                {
                    ModelState.AddModelError("MainPhoto", "Photo should be image type");
                    vm.Categories = await GetCategoriesAsync();
                    vm.Colors= await GetColorsAsync();
                    vm.Sizes= await GetSizesAsync();
                    vm.ProductImages = existed.ProductImages;
                    return View(vm);
                }

                if (!vm.MainPhoto.IsValidSize(5, FileSize.Megabite))
                {
                    ModelState.AddModelError("MainPhoto", "Photo should be 5 mb");
                    vm.Categories = await GetCategoriesAsync();
                    vm.Colors = await GetColorsAsync();
                    vm.Sizes = await GetSizesAsync();
                    vm.ProductImages = existed.ProductImages;
                    return View(existed);
                }
                ProductImage mainPhoto = existed.ProductImages.FirstOrDefault(pi=>pi.IsPrimary==true);
                mainPhoto.URL.Delete(_env.WebRootPath, "img");
                mainPhoto.URL = await vm.MainPhoto.CreateAsync(_env.WebRootPath, "img");

            }

            if(vm.HoverPhoto is not null)
            {
                if (!vm.HoverPhoto.IsValidType(FileType.Image))
                {
                    ModelState.AddModelError("HoverPhoto", "Photo should be image type");
                    vm.Categories = await GetCategoriesAsync();
                    vm.Colors = await GetColorsAsync();
                    vm.Sizes = await GetSizesAsync();
                    vm.ProductImages = existed.ProductImages;
                    return View(vm);
                }
                if (!vm.HoverPhoto.IsValidSize(5, FileSize.Megabite))
                {
                    ModelState.AddModelError("HoverPhoto", "Photo should be 5 mb");
                    vm.Categories = await GetCategoriesAsync();
                    vm.Colors = await GetColorsAsync();
                    vm.Sizes = await GetSizesAsync();
                    vm.ProductImages = existed.ProductImages;
                    return View(existed);
                }

                ProductImage hoverPhoto = existed.ProductImages.FirstOrDefault(pi => pi.IsPrimary == false);
                hoverPhoto.URL.Delete(_env.WebRootPath, "img");
                hoverPhoto.URL = await vm.HoverPhoto.CreateAsync(_env.WebRootPath, "img");


            }


            if (vm.ColorIds is not null)
            {
                foreach (int colId in vm.ColorIds)
                {
                    if (!await _context.Colors.AnyAsync(t => t.Id == colId))
                    {
                        ModelState.AddModelError("ColIds", "This color doesn't exist,please try again");
                        vm.Categories = await GetCategoriesAsync();
                        vm.Colors = await GetColorsAsync();
                        vm.Sizes = await GetSizesAsync();
                        vm.ProductImages = existed.ProductImages;
                        return View(vm);
                    }
                    if (!existed.ProductColors.Exists(pc => pc.ColorId == colId))
                    {
                        existed.ProductColors.Add(new ProductColor { ColorId = colId });
                    }

                    existed.ProductColors = existed.ProductColors.Where(pc => vm.ColorIds.Exists(id => id== pc.ColorId)).ToList();

                }
            }
            if(vm.SizeIds is not null)
            {
                foreach(int sizeId in  vm.SizeIds)
                {
                    if(!await _context.Sizes.AnyAsync(s=>s.Id== sizeId))
                    {
                        ModelState.AddModelError("SizeIds", "This size doesn't exist,please try again");
                        vm.Categories = await GetCategoriesAsync();
                        vm.Colors = await GetColorsAsync();
                        vm.Sizes = await GetSizesAsync();
                        vm.ProductImages = existed.ProductImages;
                        return View(vm);
                    }

                    if (!existed.ProductSizes.Exists(pc => pc.SizeId == sizeId))
                    {
                        existed.ProductSizes.Add(new ProductSize { SizeId = sizeId });
                    }

                    existed.ProductSizes = existed.ProductSizes.Where(pc => vm.SizeIds.Exists(id => id == pc.SizeId)).ToList();

                }
            }
            
            existed.Name = vm.Name;
            existed.Description = vm.Description;
            existed.Price = vm.Price;
            existed.Discount = vm.Discount;

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
            Product product = await _context.Products.Include(p=>p.Category).Include(p => p.ProductImages).Include(p => p.ProductColors).ThenInclude(pc=>pc.Color).Include(p => p.ProductSizes).ThenInclude(ps=>ps.Size).FirstOrDefaultAsync(p => p.Id == id);
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