using Nooksy.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nooksy.Models;
using Nooksy.DataAccess.Data;

namespace Nooksy.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly AppDbContext _db;

        public ShoppingCartRepository(AppDbContext db):base(db)
        {
            _db = db;
        }
        public void Update(ShoppingCart obj)
        {
            _db.Update(obj);

        }
    }
}
