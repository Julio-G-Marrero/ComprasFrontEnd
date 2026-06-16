using Domain.Dtos;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PurchaseReportManager.Proxy.Abstractions;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PurchaseReportManager.Proxy;

internal sealed class PurchaseReportProxy(
    HttpClient httpClient, IOptions<PurchaseReportProxyOptions> options,
    ILogger<PurchaseReportProxy> logger) : IPurchaseReportProxy
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<HandlerRequestResult<IReadOnlyList<TenantInfoDto>>> GetTenantsAsync(
        CancellationToken cancellationToken = default)
    {
        HandlerRequestResult<IReadOnlyList<TenantInfoDto>> result;
        try
        {
            using var response = await httpClient.GetAsync("/api/tenants", cancellationToken);
            response.EnsureSuccessStatusCode();
            var data = await response.Content
                .ReadFromJsonAsync<List<TenantInfoDto>>(JsonOptions, cancellationToken);
            result = HandlerRequestResult<IReadOnlyList<TenantInfoDto>>.SuccessResult(
                data is null ? [] : data.AsReadOnly());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching tenants");
            result = HandlerRequestResult<IReadOnlyList<TenantInfoDto>>.ErrorResult(ex.Message);
        }
        return result;
    }

    public async Task<HandlerRequestResult<IReadOnlyList<string>>> GetFamiliesAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        HandlerRequestResult<IReadOnlyList<string>> result;
        try
        {
            using var request = TenantRequest(HttpMethod.Get, "/api/catalog/families", tenantId);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var data = await response.Content
                .ReadFromJsonAsync<List<string>>(JsonOptions, cancellationToken);
            result = HandlerRequestResult<IReadOnlyList<string>>.SuccessResult(
                data is null ? [] : data.AsReadOnly());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching families for tenant {TenantId}", tenantId);
            result = HandlerRequestResult<IReadOnlyList<string>>.ErrorResult(ex.Message);
        }
        return result;
    }

    public async Task<HandlerRequestResult<IReadOnlyList<string>>> GetSubfamiliesAsync(
        string tenantId,
        string family,
        CancellationToken cancellationToken = default)
    {
        HandlerRequestResult<IReadOnlyList<string>> result;
        try
        {
            var url = $"/api/catalog/subfamilies?familia={Uri.EscapeDataString(family)}";
            using var request = TenantRequest(HttpMethod.Get, url, tenantId);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var data = await response.Content
                .ReadFromJsonAsync<List<string>>(JsonOptions, cancellationToken);
            result = HandlerRequestResult<IReadOnlyList<string>>.SuccessResult(
                data is null ? [] : data.AsReadOnly());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching subfamilies for tenant {TenantId} family {Family}", tenantId, family);
            result = HandlerRequestResult<IReadOnlyList<string>>.ErrorResult(ex.Message);
        }
        return result;
    }

    public async Task<HandlerRequestResult<IReadOnlyList<PurchaseReportLineDto>>> GetPurchaseReportAsync(
        string tenantId,
        string family,
        string? subFamily,
        int windowDays,
        int reviewFrequencyDays,
        decimal serviceLevel,
        int defaultSupplierDays,
        decimal minOperationalStock,
        decimal xyzXThreshold,
        decimal xyzYThreshold,
        CancellationToken cancellationToken = default)
    {
        HandlerRequestResult<IReadOnlyList<PurchaseReportLineDto>> result;
        try
        {
            var url = BuildReportUrl(family, subFamily, windowDays, reviewFrequencyDays, serviceLevel, defaultSupplierDays, minOperationalStock, xyzXThreshold, xyzYThreshold);
            using var request = TenantRequest(HttpMethod.Get, url, tenantId);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content
                .ReadFromJsonAsync<HandlerRequestResult<IReadOnlyList<PurchaseReportLineDto>>>(JsonOptions, cancellationToken);

            result = apiResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching purchase report for tenant {TenantId} family {Family}", tenantId, family);
            result = HandlerRequestResult<IReadOnlyList<PurchaseReportLineDto>>.ErrorResult(ex.Message);
        }
        return result;
    }

    public async Task<HandlerRequestResult<IReadOnlyList<PurchaseReportLineDto>>> GetAllPurchaseReportAsync(
        string tenantId,
        int windowDays,
        int reviewFrequencyDays,
        decimal serviceLevel,
        int defaultSupplierDays,
        decimal minOperationalStock,
        decimal xyzXThreshold,
        decimal xyzYThreshold,
        CancellationToken cancellationToken = default)
    {
        HandlerRequestResult<IReadOnlyList<PurchaseReportLineDto>> result;
        try
        {
            var url = BuildParamsUrl("/api/reports/purchase/all", windowDays, reviewFrequencyDays, serviceLevel, defaultSupplierDays, minOperationalStock, xyzXThreshold, xyzYThreshold);
            using var request = TenantRequest(HttpMethod.Get, url, tenantId);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content
                .ReadFromJsonAsync<HandlerRequestResult<IReadOnlyList<PurchaseReportLineDto>>>(JsonOptions, cancellationToken);

            result = apiResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all purchase report for tenant {TenantId}", tenantId);
            result = HandlerRequestResult<IReadOnlyList<PurchaseReportLineDto>>.ErrorResult(ex.Message);
        }
        return result;
    }

    private HttpRequestMessage TenantRequest(HttpMethod method, string url, string tenantId)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("X-Tenant-Id", tenantId);
        request.Headers.Add("X-Application-Id", options.Value.ApplicationId);
        return request;
    }

    private static string BuildReportUrl(string family, string? subFamily,
        int windowDays, int reviewFrequencyDays, decimal serviceLevel,
        int defaultSupplierDays, decimal minOperationalStock, decimal xyzX, decimal xyzY)
    {
        var sb = new StringBuilder("/api/reports/purchase?family=")
            .Append(Uri.EscapeDataString(family));
        if (!string.IsNullOrWhiteSpace(subFamily))
            sb.Append("&subFamily=").Append(Uri.EscapeDataString(subFamily));
        AppendParams(sb, windowDays, reviewFrequencyDays, serviceLevel, defaultSupplierDays, minOperationalStock, xyzX, xyzY);
        return sb.ToString();
    }

    private static string BuildParamsUrl(string baseUrl,
        int windowDays, int reviewFrequencyDays, decimal serviceLevel,
        int defaultSupplierDays, decimal minOperationalStock, decimal xyzX, decimal xyzY)
    {
        var sb = new StringBuilder(baseUrl).Append('?');
        AppendParams(sb, windowDays, reviewFrequencyDays, serviceLevel, defaultSupplierDays, minOperationalStock, xyzX, xyzY, firstParam: true);
        return sb.ToString();
    }

    private static void AppendParams(StringBuilder sb, int windowDays, int reviewFrequencyDays,
        decimal serviceLevel, int defaultSupplierDays, decimal minOperationalStock,
        decimal xyzX, decimal xyzY, bool firstParam = false)
    {
        var ic = System.Globalization.CultureInfo.InvariantCulture;
        var sep = firstParam ? string.Empty : "&";
        sb.Append(sep).Append("windowDays=").Append(windowDays)
          .Append("&reviewFrequencyDays=").Append(reviewFrequencyDays)
          .Append("&serviceLevel=").Append(serviceLevel.ToString("G", ic))
          .Append("&defaultSupplierDays=").Append(defaultSupplierDays)
          .Append("&minOperationalStock=").Append(minOperationalStock.ToString("G", ic))
          .Append("&xyzXThreshold=").Append(xyzX.ToString("G", ic))
          .Append("&xyzYThreshold=").Append(xyzY.ToString("G", ic));
    }
}
