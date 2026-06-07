# Nooksy Codebase Documentation

Nooksy is an ASP.NET Core MVC bookstore application built with .NET 8, Entity Framework Core, ASP.NET Core Identity, SQL Server, Razor views, Bootstrap, jQuery DataTables, and Stripe Checkout.

The solution is organized as a small layered application:

- `Nooksy.Web`: MVC web application, Razor views, Identity pages, static assets, startup configuration, and controllers.
- `Nooksy.DataAccess`: Entity Framework Core `DbContext`, migrations, repositories, and database initializer.
- `Nooksy.Models`: domain entities and view models shared across the web and data layers.
- `Nooksy.Utility`: shared constants, Stripe settings, business rules, and email sender implementation.
- `Nooksy.Tests`: lightweight automated tests for pricing, cart, and order-status rules.

## Solution Structure

```text
Nooksy.sln
Nooksy.Web/
  Areas/
    Admin/
      Controllers/
      Views/
    Customer/
      Controllers/
      Views/
    Identity/
      Pages/
  Controllers/
  ViewComponents/
  Views/
  wwwroot/
  Program.cs
  appsettings.json
Nooksy.DataAccess/
  Data/
  DbInitializer/
  Migrations/
  Repository/
Nooksy.Models/
  ViewModel/
Nooksy.Utility/
Nooksy.Tests/
```

## Runtime Stack

- Target framework: `.NET 8`
- Web framework: ASP.NET Core MVC with Razor Pages for Identity
- Authentication and authorization: ASP.NET Core Identity
- ORM: Entity Framework Core
- Database provider: SQL Server
- Payment provider: Stripe Checkout
- Frontend libraries: Bootstrap, Bootstrap Icons, jQuery, jQuery Validation, DataTables, SweetAlert2, Toastr, TinyMCE

The local machine currently has .NET SDK `10.0.204` installed. The project itself targets `net8.0`, so a .NET 8 SDK or newer SDK capable of building .NET 8 projects is required.

## Application Startup

Startup is configured in `Nooksy.Web/Program.cs`.

The app registers:

- MVC controllers with views
- Razor Pages for Identity
- `AppDbContext` using SQL Server
- ASP.NET Core Identity with `IdentityUser` and `IdentityRole`
- Cookie paths for login, logout, and access denied pages
- Distributed memory cache and session state
- `IUnitOfWork` repository abstraction
- `IEmailSender`
- `IDbInitializer`
- Localization with supported cultures `en-US`, `fr-FR`, `de-DE`, and `es-ES`
- Stripe API key from configuration

The default route is:

```text
{area=Customer}/{controller=Home}/{action=Index}/{id?}
```

That means the storefront product list is the default landing page.

## Configuration

Configuration lives in:

- `Nooksy.Web/appsettings.json`
- `Nooksy.Web/appsettings.Development.json`
- `Nooksy.Web/Properties/launchSettings.json`

