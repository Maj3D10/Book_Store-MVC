using Bunit;
using FluentAssertions;
using Xunit;
using Nooksy.Client.Components.UI;

namespace Nooksy.Tests.Unit.Components.UI;

public class NooksyBadgeTests : TestContext
{
    [Theory]
    [InlineData("Pending", "nooksy-badge-pending")]
    [InlineData("Approved", "nooksy-badge-approved")]
    [InlineData("Processing", "nooksy-badge-inprocess")]
    [InlineData("Shipped", "nooksy-badge-shipped")]
    [InlineData("Cancelled", "nooksy-badge-cancelled")]
    [InlineData("Refunded", "nooksy-badge-refunded")]
    [InlineData("ApprovedForDelayedPayment", "nooksy-badge-approved")]
    [InlineData("unknown-garbage", "nooksy-badge-pending")]
    [InlineData("", "nooksy-badge-pending")]
    public void Status_MapsToCorrectCssClass(string status, string expectedClass)
    {
        var cut = RenderComponent<NooksyBadge>(p => p
            .Add(c => c.Status, status)
            .Add(c => c.Text, status));

        cut.Markup.Should().Contain(expectedClass);
    }

    [Fact]
    public void Text_RendersInsideSpan()
    {
        var cut = RenderComponent<NooksyBadge>(p => p
            .Add(c => c.Status, "Approved")
            .Add(c => c.Text, "Approved"));

        cut.Markup.Should().Contain("Approved");
    }
}
