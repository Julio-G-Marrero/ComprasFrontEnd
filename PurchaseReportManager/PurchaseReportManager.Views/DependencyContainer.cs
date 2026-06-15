using Microsoft.Extensions.DependencyInjection;

namespace PurchaseReportManager.Views;

public static class DependencyContainer
{
    public static IServiceCollection AddPurchaseReportManagerViews(
        this IServiceCollection services)
    {
        return services;
    }
}
