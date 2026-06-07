# Nooksy Blazor — Frontend Testing Layer Plan

> **Stack:** Blazor Server · ASP.NET Core Identity · EF Core (SQL Server) · Stripe.net · Blazored.Toast  
> **Test pyramid target:** Unit (fast, isolated) → Integration (real DB, real DI) → E2E (real browser, real app)

---

## 1. Technology Stack Decisions

| Layer | Tool | Reason |
|-------|------|--------|
| **Unit** | `bUnit` + `xUnit` + `Moq` | bUnit is the standard Blazor component testing library; renders components in a headless DOM and exposes full Blazor lifecycle |
| **Unit assertions** | `FluentAssertions` | Readable, expressive assertion syntax |
| **Integration** | `Microsoft.AspNetCore.Mvc.Testing` + `EF Core InMemory` or `Testcontainers` (SQL Server) | Spins up the full Blazor Server host in-process; InMemory for speed, Testcontainers for fidelity |
| **E2E** | `Playwright` for .NET (`Microsoft.Playwright`) | First-class .NET API, reliable auto-waiting, network interception for Stripe mocking |
| **Code coverage** | `Coverlet` + `ReportGenerator` | Produces HTML coverage reports, integrates with CI |

---

## 2. Project Structure

```
Nooksy.sln
├── Nooksy.Client/                   ← app under test
├── Nooksy.Models/
├── Nooksy.DataAccess/
├── Nooksy.Utility/
│
└── tests/
    ├── Nooksy.Tests.Unit/           ← bUnit + xUnit + Moq
    │   ├── Components/
    │   │   ├── UI/
    │   │   │   ├── NooksyButtonTests.cs
    │   │   │   ├── NooksyBadgeTests.cs
    │   │   │   ├── NooksyModalTests.cs
    │   │   │   ├── NooksyAlertTests.cs
    │   │   │   ├── DataTableTests.cs
    │   │   │   ├── CartBadgeTests.cs
    │   │   │   ├── ImageUploaderTests.cs
    │   │   │   └── LoadingSpinnerTests.cs
    │   │   ├── Layout/
    │   │   │   ├── MainLayoutTests.cs
    │   │   │   └── AdminLayoutTests.cs
    │   │   └── Pages/
    │   │       ├── Admin/
    │   │       │   ├── DashboardTests.cs
    │   │       │   ├── CategoriesIndexTests.cs
    │   │       │   ├── CategoriesUpsertTests.cs
    │   │       │   ├── ProductsIndexTests.cs
    │   │       │   ├── ProductsUpsertTests.cs
    │   │       │   ├── OrdersIndexTests.cs
    │   │       │   ├── OrderDetailTests.cs
    │   │       │   └── UsersIndexTests.cs
    │   │       └── Customer/
    │   │           ├── HomeTests.cs
    │   │           ├── ProductDetailsTests.cs
    │   │           ├── CartTests.cs
    │   │           ├── CheckoutTests.cs
    │   │           └── OrderConfirmationTests.cs
    │   └── State/
    │       └── CartStateTests.cs
    │
    ├── Nooksy.Tests.Integration/    ← WebApplicationFactory + real DI
    │   ├── Setup/
    │   │   ├── NooksyWebAppFactory.cs
    │   │   └── DatabaseSeeder.cs
    │   ├── Admin/
    │   │   ├── CategoryCrudTests.cs
    │   │   ├── ProductCrudTests.cs
    │   │   ├── OrderWorkflowTests.cs
    │   │   └── UserManagementTests.cs
    │   └── Customer/
    │       ├── CartFlowTests.cs
    │       └── CheckoutFlowTests.cs
    │
    └── Nooksy.Tests.E2E/            ← Playwright, real browser
        ├── Setup/
        │   ├── PlaywrightFixture.cs
        │   └── TestDataSeeder.cs
        ├── Pages/
        │   ├── HomePageTests.cs
        │   ├── ShopPageTests.cs
        │   ├── CartPageTests.cs
        │   ├── CheckoutPageTests.cs
        │   └── AdminDashboardTests.cs
        └── Flows/
            ├── GuestBrowsingFlow.cs
            ├── CustomerPurchaseFlow.cs
            └── AdminOrderWorkflowFlow.cs
```

---

## 3. NuGet Packages Per Test Project

### `Nooksy.Tests.Unit`
```xml
<PackageReference Include="bunit" Version="1.*" />
<PackageReference Include="xunit" Version="2.*" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
<PackageReference Include="Moq" Version="4.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
<PackageReference Include="coverlet.collector" Version="6.*" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
<!-- For Blazor Server auth context in bUnit -->
<PackageReference Include="Microsoft.AspNetCore.Components.Authorization" Version="8.*" />
```

### `Nooksy.Tests.Integration`
```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.*" />
<!-- OR for SQL-fidelity: -->
<PackageReference Include="Testcontainers.MsSql" Version="3.*" />
<PackageReference Include="xunit" Version="2.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
```

### `Nooksy.Tests.E2E`
```xml
<PackageReference Include="Microsoft.Playwright" Version="1.*" />
<PackageReference Include="Microsoft.Playwright.MSTest" Version="1.*" />
<!-- or xunit runner: -->
<PackageReference Include="xunit" Version="2.*" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
```

---

## 4. Unit Tests — Component by Component

### 4.1 `CartState.cs` (pure C# — no bUnit needed)

```csharp
// Nooksy.Tests.Unit/State/CartStateTests.cs
public class CartStateTests
{
    // TEST: Initial CartCount is 0
    [Fact]
    public void InitialCartCount_IsZero()

    // TEST: SetCount sets exact value and fires OnChange
    [Fact]
    public void SetCount_SetsValueAndNotifies()

    // TEST: Increment adds 1 by default
    [Fact]
    public void Increment_DefaultAmount_AddsOne()

    // TEST: Increment adds custom amount
    [Theory, InlineData(3), InlineData(10)]
    public void Increment_CustomAmount_AddsCorrectly(int amount)

    // TEST: Decrement subtracts correctly
    [Fact]
    public void Decrement_ReducesCount()

    // TEST: Decrement cannot go below 0
    [Fact]
    public void Decrement_NeverGoesNegative()

    // TEST: OnChange NOT fired when value is set to same value (no spurious renders)
    [Fact]
    public void SetCount_SameValue_DoesNotFireOnChange()
}
```

