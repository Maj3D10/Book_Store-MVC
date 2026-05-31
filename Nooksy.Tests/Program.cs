using Nooksy.Models;
using Nooksy.Utility;

var tests = new (string Name, Action Run)[]
{
    ("pricing uses base tier through quantity 50", PricingUsesBaseTierThroughQuantity50),
    ("pricing uses 50 tier through quantity 100", PricingUses50TierThroughQuantity100),
    ("pricing uses 100 tier above quantity 100", PricingUses100TierAboveQuantity100),
    ("cart merge adds incoming count", CartMergeAddsIncomingCount),
    ("start processing sets processing status", StartProcessingSetsProcessingStatus),
    ("ship sets shipment fields and delayed due date", ShipSetsShipmentFieldsAndDelayedDueDate),
    ("ship does not set due date for paid orders", ShipDoesNotSetDueDateForPaidOrders)
};

var failed = 0;

foreach (var test in tests)
{
    try
    {
        test.Run();
        Console.WriteLine($"PASS {test.Name}");
    }
    catch (Exception ex)
    {
        failed++;
        Console.WriteLine($"FAIL {test.Name}: {ex.Message}");
    }
}

if (failed > 0)
{
    Console.WriteLine($"{failed} test(s) failed.");
    return 1;
}

Console.WriteLine($"{tests.Length} test(s) passed.");
return 0;

static Product TestProduct() => new()
{
    Title = "Test Book",
    Author = "Test Author",
    Description = "Test Description",
    ISBN = "TEST-001",
    Price = 10,
    Price50 = 8,
    Price100 = 6,
    ListPrice = 12
};

static void PricingUsesBaseTierThroughQuantity50()
{
    var product = TestProduct();

    AssertEqual(10, PricingRules.GetPriceBasedOnQuantity(product, 1));
    AssertEqual(10, PricingRules.GetPriceBasedOnQuantity(product, 50));
}

static void PricingUses50TierThroughQuantity100()
{
    var product = TestProduct();

    AssertEqual(8, PricingRules.GetPriceBasedOnQuantity(product, 51));
    AssertEqual(8, PricingRules.GetPriceBasedOnQuantity(product, 100));
}

static void PricingUses100TierAboveQuantity100()
{
    var product = TestProduct();

    AssertEqual(6, PricingRules.GetPriceBasedOnQuantity(product, 101));
}

static void CartMergeAddsIncomingCount()
{
    var existing = new ShoppingCart { Count = 2 };
    var incoming = new ShoppingCart { Count = 3 };

    CartRules.MergeCartItem(existing, incoming);

    AssertEqual(5, existing.Count);
}

static void StartProcessingSetsProcessingStatus()
{
    var order = new OrderHeader();

    OrderStatusRules.StartProcessing(order);

    AssertEqual(SD.StatusInProcess, order.OrderStatus);
}

static void ShipSetsShipmentFieldsAndDelayedDueDate()
{
    var shippedAt = new DateTime(2026, 5, 28, 10, 30, 0);
    var order = new OrderHeader { PaymentStatus = SD.PaymentStatusDelayedPayment };

    OrderStatusRules.Ship(order, "TRACK-1", "UPS", shippedAt);

    AssertEqual("TRACK-1", order.TrackingNumber);
    AssertEqual("UPS", order.Carrier);
    AssertEqual(SD.StatusShipped, order.OrderStatus);
    AssertEqual(shippedAt, order.ShippingDate);
    AssertEqual(DateOnly.FromDateTime(shippedAt.AddDays(30)), order.PaymentDueDate);
}

static void ShipDoesNotSetDueDateForPaidOrders()
{
    var shippedAt = new DateTime(2026, 5, 28, 10, 30, 0);
    var order = new OrderHeader { PaymentStatus = SD.PaymentStatusApproved };

    OrderStatusRules.Ship(order, "TRACK-2", "FedEx", shippedAt);

    AssertEqual(default(DateOnly), order.PaymentDueDate);
}

static void AssertEqual<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"Expected {expected}, got {actual}.");
    }
}
