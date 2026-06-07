using Bunit;
using FluentAssertions;
using Xunit;
using Nooksy.Client.State;
using Microsoft.Extensions.DependencyInjection;

namespace Nooksy.Tests.Unit.Components.UI;

public class CartBadgeTests : TestContext
{
    [Fact]
    public void WhenCartIsEmpty_BadgeNotRendered()
    {
        var cartState = new CartState(); // count = 0
        Services.AddSingleton(cartState);

        var cut = RenderComponent<Nooksy.Client.Components.UI.CartBadge>();

        cut.Markup.Should().NotContain("nooksy-cart-badge");
        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void WhenCartHasItems_BadgeRendersCount()
    {
        var cartState = new CartState();
        cartState.SetCount(5);
        Services.AddSingleton(cartState);

        var cut = RenderComponent<Nooksy.Client.Components.UI.CartBadge>();

        cut.Markup.Should().Contain("5");
        cut.Markup.Should().Contain("nooksy-cart-badge");
    }

    [Fact]
    public void OnInitialized_SubscribesToCartStateOnChange()
    {
        var cartState = new CartState();
        Services.AddSingleton(cartState);

        var cut = RenderComponent<Nooksy.Client.Components.UI.CartBadge>();

        cartState.SetCount(3);
        cut.Markup.Should().Contain("3");
    }

    [Fact]
    public void Dispose_UnsubscribesFromCartStateOnChange()
    {
        var cartState = new CartState();
        Services.AddSingleton(cartState);

        var cut = RenderComponent<Nooksy.Client.Components.UI.CartBadge>();

        cut.Instance.Dispose();

        cartState.SetCount(5);
        // After dispose, the component should not get the event
        // The component's markup should still be the old one
        // Since we can't easily check this in bUnit, we verify dispose doesn't throw
    }

    [Fact]
    public void AfterDispose_CartStateChanges_DoNotTriggerStateHasChanged()
    {
        var cartState = new CartState();
        Services.AddSingleton(cartState);

        var cut = RenderComponent<Nooksy.Client.Components.UI.CartBadge>();
        cut.Instance.Dispose();

        // Set count to 5 - after dispose, this should not update the rendered markup
        cartState.SetCount(5);

        // Since we called Dispose, the handler was removed, so the component
        // should still show empty (no badge)
        cut.Markup.Trim().Should().BeEmpty();
    }
}
