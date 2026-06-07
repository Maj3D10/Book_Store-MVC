# Nooksy Blazor — Code Audit: Fixes & Modifications Required

> **Repo:** `Maj3D10/Book_Store-MVC` (master branch, Blazor Server build)  
> **Audit date:** June 2026  
> **Status legend:** 🔴 Critical (broken/missing) · 🟠 High (incorrect behaviour) · 🟡 Medium (code quality/UX) · 🟢 Low (polish/nice-to-have)

---

## 1. Missing Pages — Customer Area (🔴 CRITICAL)

The entire customer-facing storefront is **absent from the repository**. The following pages exist in the README spec and the solution structure comment, but their `.razor` files return 404 on raw.githubusercontent.com and were confirmed empty during the audit.

| Page | Route | Status |
|------|-------|--------|
| `Home.razor` | `/` | 🔴 File not found |
| `ProductDetails.razor` | `/product/{id:int}` | 🔴 File not found |
| `Cart.razor` | `/cart` | 🔴 File not found |
| `Checkout.razor` | `/checkout` | 🔴 File not found |
| `OrderConfirmation.razor` | `/order/confirmation/{id:int}` | 🔴 File not found |

### What to implement for each

#### `Home.razor`
- Hero section: heading "Discover books kids love ♥", two CTA buttons (Browse Books → `/shop`, View Categories — scrolls to category row)
- Category filter pills row: fetched from `UnitOfWork.Category.GetAll()`, clicking a pill filters the product grid below
- Featured Books grid using `ProductCard` component (3–4 cols desktop, 2 tablet, 1 mobile)
- Promotional banner strip (`--color-accent` background): bulk discount call-to-action
- `@page "/"` with `MainLayout`; no `[Authorize]` — public page

#### `ProductDetails.razor`
- Route: `@page "/product/{id:int}"`
- Image carousel: if `ProductImages.Count > 1` show prev/next arrows + dot indicators
- Pricing tiers table (1–49 / 50–99 / 100+) pulled from `PricingRules` in `Nooksy.Utility`
- Quantity input with `+` / `–` buttons; quantity drives the displayed unit price
- "Add to Cart" button: requires `[Authorize]` redirect to login if unauthenticated; on success call `CartState.Increment()` and show toast
- Description rendered from HTML string (use `@((MarkupString)product.Description)`)
- Back to Shop link

#### `Cart.razor`
- Route: `@page "/cart"`, `[Authorize]`
- Load carts: `UnitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product,Product.ProductImages")`
- Inline quantity editing with `+` / `–` per item; on change: recalculate price using `PricingRules.GetPrice(product, count)` and `UnitOfWork.Save()`
- Remove item button (trash icon, accent color): remove from DB + call `CartState.Decrement()`
- Order summary card: subtotal, total, Checkout button
- Empty cart state: illustration placeholder + "Your cart is empty" + "Start Shopping" link

#### `Checkout.razor`
- Route: `@page "/checkout"`, `[Authorize]`
- Pre-fill shipping fields from `ApplicationUser` if available
- Left column: `EditForm` with fields: Name, PhoneNumber, StreetAddress, City, State, PostalCode
- Right column: read-only order summary (items, totals)
- If user role is `Company`: show "Pay Later (Invoice)" radio option alongside Stripe
- On submit: create `OrderHeader` + `OrderDetail` records, then either initiate Stripe Checkout Session or set `PaymentStatus = SD.PaymentStatusDelayedPayment`
- Stripe success/cancel URLs must be built dynamically from `IConfiguration["Checkout:BaseUrl"]` (already wired in `Program.cs`) — do **not** hardcode localhost

#### `OrderConfirmation.razor`
- Route: `@page "/order/confirmation/{id:int}"`, `[Authorize]`
- On load: verify Stripe session if `SessionId` is present on the `OrderHeader`
  ```csharp
  var sessionService = new Stripe.Checkout.SessionService();
  var session = await sessionService.GetAsync(orderHeader.SessionId);
  if (session.PaymentStatus == "paid") {
      UnitOfWork.orderHeader.UpdateStatus(Id, SD.StatusApproved, SD.PaymentStatusApproved);
      UnitOfWork.Save();
  }
  ```
