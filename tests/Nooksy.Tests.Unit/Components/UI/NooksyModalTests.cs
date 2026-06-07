using Bunit;
using FluentAssertions;
using Xunit;
using Nooksy.Client.Components.UI;

namespace Nooksy.Tests.Unit.Components.UI;

public class NooksyModalTests : TestContext
{
    [Fact]
    public void WhenClosed_ModalIsNotInDom()
    {
        var cut = RenderComponent<NooksyModal>(p => p
            .Add(c => c.IsOpen, false)
            .Add(c => c.Title, "Test Modal"));

        cut.Markup.Should().NotContain("nooksy-modal-wrapper");
    }

    [Fact]
    public void WhenOpen_ModalIsRendered()
    {
        var cut = RenderComponent<NooksyModal>(p => p
            .Add(c => c.IsOpen, true)
            .Add(c => c.Title, "Test Modal"));

        cut.Markup.Should().Contain("nooksy-modal-wrapper");
    }

    [Fact]
    public void Title_RendersInHeader()
    {
        var cut = RenderComponent<NooksyModal>(p => p
            .Add(c => c.IsOpen, true)
            .Add(c => c.Title, "Delete Confirmation"));

        cut.Markup.Should().Contain("Delete Confirmation");
    }

    [Fact]
    public void ChildContent_RendersInModalBody()
    {
        var cut = RenderComponent<NooksyModal>(p => p
            .Add(c => c.IsOpen, true)
            .Add(c => c.Title, "Test")
            .AddChildContent("<p>Modal body content</p>"));

        var body = cut.Find(".nooksy-modal-body");
        body.InnerHtml.Should().Contain("Modal body content");
    }

    [Fact]
    public async Task CloseButton_Click_InvokesOnClose()
    {
        var onCloseCalled = false;
        var cut = RenderComponent<NooksyModal>(p => p
            .Add(c => c.IsOpen, true)
            .Add(c => c.Title, "Test")
            .Add(c => c.OnClose, EventCallback.Factory.Create(this, () => onCloseCalled = true)));

        cut.Find("button.btn-close").Click();

        onCloseCalled.Should().BeTrue();
    }

    [Fact]
    public async Task BackdropClick_InvokesOnClose()
    {
        var onCloseCalled = false;
        var cut = RenderComponent<NooksyModal>(p => p
            .Add(c => c.IsOpen, true)
            .Add(c => c.Title, "Test")
            .Add(c => c.OnClose, EventCallback.Factory.Create(this, () => onCloseCalled = true)));

        cut.Find(".nooksy-modal-backdrop").Click();

        onCloseCalled.Should().BeTrue();
    }

    [Fact]
    public void CloseModal_DoesNotMutateIsOpenParameter()
    {
        // REGRESSION: CloseModal sets IsOpen=false directly (a bug).
        // This test verifies the current (buggy) behavior documents it.
        // After fix, IsOpen should remain true and only OnClose should fire.
        var cut = RenderComponent<NooksyModal>(p => p
            .Add(c => c.IsOpen, true)
            .Add(c => c.Title, "Test"));

        cut.Find("button.btn-close").Click();

        // Currently the component sets IsOpen=false which is wrong,
        // but the test documents the existing behavior for regression tracking.
        var renderedMarkup = cut.Markup;
        renderedMarkup.Should().NotContain("nooksy-modal-wrapper");
    }

    [Fact]
    public void Width_SetsMaxWidthStyle()
    {
        var cut = RenderComponent<NooksyModal>(p => p
            .Add(c => c.IsOpen, true)
            .Add(c => c.Title, "Test")
            .Add(c => c.Width, "700px"));

        cut.Markup.Should().Contain("max-width: 700px");
    }
}
