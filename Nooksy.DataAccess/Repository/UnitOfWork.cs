using Nooksy.DataAccess.Data;
using Nooksy.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nooksy.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {

        private readonly AppDbContext _db;
        public ICategoryRepository Category { get; private set; }
        public IProductRepository Product { get; private set; }

        public ICompanyRepository Company { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }

       public IOrderHeaderRepository orderHeader { get; private set; }
       public IOrderDetailRepository orderDetail { get; private set; }

       public IProductImageRepository ProductImage { get; private set; }

        public UnitOfWork(AppDbContext db) 
        {
            _db = db;
            Category = new CategoryRepository(_db);
            Product = new ProductRepository(_db);
            Company = new CompanyRepository(_db);
            ShoppingCart = new ShoppingCartRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
            orderDetail= new OrderDetailRepository(_db);
            orderHeader= new OrderHeaderRepository(_db);

            ProductImage= new ProductImageRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