---

### 4.2 `NooksyButton.razor`

**What to verify:** variant CSS class, loading spinner, disabled state, type attribute, OnClick callback.

```csharp
// Nooksy.Tests.Unit/Components/UI/NooksyButtonTests.cs
public class NooksyButtonTests : TestContext
{
    // TEST: Default render — has btn-nooksy-primary class
    [Fact]
    public void Renders_WithPrimaryVariantByDefault()

    // TEST: Accent variant → btn-nooksy-accent class
    [Theory]
    [InlineData(NooksyButton.NooksyButtonVariant.Accent, "btn-nooksy-accent")]
    [InlineData(NooksyButton.NooksyButtonVariant.Danger, "btn-nooksy-danger")]
    [InlineData(NooksyButton.NooksyButtonVariant.Outline, "btn-nooksy-outline")]
    public void Renders_CorrectClassForVariant(NooksyButtonVariant variant, string expectedClass)

    // TEST: IsLoading=true shows spinner, hides ChildContent
    [Fact]
    public void WhenLoading_ShowsSpinner_HidesContent()

    // TEST: IsLoading=true disables the button element
    [Fact]
    public void WhenLoading_ButtonIsDisabled()

    // TEST: Disabled=true disables without showing spinner
    [Fact]
    public void WhenDisabled_ButtonIsDisabledWithoutSpinner()

    // TEST: Type="submit" sets correct HTML attribute
    [Fact]
    public void Type_Submit_SetsHtmlAttribute()

    // TEST: OnClick fires when button is clicked
    [Fact]
    public void OnClick_Fires_WhenClicked()

    // TEST: OnClick does NOT fire when disabled
    [Fact]
    public void OnClick_DoesNotFire_WhenDisabled()

    // TEST: ChildContent renders text correctly
    [Fact]
    public void ChildContent_RendersInButton()

    // TEST: Size="sm" adds btn-sm class
    [Theory, InlineData("sm"), InlineData("lg")]
    public void Size_AddsCorrectSizeClass(string size)
}
```

---

### 4.3 `NooksyBadge.razor`

```csharp
public class NooksyBadgeTests : TestContext
{
    // TEST: "Pending" status → nooksy-badge-pending
    // TEST: "Approved" status → nooksy-badge-approved
    // TEST: "Processing" status → nooksy-badge-inprocess (SD.StatusInProcess = "Processing")
    // TEST: "Shipped" → nooksy-badge-shipped
    // TEST: "Cancelled" → nooksy-badge-cancelled
    // TEST: "Refunded" → nooksy-badge-refunded
    // TEST: "ApprovedForDelayedPayment" → nooksy-badge-approved (B2B mapping)
    // TEST: Unknown status → nooksy-badge-pending (fallback)
    // TEST: Empty status string → nooksy-badge-pending (null guard)
    // TEST: Text parameter renders inside the span
    [Theory]
    [InlineData("Pending",                    "nooksy-badge-pending")]
    [InlineData("Approved",                   "nooksy-badge-approved")]
    [InlineData("Processing",                 "nooksy-badge-inprocess")]
    [InlineData("Shipped",                    "nooksy-badge-shipped")]
    [InlineData("Cancelled",                  "nooksy-badge-cancelled")]
    [InlineData("Refunded",                   "nooksy-badge-refunded")]
    [InlineData("ApprovedForDelayedPayment",  "nooksy-badge-approved")]
    [InlineData("unknown-garbage",            "nooksy-badge-pending")]
    [InlineData("",                           "nooksy-badge-pending")]
    public void Status_MapsToCorrectCssClass(string status, string expectedClass)
}
```

---

### 4.4 `NooksyModal.razor`

```csharp
public class NooksyModalTests : TestContext
{
    // TEST: IsOpen=false → modal wrapper NOT rendered in DOM
    [Fact]
    public void WhenClosed_ModalIsNotInDom()

    // TEST: IsOpen=true → modal wrapper IS rendered
    [Fact]
    public void WhenOpen_ModalIsRendered()

    // TEST: Title parameter renders in the header h3
    [Fact]
    public void Title_RendersInHeader()

    // TEST: ChildContent renders inside nooksy-modal-body
    [Fact]
    public void ChildContent_RendersInModalBody()

    // TEST: Clicking close button invokes OnClose callback
    [Fact]
    public async Task CloseButton_Click_InvokesOnClose()

    // TEST: Clicking backdrop invokes OnClose callback
    [Fact]
    public async Task BackdropClick_InvokesOnClose()

    // REGRESSION (from audit item 17): CloseModal must NOT mutate IsOpen directly
    [Fact]
    public void CloseModal_DoesNotMutateIsOpenParameter()
    // Verify: after close button click, IsOpen [Parameter] still has the original value passed in
    // (only OnClose is invoked — parent owns state)

    // TEST: Width parameter sets max-width inline style
    [Fact]
    public void Width_SetsMaxWidthStyle()
}
```

---

### 4.5 `NooksyAlert.razor`

```csharp
public class NooksyAlertTests : TestContext
{
    // TEST: IsVisible=true by default → alert rendered
    [Fact]
    public void Alert_IsVisibleByDefault()

    // TEST: Message parameter renders text
    [Fact]
    public void Message_RendersTextContent()

    // TEST: ChildContent renders when Message is empty
    [Fact]
    public void ChildContent_RendersWhenMessageEmpty()

    // TEST: Dismissible=true shows close button
    [Fact]
    public void Dismissible_ShowsCloseButton()

    // TEST: Dismissible=false hides close button
    [Fact]
    public void NonDismissible_HidesCloseButton()

    // TEST: Clicking dismiss hides the alert (IsVisible→false)
    [Fact]
    public async Task DismissButton_Click_HidesAlert()

    // TEST: Type mapping — "Success"→alert-success, "Warning"→alert-warning, etc.
    [Theory]
    [InlineData("Success", "alert-success",  "check-circle-fill")]
    [InlineData("Warning", "alert-warning",  "exclamation-triangle-fill")]
    [InlineData("Error",   "alert-danger",   "x-circle-fill")]
    [InlineData("Info",    "alert-info",     "info-circle-fill")]
    public void Type_MapsToCorrectAlertClassAndIcon(string type, string cssClass, string iconClass)
}
```

