namespace Domain.Dtos;

public class PurchaseReportRequestDto(int windowsDays, int reviewFrequencyDays, decimal zService, int defaultSupplierDays,
    decimal minOperationalStock, decimal xyzXThreshold, decimal xyzYThreshold, List<string> families)
{
    public int WindowsDays => windowsDays;
    public int ReviewFrequencyDays => reviewFrequencyDays;
    public decimal ZService => zService;
    public int DefaultSupplierDays => defaultSupplierDays;
    public decimal MinOperationalStock => minOperationalStock;
    public decimal XyzXThreshold => xyzXThreshold;
    public decimal XyzYThreshold => xyzYThreshold;
    public List<string> Families => families;
}
