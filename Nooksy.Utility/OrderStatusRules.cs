using Nooksy.Models;

namespace Nooksy.Utility
{
    public static class OrderStatusRules
    {
        public static void StartProcessing(OrderHeader orderHeader)
        {
            orderHeader.OrderStatus = SD.StatusInProcess;
        }

        public static void Ship(OrderHeader orderHeader, string? trackingNumber, string? carrier, DateTime shippedAt)
        {
            orderHeader.TrackingNumber = trackingNumber;
            orderHeader.Carrier = carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = shippedAt;

            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(shippedAt.AddDays(30));
            }
        }
    }
}
