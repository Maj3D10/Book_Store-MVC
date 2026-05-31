using Nooksy.Models;

namespace Nooksy.Utility
{
    public static class CartRules
    {
        public static void MergeCartItem(ShoppingCart existingCartItem, ShoppingCart incomingCartItem)
        {
            existingCartItem.Count += incomingCartItem.Count;
        }
    }
}