---

### 4.6 `DataTable.razor`

```csharp
public class DataTableTests : TestContext
{
    // TEST: Renders correct number of rows for given Items
    [Fact]
    public void Renders_CorrectNumberOfRows()

    // TEST: Shows "No matching records" when Items is empty
    [Fact]
    public void EmptyItems_ShowsNoRecordsMessage()

    // TEST: Search filters rows matching query (uses SearchMatch)
    [Fact]
    public void SearchQuery_FiltersRows()

    // TEST: Search sets CurrentPage to 1
    [Fact]
    public void SearchQuery_ResetsToFirstPage()

    // TEST: Search with no match → empty state row
    [Fact]
    public void SearchQuery_NoMatch_ShowsNoRecordsMessage()

    // TEST: Pagination — only PageSize rows on page 1 when Items > PageSize
    [Fact]
    public void Pagination_ShowsOnlyPageSizeRowsOnFirstPage()

    // TEST: Next page shows next batch
    [Fact]
    public void NextPage_Button_ShowsNextBatch()

    // TEST: Previous page disabled on page 1
    [Fact]
    public void PreviousPage_DisabledOnFirstPage()

    // TEST: Next page disabled on last page
    [Fact]
    public void NextPage_DisabledOnLastPage()

    // TEST: Changing PageSize selector updates row count (REGRESSION from audit item 16)
    // Verify no Blazor "Parameter mutated" warning — use private backing field
    [Fact]
    public void PageSizeSelect_DoesNotMutateParameter()

    // TEST: Correct "Showing X to Y of Z entries" footer text
    [Fact]
    public void ShowsCorrectEntryCountFooter()

    // TEST: HeaderTemplate renders in <thead>
    [Fact]
    public void HeaderTemplate_RendersInThead()

    // TEST: RowTemplate renders per item
    [Fact]
    public void RowTemplate_RendersPerItem()
}
```

---

### 4.7 `CartBadge.razor`

```csharp
public class CartBadgeTests : TestContext
{
    // TEST: CartCount=0 → badge NOT rendered
    [Fact]
    public void WhenCartIsEmpty_BadgeNotRendered()

    // TEST: CartCount=5 → badge renders "5"
    [Fact]
    public void WhenCartHasItems_BadgeRendersCount()

    // TEST: CartState.OnChange → StateHasChanged called (badge updates)
    [Fact]
    public async Task OnCartStateChange_BadgeUpdates()

    // TEST: Component subscribes to CartState.OnChange on init
    [Fact]
    public void OnInitialized_SubscribesToCartStateOnChange()

    // TEST (REGRESSION from audit item 29): Component implements IDisposable
    // and unsubscribes on Dispose to prevent memory leak
    [Fact]
    public void Dispose_UnsubscribesFromCartStateOnChange()

    // TEST: After Dispose, CartState.OnChange no longer triggers renders
    [Fact]
    public async Task AfterDispose_CartStateChanges_DoNotTriggerStateHasChanged()
}
```

---

### 4.8 `DataTable.razor` + `LoadingSpinner.razor`

```csharp
public class LoadingSpinnerTests : TestContext
{
    // TEST: Message parameter renders in the paragraph
    [Fact]
    public void Message_RendersInParagraph()

    // TEST: Default message used when Message not set
    [Fact]
    public void DefaultMessage_UsedWhenNotProvided()

    // TEST: Contains spinner-border element
    [Fact]
    public void ContainsSpinnerBorderElement()
}
```

---

### 4.9 `Admin/Categories/Index.razor`

**Setup:** Mock `IUnitOfWork`, return seed categories. Mock `IToastService`.

```csharp
public class CategoriesIndexTests : TestContext
{
    // TEST: Page renders category list after load
    [Fact]
    public async Task OnLoad_ShowsCategories()

    // TEST: While isLoading=true, LoadingSpinner rendered
    // (hard to catch timing in bUnit — assert LoadingSpinner in initial render before OnInitializedAsync completes)
    [Fact]
    public void BeforeLoad_ShowsLoadingSpinner()

    // TEST: Create Category button links to /admin/categories/upsert
    [Fact]
    public void CreateButton_HasCorrectHref()

    // TEST: Edit button per row links to /admin/categories/upsert/{id}
    [Fact]
    public void EditButton_HasCorrectHrefPerRow()

    // TEST: Clicking Delete button opens modal
    [Fact]
    public async Task DeleteButton_Click_OpensModal()

    // TEST: Modal shows category name in confirmation message
    [Fact]
    public async Task DeleteModal_ShowsCategoryName()

    // TEST: Confirming delete calls UnitOfWork.Category.Remove + UnitOfWork.Save
    [Fact]
    public async Task ConfirmDelete_CallsRemoveAndSave()

    // TEST: Successful delete shows success toast
    [Fact]
    public async Task ConfirmDelete_ShowsSuccessToast()

    // TEST: Cancelling delete closes modal without calling Remove
    [Fact]
    public async Task CancelDelete_ClosesModal_NoRemoveCalled()

    // TEST: UnitOfWork throws → shows error toast, modal stays closed
    [Fact]
    public async Task DeleteException_ShowsErrorToast()

    // TEST: Page requires Admin or Employee role ([Authorize] attribute is present)
    [Fact]
    public void Page_HasAuthorizeAttributeWithAdminEmployeeRoles()
}
```

---

### 4.10 `Admin/Categories/Upsert.razor`

**REGRESSION coverage for audit item 4** — `NooksyButtonVariant` enum reference.

