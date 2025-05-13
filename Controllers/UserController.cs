using QuickMeds.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickMeds.Data;
using QuickMeds.Interfaces;
using QuickMeds.Models.DomainModels;
using QuickMeds.Models.ViewModels;
using QuickMeds.Types;

namespace QuickMeds.Controllers
{
    public class UserController : Controller
    {
        private readonly SqlDbContext dbContext;    // encapsulated feilds
        private readonly ITokenService tokenService;

        public UserController(SqlDbContext dbContext, ITokenService tokenService)
        {
            this.dbContext = dbContext;
            this.tokenService = tokenService;
        }
        [HttpGet]
        public ActionResult Account()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Register(User user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.errorMessage = "All details Required!";
                    return View();
                }
                //   var existingUser = await sqlDbContext.Users.FindAsync(user.UserId);   // findAsync is for PK

                var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);   // findAsync is for PK



                if (existingUser != null)
                {

                    ViewBag.errorMessage = "User Already Exists";
                    return View();

                }

                var encryptPass = BCrypt.Net.BCrypt.HashPassword(user.Password);

                user.Password = encryptPass;



                var newUser = await dbContext.Users.AddAsync(user);
                await dbContext.SaveChangesAsync();


                ViewBag.successMessage = "User Created Succefully!";

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = ex.Message;
                return View("Error");
            }


        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginView model)
        {
           /* if (string.IsNullOrEmpty(model.Email))
            {
                ViewBag.errorMessage = "Email feild is required!";
                return View();
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                ViewBag.errorMessage = "Password Feild is Required!";
                return View();
            }


            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);   // db query 

            if (user == null)
            {
                ViewBag.errorMessage = "User Not Found!";
                return View();
            }

            var passVerify = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);

            if (passVerify)
            {

                var token = tokenService.CreateToken(user.UserId, user.Email, user.Username, 60 * 24);

                HttpContext.Response.Cookies.Append(
                    "QuikMedsToken",
                    token,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddHours(24)
                    }
                );

                ViewBag.successMessage = "Logged in succesfully!";
                return View();
            }

            ViewBag.errorMessage = "Password incorrect!";
            return View();*/
        

            try
            {

                if (!ModelState.IsValid)
                {
                    ViewBag.errorMessage = "All credentials Required!";
                    return View();
                }

                var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);


                if (existingUser == null)
                {

                    ViewBag.errorMessage = "User not Found!";
                    return View();

                }

                var checkPass = BCrypt.Net.BCrypt.Verify(model.Password, existingUser.Password);

                if (checkPass)
                {

                    var token = tokenService.CreateToken(existingUser.UserId, model.Email, existingUser.Username, 60 * 24);

                    //    Console.WriteLine(token);

                    HttpContext.Response.Cookies.Append("AuthorizationToken", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddHours(72)
                    });


                    var returnUrl = HttpContext.Session.GetString("ReturnUrl");


                    HttpContext.Session.Remove("ReturnUrl");
                    HttpContext.Session.SetString("UserId", existingUser.UserId.ToString());



                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }


                    else
                    {

                        return RedirectToAction("UserIndex", "Home");
                    }
                }
                else
                {
                    ViewBag.errorMessage = "PassWord incorrect!";
                    return View();
                }

            }
            catch (Exception ex)
            {

                ViewBag.errorMessage = ex.Message;
                return View("Error");
            }

        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> Cart()
        {
            try
            {

                Guid? userId = HttpContext.Items["UserId"] as Guid?;

                var cart = await dbContext.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId); // finding cart of user 

                var viewModel = new CartViewModel();


                if (cart == null || cart.CartItems.Count == 0)
                {
                    ViewBag.CartEmpty = "Your Cart is Empty";    // used in if condition
                    return View(viewModel);
                }

                // for efficency there is serperated cart profucts db query
                var cartItems = await dbContext.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cart.CartId)
                .ToListAsync();


                viewModel.CartItems = cartItems;
                viewModel.Cart = cart;


                return View(viewModel);
            }
            catch (System.Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");

            }

        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Orders(OrderStatus OrderFilter)
        {


            try
            {
                Guid? userId = HttpContext.Items["UserId"] as Guid?;

                // var orders = await dbContext.Orders.Where(o => o.BuyerId == userId).ToListAsync();// finding order using orderId

                //  var orderproducts = await dbContext.OrderProducts
                // .Include(op => op.Product)
                // .Where(op => orders.Select(o=>o.OrderId).Contains(op.OrderId))
                // .ToListAsync();
                if (OrderFilter == OrderStatus.Cancelled)
                {
                    var orders = await dbContext.Orders
                                      .Include(o => o.OrderItems)  // Include OrderProducts
                                      .ThenInclude(op => op.Product)  // Include related Product
                                      .Where(o => o.UserId == userId && o.OrderStatus == OrderFilter)
                                      .ToListAsync();

                    var viewModel = new OrderViewModel
                    {
                        Orders = orders
                    };
                    ViewBag.OrderFilter = "Cancelled";
                    return View(viewModel);
                }
                else if (OrderFilter == OrderStatus.InTransit)
                {

                    var orders = await dbContext.Orders
                                  .Include(o => o.OrderItems)  // Include OrderProducts
                                  .ThenInclude(op => op.Product)  // Include related Product
                                  .Where(o => o.UserId == userId && o.OrderStatus == OrderFilter)
                                  .ToListAsync();

                    var viewModel = new OrderViewModel
                    {
                        Orders = orders
                    };
                    ViewBag.OrderFilter = "In Transit";
                    return View(viewModel);
                }
                else
                {
                    var orders = await dbContext.Orders
                                 .Include(o => o.OrderItems)  // Include OrderProducts
                                 .ThenInclude(op => op.Product)  // Include related Product
                                 .Where(o => o.UserId == userId && o.OrderStatus != OrderStatus.Cancelled)
                                 .ToListAsync();

                    var viewModel = new OrderViewModel
                    {
                        Orders = orders
                    };
                    ViewBag.OrderFilter = "Recent";
                    return View(viewModel);
                }
            }
            catch (System.Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
                throw;
            }

        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateAddress(Address address)
        {

            try
            {
                Guid? userId = HttpContext.Items["UserId"] as Guid?;
                if (userId == null)
                {
                    return RedirectToAction("Login", "User");
                }

                var availableAdderess = await dbContext.Addresses.FirstOrDefaultAsync(a => a.UserId == userId);


                if (ModelState.IsValid)
                {
                    address.UserId = (Guid)userId;
                    await dbContext.Addresses.AddAsync(address);
                    await dbContext.SaveChangesAsync();
                    return RedirectToAction("CheckOut", "Order");
                }

                TempData["ErrorMessage"] = "Address updation un-successfull !";
                return RedirectToAction("CheckOut", "Order");

            }
            catch (System.Exception ex)
            {

                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }

        }
        [HttpGet]
        public ActionResult Logout()
        {
            HttpContext.Response.Cookies.Delete("AuthorizationToken");
            HttpContext.Session.Clear();

            TempData["LogoutMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }


    }


}






