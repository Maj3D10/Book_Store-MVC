using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Nooksy.DataAccess.Repository.IRepository;
using Nooksy.Models;
using System.Linq.Expressions;
using System.Security.Claims;
using Nooksy.Utility;

namespace Nooksy.Tests.Unit.Helpers;

/// <summary>
/// Shared test helpers for bUnit tests.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Sets up bUnit context with a fake authenticated user.
    /// </summary>
    public static void SetAuthenticatedUser(
        TestContext ctx,
        string userId = "test-user-1",
        string name = "Test User",
        string role = SD.Role_Customer)
    {
        var authStateProvider = new FakeAuthStateProvider(userId, name, role);
        ctx.Services.AddSingleton<AuthenticationStateProvider>(authStateProvider);
        ctx.Services.AddAuthorizationCore();
    }

    /// <summary>
    /// Sets up bUnit context with a custom ClaimsPrincipal.
    /// </summary>
    public static void SetAuthenticatedUser(TestContext ctx, ClaimsPrincipal principal)
    {
        var authStateProvider = new FakeAuthStateProvider(principal);
        ctx.Services.AddSingleton<AuthenticationStateProvider>(authStateProvider);
        ctx.Services.AddAuthorizationCore();
    }

    /// <summary>
    /// Creates a mocked IUnitOfWork returning given seed data.
    /// </summary>
    public static Mock<IUnitOfWork> CreateMockUnitOfWork(
        List<Category>? categories = null,
        List<Product>? products = null,
        List<OrderHeader>? orders = null,
        List<OrderDetail>? orderDetails = null,
        List<ShoppingCart>? shoppingCarts = null,
        List<ProductImage>? productImages = null,
        List<ApplicationUser>? users = null,
        List<Company>? companies = null)
    {
        var mock = new Mock<IUnitOfWork>();

        // Repository mocks
        var catRepo = new Mock<ICategoryRepository>();
        catRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<Category, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<Category>, IOrderedQueryable<Category>>>()))
            .Returns((Expression<Func<Category, bool>>? filter, string? includeProperties, Func<IQueryable<Category>, IOrderedQueryable<Category>>? orderBy) =>
            {
                var query = (categories ?? new List<Category>()).AsQueryable();
                if (filter != null) query = query.Where(filter);
                if (orderBy != null) query = orderBy(query);
                return query.ToList();
            });
        catRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<Category, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .Returns((Expression<Func<Category, bool>> filter, string? includeProperties, bool tracked) =>
                (categories ?? new List<Category>()).AsQueryable().FirstOrDefault(filter)!);
        mock.Setup(u => u.Category).Returns(catRepo.Object);

        // Products
        var prodRepo = new Mock<IProductRepository>();
        prodRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>()))
            .Returns((Expression<Func<Product, bool>>? filter, string? includeProperties, Func<IQueryable<Product>, IOrderedQueryable<Product>>? orderBy) =>
            {
                var query = (products ?? new List<Product>()).AsQueryable();
                if (filter != null) query = query.Where(filter);
                if (orderBy != null) query = orderBy(query);
                return query.ToList();
            });
        prodRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .Returns((Expression<Func<Product, bool>> filter, string? includeProperties, bool tracked) =>
                (products ?? new List<Product>()).AsQueryable().FirstOrDefault(filter)!);
        mock.Setup(u => u.Product).Returns(prodRepo.Object);

        // Orders
        var orderRepo = new Mock<IOrderHeaderRepository>();
        orderRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<OrderHeader>, IOrderedQueryable<OrderHeader>>>()))
            .Returns((Expression<Func<OrderHeader, bool>>? filter, string? includeProperties, Func<IQueryable<OrderHeader>, IOrderedQueryable<OrderHeader>>? orderBy) =>
            {
                var query = (orders ?? new List<OrderHeader>()).AsQueryable();
                if (filter != null) query = query.Where(filter);
                if (orderBy != null) query = orderBy(query);
                return query.ToList();
            });
        orderRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .Returns((Expression<Func<OrderHeader, bool>> filter, string? includeProperties, bool tracked) =>
                (orders ?? new List<OrderHeader>()).AsQueryable().FirstOrDefault(filter)!);
        mock.Setup(u => u.orderHeader).Returns(orderRepo.Object);

        // Order Details
        var odRepo = new Mock<IOrderDetailRepository>();
        odRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<OrderDetail, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<OrderDetail>, IOrderedQueryable<OrderDetail>>>()))
            .Returns((Expression<Func<OrderDetail, bool>>? filter, string? includeProperties, Func<IQueryable<OrderDetail>, IOrderedQueryable<OrderDetail>>? orderBy) =>
            {
                var query = (orderDetails ?? new List<OrderDetail>()).AsQueryable();
                if (filter != null) query = query.Where(filter);
                if (orderBy != null) query = orderBy(query);
                return query.ToList();
            });
        mock.Setup(u => u.orderDetail).Returns(odRepo.Object);

        // Shopping Cart
        var cartRepo = new Mock<IShoppingCartRepository>();
        cartRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<ShoppingCart>, IOrderedQueryable<ShoppingCart>>>()))
            .Returns((Expression<Func<ShoppingCart, bool>>? filter, string? includeProperties, Func<IQueryable<ShoppingCart>, IOrderedQueryable<ShoppingCart>>? orderBy) =>
            {
                var query = (shoppingCarts ?? new List<ShoppingCart>()).AsQueryable();
                if (filter != null) query = query.Where(filter);
                if (orderBy != null) query = orderBy(query);
                return query.ToList();
            });
        cartRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .Returns((Expression<Func<ShoppingCart, bool>> filter, string? includeProperties, bool tracked) =>
                (shoppingCarts ?? new List<ShoppingCart>()).AsQueryable().FirstOrDefault(filter)!);
        mock.Setup(u => u.ShoppingCart).Returns(cartRepo.Object);

        // Product Images
        var piRepo = new Mock<IProductImageRepository>();
        piRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<ProductImage, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<ProductImage>, IOrderedQueryable<ProductImage>>>()))
            .Returns((Expression<Func<ProductImage, bool>>? filter, string? includeProperties, Func<IQueryable<ProductImage>, IOrderedQueryable<ProductImage>>? orderBy) =>
            {
                var query = (productImages ?? new List<ProductImage>()).AsQueryable();
                if (filter != null) query = query.Where(filter);
                if (orderBy != null) query = orderBy(query);
                return query.ToList();
            });
        mock.Setup(u => u.ProductImage).Returns(piRepo.Object);

        // Users
        var userRepo = new Mock<IApplicationUserRepository>();
        userRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<ApplicationUser, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<ApplicationUser>, IOrderedQueryable<ApplicationUser>>>()))
            .Returns((Expression<Func<ApplicationUser, bool>>? filter, string? includeProperties, Func<IQueryable<ApplicationUser>, IOrderedQueryable<ApplicationUser>>? orderBy) =>
            {
                var query = (users ?? new List<ApplicationUser>()).AsQueryable();
                if (filter != null) query = query.Where(filter);
                if (orderBy != null) query = orderBy(query);
                return query.ToList();
            });
        userRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ApplicationUser, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .Returns((Expression<Func<ApplicationUser, bool>> filter, string? includeProperties, bool tracked) =>
                (users ?? new List<ApplicationUser>()).AsQueryable().FirstOrDefault(filter)!);
        mock.Setup(u => u.ApplicationUser).Returns(userRepo.Object);

        // Companies
        var coRepo = new Mock<ICompanyRepository>();
        coRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<Company, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<Func<IQueryable<Company>, IOrderedQueryable<Company>>>()))
            .Returns((Expression<Func<Company, bool>>? filter, string? includeProperties, Func<IQueryable<Company>, IOrderedQueryable<Company>>? orderBy) =>
            {
                var query = (companies ?? new List<Company>()).AsQueryable();
                if (filter != null) query = query.Where(filter);
                if (orderBy != null) query = orderBy(query);
                return query.ToList();
            });
        mock.Setup(u => u.Company).Returns(coRepo.Object);

        return mock;
    }
}
