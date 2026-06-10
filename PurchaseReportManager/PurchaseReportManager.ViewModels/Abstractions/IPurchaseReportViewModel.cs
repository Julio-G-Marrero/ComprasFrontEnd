using Domain;

namespace PurchaseReportManager.ViewModels;

public interface IPurchaseReportViewModel
{
    // Constantes expuestas como propiedades de instancia
    string TodasFamilias { get; }
    IReadOnlyList<string> XyzPresetNames { get; }

    // Catálogos
    IReadOnlyList<TenantOption> Tenants { get; }
    IReadOnlyList<string> Families { get; }
    IReadOnlyList<string> Subfamilies { get; }

    // Selecciones
    string SelectedTenantId { get; set; }
    string SelectedFamilia { get; set; }
    string? SelectedSubFamilia { get; set; }

    // Parámetros del cálculo
    int WindowDays { get; set; }
    int ReviewFrequencyDays { get; set; }
    decimal ServiceLevelPercent { get; set; }
    int DefaultSupplierDays { get; set; }
    decimal MinOperationalStock { get; set; }
    int XyzTolerancePreset { get; set; }

    // Estado de carga
    event Action? StateChanged;
    bool IsLoading { get; }
    bool IsLoadingCatalog { get; }
    string? ErrorMessage { get; }
    string? CatalogErrorMessage { get; }

    // Reporte
    IReadOnlyList<PurchaseReportLine> Items { get; }
    PurchaseReportLine? SelectedItem { get; }
    IReadOnlyList<PurchaseReportLine> FilteredItems { get; }
    IReadOnlyList<PurchaseReportLine> PagedItems { get; }

    // Filtros locales
    bool SoloConCompraSugerida { get; set; }
    bool SoloCriticos { get; set; }
    bool SoloRequiereRevision { get; set; }
    string SearchText { get; set; }

    // Ordenamiento
    string SortColumn { get; }
    bool SortAscending { get; }

    // Paginación
    int CurrentPage { get; }
    int TotalPages { get; }
    int PagedFrom { get; }
    int PagedTo { get; }

    // KPIs / Resumen
    int TotalSkus { get; }
    int SkusConCompraSugerida { get; }
    decimal CantidadTotalSugerida { get; }
    int ProductosCriticos { get; }
    int ProductosRevision { get; }
    decimal VentasTotales { get; }
    decimal TotalExistenciaEfectiva { get; }

    // Métodos
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task OnTenantChangedAsync(CancellationToken cancellationToken = default);
    Task OnFamiliaChangedAsync(CancellationToken cancellationToken = default);
    Task LoadAsync(CancellationToken cancellationToken = default);
    void RestoreDefaults();
    void SetSort(string column);
    void ResetPagination();
    void GoToNextPage();
    void GoToPreviousPage();
    void ToggleSoloCompraSugerida();
    void ToggleSoloCriticos();
    void ToggleSoloRevision();
    void ClearFilters();
    void SelectItem(PurchaseReportLine item);
    void ClearSelection();
}
