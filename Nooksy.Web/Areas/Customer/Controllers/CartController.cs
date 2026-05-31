using Nooksy.DataAccess.Repository.IRepository;
using Nooksy.Models;
using Nooksy.Models.ViewModel;
using Nooksy.Utility;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace Nooksy.Web.Areas.Customer.Controllers
{
    [Area("Customer")]

    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        [BindProperty]
        public ShoppingCartVM shoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),

                orderheader = new()
            };
            IEnumerable<ProductImage> productImages = _unitOfWork.ProductImage.GetAll();


            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Product.ProductImages = productImages.Where(u => u.ProductId == cart.Product.Id).ToList();


                cart.Price = PricingRules.GetPriceBasedOnQuantity(cart.Product, cart.Count);
                shoppingCartVM.orderheader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(shoppingCartVM);
        }
        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            cartFromDb.Count++;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            if (cartFromDb.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDb);

                HttpContext.Session.SetInt32(SD.SessionCart,
              _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
            }
            else
            {
                cartFromDb.Count--;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));

        }

        public IActionResult Remove(int cartId)
        {


            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);




            _unitOfWork.ShoppingCart.Remove(cartFromDb);


            HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart
           .GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);


            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));

        }
        public IActionResult Summary()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),

                orderheader = new()
            };
            shoppingCartVM.orderheader.appUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);


            shoppingCartVM.orderheader.Name = shoppingCartVM.orderheader.appUser.Name;
            shoppingCartVM.orderheader.PhoneNumber = shoppingCartVM.orderheader.appUser.PhoneNumber;
            shoppingCartVM.orderheader.City = shoppingCartVM.orderheader.appUser.City;
            shoppingCartVM.orderheader.StreetAddress = shoppingCartVM.orderheader.appUser.StreetAddress;
            shoppingCartVM.orderheader.State = shoppingCartVM.orderheader.appUser.State;
            shoppingCartVM.orderheader.PostalCode = shoppingCartVM.orderheader.appUser.PostalCode;



            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = PricingRules.GetPriceBasedOnQuantity(cart.Product, cart.Count);
                shoppingCartVM.orderheader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(shoppingCartVM);
        }




        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");

            shoppingCartVM.orderheader.OrderDate = System.DateTime.Now;
            shoppingCartVM.orderheader.ApplicationUserId = userId;


            ApplicationUser appUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = PricingRules.GetPriceBasedOnQuantity(cart.Product, cart.Count);
                shoppingCartVM.orderheader.OrderTotal += (cart.Price * cart.Count);
            }

            if (appUser.CompanyId.GetValueOrDefault() == 0)
            {
                shoppingCartVM.orderheader.OrderStatus = SD.StatusPending;
                shoppingCartVM.orderheader.PaymentStatus = SD.PaymentStatusPending;
            }
            else
            {
                shoppingCartVM.orderheader.OrderStatus = SD.StatusApproved;
                shoppingCartVM.orderheader.PaymentStatus = SD.PaymentStatusDelayedPayment;
            }
            _unitOfWork.orderHeader.Add(shoppingCartVM.orderheader);
            _unitOfWork.Save();

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = shoppingCartVM.orderheader.Id,
                    Price = cart.Price,
                    Count = cart.Count,
                };
                _unitOfWork.orderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }
            if (appUser.CompanyId.GetValueOrDefault() == 0)
            {
                var stripeSecretKey = _configuration.GetSection("Stripe:SecretKey").Get<string>();
                if (string.IsNullOrWhiteSpace(stripeSecretKey))
                {
                    throw new InvalidOperationException("Stripe:SecretKey must be configured before starting a checkout session.");
                }

                string domain = GetCheckoutBaseUrl();

                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = domain+$"Customer/Cart/OrderConfirmation?id={shoppingCartVM.orderheader.Id}",
                    CancelUrl = domain+$"Customer/Cart/Index",
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                  
                    Mode = "payment",
                };

                foreach (var item in shoppingCartVM.ShoppingCartList)
                {
                    var SessionLineItem = new SessionLineItemOptions()
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData=new SessionLineItemPriceDataProductDataOptions
                            {
                                Name=item.Product.Title
                            }
                        },
                        Quantity=item.Count
                    };
                    options.LineItems.Add(SessionLineItem);
                }



                var service = new Stripe.Checkout.SessionService();
                Stripe.Checkout.Session session = service.Create(options);
                _unitOfWork.orderHeader.UpdateStripePaymentID(shoppingCartVM.orderheader.Id,session.Id,session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Location = session.Url;
                return new StatusCodeResult(303);
            }



            return RedirectToAction(nameof(OrderConfirmation), new { id = shoppingCartVM.orderheader.Id });
        }

        public IActionResult OrderConfirmation(int Id)
        {

            OrderHeader orderHeader = _unitOfWork.orderHeader.Get(u=>u.Id==Id,includeProperties : "appUser");
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                  var service =new SessionService();
                Session session=service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.orderHeader.UpdateStripePaymentID(Id, session.Id, session.PaymentIntentId);

                    _unitOfWork.orderHeader.UpdateStatus(Id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
                HttpContext.Session.Clear();
            }
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();

            return View(Id);
        }

        private string GetCheckoutBaseUrl()
        {
            var configuredBaseUrl = _configuration.GetSection("Checkout:BaseUrl").Get<string>();
            var baseUrl = string.IsNullOrWhiteSpace(configuredBaseUrl)
                ? $"{Request.Scheme}://{Request.Host}"
                : configuredBaseUrl.TrimEnd('/');

            return $"{baseUrl}/";
        }


    }
}
