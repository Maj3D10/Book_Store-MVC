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
    public class ApplicationUserRepository : Repository<ApplicationUser>,IApplicationUserRepository
    {
        private readonly AppDbContext _db;
        public ApplicationUserRepository(AppDbContext db): base(db)
        {
            _db = db; 
        }

      

     
    }
}