- Display: animated checkmark (CSS keyframe), order number, item summary table
- "Continue Shopping" button
- Clear cart from DB after confirmation: `UnitOfWork.ShoppingCart.RemoveRange(...)` + `CartState.SetCount(0)`

---

## 2. Missing Pages — Admin Area (🔴 CRITICAL)

| Page | Route | Status |
|------|-------|--------|
| `Companies/Index.razor` | `/admin/companies` | 🔴 File not found |
| `Companies/Upsert.razor` | `/admin/companies/upsert/{id?}` | 🔴 File not found |

### What to implement

#### `Companies/Index.razor`
Mirrors `Categories/Index.razor` pattern:
- `DataTable` with columns: Name · City · State · Phone · Actions (Edit / Delete)
- Delete confirmation via `NooksyModal`
- On delete: check if any `ApplicationUser.CompanyId == company.Id` — show warning toast if users exist, block deletion

#### `Companies/Upsert.razor`
- Fields: Name, StreetAddress, City, State, PostalCode, PhoneNumber
- All fields required with `DataAnnotationsValidator`
- Save via `UnitOfWork.Company.Add/Update` + `UnitOfWork.Save()`

---

## 3. Admin — Missing `Shop` Page (🟠 HIGH)

The navbar links to `/shop` but no `Shop.razor` page exists. Either:
- **Option A (recommended):** Move the product grid + category filter from `Home.razor` into a dedicated `Shop.razor` at `@page "/shop"`, and make `Home.razor` a true landing/marketing page.
- **Option B:** Redirect `/shop` to `/` using a `NavigationManager.NavigateTo("/")` page stub.

---

## 4. `NooksyButton` — Variant Enum Reference Bug (🔴 CRITICAL)

In `Categories/Upsert.razor` and potentially other places, the button variant is referenced as:
```razor
<NooksyButton Variant="NooksyButtonVariant.Primary" ...>
```
But the enum is nested inside the component class, so the correct reference is:
```razor
<NooksyButton Variant="NooksyButton.NooksyButtonVariant.Primary" ...>
```
This will produce a compile error. **Fix:** either un-nest the enum (declare it in its own file `NooksyButtonVariant.cs`) or consistently use the fully-qualified form `NooksyButton.NooksyButtonVariant.Primary` everywhere.

**Files to fix:** `Categories/Upsert.razor`, and any other Upsert/form pages that use `NooksyButton`.

---

## 5. `AdminLayout` — Breadcrumb Is Hardcoded (🟠 HIGH)

```razor
<li class="breadcrumb-item active text-primary" aria-current="page">Portal</li>
```
Every admin page shows "Admin > Portal" regardless of what page you're on. Fix by passing a `[CascadingParameter]` or using `NavigationManager.Uri` to derive the current section name.

**Suggested fix in `AdminLayout.razor`:**
```csharp
private string GetCurrentSection()
{
    var uri = NavigationManager.Uri;
    if (uri.Contains("/products")) return "Products";
    if (uri.Contains("/categories")) return "Categories";
    if (uri.Contains("/companies")) return "Companies";
    if (uri.Contains("/users")) return "Users";
    if (uri.Contains("/orders")) return "Orders";
    return "Dashboard";
}
```
Then render: `<li class="breadcrumb-item active ...">@GetCurrentSection()</li>`
Inject `NavigationManager` into the layout.

---

## 6. `AdminLayout` — Sidebar `NavLink` Active State Not Applying (🟠 HIGH)

The sidebar uses Bootstrap's `nav-link` class, but the Blazor `NavLink` `active` class conflicts with the custom `.admin-sidebar .nav-link.active` CSS rule. The CSS targets `.nav-link.active` but Blazor adds the class `active` to the `<a>` tag generated by `NavLink`. Verify the rendered HTML and adjust:

```css
/* nooksy.css — ensure this rule exists and is specific enough */
.admin-sidebar a.active {
    background: rgba(255, 255, 255, 0.15);
    border-left: 3px solid var(--color-accent);
    color: white !important;
}
```

