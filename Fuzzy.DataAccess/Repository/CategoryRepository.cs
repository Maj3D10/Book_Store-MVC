using Fuzzy.DataAccess.Data;
using Fuzzy.DataAccess.Repository.IRepository;
using Fuzzy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Fuzzy.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>,ICategoryRepository
    {
        private readonly AppDbContext _db;
        public CategoryRepository(AppDbContext db): base(db)
        {
            _db = db; 
        }

      

        public void Update(Category category)
        {
            _db.Update(category);
        }
    }
}