```csharp
public class CategoriesUpsertTests : TestContext
{
    // TEST: Id=0 → renders "Create Category" heading and button text
    [Fact]
    public void WhenIdIsZero_ShowsCreateMode()

    // TEST: Id=5 → loads existing category, renders "Edit Category"
    [Fact]
    public void WhenIdProvided_LoadsExistingCategory()

    // TEST: NooksyButton uses NooksyButton.NooksyButtonVariant.Primary (not bare NooksyButtonVariant)
    // Verify component compiles and renders — this is the regression test for audit item 4
    [Fact]
    public void SubmitButton_RendersWithoutEnumReferenceError()

    // TEST: Submitting empty form shows validation messages
    [Fact]
    public async Task EmptyForm_ShowsValidationMessages()

    // TEST: Valid form (create) → calls UnitOfWork.Category.Add + Save + navigates back
    [Fact]
    public async Task ValidCreate_CallsAddAndNavigates()

    // TEST: Valid form (edit) → calls UnitOfWork.Category.Update + Save + navigates back
    [Fact]
    public async Task ValidEdit_CallsUpdateAndNavigates()

    // TEST: IsLoading=true on submit → button shows spinner
    [Fact]
    public async Task OnSubmit_ButtonShowsSpinner()

    // TEST: Cancel link has href to /admin/categories
    [Fact]
    public void CancelLink_HasCorrectHref()
}
```

---

### 4.11 `Admin/Products/Index.razor`

```csharp
public class ProductsIndexTests : TestContext
{
    // TEST: Products load and display in DataTable
    [Fact]
    public async Task OnLoad_ShowsProductsInTable()

    // TEST: Cover image shown when ProductImages is non-empty
    [Fact]
    public async Task WithImages_ShowsCoverImage()

    // TEST: Fallback book icon shown when ProductImages is empty
    [Fact]
    public async Task WithoutImages_ShowsFallbackIcon()

    // TEST (REGRESSION audit item 12): LoadProducts does NOT make second GetAll call for images
    // Verify UnitOfWork.ProductImage.GetAll is called 0 times after the fix
    [Fact]
    public async Task LoadProducts_DoesNotCallProductImageGetAllSeparately()

    // TEST: Delete modal flow (same pattern as Categories)
    [Fact]
    public async Task DeleteFlow_RemovesImagesAndProduct()

    // TEST: Delete removes ProductImages first, then Product
    [Fact]
    public async Task Delete_RemovesImagesBeforeProduct()
}
```

---

### 4.12 `Admin/Orders/Index.razor`

```csharp
public class OrdersIndexTests : TestContext
{
    // TEST: All orders shown when selectedStatus=null (All tab active)
    [Fact]
    public async Task AllTab_ShowsAllOrders()

    // TEST: Filter by "Pending" → only Pending orders shown
    [Theory]
    [InlineData(SD.StatusPending)]
    [InlineData(SD.StatusApproved)]
    [InlineData(SD.StatusInProcess)]
    [InlineData(SD.StatusShipped)]
    [InlineData(SD.StatusCancelled)]
    public async Task StatusFilter_ShowsOnlyMatchingOrders(string status)

    // TEST: Active tab has "active" CSS class
    [Fact]
    public async Task ActiveFilterTab_HasActiveCssClass()

    // TEST: View Details button links to /admin/orders/{id}
    [Fact]
    public async Task ViewDetails_LinkHasCorrectHref()

    // TEST: NooksyBadge renders for both OrderStatus and PaymentStatus
    [Fact]
    public async Task Rows_ShowStatusBadges()
}
```

---

### 4.13 `Admin/Orders/Detail.razor`

```csharp
public class OrderDetailTests : TestContext
{
    // TEST: Order not found (null) → shows "Order not found" message, not crash
    [Fact]
    public async Task WhenOrderNotFound_ShowsNotFoundMessage()

    // TEST: Order header fields rendered (customer, address, dates)
    [Fact]
    public async Task OrderHeader_RendersCustomerAndShippingInfo()

    // TEST: Order items table renders all OrderDetail rows
    [Fact]
    public async Task OrderItems_RendersAllDetails()

    // TEST (REGRESSION audit item 9): Load does NOT call ProductImage.GetAll separately
    [Fact]
    public async Task LoadOrder_UsesIncludePropertiesForImages()

    // TEST: StartProcessing button shown when status is Approved/Pending
    [Theory, InlineData(SD.StatusApproved), InlineData(SD.StatusPending)]
    public async Task StartProcessing_Button_ShownForCorrectStatuses(string status)

    // TEST: Ship Order inputs shown only when status is InProcess
    [Fact]
    public async Task ShipOrderInputs_ShownOnlyWhenInProcess()

    // TEST: ShipOrder with empty carrier/tracking shows warning toast
    [Fact]
    public async Task ShipOrder_EmptyCarrier_ShowsWarningToast()

    // TEST: ShipOrder with valid inputs calls UpdateStatus + Save
    [Fact]
    public async Task ShipOrder_Valid_UpdatesStatusAndSaves()

    // TEST (REGRESSION audit item 10): CancelOrder does NOT set StripeConfiguration.ApiKey mid-call
    // Verify StripeConfiguration.ApiKey is already set from Program.cs — CancelOrder must not override it
    [Fact]
    public async Task CancelOrder_DoesNotMutateStripeConfigApiKey()

    // TEST: Cancel button NOT shown for Shipped/Cancelled/Refunded orders
    [Theory, InlineData(SD.StatusShipped), InlineData(SD.StatusCancelled), InlineData(SD.StatusRefunded)]
    public async Task CancelButton_HiddenForFinalStatuses(string status)
}
```

---

### 4.14 `Admin/Users/Index.razor`

