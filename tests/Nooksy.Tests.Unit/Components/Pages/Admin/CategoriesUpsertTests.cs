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

public class CategoriesUpsertTests : TestContext
{
    [Fact]
    public void WhenIdIsZero_ShowsCreateMode()
    {
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);
        var mockUow = TestHelpers.CreateMockUnitOfWork();
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.Admin.Categories.Upsert>(p => p
            .Add(c => c.Id, 0));

        cut.Markup.Should().Contain("Create Category");
    }

    [Fact]
    public void WhenIdProvided_LoadsExistingCategory()
    {
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);
        var categories = new List<Category> { new() { Id = 5, Name = "Sci-Fi", DisplayOrder = 1 } };
        var mockUow = TestHelpers.CreateMockUnitOfWork(categories: categories);
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.Admin.Categories.Upsert>(p => p
            .Add(c => c.Id, 5));

        cut.Markup.Should().Contain("Edit Category");
    }

    [Fact]
    public void SubmitButton_RendersWithoutEnumReferenceError()
    {
        // Regression test for audit item 4: NooksyButtonVariant enum reference
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);
        var mockUow = TestHelpers.CreateMockUnitOfWork();
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.Admin.Categories.Upsert>(p => p
            .Add(c => c.Id, 0));

        // Should render without compilation errors
        cut.Markup.Should().Contain("btn-nooksy");
    }

    [Fact]
    public void CancelLink_HasCorrectHref()
    {
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);
        var mockUow = TestHelpers.CreateMockUnitOfWork();
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.Admin.Categories.Upsert>(p => p
            .Add(c => c.Id, 0));

        cut.Find("a[href='admin/categories']").Should().NotBeNull();
    }

    [Fact]
    public async Task BackToCategoriesLink_Renders()
    {
        TestHelpers.SetAuthenticatedUser(this, "admin-1", "Admin User", "Admin");
        var toastMock = new Mock<IToastService>();
        Services.AddSingleton(toastMock.Object);
        var mockUow = TestHelpers.CreateMockUnitOfWork();
        Services.AddSingleton<IUnitOfWork>(mockUow.Object);

        var cut = RenderComponent<Nooksy.Client.Components.Pages.Admin.Categories.Upsert>(p => p
            .Add(c => c.Id, 0));

        cut.Markup.Should().Contain("Back to Categories");
    }
}
