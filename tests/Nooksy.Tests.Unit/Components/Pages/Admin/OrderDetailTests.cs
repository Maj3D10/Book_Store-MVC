using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Moq;
using FluentAssertions;
using Xunit;
using Nooksy.DataAccess.Repository.IRepository;
using Nooksy.Models;
using Nooksy.Utility;
using Nooksy.Tests.Unit.Helpers;

namespace Nooksy.Tests.Unit.Components.Pages.Admin;

public class OrderDetailTests : TestContext
{
    [Fact]
    public async Task WhenOrderNotFound_ShowsNotFoundMessage()
    {
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);
        Services.AddSingleton(Mock.Of<IConfiguration>());
        var mockUow = TestHelpers.CreateMockUnitOfWork(); // no orders
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.Admin.Orders.Detail>(p => p
            .Add(c => c.Id, 999));

        cut.WaitForState(() => cut.Markup.Contains("Order not found") || !cut.Markup.Contains("Loading..."));

        cut.Markup.Should().Contain("Order not found");
    }

    [Fact]
    public async Task OrderHeader_RendersCustomerAndShippingInfo()
    {
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);
        Services.AddSingleton(Mock.Of<IConfiguration>());

        var orders = new List<OrderHeader>
        {
            new()
            {
                Id = 1, ApplicationUserId = "user-1", OrderStatus = SD.StatusPending, PaymentStatus = SD.PaymentStatusPending,
                OrderTotal = 99.99, Name = "John Doe", PhoneNumber = "555-0100",
                StreetAddress = "123 Main St", City = "Springfield", State = "IL", PostalCode = "62701",
                OrderDate = DateTime.Now, appUser = new ApplicationUser { Name = "John Doe", Email = "john@test.com" }
            }
        };
        var mockUow = TestHelpers.CreateMockUnitOfWork(orders: orders);
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.Admin.Orders.Detail>(p => p
            .Add(c => c.Id, 1));

        cut.WaitForState(() => cut.Markup.Contains("John Doe"));

        cut.Markup.Should().Contain("John Doe");
        cut.Markup.Should().Contain("john@test.com");
        cut.Markup.Should().Contain("123 Main St");
    }
}
