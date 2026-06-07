using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Nooksy.Tests.Unit.Helpers;

/// <summary>
/// Fake AuthenticationStateProvider for bUnit tests.
/// Returns a pre-configured AuthenticationState without needing real Identity.
/// </summary>
public class FakeAuthStateProvider : AuthenticationStateProvider
{
    private readonly Task<AuthenticationState> _authState;

    public FakeAuthStateProvider(string userId = "test-user-1", string name = "Test User", string role = "Customer")
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, role),
        }, "TestAuth"));

        _authState = Task.FromResult(new AuthenticationState(principal));
    }

    public FakeAuthStateProvider(ClaimsPrincipal principal)
    {
        _authState = Task.FromResult(new AuthenticationState(principal));
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => _authState;
}
