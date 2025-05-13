using QuickMeds.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickMeds.Data;
using QuickMeds.Interfaces;
using QuickMeds.Models.DomainModels;
using QuickMeds.Models.JunctionModels;
using QuickMeds.Models.ViewModels;
using QuickMeds.Types;

namespace QuickMeds.Controllers
{
    public class ProductController : Controller
    {
        private readonly SqlDbContext dbContext;
        private readonly ITokenService tokenService;
        public ProductController(SqlDbContext dbContext, ITokenService tokenService)
        {
            this.dbContext = dbContext;
            this.tokenService = tokenService;
        }
        [HttpGet]
        public async Task<ActionResult> Index(ProductCategory category)
        {

            try
            {
                if (category == ProductCategory.All)
                {
                    var products = await dbContext.Products.Where(p => p.IsActive).ToListAsync();

                    var viewModel = new ProductViewModel
                    {
                        Products = products,
                    };

                    ViewBag.category = category.ToString();

                    return View(viewModel);
                }
                else
                {
                    var products = await dbContext.Products.Where(p => p.IsActive && p.Category == category).ToListAsync();

                    var viewModel = new ProductViewModel
                    {
                        Products = products,
                    };

                    ViewBag.category = category.ToString();

                    return View(viewModel);
                }


            }
            catch (System.Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                Console.WriteLine(ex.Message);
                return View("Error");
                throw;
            }
            // get all products

        }



        [HttpGet]
        public async Task<ActionResult> Details(Guid ProductId)
        {
            try
            {

                var product = await dbContext.Products.FirstOrDefaultAsync(p => p.ProductId == ProductId && p.IsActive);

                var viewModel = new ProductViewModel
                {
                    Product = product,
                };

                // ViewBag.SizeList = new SelectList(Enum.GetValues(typeof(ProductSize)));
                return View(viewModel);
            }
            catch (System.Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");

            }
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> AddToCart(Guid ProductId, int quantity, string size, string color)
        {
            try
            {
                Guid? userId = HttpContext.Items["UserId"] as Guid?;
                if (userId == null)
                {
                    ViewBag.ErrorMessage = "User not logged in.";
                    return View("Error");
                }

                var product = await dbContext.Products.FindAsync(ProductId);

                if (product == null)
                {
                    ViewBag.ErrorMessage = "Product not found.";
                    return View("Error");
                }

                decimal discountedPrice = product.OldPrice - (product.OldPrice * product.Discount / 100);

                var cart = await dbContext.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                // var cart = await dbContext.Carts
                //         .Include(c => c.CartItems)
                //         .ThenInclude(ci => ci.Product) // in case you want product price directly
                //         .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = (Guid)userId,
                        CartValue = 0
                    };
                    await dbContext.Carts.AddAsync(cart);
                    await dbContext.SaveChangesAsync();
                }

                var existingCartItem = await dbContext
                    .CartItems.FirstOrDefaultAsync(cp => cp.CartId == cart.CartId && cp.ProductId == ProductId);

                // var existingCartItem = cart.CartItems
                // .FirstOrDefault(ci => ci.ProductId == ProductId );

                // && ci.Size == size && ci.Color == color


                if (existingCartItem == null)
                {
                    var cartItem = new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = ProductId,
                        Quantity = quantity,
                    };

                    await dbContext.CartItems.AddAsync(cartItem);
                    // cart.CartItems.Add(cartItem);


                }
                else
                {
                    existingCartItem.Quantity += quantity;
                }

                cart.CartValue += discountedPrice * quantity;

                await dbContext.SaveChangesAsync();
                return RedirectToAction("Cart", "User");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }

        // check add to cart for differnet color and sizes
        [Authorize]
        [HttpGet]
        public async Task<ActionResult> AddToCart(Guid ProductId)
        {
            try
            {
                Guid? userId = HttpContext.Items["UserId"] as Guid?;
                if (userId == null)
                {
                    ViewBag.ErrorMessage = "User not logged in.";
                    return View("Error");
                }

                var product = await dbContext.Products.FindAsync(ProductId);

                if (product == null)
                {
                    ViewBag.ErrorMessage = "Product not found.";
                    return View("Error");
                }

                decimal discountedPrice = product.OldPrice - (product.OldPrice * product.Discount / 100);

                var cart = await dbContext.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                // var cart = await dbContext.Carts
                //         .Include(c => c.CartItems)
                //         .ThenInclude(ci => ci.Product) // in case you want product price directly
                //         .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = (Guid)userId,
                        CartValue = 0
                    };
                    await dbContext.Carts.AddAsync(cart);
                    await dbContext.SaveChangesAsync();
                }

                var existingCartItem = await dbContext
                    .CartItems.FirstOrDefaultAsync(cp => cp.CartId == cart.CartId && cp.ProductId == ProductId);

                // var existingCartItem = cart.CartItems
                // .FirstOrDefault(ci => ci.ProductId == ProductId );

                // && ci.Size == size && ci.Color == color


                if (existingCartItem == null)
                {
                    var cartItem = new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = ProductId,
                        Quantity = 1,
                    };

                    await dbContext.CartItems.AddAsync(cartItem);
                    // cart.CartItems.Add(cartItem);


                }
                else
                {
                    existingCartItem.Quantity += 1;
                }

                cart.CartValue += discountedPrice * 1;

                await dbContext.SaveChangesAsync();
                return RedirectToAction("Cart", "User");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }




    }
}





    
