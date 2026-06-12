using Domain;
using PurchaseReportManager.Proxy.Dtos;

namespace PurchaseReportManager.Proxy.Adapters;

internal static class PurchaseReportLineAdapter
{
    internal static PurchaseReportLine ToModel(PurchaseReportLineDto dto) => new()
    {
        Sku = dto.Sku,
        Description = dto.Description,
        Family = dto.Family,
        SubFamily = dto.SubFamily,
        RequiresStock = dto.RequiresStock,
        EffectiveStock = dto.EffectiveStock,
        SalesPeriodQuantity = dto.SalesPeriodQuantity,
        DailyDemand = dto.DailyDemand,
        DailyStandardDeviation = dto.DailyStandardDeviation,
        CoefficientOfVariation = dto.CoefficientOfVariation,
        Abc = dto.Abc,
        Xyz = dto.Xyz,
        SupplierLeadTimeDays = dto.SupplierLeadTimeDays,
        ReviewFrequencyDays = dto.ReviewFrequencyDays,
        ProtectionPeriodDays = dto.ProtectionPeriodDays,
        SafetyStock = dto.SafetyStock,
        MinimumOperationalStock = dto.MinimumOperationalStock,
        Rop = dto.Rop,
        RotationTargetInventory = dto.RotationTargetInventory,
        FinalTargetInventory = dto.FinalTargetInventory,
        GrossQuantity = dto.GrossQuantity,
        UnitsPerPackage = dto.UnitsPerPackage,
        SuggestedPackages = dto.SuggestedPackages,
        SuggestedQuantity = dto.SuggestedQuantity,
        PurchaseReason = dto.PurchaseReason,
        AlertLevel = dto.AlertLevel,
        RequiresReview = dto.RequiresReview,
        ReviewReason = dto.ReviewReason,
        CurrentCoverageDays = dto.CurrentCoverageDays,
        ExcessInventory = dto.ExcessInventory,
        ExcessInventoryPercentage = dto.ExcessInventoryPercentage,
        InventoryStatus = dto.InventoryStatus,
        InventoryReason = dto.InventoryReason
    };
}
