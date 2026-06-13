using Domain;
using PurchaseReportManager.Proxy;
using PurchaseReportManager.Proxy.Abstractions;
using PurchaseReportManager.ViewModels.Abstractions;

namespace PurchaseReportManager.ViewModels;

internal sealed class PurchaseReportViewModel(IPurchaseReportProxy proxy) : IPurchaseReportViewModel
{
    public const string AllFamilies = "Todas";

    public static readonly string[] XyzPresetNames =
        ["Muy estricta", "Estricta", "Normal tienda", "Tolerante", "Muy tolerante"];

    string IPurchaseReportViewModel.AllFamilies => AllFamilies;
    IReadOnlyList<string> IPurchaseReportViewModel.XyzPresetNames => XyzPresetNames;

    private static readonly (decimal X, decimal Y)[] XyzPresets =
    [
        (0.50m, 1.00m),
        (0.75m, 1.50m),
        (1.00m, 2.00m),
        (1.50m, 3.00m),
        (2.00m, 4.00m),
    ];

    private IReadOnlyList<PurchaseReportLine> _items = [];

    // Catalogs
    public IReadOnlyList<TenantOption> Tenants { get; private set; } = [];
    public IReadOnlyList<string> Families { get; private set; } = [];
    public IReadOnlyList<string> Subfamilies { get; private set; } = [];

    // Selections
    public string SelectedTenantId { get; set; } = string.Empty;
    public string SelectedFamily { get; set; } = AllFamilies;
    public string? SelectedSubFamily { get; set; }

    // Calculation parameters
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

    // Loading state
    public event Action? StateChanged;
    public event Action<string>? OnFailure;

    private void NotifyFailure(string? message)
    {
        ErrorMessage = message;
        OnFailure?.Invoke(message ?? string.Empty);
    }

    private void NotifyCatalogFailure(string message)
    {
        CatalogErrorMessage = message;
        OnFailure?.Invoke(message);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set { _isLoading = value; StateChanged?.Invoke(); }
    }

