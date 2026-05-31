using Nooksy.Models;

namespace Nooksy.Utility
{
    public static class PricingRules
    {
        public static double GetPriceBasedOnQuantity(Product product, int count)
        {
            if (count <= 50)
            {
                return product.Price;
            }

            if (count <= 100)
            {
                return product.Price50;
            }

            return product.Price100;
        }
    }
}
