using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuickMeds.Data;
using QuickMeds.Interfaces;
using QuickMeds.Middlewares;
using QuickMeds.Models.DomainModels;
using QuickMeds.Models.ViewModels;
using QuickMeds.Types;

namespace QuickMeds.Controllers
{
    public class AdminController : Controller
    {
        private readonly ITokenService tokenService;
        private readonly SqlDbContext dbContext;
        private readonly ICloudinaryService cloudinary;

        public AdminController(SqlDbContext dbContext, ITokenService tokenService, ICloudinaryService cloudinary)
        {
            this.tokenService = tokenService;
            this.dbContext = dbContext;
            this.cloudinary = cloudinary;
        }



        [Authorize]
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            Guid? userId = HttpContext.Items["UserId"] as Guid?;

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user?.Role != Role.Admin)
            {
                return RedirectToAction("Login", "User");
            }
            var usersCount = await dbContext.Users.CountAsync();
            var ordersCount = await dbContext.Orders.CountAsync();
            var productsCount = await dbContext.Products.CountAsync();

            ViewBag.TotalUsers = usersCount;
            ViewBag.TotalOrders = ordersCount;
            ViewBag.TotalProducts = productsCount;


            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> CreateProduct()
        {

             Guid? userId = HttpContext.Items["UserId"] as Guid?;

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user?.Role != Role.Admin)
            {
                return RedirectToAction("Login", "User");
            }
            ViewBag.CategoryList = new SelectList(Enum.GetValues(typeof(ProductCategory)));
        
            return View();
        }



        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateProduct(Product product, IFormFile ImageFile)
        {

            try
            {
                ViewBag.CategoryList = new SelectList(Enum.GetValues(typeof(ProductCategory)));

                /*if (!ModelState.IsValid)
                {
                    ViewBag.ErrorMessage = "Invalid Product Data";
                    return View();
                }*/

                if (ImageFile != null && ImageFile.Length > 0)
                {

                    var uploadResult = await cloudinary.UploadImageAsync(ImageFile);
                    if (uploadResult != null)
                    {
                        product.ImageUrl = uploadResult;
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Image Upload Failed";
                        return View();
                    }

                }


                await dbContext.Products.AddAsync(product);
                await dbContext.SaveChangesAsync();


                
                TempData["SuccessMessage"] = "Product Created Successfully";
                return RedirectToAction("Index");


            }
            catch (System.Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");

            }

        }

        [HttpGet]
        public async Task<ActionResult> ProductList()
        {
            try
            {
                var products = await dbContext.Products.ToListAsync();

                var viewModel = new ProductViewModel
                {
                    Products = products
                };
                return View(viewModel);
            }
            catch (System.Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");

            }

        }

        [HttpGet]
        public async Task<ActionResult> OrderList()
        {

            try
            {
                var orders = await dbContext.Orders.ToListAsync();

                var viewModel = new OrderViewModel
                {
                    Orders = orders
                };
                return View(viewModel);
            }
            catch (System.Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");

            }

        }

        [HttpGet]
        public async Task<ActionResult> UserDb()
        {

            try
            {
                var users = await dbContext.Users.ToListAsync();

                var viewModel = new UserViewModel
                {
                    Users = users
                };
                return View(viewModel);
            }
            catch (System.Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");

            }

        }


    }
}
    
