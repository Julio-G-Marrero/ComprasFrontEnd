using Domain;
using PurchaseReportManager.Proxy;

namespace PurchaseReportManager.ViewModels;

public sealed class PurchaseReportViewModel(IPurchaseReportProxy proxy)
{
    private IReadOnlyList<PurchaseReportLine> _items = [];

    public string TenantId { get; set; } = "FSPCORONA_NEW";
    public string Familia { get; set; } = "3M";
    public string? SubFamilia { get; set; }

    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }
    public IReadOnlyList<PurchaseReportLine> Items => _items;
    public PurchaseReportLine? SelectedItem { get; private set; }

    public bool SoloConCompraSugerida { get; set; }
    public bool SoloCriticos { get; set; }
    public bool SoloRequiereRevision { get; set; }

    public IReadOnlyList<PurchaseReportLine> FilteredItems =>
        _items
            .Where(x => !SoloConCompraSugerida || x.TieneSugerencia)
            .Where(x => !SoloCriticos || x.EsCritico)
            .Where(x => !SoloRequiereRevision || x.RequiereRevision)
            .ToList()
            .AsReadOnly();

    public int TotalSkus => FilteredItems.Count;
    public int SkusConCompraSugerida => FilteredItems.Count(x => x.TieneSugerencia);
    public decimal CantidadTotalSugerida => FilteredItems.Sum(x => x.CantidadSugerida);
    public int ProductosCriticos => FilteredItems.Count(x => x.EsCritico);
    public int ProductosRevision => FilteredItems.Count(x => x.RequiereRevision);
    public decimal VentasTotales45Dias => FilteredItems.Sum(x => x.Ventas45Dias);
    public decimal TotalExistenciaEfectiva => FilteredItems.Sum(x => x.ExistenciaEfectiva);

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Familia))
        {
            ErrorMessage = "La familia es obligatoria.";
            return;
        }

        IsLoading = true;
        try
        {
            _items = await proxy.GetPurchaseReportAsync(TenantId, Familia, SubFamilia, cancellationToken);
            SelectedItem = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _items = [];
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void ClearFilters()
    {
        SoloConCompraSugerida = false;
        SoloCriticos = false;
        SoloRequiereRevision = false;
    }

    public void SelectItem(PurchaseReportLine item) => SelectedItem = item;
    public void ClearSelection() => SelectedItem = null;
}
