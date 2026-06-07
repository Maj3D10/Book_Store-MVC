using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Nooksy.DataAccess.Data;
using Nooksy.DataAccess.Repository.IRepository;
using Nooksy.Models;
using Nooksy.Tests.Integration.Setup;
using Xunit;

namespace Nooksy.Tests.Integration.Admin;

public class CategoryCrudTests : IClassFixture<NooksyWebAppFactory>
{
    private readonly NooksyWebAppFactory _factory;

    public CategoryCrudTests(NooksyWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddCategory_PersistsToDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var category = new Category { Name = "Test Category", DisplayOrder = 5 };
        uow.Category.Add(category);
        uow.Save();

        var saved = await db.Categories.FindAsync(category.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Test Category");
    }

    [Fact]
    public async Task UpdateCategory_ReflectsInGetAll()
    {
        using var scope = _factory.Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var category = new Category { Name = "Original", DisplayOrder = 1 };
        uow.Category.Add(category);
        uow.Save();

        category.Name = "Updated";
        uow.Category.Update(category);
        uow.Save();

        var all = uow.Category.GetAll().ToList();
        all.Should().Contain(c => c.Name == "Updated");
        all.Should().NotContain(c => c.Name == "Original");
    }

    [Fact]
    public async Task RemoveCategory_NoLongerInGetAll()
    {
        using var scope = _factory.Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var category = new Category { Name = "ToDelete", DisplayOrder = 99 };
        uow.Category.Add(category);
        uow.Save();

        uow.Category.Remove(category);
        uow.Save();

        var all = uow.Category.GetAll().ToList();
        all.Should().NotContain(c => c.Name == "ToDelete");
    }
}