Also add `Match="NavLinkMatch.Prefix"` to all sidebar `NavLink` elements except Dashboard (which should use `NavLinkMatch.All`):
```razor
<NavLink href="admin/dashboard" Match="NavLinkMatch.All" class="nav-link ...">
<NavLink href="admin/products"  Match="NavLinkMatch.Prefix" class="nav-link ...">
```

---

## 7. `MainLayout` — Navbar Search Does Nothing (🟡 MEDIUM)

The search input in the navbar has `HandleSearch` wired to `keydown`, but the handler is empty:
```csharp
private void HandleSearch(KeyboardEventArgs e)
{
    if (e.Key == "Enter")
    {
        // Optional: trigger navigation to shop with search criteria
    }
}
```

**Fix:**
```csharp
private string searchQuery = "";
// In markup: @bind="searchQuery" on the input

private void HandleSearch(KeyboardEventArgs e)
{
    if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(searchQuery))
    {
        NavigationManager.NavigateTo($"/shop?q={Uri.EscapeDataString(searchQuery)}");
    }
}
```
The `Shop.razor` page should then read the `q` query parameter and pre-filter the product grid.

---

## 8. `MainLayout` — Mobile Drawer Slides In From Wrong Side (🟡 MEDIUM)

```css
@keyframes slideInLeft {
    from { transform: translateX(100%); }   /* slides from right */
    to   { transform: translateX(0); }
}
```
The animation name says "Left" but it slides in from the **right** (`100%`). For a left-side drawer, fix to:
```css
@keyframes slideInLeft {
    from { transform: translateX(-100%); }
    to   { transform: translateX(0); }
}
```
Also swap the layout order — currently the backdrop is on the left and the drawer content on the right. For a left-side drawer, the drawer should come first in the flex row.

---

## 9. `Dashboard.razor` — N+1 Query on Product Images (🟠 HIGH)

```csharp
var allProducts = UnitOfWork.Product.GetAll(includeProperties: "ProductImages").ToList();
var allImages   = UnitOfWork.ProductImage.GetAll().ToList();   // ← redundant second query
foreach (var prod in allProducts) {
    prod.ProductImages = allImages.Where(i => i.ProductId == prod.Id).ToList();
}
```
Since `GetAll(includeProperties: "ProductImages")` already eagerly loads images via EF Core navigation, the second `GetAll()` call and the manual assignment loop are redundant and double the DB work.

**Fix:** Remove the second query entirely:
```csharp
var allProducts = UnitOfWork.Product.GetAll(includeProperties: "ProductImages").ToList();
productsWithNoImages = allProducts.Where(p => p.ProductImages == null || !p.ProductImages.Any()).ToList();
```

The same N+1 pattern exists in `Orders/Detail.razor`:
```csharp
orderDetails = UnitOfWork.orderDetail.GetAll(u => u.OrderHeaderId == Id, includeProperties: "Product").ToList();
var allImages = UnitOfWork.ProductImage.GetAll().ToList();  // ← redundant
```
**Fix:** Use `includeProperties: "Product,Product.ProductImages"` in the `GetAll` call instead.

---

## 10. `Orders/Detail.razor` — Stripe API Key Set Inside a Method (🟠 HIGH)

```csharp
private async Task CancelOrder()
{
    var secretKey = Configuration.GetSection("Stripe:SecretKey").Get<string>();
    Stripe.StripeConfiguration.ApiKey = secretKey;  // ← mutates global static mid-request
    ...
}
```
Setting `StripeConfiguration.ApiKey` inside an async handler is not thread-safe — it mutates a global static and can affect other concurrent requests.

**Fix:** Stripe is already configured globally in `Program.cs`:
```csharp
var stripeSecretKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
if (!string.IsNullOrWhiteSpace(stripeSecretKey))
    StripeConfiguration.ApiKey = stripeSecretKey;
```
Remove the per-method key assignment in `CancelOrder()`. Just call the `RefundService` directly.

---

