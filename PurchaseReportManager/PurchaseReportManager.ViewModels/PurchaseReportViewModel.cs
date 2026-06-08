using Domain;
using PurchaseReportManager.Proxy;

namespace PurchaseReportManager.ViewModels;

public sealed class PurchaseReportViewModel(IPurchaseReportProxy proxy)
{
    public const string TodasFamilias = "Todas";

    public static readonly string[] XyzPresetNames =
        ["Muy estricta", "Estricta", "Normal tienda", "Tolerante", "Muy tolerante"];

    private static readonly (decimal X, decimal Y)[] XyzPresets =
    [
        (0.50m, 1.00m),
        (0.75m, 1.50m),
        (1.00m, 2.00m),
        (1.50m, 3.00m),
        (2.00m, 4.00m),
    ];

    private IReadOnlyList<PurchaseReportLine> _items = [];

    // Catálogos
    public IReadOnlyList<TenantOption> Tenants { get; private set; } = [];
    public IReadOnlyList<string> Families { get; private set; } = [];
    public IReadOnlyList<string> Subfamilies { get; private set; } = [];

    // Selecciones
    public string SelectedTenantId { get; set; } = string.Empty;
    public string SelectedFamilia { get; set; } = TodasFamilias;
    public string? SelectedSubFamilia { get; set; }

    // Parámetros del cálculo
    public int WindowDays { get; set; } = 45;
    public int ReviewFrequencyDays { get; set; } = 7;
    public decimal ServiceLevelPercent { get; set; } = 95m;
    public int DefaultSupplierDays { get; set; } = 14;
    public decimal MinOperationalStock { get; set; } = 1m;
    public int XyzTolerancePreset { get; set; } = 4;
    public decimal XyzXThreshold => XyzPresets[XyzTolerancePreset - 1].X;
    public decimal XyzYThreshold => XyzPresets[XyzTolerancePreset - 1].Y;

    public void RestoreDefaults()
    {
        WindowDays = 45;
        ReviewFrequencyDays = 7;
        ServiceLevelPercent = 95m;
        DefaultSupplierDays = 14;
        MinOperationalStock = 1m;
        XyzTolerancePreset = 4;
    }

