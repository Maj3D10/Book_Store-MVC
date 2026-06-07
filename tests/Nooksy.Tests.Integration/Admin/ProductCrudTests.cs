using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Nooksy.DataAccess.Data;
using Nooksy.DataAccess.Repository.IRepository;
using Nooksy.Models;
using Nooksy.Tests.Integration.Setup;
using Xunit;

namespace Nooksy.Tests.Integration.Admin;

public class ProductCrudTests : IClassFixture<NooksyWebAppFactory>
{
    private readonly NooksyWebAppFactory _factory;

    public ProductCrudTests(NooksyWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddProduct_WithValidData_PersistsWithCategoryFk()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var product = new Product
        {
            Title = "New Product",
            Author = "Author",
            ISBN = "9876543210987",
            Description = "Description",
            ListPrice = 29.99,
            Price = 24.99,
            Price50 = 22.99,
            Price100 = 19.99,
            CategoryId = 1
        };

        uow.Product.Add(product);
        uow.Save();

        var saved = await db.Products.FindAsync(product.Id);
        saved.Should().NotBeNull();
        saved!.CategoryId.Should().Be(1);
    }

    [Fact]
    public async Task GetAllWithIncludes_LoadsNavigationProperties()
    {
        using var scope = _factory.Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // The seeded product has CategoryId = 1
        var products = uow.Product.GetAll(includeProperties: "Category").ToList();

        products.Should().NotBeEmpty();
        products.First().Category.Should().NotBeNull();
        products.First().Category.Name.Should().Be("Fiction");
    }
}