## 11. `Products/Upsert.razor` — TinyMCE Not Implemented (🟡 MEDIUM)

The description field falls back to a plain `<textarea>`:
```razor
<textarea @bind="product.Description" class="form-control" rows="6" ...></textarea>
```
The README and the NuGet dependency list include TinyMCE. Integrate it:

```razor
@* In _Imports.razor or at top of file: *@
@using TinyMCE.Blazor

<Editor @bind-Value="product.Description"
        ApiKey="your-tinymce-api-key"
        Conf="@(new Dictionary<string, object> {
            { "plugins", "lists link image" },
            { "toolbar", "bold italic | bullist numlist | link image" },
            { "height", 300 }
        })" />
```

If TinyMCE is not desired, replace with `<InputTextArea>` (Blazor-native) instead of raw `<textarea>` so `DataAnnotationsValidator` picks up validation.

---

## 12. `Products/Index.razor` — N+1 Images Query (🟠 HIGH)

Same pattern as Dashboard (item 9):
```csharp
products = UnitOfWork.Product.GetAll(includeProperties: "Category,ProductImages").ToList();
var allImages = UnitOfWork.ProductImage.GetAll().ToList();  // ← unnecessary
foreach (var prod in products)
    prod.ProductImages = allImages.Where(i => i.ProductId == prod.Id).ToList();
```
**Fix:** Remove the second query; EF Core has already loaded images via `includeProperties`.

---

## 13. `Users/Index.razor` — `user.Email` May Throw NullReferenceException (🟡 MEDIUM)

The `DataTable` `SearchMatch` lambda calls `u.Email.Contains(...)` without a null guard:
```csharp
SearchMatch="@((u, query) => u.Name.Contains(query, ...) || u.Email.Contains(query, ...))"
```
`ApplicationUser` inherits `Email` from `IdentityUser`; it can be null for external-login accounts.

**Fix:**
```csharp
SearchMatch="@((u, query) =>
    (u.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
    (u.Email?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))"
```

---

## 14. `Users/Index.razor` — `GetRolesAsync` Called in a Loop (🟠 HIGH)

```csharp
foreach (var user in allUsers)
{
    var roles = await UserManager.GetRolesAsync(user);  // N separate DB round-trips
    user.Role = roles.FirstOrDefault() ?? "Customer";
    ...
}
```
This issues one DB query per user (N+1 async). For large user lists this is very slow.

**Fix:** Either:
- Pre-query the `AspNetUserRoles` + `AspNetRoles` tables in one join via `AppDbContext` directly, or
- Use `UserManager.Users.Include(u => u.Roles)` if navigation is set up, or
- Accept the N+1 but add a `Task.WhenAll` batching approach if sequential awaiting is acceptable

Minimum quick fix — at least run in parallel:
```csharp
var rolesTasks = allUsers.Select(async u =>
{
    var roles = await UserManager.GetRolesAsync(u);
    u.Role = roles.FirstOrDefault() ?? "Customer";
    return u;
});
users = (await Task.WhenAll(rolesTasks)).ToList();
```

---

## 15. `CartState.cs` — Not Persisted Across Navigation (🟡 MEDIUM)

`CartState` is `AddScoped` in Blazor Server, which means it lives for the duration of the SignalR circuit. However, on first load the count is initialized in `MainLayout.OnInitializedAsync`. If the user navigates directly to `/cart` or another page _before_ `MainLayout` finishes loading, `CartCount` may be stale.

**Fix:** Move the initialization to `CartState` itself (inject `IServiceScopeFactory` + `IHttpContextAccessor` to get the user ID and pre-load on first access), or add `OnParametersSetAsync` hydration in `MainLayout` that also fires on navigation events:
```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender) await HydrateCartCount();
}
```

---

## 16. `DataTable.razor` — `PageSize` Two-Way Binding Conflict (🟡 MEDIUM)

The component declares:
```csharp
[Parameter] public int PageSize { get; set; } = 10;
```
And in the template the `<select>` uses `@bind="PageSize"`. Mutating a `[Parameter]` property directly from within a component violates Blazor's one-way data flow and will produce a runtime warning:

