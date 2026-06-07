using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Nooksy.DataAccess.Repository.IRepository;
using Nooksy.Models;
using Nooksy.Utility;
using Nooksy.Tests.Integration.Setup;
using Xunit;

namespace Nooksy.Tests.Integration.Admin;

public class OrderWorkflowTests : IClassFixture<NooksyWebAppFactory>
{
    private readonly NooksyWebAppFactory _factory;

    public OrderWorkflowTests(NooksyWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateOrder_PersistsWithPendingStatus()
    {
        using var scope = _factory.Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var order = new OrderHeader
        {
            ApplicationUserId = "test-user",
            Name = "Test User",
            PhoneNumber = "555-0000",
            StreetAddress = "123 Test St",
            City = "Test City",
            State = "TS",
            PostalCode = "12345",
            OrderDate = DateTime.Now,
            OrderStatus = SD.StatusPending,
            PaymentStatus = SD.PaymentStatusPending,
            OrderTotal = 50.00
        };

        uow.orderHeader.Add(order);
        uow.Save();

        var saved = uow.orderHeader.Get(o => o.Id == order.Id);
        saved.Should().NotBeNull();
        saved!.OrderStatus.Should().Be(SD.StatusPending);
    }

    [Fact]
    public async Task UpdateStatus_ToApproved_ChangesDbRecord()
    {
        using var scope = _factory.Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var order = new OrderHeader
        {
            ApplicationUserId = "test-user",
            Name = "Test User",
            PhoneNumber = "555-0000",
            StreetAddress = "123 Test St",
            City = "Test City",
            State = "TS",
            PostalCode = "12345",
            OrderDate = DateTime.Now,
            OrderStatus = SD.StatusPending,
            PaymentStatus = SD.PaymentStatusPending,
            OrderTotal = 50.00
        };

        uow.orderHeader.Add(order);
        uow.Save();

        uow.orderHeader.UpdateStatus(order.Id, SD.StatusApproved, SD.PaymentStatusApproved);
        uow.Save();

        var updated = uow.orderHeader.Get(o => o.Id == order.Id);
        updated!.OrderStatus.Should().Be(SD.StatusApproved);
        updated.PaymentStatus.Should().Be(SD.PaymentStatusApproved);
    }
}