    public bool IsLoadingCatalog { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? CatalogErrorMessage { get; private set; }

    // Report data
    public IReadOnlyList<PurchaseReportLine> Items => _items;
    public PurchaseReportLine? SelectedItem { get; private set; }

    // Local filters
    public bool OnlyWithSuggestedPurchase { get; set; }
    public bool OnlyCritical { get; set; }
    public bool OnlyRequiresReview { get; set; }
    public string SearchText { get; set; } = string.Empty;

    // Sorting
    public string SortColumn { get; private set; } = "priority";
    public bool SortAscending { get; private set; } = false;

    public void SetSort(string column)
    {
        if (SortColumn == column)
            SortAscending = !SortAscending;
        else
        {
            SortColumn = column;
            SortAscending = column is "sku" or "description" or "family" or "subfamily" or "abc" or "xyz";
        }
        ResetPagination();
    }

    // Pagination
    public const int PageSize = 20;
    public int CurrentPage { get; private set; } = 1;

    public IReadOnlyList<PurchaseReportLine> FilteredItems
    {
        get
        {
            var filtered = _items
                .Where(x => !OnlyWithSuggestedPurchase || x.HasSuggestion)
                .Where(x => !OnlyCritical || x.IsCritical)
                .Where(x => !OnlyRequiresReview || x.RequiresReview)
                .Where(x => string.IsNullOrWhiteSpace(SearchText) || MatchesSearch(x));
            return SortItems(filtered).ToList().AsReadOnly();
        }
    }

    private bool MatchesSearch(PurchaseReportLine x)
    {
        var q = SearchText;
        var inventoryAlias = x.InventoryStatus.ToUpperInvariant() == "SALUDABLE" ? "óptimo" : string.Empty;
        return x.Sku.Contains(q, StringComparison.OrdinalIgnoreCase)
            || x.Description.Contains(q, StringComparison.OrdinalIgnoreCase)
            || LegacyTextNormalizer.Normalize(x.Description).Contains(q, StringComparison.OrdinalIgnoreCase)
            || x.Family.Contains(q, StringComparison.OrdinalIgnoreCase)
            || LegacyTextNormalizer.Normalize(x.Family).Contains(q, StringComparison.OrdinalIgnoreCase)
            || x.SubFamily.Contains(q, StringComparison.OrdinalIgnoreCase)
            || LegacyTextNormalizer.Normalize(x.SubFamily).Contains(q, StringComparison.OrdinalIgnoreCase)
            || x.Abc.Contains(q, StringComparison.OrdinalIgnoreCase)
            || x.Xyz.Contains(q, StringComparison.OrdinalIgnoreCase)
            || x.PurchaseReason.Contains(q, StringComparison.OrdinalIgnoreCase)
            || LegacyTextNormalizer.Normalize(x.PurchaseReason).Contains(q, StringComparison.OrdinalIgnoreCase)
            || x.AlertLevel.Contains(q, StringComparison.OrdinalIgnoreCase)
            || x.InventoryStatus.Contains(q, StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrEmpty(inventoryAlias) && inventoryAlias.Contains(q, StringComparison.OrdinalIgnoreCase));
    }

    private IEnumerable<PurchaseReportLine> SortItems(IEnumerable<PurchaseReportLine> items) =>
        SortColumn switch
        {
            "sku"          => Ord(items, x => x.Sku),
            "description"  => Ord(items, x => x.Description),
            "family"       => Ord(items, x => x.Family),
            "subfamily"    => Ord(items, x => x.SubFamily),
            "stock"        => Ord(items, x => x.EffectiveStock),
            "sales"        => Ord(items, x => x.SalesPeriodQuantity),
            "abc"          => Ord(items, x => x.Abc),
            "xyz"          => Ord(items, x => x.Xyz),
            "suggestedqty" => Ord(items, x => x.SuggestedQuantity),
            "alert"        => Ord(items, x => AlertOrder(x.AlertLevel)),
            "inventory"    => Ord(items, x => x.InventoryStatus),
            _              => items
                                .OrderByDescending(x => AlertOrder(x.AlertLevel))
                                .ThenByDescending(x => x.HasSuggestion)
                                .ThenByDescending(x => x.SuggestedQuantity),
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

    // Summary (computed over FilteredItems)
    public int TotalSkus => FilteredItems.Count;
    public int SkusWithSuggestedPurchase => FilteredItems.Count(x => x.HasSuggestion);
    public decimal TotalSuggestedQuantity => FilteredItems.Sum(x => x.SuggestedQuantity);
    public int CriticalProducts => FilteredItems.Count(x => x.IsCritical);
    public int ReviewProducts => FilteredItems.Count(x => x.RequiresReview);
    public decimal TotalSales => FilteredItems.Sum(x => x.SalesPeriodQuantity);
    public decimal TotalEffectiveStock => FilteredItems.Sum(x => x.EffectiveStock);

    public async Task InitializeViewModel(CancellationToken cancellationToken = default)
    {
        CatalogErrorMessage = null;
        IsLoadingCatalog = true;
        try
        {
            var result = await proxy.GetTenantsAsync(cancellationToken);
            if (result.IsSuccess)
                Tenants = result.Data ?? [];
            else
                NotifyCatalogFailure($"Error al cargar tenants: {result.ErrorMessage}");
        }
        finally
        {
            IsLoadingCatalog = false;
        }
    }

    public async Task OnTenantChangedAsync(CancellationToken cancellationToken = default)
    {
        SelectedFamily = AllFamilies;
        SelectedSubFamily = null;
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

    public async Task OnFamilyChangedAsync(CancellationToken cancellationToken = default)
    {
        SelectedSubFamily = null;
        Subfamilies = [];
        _items = [];
        SelectedItem = null;
        CatalogErrorMessage = null;
        ResetPagination();

        if (SelectedFamily == AllFamilies || string.IsNullOrEmpty(SelectedFamily))
            SelectedFamily = AllFamilies;
        else
        {
            IsLoadingCatalog = true;
            try
            {
                var result = await proxy.GetSubfamiliesAsync(SelectedTenantId, SelectedFamily, cancellationToken);
                if (result.IsSuccess)
                    Subfamilies = result.Data ?? [];
                else
                    NotifyCatalogFailure($"Error al cargar subfamilias: {result.ErrorMessage}");
            }
            finally
            {
                IsLoadingCatalog = false;
            }
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
            HandlerRequestResult<IReadOnlyList<PurchaseReportLine>> result;

            if (SelectedFamily == AllFamilies || string.IsNullOrWhiteSpace(SelectedFamily))
            {
                SelectedFamily = AllFamilies;
                result = await proxy.GetAllPurchaseReportAsync(
                    SelectedTenantId, WindowDays, ReviewFrequencyDays, ServiceLevelPercent,
                    DefaultSupplierDays, MinOperationalStock,
                    XyzXThreshold, XyzYThreshold, cancellationToken);
            }
            else
            {
                var subFamily = string.IsNullOrEmpty(SelectedSubFamily) ? null : SelectedSubFamily;
                result = await proxy.GetPurchaseReportAsync(
                    SelectedTenantId, SelectedFamily, subFamily,
                    WindowDays, ReviewFrequencyDays, ServiceLevelPercent,
                    DefaultSupplierDays, MinOperationalStock,
                    XyzXThreshold, XyzYThreshold, cancellationToken);
            }

            if (result.IsSuccess)
                _items = result.Data ?? [];
            else
            {
                _items = [];
                NotifyFailure(result.ErrorMessage);
            }

            SelectedItem = null;
            SearchText = string.Empty;
            SortColumn = "priority";
            SortAscending = false;
            ResetPagination();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadFamiliesAsync(CancellationToken cancellationToken)
    {
        var result = await proxy.GetFamiliesAsync(SelectedTenantId, cancellationToken);
        if (result.IsSuccess)
            Families = (result.Data ?? []).Where(f => !string.IsNullOrEmpty(f)).ToList().AsReadOnly();
        else
        {
            Families = [];
            NotifyCatalogFailure($"Error al cargar familias: {result.ErrorMessage}");
        }
    }

    public void ResetPagination() => CurrentPage = 1;
    public void GoToNextPage() { if (CurrentPage < TotalPages) CurrentPage++; }
    public void GoToPreviousPage() { if (CurrentPage > 1) CurrentPage--; }

    public void ToggleOnlyWithSuggestedPurchase() { OnlyWithSuggestedPurchase = !OnlyWithSuggestedPurchase; ResetPagination(); }
    public void ToggleOnlyCritical() { OnlyCritical = !OnlyCritical; ResetPagination(); }
    public void ToggleOnlyRequiresReview() { OnlyRequiresReview = !OnlyRequiresReview; ResetPagination(); }

    public void ClearFilters()
    {
        OnlyWithSuggestedPurchase = false;
        OnlyCritical = false;
        OnlyRequiresReview = false;
        SearchText = string.Empty;
        ResetPagination();
    }

    public void SelectItem(PurchaseReportLine item) => SelectedItem = item;
    public void ClearSelection() => SelectedItem = null;
}