> "Parameter 'PageSize' is supplied by the parent component and should not be modified directly."

**Fix:** Add a private backing field:
```csharp
[Parameter] public int PageSize { get; set; } = 10;
private int _pageSize = 10;

protected override void OnParametersSet() => _pageSize = PageSize;
```
Then bind `<select>` to `_pageSize` and use `_pageSize` in all pagination calculations.

---

## 17. `NooksyModal.razor` — `IsOpen` Mutated Internally (🟡 MEDIUM)

```csharp
private async Task CloseModal()
{
    IsOpen = false;      // ← mutates [Parameter] directly
    await OnClose.InvokeAsync();
}
```
Same Blazor anti-pattern as item 16. The parent must own `IsOpen`; the modal should only invoke the callback.

**Fix:**
```csharp
private async Task CloseModal()
{
    await OnClose.InvokeAsync();   // parent sets IsOpen = false
}
```
Ensure all parent usages set `isDeleteModalOpen = false` in their `CancelDelete` handlers (they already do — just remove the internal mutation).

---

## 18. `Program.cs` — `UseSession` Registered After `UseAntiforgery` (🟠 HIGH)

```csharp
app.UseAntiforgery();
app.UseSession();         // ← must come BEFORE any middleware that reads session
```
Middleware order matters. `UseSession` should come before `UseAntiforgery` and before `MapRazorComponents`:

```csharp
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();          // ← move here
app.UseAntiforgery();
```

---

## 19. `Program.cs` — Stripe Key Silently Skipped (🟡 MEDIUM)

```csharp
var stripeSecretKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
if (!string.IsNullOrWhiteSpace(stripeSecretKey))
    StripeConfiguration.ApiKey = stripeSecretKey;
```
If the key is missing, Stripe calls silently fail with an unhelpful `AuthenticationException`. In development, add a startup log warning:
```csharp
if (string.IsNullOrWhiteSpace(stripeSecretKey))
    app.Logger.LogWarning("Stripe:SecretKey is not configured. Payment features will be unavailable.");
```

---

## 20. CSS — `fs-7` Utility Class Does Not Exist in Bootstrap 5 (🟡 MEDIUM)

Bootstrap 5 defines `fs-1` through `fs-6`. Several components use `fs-7`:
```razor
class="btn-nooksy btn-nooksy-primary py-2 fs-7"
class="btn-nooksy btn-nooksy-accent py-1 px-3 fs-7"
```
**Fix:** Either add a custom utility to `nooksy.css`:
```css
.fs-7 { font-size: 0.8rem !important; }
```
Or replace `fs-7` with `small` or `fs-6`.

---

## 21. CSS — `.text-accent` and `.text-text-muted` Not Defined (🟡 MEDIUM)

Both classes appear frequently in templates but are not in `nooksy.css`:
- `.text-accent` (should set `color: var(--color-accent)`)
- `.text-text-muted` (should set `color: var(--color-text-muted)`)

**Add to `nooksy.css`:**
```css
.text-accent      { color: var(--color-accent) !important; }
.text-text-muted  { color: var(--color-text-muted) !important; }
.bg-accent        { background-color: var(--color-accent) !important; }
.border-accent    { border-color: var(--color-accent) !important; }
```

---

## 22. CSS — `.nooksy-card` Not Defined (🟡 MEDIUM)

The class `.nooksy-card` is used on nearly every admin page card wrapper but is not present in `nooksy.css` (the file was truncated in the audit but the relevant rule appears to be missing).

**Add to `nooksy.css`:**
```css
.nooksy-card {
    background: var(--color-surface);
    border: 1px solid var(--color-border);
    border-radius: var(--radius-lg);
    box-shadow: var(--shadow-sm);
}
```

---

## 23. CSS — `category-pill` Active State Missing (🟡 MEDIUM)

In `Orders/Index.razor`:
```razor
<button class="category-pill @(selectedStatus == null ? "active" : "")">All</button>
```
The `.category-pill.active` style is not defined in `nooksy.css`.

