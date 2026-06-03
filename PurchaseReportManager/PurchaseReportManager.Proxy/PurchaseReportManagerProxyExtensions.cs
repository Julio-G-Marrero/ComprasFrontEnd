using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PurchaseReportManager.Proxy;

public static class PurchaseReportManagerProxyExtensions
{
    public static IServiceCollection AddPurchaseReportManagerProxy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PurchaseReportProxyOptions>(
            configuration.GetSection(PurchaseReportProxyOptions.Section));

        services.AddHttpClient<IPurchaseReportProxy, PurchaseReportProxy>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl no está configurado."));
        });

        return services;
    }
}
