using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickMeds.Middlewares;
using QuickMeds.Data;
using QuickMeds.Models;
using QuickMeds.Models.ViewModels;
using QuickMeds.Types;

namespace QuickMeds.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SqlDbContext dbContext;
    public HomeController(ILogger<HomeController> logger, SqlDbContext dbContext)
    {
        _logger = logger;
        this.dbContext = dbContext;
    }

    public  IActionResult Index()
    {
   
            return View();
  
    }


    [HttpGet]
    public async Task<IActionResult> LoadTab()
    {
        try
        {
            var products = await dbContext.Products.Where(p => p.IsActive && p.Category == ProductCategory.MenGrooming ).ToListAsync();

            if (products == null)
            {
                ViewBag.Message = "No Products in this Category";
            }

            var viewModel = new ProductViewModel
            {
                Products = products
            };

            return PartialView("~/Views/Shared/molecules/Tab.cshtml", viewModel);
        }
        catch (System.Exception ex)
        {

            ViewBag.ErrorMessage = ex.Message;
            return View("Error");
        }


    }

 


    [Authorize]
    [HttpGet]
    public async Task<IActionResult> UserIndex()
    {
        try
        {
            Guid? userId = HttpContext.Items["UserId"] as Guid?;

            // fetch products from the database

            var user = await dbContext.Users.FindAsync(userId);


            var products = await dbContext.Products.Where(p => p.IsActive).ToListAsync();

            if (user == null || products == null)
            {

                ViewBag.ErrorMessage = "Something Went Wrong . Try again after Sometime";
                return View("Error");
            }


            var viewModel = new ProductViewModel
            {
                Products = products,
                User = user
            };

            return View(viewModel);
        }
        catch (System.Exception ex)
        {
            // Log the exception
            ViewBag.ErrorMessage = ex.Message;
            return View("Error");

        }

    }

    public IActionResult Privacy()
    {
        return View();
    }
    public IActionResult Contact()
    {
        return View();
    }
    public IActionResult About()
    {
        return View();
    }
     public IActionResult TermsAndConditions()
    {
        return View();
    }
     public IActionResult Help()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
