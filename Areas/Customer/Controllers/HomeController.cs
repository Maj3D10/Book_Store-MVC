using System.Diagnostics;
using Fuzzy.DataAccess.Repository.IRepository;
using Fuzzy.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace FuzzyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]

    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
           
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList=_unitOfWork.Product.GetAll(includeProperties: "Category");
            return View(productList);
        } 
        public IActionResult Details(int ProductId)
        {
            Product product=_unitOfWork.Product.Get(u=>u.Id== ProductId, includeProperties: "Category");
            return View(product);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult RedirectToGitHubRepo()
        {
            // Redirect to a GitHub repository URL
            return Redirect("https://github.com/Maj3D10/Fuzzy_Web-MVC");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