Important configuration keys:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "...",
    "AppDbContextConnection": "..."
  },
  "Stripe": {
    "SecretKey": "",
    "PublishableKey": ""
  },
  "Checkout": {
    "BaseUrl": ""
  }
}
```

`Program.cs` reads `AppDbContextConnection` from configuration and passes it to EF Core:

```csharp
options.UseSqlServer(connectionString)
```

Stripe keys are intentionally blank in `appsettings.json`. Configure them with user secrets, environment variables, or a deployment secret manager.

## Running Locally

From the solution root:

```powershell
dotnet restore Nooksy.sln
dotnet build Nooksy.sln
dotnet run --project Nooksy.Web/Nooksy.Web.csproj --launch-profile https
```

The HTTPS launch profile uses:

```text
https://localhost:7128
http://localhost:5053
```

The IIS Express profile uses:

```text
https://localhost:44325
http://localhost:28208
```

Stripe checkout callbacks use `Checkout:BaseUrl` when configured. If it is blank, callback URLs are built from the current request scheme and host.

## Database and Seeding

The database context is `Nooksy.DataAccess/Data/AppDbContext.cs`.

It inherits from:

```csharp
IdentityDbContext<IdentityUser>
```

Registered DbSets:

- `Categories`
- `Products`
- `ApplicationUsers`
- `Companies`
- `ShoppingCarts`
- `OrderHeaders`
- `OrderDetails`
- `ProductImages`

`OnModelCreating` seeds:

- 3 categories
- 3 companies
- 6 products

The database initializer is `Nooksy.DataAccess/DbInitializer/DbInitilializer.cs`.

On app startup, it:

- Applies pending EF Core migrations.
- Creates roles if they do not exist.
- Creates a default admin user when roles are first created.

Default admin:

```text
Email: admin@dotnetmastery.com
Password: Admin123*
Role: Admin
```

Roles are defined in `Nooksy.Utility/SD.cs`:

- `Customer`
- `Company`
- `Admin`
- `Employee`

## Entity Model

### Category

Represents a product category.

Fields:

- `Id`
- `Name`
- `DisplayOrder`

### Product

Represents a book/product in the catalog.

Fields:

- `Id`
- `Title`
- `Description`
- `ISBN`
- `Author`
- `ListPrice`
- `Price`
- `Price50`
- `Price100`
- `CategoryId`
- `Category`
- `ProductImages`

The app supports quantity-based pricing:

- 1-50 items: `Price`
- 51-100 items: `Price50`
- More than 100 items: `Price100`

### ProductImage

Represents one image belonging to a product.

Fields:

- `Id`
- `ImageUrl`
- `ProductId`
- `Product`

Uploaded product images are stored under:

```text
Nooksy.Web/wwwroot/images/products/product-{ProductId}/
```

### Company

Represents a company account for delayed payment purchases.

Fields:

- `Id`
- `Name`
- `StreetAddress`
- `City`
- `State`
- `PostalCode`
- `PhoneNumber`

### ApplicationUser

Extends ASP.NET Core Identity users with profile and company fields.

Fields:

- `Name`
- `StreetAddress`
- `Address`
- `State`
- `PostalCode`
- `City`
- `CompanyId`
- `company`
- `Role` as a non-mapped convenience property

### ShoppingCart

Represents a product in a user's cart.

Fields:

- `Id`
- `Product`
- `ProductId`
- `Count`
- `ApplicationUserId`
- `Price` as a non-mapped calculated value

### OrderHeader

Represents an order summary, customer shipping details, and payment state.

Fields include:

- `ApplicationUserId`
- `OrderDate`
- `ShippingDate`
- `OrderTotal`
- `OrderStatus`
- `PaymentStatus`
- `TrackingNumber`
- `Carrier`
- `PaymentDate`
- `PaymentDueDate`
- `SessionId`
- `PaymentIntenId`
- Shipping name, phone, street, city, state, and postal code

### OrderDetail

Represents a line item in an order.

Fields:

- `OrderHeaderId`
- `ProductId`
- `Count`
- `Price`

## Repository Layer

The data layer uses a generic repository plus a unit of work.

Generic repository:

```text
Nooksy.DataAccess/Repository/Repository.cs
```

It provides:

- `Add`
- `Get`
- `GetAll`
- `Remove`
- `RemoveRange`

The `includeProperties` string parameter is used to include navigation properties, for example:

```csharp
_unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages")
```

The unit of work is:

```text
Nooksy.DataAccess/Repository/UnitOfWork.cs
```

It exposes repositories for:

- Category
- Product
- Company
- ShoppingCart
- ApplicationUser
- OrderHeader
- OrderDetail
- ProductImage

Specialized repositories add update behavior for entities that need it. `OrderHeaderRepository` also handles status transitions and Stripe session/payment intent IDs.

## Web Areas and Features

### Customer Area

Controllers:

- `Nooksy.Web/Areas/Customer/Controllers/HomeController.cs`
- `Nooksy.Web/Areas/Customer/Controllers/CartController.cs`

Storefront features:

- List products on the home page.
- Show product detail page with image carousel, category, ISBN, pricing tiers, description, and quantity.
- Add products to cart for authenticated users.
- Merge duplicate cart entries by increasing quantity.
- Track cart count in session.
- View cart.
- Increase, decrease, or remove cart items.
- Enter shipping details on order summary.
- Place orders.
- Route individual customers to Stripe Checkout.
- Allow company users to place delayed-payment orders without immediate Stripe checkout.
- Confirm payment, clear cart, and show order confirmation.

### Admin Area

Controllers:

- `Nooksy.Web/Areas/Admin/Controllers/CategoryController.cs`
- `Nooksy.Web/Areas/Admin/Controllers/ProductController.cs`
- `Nooksy.Web/Areas/Admin/Controllers/CompanyController.cs`
- `Nooksy.Web/Areas/Admin/Controllers/UserController.cs`
- `Nooksy.Web/Areas/Admin/Controllers/OrderController.cs`

Admin-only features:

- Category CRUD
- Product CRUD
- Product image upload and deletion
- Company CRUD
- User listing
- User lock/unlock
- User role management
- Assign users to companies

Order management features:

- List orders
- Filter orders by status
- View order details
- Update shipping/contact details
- Mark order as processing
- Mark order as shipped
- Add tracking number and carrier
- Set delayed payment due date for company orders

Order management requires authentication. Updating and shipping orders is limited to `Admin` and `Employee`.

### Identity Area

Identity pages are scaffolded under:

```text
Nooksy.Web/Areas/Identity/Pages/
```

The custom registration page extends the default Identity flow by collecting:

- Name
- Address fields
- Phone number
- Role
- Company assignment for company users

If an admin creates a user, the admin remains signed in. If a public user registers, that new user is signed in as a customer by default unless another role is selected.

## Shopping Cart Session

The cart count is stored in session using:

```csharp
SD.SessionCart
```

The shopping cart view component is:

```text
Nooksy.Web/ViewComponents/ShoppingCartViewComponent.cs
```

It:

- Reads the authenticated user's cart count from the database when the session value is missing.
- Displays `0` and clears session for anonymous users.

The shared layout invokes it in the navigation bar.

## Payment Flow

Payment logic is in:

```text
Nooksy.Web/Areas/Customer/Controllers/CartController.cs
```

Flow:

1. User opens cart summary.
2. App builds an `OrderHeader` and `OrderDetail` rows from the shopping cart.
3. If the user is not tied to a company, order/payment status is set to pending.
4. A Stripe Checkout session is created.
5. The order stores `SessionId` and `PaymentIntenId`.
6. Browser is redirected to Stripe.
7. On order confirmation, the app retrieves the Stripe session.
8. If paid, the order is marked approved and payment approved.
9. Cart rows are removed.

Company users skip immediate Stripe payment:

- `OrderStatus`: `Approved`
- `PaymentStatus`: `ApprovedForDelayedPayment`

When a delayed-payment order is shipped, `PaymentDueDate` is set to 30 days after shipment.

## Order Status Values

Defined in `Nooksy.Utility/SD.cs`:

- `Pending`
- `Approved`
- `Processing`
- `Shipped`
- `Cancelled`
- `Refunded`

Payment statuses:

- `Pending`
- `Approved`
- `ApprovedForDelayedPayment`
- `Rejected`

## Client-Side Assets

Static files live under:

```text
Nooksy.Web/wwwroot/
```

Important JavaScript files:

- `wwwroot/js/product.js`: DataTables product listing and AJAX deletion.
- `wwwroot/js/company.js`: DataTables company listing and AJAX deletion.
- `wwwroot/js/user.js`: DataTables user listing and lock/unlock AJAX calls.
- `wwwroot/js/order.js`: DataTables order listing by status.
- `wwwroot/js/site.js`: default app script file.

The layout imports CDN assets for Bootstrap Icons, Toastr, DataTables, SweetAlert2, and TinyMCE.

## Common Routes

Storefront:

```text
GET  /Customer/Home/Index
GET  /Customer/Home/Details?productId={id}
POST /Customer/Home/Details
```

Cart:

```text
GET  /Customer/Cart/Index
GET  /Customer/Cart/Plus?cartId={id}
GET  /Customer/Cart/Minus?cartId={id}
GET  /Customer/Cart/Remove?cartId={id}
GET  /Customer/Cart/Summary
POST /Customer/Cart/Summary
GET  /Customer/Cart/OrderConfirmation?id={orderId}
```

Admin categories:

```text
GET  /Admin/Category/Index
GET  /Admin/Category/Create
POST /Admin/Category/Create
GET  /Admin/Category/Edit/{id}
POST /Admin/Category/Edit
GET  /Admin/Category/Delete/{id}
POST /Admin/Category/Delete
```

Admin products:

```text
GET    /Admin/Product/Index
GET    /Admin/Product/Upsert?id={id}
POST   /Admin/Product/Upsert
GET    /Admin/Product/GetAll
DELETE /Admin/Product/Delete/{id}
GET    /Admin/Product/DeleteImage?imageId={id}
```

Admin companies:

```text
GET    /Admin/Company/Index
GET    /Admin/Company/Upsert?id={id}
POST   /Admin/Company/Upsert
GET    /Admin/Company/GetAll
DELETE /Admin/Company/Delete/{id}
```

Admin users:

```text
GET  /Admin/User/Index
GET  /Admin/User/GetAll
GET  /Admin/User/RoleManagment?userId={id}
POST /Admin/User/RoleManagment
POST /Admin/User/LockUnlock
```

Orders:

```text
GET  /Admin/Order/Index
GET  /Admin/Order/GetAll?status={status}
GET  /Admin/Order/Details?orderId={id}
POST /Admin/Order/UpdateOrderDetail
POST /Admin/Order/StartProcessing
POST /Admin/Order/ShipOrder
```

## View Models

View models live in:

```text
Nooksy.Models/ViewModel/
```

- `ProductVM`: product plus category dropdown items.
- `ShoppingCartVM`: cart rows plus order header.
- `OrderVM`: order header plus order detail rows.
- `RoleManagementVM`: application user plus role and company dropdowns.

## Business Rules and Tests

Shared business rules live in `Nooksy.Utility`:

- `PricingRules`: quantity-based product pricing.
- `CartRules`: cart merge behavior.
- `OrderStatusRules`: order processing and shipping transitions.

Run the automated checks with:

```powershell
dotnet run --project Nooksy.Tests/Nooksy.Tests.csproj
```

Current coverage includes pricing tier boundaries, cart merge behavior, order processing, and shipping due-date behavior for delayed-payment orders.

## Development Notes

- EF migrations are already present under `Nooksy.DataAccess/Migrations`.
- `DbInitializer.Initialize()` catches migration exceptions and suppresses them, so database migration problems may not surface clearly at startup.
- `EmailSender` is a no-op implementation that returns `Task.CompletedTask`; email confirmation messages are not actually sent.
- The solution currently builds with nullable-reference warnings in some MVC, Identity, and controller code.

## Completed Maintenance

1. Renamed the solution, project files, folders, namespaces, and app branding to Nooksy.
2. Removed committed Stripe secret values from `appsettings.json`.
3. Replaced the hardcoded SQL Server connection string in `Program.cs` with configuration.
4. Replaced the hardcoded Stripe checkout callback domain with configurable/request-derived base URL handling.
5. Fixed `order.js` status parameter handling.
6. Fixed malformed script tags in `_Layout.cshtml`.
7. Added automated tests for pricing, cart behavior, and order status transitions.
8. Normalized old inconsistent namespace references.
