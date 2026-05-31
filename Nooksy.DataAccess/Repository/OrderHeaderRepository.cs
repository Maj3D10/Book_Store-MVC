using Nooksy.DataAccess.Data;
using Nooksy.DataAccess.Repository.IRepository;
using Nooksy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Nooksy.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly AppDbContext _db;
        public OrderHeaderRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }



        public void Update(OrderHeader obj)
        {
            _db.Update(obj);
        }

        public void UpdateStatus(int Id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == Id);
            if (orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentID(int Id, string sessionID, string? paymentIntenID = null)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == Id);

            if (!string.IsNullOrEmpty(sessionID))
            {
                orderFromDb.SessionId = sessionID;

            }

            if (!string.IsNullOrEmpty(paymentIntenID))
            {

                orderFromDb.PaymentIntenId = paymentIntenID;
                orderFromDb.PaymentDate = DateTime.Now;
            }

        }
    }
}
