using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using FluentAssertions;
using Xunit;
using Nooksy.Tests.Unit.Helpers;

namespace Nooksy.Tests.Unit.Components.Layout;

public class AdminLayoutTests : TestContext
{
    [Fact]
    public void AdminLayout_Renders_Sidebar()
    {
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");

        var cut = RenderComponent<Nooksy.Client.Components.Layout.AdminLayout>();

        cut.Markup.Should().Contain("admin-sidebar");
        cut.Markup.Should().Contain("Dashboard");
        cut.Markup.Should().Contain("Categories");
        cut.Markup.Should().Contain("Products");
        cut.Markup.Should().Contain("Users");
        cut.Markup.Should().Contain("Orders");
    }

    [Fact]
    public void AdminLayout_Breadcrumb_ShowsAdminAndPortal()
    {
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");

        var cut = RenderComponent<Nooksy.Client.Components.Layout.AdminLayout>();

        cut.Markup.Should().Contain("breadcrumb");
        cut.Markup.Should().Contain("Admin");
    }
}
