using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Web.Components;

// TEMPORARY: no real login exists yet. Treats every visitor as an authenticated
// user with no roles/claims so AuthorizeView + the Niux Shell policies don't
// throw. Replace with a real AuthenticationStateProvider once login is implemented.
public class StubAuthenticationStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "stub-user")], authenticationType: "Stub");
        var user = new ClaimsPrincipal(identity);
        return Task.FromResult(new AuthenticationState(user));
    }
}
