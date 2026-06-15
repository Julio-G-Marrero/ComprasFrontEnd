using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PurchaseReportManager.Proxy.Abstractions;
using PurchaseReportManager.Proxy.Adapters;
using PurchaseReportManager.Proxy.Dtos;

namespace PurchaseReportManager.Proxy;

internal sealed class PurchaseReportProxy(
    HttpClient httpClient,
    IOptions<PurchaseReportProxyOptions> options,
    ILogger<PurchaseReportProxy> logger) : IPurchaseReportProxy
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly PurchaseReportProxyOptions _options = options.Value;

    public async Task<HandlerRequestResult<IReadOnlyList<TenantOption>>> GetTenantsAsync(
        CancellationToken cancellationToken = default)
    {
        HandlerRequestResult<IReadOnlyList<TenantOption>> result;
        try
        {
            using var response = await httpClient.GetAsync("/api/tenants", cancellationToken);
            response.EnsureSuccessStatusCode();
            var data = await response.Content
                .ReadFromJsonAsync<List<TenantOption>>(JsonOptions, cancellationToken);
            result = HandlerRequestResult<IReadOnlyList<TenantOption>>.Ok(
                data is null ? [] : data.AsReadOnly());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener tenants");
            result = HandlerRequestResult<IReadOnlyList<TenantOption>>.Fail(ex.Message);
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
            result = HandlerRequestResult<IReadOnlyList<string>>.Ok(
                data is null ? [] : data.AsReadOnly());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener familias para tenant {TenantId}", tenantId);
            result = HandlerRequestResult<IReadOnlyList<string>>.Fail(ex.Message);
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
            result = HandlerRequestResult<IReadOnlyList<string>>.Ok(
                data is null ? [] : data.AsReadOnly());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener subfamilias para tenant {TenantId} family {Family}", tenantId, family);
            result = HandlerRequestResult<IReadOnlyList<string>>.Fail(ex.Message);
        }
        return result;
    }

    public async Task<HandlerRequestResult<IReadOnlyList<PurchaseReportLine>>> GetPurchaseReportAsync(
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
        HandlerRequestResult<IReadOnlyList<PurchaseReportLine>> result;
        try
        {
            var url = BuildReportUrl(family, subFamily, windowDays, reviewFrequencyDays, serviceLevel, defaultSupplierDays, minOperationalStock, xyzXThreshold, xyzYThreshold);
            using var request = TenantRequest(HttpMethod.Get, url, tenantId);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponseDto<List<PurchaseReportLineDto>>>(JsonOptions, cancellationToken);

            if (apiResponse is null || !apiResponse.Success)
            {
                var msg = apiResponse?.Message ?? "La API devolvió un resultado fallido.";
                logger.LogError("GetPurchaseReportAsync: {ErrorMessage}", msg);
                result = HandlerRequestResult<IReadOnlyList<PurchaseReportLine>>.Fail(msg);
            }
            else if (apiResponse.Data is null or { Count: 0 })
            {
                result = HandlerRequestResult<IReadOnlyList<PurchaseReportLine>>.Ok([]);
            }
            else
            {
                var items = PurchaseReportLineAdapter.ToModels(apiResponse.Data).AsReadOnly();
                var f = items[0];
                logger.LogInformation(
                    "GetPurchaseReportAsync OK — items={Count} | SKU={Sku} | SalesPeriodQuantity={Ventas} | SuggestedQuantity={Sugerida} | AlertLevel={Alerta}",
                    items.Count, f.Sku, f.SalesPeriodQuantity, f.SuggestedQuantity, f.AlertLevel);
                result = HandlerRequestResult<IReadOnlyList<PurchaseReportLine>>.Ok(items);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener reporte de compra para tenant {TenantId} family {Family}", tenantId, family);
            result = HandlerRequestResult<IReadOnlyList<PurchaseReportLine>>.Fail(ex.Message);
        }
        return result;
    }

    public async Task<HandlerRequestResult<IReadOnlyList<PurchaseReportLine>>> GetAllPurchaseReportAsync(
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
        HandlerRequestResult<IReadOnlyList<PurchaseReportLine>> result;
        try
        {
            var url = BuildParamsUrl("/api/reports/purchase/all", windowDays, reviewFrequencyDays, serviceLevel, defaultSupplierDays, minOperationalStock, xyzXThreshold, xyzYThreshold);
            using var request = TenantRequest(HttpMethod.Get, url, tenantId);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponseDto<List<PurchaseReportLineDto>>>(JsonOptions, cancellationToken);

            if (apiResponse is null || !apiResponse.Success)
            {
                var msg = apiResponse?.Message ?? "La API devolvió un resultado fallido.";
                logger.LogError("GetAllPurchaseReportAsync: {ErrorMessage}", msg);
                result = HandlerRequestResult<IReadOnlyList<PurchaseReportLine>>.Fail(msg);
            }
            else if (apiResponse.Data is null or { Count: 0 })
            {
                result = HandlerRequestResult<IReadOnlyList<PurchaseReportLine>>.Ok([]);
            }
            else
            {
                result = HandlerRequestResult<IReadOnlyList<PurchaseReportLine>>.Ok(
                    PurchaseReportLineAdapter.ToModels(apiResponse.Data).AsReadOnly());
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener reporte completo de compra para tenant {TenantId}", tenantId);
            result = HandlerRequestResult<IReadOnlyList<PurchaseReportLine>>.Fail(ex.Message);
        }
        return result;
    }

    private HttpRequestMessage TenantRequest(HttpMethod method, string url, string tenantId)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("X-Tenant-Id", tenantId);
        request.Headers.Add("X-Application-Id", _options.ApplicationId);
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
