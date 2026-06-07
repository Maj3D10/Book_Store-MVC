using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using FluentAssertions;
using Xunit;
using Nooksy.DataAccess.Repository.IRepository;
using Nooksy.Models;
using Nooksy.Client.State;
using Nooksy.Utility;
using Nooksy.Tests.Unit.Helpers;

namespace Nooksy.Tests.Unit.Components.Pages.Customer;

public class ProductDetailsTests : TestContext
{
    [Fact]
    public async Task WhenProductNotFound_ShowsNotFoundMessage()
    {
        TestHelpers.SetAuthenticatedUser(this, "user-1", "Test User", "Customer");
        Services.AddSingleton(new CartState());
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);

        var mockUow = TestHelpers.CreateMockUnitOfWork(); // no products
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.ProductDetails>(p => p
            .Add(c => c.Id, 999));

        cut.WaitForState(() => cut.Markup.Contains("Book not found") || !cut.Markup.Contains("Loading"));

        cut.Markup.Should().Contain("Book not found");
    }

    [Fact]
    public async Task ProductDetailPage_ShowsTitleAndPricingTable()
    {
        TestHelpers.SetAuthenticatedUser(this, "user-1", "Test User", "Customer");
        Services.AddSingleton(new CartState());
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);

        var products = new List<Product>
        {
            new()
            {
                Id = 1, Title = "Test Book", Author = "Test Author", ISBN = "1234567890123",
                Description = "A great book", ListPrice = 19.99, Price = 14.99, Price50 = 12.99, Price100 = 10.99,
                CategoryId = 1, Category = new Category { Name = "Fiction" },
                ProductImages = new List<ProductImage> { new() { ImageUrl = "/img.jpg" } }
            }
        };
        var mockUow = TestHelpers.CreateMockUnitOfWork(products: products);
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.ProductDetails>(p => p
            .Add(c => c.Id, 1));

        cut.WaitForState(() => cut.Markup.Contains("Test Book"));

        cut.Markup.Should().Contain("Test Book");
        cut.Markup.Should().Contain("Test Author");
        cut.Markup.Should().Contain("14.99");
    }
}
