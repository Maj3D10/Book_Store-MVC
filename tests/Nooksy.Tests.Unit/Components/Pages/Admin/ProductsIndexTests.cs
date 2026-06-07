using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using FluentAssertions;
using Xunit;
using Nooksy.DataAccess.Repository.IRepository;
using Nooksy.Models;
using Nooksy.Utility;
using Nooksy.Tests.Unit.Helpers;

namespace Nooksy.Tests.Unit.Components.Pages.Admin;

public class ProductsIndexTests : TestContext
{
    private List<Product> GetSampleProducts()
    {
        return new List<Product>
        {
            new() { Id = 1, Title = "Book One", Author = "Author A", ISBN = "123", Price = 14.99, CategoryId = 1, Category = new Category { Name = "Fiction" }, ProductImages = new List<ProductImage> { new() { ImageUrl = "/img1.jpg" } } },
            new() { Id = 2, Title = "Book Two", Author = "Author B", ISBN = "456", Price = 19.99, CategoryId = 2, Category = new Category { Name = "Non-Fiction" }, ProductImages = new List<ProductImage>() },
        };
    }

    [Fact]
    public async Task OnLoad_ShowsProductsInTable()
    {
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);
        var products = GetSampleProducts();
        var mockUow = TestHelpers.CreateMockUnitOfWork(products: products);
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.Admin.Products.Index>();

        cut.WaitForState(() => cut.Markup.Contains("Book One"));

        cut.Markup.Should().Contain("Book One");
        cut.Markup.Should().Contain("Book Two");
    }

    [Fact]
    public async Task WithImages_ShowsCoverImage()
    {
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);
        var products = GetSampleProducts();
        var mockUow = TestHelpers.CreateMockUnitOfWork(products: products);
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.Admin.Products.Index>();

        cut.WaitForState(() => cut.Markup.Contains("Book One"));

        cut.Markup.Should().Contain("/img1.jpg");
    }

    [Fact]
    public async Task WithoutImages_ShowsFallbackIcon()
    {
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);
        var products = GetSampleProducts();
        var mockUow = TestHelpers.CreateMockUnitOfWork(products: products);
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.Admin.Products.Index>();

        cut.WaitForState(() => cut.Markup.Contains("Book Two"));

        // Book Two has no images, should show fallback icon
        cut.Markup.Should().Contain("bi-book");
    }
}
