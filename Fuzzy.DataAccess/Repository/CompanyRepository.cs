using Fuzzy.DataAccess.Data;
using Fuzzy.DataAccess.Repository.IRepository;
using Fuzzy.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuzzy.DataAccess.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private readonly AppDbContext _db;
        public CompanyRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Company company)
        {
            _db.Update(company);
        }
    }
    


   
}