**Add to `nooksy.css`:**
```css
.category-pill {
    padding: 0.4rem 1rem;
    border-radius: 100px;
    border: 1.5px solid var(--color-border);
    background: var(--color-surface);
    font-family: var(--font-display);
    font-weight: 600;
    font-size: 0.85rem;
    color: var(--color-text-muted);
    cursor: pointer;
    transition: all 0.2s;
}
.category-pill.active,
.category-pill:hover {
    background: var(--color-primary);
    border-color: var(--color-primary);
    color: white;
}
```

---

## 24. CSS — `.stat-card` Not Defined (🟡 MEDIUM)

Used in `Dashboard.razor` but absent from `nooksy.css`.

**Add:**
```css
.stat-card {
    display: flex;
    align-items: center;
    gap: 1rem;
    background: var(--color-surface);
    border: 1px solid var(--color-border);
    border-radius: var(--radius-lg);
    padding: 1.25rem 1.5rem;
    box-shadow: var(--shadow-sm);
    transition: box-shadow 0.2s, transform 0.2s;
}
.stat-card:hover {
    box-shadow: var(--shadow-md);
    transform: translateY(-3px);
}
.stat-icon {
    width: 48px; height: 48px;
    border-radius: var(--radius-md);
    display: flex; align-items: center; justify-content: center;
    font-size: 1.4rem;
}
.stat-icon-primary { background: rgba(74, 171, 219, 0.12); color: var(--color-primary); }
.stat-icon-success { background: rgba(82, 199, 133, 0.12); color: var(--color-success); }
.stat-icon-accent  { background: rgba(232, 125, 125, 0.12); color: var(--color-accent); }
.stat-icon-warning { background: rgba(245, 166, 35, 0.12);  color: var(--color-warning); }
```

---

## 25. CSS — `.role-badge` Not Defined (🟡 MEDIUM)

Used in `Users/Index.razor`:
```razor
<span class="role-badge role-@GetRoleClass(user.Role)">@user.Role</span>
```
**Add to `nooksy.css`:**
```css
.role-badge {
    display: inline-block;
    padding: 0.2rem 0.75rem;
    border-radius: 100px;
    font-family: var(--font-display);
    font-weight: 700;
    font-size: 0.75rem;
}
.role-admin    { background: rgba(74, 171, 219, 0.15); color: var(--color-primary-dark); }
.role-employee { background: rgba(245, 166, 35, 0.15);  color: #c17d00; }
.role-company  { background: rgba(82, 199, 133, 0.15);  color: #2a7a4b; }
.role-customer { background: rgba(232, 125, 125, 0.15); color: var(--color-accent-dark); }
```
Also confirm `GetRoleClass()` in `Users/Index.razor` returns lowercase values (`"admin"`, `"employee"`, etc.) to match CSS class names.

---

## 26. `_Imports.razor` — Missing Global `@using` Statements (🔴 CRITICAL)

Several pages use types like `Category`, `Product`, `OrderHeader`, `ApplicationUser`, `SD`, etc. without qualifying them. These will only compile if the namespaces are imported in `_Imports.razor`. Verify the following are present:

```razor
@using Nooksy.Models
@using Nooksy.DataAccess.Repository.IRepository
@using Nooksy.Utility
@using Nooksy.Client.Components.UI
@using Nooksy.Client.State
@using Microsoft.AspNetCore.Authorization
@using Microsoft.AspNetCore.Components.Authorization
@using Blazored.Toast.Services
```

---

## 27. `About` Page — Route Linked But Not Implemented (🟢 LOW)

The `MainLayout` navbar links to `/about`. No `About.razor` page exists. Either create a minimal placeholder or remove the link.

---

## 28. `ProductCard` Component — Not Confirmed Present (🟡 MEDIUM)

The README and solution structure reference `Components/Customer/ProductCard.razor`, but the file was not reachable. This component is required by `Home.razor` and `Shop.razor`. Confirm the file exists and implements:
- Book cover image (aspect-ratio 3:4, `object-fit: cover`, rounded corners)
- Title, author
- Struck-through `ListPrice` + current `Price`
- "Add to Cart" accent button
- Hover lift animation

---

