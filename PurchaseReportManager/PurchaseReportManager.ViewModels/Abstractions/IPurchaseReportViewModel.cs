using Domain;

namespace PurchaseReportManager.ViewModels;

public interface IPurchaseReportViewModel
{
    // Constants exposed as instance members
    string AllFamilies { get; }
    IReadOnlyList<string> XyzPresetNames { get; }

    // Catalogs
    IReadOnlyList<TenantOption> Tenants { get; }
    IReadOnlyList<string> Families { get; }
    IReadOnlyList<string> Subfamilies { get; }

    // Selections
    string SelectedTenantId { get; set; }
    string SelectedFamily { get; set; }
    string? SelectedSubFamily { get; set; }

    // Calculation parameters
    int WindowDays { get; set; }
    int ReviewFrequencyDays { get; set; }
    decimal ServiceLevelPercent { get; set; }
    int DefaultSupplierDays { get; set; }
    decimal MinOperationalStock { get; set; }
    int XyzTolerancePreset { get; set; }

    // Loading state
    event Action? StateChanged;
    event Action<string>? OnFailure;
    bool IsLoading { get; }
    bool IsLoadingCatalog { get; }
    string? ErrorMessage { get; }
    string? CatalogErrorMessage { get; }

    // Report data
    IReadOnlyList<PurchaseReportLine> Items { get; }
    PurchaseReportLine? SelectedItem { get; }
    IReadOnlyList<PurchaseReportLine> FilteredItems { get; }
    IReadOnlyList<PurchaseReportLine> PagedItems { get; }

    // Local filters
    bool OnlyWithSuggestedPurchase { get; set; }
    bool OnlyCritical { get; set; }
    bool OnlyRequiresReview { get; set; }
    string SearchText { get; set; }

    // Sorting
    string SortColumn { get; }
    bool SortAscending { get; }

    // Pagination
    int CurrentPage { get; }
    int TotalPages { get; }
    int PagedFrom { get; }
    int PagedTo { get; }

    // KPIs / Summary
    int TotalSkus { get; }
    int SkusConCompraSugerida { get; }
    decimal TotalSuggestedQuantity { get; }
    int CriticalProducts { get; }
    int ReviewProducts { get; }
    decimal TotalSales { get; }
    decimal TotalEffectiveStock { get; }

    // Methods
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task OnTenantChangedAsync(CancellationToken cancellationToken = default);
    Task OnFamilyChangedAsync(CancellationToken cancellationToken = default);
    Task LoadAsync(CancellationToken cancellationToken = default);
    void RestoreDefaults();
    void SetSort(string column);
    void ResetPagination();
    void GoToNextPage();
    void GoToPreviousPage();
    void ToggleOnlyWithSuggestedPurchase();
    void ToggleOnlyCritical();
    void ToggleOnlyRequiresReview();
    void ClearFilters();
    void SelectItem(PurchaseReportLine item);
    void ClearSelection();
}
