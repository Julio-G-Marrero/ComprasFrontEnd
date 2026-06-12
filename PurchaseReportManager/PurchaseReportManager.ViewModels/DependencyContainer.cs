using Microsoft.Extensions.DependencyInjection;
using PurchaseReportManager.ViewModels.Abstractions;

namespace PurchaseReportManager.ViewModels;

public static class DependencyContainer
{
    public static IServiceCollection AddPurchaseReportManagerViewModels(
        this IServiceCollection services)
    {
        services.AddScoped<IPurchaseReportViewModel, PurchaseReportViewModel>();
        return services;
    }
}