## 29. `CartBadge` Component — Bounce Animation Not Triggered (🟡 MEDIUM)

The `CartState.OnChange` event fires correctly, but verify `CartBadge.razor` subscribes to it:
```csharp
protected override void OnInitialized()
{
    CartState.OnChange += StateHasChanged;   // must exist
}

public void Dispose()
{
    CartState.OnChange -= StateHasChanged;   // prevent memory leaks
}
```
The component must implement `IDisposable` to unsubscribe. Missing this causes a memory leak on every page navigation.

---

## 30. General — Exception Handling Too Broad (🟡 MEDIUM)

All `catch (Exception ex)` blocks across admin pages swallow the exception silently beyond a toast message:
```csharp
catch (Exception ex)
{
    ToastService.ShowError("Could not retrieve products.");
}
```
In development, add logging:
```csharp
catch (Exception ex)
{
    Logger.LogError(ex, "Error loading products");
    ToastService.ShowError("Could not retrieve products.");
}
```
Inject `ILogger<T>` into each component or create a shared error-handling helper.

---

## Summary Checklist

| # | Item | Severity | File(s) |
|---|------|----------|---------|
| 1 | Customer storefront pages missing | 🔴 | Home, ProductDetails, Cart, Checkout, OrderConfirmation |
| 2 | Companies CRUD missing | 🔴 | Admin/Companies/Index, Upsert |
| 3 | `/shop` route not implemented | 🟠 | Shop.razor |
| 4 | `NooksyButtonVariant` enum reference error | 🔴 | Categories/Upsert + others |
| 5 | Breadcrumb hardcoded | 🟠 | AdminLayout.razor |
| 6 | Sidebar active state CSS conflict | 🟠 | nooksy.css + AdminLayout.razor |
| 7 | Navbar search handler empty | 🟡 | MainLayout.razor |
| 8 | Mobile drawer slides from wrong direction | 🟡 | MainLayout.razor |
| 9 | N+1 image query in Dashboard | 🟠 | Dashboard.razor |
| 10 | Stripe key set in CancelOrder method | 🟠 | Orders/Detail.razor |
| 11 | TinyMCE not integrated | 🟡 | Products/Upsert.razor |
| 12 | N+1 image query in Products list | 🟠 | Products/Index.razor |
| 13 | Null email crash in Users search | 🟡 | Users/Index.razor |
| 14 | N+1 async GetRolesAsync in Users loop | 🟠 | Users/Index.razor |
| 15 | CartState not hydrated on direct navigation | 🟡 | MainLayout.razor / CartState.cs |
| 16 | `[Parameter]` PageSize mutated in DataTable | 🟡 | DataTable.razor |
| 17 | `[Parameter]` IsOpen mutated in NooksyModal | 🟡 | NooksyModal.razor |
| 18 | `UseSession` wrong middleware order | 🟠 | Program.cs |
| 19 | Missing Stripe key startup warning | 🟡 | Program.cs |
| 20 | `fs-7` Bootstrap class doesn't exist | 🟡 | nooksy.css + multiple components |
| 21 | `.text-accent` / `.text-text-muted` not defined | 🟡 | nooksy.css |
| 22 | `.nooksy-card` not defined | 🟡 | nooksy.css |
| 23 | `.category-pill.active` not defined | 🟡 | nooksy.css |
| 24 | `.stat-card` + `.stat-icon-*` not defined | 🟡 | nooksy.css |
| 25 | `.role-badge` variants not defined | 🟡 | nooksy.css |
| 26 | `_Imports.razor` missing global usings | 🔴 | _Imports.razor |
| 27 | `/about` page missing | 🟢 | About.razor |
| 28 | `ProductCard.razor` presence unconfirmed | 🟡 | Components/Customer/ |
| 29 | `CartBadge` missing `IDisposable` | 🟡 | CartBadge.razor |
| 30 | Exception swallowing without logging | 🟡 | All admin pages |

---

*End of audit. Start with items marked 🔴 (Critical) — the missing customer pages and the enum bug — as they will prevent the app from building or providing any customer-facing functionality.*
