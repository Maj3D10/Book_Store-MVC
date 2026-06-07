using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using FluentAssertions;
using Xunit;
using Nooksy.Client.State;
using Nooksy.DataAccess.Repository.IRepository;
using Nooksy.Tests.Unit.Helpers;

namespace Nooksy.Tests.Unit.Components.Layout;

public class MainLayoutTests : TestContext
{
    [Fact]
    public void UnauthenticatedUser_CartLinkGoesToLogin()
    {
        Services.AddSingleton(new CartState());
        Services.AddSingleton(Mock.Of<IUnitOfWork>());
        var authStateProvider = new FakeAuthStateProvider("", "", "");
        Services.AddSingleton<AuthenticationStateProvider>(authStateProvider);
        Services.AddAuthorizationCore();

        var cut = RenderComponent<Nooksy.Client.Components.Layout.MainLayout>();

        // Should have login link for cart
        cut.Markup.Should().Contain("Account/Login");
    }

    [Fact]
    public void AuthenticatedUser_CartLinkGoesToCart()
    {
        Services.AddSingleton(new CartState());
        Services.AddSingleton(Mock.Of<IUnitOfWork>());
        TestHelpers.SetAuthenticatedUser(this, "user-1", "Test User", "Customer");

        var cut = RenderComponent<Nooksy.Client.Components.Layout.MainLayout>();

        cut.Markup.Should().Contain("cart\"");
    }

    [Fact]
    public void AdminUser_SeesAdminPortalLink()
    {
        Services.AddSingleton(new CartState());
        Services.AddSingleton(Mock.Of<IUnitOfWork>());
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");

        var cut = RenderComponent<Nooksy.Client.Components.Layout.MainLayout>();

        cut.Markup.Should().Contain("Admin Portal");
    }

    [Fact]
    public void CustomerUser_NoAdminPortalLink()
    {
        Services.AddSingleton(new CartState());
        Services.AddSingleton(Mock.Of<IUnitOfWork>());
        TestHelpers.SetAuthenticatedUser(this, "user-1", "Test User", "Customer");

        var cut = RenderComponent<Nooksy.Client.Components.Layout.MainLayout>();

        cut.Markup.Should().NotContain("Admin Portal");
    }

    [Fact]
    public void Unauthenticated_ShowsLoginAndRegisterButtons()
    {
        Services.AddSingleton(new CartState());
        Services.AddSingleton(Mock.Of<IUnitOfWork>());
        var authStateProvider = new FakeAuthStateProvider("", "", "");
        Services.AddSingleton<AuthenticationStateProvider>(authStateProvider);
        Services.AddAuthorizationCore();

        var cut = RenderComponent<Nooksy.Client.Components.Layout.MainLayout>();

        cut.Markup.Should().Contain("Log In");
        cut.Markup.Should().Contain("Register");
    }

    [Fact]
    public void Authenticated_ShowsUserDropdown()
    {
        Services.AddSingleton(new CartState());
        Services.AddSingleton(Mock.Of<IUnitOfWork>());
        TestHelpers.SetAuthenticatedUser(this, "user-1", "Test User", "Customer");

        var cut = RenderComponent<Nooksy.Client.Components.Layout.MainLayout>();

        cut.Markup.Should().Contain("userMenuButton");
    }

    [Fact]
    public async Task HamburgerButton_TogglesDrawer()
    {
        Services.AddSingleton(new CartState());
        Services.AddSingleton(Mock.Of<IUnitOfWork>());
        TestHelpers.SetAuthenticatedUser(this, "user-1", "Test User", "Customer");

        var cut = RenderComponent<Nooksy.Client.Components.Layout.MainLayout>();

        // Initially drawer should not be visible
        cut.Markup.Should().NotContain("mobile-drawer");

        // Click hamburger
        cut.Find("button[aria-label='Toggle navigation']").Click();

        // Drawer should now be visible
        cut.Markup.Should().Contain("mobile-drawer");
    }
}