```csharp
public class UsersIndexTests : TestContext
{
    // TEST: Users list renders with Name, Email, Role columns
    [Fact]
    public async Task OnLoad_ShowsUserList()

    // TEST (REGRESSION audit item 13): Null email does not crash search
    [Fact]
    public async Task SearchWithNullEmail_DoesNotThrowNullReferenceException()

    // TEST: GetRoleClass maps correctly to CSS suffix
    [Theory]
    [InlineData("Admin",    "admin")]
    [InlineData("Customer", "customer")]
    [InlineData("Company",  "company")]
    [InlineData("Employee", "employee")]
    [InlineData(null,       "customer")]
    public void GetRoleClass_ReturnsCorrectCssSuffix(string role, string expected)

    // TEST: IsUserLocked=true → shows Lock badge and "Unlock" button
    [Fact]
    public async Task LockedUser_ShowsLockBadgeAndUnlockButton()

    // TEST: IsUserLocked=false → shows Active badge and "Lock" button
    [Fact]
    public async Task ActiveUser_ShowsActiveBadgeAndLockButton()

    // TEST: ToggleLock — unlocking calls UserManager.SetLockoutEndDateAsync(user, null)
    [Fact]
    public async Task ToggleLock_Unlock_SetsLockoutEndToNull()

    // TEST: ToggleLock — locking calls SetLockoutEndDateAsync with 100 years from now
    [Fact]
    public async Task ToggleLock_Lock_SetsLockoutEndFarFuture()

    // TEST: Company name resolved from companies list (no FK navigation needed in view)
    [Fact]
    public async Task CompanyName_ResolvedFromCompaniesList()
}
```

---

### 4.15 `Admin/Dashboard.razor`

```csharp
public class DashboardTests : TestContext
{
    // TEST: Stat cards render correct totals from UnitOfWork
    [Fact]
    public async Task StatCards_ShowCorrectTotals()

    // TEST: Revenue only sums Approved + DelayedPayment orders
    [Fact]
    public async Task Revenue_OnlySumsApprovedAndDelayedPaymentOrders()

    // TEST (REGRESSION audit item 9): No second ProductImage.GetAll call
    [Fact]
    public async Task OnLoad_NoRedundantProductImageQuery()

    // TEST: Warning alert shown when products with no images exist
    [Fact]
    public async Task WithProductsWithNoImages_ShowsWarningAlert()

    // TEST: No warning when all products have images
    [Fact]
    public async Task WhenAllProductsHaveImages_NoWarningAlert()

    // TEST: Recent orders table shows last 10 by descending date
    [Fact]
    public async Task RecentOrders_ShowsLast10OrdersByDate()

    // TEST: UnitOfWork throws → shows error toast, page does not crash
    [Fact]
    public async Task OnLoadException_ShowsErrorToast()
}
```

---

### 4.16 `MainLayout.razor`

**Note:** MainLayout injects `IUnitOfWork`, `AuthStateProvider`, and `CartState`. Mock all three.

```csharp
public class MainLayoutTests : TestContext
{
    // TEST: Unauthenticated user — cart icon links to /Account/Login
    [Fact]
    public async Task UnauthenticatedUser_CartLinkGoesToLogin()

    // TEST: Authenticated user — cart icon links to /cart
    [Fact]
    public async Task AuthenticatedUser_CartLinkGoesToCart()

    // TEST: Authenticated user — CartState.SetCount called with DB cart item count on init
    [Fact]
    public async Task OnInit_CartState_SetCountCalledWithCartItemCount()

    // TEST: Admin/Employee sees "Admin Portal" link
    [Fact]
    public async Task AdminUser_SeesAdminPortalLink()

    // TEST: Customer role — no "Admin Portal" link
    [Fact]
    public async Task CustomerUser_NoAdminPortalLink()

    // TEST: Mobile menu toggle — ToggleMobileMenu shows/hides drawer
    [Fact]
    public async Task HamburgerButton_TogglesDrawer()

    // TEST (REGRESSION audit item 8): Mobile drawer slides from LEFT (translateX(-100%))
    [Fact]
    public void MobileDrawer_SlideInAnimation_IsFromLeft()

    // TEST (REGRESSION audit item 7): HandleSearch with key "Enter" navigates to /shop?q=
    [Fact]
    public async Task HandleSearch_EnterKey_NavigatesToShopWithQuery()

    // TEST: HandleSearch with non-Enter key does nothing
    [Fact]
    public async Task HandleSearch_NonEnterKey_DoesNotNavigate()

    // TEST: Unauthenticated — shows Login and Register buttons
    [Fact]
    public async Task Unauthenticated_ShowsLoginAndRegisterButtons()

    // TEST: Authenticated — shows user dropdown with Name initial
    [Fact]
    public async Task Authenticated_ShowsUserDropdown()
}
```

---

## 5. Integration Tests

Integration tests use `WebApplicationFactory<Program>` with a real DI container and either `EF Core InMemory` or a SQL Server Testcontainer. They do **not** use bUnit — they test the full Blazor Server request/response pipeline.

### 5.1 Factory Setup

```csharp
// Nooksy.Tests.Integration/Setup/NooksyWebAppFactory.cs
public class NooksyWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove real DbContext, replace with InMemory
            var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("NooksyTestDb_" + Guid.NewGuid()));

            // Stripe — use a test/mock key so no real charges
            services.Configure<StripeSettings>(opts => opts.SecretKey = "sk_test_fake");

            // Seed test data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            DatabaseSeeder.Seed(db);
        });
    }
}
```

```csharp
// Nooksy.Tests.Integration/Setup/DatabaseSeeder.cs
public static class DatabaseSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (!db.Categories.Any())
        {
            db.Categories.AddRange(
                new Category { Id = 1, Name = "Fiction",    DisplayOrder = 1 },
                new Category { Id = 2, Name = "Non-Fiction", DisplayOrder = 2 }
            );
        }
        if (!db.Products.Any())
        {
            db.Products.Add(new Product
            {
                Id = 1, Title = "Test Book", Author = "Author A", ISBN = "1234567890123",
                Description = "A test book", ListPrice = 19.99, Price = 14.99,
                Price50 = 12.99, Price100 = 10.99, CategoryId = 1
            });
        }
        db.SaveChanges();
    }
}
```

---

### 5.2 Category CRUD Integration

