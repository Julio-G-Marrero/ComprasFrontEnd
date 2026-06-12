using Domain;

namespace PurchaseReportManager.Proxy.Abstractions;

public interface IPurchaseReportProxy
{
    Task<HandlerRequestResult<IReadOnlyList<TenantOption>>> GetTenantsAsync(
        CancellationToken cancellationToken = default);

    Task<HandlerRequestResult<IReadOnlyList<string>>> GetFamiliesAsync(
        string tenantId,
        CancellationToken cancellationToken = default);

    Task<HandlerRequestResult<IReadOnlyList<string>>> GetSubfamiliesAsync(
        string tenantId,
        string family,
        CancellationToken cancellationToken = default);

    Task<HandlerRequestResult<IReadOnlyList<PurchaseReportLine>>> GetPurchaseReportAsync(
        string tenantId,
        string family,
        string? subFamily,
        int windowDays,
        int reviewFrequencyDays,
        decimal serviceLevel,
        int defaultSupplierDays,
        decimal minOperationalStock,
        decimal xyzXThreshold,
        decimal xyzYThreshold,
        CancellationToken cancellationToken = default);

    Task<HandlerRequestResult<IReadOnlyList<PurchaseReportLine>>> GetAllPurchaseReportAsync(
        string tenantId,
        int windowDays,
        int reviewFrequencyDays,
        decimal serviceLevel,
        int defaultSupplierDays,
        decimal minOperationalStock,
        decimal xyzXThreshold,
        decimal xyzYThreshold,
        CancellationToken cancellationToken = default);
}
