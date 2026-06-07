using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Nooksy.DataAccess.Repository.IRepository;
using Nooksy.Models;
using Nooksy.Tests.Integration.Setup;
using Xunit;

namespace Nooksy.Tests.Integration.Customer;

public class CartFlowTests : IClassFixture<NooksyWebAppFactory>
{
    private readonly NooksyWebAppFactory _factory;

    public CartFlowTests(NooksyWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddToCart_CreatesShoppingCartRecord()
    {
        using var scope = _factory.Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var cart = new ShoppingCart
        {
            ApplicationUserId = "test-user-1",
            ProductId = 1,
            Count = 2
        };

        uow.ShoppingCart.Add(cart);
        uow.Save();

        var saved = uow.ShoppingCart.Get(c => c.ApplicationUserId == "test-user-1" && c.ProductId == 1);
        saved.Should().NotBeNull();
        saved!.Count.Should().Be(2);
    }

    [Fact]
    public async Task RemoveCartItem_DeletesRecord()
    {
        using var scope = _factory.Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var cart = new ShoppingCart
        {
            ApplicationUserId = "test-user-2",
            ProductId = 1,
            Count = 1
        };

        uow.ShoppingCart.Add(cart);
        uow.Save();

        uow.ShoppingCart.Remove(cart);
        uow.Save();

        var all = uow.ShoppingCart.GetAll(c => c.ApplicationUserId == "test-user-2").ToList();
        all.Should().BeEmpty();
    }
}