```csharp
// Nooksy.Tests.Integration/Admin/CategoryCrudTests.cs
public class CategoryCrudTests : IClassFixture<NooksyWebAppFactory>
{
    // TEST: GET /admin/categories returns 200 (when authenticated as Admin)
    [Fact]
    public async Task GetCategories_Returns200_WhenAdmin()

    // TEST: GET /admin/categories returns 302 redirect (when unauthenticated)
    [Fact]
    public async Task GetCategories_Redirects_WhenUnauthenticated()

    // TEST: Category added via UnitOfWork persists in DB
    [Fact]
    public async Task AddCategory_PersistsToDatabase()

    // TEST: Category updated via UnitOfWork reflects in subsequent GetAll
    [Fact]
    public async Task UpdateCategory_ReflectsInGetAll()

    // TEST: Category removed via UnitOfWork no longer appears in GetAll
    [Fact]
    public async Task RemoveCategory_NoLongerInGetAll()

    // TEST: Category.Name MaxLength(30) validation enforced at DB level
    [Fact]
    public async Task CategoryName_ExceedingMaxLength_ThrowsDbException()

    // TEST: Category.DisplayOrder out of range fails DataAnnotations
    [Fact]
    public async Task CategoryDisplayOrder_OutOfRange_FailsValidation()
}
```

---

### 5.3 Product CRUD Integration

```csharp
public class ProductCrudTests : IClassFixture<NooksyWebAppFactory>
{
    // TEST: Add product with all required fields → persists with correct FK
    [Fact]
    public async Task AddProduct_WithValidData_PersistsWithCategoryFk()

    // TEST: Add ProductImage → associated to correct ProductId
    [Fact]
    public async Task AddProductImage_AssociatesToProduct()

    // TEST: GetAll with includeProperties:"Category,ProductImages" → navigations loaded
    [Fact]
    public async Task GetAllWithIncludes_LoadsNavigationProperties()

    // TEST: Remove product → associated ProductImages cascade deleted (check DB constraint)
    [Fact]
    public async Task RemoveProduct_CascadeDeletesProductImages()

    // TEST: Price fields accept 0.01–1000 range; out-of-range throws
    [Theory, InlineData(0), InlineData(1001)]
    public async Task ProductPrice_OutOfRange_FailsRangeValidation(double price)
}
```

---

### 5.4 Order Workflow Integration

```csharp
public class OrderWorkflowTests : IClassFixture<NooksyWebAppFactory>
{
    // TEST: Create OrderHeader with Status=Pending → persists
    [Fact]
    public async Task CreateOrder_PersistsWithPendingStatus()

    // TEST: UpdateStatus → Approved changes OrderStatus in DB
    [Fact]
    public async Task UpdateStatus_ToApproved_ChangesDbRecord()

    // TEST: UpdateStatus → Shipped updates Carrier, TrackingNumber, ShippingDate
    [Fact]
    public async Task UpdateStatus_ToShipped_UpdatesShippingFields()

    // TEST: Revenue sum only counts Approved + ApprovedForDelayedPayment statuses
    [Fact]
    public async Task Revenue_ExcludesPendingAndCancelledOrders()

    // TEST: OrderHeader without shipping address fails required validation
    [Fact]
    public async Task OrderHeader_MissingRequiredFields_FailsValidation()
}
```

---

### 5.5 Cart Flow Integration

```csharp
public class CartFlowTests : IClassFixture<NooksyWebAppFactory>
{
    // TEST: Add item to cart → ShoppingCart record created with correct ProductId + UserId
    [Fact]
    public async Task AddToCart_CreatesShoppingCartRecord()

    // TEST: Increment cart item count → Count updated in DB
    [Fact]
    public async Task IncrementCartItem_UpdatesCount()

    // TEST: Remove cart item → record deleted from DB
    [Fact]
    public async Task RemoveCartItem_DeletesRecord()

    // TEST: GetAll with includeProperties:"Product,Product.ProductImages" → all loaded in one query
    [Fact]
    public async Task GetCartWithIncludes_LoadsProductAndImages()

    // TEST: Price computed via PricingRules for tier 1–49
    [Theory, InlineData(1, 14.99), InlineData(49, 14.99)]
    public async Task CartPrice_UsesCorrectTier_For1To49(int count, double expectedPrice)

    // TEST: Price computed for tier 50–99
    [Theory, InlineData(50, 12.99), InlineData(99, 12.99)]
    public async Task CartPrice_UsesCorrectTier_For50To99(int count, double expectedPrice)

    // TEST: Price computed for tier 100+
    [Theory, InlineData(100, 10.99)]
    public async Task CartPrice_UsesCorrectTier_For100Plus(int count, double expectedPrice)
}
```

---

## 6. E2E Tests — Playwright

E2E tests boot a real Blazor Server app (via `WebApplicationFactory` or a local `dotnet run`), open a real browser (Chromium by default), and assert UI state.

### 6.1 Playwright Fixture

```csharp
// Nooksy.Tests.E2E/Setup/PlaywrightFixture.cs
public class PlaywrightFixture : IAsyncLifetime
{
    public IBrowser Browser { get; private set; } = null!;
    public string BaseUrl { get; } = "http://localhost:5100";

    public async Task InitializeAsync()
    {
        var playwright = await Playwright.CreateAsync();
        Browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = true,
            SlowMo = 0   // increase locally for debugging
        });
    }

    public async Task DisposeAsync() => await Browser.CloseAsync();
}
```

**Network interception for Stripe:** Use `page.RouteAsync("https://api.stripe.com/**", ...)` to return mock successful responses and avoid real charges in E2E.

---

### 6.2 Guest Browsing Flow

```csharp
// Nooksy.Tests.E2E/Flows/GuestBrowsingFlow.cs
public class GuestBrowsingFlow : IClassFixture<PlaywrightFixture>
{
    // TEST: Home page loads — hero heading visible
    [Fact]
    public async Task HomePage_HeroHeading_IsVisible()

    // TEST: Clicking "Browse Books" navigates to /shop
    [Fact]
    public async Task HomePageCTA_BrowseBooks_NavigatesToShop()

    // TEST: Product cards render on shop page (at least 1 visible)
    [Fact]
    public async Task ShopPage_ShowsProductCards()

    // TEST: Clicking a product card navigates to /product/{id}
    [Fact]
    public async Task ProductCard_Click_NavigatesToDetailPage()

    // TEST: Product detail page shows title, author, pricing table
    [Fact]
    public async Task ProductDetailPage_ShowsTitleAndPricingTable()

    // TEST: Clicking "Add to Cart" as guest redirects to /Account/Login
    [Fact]
    public async Task AddToCart_AsGuest_RedirectsToLogin()

    // TEST: Cart icon for guest links to /Account/Login (not /cart)
    [Fact]
    public async Task CartIcon_AsGuest_LinksToLogin()

    // TEST: Navbar shows Login + Register buttons when unauthenticated
    [Fact]
    public async Task Navbar_Unauthenticated_ShowsLoginAndRegisterButtons()

    // TEST: Mobile nav hamburger opens drawer
    [Fact]
    public async Task MobileNav_HamburgerClick_OpensDrawer()

    // TEST: Search Enter key navigates to /shop?q=<term>
    [Fact]
    public async Task SearchBar_EnterKey_NavigatesToShopWithQuery()
}
```

