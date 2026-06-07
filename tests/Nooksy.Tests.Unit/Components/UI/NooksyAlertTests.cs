using Bunit;
using FluentAssertions;
using Xunit;
using Nooksy.Client.Components.UI;

namespace Nooksy.Tests.Unit.Components.UI;

public class NooksyAlertTests : TestContext
{
    [Fact]
    public void Alert_IsVisibleByDefault()
    {
        var cut = RenderComponent<NooksyAlert>(p => p
            .Add(c => c.Message, "Test message"));

        cut.Markup.Should().Contain("alert");
    }

    [Fact]
    public void Message_RendersTextContent()
    {
        var cut = RenderComponent<NooksyAlert>(p => p
            .Add(c => c.Message, "Hello World"));

        cut.Markup.Should().Contain("Hello World");
    }

    [Fact]
    public void ChildContent_RendersWhenMessageEmpty()
    {
        var cut = RenderComponent<NooksyAlert>(p => p
            .AddChildContent("<strong>Custom content</strong>"));

        cut.Markup.Should().Contain("Custom content");
    }

    [Fact]
    public void Dismissible_ShowsCloseButton()
    {
        var cut = RenderComponent<NooksyAlert>(p => p
            .Add(c => c.Message, "Test")
            .Add(c => c.Dismissible, true));

        cut.FindAll("button.btn-close").Count.Should().Be(1);
    }

    [Fact]
    public void NonDismissible_HidesCloseButton()
    {
        var cut = RenderComponent<NooksyAlert>(p => p
            .Add(c => c.Message, "Test")
            .Add(c => c.Dismissible, false));

        cut.FindAll("button.btn-close").Count.Should().Be(0);
    }

    [Fact]
    public async Task DismissButton_Click_HidesAlert()
    {
        var cut = RenderComponent<NooksyAlert>(p => p
            .Add(c => c.Message, "Test")
            .Add(c => c.Dismissible, true));

        cut.Find("button.btn-close").Click();

        cut.Markup.Should().NotContain("alert");
    }

    [Theory]
    [InlineData("Success", "alert-success", "check-circle-fill")]
    [InlineData("Warning", "alert-warning", "exclamation-triangle-fill")]
    [InlineData("Error", "alert-danger", "x-circle-fill")]
    [InlineData("Info", "alert-info", "info-circle-fill")]
    public void Type_MapsToCorrectAlertClassAndIcon(string type, string cssClass, string iconClass)
    {
        var cut = RenderComponent<NooksyAlert>(p => p
            .Add(c => c.Type, type)
            .Add(c => c.Message, "Test"));

        cut.Markup.Should().Contain(cssClass);
        cut.Markup.Should().Contain(iconClass);
    }
}
