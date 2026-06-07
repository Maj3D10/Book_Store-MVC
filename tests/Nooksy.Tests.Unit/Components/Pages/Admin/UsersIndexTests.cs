using Bunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using FluentAssertions;
using Xunit;
using Nooksy.DataAccess.Repository.IRepository;
using Nooksy.Models;
using Nooksy.Utility;
using Nooksy.Tests.Unit.Helpers;

namespace Nooksy.Tests.Unit.Components.Pages.Admin;

public class UsersIndexTests : TestContext
{
    [Fact]
    public async Task OnLoad_ShowsUserList()
    {
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);

        var users = new List<ApplicationUser>
        {
            new() { Id = "u1", Name = "Alice", Email = "alice@test.com", Role = "Customer" },
            new() { Id = "u2", Name = "Bob", Email = "bob@test.com", Role = "Admin" },
        };
        var mockUow = TestHelpers.CreateMockUnitOfWork(users: users);
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync((ApplicationUser u) => new List<string> { u.Role });
        Services.AddSingleton(userManagerMock.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.Admin.Users.Index>();

        cut.WaitForState(() => cut.Markup.Contains("Alice"));

        cut.Markup.Should().Contain("Alice");
        cut.Markup.Should().Contain("Bob");
    }
}