---

### 6.3 Customer Purchase Flow (Happy Path)

```csharp
// Nooksy.Tests.E2E/Flows/CustomerPurchaseFlow.cs
public class CustomerPurchaseFlow : IClassFixture<PlaywrightFixture>
{
    // SETUP: Register a test customer user (or seed one and log in)

    // TEST: Authenticated customer — Add to Cart shows success toast
    [Fact]
    public async Task AddToCart_AsCustomer_ShowsSuccessToast()

    // TEST: Cart badge increments after Add to Cart
    [Fact]
    public async Task AddToCart_CartBadgeIncrements()

    // TEST: Cart page (/cart) shows added item
    [Fact]
    public async Task CartPage_ShowsAddedItem()

    // TEST: Cart — quantity +/- updates line total
    [Fact]
    public async Task CartPage_QuantityChange_UpdatesLineTotal()

    // TEST: Cart — Remove item removes row and updates badge
    [Fact]
    public async Task CartPage_RemoveItem_UpdatesBadge()

    // TEST: Checkout page — pre-filled from user profile
    [Fact]
    public async Task CheckoutPage_PreFillsUserAddress()

    // TEST: Checkout page — validation errors shown on empty submit
    [Fact]
    public async Task CheckoutPage_EmptySubmit_ShowsValidationErrors()

    // TEST: Checkout with mocked Stripe → redirected to Order Confirmation
    [Fact]
    public async Task CheckoutFlow_WithMockedStripe_ReachesConfirmation()

    // TEST: Order Confirmation page shows order number and "Continue Shopping" button
    [Fact]
    public async Task OrderConfirmationPage_ShowsOrderNumber()

    // TEST: Cart badge is 0 after successful checkout
    [Fact]
    public async Task AfterCheckout_CartBadgeIsZero()
}
```

---

### 6.4 Admin Order Workflow Flow

```csharp
// Nooksy.Tests.E2E/Flows/AdminOrderWorkflowFlow.cs
public class AdminOrderWorkflowFlow : IClassFixture<PlaywrightFixture>
{
    // SETUP: Seed a Pending order; log in as Admin

    // TEST: Admin dashboard loads with stat cards
    [Fact]
    public async Task Dashboard_LoadsWithStatCards()

    // TEST: Recent orders table shows last order
    [Fact]
    public async Task Dashboard_RecentOrdersTable_ShowsOrder()

    // TEST: Admin can filter orders by status on /admin/orders
    [Fact]
    public async Task OrdersList_StatusFilter_FiltersCorrectly()

    // TEST: Admin clicks View Details → Order Detail page loads
    [Fact]
    public async Task ViewDetails_NavigatesToOrderDetail()

    // TEST: "Start Processing" button changes status badge to Processing
    [Fact]
    public async Task StartProcessing_UpdatesStatusBadge()

    // TEST: Carrier + Tracking Number inputs appear after Processing
    [Fact]
    public async Task AfterProcessing_ShippingInputsAppear()

    // TEST: Shipping without carrier → warning toast
    [Fact]
    public async Task ShipOrder_NoCarrier_ShowsWarning()

    // TEST: Ship order with valid inputs → status badge changes to Shipped
    [Fact]
    public async Task ShipOrder_Valid_UpdatesStatusToShipped()

    // TEST: Cancel button disappears after order is Shipped
    [Fact]
    public async Task AfterShipped_CancelButtonHidden()

    // TEST: Sidebar active state — Products link highlighted on /admin/products
    [Fact]
    public async Task Sidebar_ActiveState_HighlightsCurrentPage()

    // TEST (REGRESSION audit item 5): Breadcrumb shows "Products" not "Portal" on /admin/products
    [Fact]
    public async Task Breadcrumb_ShowsCorrectSectionName()
}
```

---

### 6.5 Admin Category + Product CRUD (E2E)

```csharp
// Nooksy.Tests.E2E/Pages/AdminDashboardTests.cs
public class AdminCrudTests : IClassFixture<PlaywrightFixture>
{
    // TEST: Create category → appears in category list
    [Fact]
    public async Task CreateCategory_AppearsInList()

    // TEST: Edit category → name updated in list
    [Fact]
    public async Task EditCategory_UpdatesInList()

    // TEST: Delete category → confirmation modal appears, confirm removes from list
    [Fact]
    public async Task DeleteCategory_ModalAndRemovesFromList()

    // TEST: Create product → appears in product list with cover image
    [Fact]
    public async Task CreateProduct_AppearsInList()

    // TEST: Image uploader — drag zone visible on product upsert
    [Fact]
    public async Task ProductUpsert_ImageUploader_IsVisible()

    // TEST: Delete product → confirmation modal, removed from list
    [Fact]
    public async Task DeleteProduct_RemovedFromList()
}
```

---

## 7. Test Coverage Targets

| Area | Unit | Integration | E2E |
|------|------|-------------|-----|
| `CartState` | 100% | — | — |
| UI Components (`NooksyButton`, `NooksyBadge`, `NooksyModal`, `NooksyAlert`, `DataTable`, `CartBadge`, `LoadingSpinner`, `ImageUploader`) | ≥ 90% | — | — |
| Admin Pages (all 6) | ≥ 80% | ≥ 70% | Key flows |
| Customer Pages (all 5 — once built) | ≥ 80% | ≥ 60% | Full purchase flow |
| Models / Data annotations | — | ≥ 80% | — |
| Auth / Role guards | Unit (attribute check) | redirect test | login flow |
| Stripe integration | mocked at unit level | mocked | network-intercepted |
| **Overall branch coverage target** | **≥ 80%** | **≥ 65%** | **Critical paths only** |

