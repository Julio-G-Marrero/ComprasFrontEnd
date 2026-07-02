using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PurchaseReportManager.Proxy.Abstractions;

namespace PurchaseReportManager.Proxy;

public static class DependencyContainer
{
    public static IServiceCollection AddPurchaseReportManagerProxy(
        this IServiceCollection services, Action<PurchaseReportProxyOptions> configureOptions)
    {
        PurchaseReportProxyOptions Options = new();
        configureOptions(Options);

        services.AddHttpClient<IPurchaseReportProxy, PurchaseReportProxy>(client =>
        {
            client.BaseAddress = new Uri(Options.BaseUrl
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl no está configurado."));
            client.Timeout = TimeSpan.FromMinutes(6);
        });

        return services;
    }
}
