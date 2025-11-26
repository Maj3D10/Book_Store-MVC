using Fuzzy.DataAccess.Repository.IRepository;
using Fuzzy.Models;
using Fuzzy.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

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


            IEnumerable<Product> productList=_unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");
            return View(productList);
        } 
        public IActionResult Details(int ProductId)
        {

            ShoppingCart cart = new()
            {
               Product=_unitOfWork.Product.Get(u=>u.Id== ProductId, includeProperties: "Category,ProductImages"),
               Count=1,
               ProductId=ProductId
            };
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {

            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId=claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;


            shoppingCart.ApplicationUserId = userId;
            

            ShoppingCart shoppingcartFromDb=_unitOfWork.ShoppingCart.Get(u=>u.ApplicationUserId==userId &&
            u.ProductId==shoppingCart.ProductId);


            if (shoppingcartFromDb != null)
            {
                shoppingcartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(shoppingcartFromDb);
                _unitOfWork.Save();


            }
            else {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count());
            }

            TempData["success"] = "Cart update successfully";

          return RedirectToAction(nameof(Index));
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
