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
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        private readonly AppDbContext _db;
        public ProductImageRepository(AppDbContext db): base(db)
        {
            _db = db; 
        }

      

        public void Update(ProductImage obj)
        {
            _db.Update(obj);
        }
    }
}
