using Common.Views.Helpers;

namespace PurchaseReportManager.ViewModels.Models;

public sealed class PurchaseReportLineModel
{
    // Identity
    public string Sku { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Family { get; init; } = string.Empty;
    public string SubFamily { get; init; } = string.Empty;

    // Stock
    public bool RequiresStock { get; init; }
    public decimal EffectiveStock { get; init; }

    // Demand
    public decimal SalesPeriodQuantity { get; init; }
    public decimal DailyDemand { get; init; }
    public decimal DailyStandardDeviation { get; init; }
    public decimal? CoefficientOfVariation { get; init; }

    // Classification
    public string Abc { get; init; } = string.Empty;
    public string Xyz { get; init; } = string.Empty;

    // Replenishment parameters
    public decimal SupplierLeadTimeDays { get; init; }
    public decimal ReviewFrequencyDays { get; init; }
    public decimal ProtectionPeriodDays { get; init; }
    public decimal SafetyStock { get; init; }
    public decimal MinimumOperationalStock { get; init; }
    public decimal Rop { get; init; }
    public decimal RotationTargetInventory { get; init; }
    public decimal FinalTargetInventory { get; init; }

    // Purchase suggestion
    public decimal GrossQuantity { get; init; }
    public decimal UnitsPerPackage { get; init; }
    public decimal SuggestedPackages { get; init; }
    public decimal SuggestedQuantity { get; init; }
    public string PurchaseReason { get; init; } = string.Empty;

    // Alert
    public string AlertLevel { get; init; } = string.Empty;

    // Review
    public bool RequiresReview { get; init; }
    public string ReviewReason { get; init; } = string.Empty;

    // Inventory analysis
    public decimal? CurrentCoverageDays { get; init; }
    public decimal ExcessInventory { get; init; }
    public decimal? ExcessInventoryPercentage { get; init; }
    public string InventoryStatus { get; init; } = string.Empty;
    public string InventoryReason { get; init; } = string.Empty;

    // UI computed — business
    public bool HasSuggestion => SuggestedQuantity > 0 || SuggestedPackages > 0;
    public bool IsCritical => AlertLevel.Equals("CRITICO", StringComparison.OrdinalIgnoreCase);

    // UI computed — display
    public string AlertBadgeClass => AlertLevel.ToUpperInvariant() switch
    {
        "CRITICO" => "badge bg-danger",
        "ALTO" => "badge bg-warning",
        "MEDIO" => "badge bg-info",
        "BAJO" => "text-muted small",
        _ => "badge bg-secondary"
    };

    public string AlertDisplayText => AlertLevel.ToUpperInvariant() switch
    {
        "BAJO" => "N/A",
        _ => AlertLevel
    };

    public string InventoryLabel => InventoryDisplayHelper.Label(InventoryStatus);
    public string InventoryBadgeClass => InventoryDisplayHelper.BadgeClass(InventoryStatus);

    // Mapping
    public static PurchaseReportLineModel FromDomain(PurchaseReportLine line) => new()
    {
        Sku = line.Sku,
        Description = line.Description,
        Family = line.Family,
        SubFamily = line.SubFamily,
        RequiresStock = line.RequiresStock,
        EffectiveStock = line.EffectiveStock,
        SalesPeriodQuantity = line.SalesPeriodQuantity,
        DailyDemand = line.DailyDemand,
        DailyStandardDeviation = line.DailyStandardDeviation,
        CoefficientOfVariation = line.CoefficientOfVariation,
        Abc = line.Abc,
        Xyz = line.Xyz,
        SupplierLeadTimeDays = line.SupplierLeadTimeDays,
        ReviewFrequencyDays = line.ReviewFrequencyDays,
        ProtectionPeriodDays = line.ProtectionPeriodDays,
        SafetyStock = line.SafetyStock,
        MinimumOperationalStock = line.MinimumOperationalStock,
        Rop = line.Rop,
        RotationTargetInventory = line.RotationTargetInventory,
        FinalTargetInventory = line.FinalTargetInventory,
        GrossQuantity = line.GrossQuantity,
        UnitsPerPackage = line.UnitsPerPackage,
        SuggestedPackages = line.SuggestedPackages,
        SuggestedQuantity = line.SuggestedQuantity,
        PurchaseReason = line.PurchaseReason,
        AlertLevel = line.AlertLevel,
        RequiresReview = line.RequiresReview,
        ReviewReason = line.ReviewReason,
        CurrentCoverageDays = line.CurrentCoverageDays,
        ExcessInventory = line.ExcessInventory,
        ExcessInventoryPercentage = line.ExcessInventoryPercentage,
        InventoryStatus = line.InventoryStatus,
        InventoryReason = line.InventoryReason
    };

    public static List<PurchaseReportLineModel> FromDomain(IEnumerable<PurchaseReportLine> lines) =>
        lines.Select(FromDomain).ToList();
}
