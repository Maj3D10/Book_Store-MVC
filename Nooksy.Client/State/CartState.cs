using System;

namespace Nooksy.Client.State
{
    public class CartState
    {
        private int _cartCount = 0;

        public int CartCount
        {
            get => _cartCount;
            private set
            {
                if (_cartCount != value)
                {
                    _cartCount = value;
                    NotifyStateChanged();
                }
            }
        }

        public event Action? OnChange;

        public void SetCount(int count)
        {
            CartCount = count;
        }

        public void Increment(int amount = 1)
        {
            CartCount += amount;
        }

        public void Decrement(int amount = 1)
        {
            CartCount = Math.Max(0, CartCount - amount);
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
