using Nooksy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nooksy.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);
        void UpdateStatus(int Id, string orderStatus,string?paymentStatus=null);
        void UpdateStripePaymentID(int Id, string sessionID,string?paymentIntenID=null);


    }
}
