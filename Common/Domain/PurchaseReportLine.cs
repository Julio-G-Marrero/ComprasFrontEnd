namespace Domain;

public sealed class PurchaseReportLine
{
    public string Sku { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Family { get; init; } = string.Empty;
    public string SubFamily { get; init; } = string.Empty;

    public bool RequiresStock { get; init; }
    public decimal EffectiveStock { get; init; }

    public decimal SalesPeriodQuantity { get; init; }
    public decimal DailyDemand { get; init; }
    public decimal DailyStandardDeviation { get; init; }
    public decimal? CoefficientOfVariation { get; init; }

    public string Abc { get; init; } = string.Empty;
    public string Xyz { get; init; } = string.Empty;

    public decimal SupplierLeadTimeDays { get; init; }
    public decimal ReviewFrequencyDays { get; init; }
    public decimal ProtectionPeriodDays { get; init; }
    public decimal SafetyStock { get; init; }
    public decimal MinimumOperationalStock { get; init; }
    public decimal Rop { get; init; }
    public decimal RotationTargetInventory { get; init; }
    public decimal FinalTargetInventory { get; init; }

    public decimal GrossQuantity { get; init; }
    public decimal UnitsPerPackage { get; init; }
    public decimal SuggestedPackages { get; init; }
    public decimal SuggestedQuantity { get; init; }
    public string PurchaseReason { get; init; } = string.Empty;

    public string AlertLevel { get; init; } = string.Empty;
    public bool RequiresReview { get; init; }
    public string ReviewReason { get; init; } = string.Empty;

    public decimal? CurrentCoverageDays { get; init; }
    public decimal ExcessInventory { get; init; }
    public decimal? ExcessInventoryPercentage { get; init; }
    public string InventoryStatus { get; init; } = string.Empty;
    public string InventoryReason { get; init; } = string.Empty;

    public bool EsCritico => AlertLevel.Equals("CRITICO", StringComparison.OrdinalIgnoreCase);
    public bool TieneSugerencia => SuggestedQuantity > 0;
}
