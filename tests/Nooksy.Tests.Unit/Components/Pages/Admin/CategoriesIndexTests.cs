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

public class CategoriesIndexTests : TestContext
{
    private List<Category> GetSampleCategories()
    {
        return new List<Category>
        {
            new() { Id = 1, Name = "Fiction", DisplayOrder = 1 },
            new() { Id = 2, Name = "Non-Fiction", DisplayOrder = 2 },
        };
    }

    [Fact]
    public async Task OnLoad_ShowsCategories()
    {
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);
        var mockUow = TestHelpers.CreateMockUnitOfWork(categories: GetSampleCategories());
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.Admin.Categories.Index>();

        cut.WaitForState(() => cut.Markup.Contains("Fiction"));

        cut.Markup.Should().Contain("Fiction");
        cut.Markup.Should().Contain("Non-Fiction");
    }

    [Fact]
    public void BeforeLoad_ShowsLoadingSpinner()
    {
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);
        var mockUow = TestHelpers.CreateMockUnitOfWork(categories: GetSampleCategories());
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.Admin.Categories.Index>();

        // Initial render should show loading spinner
        cut.Find("LoadingSpinner");
    }

    [Fact]
    public void CreateButton_HasCorrectHref()
    {
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);
        var mockUow = TestHelpers.CreateMockUnitOfWork(categories: GetSampleCategories());
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.Admin.Categories.Index>();
        cut.WaitForState(() => cut.Markup.Contains("Create Category"));

        var createLink = cut.Find("a[href='admin/categories/upsert']");
        createLink.Should().NotBeNull();
    }

    [Fact]
    public void Page_HasAuthorizeAttributeWithAdminEmployeeRoles()
    {
        // Verify the [Authorize] attribute is present on the page component
        var attrs = typeof(Nooksy.Client.Components.Pages.Admin.Categories.Index)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);

        attrs.Should().NotBeEmpty();
        var authAttr = attrs[0] as Microsoft.AspNetCore.Authorization.AuthorizeAttribute;
        authAttr.Roles.Should().Contain("Admin");
        authAttr.Roles.Should().Contain("Employee");
    }
}
