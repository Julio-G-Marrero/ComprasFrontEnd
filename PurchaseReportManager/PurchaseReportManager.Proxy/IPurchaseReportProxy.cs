using Domain;

namespace PurchaseReportManager.Proxy;

public interface IPurchaseReportProxy
{
    Task<IReadOnlyList<PurchaseReportLine>> GetPurchaseReportAsync(
        string tenantId,
        string familia,
        string? subFamilia = null,
        CancellationToken cancellationToken = default);
}
