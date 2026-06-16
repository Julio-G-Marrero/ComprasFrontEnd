namespace Domain.Dtos;

public class PurchaseReportLineDto
{
    public string Sku { get; init; }
    public string Description { get; init; }
    public string Family { get; init; }
    public string SubFamily { get; init; }
    public bool RequiresStock { get; init; }
    public decimal EffectiveStock { get; init; }
    public decimal SalesPeriodQuantity { get; init; }
    public decimal DailyDemand { get; init; }
    public decimal DailyStandardDeviation { get; init; }
    public decimal? CoefficientOfVariation { get; init; }
    public string ABC { get; init; }
    public string XYZ { get; init; }
    public int SupplierLeadTimeDays { get; init; }
    public int ReviewFrequencyDays { get; init; }
    public int ProtectionPeriodDays { get; init; }
    public decimal SafetyStock { get; init; }
    public decimal MinimumOperationalStock { get; init; }
    public decimal Rop { get; init; }
    public decimal RotationTargetInventory { get; init; }
    public decimal FinalTargetInventory { get; init; }
    public decimal GrossQuantity { get; init; }
    public int UnitsPerPackage { get; init; }
    public int SuggestedPackages { get; init; }
    public decimal SuggestedQuantity { get; init; }
    public string PurchaseReason { get; init; }
    public string AlertLevel { get; init; }
    public bool RequiresReview { get; init; }
    public string ReviewReason { get; init; }
    public decimal? CurrentCoverageDays { get; init; }
    public decimal ExcessInventory { get; init; }
    public decimal? ExcessInventoryPercentage { get; init; }
    public string InventoryStatus { get; init; }
    public string InventoryReason { get; init; }
}
