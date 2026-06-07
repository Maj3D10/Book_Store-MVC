using Xunit;
using FluentAssertions;
using Nooksy.Client.State;

namespace Nooksy.Tests.Unit.State;

public class CartStateTests
{
    [Fact]
    public void InitialCartCount_IsZero()
    {
        var cartState = new CartState();
        cartState.CartCount.Should().Be(0);
    }

    [Fact]
    public void SetCount_SetsValueAndNotifies()
    {
        var cartState = new CartState();
        var notified = false;
        cartState.OnChange += () => notified = true;

        cartState.SetCount(5);

        cartState.CartCount.Should().Be(5);
        notified.Should().BeTrue();
    }

    [Fact]
    public void Increment_DefaultAmount_AddsOne()
    {
        var cartState = new CartState();
        cartState.SetCount(3);

        cartState.Increment();

        cartState.CartCount.Should().Be(4);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(10)]
    public void Increment_CustomAmount_AddsCorrectly(int amount)
    {
        var cartState = new CartState();
        cartState.SetCount(5);

        cartState.Increment(amount);

        cartState.CartCount.Should().Be(5 + amount);
    }

    [Fact]
    public void Decrement_ReducesCount()
    {
        var cartState = new CartState();
        cartState.SetCount(5);

        cartState.Decrement(2);

        cartState.CartCount.Should().Be(3);
    }

    [Fact]
    public void Decrement_NeverGoesNegative()
    {
        var cartState = new CartState();
        cartState.SetCount(1);

        cartState.Decrement(5);

        cartState.CartCount.Should().Be(0);
    }

    [Fact]
    public void SetCount_SameValue_DoesNotFireOnChange()
    {
        var cartState = new CartState();
        cartState.SetCount(3);
        var notificationCount = 0;
        cartState.OnChange += () => notificationCount++;

        cartState.SetCount(3);

        notificationCount.Should().Be(0);
    }
}
