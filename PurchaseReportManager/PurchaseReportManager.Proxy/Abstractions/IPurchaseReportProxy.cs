using Domain;
using Domain.Dtos;
using Domain.ValueObjects;

namespace PurchaseReportManager.Proxy.Abstractions;

public interface IPurchaseReportProxy
{
    Task<HandlerRequestResult<IReadOnlyList<TenantInfoDto>>> GetTenantsAsync(
        CancellationToken cancellationToken = default);

    Task<HandlerRequestResult<IReadOnlyList<string>>> GetFamiliesAsync(
        string tenantId,
        CancellationToken cancellationToken = default);

    Task<HandlerRequestResult<IReadOnlyList<string>>> GetSubfamiliesAsync(
        string tenantId,
        string family,
        CancellationToken cancellationToken = default);

    Task<HandlerRequestResult<IReadOnlyList<PurchaseReportLineDto>>> GetPurchaseReportAsync(
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

    Task<HandlerRequestResult<IReadOnlyList<PurchaseReportLineDto>>> GetAllPurchaseReportAsync(
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
