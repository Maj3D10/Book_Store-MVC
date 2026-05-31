using Nooksy.DataAccess.Data;
using Nooksy.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nooksy.Models;

namespace Nooksy.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly AppDbContext _db;
        public ProductRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Product product)
        {
            Product productFromDb=_db.Products.FirstOrDefault(s => s.Id == product.Id);
            if (productFromDb != null)
            {
                productFromDb.Title = product.Title;
                productFromDb.Price = product.Price;
                productFromDb.CategoryId = product.CategoryId;
                productFromDb.Description = product.Description;
                productFromDb.Author = product.Author;
                productFromDb.ISBN = product.ISBN;
                productFromDb.ListPrice = product.ListPrice;
                productFromDb.Price50 = product.Price50;
                productFromDb.Price100 = product.Price100;

                productFromDb.ProductImages = product.ProductImages;
                //if (product.ImageUrl != null)
                //{
                //    productFromDb.ImageUrl = product.ImageUrl;
                //}

            }
        }
    }
}