---

## 8. CI Pipeline Configuration (GitHub Actions)

```yaml
# .github/workflows/test.yml
name: Nooksy Tests

on: [push, pull_request]

jobs:
  unit-and-integration:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '8.x' }

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Unit Tests + Coverage
        run: |
          dotnet test tests/Nooksy.Tests.Unit \
            --collect:"XPlat Code Coverage" \
            --results-directory ./coverage \
            --no-build --configuration Release

      - name: Integration Tests
        run: |
          dotnet test tests/Nooksy.Tests.Integration \
            --no-build --configuration Release

      - name: Coverage Report
        uses: danielpalme/ReportGenerator-GitHub-Action@5
        with:
          reports: coverage/**/coverage.cobertura.xml
          targetdir: coverage/report
          reporttypes: HtmlInline_AzurePipelines;Cobertura

      - name: Upload Coverage
        uses: actions/upload-artifact@v4
        with:
          name: coverage-report
          path: coverage/report

  e2e:
    runs-on: ubuntu-latest
    needs: unit-and-integration
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '8.x' }

      - name: Install Playwright Browsers
        run: pwsh tests/Nooksy.Tests.E2E/bin/Release/net8.0/playwright.ps1 install --with-deps chromium

      - name: Start App Server
        run: |
          dotnet run --project Nooksy.Client --configuration Release &
          sleep 10  # wait for server to start

      - name: E2E Tests
        run: dotnet test tests/Nooksy.Tests.E2E --no-build --configuration Release
        env:
          BASE_URL: http://localhost:5000
          ConnectionStrings__AppDbContextConnection: "Server=...;..."
```

---

## 9. bUnit Setup Helpers (Reusable)

These helpers reduce boilerplate across test classes.

```csharp
// Nooksy.Tests.Unit/Helpers/TestHelpers.cs

/// <summary>Sets up bUnit context with a fake authenticated user.</summary>
public static void SetAuthenticatedUser(
    TestContext ctx,
    string userId = "user-1",
    string name = "Test User",
    string role = SD.Role_Customer)
{
    var authState = Task.FromResult(new AuthenticationState(
        new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, role),
        }, "TestAuth"))));

    ctx.Services.AddSingleton<AuthenticationStateProvider>(
        new FakeAuthStateProvider(authState));
    ctx.Services.AddAuthorizationCore();
}

/// <summary>Creates a mocked IUnitOfWork returning given seed data.</summary>
public static Mock<IUnitOfWork> CreateMockUnitOfWork(
    List<Category>? categories = null,
    List<Product>? products = null,
    List<OrderHeader>? orders = null)
{
    var mock = new Mock<IUnitOfWork>();

    // Categories
    var catRepo = new Mock<ICategoryRepository>();
    catRepo.Setup(r => r.GetAll(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<string>(), It.IsAny<Func<IQueryable<Category>, IOrderedQueryable<Category>>>()))
           .Returns(categories ?? new List<Category>());
    mock.Setup(u => u.Category).Returns(catRepo.Object);

    // Products
    var prodRepo = new Mock<IProductRepository>();
    prodRepo.Setup(r => r.GetAll(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string>(), It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>()))
            .Returns(products ?? new List<Product>());
    mock.Setup(u => u.Product).Returns(prodRepo.Object);

    // Orders
    var orderRepo = new Mock<IOrderHeaderRepository>();
    orderRepo.Setup(r => r.GetAll(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>(), It.IsAny<Func<IQueryable<OrderHeader>, IOrderedQueryable<OrderHeader>>>()))
             .Returns(orders ?? new List<OrderHeader>());
    mock.Setup(u => u.orderHeader).Returns(orderRepo.Object);

    return mock;
}
```

---

## 10. Key Regression Tests Summary

These tests directly target the **30 bugs found in the audit**. They must all pass before merging any fix PR.

| Audit # | Bug | Test Class | Test Method |
|---------|-----|------------|-------------|
| 4 | `NooksyButtonVariant` enum ref error | `CategoriesUpsertTests` | `SubmitButton_RendersWithoutEnumReferenceError` |
| 7 | Search handler empty | `MainLayoutTests` | `HandleSearch_EnterKey_NavigatesToShopWithQuery` |
| 8 | Mobile drawer wrong direction | `MainLayoutTests` | `MobileDrawer_SlideInAnimation_IsFromLeft` |
| 9 | N+1 image query Dashboard | `DashboardTests` | `OnLoad_NoRedundantProductImageQuery` |
| 9 | N+1 image query Products | `ProductsIndexTests` | `LoadProducts_DoesNotCallProductImageGetAllSeparately` |
| 9 | N+1 image query OrderDetail | `OrderDetailTests` | `LoadOrder_UsesIncludePropertiesForImages` |
| 10 | Stripe key set in method | `OrderDetailTests` | `CancelOrder_DoesNotMutateStripeConfigApiKey` |
| 13 | Null email crash | `UsersIndexTests` | `SearchWithNullEmail_DoesNotThrowNullReferenceException` |
| 15 | CartState not hydrated | `MainLayoutTests` | `OnInit_CartState_SetCountCalledWithCartItemCount` |
| 16 | `[Parameter]` PageSize mutated | `DataTableTests` | `PageSizeSelect_DoesNotMutateParameter` |
| 17 | `[Parameter]` IsOpen mutated | `NooksyModalTests` | `CloseModal_DoesNotMutateIsOpenParameter` |
| 29 | CartBadge memory leak | `CartBadgeTests` | `Dispose_UnsubscribesFromCartStateOnChange` |
| 5 | Breadcrumb hardcoded | `AdminOrderWorkflowFlow` | `Breadcrumb_ShowsCorrectSectionName` |
| 6 | Sidebar active state | `AdminOrderWorkflowFlow` | `Sidebar_ActiveState_HighlightsCurrentPage` |

---

*Total planned tests: ~130 unit · ~35 integration · ~40 E2E = ~205 tests across the three layers.*
