using Bunit;
using FluentAssertions;
using Xunit;
using Nooksy.Client.Components.UI;

namespace Nooksy.Tests.Unit.Components.UI;

public class LoadingSpinnerTests : TestContext
{
    [Fact]
    public void Message_RendersInParagraph()
    {
        var cut = RenderComponent<LoadingSpinner>(p => p
            .Add(c => c.Message, "Loading data..."));

        cut.Markup.Should().Contain("Loading data...");
    }

    [Fact]
    public void DefaultMessage_UsedWhenNotProvided()
    {
        var cut = RenderComponent<LoadingSpinner>();

        cut.Markup.Should().Contain("Opening book box...");
    }

    [Fact]
    public void ContainsSpinnerBorderElement()
    {
        var cut = RenderComponent<LoadingSpinner>();

        cut.Find(".spinner-border").Should().NotBeNull();
    }
}