    // Estado de carga
    public event Action? StateChanged;

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set { _isLoading = value; StateChanged?.Invoke(); }
    }

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

    // Ordenamiento
    public string SortColumn { get; private set; } = "prioridad";
    public bool SortAscending { get; private set; } = false;

    public void SetSort(string column)
    {
        if (SortColumn == column)
            SortAscending = !SortAscending;
        else
        {
            SortColumn = column;
            SortAscending = column is "sku" or "descripcion" or "familia" or "subfamilia" or "abc" or "xyz";
        }
        ResetPagination();
    }

    // Paginación
    public const int PageSize = 20;
    public int CurrentPage { get; private set; } = 1;

    public IReadOnlyList<PurchaseReportLine> FilteredItems
    {
        get
        {
            var filtered = _items
                .Where(x => !SoloConCompraSugerida || x.TieneSugerencia)
                .Where(x => !SoloCriticos || x.EsCritico)
                .Where(x => !SoloRequiereRevision || x.RequiereRevision)
                .Where(x => string.IsNullOrWhiteSpace(SearchText) || MatchesSearch(x));
            return SortItems(filtered).ToList().AsReadOnly();
        }
    }

    private bool MatchesSearch(PurchaseReportLine x)
    {
        var q = SearchText;
        var inventarioAlias = x.EstadoInventario.ToUpperInvariant() == "SALUDABLE" ? "óptimo" : string.Empty;
        return x.Sku.Contains(q, StringComparison.OrdinalIgnoreCase)
            || x.Descripcion.Contains(q, StringComparison.OrdinalIgnoreCase)
            || LegacyTextNormalizer.Normalize(x.Descripcion).Contains(q, StringComparison.OrdinalIgnoreCase)
            || x.Familia.Contains(q, StringComparison.OrdinalIgnoreCase)
            || LegacyTextNormalizer.Normalize(x.Familia).Contains(q, StringComparison.OrdinalIgnoreCase)
            || x.SubFamilia.Contains(q, StringComparison.OrdinalIgnoreCase)
            || LegacyTextNormalizer.Normalize(x.SubFamilia).Contains(q, StringComparison.OrdinalIgnoreCase)
            || x.Abc.Contains(q, StringComparison.OrdinalIgnoreCase)
            || x.Xyz.Contains(q, StringComparison.OrdinalIgnoreCase)
            || x.MotivoCompra.Contains(q, StringComparison.OrdinalIgnoreCase)
            || LegacyTextNormalizer.Normalize(x.MotivoCompra).Contains(q, StringComparison.OrdinalIgnoreCase)
            || x.NivelAlerta.Contains(q, StringComparison.OrdinalIgnoreCase)
            || x.EstadoInventario.Contains(q, StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrEmpty(inventarioAlias) && inventarioAlias.Contains(q, StringComparison.OrdinalIgnoreCase));
    }

    private IEnumerable<PurchaseReportLine> SortItems(IEnumerable<PurchaseReportLine> items) =>
        SortColumn switch
        {
            "sku"         => Ord(items, x => x.Sku),
            "descripcion" => Ord(items, x => x.Descripcion),
            "familia"     => Ord(items, x => x.Familia),
            "subfamilia"  => Ord(items, x => x.SubFamilia),
            "existencia"  => Ord(items, x => x.ExistenciaEfectiva),
            "ventas"      => Ord(items, x => x.Ventas45Dias),
            "abc"         => Ord(items, x => x.Abc),
            "xyz"         => Ord(items, x => x.Xyz),
            "cantsugerida"=> Ord(items, x => x.CantidadSugerida),
            "alerta"      => Ord(items, x => AlertOrder(x.NivelAlerta)),
            "inventario"  => Ord(items, x => x.EstadoInventario),
            _             => items
                                .OrderByDescending(x => AlertOrder(x.NivelAlerta))
                                .ThenByDescending(x => x.TieneSugerencia)
                                .ThenByDescending(x => x.CantidadSugerida),
        };

    private IOrderedEnumerable<PurchaseReportLine> Ord<T>(
        IEnumerable<PurchaseReportLine> items, Func<PurchaseReportLine, T> key) where T : IComparable<T> =>
        SortAscending ? items.OrderBy(key) : items.OrderByDescending(key);

    private static int AlertOrder(string nivel) => nivel.ToUpperInvariant() switch
    {
        "CRITICO" => 4, "ALTO" => 3, "MEDIO" => 2, "BAJO" => 1, _ => 0
    };

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
    public decimal VentasTotales => FilteredItems.Sum(x => x.Ventas45Dias);
    public decimal TotalExistenciaEfectiva => FilteredItems.Sum(x => x.ExistenciaEfectiva);

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        CatalogErrorMessage = null;
        IsLoadingCatalog = true;
        try
        {
            Tenants = await proxy.GetTenantsAsync(cancellationToken);
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
        SelectedFamilia = TodasFamilias;
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

        await LoadAsync(cancellationToken);
    }

    public async Task OnFamiliaChangedAsync(CancellationToken cancellationToken = default)
    {
        SelectedSubFamilia = null;
        Subfamilies = [];
        _items = [];
        SelectedItem = null;
        CatalogErrorMessage = null;
        ResetPagination();

        if (SelectedFamilia == TodasFamilias || string.IsNullOrEmpty(SelectedFamilia))
        {
            SelectedFamilia = TodasFamilias;
            await LoadAsync(cancellationToken);
            return;
        }

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

        await LoadAsync(cancellationToken);
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(SelectedTenantId))
            return;

        IsLoading = true;
        try
        {
            if (SelectedFamilia == TodasFamilias || string.IsNullOrWhiteSpace(SelectedFamilia))
            {
                SelectedFamilia = TodasFamilias;
                _items = await proxy.GetAllPurchaseReportAsync(
                    SelectedTenantId, WindowDays, ReviewFrequencyDays, ServiceLevelPercent,
                    DefaultSupplierDays, MinOperationalStock,
                    XyzXThreshold, XyzYThreshold, cancellationToken);
            }
            else
            {
                var subfamilia = string.IsNullOrEmpty(SelectedSubFamilia) ? null : SelectedSubFamilia;
                _items = await proxy.GetPurchaseReportAsync(
                    SelectedTenantId, SelectedFamilia, subfamilia,
                    WindowDays, ReviewFrequencyDays, ServiceLevelPercent,
                    DefaultSupplierDays, MinOperationalStock,
                    XyzXThreshold, XyzYThreshold, cancellationToken);
            }
            SelectedItem = null;
            SearchText = string.Empty;
            SortColumn = "prioridad";
            SortAscending = false;
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
