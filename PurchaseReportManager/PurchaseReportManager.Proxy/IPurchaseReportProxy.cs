using Domain;

namespace PurchaseReportManager.Proxy;

public interface IPurchaseReportProxy
{
    Task<IReadOnlyList<TenantOption>> GetTenantsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetFamiliesAsync(
        string tenantId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetSubfamiliesAsync(
        string tenantId,
        string familia,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PurchaseReportLine>> GetPurchaseReportAsync(
        string tenantId,
        string familia,
        string? subFamilia,
        int windowDays,
        int reviewFrequencyDays,
        decimal serviceLevel,
        int defaultSupplierDays,
        decimal minOperationalStock,
        decimal xyzXThreshold,
        decimal xyzYThreshold,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PurchaseReportLine>> GetAllPurchaseReportAsync(
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
