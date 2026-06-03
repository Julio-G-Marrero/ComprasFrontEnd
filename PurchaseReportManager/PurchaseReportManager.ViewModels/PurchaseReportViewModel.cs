using Domain;
using PurchaseReportManager.Proxy;

namespace PurchaseReportManager.ViewModels;

public sealed class PurchaseReportViewModel(IPurchaseReportProxy proxy)
{
    private IReadOnlyList<PurchaseReportLine> _items = [];

    // Catálogos
    public IReadOnlyList<TenantOption> Tenants { get; private set; } = [];
    public IReadOnlyList<string> Families { get; private set; } = [];
    public IReadOnlyList<string> Subfamilies { get; private set; } = [];

    // Selecciones
    public string SelectedTenantId { get; set; } = string.Empty;
    public string SelectedFamilia { get; set; } = string.Empty;
    public string? SelectedSubFamilia { get; set; }

    // Estado de carga
    public bool IsLoading { get; private set; }
    public bool IsLoadingCatalog { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? CatalogErrorMessage { get; private set; }

    // Reporte
    public IReadOnlyList<PurchaseReportLine> Items => _items;
    public PurchaseReportLine? SelectedItem { get; private set; }

    // Filtros locales
    public bool SoloConCompraSugerida { get; set; }
    public bool SoloCriticos { get; set; }
    public bool SoloRequiereRevision { get; set; }
    public string SearchText { get; set; } = string.Empty;

    // Paginación
    public const int PageSize = 20;
    public int CurrentPage { get; private set; } = 1;

    public IReadOnlyList<PurchaseReportLine> FilteredItems =>
        _items
            .Where(x => !SoloConCompraSugerida || x.TieneSugerencia)
            .Where(x => !SoloCriticos || x.EsCritico)
            .Where(x => !SoloRequiereRevision || x.RequiereRevision)
            .Where(x => string.IsNullOrWhiteSpace(SearchText) ||
                        x.Sku.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        x.Descripcion.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();

    public int TotalPages => Math.Max(1, (int)Math.Ceiling(FilteredItems.Count / (double)PageSize));
    public int PagedFrom => FilteredItems.Count == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
    public int PagedTo => Math.Min(CurrentPage * PageSize, FilteredItems.Count);

    public IReadOnlyList<PurchaseReportLine> PagedItems =>
        FilteredItems.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList().AsReadOnly();

    // Resumen (calculado sobre FilteredItems)
    public int TotalSkus => FilteredItems.Count;
    public int SkusConCompraSugerida => FilteredItems.Count(x => x.TieneSugerencia);
    public decimal CantidadTotalSugerida => FilteredItems.Sum(x => x.CantidadSugerida);
    public int ProductosCriticos => FilteredItems.Count(x => x.EsCritico);
    public int ProductosRevision => FilteredItems.Count(x => x.RequiereRevision);
    public decimal VentasTotales45Dias => FilteredItems.Sum(x => x.Ventas45Dias);
    public decimal TotalExistenciaEfectiva => FilteredItems.Sum(x => x.ExistenciaEfectiva);

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        CatalogErrorMessage = null;
        IsLoadingCatalog = true;
        try
        {
            Tenants = await proxy.GetTenantsAsync(cancellationToken);

            var preferred = Tenants.FirstOrDefault(t => t.Id == "FSPCORONA_NEW")
                            ?? Tenants.FirstOrDefault();
            if (preferred is not null)
            {
                SelectedTenantId = preferred.Id;
                await LoadFamiliesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            CatalogErrorMessage = $"Error al cargar tenants: {ex.Message}";
        }
        finally
        {
            IsLoadingCatalog = false;
        }
    }

    public async Task OnTenantChangedAsync(CancellationToken cancellationToken = default)
    {
        SelectedFamilia = string.Empty;
        SelectedSubFamilia = null;
        Families = [];
        Subfamilies = [];
        _items = [];
        SelectedItem = null;
        CatalogErrorMessage = null;
        ResetPagination();

        if (string.IsNullOrEmpty(SelectedTenantId)) return;

        IsLoadingCatalog = true;
        try
        {
            await LoadFamiliesAsync(cancellationToken);
        }
        finally
        {
            IsLoadingCatalog = false;
        }
    }

    public async Task OnFamiliaChangedAsync(CancellationToken cancellationToken = default)
    {
        SelectedSubFamilia = null;
        Subfamilies = [];
        _items = [];
        SelectedItem = null;
        CatalogErrorMessage = null;
        ResetPagination();

        if (string.IsNullOrEmpty(SelectedFamilia)) return;

        IsLoadingCatalog = true;
        try
        {
            Subfamilies = await proxy.GetSubfamiliesAsync(SelectedTenantId, SelectedFamilia, cancellationToken);
        }
        catch (Exception ex)
        {
            CatalogErrorMessage = $"Error al cargar subfamilias: {ex.Message}";
        }
        finally
        {
            IsLoadingCatalog = false;
        }
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(SelectedTenantId))
        {
            ErrorMessage = "Debe seleccionar un tenant.";
            return;
        }
        if (string.IsNullOrWhiteSpace(SelectedFamilia))
        {
            ErrorMessage = "La familia es obligatoria.";
            return;
        }

        IsLoading = true;
        try
        {
            var subfamilia = string.IsNullOrEmpty(SelectedSubFamilia) ? null : SelectedSubFamilia;
            _items = await proxy.GetPurchaseReportAsync(SelectedTenantId, SelectedFamilia, subfamilia, cancellationToken);
            SelectedItem = null;
            SearchText = string.Empty;
            ResetPagination();
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

    private async Task LoadFamiliesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var all = await proxy.GetFamiliesAsync(SelectedTenantId, cancellationToken);
            Families = all.Where(f => !string.IsNullOrEmpty(f)).ToList().AsReadOnly();
        }
        catch (Exception ex)
        {
            CatalogErrorMessage = $"Error al cargar familias: {ex.Message}";
            Families = [];
        }
    }

    public void ResetPagination() => CurrentPage = 1;
    public void GoToNextPage() { if (CurrentPage < TotalPages) CurrentPage++; }
    public void GoToPreviousPage() { if (CurrentPage > 1) CurrentPage--; }

    public void ToggleSoloCompraSugerida() { SoloConCompraSugerida = !SoloConCompraSugerida; ResetPagination(); }
    public void ToggleSoloCriticos() { SoloCriticos = !SoloCriticos; ResetPagination(); }
    public void ToggleSoloRevision() { SoloRequiereRevision = !SoloRequiereRevision; ResetPagination(); }

    public void ClearFilters()
    {
        SoloConCompraSugerida = false;
        SoloCriticos = false;
        SoloRequiereRevision = false;
        SearchText = string.Empty;
        ResetPagination();
    }

    public void SelectItem(PurchaseReportLine item) => SelectedItem = item;
    public void ClearSelection() => SelectedItem = null;
}
