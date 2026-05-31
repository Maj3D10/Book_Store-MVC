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
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private readonly AppDbContext _db;
        public OrderDetailRepository(AppDbContext db): base(db)
        {
            _db = db; 
        }

      

        public void Update(OrderDetail obj)
        {
            _db.Update(obj);
        }
    }
}
