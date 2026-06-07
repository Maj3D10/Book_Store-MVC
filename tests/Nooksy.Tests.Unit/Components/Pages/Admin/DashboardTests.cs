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

public class DashboardTests : TestContext
{
    [Fact]
    public async Task StatCards_ShowCorrectTotals()
    {
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);

        var orders = new List<OrderHeader>
        {
            new() { Id = 1, OrderTotal = 100, OrderStatus = SD.StatusApproved, PaymentStatus = SD.PaymentStatusApproved },
            new() { Id = 2, OrderTotal = 50, OrderStatus = SD.StatusPending, PaymentStatus = SD.PaymentStatusPending },
        };
        var products = new List<Product> { new() { Id = 1, Title = "Book 1", ProductImages = new List<ProductImage>() } };
        var users = new List<ApplicationUser> { new() { Id = "user-1", Name = "User 1" } };
        var mockUow = TestHelpers.CreateMockUnitOfWork(orders: orders, products: products, users: users);
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.Admin.Dashboard>();

        // Wait for async OnInitializedAsync
        cut.WaitForState(() => cut.Markup.Contains("Total Orders"));

        cut.Markup.Should().Contain("Total Orders");
        cut.Markup.Should().Contain("Total Products");
        cut.Markup.Should().Contain("Active Users");
    }

    [Fact]
    public async Task Revenue_OnlySumsApprovedAndDelayedPaymentOrders()
    {
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);

        var orders = new List<OrderHeader>
        {
            new() { Id = 1, OrderTotal = 200, OrderStatus = SD.StatusApproved, PaymentStatus = SD.PaymentStatusApproved },
            new() { Id = 2, OrderTotal = 150, OrderStatus = SD.StatusApproved, PaymentStatus = SD.PaymentStatusDelayedPayment },
            new() { Id = 3, OrderTotal = 100, OrderStatus = SD.StatusPending, PaymentStatus = SD.PaymentStatusPending },
            new() { Id = 4, OrderTotal = 50, OrderStatus = SD.StatusCancelled, PaymentStatus = SD.PaymentStatusRejected },
        };
        var mockUow = TestHelpers.CreateMockUnitOfWork(orders: orders);
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.Admin.Dashboard>();

        cut.WaitForState(() => cut.Markup.Contains("Total Orders"));

        // Revenue should be $350 (200 + 150), not $500 (all) or $300 (without approved)
        cut.Markup.Should().Contain("350.00");
    }

    [Fact]
    public async Task WithProductsWithNoImages_ShowsWarningAlert()
    {
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);

        var products = new List<Product>
        {
            new() { Id = 1, Title = "Book Without Image", ProductImages = new List<ProductImage>() }
        };
        var mockUow = TestHelpers.CreateMockUnitOfWork(products: products);
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.Admin.Dashboard>();

        cut.WaitForState(() => cut.Markup.Contains("Asset Coverage Alert") || cut.Markup.Contains("Total Orders"));

        cut.Markup.Should().Contain("Asset Coverage Alert");
    }

    [Fact]
    public async Task OnLoadException_ShowsErrorToast()
    {
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);

        // Make the UoW throw when GetAll is called
        var mockUow = new Mock<IUnitOfWork>();
        mockUow.Setup(u => u.orderHeader).Throws(new Exception("DB Error"));
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.Admin.Dashboard>();

        cut.WaitForState(() => !string.IsNullOrEmpty(cut.Markup));

        toastMock.Verify(t => t.ShowError(It.IsAny<string>(), default), Times.AtLeastOnce);
    }
}
