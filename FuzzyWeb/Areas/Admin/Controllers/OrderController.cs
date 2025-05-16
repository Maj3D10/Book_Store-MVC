using Fuzzy.DataAccess.Repository;
using Fuzzy.DataAccess.Repository.IRepository;
using Fuzzy.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace FuzzyBook.Web.Areas.Admin.Controllers
{
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork ;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ;
        }

        public IActionResult Index()
        {
            return View();
        }
        #region API Calls

        [HttpGet]
        public IActionResult getall()
        {
            List<OrderHeader> ordertList = _unitOfWork.orderHeader.GetAll(includeProperties: "appUser").ToList();
            return Json(new { data = ordertList });
        }
        #endregion

    }
}
