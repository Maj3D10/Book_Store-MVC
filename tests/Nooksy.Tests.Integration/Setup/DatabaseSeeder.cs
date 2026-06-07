using Nooksy.DataAccess.Data;
using Nooksy.Models;

namespace Nooksy.Tests.Integration.Setup;

public static class DatabaseSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (!db.Categories.Any())
        {
            db.Categories.AddRange(
                new Category { Id = 1, Name = "Fiction", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Non-Fiction", DisplayOrder = 2 }
            );
        }

        if (!db.Products.Any())
        {
            db.Products.Add(new Product
            {
                Id = 1,
                Title = "Test Book",
                Author = "Author A",
                ISBN = "1234567890123",
                Description = "A test book for integration testing",
                ListPrice = 19.99,
                Price = 14.99,
                Price50 = 12.99,
                Price100 = 10.99,
                CategoryId = 1
            });
        }

        db.SaveChanges();
    }
}
