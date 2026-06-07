using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Moq;
using FluentAssertions;
using Xunit;
using Nooksy.Client.Components.UI;

namespace Nooksy.Tests.Unit.Components.UI;

public class NooksyButtonTests : TestContext
{
    [Fact]
    public void Renders_WithPrimaryVariantByDefault()
    {
        var cut = RenderComponent<NooksyButton>(p => p
            .AddChildContent("Click me"));

        cut.Markup.Should().Contain("btn-nooksy-primary");
    }

    [Theory]
    [InlineData(NooksyButton.NooksyButtonVariant.Accent, "btn-nooksy-accent")]
    [InlineData(NooksyButton.NooksyButtonVariant.Danger, "btn-nooksy-danger")]
    [InlineData(NooksyButton.NooksyButtonVariant.Outline, "btn-nooksy-outline")]
    public void Renders_CorrectClassForVariant(NooksyButton.NooksyButtonVariant variant, string expectedClass)
    {
        var cut = RenderComponent<NooksyButton>(p => p
            .Add(c => c.Variant, variant)
            .AddChildContent("Click me"));

        cut.Markup.Should().Contain(expectedClass);
    }

    [Fact]
    public void WhenLoading_ShowsSpinner_HidesContent()
    {
        var cut = RenderComponent<NooksyButton>(p => p
            .Add(c => c.IsLoading, true)
            .AddChildContent("Click me"));

        cut.Markup.Should().Contain("spinner-border");
        cut.Markup.Should().NotContain("Click me");
    }

    [Fact]
    public void WhenLoading_ButtonIsDisabled()
    {
        var cut = RenderComponent<NooksyButton>(p => p
            .Add(c => c.IsLoading, true));

        cut.Find("button").Attributes["disabled"].Should().NotBeNull();
    }

    [Fact]
    public void WhenDisabled_ButtonIsDisabledWithoutSpinner()
    {
        var cut = RenderComponent<NooksyButton>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent("Click me"));

        cut.Find("button").Attributes["disabled"].Should().NotBeNull();
        cut.Markup.Should().NotContain("spinner-border");
        cut.Markup.Should().Contain("Click me");
    }

    [Fact]
    public void Type_Submit_SetsHtmlAttribute()
    {
        var cut = RenderComponent<NooksyButton>(p => p
            .Add(c => c.Type, "submit")
            .AddChildContent("Submit"));

        cut.Find("button").GetAttribute("type").Should().Be("submit");
    }

    [Fact]
    public void OnClick_Fires_WhenClicked()
    {
        var clicked = false;
        var cut = RenderComponent<NooksyButton>(p => p
            .Add(c => c.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => clicked = true))
            .AddChildContent("Click me"));

        cut.Find("button").Click();

        clicked.Should().BeTrue();
    }

    [Fact]
    public void OnClick_DoesNotFire_WhenDisabled()
    {
        var clicked = false;
        var cut = RenderComponent<NooksyButton>(p => p
            .Add(c => c.Disabled, true)
            .Add(c => c.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => clicked = true))
            .AddChildContent("Click me"));

        cut.Find("button").Click();

        clicked.Should().BeFalse();
    }

    [Fact]
    public void ChildContent_RendersInButton()
    {
        var cut = RenderComponent<NooksyButton>(p => p
            .AddChildContent("Save Changes"));

        cut.Markup.Should().Contain("Save Changes");
    }

    [Theory]
    [InlineData("sm")]
    [InlineData("lg")]
    public void Size_AddsCorrectSizeClass(string size)
    {
        var cut = RenderComponent<NooksyButton>(p => p
            .Add(c => c.Size, size)
            .AddChildContent("Click me"));

        if (size == "sm")
        {
            cut.Markup.Should().Contain("btn-sm");
        }
        else
        {
            cut.Markup.Should().Contain("btn-lg");
        }
    }
}
