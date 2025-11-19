using Fuzzy.DataAccess.Repository;
using Fuzzy.DataAccess.Repository.IRepository;
using Fuzzy.Models;
using Fuzzy.Models.ViewModel;
using Fuzzy.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe.Climate;
using System.Security.Claims;

namespace FuzzyBook.Web.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize]

    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork ;
        [BindProperty]
        public OrderVM OrderVM { get; set; }


        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Details(int orderId)
        {
            OrderVM= new()
            {
                orderheader = _unitOfWork.orderHeader.Get(u => u.Id == orderId, includeProperties: "appUser"),
                orderdetail = _unitOfWork.orderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "product")


            };
            return View(OrderVM);
        }



        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.orderHeader.Get(u => u.Id == OrderVM.orderheader.Id);
            orderHeaderFromDb.Name = OrderVM.orderheader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.orderheader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.orderheader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.orderheader.City;
            orderHeaderFromDb.State = OrderVM.orderheader.State;
            orderHeaderFromDb.PostalCode = OrderVM.orderheader.PostalCode;
            if (!string.IsNullOrEmpty(OrderVM.orderheader.Carrier))
            {
                orderHeaderFromDb.Carrier = OrderVM.orderheader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.orderheader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.orderheader.TrackingNumber;
            }
            _unitOfWork.orderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Order Details Updated Successfully.";


            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.orderHeader.UpdateStatus(OrderVM.orderheader.Id,SD.StatusInProcess);
            _unitOfWork.Save();

            TempData["Success"] = "Order Details Updated Successfully";

            return RedirectToAction(nameof(Details), new {orderId=OrderVM.orderheader.Id});


        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {

            var orderHeader = _unitOfWork.orderHeader.Get(u => u.Id == OrderVM.orderheader.Id);
            orderHeader.TrackingNumber = OrderVM.orderheader.TrackingNumber;
            orderHeader.Carrier = OrderVM.orderheader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            _unitOfWork.orderHeader.Update(orderHeader);
            _unitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderheader.Id });
        }   



        #region API Calls

        [HttpGet]
        public IActionResult getall(string status)
        {
            IEnumerable<OrderHeader> objorderHeaders;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {

                objorderHeaders= _unitOfWork.orderHeader.GetAll(includeProperties: "appUser").ToList();
            }
            else
            {
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objorderHeaders = _unitOfWork.orderHeader.GetAll(u=>u.ApplicationUserId==userId,includeProperties: "appUser").ToList();


            }





            switch (status)
                {
                    case "pending":
                        objorderHeaders = objorderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                        break;
                    case "inprocess":
                        objorderHeaders = objorderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                        break;
                    case "completed":
                        objorderHeaders = objorderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                        break;
                    case "approved":
                        objorderHeaders = objorderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                        break;
                    default:
                        break;

                }


            return Json(new { data = objorderHeaders });
        }
        #endregion

    }
}
