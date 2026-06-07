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

public class OrdersIndexTests : TestContext
{
    private List<OrderHeader> GetSampleOrders()
    {
        return new List<OrderHeader>
        {
            new() { Id = 1, OrderStatus = SD.StatusPending, PaymentStatus = SD.PaymentStatusPending, OrderTotal = 50, OrderDate = DateTime.Now, appUser = new ApplicationUser { Name = "User 1" } },
            new() { Id = 2, OrderStatus = SD.StatusApproved, PaymentStatus = SD.PaymentStatusApproved, OrderTotal = 100, OrderDate = DateTime.Now, appUser = new ApplicationUser { Name = "User 2" } },
            new() { Id = 3, OrderStatus = SD.StatusShipped, PaymentStatus = SD.PaymentStatusApproved, OrderTotal = 75, OrderDate = DateTime.Now, appUser = new ApplicationUser { Name = "User 3" } },
        };
    }

    [Fact]
    public async Task AllTab_ShowsAllOrders()
    {
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);
        var orders = GetSampleOrders();
        var mockUow = TestHelpers.CreateMockUnitOfWork(orders: orders);
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.Admin.Orders.Index>();

        cut.WaitForState(() => cut.Markup.Contains("User 1"));

        // All 3 orders should be shown
        cut.Markup.Should().Contain("User 1");
        cut.Markup.Should().Contain("User 2");
        cut.Markup.Should().Contain("User 3");
    }

    [Fact]
    public void ViewDetails_LinkHasCorrectHref()
    {
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);
        var orders = GetSampleOrders();
        var mockUow = TestHelpers.CreateMockUnitOfWork(orders: orders);
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.Admin.Orders.Index>();
        cut.WaitForState(() => cut.Markup.Contains("User 1"));

        var viewLinks = cut.FindAll("a[href*='admin/orders/']");
        viewLinks.Count.Should().Be(3);
    }
}
