# Nooksy

Nooksy is a children's ebook store built with .NET 8 Blazor Server, featuring catalog browsing, role-based administration, shopping cart checkout, order management, and Stripe payment integration.

The solution is organized as a layered .NET application with separate projects for the Blazor UI, data access, shared models, utility/business rules, and lightweight automated tests.

## Features

- Public bookstore catalog with product cards and detail pages
- Product images with carousel support
- Quantity-based pricing tiers
- Customer shopping cart with live cart badge count
- Stripe Checkout for standard customer orders
- Delayed-payment flow for company accounts
- Admin dashboard with stat cards and recent orders
- Full CRUD for categories, products (with image upload), and companies
- User management with role badges and lock/unlock
- Order management with status filter tabs and workflow actions
- Product image upload and deletion via drag-and-drop uploader
- ASP.NET Core Identity authentication (Blazor Identity pages)
- Role-based authorization for `Customer`, `Company`, `Admin`, and `Employee`
- Order status workflow: pending, approved, processing, shipped, cancelled, refunded

## Tech Stack

- .NET 8
- Blazor Server (interactive server-side rendering)
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- Stripe.net
- Bootstrap 5
- Blazored.Toast
- TinyMCE
- Google Fonts (Nunito + DM Sans)

## Solution Structure

```text
Nooksy.sln
Nooksy.Client/       Blazor Server UI (customer + admin + identity pages)
Nooksy.Web/          Original ASP.NET Core MVC app (legacy)
Nooksy.DataAccess/   EF Core DbContext, migrations, repositories, seeding
Nooksy.Models/       Domain models and view models
Nooksy.Utility/      Shared constants, Stripe settings, business rules, email sender
Nooksy.Tests/        Lightweight console-based automated tests
```

### UI Project Structure (Nooksy.Client)

```text
Nooksy.Client/
├── Components/
│   ├── Layout/           MainLayout, AdminLayout (collapsible sidebar)
│   ├── Pages/
│   │   ├── Customer/     Home, Product Details, Cart, Checkout, Order Confirmation
│   │   └── Admin/        Dashboard, Categories, Products, Companies, Users, Orders
│   ├── Customer/         ProductCard component
│   ├── UI/               Reusable: Button, Badge, Alert, Modal, DataTable,
│   │                     ImageUploader, LoadingSpinner, CartBadge
│   └── Account/          Identity pages (Login, Register, Manage, etc.)
├── State/                CartState scoped service
├── wwwroot/
│   ├── css/nooksy.css    Brand design system
│   └── bootstrap/        Bootstrap 5
└── Program.cs            DI, Identity, Stripe, middleware
```

## Getting Started

### Prerequisites

- .NET 8 SDK or newer
- SQL Server or SQL Server LocalDB
- Visual Studio 2022, JetBrains Rider, or VS Code
- Stripe test keys for checkout testing

### Clone and Build

```powershell
git clone https://github.com/Maj3D10/Nooksy.git
cd Nooksy/src
dotnet restore Nooksy.sln
dotnet build Nooksy.sln
```

### Configure the Database

The app reads the EF Core connection string from configuration:

```json
{
  "ConnectionStrings": {
    "AppDbContextConnection": "Server=.;Database=Nooksy;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Update `Nooksy.Client/appsettings.json`, user secrets, or environment variables if your SQL Server instance uses a different server name.

EF Core migrations are included under:

```text
Nooksy.DataAccess/Migrations/
```

On startup, the database initializer applies pending migrations and seeds roles, sample catalog data, companies, and the default admin account.

### Configure Stripe

Stripe keys are intentionally not committed. Configure them locally with user secrets or environment variables.

Example using user secrets from the client project directory:

```powershell
cd Nooksy.Client
dotnet user-secrets init
dotnet user-secrets set "Stripe:SecretKey" "your_stripe_secret_key"
dotnet user-secrets set "Stripe:PublishableKey" "your_stripe_publishable_key"
```

Optional checkout base URL:

```powershell
dotnet user-secrets set "Checkout:BaseUrl" "https://localhost:7128"
```

If `Checkout:BaseUrl` is empty, the checkout callback URLs are built from the current request host.

### Run the App

```powershell
dotnet run --project Nooksy.Client/Nooksy.Client.csproj
```

Default project URLs:

```text
https://localhost:7128
http://localhost:5053
```

## Default Admin Login

The database initializer creates this admin user when roles are first seeded:

```text
Email: admin@nooksy.com
Password: Admin123*
```

## Configuration

Main configuration files:

```text
Nooksy.Client/appsettings.json
Nooksy.Client/appsettings.Development.json
Nooksy.Client/Properties/launchSettings.json
```

Important settings:

```json
{
  "ConnectionStrings": {
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

## Pages & Features

### Customer Area (Public Storefront)

| Page | Route | Description |
|------|-------|-------------|
| Home | `/` | Hero section, category filter pills, featured books grid, promotional banner |
| Product Details | `/product/{id}` | Image carousel, pricing tiers table, quantity selector, add to cart |
| Cart | `/cart` | Item list with inline quantity controls, line totals, order summary |
| Checkout | `/checkout` | Shipping form, B2B pay-later option, Stripe payment or delayed payment |
| Order Confirmation | `/order/confirmation/{id}` | Success animation, order summary, Stripe session verification |

### Admin Area

All admin pages require `Admin` or `Employee` role.

| Page | Route | Description |
|------|-------|-------------|
| Dashboard | `/admin/dashboard` | Stat cards (orders, revenue, products, users), recent orders table, low-image alert |
| Categories | `/admin/categories` | DataTable with search/pagination, create, edit, delete |
| Products | `/admin/products` | DataTable with cover thumbnails, create, edit, delete |
| Companies | `/admin/companies` | DataTable with search, create, edit, delete |
| Users | `/admin/users` | Role badges (color-coded), lock/unlock toggle |
| Orders | `/admin/orders` | Status filter tabs, order detail with workflow actions (process, ship, cancel) |

### Identity

Authentication pages are Blazor components under `Components/Account/`:

- Login, Register (with extra fields: name, phone, address, role, company selection)
- Password management, two-factor, email confirmation
- Profile management

Pages are styled to match the Nooksy brand: centered card, logo, rounded corners, blue/coral accents.

## Reusable UI Components

| Component | Parameters | Purpose |
|-----------|-----------|---------|
| `NooksyButton` | Variant, Size, Disabled, IsLoading, Type, OnClick | Primary/accent/outline/danger buttons with loading spinner |
| `NooksyBadge` | Status, Text | Color-coded status badges (pending, approved, shipped, etc.) |
| `NooksyAlert` | Type, Message, Dismissible, ChildContent | Success/warning/error/info alerts |
| `NooksyModal` | Title, IsOpen, Width, OnClose, ChildContent | Animated modal with backdrop blur |
| `DataTable` | TItem, Items, HeaderTemplate, RowTemplate, SearchMatch, PageSize | Search, pagination, sortable columns |
| `ImageUploader` | ExistingImageUrls, OnDeleteExistingImage, OnFilesSelected | Drag-and-drop zone, preview grid with delete |
| `CartBadge` | (injected CartState) | Coral badge with bounce animation on count change |
| `LoadingSpinner` | Message | Animated spinner with Nooksy branding |

## State Management

`CartState` is a scoped service that holds the cart item count and exposes an `OnChange` event. It is injected into `MainLayout` and the Cart page. When the count changes, `CartBadge` plays a bounce animation.

## Brand Design

The brand design system is defined in `wwwroot/css/nooksy.css`:

```css
--color-primary:      #4AABDB   /* Sky blue */
--color-accent:       #E87D7D   /* Coral */
--color-primary-dark: #2E8DB8
--color-accent-dark:  #D05F5F
--color-bg:           #F7FBFE
--color-surface:      #FFFFFF
--color-text:         #1A2B3C
--color-text-muted:   #6B8299
--color-border:       #D6EAF5
--color-success:      #52C785
--color-warning:      #F5A623
--color-danger:       #E05C5C
```

Typography: Nunito (headings), DM Sans (body). Rounded corners (12px–20px), soft shadows, no harsh edges.

## Data Model

Core entities:

- `Category`
- `Product`
- `ProductImage`
- `Company`
- `ApplicationUser` (extends `IdentityUser` with Name, Address, CompanyId)
- `ShoppingCart`
- `OrderHeader`
- `OrderDetail`

The data access layer uses a generic repository pattern with `UnitOfWork` to coordinate database operations.

## Business Rules

Shared rules live in `Nooksy.Utility`:

- `PricingRules`: quantity-based product pricing (tiers at 1–49, 50–99, 100+)
- `CartRules`: cart item merge behavior
- `OrderStatusRules`: processing and shipping transitions
- `SD`: role, order status, payment status, and session constants

## Tests

Run the lightweight automated test project:

```powershell
dotnet run --project Nooksy.Tests/Nooksy.Tests.csproj
```

Current coverage includes:

- Pricing tier boundaries
- Cart item merge behavior
- Order processing transition
- Shipping transition and delayed-payment due date rules

## Verification

```text
dotnet build Nooksy.sln
```

Result:

```text
Build: 0 errors
```

## License

No license file is currently included.
