using Microsoft.Extensions.DependencyInjection;

namespace PurchaseReportManager.ViewModels;

public static class PurchaseReportManagerViewModelExtensions
{
    public static IServiceCollection AddPurchaseReportManagerViewModels(
        this IServiceCollection services)
    {
        services.AddScoped<IPurchaseReportViewModel, PurchaseReportViewModel>();
        return services;
    }
}
