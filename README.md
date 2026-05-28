# FuzzyWeb

FuzzyWeb is an ASP.NET Core MVC bookstore application with catalog browsing, role-based administration, shopping cart checkout, order management, and Stripe payment integration.

The project is organized as a layered .NET solution with separate projects for the web UI, data access, shared models, and utility code.

## Features

- Public bookstore catalog with product cards and detail pages
- Product images with carousel support
- Quantity-based pricing tiers
- Customer shopping cart with session-based cart count
- Stripe Checkout for standard customer orders
- Delayed-payment flow for company accounts
- Admin dashboard for categories, products, companies, users, and orders
- Product image upload and deletion
- ASP.NET Core Identity authentication
- Role-based authorization for `Customer`, `Company`, `Admin`, and `Employee`
- User lock/unlock and role management
- Order status workflow: pending, approved, processing, shipped, cancelled, refunded

## Tech Stack

- .NET 8
- ASP.NET Core MVC
- Razor Pages for Identity
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- Stripe.net
- Bootstrap
- jQuery
- DataTables
- SweetAlert2
- Toastr
- TinyMCE

## Solution Structure

```text
FuzzyWeb.sln
FuzzyWeb/          ASP.NET Core MVC web app
Fuzzy.DataAccess/  EF Core DbContext, migrations, repositories, seeding
Fuzzy.Models/      Domain models and view models
Fuzzy.Utility/     Shared constants, Stripe settings, email sender
```

## Getting Started

### Prerequisites

- .NET 8 SDK or newer
- SQL Server or SQL Server LocalDB
- Visual Studio 2022, JetBrains Rider, or VS Code
- Stripe test keys for checkout testing

### Clone and Build

```powershell
git clone https://github.com/Maj3D10/Fuzzy_Web-MVC.git
cd Fuzzy_Web-MVC/src
dotnet restore FuzzyWeb.sln
dotnet build FuzzyWeb.sln
```

### Configure the Database

The current app configures SQL Server in `FuzzyWeb/Program.cs`:

```csharp
options.UseSqlServer("Server=.;Database=FuzzyBook;Trusted_Connection=True;TrustServerCertificate=True")
```

Make sure your local SQL Server instance is available at `Server=.` or update the connection string before running.

EF Core migrations are included under:

```text
Fuzzy.DataAccess/Migrations/
```

On startup, the database initializer applies pending migrations and seeds roles, sample catalog data, companies, and the default admin account.

### Run the App

```powershell
dotnet run --project FuzzyWeb/FuzzyBook.Web.csproj --launch-profile https
```

Default project URLs:

```text
https://localhost:7128
http://localhost:5053
```

IIS Express URLs:

```text
https://localhost:44325
http://localhost:28208
```

Stripe success and cancel URLs are currently hardcoded to `https://localhost:44325/` in the cart checkout flow, so use the IIS Express HTTPS URL or update the domain in `CartController`.

## Default Admin Login

The database initializer creates this admin user when roles are first seeded:

```text
Email: admin@dotnetmastery.com
Password: Admin123*
```

## Configuration

Main configuration files:

```text
FuzzyWeb/appsettings.json
FuzzyWeb/appsettings.Development.json
FuzzyWeb/Properties/launchSettings.json
```

Important settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "...",
    "AppDbContextConnection": "..."
  },
  "Stripe": {
    "SecretKey": "...",
    "PublishableKey": "..."
  }
}
```

## Security Notes

This repository currently stores Stripe keys in `appsettings.json`. Before deploying or sharing a production copy:

- Move secrets to user secrets, environment variables, or a secret manager.
- Rotate any keys that were committed.
- Avoid hardcoding checkout callback domains.
- Review admin credentials and password policy.

## Main Application Areas

### Customer

Customer-facing catalog and checkout code lives under:

```text
FuzzyWeb/Areas/Customer/
```

Key flows:

- Browse products
- View product details
- Add items to cart
- Update cart quantities
- Submit checkout summary
- Pay through Stripe or use delayed payment for company accounts
- Receive order confirmation

### Admin

Admin code lives under:

```text
FuzzyWeb/Areas/Admin/
```

Admin users can manage:

- Categories
- Products
- Product images
- Companies
- Users
- Roles
- Orders
- Shipping status

### Identity

Authentication pages live under:

```text
FuzzyWeb/Areas/Identity/Pages/
```

The registration page has been customized to collect profile fields, role selection, and company assignment.

## Data Model

Core entities:

- `Category`
- `Product`
- `ProductImage`
- `Company`
- `ApplicationUser`
- `ShoppingCart`
- `OrderHeader`
- `OrderDetail`

The data access layer uses a generic repository pattern with `UnitOfWork` to coordinate database operations.

## Documentation

Detailed internal codebase documentation is available in:

```text
DOCUMENTATION.md
```

That file includes architecture notes, routes, entity details, payment flow, repository structure, and maintenance observations.

## Build Status

Current local verification:

```text
dotnet build FuzzyWeb.sln
```

Result:

```text
0 errors
86 warnings
```

Most warnings are existing nullable-reference warnings. The app builds successfully.

## Known Maintenance Items

- Move Stripe secrets out of source control.
- Replace the hardcoded SQL Server connection string with configuration.
- Fix the order list JavaScript status parameter handling.
- Fix malformed script tags in the shared layout.
- Add automated tests for pricing, cart behavior, and order status transitions.
- Normalize inconsistent namespaces such as `FuzzyWeb`, `FuzzyBook.Web`, and `BulkyBookWeb`.

## License

No license file is currently included.
