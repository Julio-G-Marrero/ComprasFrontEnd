using Microsoft.AspNetCore.Components.Authorization;
using PurchaseReportManager.Proxy;
using PurchaseReportManager.ViewModels;
using Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPurchaseReportManagerProxy(options => builder.Configuration.GetSection(PurchaseReportProxyOptions.Section).Bind(options));
builder.Services.AddPurchaseReportManagerViewModels();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// TEMPORARY: no real login exists yet. These policies all allow any (stub)
// authenticated user, just so the Niux Shell sidebar's AuthorizeView checks
// don't throw. Replace with real policy rules once auth is implemented.
builder.Services.AddAuthorizationCore(options =>
{
    string[] policyNames =
    [
        "BudgetAuthorization.Page.Orders",
        "Valuation.Page.Orders",
        "Valuation.Page.AssignOrder",
        "Budget.Page.Orders",
        "MultiAccess.Page.Users",
        "MultiAccess.Page.Application",
        "MultiAccess.Page.Tenant",
        "MultiAccess.Page.Role",
        "Wearehouse.Page.Orders",
        "Wearehouse.Page.ViewCost",
        "Inventory.Page.Scan",
        "PurchaseReportManager.Page.Report",
    ];

    foreach (var policyName in policyNames)
    {
        options.AddPolicy(policyName, policy => policy.RequireAssertion(_ => true));
    }
});
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, StubAuthenticationStateProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(PurchaseReportManager.Views.Pages.PurchaseReportPage).Assembly);

app.Run();
