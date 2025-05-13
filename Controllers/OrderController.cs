using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickMeds.Data;
using QuickMeds.Interfaces;
using QuickMeds.Middlewares;
using QuickMeds.Models.ViewModels;

namespace QuickMeds.Controllers
{
    public class OrderController : Controller
    {
        private readonly SqlDbContext dbContext;
        private readonly IMailService mailService;
        public OrderController(SqlDbContext dbContext, IMailService mailService)
        {

            this.dbContext = dbContext;
            this.mailService = mailService;

        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CheckOut(Guid CartId)
        {

            try
            {
                Guid? userId = HttpContext.Items["UserId"] as Guid?;




                var cart = await dbContext.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.CartId == CartId); // finding cart of user 

                if (cart == null || cart.CartValue == 0)
                {
                    return RedirectToAction("Cart", "User");
                }


                var address = await dbContext.Addresses.FirstOrDefaultAsync(a => a.UserId == userId);


                var cartItems = await dbContext.CartItems
                .Include(cp => cp.Product)
                .Where(cp => cp.CartId == cart.CartId)
                .ToListAsync();

                var viewModel = new HybridViewModel
                {
                    CartItems = cartItems,
                    Cart = cart,
                    Address = address
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