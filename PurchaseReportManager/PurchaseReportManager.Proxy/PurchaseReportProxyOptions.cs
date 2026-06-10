namespace PurchaseReportManager.Proxy;

internal sealed class PurchaseReportProxyOptions
{
    public const string Section = "ApiSettings";
    public string BaseUrl { get; set; } = string.Empty;
    public string ApplicationId { get; set; } = "1";
}
